using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inzynierka.Migrations
{
    /// <inheritdoc />
    public partial class unitupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Units",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PromptDescription",
                table: "Units",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "PromptDescription",
                table: "Units");
        }
    }
}
