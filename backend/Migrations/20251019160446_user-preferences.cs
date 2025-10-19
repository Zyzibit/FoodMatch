using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inzynierka.Migrations
{
    /// <inheritdoc />
    public partial class userpreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FoodPreferences_DailyCalorieGoal",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FoodPreferences_DailyCarbohydrateGoal",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FoodPreferences_DailyFatGoal",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FoodPreferences_DailyProteinGoal",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "FoodPreferences_HasGlutenIntolerance",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FoodPreferences_HasLactoseIntolerance",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FoodPreferences_HasNutAllergy",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FoodPreferences_IsVegan",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FoodPreferences_IsVegetarian",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FoodPreferences_DailyCalorieGoal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FoodPreferences_DailyCarbohydrateGoal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FoodPreferences_DailyFatGoal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FoodPreferences_DailyProteinGoal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FoodPreferences_HasGlutenIntolerance",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FoodPreferences_HasLactoseIntolerance",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FoodPreferences_HasNutAllergy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FoodPreferences_IsVegan",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FoodPreferences_IsVegetarian",
                table: "AspNetUsers");
        }
    }
}
