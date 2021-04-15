using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Ganss.XSS;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages.Status
{
	public class StatusModel : PageModel
	{
		private readonly Database database;
		private readonly HtmlSanitizer sanitizer;
		private readonly QuestionHandler questionHandler;
		private readonly CaptchaValidator validator;
		private readonly MarkdownPipeline markdown;

		public Question Question { get; set; }

		public List<Message> Messages { get; set; }
		public string QuestionHtml { get; set; }
		public string AnswerHtml { get; set; }
		public string SiteKey { get; set; }

		public bool IsAdmin => HttpContext.User.Identity.IsAuthenticated;

		[BindProperty]
		public MessageModel Input { get; set; }

		public class MessageModel
		{
			[MinLength(5, ErrorMessage = "Betreff zu kurz.")]
			[Display(Name = "Betreff (Optional)")]
			public string Title { get; set; }

			[Required(ErrorMessage = "Bitte gib deine Nachricht ein.")]
			[MinLength(10, ErrorMessage = "Nachricht zu kurz.")]
			[Display(Name = "Deine Nachricht")]
			public string Message { get; set; }
		}

		public StatusModel(Database database, HtmlSanitizer sanitizer, QuestionHandler questionHandler, IConfiguration configuration, CaptchaValidator validator, MarkdownPipeline markdown)
		{
			this.database = database;
			this.sanitizer = sanitizer;
			this.questionHandler = questionHandler;
			this.validator = validator;
			this.markdown = markdown;
			SiteKey = configuration.GetValue<string>("Captcha:SiteKey");
		}

		public async Task<IActionResult> OnGetAsync(string id)
		{
			if (id is null)
				return NotFound();

			Question = await database.Questions.FirstOrDefaultAsync(q => q.Identifier == id);

			if (Question is null)
				return NotFound();

			Messages = Question.Messages;

			return Page();
		}

		public async Task<IActionResult> OnPostAsync(string id)
		{
			if (id is null)
				return NotFound();


			Question = await database.Questions.FirstOrDefaultAsync(q => q.Identifier == id);

			if (Question is null)
				return NotFound();

			if (Question.QuestionState == Question.State.Blocked)
				return Forbid();
			Messages = Question.Messages;

			if (!ModelState.IsValid)
				return Page();

#if !DEBUG
			if (!IsAdmin && !await validator.Validate())
			{
				ModelState.AddModelError(string.Empty, "Recaptcha nicht valide");
				return Page();
			}
#endif

			await questionHandler.SendMessageMarkdown(Question, Input.Title, Input.Message, IsAdmin ? Message.Author.Answerer : Message.Author.Asker);

			return Page();
		}
	}
}
