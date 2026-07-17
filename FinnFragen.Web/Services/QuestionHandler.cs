using FinnFragen.Web.Data;
using Ganss.Xss;
using Markdig;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services;

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
        this.sanitizer = htmlSanitizer;
    }

    public Task SendMessageMarkdown(Question question, string title, string markdown, Message.Author author, bool sanitize = true, string template = null)
    {
        string html = Markdown.ToHtml(markdown, this.markdownPipeline);
        if (sanitize)
            html = this.sanitizer.Sanitize(html);
        string text = Markdown.ToPlainText(markdown, this.markdownPipeline);
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
            MessageTitle = string.IsNullOrWhiteSpace(title) ? "Nachricht von " + (author == Message.Author.Answerer ? "Finn" : question.Name) : title,
            Question = question
        };

        this.database.Messages.Add(message);

        if (!string.IsNullOrWhiteSpace(question.Email))
            this.notificationBuilder.PushForMessage(template ?? ("NewMessage" + (author == Message.Author.Asker ? "Admin" : "User")), message, author == Message.Author.Asker);

        await this.database.SaveChangesAsync();
    }

    public Task AnswerQuestionMarkdown(Question question, string markdown, Database db = null)
    {
        db ??= this.database;

        string html = Markdown.ToHtml(markdown, this.markdownPipeline);
        string text = Markdown.ToPlainText(markdown, this.markdownPipeline);
        return AnswerQuestion(question, text, html, db, markdown);
    }

    public async Task AnswerQuestion(Question question, string text, string html, Database db = null, string source = null)
    {
        html ??= text;
        db ??= this.database;

        question.AnswerSource = source;
        question.AnswerText = text;
        question.AnswerHtml = html;

        question.AnswerDate = DateTime.Now;
        question.QuestionState = Question.State.Answered;

        await db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(question.Email))
            this.notificationBuilder.PushForQuestion("QuestionAnswered", question, true, false);
    }
    public Task DraftQuestionMarkdown(Question question, string markdown, Database db = null)
    {
        db ??= this.database;

        string html = Markdown.ToHtml(markdown, this.markdownPipeline);
        string text = Markdown.ToPlainText(markdown, this.markdownPipeline);
        return DraftQuestion(question, text, html, db, markdown);
    }
    public async Task DraftQuestion(Question question, string text, string html, Database db = null, string source = null)
    {
        html ??= text;
        db ??= this.database;

        question.AnswerSource = source;
        question.AnswerText = text;
        question.AnswerHtml = html;

        await db.SaveChangesAsync();
    }

    public async Task BlockQuestion(Question question, bool byUser = false, Database db = null)
    {
        db ??= this.database;
        question.QuestionState = Question.State.Blocked;
        await db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(question.Email))
            this.notificationBuilder.PushForQuestion("QuestionBlocked" + (byUser ? "Admin" : "User"), question, false, byUser);
    }

    public async Task SetQuestionState(Question question, Question.State state, Database db = null)
    {
        db ??= this.database;
        question.QuestionState = state;
        await db.SaveChangesAsync();
    }

    public async Task BlockQuestionContent(Question question, string text, string html, Database db = null)
    {
        await BlockQuestion(question, false, db);

        await SendMessage(question, "Frage blockiert", text, html, Message.Author.Answerer, "QuestionBlockedContent");
    }

    public async Task BlockQuestionContentMarkdown(Question question, string markdown, Database db = null)
    {
        await BlockQuestion(question, false, db);

        await SendMessageMarkdown(question, "Frage blockiert", markdown, Message.Author.Answerer, false, "QuestionBlockedContent");
    }

    public async Task DeleteQuestion(Question question, bool byUser = false, Database db = null)
    {
        db ??= this.database;
        db.Questions.Remove(question);
        await db.SaveChangesAsync();

        if (byUser)
            this.notificationBuilder.PushForQuestion("QuestionDeletedAdmin", question, false, byUser);
        else if (!string.IsNullOrWhiteSpace(question.Email))
            this.notificationBuilder.PushForQuestion("QuestionDeletedUser", question, false, byUser);
    }

    public async Task EditQuestion(Question question, string title, string markdown, Database db = null)
    {
        db ??= this.database;

        string html = Markdown.ToHtml(markdown, this.markdownPipeline);
        string text = Markdown.ToPlainText(markdown, this.markdownPipeline);

        question.Title = title;
        question.QuestionHtml = html;
        question.QuestionText = text;
        question.QuestionSource = markdown;
        await db.SaveChangesAsync();
    }

    public Task<Question> QuestionFromId(string id) => this.database.Questions.FirstOrDefaultAsync(q => q.Identifier == id);
    public Task<Question> QuestionFromName(string name) => this.database.Questions.FirstOrDefaultAsync(q => q.ShortName == name);
}
