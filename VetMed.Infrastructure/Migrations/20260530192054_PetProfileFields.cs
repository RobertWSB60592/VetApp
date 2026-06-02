using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetMed.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PetProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Species",
                table: "Pets",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Pets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Pets",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MicrochipNumber",
                table: "Pets",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Pets",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "Pets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Nieznana");

            migrationBuilder.AddColumn<bool>(
                name: "Sterilized",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "MicrochipNumber",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Sterilized",
                table: "Pets");

            migrationBuilder.AlterColumn<string>(
                name: "Species",
                table: "Pets",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Pets",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
