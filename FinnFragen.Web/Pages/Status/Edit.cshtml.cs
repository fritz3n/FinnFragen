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
	public class EditModel : PageModel
	{
		private readonly QuestionHandler questionHandler;

		public EditModel(QuestionHandler questionHandler)
		{
			this.questionHandler = questionHandler;
		}

		[BindProperty]
		public MessageModel Input { get; set; }

		public Question Question { get; set; }

		public class MessageModel
		{
			[Required(ErrorMessage = "Bitte gib deinen Titel ein.")]
			[Display(Name = "Titel")]
			public string Title { get; set; }
			[Required(ErrorMessage = "Bitte gib die Frage ein.")]
			[Display(Name = "Frage")]
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

			Input = new()
			{
				Message = Question.QuestionSource,
				Title = Question.Title
			};

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

			await questionHandler.EditQuestion(Question, Input.Title, Input.Message);

			return Redirect(ret ?? ("/Status/Status/" + id));
		}
	}
}
