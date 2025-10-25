using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inzynierka.Migrations
{
    /// <inheritdoc />
    public partial class mig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAiGenerated",
                table: "Products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "estimatedCarbohydrates",
                table: "Products",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "estimatedFats",
                table: "Products",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "estimatedProteins",
                table: "Products",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAiGenerated",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "estimatedCarbohydrates",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "estimatedFats",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "estimatedProteins",
                table: "Products");
        }
    }
}
