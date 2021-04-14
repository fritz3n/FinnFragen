using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Ganss.XSS;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<Database>(options =>
				options.UseLazyLoadingProxies().UseSqlServer(
					Configuration.GetConnectionString("Ingo"), options => options.EnableRetryOnFailure()));
			services.AddDatabaseDeveloperPageExceptionFilter();
			services.AddHttpContextAccessor();
			services.AddDefaultIdentity<IdentityUser>(options =>
			{
				options.SignIn.RequireConfirmedAccount = false;
				options.Password.RequireDigit = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
			})
				.AddEntityFrameworkStores<Database>();
			services.AddRazorPages();

			services.AddHttpClient("recaptcha", c =>
			{
				c.BaseAddress = new Uri("https://www.google.com/");
			});

			services.AddScoped<CaptchaValidator>();
			services.AddScoped<QuestionHandler>();
			services.AddSingleton(s => new MarkdownPipelineBuilder().UseAdvancedExtensions().UseEmojiAndSmiley().UseBootstrap().Build());

			services.AddSingleton(s =>
			{
				var sanitizer = new HtmlSanitizer();
				sanitizer.AllowedAttributes.Add("class");
				sanitizer.AllowedClasses.Clear();
				sanitizer.AllowedClasses.Add("blockquote");
				sanitizer.AllowedClasses.Add("table");
				sanitizer.AllowedClasses.Add("img-fluid");
				sanitizer.AllowedClasses.Add("figure");
				sanitizer.AllowedClasses.Add("figure-caption");
				return sanitizer;
			});

			services.AddSingleton<NotifyQueue>();
			services.AddSingleton<NotificationBuilder>();
			services.AddHostedService<NotifyService>();
			services.AddHostedService<ImapService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseStatusCodePagesWithReExecute("/Code", "?code={0}");

			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapRazorPages();
			});
		}
	}
}
