using Microsoft.EntityFrameworkCore.Migrations;

namespace FinnFragen.Web.Data.Migrations
{
    public partial class QuestionText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnswerText",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionText",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerText",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuestionText",
                table: "Questions");
        }
    }
}
