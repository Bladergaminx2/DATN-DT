using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class PolytechPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CameraSaus",
                columns: table => new
                {
                    IdCameraSau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoPhanGiaiCamSau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoLuongOngKinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhNangCamSau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuayVideoCamSau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaCamSau = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraSaus", x => x.IdCameraSau);
                });

            migrationBuilder.CreateTable(
                name: "CameraTruocs",
                columns: table => new
                {
                    IdCamTruoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoPhanGiaiCamTruoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhNangCamTruoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuayVideoCamTruoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaCamTruoc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraTruocs", x => x.IdCamTruoc);
                });

            migrationBuilder.CreateTable(
                name: "ChucVus",
                columns: table => new
                {
                    IdChucVu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChucVu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChucVus", x => x.IdChucVu);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    IdKhachHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTenKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SdtKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChiKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiemTichLuy = table.Column<int>(type: "int", nullable: true),
                    TrangThaiKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.IdKhachHang);
                });

            migrationBuilder.CreateTable(
                name: "Khos",
                columns: table => new
                {
                    IdKho = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChiKho = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khos", x => x.IdKho);
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMais",
                columns: table => new
                {
                    IdKhuyenMai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaKhuyenMai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiGiam = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApDungVoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThaiKM = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMais", x => x.IdKhuyenMai);
                });

            migrationBuilder.CreateTable(
                name: "ManHinhs",
                columns: table => new
                {
                    IdManHinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CongNgheManHinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KichThuoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoPhanGiai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhNangMan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaMan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManHinhs", x => x.IdManHinh);
                });

            migrationBuilder.CreateTable(
                name: "Pins",
                columns: table => new
                {
                    IdPin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiPin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DungLuongPin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CongNgheSac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaPin = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pins", x => x.IdPin);
                });

            migrationBuilder.CreateTable(
                name: "RAMs",
                columns: table => new
                {
                    IdRAM = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DungLuongRAM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaRAM = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RAMs", x => x.IdRAM);
                });

            migrationBuilder.CreateTable(
                name: "ROMs",
                columns: table => new
                {
                    IdROM = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DungLuongROM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaROM = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROMs", x => x.IdROM);
                });

            migrationBuilder.CreateTable(
                name: "ThuongHieus",
                columns: table => new
                {
                    IdThuongHieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThuongHieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiThuongHieu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuongHieus", x => x.IdThuongHieu);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    IdNhanVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTaiKhoanNV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoTenNhanVien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdChucVu = table.Column<int>(type: "int", nullable: true),
                    SdtNhanVien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailNhanVien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChiNV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayVaoLam = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThaiNV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChucVuIdChucVu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.IdNhanVien);
                    table.ForeignKey(
                        name: "FK_NhanViens_ChucVus_ChucVuIdChucVu",
                        column: x => x.ChucVuIdChucVu,
                        principalTable: "ChucVus",
                        principalColumn: "IdChucVu");
                });

            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    IdDonHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdKhachHang = table.Column<int>(type: "int", nullable: true),
                    MaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiaChiGiaoHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiDH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhachHangIdKhachHang = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangs", x => x.IdDonHang);
                    table.ForeignKey(
                        name: "FK_DonHangs_KhachHangs_KhachHangIdKhachHang",
                        column: x => x.KhachHangIdKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "IdKhachHang");
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    IdGioHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdKhachHang = table.Column<int>(type: "int", nullable: true),
                    NgayTaoGio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThaiGio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhachHangIdKhachHang = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.IdGioHang);
                    table.ForeignKey(
                        name: "FK_GioHangs_KhachHangs_KhachHangIdKhachHang",
                        column: x => x.KhachHangIdKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "IdKhachHang");
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    IdSanPham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSanPham = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenSanPham = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdThuongHieu = table.Column<int>(type: "int", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaGoc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiaNiemYet = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrangThaiSP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VAT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThuongHieuIdThuongHieu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.IdSanPham);
                    table.ForeignKey(
                        name: "FK_SanPhams_ThuongHieus_ThuongHieuIdThuongHieu",
                        column: x => x.ThuongHieuIdThuongHieu,
                        principalTable: "ThuongHieus",
                        principalColumn: "IdThuongHieu");
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    IdHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdKhachHang = table.Column<int>(type: "int", nullable: true),
                    IdNhanVien = table.Column<int>(type: "int", nullable: true),
                    IdKhuyenMai = table.Column<int>(type: "int", nullable: true),
                    TrangThaiHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NgayLapHoaDon = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KhachHangIdKhachHang = table.Column<int>(type: "int", nullable: true),
                    NhanVienIdNhanVien = table.Column<int>(type: "int", nullable: true),
                    KhuyenMaiIdKhuyenMai = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.IdHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDons_KhachHangs_KhachHangIdKhachHang",
                        column: x => x.KhachHangIdKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "IdKhachHang");
                    table.ForeignKey(
                        name: "FK_HoaDons_KhuyenMais_KhuyenMaiIdKhuyenMai",
                        column: x => x.KhuyenMaiIdKhuyenMai,
                        principalTable: "KhuyenMais",
                        principalColumn: "IdKhuyenMai");
                    table.ForeignKey(
                        name: "FK_HoaDons_NhanViens_NhanVienIdNhanVien",
                        column: x => x.NhanVienIdNhanVien,
                        principalTable: "NhanViens",
                        principalColumn: "IdNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "ModelSanPhams",
                columns: table => new
                {
                    IdModelSanPham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdSanPham = table.Column<int>(type: "int", nullable: true),
                    IdManHinh = table.Column<int>(type: "int", nullable: true),
                    IdCameraTruoc = table.Column<int>(type: "int", nullable: true),
                    IdCameraSau = table.Column<int>(type: "int", nullable: true),
                    IdPin = table.Column<int>(type: "int", nullable: true),
                    IdRAM = table.Column<int>(type: "int", nullable: true),
                    IdROM = table.Column<int>(type: "int", nullable: true),
                    Mau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaBanModel = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SanPhamIdSanPham = table.Column<int>(type: "int", nullable: true),
                    ManHinhIdManHinh = table.Column<int>(type: "int", nullable: true),
                    CameraTruocIdCamTruoc = table.Column<int>(type: "int", nullable: true),
                    CameraSauIdCameraSau = table.Column<int>(type: "int", nullable: true),
                    PinIdPin = table.Column<int>(type: "int", nullable: true),
                    RAMIdRAM = table.Column<int>(type: "int", nullable: true),
                    ROMIdROM = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelSanPhams", x => x.IdModelSanPham);
                    table.ForeignKey(
                        name: "FK_ModelSanPhams_CameraSaus_CameraSauIdCameraSau",
                        column: x => x.CameraSauIdCameraSau,
                        principalTable: "CameraSaus",
                        principalColumn: "IdCameraSau");
                    table.ForeignKey(
                        name: "FK_ModelSanPhams_CameraTruocs_CameraTruocIdCamTruoc",
                        column: x => x.CameraTruocIdCamTruoc,
                        principalTable: "CameraTruocs",
                        principalColumn: "IdCamTruoc");
                    table.ForeignKey(
                        name: "FK_ModelSanPhams_ManHinhs_ManHinhIdManHinh",
                        column: x => x.ManHinhIdManHinh,
                        principalTable: "ManHinhs",
                        principalColumn: "IdManHinh");
                    table.ForeignKey(
                        name: "FK_ModelSanPhams_Pins_PinIdPin",
                        column: x => x.PinIdPin,
                        principalTable: "Pins",
                        principalColumn: "IdPin");
                    table.ForeignKey(
                        name: "FK_ModelSanPhams_RAMs_RAMIdRAM",
                        column: x => x.RAMIdRAM,
                        principalTable: "RAMs",
                        principalColumn: "IdRAM");
                    table.ForeignKey(
                        name: "FK_ModelSanPhams_ROMs_ROMIdROM",
                        column: x => x.ROMIdROM,
                        principalTable: "ROMs",
                        principalColumn: "IdROM");
                    table.ForeignKey(
                        name: "FK_ModelSanPhams_SanPhams_SanPhamIdSanPham",
                        column: x => x.SanPhamIdSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "IdSanPham");
                });

            migrationBuilder.CreateTable(
                name: "ThanhToans",
                columns: table => new
                {
                    IdThanhToan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdHoaDon = table.Column<int>(type: "int", nullable: true),
                    HinhThuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoaDonIdHoaDon = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToans", x => x.IdThanhToan);
                    table.ForeignKey(
                        name: "FK_ThanhToans_HoaDons_HoaDonIdHoaDon",
                        column: x => x.HoaDonIdHoaDon,
                        principalTable: "HoaDons",
                        principalColumn: "IdHoaDon");
                });

            migrationBuilder.CreateTable(
                name: "AnhSanPhams",
                columns: table => new
                {
                    IdAnh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    DuongDan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelSanPhamIdModelSanPham = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhSanPhams", x => x.IdAnh);
                    table.ForeignKey(
                        name: "FK_AnhSanPhams_ModelSanPhams_ModelSanPhamIdModelSanPham",
                        column: x => x.ModelSanPhamIdModelSanPham,
                        principalTable: "ModelSanPhams",
                        principalColumn: "IdModelSanPham");
                });

            migrationBuilder.CreateTable(
                name: "DonHangChiTiets",
                columns: table => new
                {
                    IdDonHangChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDonHang = table.Column<int>(type: "int", nullable: true),
                    IdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    IdKhuyenMai = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DonHangIdDonHang = table.Column<int>(type: "int", nullable: true),
                    ModelSanPhamIdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    KhuyenMaiIdKhuyenMai = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangChiTiets", x => x.IdDonHangChiTiet);
                    table.ForeignKey(
                        name: "FK_DonHangChiTiets_DonHangs_DonHangIdDonHang",
                        column: x => x.DonHangIdDonHang,
                        principalTable: "DonHangs",
                        principalColumn: "IdDonHang");
                    table.ForeignKey(
                        name: "FK_DonHangChiTiets_KhuyenMais_KhuyenMaiIdKhuyenMai",
                        column: x => x.KhuyenMaiIdKhuyenMai,
                        principalTable: "KhuyenMais",
                        principalColumn: "IdKhuyenMai");
                    table.ForeignKey(
                        name: "FK_DonHangChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                        column: x => x.ModelSanPhamIdModelSanPham,
                        principalTable: "ModelSanPhams",
                        principalColumn: "IdModelSanPham");
                });

            migrationBuilder.CreateTable(
                name: "GioHangChiTiets",
                columns: table => new
                {
                    IdGioHangChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdGioHang = table.Column<int>(type: "int", nullable: true),
                    IdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GioHangIdGioHang = table.Column<int>(type: "int", nullable: true),
                    ModelSanPhamIdModelSanPham = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangChiTiets", x => x.IdGioHangChiTiet);
                    table.ForeignKey(
                        name: "FK_GioHangChiTiets_GioHangs_GioHangIdGioHang",
                        column: x => x.GioHangIdGioHang,
                        principalTable: "GioHangs",
                        principalColumn: "IdGioHang");
                    table.ForeignKey(
                        name: "FK_GioHangChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                        column: x => x.ModelSanPhamIdModelSanPham,
                        principalTable: "ModelSanPhams",
                        principalColumn: "IdModelSanPham");
                });

            migrationBuilder.CreateTable(
                name: "Imeis",
                columns: table => new
                {
                    IdImei = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaImei = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelSanPhamIdModelSanPham = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imeis", x => x.IdImei);
                    table.ForeignKey(
                        name: "FK_Imeis_ModelSanPhams_ModelSanPhamIdModelSanPham",
                        column: x => x.ModelSanPhamIdModelSanPham,
                        principalTable: "ModelSanPhams",
                        principalColumn: "IdModelSanPham");
                });

            migrationBuilder.CreateTable(
                name: "TonKhos",
                columns: table => new
                {
                    IdTonKho = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    IdKho = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    ModelSanPhamIdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    KhoIdKho = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TonKhos", x => x.IdTonKho);
                    table.ForeignKey(
                        name: "FK_TonKhos_Khos_KhoIdKho",
                        column: x => x.KhoIdKho,
                        principalTable: "Khos",
                        principalColumn: "IdKho");
                    table.ForeignKey(
                        name: "FK_TonKhos_ModelSanPhams_ModelSanPhamIdModelSanPham",
                        column: x => x.ModelSanPhamIdModelSanPham,
                        principalTable: "ModelSanPhams",
                        principalColumn: "IdModelSanPham");
                });

            migrationBuilder.CreateTable(
                name: "BaoHanhs",
                columns: table => new
                {
                    IdBaoHanh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdImei = table.Column<int>(type: "int", nullable: true),
                    IdKhachHang = table.Column<int>(type: "int", nullable: true),
                    IdNhanVien = table.Column<int>(type: "int", nullable: true),
                    NgayNhan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayTra = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XuLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChiPhiPhatSinh = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ImeiIdImei = table.Column<int>(type: "int", nullable: true),
                    KhachHangIdKhachHang = table.Column<int>(type: "int", nullable: true),
                    NhanVienIdNhanVien = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoHanhs", x => x.IdBaoHanh);
                    table.ForeignKey(
                        name: "FK_BaoHanhs_Imeis_ImeiIdImei",
                        column: x => x.ImeiIdImei,
                        principalTable: "Imeis",
                        principalColumn: "IdImei");
                    table.ForeignKey(
                        name: "FK_BaoHanhs_KhachHangs_KhachHangIdKhachHang",
                        column: x => x.KhachHangIdKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "IdKhachHang");
                    table.ForeignKey(
                        name: "FK_BaoHanhs_NhanViens_NhanVienIdNhanVien",
                        column: x => x.NhanVienIdNhanVien,
                        principalTable: "NhanViens",
                        principalColumn: "IdNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "HoaDonChiTiets",
                columns: table => new
                {
                    IdHoaDonChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdHoaDon = table.Column<int>(type: "int", nullable: true),
                    IdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    IdImei = table.Column<int>(type: "int", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HoaDonIdHoaDon = table.Column<int>(type: "int", nullable: true),
                    ModelSanPhamIdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    ImeiIdImei = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonChiTiets", x => x.IdHoaDonChiTiet);
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_HoaDons_HoaDonIdHoaDon",
                        column: x => x.HoaDonIdHoaDon,
                        principalTable: "HoaDons",
                        principalColumn: "IdHoaDon");
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_Imeis_ImeiIdImei",
                        column: x => x.ImeiIdImei,
                        principalTable: "Imeis",
                        principalColumn: "IdImei");
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_ModelSanPhams_ModelSanPhamIdModelSanPham",
                        column: x => x.ModelSanPhamIdModelSanPham,
                        principalTable: "ModelSanPhams",
                        principalColumn: "IdModelSanPham");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnhSanPhams_ModelSanPhamIdModelSanPham",
                table: "AnhSanPhams",
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
                name: "IX_DonHangs_KhachHangIdKhachHang",
                table: "DonHangs",
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
                name: "IX_GioHangs_KhachHangIdKhachHang",
                table: "GioHangs",
                column: "KhachHangIdKhachHang");

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
                name: "IX_Imeis_ModelSanPhamIdModelSanPham",
                table: "Imeis",
                column: "ModelSanPhamIdModelSanPham");

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
                name: "IX_NhanViens_ChucVuIdChucVu",
                table: "NhanViens",
                column: "ChucVuIdChucVu");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_ThuongHieuIdThuongHieu",
                table: "SanPhams",
                column: "ThuongHieuIdThuongHieu");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_HoaDonIdHoaDon",
                table: "ThanhToans",
                column: "HoaDonIdHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_TonKhos_KhoIdKho",
                table: "TonKhos",
                column: "KhoIdKho");

            migrationBuilder.CreateIndex(
                name: "IX_TonKhos_ModelSanPhamIdModelSanPham",
                table: "TonKhos",
                column: "ModelSanPhamIdModelSanPham");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnhSanPhams");

            migrationBuilder.DropTable(
                name: "BaoHanhs");

            migrationBuilder.DropTable(
                name: "DonHangChiTiets");

            migrationBuilder.DropTable(
                name: "GioHangChiTiets");

            migrationBuilder.DropTable(
                name: "HoaDonChiTiets");

            migrationBuilder.DropTable(
                name: "ThanhToans");

            migrationBuilder.DropTable(
                name: "TonKhos");

            migrationBuilder.DropTable(
                name: "DonHangs");

            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "Imeis");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "Khos");

            migrationBuilder.DropTable(
                name: "ModelSanPhams");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "KhuyenMais");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "CameraSaus");

            migrationBuilder.DropTable(
                name: "CameraTruocs");

            migrationBuilder.DropTable(
                name: "ManHinhs");

            migrationBuilder.DropTable(
                name: "Pins");

            migrationBuilder.DropTable(
                name: "RAMs");

            migrationBuilder.DropTable(
                name: "ROMs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "ChucVus");

            migrationBuilder.DropTable(
                name: "ThuongHieus");
        }
    }
}
