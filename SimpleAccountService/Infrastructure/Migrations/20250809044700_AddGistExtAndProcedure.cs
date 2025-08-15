using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simple_Account_Service.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGistExtAndProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_interest_accrual_at",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: true);

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
            migrationBuilder.DropColumn(
                name: "last_interest_accrual_at",
                table: "accounts");

            migrationBuilder.Sql("DROP EXTENSION IF EXISTS btree_gist;");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS accrue_interest(uuid);");
        }
    }
}
