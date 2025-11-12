using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inzynierka.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlanReceiptRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlans_Receipts_ReceiptId",
                table: "MealPlans");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlans_Receipts_ReceiptId",
                table: "MealPlans",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlans_Receipts_ReceiptId",
                table: "MealPlans");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlans_Receipts_ReceiptId",
                table: "MealPlans",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id");
        }
    }
}
