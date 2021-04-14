using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Ganss.XSS;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace FinnFragen.Web.Pages
{
	public class AskModel : PageModel
	{
		private readonly ILogger<AskModel> _logger;
		private readonly NotificationBuilder notificationBuilder;
		private readonly HtmlSanitizer sanitizer;
		private readonly IConfiguration configuration;
		private readonly CaptchaValidator validator;
		private readonly MarkdownPipeline markdown;
		private readonly Database database;

		public string SiteKey { get; set; }

		[BindProperty]
		public InputModel Input { get; set; }

		public class InputModel
		{
			[Required(ErrorMessage = "Bitte gib deinen Namen ein.")]
			[MinLength(3, ErrorMessage = "Name zu kurz.")]
			[Display(Name = "Dein Name")]
			public string Name { get; set; }

			[Required(ErrorMessage = "Bitte gib einen Titel ein.")]
			[MinLength(5, ErrorMessage = "Titel zu kurz.")]
			[Display(Name = "Titel der Frage")]
			public string Title { get; set; }

			[Required(ErrorMessage = "Bitte gib eine Frage ein.")]
			[MinLength(10, ErrorMessage = "Frage zu kurz.")]
			[Display(Name = "Deine Frage")]
			public string Question { get; set; }
			[EmailAddress(ErrorMessage = "Bitte gebe eine valide Emailadresse ein.")]
			[Display(Name = "Deine Emailadresse für Rückfragen/Benachrichtigungen (Optional)")]
			public string Email { get; set; }

			[Required(ErrorMessage = "Bitte gib mindestens einen Tag ein. (e.g. C#, Java oder Mathe)")]
			public string Tags { get; set; }

			[Display(Name = "Frage im Browser speichern?")]
			public bool SaveId { get; set; }

			[Display(Name = "Ich bin mit der veröffentlichung meiner Frage nach Beantwortung einverstanden. Eine Frage kann jederzeit mit der zugehörigen ID gelöscht werden.")]
			public bool Consent { get; set; }
		}

		public AskModel(ILogger<AskModel> logger, NotificationBuilder notificationBuilder, HtmlSanitizer sanitizer, IConfiguration configuration, CaptchaValidator validator, MarkdownPipeline markdown, Database database)
		{
			_logger = logger;
			this.notificationBuilder = notificationBuilder;
			this.sanitizer = sanitizer;
			this.configuration = configuration;
			this.validator = validator;
			this.markdown = markdown;
			this.database = database;
			SiteKey = configuration.GetValue<string>("Captcha:SiteKey");
		}

		public IActionResult OnGet()
		{
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
				return Page();

#if !DEBUG
			if (!await validator.Validate())
			{
				ModelState.AddModelError(string.Empty, "Recaptcha nicht valide");
				return Page();
			}
#endif
			if (!Input.Consent)
			{
				ModelState.AddModelError(string.Empty, "Bitte stimme der Veröffentlichung deiner Frage zu.");
				return Page();
			}

			string dirty = Markdown.ToHtml(Input.Question, markdown);
			string html = sanitizer.Sanitize(dirty);
			string text = Markdown.ToPlainText(Input.Question, markdown);
			List<string> tags = Input.Tags?.Split(',').Select(s => s.Trim()).Select(s => s.Substring(0, Math.Min(10, s.Length))).ToList() ?? new List<string>();

			string shortName = Regex.Replace(Input.Title, @"[^\u0000-\u007F]+", string.Empty); // Strip non-ascii characters
			shortName = Regex.Replace(shortName.ToLower(), @"\s+", "-");

			const int maxLength = 50;
			const int minLength = 20;


			// Find the highest cut point that lies under maxlength and on a word boundary
			int cut = Math.Min(maxLength, shortName.Length);
			int c = 0;

			while ((c = shortName.IndexOf('-', c + 1)) != -1)
			{
				if (c <= maxLength)
					cut = c;
				else
					break;
			}

			shortName = shortName.Substring(0, cut);

			while (shortName.Length < minLength)
			{
				shortName += new Guid().ToString().Substring(0, minLength - shortName.Length);
			}

			while (await database.Questions.AnyAsync(q => q.ShortName == shortName))
			{
				shortName += "-" + new Guid().ToString().Substring(0, 6);
			}

			string id = await database.GetNewID();
			Question question;
			database.Questions.Add(question = new()
			{
				Name = Input.Name,
				Title = Input.Title,
				TagString = string.Join(',', Input.Tags?.Split(',').Select(s => s.Trim()) ?? Array.Empty<string>()),
				Email = Input.Email,
				QuestionHtml = html,
				QuestionText = text,
				QuestionSource = Input.Question,
				QuestionDate = DateTime.Now,
				Identifier = id,
				QuestionState = Question.State.Asked,
				ShortName = shortName
			});

			await database.SaveChangesAsync();

			if (!string.IsNullOrWhiteSpace(Input.Email))
			{
				notificationBuilder.PushForQuestion("NewQuestionUser", question);
			}

			notificationBuilder.PushForQuestion("NewQuestionAdmin", question, false, true);

			return Redirect($"/QuestionConfirm?id={id}&email={!string.IsNullOrWhiteSpace(Input.Email)}" + (Input.SaveId ? $"&save=1&name=" + HttpUtility.UrlEncode(Input.Title) : ""));
		}
	}
}
