using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceStudio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorClientEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_CvrNumber",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Siret",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BillingCountryName",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BillingPostalCode",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BillingStreet",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "EmailInvoices",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "EmailMarketing",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "EmailReminders",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IntraCommunityVatFr",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethod",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "RiskNotes",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UseSeparateBillingAddress",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "VatNumber",
                table: "Clients");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_IsActive_Type",
                table: "Clients",
                newName: "IX_Clients_Active_Type");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_Country_IsActive",
                table: "Clients",
                newName: "IX_Clients_Country_Active");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalRevenue",
                table: "Clients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "TotalInvoices",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "OverdueAmount",
                table: "Clients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastInvoiceDate",
                table: "Clients",
                type: "datetime2",
                nullable: true,
                comment: "Date of most recent invoice",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InternalReference",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "Internal client reference code",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FirstInvoiceDate",
                table: "Clients",
                type: "datetime2",
                nullable: true,
                comment: "Date of first invoice for this client",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "Client category (VIP, Regular, Prospect, etc.)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AveragePaymentDays",
                table: "Clients",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "Siren",
                table: "Clients",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true,
                comment: "French SIREN number (9 digits)");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Category",
                table: "Clients",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Category_Active",
                table: "Clients",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CvrNumber",
                table: "Clients",
                column: "CvrNumber",
                unique: true,
                filter: "[CvrNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_LastInvoiceDate",
                table: "Clients",
                column: "LastInvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OverdueAmount",
                table: "Clients",
                column: "OverdueAmount",
                filter: "[OverdueAmount] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Priority",
                table: "Clients",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Siren",
                table: "Clients",
                column: "Siren",
                filter: "[Siren] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Siret",
                table: "Clients",
                column: "Siret",
                unique: true,
                filter: "[Siret] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TotalRevenue",
                table: "Clients",
                column: "TotalRevenue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Category",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Category_Active",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_CvrNumber",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_LastInvoiceDate",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_OverdueAmount",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Priority",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Siren",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Siret",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TotalRevenue",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Siren",
                table: "Clients");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_Country_Active",
                table: "Clients",
                newName: "IX_Clients_Country_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_Active_Type",
                table: "Clients",
                newName: "IX_Clients_IsActive_Type");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalRevenue",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "TotalInvoices",
                table: "Clients",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "OverdueAmount",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastInvoiceDate",
                table: "Clients",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Date of most recent invoice");

            migrationBuilder.AlterColumn<string>(
                name: "InternalReference",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Internal client reference code");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FirstInvoiceDate",
                table: "Clients",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Date of first invoice for this client");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Client category (VIP, Regular, Prospect, etc.)");

            migrationBuilder.AlterColumn<double>(
                name: "AveragePaymentDays",
                table: "Clients",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountryName",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingPostalCode",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingStreet",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailInvoices",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailMarketing",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailReminders",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IntraCommunityVatFr",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "French intra-community VAT number");

            migrationBuilder.AddColumn<string>(
                name: "PreferredContactMethod",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RiskLevel",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RiskNotes",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseSeparateBillingAddress",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VatNumber",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CvrNumber",
                table: "Clients",
                column: "CvrNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Siret",
                table: "Clients",
                column: "Siret");
        }
    }
}
