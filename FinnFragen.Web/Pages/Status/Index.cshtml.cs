using FinnFragen.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages.Status
{
	public class IndexModel : PageModel
	{
		private readonly Database database;

		public IndexModel(Database database)
		{
			this.database = database;
		}

		[BindProperty]
		[Display(Name = "ID eingeben")]
		[Required(ErrorMessage = "Bitte gebe deine Fragen-ID ein.")]
		[MinLength(10, ErrorMessage = "IDs sind 10 Zeichen Lang.")]
		public string ID { get; set; }

		public async Task<IActionResult> OnPost()
		{
			if (ID == null || !await database.Questions.AnyAsync(q => q.Identifier == ID))
			{
				ModelState.AddModelError(string.Empty, "ID nicht gefunden. Womöglich wurde die Nachricht gelöscht.");
				return Page();
			}

			return Redirect("/Status/Status/" + ID);
		}
	}
}
