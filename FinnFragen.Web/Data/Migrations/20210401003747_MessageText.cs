using Microsoft.EntityFrameworkCore.Migrations;

namespace FinnFragen.Web.Data.Migrations
{
    public partial class MessageText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MessageText",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageText",
                table: "Messages");
        }
    }
}
