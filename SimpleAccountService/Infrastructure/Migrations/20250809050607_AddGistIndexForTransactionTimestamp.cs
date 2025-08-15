using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simple_Account_Service.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGistIndexForTransactionTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_transactions_timestamp",
                table: "transactions",
                column: "timestamp")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transactions_timestamp",
                table: "transactions");
        }
    }
}
