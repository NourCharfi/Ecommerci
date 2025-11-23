using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionArticles.Migrations
{
    /// <inheritdoc />
    public partial class updateInfAPPoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyXGetYFreeQuantity",
                table: "Discounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuyXGetYFreeQuantity",
                table: "Discounts",
                type: "int",
                nullable: true);
        }
    }
}
