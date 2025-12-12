using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class adressuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChiKhachHang",
                table: "KhachHangs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiaChiKhachHang",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
