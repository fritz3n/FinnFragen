using Microsoft.EntityFrameworkCore.Migrations;

namespace FinnFragen.Web.Data.Migrations
{
	public partial class FullText : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(
				sql: "CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;",
				suppressTransaction: true);
			migrationBuilder.Sql(
				sql: "CREATE FULLTEXT INDEX ON Questions (AnswerText Language 1031, QuestionText Language 1031, Title Language 1031) KEY INDEX PK_Questions;",
				suppressTransaction: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
