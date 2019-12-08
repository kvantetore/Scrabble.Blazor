using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Scrabble.Web.Server.Migrations
{
    public partial class GameEnd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "End",
                table: "Games",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "End",
                table: "Games");
        }
    }
}
