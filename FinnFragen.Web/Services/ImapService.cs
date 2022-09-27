using FinnFragen.Web.Data;
using Ganss.XSS;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services
{
	public class ImapService : IHostedService
	{
		CancellationTokenSource tokenSource = new();
		AsyncManualResetEvent stoppedEvent = new(true);
		private Timer timer;
		private readonly NotifyQueue queue;
		private readonly ILogger<ImapService> logger;
		private readonly IConfiguration config;
		private readonly IServiceProvider serviceProvider;
		private readonly HtmlSanitizer sanitizer;
		private Database database;
		private QuestionHandler questionHandler;
		private Regex idRegex = new Regex(@"ID(\w{10})");
		private Regex actionRegex = new Regex(@"^(?:<\w+>)?\s*-#-([^<\n\r]+)(?:\s*<\/\w+>)?");

		private CancellationToken Token => tokenSource.Token;

		public ImapService(NotifyQueue queue, ILogger<ImapService> logger, IConfiguration configuration, IServiceProvider serviceProvider, HtmlSanitizer sanitizer)
		{
			this.queue = queue;
			this.logger = logger;
			config = configuration;
			this.serviceProvider = serviceProvider;
			this.sanitizer = sanitizer;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			timer = new Timer(Service, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(20));

			return Task.CompletedTask;
		}

		public async void Service(object state)
		{
			try
			{
				stoppedEvent.Reset();
				tokenSource.Token.ThrowIfCancellationRequested();

				using IServiceScope scope = serviceProvider.CreateScope();
				database = scope.ServiceProvider.GetRequiredService<Database>();
				questionHandler = scope.ServiceProvider.GetRequiredService<QuestionHandler>();

				IConfigurationSection section = config.GetSection("mail");
				IConfigurationSection admin = config.GetSection("Admin");

				using var client = new ImapClient();
				await client.ConnectAsync(section.GetValue<string>("Host"), section.GetValue<int>("ImapPort"), SecureSocketOptions.StartTls, Token);
				await client.AuthenticateAsync(section.GetValue<string>("User"), section.GetValue<string>("Password"), Token);

				IMailFolder errors = await CreateOrGetErrorFolder(client);
				IMailFolder inbox = client.Inbox;
				await inbox.OpenAsync(FolderAccess.ReadWrite, Token);

				SearchQuery query = SearchQuery.NotSeen;

				IList<UniqueId> messages = await inbox.SearchAsync(query, Token);

				foreach (UniqueId uid in messages)
				{
					MimeMessage message = await inbox.GetMessageAsync(uid);

					Result result = Result.None;

					try
					{
						if (message.From.Mailboxes.First().Address == admin.GetValue<string>("Email"))
						{
							result = await HandleAdminMessage(message);
						}
						else
						{
							result = await HandleUserMessage(message);
						}
					}
					catch (OperationCanceledException)
					{
						throw;
					}
					catch (Exception e)
					{
						logger.LogError(e, "Error while processing message. Ignoring. Subject: " + message.Subject);

						//await inbox.MoveToAsync(uid, errors, Token);
						result = Result.Seen;
					}

					if (result == Result.Delete)
					{
						await inbox.AddFlagsAsync(uid, MessageFlags.Deleted, true, Token);
						await inbox.ExpungeAsync(Token);
					}
					else if (result == Result.Seen)
					{
						await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, Token);
					}
					else
					{
						await client.NoOpAsync(Token);
					}
				}

				if (messages.Count != 0)
				{
					logger.LogInformation($"Processed {messages.Count} emails.");
				}

				await client.DisconnectAsync(true);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Error while retrieving messages.");
			}
			finally
			{
				stoppedEvent.Set();
			}
		}


		private async Task<Result> HandleAdminMessage(MimeMessage message)
		{
			Question q = await AssociateQuestion(message);
			if (q is null)
				return Result.Delete;

			string messageText = message.TextBody ?? message.HtmlBody;

			string action = "message";
			Match m = actionRegex.Match(messageText);

			if (m.Success)
			{
				action = m.Groups[1].Value;
				messageText = messageText.Substring(m.Index + m.Length);
			}

			switch (action)
			{
				case "message":
				case "message nomarkdown":
					string body;
					body = messageText;
					

					if (action.Contains("nomarkdown"))
						await questionHandler.SendMessage(q, body, null, Message.Author.Answerer);
					else
						await questionHandler.SendMessageMarkdown(q, body, Message.Author.Answerer, false);
					break;

				case "answer":
				case "answer nomarkdown":
					if (action.Contains("nomarkdown"))
						await questionHandler.AnswerQuestion(q, messageText, null, database);
					else
						await questionHandler.AnswerQuestionMarkdown(q, messageText, database);
					break;

				case "block":
				case "block nomarkdown":
					if (string.IsNullOrWhiteSpace(messageText))
						await questionHandler.BlockQuestion(q, false, database);
					if (action.Contains("nomarkdown"))
						await questionHandler.BlockQuestionContent(q, messageText, null, database);
					else
						await questionHandler.BlockQuestionContentMarkdown(q, messageText, database);
					break;

				case "delete":
					await questionHandler.DeleteQuestion(q, false, database);
					break;
				default:
					throw new InvalidOperationException($"Action '{action}' not found");
			}

			return Result.Seen;
		}

		private Regex cutRegex = new Regex(@"^.*(?<!=)-=-=-=-=-=-(?!=).*$", RegexOptions.Multiline);
		private async Task<Result> HandleUserMessage(MimeMessage message)
		{
			Question q = await AssociateQuestion(message);
			if (q is null)
				return Result.Delete;


			string title = message.Subject;
			if (message.TextBody is not null)
			{
				string body = message.TextBody;
				Match match = cutRegex.Match(body);
				if (match.Success)
					body = body.Substring(0, match.Index);

				await questionHandler.SendMessageMarkdown(q, body, Message.Author.Asker);
			}
			else if (message.HtmlBody is not null)
			{
				string html = sanitizer.Sanitize(message.HtmlBody);

				Match match = cutRegex.Match(html);
				if (match.Success)
					html = html.Substring(0, match.Index);

				await questionHandler.SendMessage(q, html, html, Message.Author.Asker);
			}
			else
			{
				return Result.Delete;
			}

			return Result.Seen;
		}

		private async Task<Question> AssociateQuestion(MimeMessage message)
		{
			Match m = idRegex.Match(message.Subject);

			if (!m.Success)
				return null;

			string id = m.Groups[1].Value;

			return await database.Questions.FirstOrDefaultAsync(queue => queue.Identifier == id);
		}

		private async Task<IMailFolder> CreateOrGetErrorFolder(ImapClient client)
		{
			const string ErrorFolderName = "Errors";

			IMailFolder toplevel = (await client.GetFoldersAsync(client.PersonalNamespaces[0])).First();

			IMailFolder error = (await toplevel.GetSubfoldersAsync(false, Token)).FirstOrDefault(m => m.Name == ErrorFolderName);

			if (error == default)
			{
				error = await toplevel.CreateAsync(ErrorFolderName, true, Token);
			}
			return error;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			timer.Dispose();
			tokenSource.Cancel();

			await stoppedEvent.WaitAsync(cancellationToken);
		}

		private enum Result
		{
			None,
			Seen,
			Delete
		}
	}
}
