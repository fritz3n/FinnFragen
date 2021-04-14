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

		public DbSet<Question> Questions { get; set; }
		public DbSet<Message> Messages { get; set; }

		public Database(DbContextOptions<Database> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Question>().HasMany(q => q.Messages).WithOne(m => m.Question).OnDelete(DeleteBehavior.Cascade);
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

		const string IdCharacters = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
		const int IdLength = 10;

		public static string GenerateID(Random rnd = null)
		{
			rnd ??= new Random();

			var sb = new StringBuilder(IdLength);

			for (int i = 0; i < IdLength; i++)
				sb.Append(IdCharacters[rnd.Next(IdCharacters.Length)]);

			return sb.ToString();
		}

		public async Task<string> GetNewID()
		{
			var rnd = new Random();
			string id;

			do
			{
				id = GenerateID(rnd);
			} while (await Questions.AnyAsync(q => q.Identifier == id));

			return id;
		}
	}
}
