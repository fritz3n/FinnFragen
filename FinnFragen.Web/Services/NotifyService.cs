using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using Nito.AsyncEx;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services
{
	public class NotifyService : IHostedService
	{
		CancellationTokenSource tokenSource = new();
		Thread t;
		AsyncManualResetEvent stoppedEvent = new();
		private readonly NotifyQueue queue;
		private readonly ILogger<NotifyService> logger;
		private readonly IConfiguration config;

		public NotifyService(NotifyQueue queue, ILogger<NotifyService> logger, IConfiguration configuration)
		{
			this.queue = queue;
			this.logger = logger;
			config = configuration;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			t = new Thread(() => _ = Service());
			t.IsBackground = true;
			t.Start();

			return Task.CompletedTask;
		}

		public async Task Service()
		{
			try
			{
				while (true)
				{
					Notification notification = await queue.WaitForNotification(tokenSource.Token);

					await SendNotification(notification);
				}
			}
			catch (Exception e)
			{
				logger.LogError(e, "Error while retrieving notification.");
			}
			finally
			{
				stoppedEvent.Set();
			}
		}

		public async Task SendNotification(Notification notification)
		{
			try
			{

				IConfigurationSection section = config.GetSection("mail");

				using var client = new SmtpClient();
				await client.ConnectAsync(section.GetValue<string>("Host"), section.GetValue<int>("SmtpPort"), SecureSocketOptions.StartTls, tokenSource.Token);
				await client.AuthenticateAsync(section.GetValue<string>("User"), section.GetValue<string>("Password"), tokenSource.Token);

				var message = new MimeMessage();

				if (notification.MessageTarget == Notification.Target.User)
				{
					message.To.Add(new MailboxAddress(notification.Name, notification.Address));
					message.From.Add(new MailboxAddress("Finn Fragen", section.GetValue<string>("From")));
				}
				else
				{
					IConfigurationSection admin = config.GetSection("Admin");

					message.From.Add(new MailboxAddress(notification.Name, section.GetValue<string>("From")));
					message.To.Add(new MailboxAddress(admin.GetValue<string>("Username"), admin.GetValue<string>("Email")));
				}


				message.Subject = notification.Subject;

				if (notification.MessageHTML is null)
				{
					message.Body = new TextPart("plain") { Text = notification.MessageText };
				}
				else if (notification.MessageText is null)
				{
					message.Body = new TextPart("html") { Text = notification.MessageHTML };
				}
				else
				{
					var html = new TextPart("html") { Text = notification.MessageHTML };
					var plain = new TextPart("plain") { Text = notification.MessageText };

					var alternative = new MultipartAlternative
					{
						plain,
						html
					};

					message.Body = alternative;
				}

				await client.SendAsync(message, tokenSource.Token);
				await client.DisconnectAsync(true, tokenSource.Token);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Error while sending Notification");
			}
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			tokenSource.Cancel();

			await stoppedEvent.WaitAsync(cancellationToken);
		}
	}
}
