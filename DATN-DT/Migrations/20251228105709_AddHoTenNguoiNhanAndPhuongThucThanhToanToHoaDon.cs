using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class AddHoTenNguoiNhanAndPhuongThucThanhToanToHoaDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_HoaDons_IdHoaDon",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_KhachHangs_IdKhachHang",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_NhanViens_IdNhanVien",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_HoaDons_IdHoaDon",
                table: "ThanhToans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HoaDons",
                table: "HoaDons");

            migrationBuilder.RenameTable(
                name: "HoaDons",
                newName: "HoaDon");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDons_IdNhanVien",
                table: "HoaDon",
                newName: "IX_HoaDon_IdNhanVien");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDons_IdKhachHang",
                table: "HoaDon",
                newName: "IX_HoaDon_IdKhachHang");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HoaDon",
                table: "HoaDon",
                column: "IdHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_KhachHangs_IdKhachHang",
                table: "HoaDon",
                column: "IdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_NhanViens_IdNhanVien",
                table: "HoaDon",
                column: "IdNhanVien",
                principalTable: "NhanViens",
                principalColumn: "IdNhanVien",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_HoaDon_IdHoaDon",
                table: "HoaDonChiTiets",
                column: "IdHoaDon",
                principalTable: "HoaDon",
                principalColumn: "IdHoaDon",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_HoaDon_IdHoaDon",
                table: "ThanhToans",
                column: "IdHoaDon",
                principalTable: "HoaDon",
                principalColumn: "IdHoaDon",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_KhachHangs_IdKhachHang",
                table: "HoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_NhanViens_IdNhanVien",
                table: "HoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_HoaDon_IdHoaDon",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_HoaDon_IdHoaDon",
                table: "ThanhToans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HoaDon",
                table: "HoaDon");

            migrationBuilder.RenameTable(
                name: "HoaDon",
                newName: "HoaDons");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDon_IdNhanVien",
                table: "HoaDons",
                newName: "IX_HoaDons_IdNhanVien");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDon_IdKhachHang",
                table: "HoaDons",
                newName: "IX_HoaDons_IdKhachHang");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HoaDons",
                table: "HoaDons",
                column: "IdHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_HoaDons_IdHoaDon",
                table: "HoaDonChiTiets",
                column: "IdHoaDon",
                principalTable: "HoaDons",
                principalColumn: "IdHoaDon",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_KhachHangs_IdKhachHang",
                table: "HoaDons",
                column: "IdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_NhanViens_IdNhanVien",
                table: "HoaDons",
                column: "IdNhanVien",
                principalTable: "NhanViens",
                principalColumn: "IdNhanVien",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_HoaDons_IdHoaDon",
                table: "ThanhToans",
                column: "IdHoaDon",
                principalTable: "HoaDons",
                principalColumn: "IdHoaDon",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
