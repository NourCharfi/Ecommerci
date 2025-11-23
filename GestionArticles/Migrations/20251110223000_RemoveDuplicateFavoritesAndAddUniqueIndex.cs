using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionArticles.Migrations
{
    public partial class RemoveDuplicateFavoritesAndAddUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove duplicate favorite rows keeping the lowest FavoriteId
            migrationBuilder.Sql(@"
WITH CTE AS (
    SELECT FavoriteId, ROW_NUMBER() OVER (PARTITION BY UserId, ProductId ORDER BY FavoriteId) AS rn
    FROM Favorites
)
DELETE FROM Favorites WHERE FavoriteId IN (SELECT FavoriteId FROM CTE WHERE rn > 1);
");

            // Create a unique index to prevent future duplicates
            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_ProductId",
                table: "Favorites",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Favorites_UserId_ProductId",
                table: "Favorites");
        }
    }
}
