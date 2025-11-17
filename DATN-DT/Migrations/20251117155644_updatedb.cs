using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class updatedb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnhSanPhams_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "AnhSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoHanhs_Imeis_ImeiIdImei",
                table: "BaoHanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoHanhs_KhachHangs_KhachHangIdKhachHang",
                table: "BaoHanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoHanhs_NhanViens_NhanVienIdNhanVien",
                table: "BaoHanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangChiTiets_DonHangs_DonHangIdDonHang",
                table: "DonHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangChiTiets_KhuyenMais_KhuyenMaiIdKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "DonHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_KhachHangs_KhachHangIdKhachHang",
                table: "DonHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangChiTiets_GioHangs_GioHangIdGioHang",
                table: "GioHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "GioHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_KhachHangs_KhachHangIdKhachHang",
                table: "GioHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_HoaDons_HoaDonIdHoaDon",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_Imeis_ImeiIdImei",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_KhachHangs_KhachHangIdKhachHang",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_KhuyenMais_KhuyenMaiIdKhuyenMai",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_NhanViens_NhanVienIdNhanVien",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_Imeis_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "Imeis");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_CameraSaus_CameraSauIdCameraSau",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_CameraTruocs_CameraTruocIdCamTruoc",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_ManHinhs_ManHinhIdManHinh",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_Pins_PinIdPin",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_RAMs_RAMIdRAM",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_ROMs_ROMIdROM",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_SanPhams_SanPhamIdSanPham",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_NhanViens_ChucVus_ChucVuIdChucVu",
                table: "NhanViens");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_ThuongHieus_ThuongHieuIdThuongHieu",
                table: "SanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_HoaDons_HoaDonIdHoaDon",
                table: "ThanhToans");

            migrationBuilder.DropForeignKey(
                name: "FK_TonKhos_Khos_KhoIdKho",
                table: "TonKhos");

            migrationBuilder.DropForeignKey(
                name: "FK_TonKhos_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "TonKhos");

            migrationBuilder.DropIndex(
                name: "IX_TonKhos_KhoIdKho",
                table: "TonKhos");

            migrationBuilder.DropIndex(
                name: "IX_TonKhos_ModelSanPhamIdModelSanPham",
                table: "TonKhos");

            migrationBuilder.DropIndex(
                name: "IX_ThanhToans_HoaDonIdHoaDon",
                table: "ThanhToans");

            migrationBuilder.DropIndex(
                name: "IX_SanPhams_ThuongHieuIdThuongHieu",
                table: "SanPhams");

            migrationBuilder.DropIndex(
                name: "IX_NhanViens_ChucVuIdChucVu",
                table: "NhanViens");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_CameraSauIdCameraSau",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_CameraTruocIdCamTruoc",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_ManHinhIdManHinh",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_PinIdPin",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_RAMIdRAM",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_ROMIdROM",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_SanPhamIdSanPham",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_Imeis_ModelSanPhamIdModelSanPham",
                table: "Imeis");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_KhachHangIdKhachHang",
                table: "HoaDons");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_KhuyenMaiIdKhuyenMai",
                table: "HoaDons");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_NhanVienIdNhanVien",
                table: "HoaDons");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_HoaDonIdHoaDon",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_ImeiIdImei",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_ModelSanPhamIdModelSanPham",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_GioHangs_KhachHangIdKhachHang",
                table: "GioHangs");

            migrationBuilder.DropIndex(
                name: "IX_GioHangChiTiets_GioHangIdGioHang",
                table: "GioHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_GioHangChiTiets_ModelSanPhamIdModelSanPham",
                table: "GioHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_DonHangs_KhachHangIdKhachHang",
                table: "DonHangs");

            migrationBuilder.DropIndex(
                name: "IX_DonHangChiTiets_DonHangIdDonHang",
                table: "DonHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_DonHangChiTiets_KhuyenMaiIdKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_DonHangChiTiets_ModelSanPhamIdModelSanPham",
                table: "DonHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_BaoHanhs_ImeiIdImei",
                table: "BaoHanhs");

            migrationBuilder.DropIndex(
                name: "IX_BaoHanhs_KhachHangIdKhachHang",
                table: "BaoHanhs");

            migrationBuilder.DropIndex(
                name: "IX_BaoHanhs_NhanVienIdNhanVien",
                table: "BaoHanhs");

            migrationBuilder.DropIndex(
                name: "IX_AnhSanPhams_ModelSanPhamIdModelSanPham",
                table: "AnhSanPhams");

            migrationBuilder.DropColumn(
                name: "KhoIdKho",
                table: "TonKhos");

            migrationBuilder.DropColumn(
                name: "ModelSanPhamIdModelSanPham",
                table: "TonKhos");

            migrationBuilder.DropColumn(
                name: "HoaDonIdHoaDon",
                table: "ThanhToans");

            migrationBuilder.DropColumn(
                name: "ThuongHieuIdThuongHieu",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "ChucVuIdChucVu",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "CameraSauIdCameraSau",
                table: "ModelSanPhams");

            migrationBuilder.DropColumn(
                name: "CameraTruocIdCamTruoc",
                table: "ModelSanPhams");

            migrationBuilder.DropColumn(
                name: "ManHinhIdManHinh",
                table: "ModelSanPhams");

            migrationBuilder.DropColumn(
                name: "PinIdPin",
                table: "ModelSanPhams");

            migrationBuilder.DropColumn(
                name: "RAMIdRAM",
                table: "ModelSanPhams");

            migrationBuilder.DropColumn(
                name: "ROMIdROM",
                table: "ModelSanPhams");

            migrationBuilder.DropColumn(
                name: "SanPhamIdSanPham",
                table: "ModelSanPhams");

            migrationBuilder.DropColumn(
                name: "ModelSanPhamIdModelSanPham",
                table: "Imeis");

            migrationBuilder.DropColumn(
                name: "KhachHangIdKhachHang",
                table: "HoaDons");

            migrationBuilder.DropColumn(
                name: "KhuyenMaiIdKhuyenMai",
                table: "HoaDons");

            migrationBuilder.DropColumn(
                name: "NhanVienIdNhanVien",
                table: "HoaDons");

            migrationBuilder.DropColumn(
                name: "HoaDonIdHoaDon",
                table: "HoaDonChiTiets");

            migrationBuilder.DropColumn(
                name: "ImeiIdImei",
                table: "HoaDonChiTiets");

            migrationBuilder.DropColumn(
                name: "ModelSanPhamIdModelSanPham",
                table: "HoaDonChiTiets");

            migrationBuilder.DropColumn(
                name: "KhachHangIdKhachHang",
                table: "GioHangs");

            migrationBuilder.DropColumn(
                name: "NgayTaoGio",
                table: "GioHangs");

            migrationBuilder.DropColumn(
                name: "TrangThaiGio",
                table: "GioHangs");

            migrationBuilder.DropColumn(
                name: "DonGia",
                table: "GioHangChiTiets");

            migrationBuilder.DropColumn(
                name: "GioHangIdGioHang",
                table: "GioHangChiTiets");

            migrationBuilder.DropColumn(
                name: "ModelSanPhamIdModelSanPham",
                table: "GioHangChiTiets");

            migrationBuilder.DropColumn(
                name: "ThanhTien",
                table: "GioHangChiTiets");

            migrationBuilder.DropColumn(
                name: "KhachHangIdKhachHang",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "DonHangIdDonHang",
                table: "DonHangChiTiets");

            migrationBuilder.DropColumn(
                name: "KhuyenMaiIdKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.DropColumn(
                name: "ModelSanPhamIdModelSanPham",
                table: "DonHangChiTiets");

            migrationBuilder.DropColumn(
                name: "ImeiIdImei",
                table: "BaoHanhs");

            migrationBuilder.DropColumn(
                name: "KhachHangIdKhachHang",
                table: "BaoHanhs");

            migrationBuilder.DropColumn(
                name: "NhanVienIdNhanVien",
                table: "BaoHanhs");

            migrationBuilder.DropColumn(
                name: "ModelSanPhamIdModelSanPham",
                table: "AnhSanPhams");

            migrationBuilder.AlterColumn<int>(
                name: "TrangThaiKhachHang",
                table: "KhachHangs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TonKhos_IdKho",
                table: "TonKhos",
                column: "IdKho");

            migrationBuilder.CreateIndex(
                name: "IX_TonKhos_IdModelSanPham",
                table: "TonKhos",
                column: "IdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_IdHoaDon",
                table: "ThanhToans",
                column: "IdHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_IdThuongHieu",
                table: "SanPhams",
                column: "IdThuongHieu");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_IdChucVu",
                table: "NhanViens",
                column: "IdChucVu");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_IdCameraSau",
                table: "ModelSanPhams",
                column: "IdCameraSau");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_IdCameraTruoc",
                table: "ModelSanPhams",
                column: "IdCameraTruoc");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_IdManHinh",
                table: "ModelSanPhams",
                column: "IdManHinh");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_IdPin",
                table: "ModelSanPhams",
                column: "IdPin");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_IdRAM",
                table: "ModelSanPhams",
                column: "IdRAM");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_IdROM",
                table: "ModelSanPhams",
                column: "IdROM");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_IdSanPham",
                table: "ModelSanPhams",
                column: "IdSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_Imeis_IdModelSanPham",
                table: "Imeis",
                column: "IdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_IdKhachHang",
                table: "HoaDons",
                column: "IdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_IdKhuyenMai",
                table: "HoaDons",
                column: "IdKhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_IdNhanVien",
                table: "HoaDons",
                column: "IdNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_IdHoaDon",
                table: "HoaDonChiTiets",
                column: "IdHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_IdImei",
                table: "HoaDonChiTiets",
                column: "IdImei");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_IdModelSanPham",
                table: "HoaDonChiTiets",
                column: "IdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_IdKhachHang",
                table: "GioHangs",
                column: "IdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_IdGioHang",
                table: "GioHangChiTiets",
                column: "IdGioHang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_IdModelSanPham",
                table: "GioHangChiTiets",
                column: "IdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_IdKhachHang",
                table: "DonHangs",
                column: "IdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangChiTiets_IdDonHang",
                table: "DonHangChiTiets",
                column: "IdDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangChiTiets_IdKhuyenMai",
                table: "DonHangChiTiets",
                column: "IdKhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangChiTiets_IdModelSanPham",
                table: "DonHangChiTiets",
                column: "IdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_BaoHanhs_IdImei",
                table: "BaoHanhs",
                column: "IdImei");

            migrationBuilder.CreateIndex(
                name: "IX_BaoHanhs_IdKhachHang",
                table: "BaoHanhs",
                column: "IdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_BaoHanhs_IdNhanVien",
                table: "BaoHanhs",
                column: "IdNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_AnhSanPhams_IdModelSanPham",
                table: "AnhSanPhams",
                column: "IdModelSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_AnhSanPhams_ModelSanPhams_IdModelSanPham",
                table: "AnhSanPhams",
                column: "IdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoHanhs_Imeis_IdImei",
                table: "BaoHanhs",
                column: "IdImei",
                principalTable: "Imeis",
                principalColumn: "IdImei",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoHanhs_KhachHangs_IdKhachHang",
                table: "BaoHanhs",
                column: "IdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoHanhs_NhanViens_IdNhanVien",
                table: "BaoHanhs",
                column: "IdNhanVien",
                principalTable: "NhanViens",
                principalColumn: "IdNhanVien",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangChiTiets_DonHangs_IdDonHang",
                table: "DonHangChiTiets",
                column: "IdDonHang",
                principalTable: "DonHangs",
                principalColumn: "IdDonHang",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangChiTiets_KhuyenMais_IdKhuyenMai",
                table: "DonHangChiTiets",
                column: "IdKhuyenMai",
                principalTable: "KhuyenMais",
                principalColumn: "IdKhuyenMai",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangChiTiets_ModelSanPhams_IdModelSanPham",
                table: "DonHangChiTiets",
                column: "IdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_KhachHangs_IdKhachHang",
                table: "DonHangs",
                column: "IdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangChiTiets_GioHangs_IdGioHang",
                table: "GioHangChiTiets",
                column: "IdGioHang",
                principalTable: "GioHangs",
                principalColumn: "IdGioHang",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangChiTiets_ModelSanPhams_IdModelSanPham",
                table: "GioHangChiTiets",
                column: "IdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_KhachHangs_IdKhachHang",
                table: "GioHangs",
                column: "IdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_HoaDons_IdHoaDon",
                table: "HoaDonChiTiets",
                column: "IdHoaDon",
                principalTable: "HoaDons",
                principalColumn: "IdHoaDon",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_Imeis_IdImei",
                table: "HoaDonChiTiets",
                column: "IdImei",
                principalTable: "Imeis",
                principalColumn: "IdImei",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_ModelSanPhams_IdModelSanPham",
                table: "HoaDonChiTiets",
                column: "IdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_KhachHangs_IdKhachHang",
                table: "HoaDons",
                column: "IdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_KhuyenMais_IdKhuyenMai",
                table: "HoaDons",
                column: "IdKhuyenMai",
                principalTable: "KhuyenMais",
                principalColumn: "IdKhuyenMai",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_NhanViens_IdNhanVien",
                table: "HoaDons",
                column: "IdNhanVien",
                principalTable: "NhanViens",
                principalColumn: "IdNhanVien",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imeis_ModelSanPhams_IdModelSanPham",
                table: "Imeis",
                column: "IdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_CameraSaus_IdCameraSau",
                table: "ModelSanPhams",
                column: "IdCameraSau",
                principalTable: "CameraSaus",
                principalColumn: "IdCameraSau",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_CameraTruocs_IdCameraTruoc",
                table: "ModelSanPhams",
                column: "IdCameraTruoc",
                principalTable: "CameraTruocs",
                principalColumn: "IdCamTruoc",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_ManHinhs_IdManHinh",
                table: "ModelSanPhams",
                column: "IdManHinh",
                principalTable: "ManHinhs",
                principalColumn: "IdManHinh",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_Pins_IdPin",
                table: "ModelSanPhams",
                column: "IdPin",
                principalTable: "Pins",
                principalColumn: "IdPin",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_RAMs_IdRAM",
                table: "ModelSanPhams",
                column: "IdRAM",
                principalTable: "RAMs",
                principalColumn: "IdRAM",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_ROMs_IdROM",
                table: "ModelSanPhams",
                column: "IdROM",
                principalTable: "ROMs",
                principalColumn: "IdROM",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_SanPhams_IdSanPham",
                table: "ModelSanPhams",
                column: "IdSanPham",
                principalTable: "SanPhams",
                principalColumn: "IdSanPham",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NhanViens_ChucVus_IdChucVu",
                table: "NhanViens",
                column: "IdChucVu",
                principalTable: "ChucVus",
                principalColumn: "IdChucVu",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_ThuongHieus_IdThuongHieu",
                table: "SanPhams",
                column: "IdThuongHieu",
                principalTable: "ThuongHieus",
                principalColumn: "IdThuongHieu",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_HoaDons_IdHoaDon",
                table: "ThanhToans",
                column: "IdHoaDon",
                principalTable: "HoaDons",
                principalColumn: "IdHoaDon",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TonKhos_Khos_IdKho",
                table: "TonKhos",
                column: "IdKho",
                principalTable: "Khos",
                principalColumn: "IdKho",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TonKhos_ModelSanPhams_IdModelSanPham",
                table: "TonKhos",
                column: "IdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnhSanPhams_ModelSanPhams_IdModelSanPham",
                table: "AnhSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoHanhs_Imeis_IdImei",
                table: "BaoHanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoHanhs_KhachHangs_IdKhachHang",
                table: "BaoHanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoHanhs_NhanViens_IdNhanVien",
                table: "BaoHanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangChiTiets_DonHangs_IdDonHang",
                table: "DonHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangChiTiets_KhuyenMais_IdKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangChiTiets_ModelSanPhams_IdModelSanPham",
                table: "DonHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_KhachHangs_IdKhachHang",
                table: "DonHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangChiTiets_GioHangs_IdGioHang",
                table: "GioHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangChiTiets_ModelSanPhams_IdModelSanPham",
                table: "GioHangChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_KhachHangs_IdKhachHang",
                table: "GioHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_HoaDons_IdHoaDon",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_Imeis_IdImei",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_ModelSanPhams_IdModelSanPham",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_KhachHangs_IdKhachHang",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_KhuyenMais_IdKhuyenMai",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_NhanViens_IdNhanVien",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_Imeis_ModelSanPhams_IdModelSanPham",
                table: "Imeis");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_CameraSaus_IdCameraSau",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_CameraTruocs_IdCameraTruoc",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_ManHinhs_IdManHinh",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_Pins_IdPin",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_RAMs_IdRAM",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_ROMs_IdROM",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ModelSanPhams_SanPhams_IdSanPham",
                table: "ModelSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_NhanViens_ChucVus_IdChucVu",
                table: "NhanViens");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_ThuongHieus_IdThuongHieu",
                table: "SanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_HoaDons_IdHoaDon",
                table: "ThanhToans");

            migrationBuilder.DropForeignKey(
                name: "FK_TonKhos_Khos_IdKho",
                table: "TonKhos");

            migrationBuilder.DropForeignKey(
                name: "FK_TonKhos_ModelSanPhams_IdModelSanPham",
                table: "TonKhos");

            migrationBuilder.DropIndex(
                name: "IX_TonKhos_IdKho",
                table: "TonKhos");

            migrationBuilder.DropIndex(
                name: "IX_TonKhos_IdModelSanPham",
                table: "TonKhos");

            migrationBuilder.DropIndex(
                name: "IX_ThanhToans_IdHoaDon",
                table: "ThanhToans");

            migrationBuilder.DropIndex(
                name: "IX_SanPhams_IdThuongHieu",
                table: "SanPhams");

            migrationBuilder.DropIndex(
                name: "IX_NhanViens_IdChucVu",
                table: "NhanViens");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_IdCameraSau",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_IdCameraTruoc",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_IdManHinh",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_IdPin",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_IdRAM",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_IdROM",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_ModelSanPhams_IdSanPham",
                table: "ModelSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_Imeis_IdModelSanPham",
                table: "Imeis");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_IdKhachHang",
                table: "HoaDons");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_IdKhuyenMai",
                table: "HoaDons");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_IdNhanVien",
                table: "HoaDons");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_IdHoaDon",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_IdImei",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_IdModelSanPham",
                table: "HoaDonChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_GioHangs_IdKhachHang",
                table: "GioHangs");

            migrationBuilder.DropIndex(
                name: "IX_GioHangChiTiets_IdGioHang",
                table: "GioHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_GioHangChiTiets_IdModelSanPham",
                table: "GioHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_DonHangs_IdKhachHang",
                table: "DonHangs");

            migrationBuilder.DropIndex(
                name: "IX_DonHangChiTiets_IdDonHang",
                table: "DonHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_DonHangChiTiets_IdKhuyenMai",
                table: "DonHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_DonHangChiTiets_IdModelSanPham",
                table: "DonHangChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_BaoHanhs_IdImei",
                table: "BaoHanhs");

            migrationBuilder.DropIndex(
                name: "IX_BaoHanhs_IdKhachHang",
                table: "BaoHanhs");

            migrationBuilder.DropIndex(
                name: "IX_BaoHanhs_IdNhanVien",
                table: "BaoHanhs");

            migrationBuilder.DropIndex(
                name: "IX_AnhSanPhams_IdModelSanPham",
                table: "AnhSanPhams");

            migrationBuilder.AddColumn<int>(
                name: "KhoIdKho",
                table: "TonKhos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelSanPhamIdModelSanPham",
                table: "TonKhos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoaDonIdHoaDon",
                table: "ThanhToans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThuongHieuIdThuongHieu",
                table: "SanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChucVuIdChucVu",
                table: "NhanViens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CameraSauIdCameraSau",
                table: "ModelSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CameraTruocIdCamTruoc",
                table: "ModelSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManHinhIdManHinh",
                table: "ModelSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PinIdPin",
                table: "ModelSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RAMIdRAM",
                table: "ModelSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ROMIdROM",
                table: "ModelSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SanPhamIdSanPham",
                table: "ModelSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TrangThaiKhachHang",
                table: "KhachHangs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ModelSanPhamIdModelSanPham",
                table: "Imeis",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhachHangIdKhachHang",
                table: "HoaDons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhuyenMaiIdKhuyenMai",
                table: "HoaDons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NhanVienIdNhanVien",
                table: "HoaDons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoaDonIdHoaDon",
                table: "HoaDonChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImeiIdImei",
                table: "HoaDonChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelSanPhamIdModelSanPham",
                table: "HoaDonChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhachHangIdKhachHang",
                table: "GioHangs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTaoGio",
                table: "GioHangs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrangThaiGio",
                table: "GioHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DonGia",
                table: "GioHangChiTiets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GioHangIdGioHang",
                table: "GioHangChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelSanPhamIdModelSanPham",
                table: "GioHangChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ThanhTien",
                table: "GioHangChiTiets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhachHangIdKhachHang",
                table: "DonHangs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DonHangIdDonHang",
                table: "DonHangChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhuyenMaiIdKhuyenMai",
                table: "DonHangChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelSanPhamIdModelSanPham",
                table: "DonHangChiTiets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImeiIdImei",
                table: "BaoHanhs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhachHangIdKhachHang",
                table: "BaoHanhs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NhanVienIdNhanVien",
                table: "BaoHanhs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelSanPhamIdModelSanPham",
                table: "AnhSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TonKhos_KhoIdKho",
                table: "TonKhos",
                column: "KhoIdKho");

            migrationBuilder.CreateIndex(
                name: "IX_TonKhos_ModelSanPhamIdModelSanPham",
                table: "TonKhos",
                column: "ModelSanPhamIdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_HoaDonIdHoaDon",
                table: "ThanhToans",
                column: "HoaDonIdHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_ThuongHieuIdThuongHieu",
                table: "SanPhams",
                column: "ThuongHieuIdThuongHieu");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_ChucVuIdChucVu",
                table: "NhanViens",
                column: "ChucVuIdChucVu");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_CameraSauIdCameraSau",
                table: "ModelSanPhams",
                column: "CameraSauIdCameraSau");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_CameraTruocIdCamTruoc",
                table: "ModelSanPhams",
                column: "CameraTruocIdCamTruoc");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_ManHinhIdManHinh",
                table: "ModelSanPhams",
                column: "ManHinhIdManHinh");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_PinIdPin",
                table: "ModelSanPhams",
                column: "PinIdPin");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_RAMIdRAM",
                table: "ModelSanPhams",
                column: "RAMIdRAM");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_ROMIdROM",
                table: "ModelSanPhams",
                column: "ROMIdROM");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhams_SanPhamIdSanPham",
                table: "ModelSanPhams",
                column: "SanPhamIdSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_Imeis_ModelSanPhamIdModelSanPham",
                table: "Imeis",
                column: "ModelSanPhamIdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_KhachHangIdKhachHang",
                table: "HoaDons",
                column: "KhachHangIdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_KhuyenMaiIdKhuyenMai",
                table: "HoaDons",
                column: "KhuyenMaiIdKhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_NhanVienIdNhanVien",
                table: "HoaDons",
                column: "NhanVienIdNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_HoaDonIdHoaDon",
                table: "HoaDonChiTiets",
                column: "HoaDonIdHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_ImeiIdImei",
                table: "HoaDonChiTiets",
                column: "ImeiIdImei");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_ModelSanPhamIdModelSanPham",
                table: "HoaDonChiTiets",
                column: "ModelSanPhamIdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_KhachHangIdKhachHang",
                table: "GioHangs",
                column: "KhachHangIdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_GioHangIdGioHang",
                table: "GioHangChiTiets",
                column: "GioHangIdGioHang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_ModelSanPhamIdModelSanPham",
                table: "GioHangChiTiets",
                column: "ModelSanPhamIdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_KhachHangIdKhachHang",
                table: "DonHangs",
                column: "KhachHangIdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangChiTiets_DonHangIdDonHang",
                table: "DonHangChiTiets",
                column: "DonHangIdDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangChiTiets_KhuyenMaiIdKhuyenMai",
                table: "DonHangChiTiets",
                column: "KhuyenMaiIdKhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangChiTiets_ModelSanPhamIdModelSanPham",
                table: "DonHangChiTiets",
                column: "ModelSanPhamIdModelSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_BaoHanhs_ImeiIdImei",
                table: "BaoHanhs",
                column: "ImeiIdImei");

            migrationBuilder.CreateIndex(
                name: "IX_BaoHanhs_KhachHangIdKhachHang",
                table: "BaoHanhs",
                column: "KhachHangIdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_BaoHanhs_NhanVienIdNhanVien",
                table: "BaoHanhs",
                column: "NhanVienIdNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_AnhSanPhams_ModelSanPhamIdModelSanPham",
                table: "AnhSanPhams",
                column: "ModelSanPhamIdModelSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_AnhSanPhams_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "AnhSanPhams",
                column: "ModelSanPhamIdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoHanhs_Imeis_ImeiIdImei",
                table: "BaoHanhs",
                column: "ImeiIdImei",
                principalTable: "Imeis",
                principalColumn: "IdImei");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoHanhs_KhachHangs_KhachHangIdKhachHang",
                table: "BaoHanhs",
                column: "KhachHangIdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoHanhs_NhanViens_NhanVienIdNhanVien",
                table: "BaoHanhs",
                column: "NhanVienIdNhanVien",
                principalTable: "NhanViens",
                principalColumn: "IdNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangChiTiets_DonHangs_DonHangIdDonHang",
                table: "DonHangChiTiets",
                column: "DonHangIdDonHang",
                principalTable: "DonHangs",
                principalColumn: "IdDonHang");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangChiTiets_KhuyenMais_KhuyenMaiIdKhuyenMai",
                table: "DonHangChiTiets",
                column: "KhuyenMaiIdKhuyenMai",
                principalTable: "KhuyenMais",
                principalColumn: "IdKhuyenMai");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "DonHangChiTiets",
                column: "ModelSanPhamIdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_KhachHangs_KhachHangIdKhachHang",
                table: "DonHangs",
                column: "KhachHangIdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangChiTiets_GioHangs_GioHangIdGioHang",
                table: "GioHangChiTiets",
                column: "GioHangIdGioHang",
                principalTable: "GioHangs",
                principalColumn: "IdGioHang");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "GioHangChiTiets",
                column: "ModelSanPhamIdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_KhachHangs_KhachHangIdKhachHang",
                table: "GioHangs",
                column: "KhachHangIdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_HoaDons_HoaDonIdHoaDon",
                table: "HoaDonChiTiets",
                column: "HoaDonIdHoaDon",
                principalTable: "HoaDons",
                principalColumn: "IdHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_Imeis_ImeiIdImei",
                table: "HoaDonChiTiets",
                column: "ImeiIdImei",
                principalTable: "Imeis",
                principalColumn: "IdImei");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "HoaDonChiTiets",
                column: "ModelSanPhamIdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_KhachHangs_KhachHangIdKhachHang",
                table: "HoaDons",
                column: "KhachHangIdKhachHang",
                principalTable: "KhachHangs",
                principalColumn: "IdKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_KhuyenMais_KhuyenMaiIdKhuyenMai",
                table: "HoaDons",
                column: "KhuyenMaiIdKhuyenMai",
                principalTable: "KhuyenMais",
                principalColumn: "IdKhuyenMai");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_NhanViens_NhanVienIdNhanVien",
                table: "HoaDons",
                column: "NhanVienIdNhanVien",
                principalTable: "NhanViens",
                principalColumn: "IdNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_Imeis_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "Imeis",
                column: "ModelSanPhamIdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_CameraSaus_CameraSauIdCameraSau",
                table: "ModelSanPhams",
                column: "CameraSauIdCameraSau",
                principalTable: "CameraSaus",
                principalColumn: "IdCameraSau");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_CameraTruocs_CameraTruocIdCamTruoc",
                table: "ModelSanPhams",
                column: "CameraTruocIdCamTruoc",
                principalTable: "CameraTruocs",
                principalColumn: "IdCamTruoc");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_ManHinhs_ManHinhIdManHinh",
                table: "ModelSanPhams",
                column: "ManHinhIdManHinh",
                principalTable: "ManHinhs",
                principalColumn: "IdManHinh");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_Pins_PinIdPin",
                table: "ModelSanPhams",
                column: "PinIdPin",
                principalTable: "Pins",
                principalColumn: "IdPin");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_RAMs_RAMIdRAM",
                table: "ModelSanPhams",
                column: "RAMIdRAM",
                principalTable: "RAMs",
                principalColumn: "IdRAM");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_ROMs_ROMIdROM",
                table: "ModelSanPhams",
                column: "ROMIdROM",
                principalTable: "ROMs",
                principalColumn: "IdROM");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelSanPhams_SanPhams_SanPhamIdSanPham",
                table: "ModelSanPhams",
                column: "SanPhamIdSanPham",
                principalTable: "SanPhams",
                principalColumn: "IdSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_NhanViens_ChucVus_ChucVuIdChucVu",
                table: "NhanViens",
                column: "ChucVuIdChucVu",
                principalTable: "ChucVus",
                principalColumn: "IdChucVu");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_ThuongHieus_ThuongHieuIdThuongHieu",
                table: "SanPhams",
                column: "ThuongHieuIdThuongHieu",
                principalTable: "ThuongHieus",
                principalColumn: "IdThuongHieu");

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_HoaDons_HoaDonIdHoaDon",
                table: "ThanhToans",
                column: "HoaDonIdHoaDon",
                principalTable: "HoaDons",
                principalColumn: "IdHoaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_TonKhos_Khos_KhoIdKho",
                table: "TonKhos",
                column: "KhoIdKho",
                principalTable: "Khos",
                principalColumn: "IdKho");

            migrationBuilder.AddForeignKey(
                name: "FK_TonKhos_ModelSanPhams_ModelSanPhamIdModelSanPham",
                table: "TonKhos",
                column: "ModelSanPhamIdModelSanPham",
                principalTable: "ModelSanPhams",
                principalColumn: "IdModelSanPham");
        }
    }
}
