using FinnFragen.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			IHost host = CreateHostBuilder(args).Build();
			using (IServiceScope scope = host.Services.CreateScope())
				await Database.Initialize(scope.ServiceProvider);
			host.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
