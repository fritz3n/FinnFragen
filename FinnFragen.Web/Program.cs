using FinnFragen.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
#if !DEBUG
			using (IServiceScope scope = host.Services.CreateScope())
			{
				Database db = scope.ServiceProvider.GetService<Database>();

				await db.Database.MigrateAsync();

				await Database.Initialize(scope.ServiceProvider);
			}
#endif
			host.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					IHostEnvironment env = hostingContext.HostingEnvironment;

					config.AddJsonFile("appsettings.json", optional: true)
						.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
						.AddJsonFile("/config/appsettings.json", optional: true);

					config.AddEnvironmentVariables();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
