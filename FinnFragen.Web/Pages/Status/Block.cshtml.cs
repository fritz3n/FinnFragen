using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages.Status
{
	public class BlockModel : PageModel
	{
		private readonly Database database;
		private readonly QuestionHandler questionHandler;

		public BlockModel(Database database, QuestionHandler questionHandler)
		{
			this.database = database;
			this.questionHandler = questionHandler;
		}

		public Question Question { get; private set; }

		public async Task<IActionResult> OnGetAsync(string id, int? state = null)
		{
			if (id is null)
				return NotFound();

			Question = await questionHandler.QuestionFromId(id);

			if (Question is null)
				return NotFound();

			if (HttpContext.User.Identity.IsAuthenticated && state is not null)
			{
				await questionHandler.SetQuestionState(Question, (Question.State)state);
				return Redirect("/Status/Status/" + Question.Identifier);
			}

			return Page();
		}
		public async Task<IActionResult> OnPostAsync(string id, string ret)
		{
			if (id is null)
				return NotFound();

			Question = await questionHandler.QuestionFromId(id);

			if (Question is null)
				return NotFound();

			await questionHandler.BlockQuestion(Question, !HttpContext.User.Identity.IsAuthenticated);

			return Redirect(ret ?? ("/Status/Status/" + Question.Identifier));
		}
	}
}
