using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages
{
	public class QuestionConfirmModel : PageModel
	{
		public string ID { get; set; }
		public bool Email { get; set; }

		public void OnGet(string id, bool email)
		{
			ID = id;
			Email = email;
		}
	}
}
