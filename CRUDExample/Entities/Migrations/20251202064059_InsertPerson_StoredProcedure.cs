using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class InsertPerson_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"CREATE PROCEDURE [dbo].[InsertPerson]
            (@Id uniqueidentifier, @Name nvarchar(40), @Email nvarchar(40), @DateOfBirth datetime2(7), @Gender nvarchar(10), @CountryID uniqueidentifier, @Address nvarchar(200), @ReceiveNewsLetters bit)
            AS BEGIN
                INSERT INTO [dbo].[Persons](Id, Name, Email, DateOfBirth, Gender, CountryID, Address, ReceiveNewsLetters)
                VALUES (@Id, @Name, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters)
            END";

            migrationBuilder.Sql(sp_InsertPerson);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"DROP PROCEDURE [dbo].[GetAllPersons]";
            migrationBuilder.Sql(sp_InsertPerson);
        }
    }
}
