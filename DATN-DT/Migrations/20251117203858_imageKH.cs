using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class imageKH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultImage",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultImage",
                table: "KhachHangs");
        }
    }
}
