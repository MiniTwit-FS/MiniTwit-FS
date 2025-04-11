using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTwitAPI.Migrations
{
    /// <inheritdoc />
    public partial class Add_HashedPasswordTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPasswordHashed",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPasswordHashed",
                table: "Users");
        }
    }
}
