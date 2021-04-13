using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages.Status
{
	public class DeleteModel : PageModel
	{
		private readonly Database database;
		private readonly QuestionHandler questionHandler;

		public DeleteModel(Database database, QuestionHandler questionHandler)
		{
			this.database = database;
			this.questionHandler = questionHandler;
		}

		public bool Deleted { get; set; }
		public Question Question { get; private set; }

		public async Task<IActionResult> OnGetAsync(string id)
		{
			if (id is null)
				return NotFound();

			Question = await questionHandler.QuestionFromId(id);

			if (Question is null)
				return NotFound();

			return Page();
		}
		public async Task<IActionResult> OnPostAsync(string id, string ret)
		{
			if (id is null)
				return NotFound();

			Question = await questionHandler.QuestionFromId(id);

			if (Question is null)
				return NotFound();

			await questionHandler.DeleteQuestion(Question, !HttpContext.User.Identity.IsAuthenticated);
			Deleted = true;

			if (!string.IsNullOrWhiteSpace(ret))
				return Redirect(ret);

			return Page();
		}
	}
}
