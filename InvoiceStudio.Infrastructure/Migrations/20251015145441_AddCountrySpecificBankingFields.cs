using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceStudio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCountrySpecificBankingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PrimaryColor",
                table: "Companies",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogoPath",
                table: "Companies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "Companies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DanishAccountNumber",
                table: "Companies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "Konto nr - Danish account number");

            migrationBuilder.AddColumn<string>(
                name: "DanishRegistrationNumber",
                table: "Companies",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                comment: "Reg.nr - 4 digit bank identifier");

            migrationBuilder.AddColumn<string>(
                name: "FrenchAccountNumber",
                table: "Companies",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                comment: "Numéro de Compte - 11 alphanumeric");

            migrationBuilder.AddColumn<string>(
                name: "FrenchBankCode",
                table: "Companies",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                comment: "Code Banque - 5 digits");

            migrationBuilder.AddColumn<string>(
                name: "FrenchBranchCode",
                table: "Companies",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                comment: "Code Guichet - 5 digits");

            migrationBuilder.AddColumn<string>(
                name: "FrenchRibKey",
                table: "Companies",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                comment: "Clé RIB - 2 digits");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_DanishRegistrationNumber",
                table: "Companies",
                column: "DanishRegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_FrenchBankCode",
                table: "Companies",
                column: "FrenchBankCode");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Iban",
                table: "Companies",
                column: "Iban");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_DanishRegistrationNumber",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_FrenchBankCode",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Iban",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DanishAccountNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DanishRegistrationNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FrenchAccountNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FrenchBankCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FrenchBranchCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FrenchRibKey",
                table: "Companies");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryColor",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogoPath",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
