using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simple_Account_Service.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsForHeaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "causation_id",
                table: "outbox_messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "correlation_id",
                table: "outbox_messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "version",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "causation_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "source",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "version",
                table: "outbox_messages");
        }
    }
}
