using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages.Questions
{
	public class ManageModel : PageModel
	{
		public IActionResult OnGet()
		{
			if (!HttpContext.User.Identity.IsAuthenticated)
				return NotFound();

			return Page();
		}
	}
}
