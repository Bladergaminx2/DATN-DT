using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class suakhuyenmai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonHangChiTiets_KhuyenMais_IdKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_KhuyenMais_IdKhuyenMai",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_IdKhuyenMai",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_DonHangChiTiets_IdKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.DropColumn(
                name: "IdKhuyenMai",
                table: "HoaDonChiTiets");

            migrationBuilder.DropColumn(
                name: "IdKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.AddColumn<decimal>(
                name: "GiaKhuyenMai",
                table: "HoaDonChiTiets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaKhuyenMai",
                table: "DonHangChiTiets",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiaKhuyenMai",
                table: "HoaDonChiTiets");

            migrationBuilder.DropColumn(
                name: "GiaKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.AddColumn<int>(
                name: "IdKhuyenMai",
                table: "HoaDonChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdKhuyenMai",
                table: "DonHangChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_IdKhuyenMai",
                table: "HoaDonChiTiets",
                column: "IdKhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangChiTiets_IdKhuyenMai",
                table: "DonHangChiTiets",
                column: "IdKhuyenMai");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangChiTiets_KhuyenMais_IdKhuyenMai",
                table: "DonHangChiTiets",
                column: "IdKhuyenMai",
                principalTable: "KhuyenMais",
                principalColumn: "IdKhuyenMai",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_KhuyenMais_IdKhuyenMai",
                table: "HoaDonChiTiets",
                column: "IdKhuyenMai",
                principalTable: "KhuyenMais",
                principalColumn: "IdKhuyenMai",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
