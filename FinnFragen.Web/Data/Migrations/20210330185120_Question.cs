using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FinnFragen.Web.Data.Migrations
{
    public partial class Question : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswerHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Identifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnswerDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TagString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");
        }
    }
}
