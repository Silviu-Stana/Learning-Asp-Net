using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class ModifyStoredProceduresToIncludeTIN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string dropIfExists = @"IF OBJECT_ID('[dbo].[InsertPerson]', 'P') IS NOT NULL
                DROP PROCEDURE [dbo].[InsertPerson];";

            migrationBuilder.Sql(dropIfExists);

            migrationBuilder.Sql(@"
            CREATE PROCEDURE [dbo].[InsertPerson]
            (
                @Id UNIQUEIDENTIFIER,
                @Name NVARCHAR(40),
                @Email NVARCHAR(40),
                @DateOfBirth DATETIME2(7),
                @Gender NVARCHAR(10),
                @CountryID UNIQUEIDENTIFIER,
                @Address NVARCHAR(200),
                @ReceiveNewsLetters BIT
            )
            AS
            BEGIN
                INSERT INTO [dbo].[Persons]
                (Id, Name, Email, DateOfBirth, Gender, CountryID, Address, ReceiveNewsLetters)
                VALUES
                (@Id, @Name, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters)
            END
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"DROP PROCEDURE [dbo].[GetAllPersons]";
            migrationBuilder.Sql(sp_InsertPerson);
        }
    }
}
