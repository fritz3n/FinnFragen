using FinnFragen.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FinnFragen.Web.Controllers
{
	[ApiController]
	public class SearchController : Controller
	{
		private readonly Database database;
		static Regex worbreakRegex = new Regex(@"\b(?!\w)");

		public SearchController(Database database)
		{
			this.database = database;
		}

		[HttpGet("questions/all")]
		public async Task<ActionResult<ResultModel>> GetAll(bool all = false, int? from = null, int? take = null)
		{
			bool isAdmin = HttpContext.User.Identity.IsAuthenticated;
			bool? pageAfter = from is null || take is null ? null : all && isAdmin;

			IQueryable<Question> query = database.Questions;
			if (!isAdmin || !all)
				query = query.Where(q => q.QuestionState == Question.State.Answered);
			int totalCount = await query.CountAsync();
			if (pageAfter == false)
				query = query.Skip((int)from).Take((int)take);

			if (!isAdmin || !all)
				query = query.OrderByDescending(q => q.AnswerDate);

			IEnumerable<QuestionModel> models = (await query.ToListAsync())
							  .Select(q => new QuestionModel(q, isAdmin));

			if (isAdmin && all)
			{
				models = models.OrderByDescending(q => q.Restricted.LastActionDate)
					.OrderBy(q => q.Restricted.LastActionPrecedence);
			}

			if (pageAfter == true)
				models = models.Skip((int)from).Take((int)take);

			return new ResultModel()
			{
				TotalCount = totalCount,
				Questions = models.ToList()
			};
		}
		[HttpGet("questions/search")]
		public async Task<ActionResult<ResultModel>> Search(string search, string tags, bool all = false, int? from = null, int? take = null)
		{
			bool isAdmin = HttpContext.User.Identity.IsAuthenticated;
			bool? pageAfter = from is null || take is null ? null : all && isAdmin;

			IQueryable<Question> query = database.Questions;

			if (!isAdmin || !all)
				query = query.Where(q => q.QuestionState == Question.State.Answered);
			int totalCount = await query.CountAsync();
			if (pageAfter == false)
				query = query.Skip((int)from).Take((int)take);

			if (!string.IsNullOrWhiteSpace(search))
			{
				search = search.Replace("\"", "");

				IEnumerable<string> terms = search.Split("||").Select(s => "\"" + s + "\"");
				search = string.Join(" OR ", terms);

				query = query.Where(q => EF.Functions.FreeText(q.Title, search) || EF.Functions.FreeText(q.QuestionText, search) || EF.Functions.FreeText(q.AnswerText, search));
			}

			if (!isAdmin || !all)
				query = query.OrderByDescending(q => q.AnswerDate);

			IEnumerable<QuestionModel> models = (await query.ToListAsync())
							  .Select(q => new QuestionModel(q, isAdmin));

			if (isAdmin && all)
			{
				models = models.OrderByDescending(q => q.Restricted.LastActionDate)
					.OrderBy(q => q.Restricted.LastActionPrecedence);
			}

			if (!string.IsNullOrWhiteSpace(tags))
			{
				IEnumerable<string> tagList = tags.Split(',').Select(s => s.Trim().ToLower());

				models = models.Where(m => tagList.All(t => m.Tags.Select(s => s.ToLower()).Contains(t)));
			}

			if (pageAfter == true)
				models = models.Skip((int)from).Take((int)take);

			return new ResultModel()
			{
				TotalCount = totalCount,
				Questions = models.ToList()
			};
		}



		public class QuestionModel
		{
			public QuestionModel(Question question, bool includeRestricted)
			{
				Name = question.Name;
				Title = question.Title;
				QuestionHtml = question.QuestionHtml;
				AnswerHtml = question.AnswerHtml;
				ShortName = question.ShortName;
				QuestionDate = question.QuestionDate;
				AnswerDate = question.AnswerDate;
				Tags = question.Tags.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

				string synopsis = question.QuestionText.Replace("\n", "").Replace("\r", "");

				MatchCollection breaks = worbreakRegex.Matches(synopsis);

				const int minCut = 200;
				const int maxCut = 230;

				int cut = Math.Min(synopsis.Length, maxCut);

				foreach (Match m in breaks)
				{
					if (minCut < m.Index && m.Index < maxCut)
						cut = m.Index;
					else if (m.Index > maxCut)
						break;
				}
				if (cut < synopsis.Length)
					Synopsis = synopsis.Substring(0, cut) + "...";
				else
					Synopsis = synopsis;

				if (includeRestricted)
				{
					Restricted = new RestrictedModel()
					{
						State = question.QuestionState.ToString(),
						Id = question.Identifier
					};

					Message lastMessage = question.Messages.OrderByDescending(m => m.Date).FirstOrDefault();

					//0 Asked
					//1 mes by asker
					//2 mes by admin
					//3 answered
					//4 blocked

					if (question.QuestionState == Question.State.Answered)
					{
						if (lastMessage is not null && lastMessage.Date > question.AnswerDate)
						{
							Restricted.LastAction = "Message by " + lastMessage.MessageAuthor.ToString();
							Restricted.LastActionPrecedence = lastMessage.MessageAuthor == Message.Author.Asker ? 1 : 2;
							Restricted.LastActionDate = lastMessage.Date;
						}
						else
						{
							Restricted.LastAction = "Answered";
							Restricted.LastActionPrecedence = 3;
							Restricted.LastActionDate = question.AnswerDate;
						}
					}
					else if (question.QuestionState == Question.State.Asked)
					{
						if (lastMessage is not null)
						{
							Restricted.LastAction = "Message by " + lastMessage.MessageAuthor.ToString();
							Restricted.LastActionPrecedence = lastMessage.MessageAuthor == Message.Author.Asker ? 1 : 2;
							Restricted.LastActionDate = lastMessage.Date;
						}
						else
						{
							Restricted.LastAction = "Asked";
							Restricted.LastActionPrecedence = 0;
							Restricted.LastActionDate = question.QuestionDate;
						}
					}
					else
					{
						Restricted.LastAction = "Blocked";
						Restricted.LastActionPrecedence = 4;
						Restricted.LastActionDate = question.QuestionDate;
					}
				}
			}

			public string Name { get; set; }
			public string Title { get; set; }
			public string QuestionHtml { get; set; }
			public string AnswerHtml { get; set; }
			public string ShortName { get; set; }

			public string Synopsis { get; set; }

			public DateTime QuestionDate { get; set; }
			public DateTime AnswerDate { get; set; }
			public string[] Tags { get; set; }

			public RestrictedModel Restricted { get; set; }
		}

		public class RestrictedModel
		{
			public string State { get; set; }
			public string Id { get; set; }
			public int LastActionPrecedence { get; set; }
			public string LastAction { get; set; }
			public DateTime LastActionDate { get; set; }
		}

		public class ResultModel
		{
			public int TotalCount { get; set; }

			public List<QuestionModel> Questions { get; set; }
		}
	}
}
