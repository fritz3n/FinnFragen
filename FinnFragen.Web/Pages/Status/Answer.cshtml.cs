using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages.Status
{
	public class AnswerModel : PageModel
	{
		private readonly QuestionHandler questionHandler;

		public AnswerModel(QuestionHandler questionHandler)
		{
			this.questionHandler = questionHandler;
		}

		[BindProperty]
		public MessageModel Input { get; set; }

		public Question Question { get; set; }

		public class MessageModel
		{
			[Required(ErrorMessage = "Bitte gib deine Nachricht ein.")]
			[Display(Name = "Antwort")]
			public string Message { get; set; }
		}

		public async Task<IActionResult> OnGetAsync(string id)
		{
			if (!HttpContext.User.Identity.IsAuthenticated)
				return NotFound();

			if (id is null)
				return NotFound();

			Question = await questionHandler.QuestionFromId(id);

			if (Question is null)
				return NotFound();

			return Page();
		}

		public async Task<IActionResult> OnPostAsync(string id, string ret)
		{
			if (!HttpContext.User.Identity.IsAuthenticated)
				return NotFound();

			if (id is null)
				return NotFound();

			Question = await questionHandler.QuestionFromId(id);

			if (Question is null)
				return NotFound();

			if (!ModelState.IsValid)
				return Page();

			await questionHandler.AnswerQuestionMarkdown(Question, Input.Message);

			return Redirect(ret ?? ("/Status/Status/" + id));
		}
	}
}
