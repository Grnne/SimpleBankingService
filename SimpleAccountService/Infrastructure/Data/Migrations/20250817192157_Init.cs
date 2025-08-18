using Microsoft.EntityFrameworkCore.Migrations;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;

#nullable disable

namespace Simple_Account_Service.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type", "checking,credit,deposit")
                .Annotation("Npgsql:Enum:transaction_type", "credit,debit");

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<AccountType>(type: "account_type", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric", nullable: true),
                    interest_rate = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    last_interest_accrual_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    frozen = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_consumed_messages",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    handler = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_consumed_messages", x => x.message_id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_dead_letters",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    handler = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    causation_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_dead_letters", x => x.message_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    published = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    counterparty_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    type = table.Column<TransactionType>(type: "transaction_type", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_transactions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_owner_id",
                table: "accounts",
                column: "owner_id")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_account_id_timestamp",
                table: "transactions",
                columns: new[] { "account_id", "timestamp" });

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE accrue_interest(account_id UUID)
                LANGUAGE plpgsql AS $$
                DECLARE
                    principal         numeric;
                    rate              numeric;
                    daily_rate        numeric;
                    days_passed       int;
                    interest          numeric;
                    last_accrual_date date;
                    current_date      date := CURRENT_DATE;
                    acc_type          public.account_type;
                BEGIN
                    SELECT balance, interest_rate, last_interest_accrual_at, type
                    INTO principal, rate, last_accrual_date, acc_type
                    FROM accounts
                    WHERE id = account_id;

                    IF principal IS NULL THEN
                        RAISE EXCEPTION 'Счет не найден: %', account_id;
                    END IF;

                    IF rate IS NULL OR rate <= 0 THEN
                        RETURN;
                    END IF;

                    IF last_accrual_date IS NULL THEN
						days_passed := 1;
					ELSE
						days_passed := current_date - last_accrual_date;
                    END IF;

                    IF days_passed <= 0 THEN
                        RETURN;
                    END IF;

                    daily_rate := rate / 365;

                    interest := abs(principal) * power(1 + daily_rate, days_passed) - abs(principal);

                    IF acc_type = 'deposit' THEN
                        UPDATE accounts
                        SET balance = balance + interest,
                        last_interest_accrual_at = current_date
                        WHERE id = account_id;
                    ELSIF acc_type = 'credit' THEN
                        UPDATE accounts
                        SET balance = balance - interest,
                        last_interest_accrual_at = current_date
                        WHERE id = account_id;
                    ELSE
                        RETURN;
                    END IF;

                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_consumed_messages");

            migrationBuilder.DropTable(
                name: "inbox_dead_letters");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.Sql("DROP EXTENSION IF EXISTS btree_gist;");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS accrue_interest(uuid);");
        }
    }
}
