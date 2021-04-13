using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages.Questions
{
	public class QuestionModel : PageModel
	{
		private readonly QuestionHandler questionHandler;

		public Question Question { get; set; }

		public QuestionModel(QuestionHandler questionHandler)
		{
			this.questionHandler = questionHandler;
		}

		public async Task<IActionResult> OnGetAsync(string name)
		{
			if (name is null)
				return NotFound();
			Question = await questionHandler.QuestionFromName(name);

			if (Question is null || Question.QuestionState != Question.State.Answered)
				return NotFound();

			return Page();
		}
	}
}
