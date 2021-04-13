using FinnFragen.Web.Data;
using Markdig;
using Microsoft.Extensions.FileProviders;
using Scriban;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services
{
	public class NotificationBuilder
	{
		private readonly MarkdownPipeline markdown;
		private readonly NotifyQueue notifiyQueue;
		private Regex contentRegex = new Regex(@"<\w+>\s*@Content@\s*<\/\w+>");

		public NotificationBuilder(NotifyQueue notifiyQueue)
		{
			markdown = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
			this.notifiyQueue = notifiyQueue;
		}



		public void PushForMessage(string templateName, Message message, bool toAdmin = true)
		{
			Notification notification = BuildFromTemplateName(
				templateName,
				message,
				message.MessageText,
				message.MessageHtml);

			notification.Address = message.Question.Email;
			notification.Name = message.Question.Name;
			notification.MessageTarget = toAdmin ? Notification.Target.Admin : Notification.Target.User;
			notifiyQueue.EnqueueNotification(notification);
		}

		public void PushForQuestion(string templateName, Question question, bool useAnswer = false, bool toAdmin = false)
		{
			Notification notification = BuildFromTemplateName(
				templateName,
				question,
				useAnswer ? question.AnswerText : question.QuestionText,
				useAnswer ? question.AnswerHtml : question.QuestionHtml);

			notification.Address = question.Email;
			notification.Name = question.Name;
			notification.MessageTarget = toAdmin ? Notification.Target.Admin : Notification.Target.User;
			notifiyQueue.EnqueueNotification(notification);
		}

		private Notification BuildFromTemplateName(string templateName, object model, string contentPlain = null, string contentHtml = null)
		{
			string content = ReadManifestData<NotificationBuilder>(templateName + ".txt");

			if (content.StartsWith("!!"))
				return BuildFromTemplate(content[2..], model, contentPlain, contentHtml, false);
			return BuildFromTemplate(content, model, contentPlain, contentHtml, true);
		}

		private Notification BuildFromTemplate(string templateText, object model, string contentPlain = null, string contentHtml = null, bool useMarkdown = true)
		{
			string[] components = templateText.Split("-Message-");

			if (components.Length != 2)
				throw new ArgumentException("Template must Contain subject and message");

			string subjectTemplate = components[0].Trim();
			string messageTemplate = components[1].Trim();

			string subject = TemplateText(subjectTemplate, model);
			string messageText = TemplateText(messageTemplate, model);

			string messagePlain;
			string messageHtml;

			if (useMarkdown)
			{
				messagePlain = Markdown.ToPlainText(messageText, markdown);
				messageHtml = Markdown.ToHtml(messageText, markdown);

				if (contentHtml is not null)
				{
					messagePlain = messagePlain.Replace("@Content@", contentPlain);
					messageHtml = contentRegex.Replace(messageHtml, contentHtml);
				}
			}
			else
			{
				messagePlain = messageText;
				if (contentHtml is not null)
				{
					messagePlain = messagePlain.Replace("@Content@", contentPlain);
					messageHtml = "<p>" + messageText.Replace("@Content@", $"</p>{contentHtml}<p>") + "</p>";
				}
				else
				{
					messageHtml = "<p>" + messageText + "</p>";
				}
			}


			return new()
			{
				MessageHTML = messageHtml,
				MessageText = messagePlain,
				Subject = subject
			};
		}

		private static string TemplateText(string templateText, object model)
		{
			var template = Template.Parse(templateText);
			return template.Render(model);
		}

		public static string ReadManifestData<TSource>(string embeddedFileName) where TSource : class
		{
			Assembly assembly = typeof(TSource).GetTypeInfo().Assembly;
			string resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					throw new InvalidOperationException("Could not load manifest resource stream.");
				}
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
