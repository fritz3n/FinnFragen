using FinnFragen.Web.Data;
using Ganss.XSS;
using Markdig;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services
{
	public class QuestionHandler
	{
		private readonly Database database;
		private readonly MarkdownPipeline markdownPipeline;
		private readonly NotificationBuilder notificationBuilder;
		private readonly HtmlSanitizer sanitizer;

		public QuestionHandler(Database database, MarkdownPipeline markdownPipeline, NotificationBuilder notificationBuilder, HtmlSanitizer htmlSanitizer)
		{
			this.database = database;
			this.markdownPipeline = markdownPipeline;
			this.notificationBuilder = notificationBuilder;
			sanitizer = htmlSanitizer;
		}

		public Task SendMessageMarkdown(Question question, string title, string markdown, Message.Author author, bool sanitize = true, string template = null)
		{
			string html = Markdown.ToHtml(markdown, markdownPipeline);
			if (sanitize)
				html = sanitizer.Sanitize(html);
			string text = Markdown.ToPlainText(markdown, markdownPipeline);
			return SendMessage(question, title, text, html, author, template);
		}

		public async Task SendMessage(Question question, string title, string text, string html, Message.Author author, string template = null)
		{
			html ??= text;

			Message message = new()
			{
				Date = DateTime.Now,
				MessageAuthor = author,
				MessageHtml = html,
				MessageText = text,
				MessageTitle = title ?? "Nachricht von " + (author == Message.Author.Answerer ? "Finn" : question.Name),
				Question = question
			};

			database.Messages.Add(message);

			if (!string.IsNullOrWhiteSpace(question.Email))
				notificationBuilder.PushForMessage(template ?? "NewMessage" + (author == Message.Author.Asker ? "Admin" : "User"), message, author == Message.Author.Asker);

			await database.SaveChangesAsync();
		}

		public Task AnswerQuestionMarkdown(Question question, string markdown, Database db = null)
		{
			db ??= database;

			string html = Markdown.ToHtml(markdown, markdownPipeline);
			string text = Markdown.ToPlainText(markdown, markdownPipeline);
			return AnswerQuestion(question, text, html, db);
		}

		public async Task AnswerQuestion(Question question, string text, string html, Database db = null)
		{
			html ??= text;
			db ??= database;

			question.AnswerText = text;
			question.AnswerHtml = html;

			question.AnswerDate = DateTime.Now;
			question.QuestionState = Question.State.Answered;

			await db.SaveChangesAsync();

			if (!string.IsNullOrWhiteSpace(question.Email))
				notificationBuilder.PushForQuestion("QuestionAnswered", question, true, false);
		}

		public async Task BlockQuestion(Question question, bool byUser = false, Database db = null)
		{
			db ??= database;
			question.QuestionState = Question.State.Blocked;
			await db.SaveChangesAsync();

			if (!string.IsNullOrWhiteSpace(question.Email))
				notificationBuilder.PushForQuestion("QuestionBlocked" + (byUser ? "Admin" : "User"), question, false, byUser);
		}

		public async Task SetQuestionState(Question question, Question.State state, Database db = null)
		{
			db ??= database;
			question.QuestionState = state;
			await db.SaveChangesAsync();
		}

		public async Task BlockQuestionContent(Question question, string text, string html, Database db = null)
		{
			await BlockQuestion(question, false, db);

			await SendMessage(question, "Nachricht blockiert", text, html, Message.Author.Answerer, "QuestionBlockedContent");
		}

		public async Task BlockQuestionContentMarkdown(Question question, string markdown, Database db = null)
		{
			await BlockQuestion(question, false, db);

			await SendMessageMarkdown(question, "Nachricht blockiert", markdown, Message.Author.Answerer, false, "QuestionBlockedContent");
		}

		public async Task DeleteQuestion(Question question, bool byUser = false, Database db = null)
		{
			db ??= database;
			db.Questions.Remove(question);
			await db.SaveChangesAsync();

			if (byUser)
				notificationBuilder.PushForQuestion("QuestionDeletedAdmin", question, false, byUser);
			else if (!string.IsNullOrWhiteSpace(question.Email))
				notificationBuilder.PushForQuestion("QuestionDeletedUser", question, false, byUser);
		}

		public async Task EditQuestion(Question question, string title, string markdown, Database db = null)
		{
			db ??= database;


			string html = Markdown.ToHtml(markdown, markdownPipeline);
			html = sanitizer.Sanitize(html);
			string text = Markdown.ToPlainText(markdown, markdownPipeline);

			question.Title = title;
			question.QuestionHtml = html;
			question.QuestionText = text;
			await db.SaveChangesAsync();
		}

		public Task<Question> QuestionFromId(string id)
		{
			return database.Questions.FirstOrDefaultAsync(q => q.Identifier == id);
		}
		public Task<Question> QuestionFromName(string name)
		{
			return database.Questions.FirstOrDefaultAsync(q => q.ShortName == name);
		}
	}
}
