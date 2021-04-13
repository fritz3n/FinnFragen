using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages.Questions
{
	public class IndexModel : PageModel
	{
		public bool IsAdmin => HttpContext.User.Identity.IsAuthenticated;
		public void OnGet()
		{
		}
	}
}
