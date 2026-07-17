using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Ganss.Xss;
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

namespace FinnFragen.Web.Pages;

public class AskModel : PageModel
{
    private readonly ILogger<AskModel> _logger;
    private readonly HomeAssistantService homeAssistant;
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
        [Display(Name = "Emailadresse für Benachrichtigungen (Optional)")]
        public string Email { get; set; }

        public string Tags { get; set; }

        [Display(Name = "Frage Merken?")]
        public bool SaveId { get; set; } = true;

        [Display(Name = "Ich bin mit der veröffentlichung meiner Frage nach Beantwortung einverstanden. Eine Frage kann jederzeit mit der zugehörigen ID gelöscht werden.")]
        public bool Consent { get; set; }
    }

    public AskModel(ILogger<AskModel> logger, HomeAssistantService homeAssistant, NotificationBuilder notificationBuilder, HtmlSanitizer sanitizer, IConfiguration configuration, CaptchaValidator validator, MarkdownPipeline markdown, Database database)
    {
        this._logger = logger;
        this.homeAssistant = homeAssistant;
        this.notificationBuilder = notificationBuilder;
        this.sanitizer = sanitizer;
        this.configuration = configuration;
        this.validator = validator;
        this.markdown = markdown;
        this.database = database;
        this.SiteKey = configuration.GetValue<string>("Captcha:SiteKey");
    }

    public IActionResult OnGet()
    {
        this.Input = new InputModel();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!this.ModelState.IsValid)
            return Page();

#if !DEBUG
			if (!await validator.Validate())
			{
				ModelState.AddModelError(string.Empty, "Recaptcha nicht valide");
				return Page();
			}
#endif
        if (!this.Input.Consent)
        {
            this.ModelState.AddModelError(string.Empty, "Bitte stimme der Veröffentlichung deiner Frage zu.");
            return Page();
        }

        string dirty = Markdown.ToHtml(this.Input.Question, this.markdown);
        string html = this.sanitizer.Sanitize(dirty);
        string text = Markdown.ToPlainText(this.Input.Question, this.markdown);
        List<string> tags = this.Input.Tags?.Split(',').Select(s => s.Trim()).Select(s => s[..Math.Min(10, s.Length)]).ToList() ?? [];

        string shortName = Regex.Replace(this.Input.Title, @"[^\u0000-\u007F]+", string.Empty); // Strip non-ascii characters
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

        shortName = shortName[..cut];

        while (shortName.Length < minLength)
        {
            shortName += "-" + Guid.NewGuid().ToString()[..(minLength - shortName.Length)];
        }

        while (await this.database.Questions.AnyAsync(q => q.ShortName == shortName))
        {
            shortName += "-" + Guid.NewGuid().ToString()[..6];
        }

        string id = await this.database.GetNewID();
        Question question;
        this.database.Questions.Add(question = new()
        {
            Name = this.Input.Name,
            Title = this.Input.Title,
            TagString = string.Join(',', this.Input.Tags?.Split(',').Select(s => s.Trim()) ?? Array.Empty<string>()),
            Email = this.Input.Email,
            QuestionHtml = html,
            QuestionText = text,
            QuestionSource = this.Input.Question,
            QuestionDate = DateTime.Now,
            Identifier = id,
            QuestionState = Question.State.Asked,
            ShortName = shortName
        });

        await this.database.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(this.Input.Email))
        {
            this.notificationBuilder.PushForQuestion("NewQuestionUser", question);
        }

        this.notificationBuilder.PushForQuestion("NewQuestionAdmin", question, false, true);

        this.homeAssistant.NotifyForQuestion(question);

        return Redirect($"/QuestionConfirm?id={id}&email={!string.IsNullOrWhiteSpace(this.Input.Email)}" + (this.Input.SaveId ? $"&save=1&name=" + HttpUtility.UrlEncode(this.Input.Title) : ""));
    }
}
