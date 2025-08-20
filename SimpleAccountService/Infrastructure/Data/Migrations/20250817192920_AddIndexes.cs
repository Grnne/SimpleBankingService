using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simple_Account_Service.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
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

        }
    }
}
