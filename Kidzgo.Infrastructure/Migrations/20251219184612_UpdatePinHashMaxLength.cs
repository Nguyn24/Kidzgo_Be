using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePinHashMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PinHash",
                schema: "public",
                table: "Profiles",
                type: "character varying(97)",
                maxLength: 97,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PinHash",
                schema: "public",
                table: "Profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(97)",
                oldMaxLength: 97,
                oldNullable: true);
        }
    }
}
