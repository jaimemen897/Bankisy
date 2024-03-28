using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TFG.Context.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bank_accounts",
                columns: table => new
                {
                    iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    balance = table.Column<decimal>(type: "numeric", nullable: false),
                    account_type = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_accounts", x => x.iban);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    dni = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    avatar = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    concept = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    iban_account_origin = table.Column<string>(type: "character varying(34)", nullable: false),
                    iban_account_destination = table.Column<string>(type: "character varying(34)", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_transactions_bank_accounts_iban_account_destination",
                        column: x => x.iban_account_destination,
                        principalTable: "bank_accounts",
                        principalColumn: "iban",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transactions_bank_accounts_iban_account_origin",
                        column: x => x.iban_account_origin,
                        principalTable: "bank_accounts",
                        principalColumn: "iban",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cards",
                columns: table => new
                {
                    card_number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    pin = table.Column<string>(type: "text", nullable: false),
                    card_type = table.Column<int>(type: "integer", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cvv = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_iban = table.Column<string>(type: "character varying(34)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cards", x => x.card_number);
                    table.ForeignKey(
                        name: "FK_cards_bank_accounts_bank_account_iban",
                        column: x => x.bank_account_iban,
                        principalTable: "bank_accounts",
                        principalColumn: "iban",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cards_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBankAccount",
                columns: table => new
                {
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountsId = table.Column<string>(type: "character varying(34)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBankAccount", x => new { x.UsersId, x.BankAccountsId });
                    table.ForeignKey(
                        name: "FK_UserBankAccount_bank_accounts_BankAccountsId",
                        column: x => x.BankAccountsId,
                        principalTable: "bank_accounts",
                        principalColumn: "iban",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBankAccount_users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cards_bank_account_iban",
                table: "cards",
                column: "bank_account_iban");

            migrationBuilder.CreateIndex(
                name: "IX_cards_user_id",
                table: "cards",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_iban_account_destination",
                table: "transactions",
                column: "iban_account_destination");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_iban_account_origin",
                table: "transactions",
                column: "iban_account_origin");

            migrationBuilder.CreateIndex(
                name: "IX_UserBankAccount_BankAccountsId",
                table: "UserBankAccount",
                column: "BankAccountsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cards");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "UserBankAccount");

            migrationBuilder.DropTable(
                name: "bank_accounts");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
