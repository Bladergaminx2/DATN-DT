using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class hoadon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_KhuyenMais_IdKhuyenMai",
                table: "HoaDons");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_IdKhuyenMai",
                table: "HoaDons");

            migrationBuilder.DropColumn(
                name: "IdKhuyenMai",
                table: "HoaDons");

            migrationBuilder.AddColumn<int>(
                name: "SoLuong",
                table: "KhuyenMais",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhuongThucThanhToan",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SdtKhachHang",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdKhuyenMai",
                table: "HoaDonChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_IdKhuyenMai",
                table: "HoaDonChiTiets",
                column: "IdKhuyenMai");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_KhuyenMais_IdKhuyenMai",
                table: "HoaDonChiTiets",
                column: "IdKhuyenMai",
                principalTable: "KhuyenMais",
                principalColumn: "IdKhuyenMai",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_KhuyenMais_IdKhuyenMai",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_IdKhuyenMai",
                table: "HoaDonChiTiets");

            migrationBuilder.DropColumn(
                name: "SoLuong",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "PhuongThucThanhToan",
                table: "HoaDons");

            migrationBuilder.DropColumn(
                name: "SdtKhachHang",
                table: "HoaDons");

            migrationBuilder.DropColumn(
                name: "IdKhuyenMai",
                table: "HoaDonChiTiets");

            migrationBuilder.AddColumn<int>(
                name: "IdKhuyenMai",
                table: "HoaDons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_IdKhuyenMai",
                table: "HoaDons",
                column: "IdKhuyenMai");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_KhuyenMais_IdKhuyenMai",
                table: "HoaDons",
                column: "IdKhuyenMai",
                principalTable: "KhuyenMais",
                principalColumn: "IdKhuyenMai",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
