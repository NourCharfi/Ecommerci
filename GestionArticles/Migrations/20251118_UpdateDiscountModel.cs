using Microsoft.EntityFrameworkCore.Migrations;

namespace GestionArticles.Migrations
{
    public partial class UpdateDiscountModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ajouter la colonne BuyXGetYFreeQuantity si elle n'existe pas
            migrationBuilder.AddColumn<int>(
                name: "BuyXGetYFreeQuantity",
                table: "Discounts",
                type: "int",
                nullable: true);

            // Ajouter la colonne Description si elle n'existe pas
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyXGetYFreeQuantity",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Discounts");
        }
    }
}
