using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simple_Account_Service.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class polishing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "processed_at",
                table: "outbox_messages",
                newName: "published_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "published_at",
                table: "outbox_messages",
                newName: "processed_at");
        }
    }
}
