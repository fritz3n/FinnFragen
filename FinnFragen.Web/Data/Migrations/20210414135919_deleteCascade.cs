using Microsoft.EntityFrameworkCore.Migrations;

namespace FinnFragen.Web.Data.Migrations
{
    public partial class deleteCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Questions_QuestionId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Questions_QuestionId",
                table: "Messages",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Questions_QuestionId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Questions_QuestionId",
                table: "Messages",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
