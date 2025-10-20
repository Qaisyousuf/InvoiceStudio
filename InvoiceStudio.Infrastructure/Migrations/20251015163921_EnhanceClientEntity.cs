using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceStudio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceClientEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId1",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Siret",
                table: "Clients",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: true,
                comment: "French SIRET number (14 digits)",
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PreferredCurrency",
                table: "Clients",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "EUR",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentTermDays",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Clients",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Internal notes about the client",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogoPath",
                table: "Clients",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "Path to client logo file",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "IntraCommunityVatFr",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "French intra-community VAT number",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DanishVatNumber",
                table: "Clients",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true,
                comment: "Danish VAT number",
                oldClrType: typeof(string),
                oldType: "nvarchar(12)",
                oldMaxLength: 12,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CvrNumber",
                table: "Clients",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                comment: "Danish CVR number (8 digits)",
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AveragePaymentDays",
                table: "Clients",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

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

            migrationBuilder.AddColumn<string>(
                name: "Category",
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

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstInvoiceDate",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalReference",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastInvoiceDate",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverdueAmount",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

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
                name: "Priority",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<int>(
                name: "TotalInvoices",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRevenue",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "UseSeparateBillingAddress",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ClientAddress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressType = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientAddress_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactType = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contact_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClientId1",
                table: "Invoices",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Country_IsActive",
                table: "Clients",
                columns: new[] { "Country", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IsActive_Type",
                table: "Clients",
                columns: new[] { "IsActive", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Type",
                table: "Clients",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAddress_ClientId",
                table: "ClientAddress",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_ClientId",
                table: "Contact",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Clients_ClientId1",
                table: "Invoices",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Clients_ClientId1",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "ClientAddress");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ClientId1",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Country_IsActive",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_IsActive_Type",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Type",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AveragePaymentDays",
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
                name: "Category",
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
                name: "FirstInvoiceDate",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "InternalReference",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LastInvoiceDate",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OverdueAmount",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethod",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Priority",
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
                name: "TotalInvoices",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UseSeparateBillingAddress",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "Siret",
                table: "Clients",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14,
                oldNullable: true,
                oldComment: "French SIRET number (14 digits)");

            migrationBuilder.AlterColumn<string>(
                name: "PreferredCurrency",
                table: "Clients",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "EUR");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentTermDays",
                table: "Clients",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Clients",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "Internal notes about the client");

            migrationBuilder.AlterColumn<string>(
                name: "LogoPath",
                table: "Clients",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Path to client logo file");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Clients",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "IntraCommunityVatFr",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "French intra-community VAT number");

            migrationBuilder.AlterColumn<string>(
                name: "DanishVatNumber",
                table: "Clients",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)",
                oldMaxLength: 12,
                oldNullable: true,
                oldComment: "Danish VAT number");

            migrationBuilder.AlterColumn<string>(
                name: "CvrNumber",
                table: "Clients",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true,
                oldComment: "Danish CVR number (8 digits)");
        }
    }
}
