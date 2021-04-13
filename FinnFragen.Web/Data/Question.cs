using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Data
{
	public class Question
	{
		public int Id { get; set; }

		public string Name { get; set; }
		public string Title { get; set; }
		public string QuestionHtml { get; set; }
		public string QuestionText { get; set; }
		public string AnswerHtml { get; set; }
		public string AnswerText { get; set; }

		public string Email { get; set; }
		public string Identifier { get; set; }

		public string ShortName { get; set; }

		public DateTime QuestionDate { get; set; }
		public DateTime AnswerDate { get; set; }

		public string TagString { get; set; }

		public string[] Tags => TagString?.Split(',') ?? Array.Empty<string>();

		public State QuestionState { get; set; }

		public virtual List<Message> Messages { get; set; }

		public enum State
		{
			Asked,
			Answered,
			Blocked
		}
	}
}
