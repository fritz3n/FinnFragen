using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Data
{
	public class Message
	{
		public int Id { get; set; }
		public virtual Question Question { get; set; }

		public DateTime Date { get; set; }
		public string MessageHtml { get; set; }
		public bool Seen { get; set; }
		public string MessageText { get; set; }
		public Author MessageAuthor { get; set; }




		public enum Author
		{
			Asker,
			Answerer
		}

	}
}
