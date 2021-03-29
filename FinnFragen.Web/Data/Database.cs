using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinnFragen.Web.Data
{
	public class Database : IdentityDbContext
	{
		public Database(DbContextOptions<Database> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
		}

		public static async Task Initialize(IServiceProvider serviceProvider)
		{
			Database context = serviceProvider.GetService<Database>();
			UserManager<IdentityUser> manager = serviceProvider.GetService<UserManager<IdentityUser>>();
			IConfiguration config = serviceProvider.GetService<IConfiguration>();
			IConfigurationSection adminSection = config.GetSection("Admin");
			string email = adminSection.GetValue<string>("Email");
			string username = adminSection.GetValue<string>("Username");
			string password = adminSection.GetValue<string>("Password");

			var user = new IdentityUser
			{
				Email = email,
				NormalizedEmail = manager.NormalizeEmail(email),
				UserName = username,
				NormalizedUserName = manager.NormalizeName(username),
				EmailConfirmed = true,
				SecurityStamp = Guid.NewGuid().ToString("D")
			};

			if (!context.Users.Any(u => u.UserName == user.UserName))
			{
				await manager.CreateAsync(user, password);
			}

			await context.SaveChangesAsync();
		}
	}
}
