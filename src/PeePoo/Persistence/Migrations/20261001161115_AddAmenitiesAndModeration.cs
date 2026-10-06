using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAmenitiesAndModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Visits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAccessible",
                table: "Places",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFree",
                table: "Places",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            NormalizeLegacyData(migrationBuilder);
        }

        private static void NormalizeLegacyData(MigrationBuilder migrationBuilder)
        {
            // Older clients stored free-form type names; map them onto the supported list.
            migrationBuilder.Sql("UPDATE Places SET Type = 'Family' WHERE Type IN ('Familiar', 'Familia', 'Family ')");
            migrationBuilder.Sql("UPDATE Places SET Type = 'Men' WHERE Type IN ('Mens Only', 'Men Only', 'Hombres')");
            migrationBuilder.Sql("UPDATE Places SET Type = 'Women' WHERE Type IN ('Womens Only', 'Women Only', 'Mujeres')");
            migrationBuilder.Sql("UPDATE Places SET Type = 'Unisex' WHERE Type NOT IN ('Unisex', 'Men', 'Women', 'Family', 'Accessible') OR Type IS NULL");
            migrationBuilder.Sql("UPDATE Places SET IsAccessible = 1 WHERE Type = 'Accessible'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "IsAccessible",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "IsFree",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "OpeningHours",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");
        }
    }
}
