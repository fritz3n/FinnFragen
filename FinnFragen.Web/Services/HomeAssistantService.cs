using FinnFragen.Web.Data;
using HADotNet.Core;
using HADotNet.Core.Clients;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services
{
	public class HomeAssistantService
	{
		public HomeAssistantService(IConfiguration config)
		{
			if (!ClientFactory.IsInitialized)
			{
				IConfigurationSection section = config.GetSection("HA");
				ClientFactory.Initialize(section.GetValue<string>("Host"), section.GetValue<string>("Token"));
			}
		}

		public void NotifyForQuestion(Question question)
		{
			Task.Run(async () => await NotifyForQuestionAsync(question));
		}

		public async Task NotifyForQuestionAsync(Question question)
		{
			ServiceClient client = ClientFactory.GetClient<ServiceClient>();
			await client.CallService("script.new_question",
				new
				{
					title = question.Title,
					name = question.Name,
					url = "https://finnfragen.de/Status/Status/" + question.Identifier
				});
		}
	}
}
