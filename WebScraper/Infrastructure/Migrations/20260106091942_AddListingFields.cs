using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ListingId (nullable) and Views (non-nullable, default 0)
            migrationBuilder.AddColumn<string>(
                name: "ListingId",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListingId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "Listings");
        }
    }
}
