using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Pages
{
	public class CodeModel : PageModel
	{
		public int Code { get; set; }

		public void OnGet(int code)
		{
			Code = code;
		}
	}
}
