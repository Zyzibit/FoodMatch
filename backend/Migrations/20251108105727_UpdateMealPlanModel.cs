using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inzynierka.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMealPlanModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "MealPlans");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MealPlans",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "MealPlans");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "MealPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
