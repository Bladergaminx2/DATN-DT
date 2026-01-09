
using System;
using DATN_DT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DATN_DT.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20260109225342_baohanh")]
    partial class baohanh
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DATN_DT.Models.AnhSanPham", b =>
                {
                    b.Property<int>("IdAnh")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdAnh"));

                    b.Property<string>("DuongDan")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("IdModelSanPham")
                        .HasColumnType("int");

                    b.HasKey("IdAnh");

                    b.HasIndex("IdModelSanPham");

                    b.ToTable("AnhSanPhams");
                });

            modelBuilder.Entity("DATN_DT.Models.BaoHanh", b =>
                {
                    b.Property<int>("IdBaoHanh")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdBaoHanh"));

                    b.Property<decimal?>("ChiPhiPhatSinh")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("IdImei")
                        .HasColumnType("int");

                    b.Property<int?>("IdKhachHang")
                        .HasColumnType("int");

                    b.Property<int?>("IdNhanVien")
                        .HasColumnType("int");

                    b.Property<string>("MoTaLoi")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("NgayNhan")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("NgayTra")
                        .HasColumnType("datetime2");

                    b.Property<string>("TrangThai")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("XuLy")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdBaoHanh");

                    b.HasIndex("IdImei");

                    b.HasIndex("IdKhachHang");

                    b.HasIndex("IdNhanVien");

                    b.ToTable("BaoHanhs");
                });

            modelBuilder.Entity("DATN_DT.Models.CameraSau", b =>
                {
                    b.Property<int>("IdCameraSau")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdCameraSau"));

                    b.Property<string>("DoPhanGiaiCamSau")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTaCamSau")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("QuayVideoCamSau")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SoLuongOngKinh")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TinhNangCamSau")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdCameraSau");

                    b.ToTable("CameraSaus");
                });

            modelBuilder.Entity("DATN_DT.Models.CameraTruoc", b =>
                {
                    b.Property<int>("IdCamTruoc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdCamTruoc"));

                    b.Property<string>("DoPhanGiaiCamTruoc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTaCamTruoc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("QuayVideoCamTruoc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TinhNangCamTruoc")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdCamTruoc");

                    b.ToTable("CameraTruocs");
                });

            modelBuilder.Entity("DATN_DT.Models.ChucVu", b =>
                {
                    b.Property<int>("IdChucVu")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdChucVu"));

                    b.Property<string>("TenChucVu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenChucVuVietHoa")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdChucVu");

                    b.ToTable("ChucVus");
                });

            modelBuilder.Entity("DATN_DT.Models.DiaChi", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Diachicuthe")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IdKhachHang")
                        .HasColumnType("int");

                    b.Property<string>("Phuongxa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Quanhuyen")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tennguoinhan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Thanhpho")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("sdtnguoinhan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("trangthai")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IdKhachHang");

                    b.ToTable("diachis");
                });

            modelBuilder.Entity("DATN_DT.Models.DonHang", b =>
                {
                    b.Property<int>("IdDonHang")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdDonHang"));

                    b.Property<string>("DiaChiGiaoHang")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GhiChu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTenNguoiNhan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("IdKhachHang")
                        .HasColumnType("int");

                    b.Property<int?>("IdNhanVien")
                        .HasColumnType("int");

                    b.Property<string>("MaDon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("NgayDat")
                        .HasColumnType("datetime2");

                    b.Property<string>("PhuongThucThanhToan")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SdtNguoiNhan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TrangThaiDH")
                        .HasColumnType("int");

                    b.Property<string>("TrangThaiHoaDon")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdDonHang");

                    b.HasIndex("IdKhachHang");

                    b.HasIndex("IdNhanVien");

                    b.ToTable("DonHangs");
                });

            modelBuilder.Entity("DATN_DT.Models.DonHangChiTiet", b =>
                {
                    b.Property<int>("IdDonHangChiTiet")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdDonHangChiTiet"));

                    b.Property<decimal?>("DonGia")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("GiaKhuyenMai")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("IdDonHang")
                        .HasColumnType("int");

                    b.Property<int?>("IdModelSanPham")
                        .HasColumnType("int");

                    b.Property<int?>("SoLuong")
                        .HasColumnType("int");

                    b.Property<decimal?>("ThanhTien")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("IdDonHangChiTiet");

                    b.HasIndex("IdDonHang");

                    b.HasIndex("IdModelSanPham");

                    b.ToTable("DonHangChiTiets");
                });

            modelBuilder.Entity("DATN_DT.Models.GioHang", b =>
                {
                    b.Property<int>("IdGioHang")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdGioHang"));

                    b.Property<int?>("IdKhachHang")
                        .HasColumnType("int");

                    b.HasKey("IdGioHang");

                    b.HasIndex("IdKhachHang");

                    b.ToTable("GioHangs");
                });

            modelBuilder.Entity("DATN_DT.Models.GioHangChiTiet", b =>
                {
                    b.Property<int>("IdGioHangChiTiet")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdGioHangChiTiet"));

                    b.Property<int?>("IdGioHang")
                        .HasColumnType("int");

                    b.Property<int?>("IdModelSanPham")
                        .HasColumnType("int");

                    b.Property<int>("SoLuong")
                        .HasColumnType("int");

                    b.HasKey("IdGioHangChiTiet");

                    b.HasIndex("IdGioHang");

                    b.HasIndex("IdModelSanPham");

                    b.ToTable("GioHangChiTiets");
                });

            modelBuilder.Entity("DATN_DT.Models.HoaDon", b =>
                {
                    b.Property<int>("IdHoaDon")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdHoaDon"));

                    b.Property<string>("HoTenNguoiNhan")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("IdKhachHang")
                        .HasColumnType("int");

                    b.Property<int?>("IdNhanVien")
                        .HasColumnType("int");

                    b.Property<DateTime?>("NgayLapHoaDon")
                        .HasColumnType("datetime2");

                    b.Property<string>("PhuongThucThanhToan")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SdtKhachHang")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("TongTien")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("TrangThaiHoaDon")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdHoaDon");

                    b.HasIndex("IdKhachHang");

                    b.HasIndex("IdNhanVien");

                    b.ToTable("HoaDon", (string)null);
                });

            modelBuilder.Entity("DATN_DT.Models.HoaDonChiTiet", b =>
                {
                    b.Property<int>("IdHoaDonChiTiet")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdHoaDonChiTiet"));

                    b.Property<decimal?>("DonGia")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("GiaKhuyenMai")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("IdHoaDon")
                        .HasColumnType("int");

                    b.Property<int?>("IdImei")
                        .HasColumnType("int");

                    b.Property<int?>("IdModelSanPham")
                        .HasColumnType("int");

                    b.Property<int?>("SoLuong")
                        .HasColumnType("int");

                    b.Property<decimal?>("ThanhTien")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("IdHoaDonChiTiet");

                    b.HasIndex("IdHoaDon");

                    b.HasIndex("IdImei");

                    b.HasIndex("IdModelSanPham");

                    b.ToTable("HoaDonChiTiets");
                });

            modelBuilder.Entity("DATN_DT.Models.Imei", b =>
                {
                    b.Property<int>("IdImei")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdImei"));

                    b.Property<int?>("IdModelSanPham")
                        .HasColumnType("int");

                    b.Property<string>("MaImei")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTa")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThai")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdImei");

                    b.HasIndex("IdModelSanPham");

                    b.ToTable("Imeis");
                });

            modelBuilder.Entity("DATN_DT.Models.KhachHang", b =>
                {
                    b.Property<int>("IdKhachHang")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdKhachHang"));

                    b.Property<string>("DefaultImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DiemTichLuy")
                        .HasColumnType("int");

                    b.Property<string>("EmailKhachHang")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTenKhachHang")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SdtKhachHang")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TrangThaiKhachHang")
                        .HasColumnType("int");

                    b.HasKey("IdKhachHang");

                    b.ToTable("KhachHangs");
                });

            modelBuilder.Entity("DATN_DT.Models.Kho", b =>
                {
                    b.Property<int>("IdKho")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdKho"));

                    b.Property<string>("DiaChiKho")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenKho")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdKho");

                    b.ToTable("Khos");
                });

            modelBuilder.Entity("DATN_DT.Models.KhuyenMai", b =>
                {
                    b.Property<int>("IdKhuyenMai")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdKhuyenMai"));

                    b.Property<string>("ApDungVoi")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("GiaTri")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("LoaiGiam")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MaKM")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTaKhuyenMai")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("NgayBatDau")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("NgayKetThuc")
                        .HasColumnType("datetime2");

                    b.Property<string>("TrangThaiKM")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdKhuyenMai");

                    b.ToTable("KhuyenMais");
                });

            modelBuilder.Entity("DATN_DT.Models.ManHinh", b =>
                {
                    b.Property<int>("IdManHinh")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdManHinh"));

                    b.Property<string>("CongNgheManHinh")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DoPhanGiai")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KichThuoc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTaMan")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TinhNangMan")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdManHinh");

                    b.ToTable("ManHinhs");
                });

            modelBuilder.Entity("DATN_DT.Models.ModelSanPham", b =>
                {
                    b.Property<int>("IdModelSanPham")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdModelSanPham"));

                    b.Property<decimal?>("GiaBanModel")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("IdCameraSau")
                        .HasColumnType("int");

                    b.Property<int?>("IdCameraTruoc")
                        .HasColumnType("int");

                    b.Property<int?>("IdManHinh")
                        .HasColumnType("int");

                    b.Property<int?>("IdPin")
                        .HasColumnType("int");

                    b.Property<int?>("IdRAM")
                        .HasColumnType("int");

                    b.Property<int?>("IdROM")
                        .HasColumnType("int");

                    b.Property<int?>("IdSanPham")
                        .HasColumnType("int");

                    b.Property<string>("Mau")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenModel")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TrangThai")
                        .HasColumnType("int");

                    b.HasKey("IdModelSanPham");

                    b.HasIndex("IdCameraSau");

                    b.HasIndex("IdCameraTruoc");

                    b.HasIndex("IdManHinh");

                    b.HasIndex("IdPin");

                    b.HasIndex("IdRAM");

                    b.HasIndex("IdROM");

                    b.HasIndex("IdSanPham");

                    b.ToTable("ModelSanPhams");
                });

            modelBuilder.Entity("DATN_DT.Models.ModelSanPhamKhuyenMai", b =>
                {
                    b.Property<int>("IdModelSanPhamKhuyenMai")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdModelSanPhamKhuyenMai"));

                    b.Property<int?>("IdKhuyenMai")
                        .HasColumnType("int");

                    b.Property<int?>("IdModelSanPham")
                        .HasColumnType("int");

                    b.Property<DateTime?>("NgayTao")
                        .HasColumnType("datetime2");

                    b.HasKey("IdModelSanPhamKhuyenMai");

                    b.HasIndex("IdKhuyenMai");

                    b.HasIndex("IdModelSanPham");

                    b.ToTable("ModelSanPhamKhuyenMais");
                });

            modelBuilder.Entity("DATN_DT.Models.NhanVien", b =>
                {
                    b.Property<int>("IdNhanVien")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdNhanVien"));

                    b.Property<string>("DiaChiNV")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmailNhanVien")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTenNhanVien")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("IdChucVu")
                        .HasColumnType("int");

                    b.Property<DateTime?>("NgayVaoLam")
                        .HasColumnType("datetime2");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SdtNhanVien")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenTaiKhoanNV")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TrangThaiNV")
                        .HasColumnType("int");

                    b.HasKey("IdNhanVien");

                    b.HasIndex("IdChucVu");

                    b.ToTable("NhanViens");
                });

            modelBuilder.Entity("DATN_DT.Models.Pin", b =>
                {
                    b.Property<int>("IdPin")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdPin"));

                    b.Property<string>("CongNgheSac")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DungLuongPin")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LoaiPin")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTaPin")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdPin");

                    b.ToTable("Pins");
                });

            modelBuilder.Entity("DATN_DT.Models.RAM", b =>
                {
                    b.Property<int>("IdRAM")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdRAM"));

                    b.Property<string>("DungLuongRAM")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTaRAM")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdRAM");

                    b.ToTable("RAMs");
                });

            modelBuilder.Entity("DATN_DT.Models.ROM", b =>
                {
                    b.Property<int>("IdROM")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdROM"));

                    b.Property<string>("DungLuongROM")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTaROM")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdROM");

                    b.ToTable("ROMs");
                });

            modelBuilder.Entity("DATN_DT.Models.SanPham", b =>
                {
                    b.Property<int>("IdSanPham")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdSanPham"));

                    b.Property<decimal?>("GiaGoc")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("GiaNiemYet")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("IdThuongHieu")
                        .HasColumnType("int");

                    b.Property<string>("MaSanPham")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTa")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenSanPham")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThaiSP")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("VAT")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("IdSanPham");

                    b.HasIndex("IdThuongHieu");

                    b.ToTable("SanPhams");
                });

            modelBuilder.Entity("DATN_DT.Models.ThanhToan", b =>
                {
                    b.Property<int>("IdThanhToan")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdThanhToan"));

                    b.Property<string>("HinhThuc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("IdHoaDon")
                        .HasColumnType("int");

                    b.Property<DateTime?>("NgayThanhToan")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("SoTien")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("IdThanhToan");

                    b.HasIndex("IdHoaDon");

                    b.ToTable("ThanhToans");
                });

            modelBuilder.Entity("DATN_DT.Models.ThuongHieu", b =>
                {
                    b.Property<int>("IdThuongHieu")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdThuongHieu"));

                    b.Property<string>("TenThuongHieu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThaiThuongHieu")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdThuongHieu");

                    b.ToTable("ThuongHieus");
                });

            modelBuilder.Entity("DATN_DT.Models.TonKho", b =>
                {
                    b.Property<int>("IdTonKho")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdTonKho"));

                    b.Property<int?>("IdKho")
                        .HasColumnType("int");

                    b.Property<int?>("IdModelSanPham")
                        .HasColumnType("int");

                    b.Property<int>("SoLuong")
                        .HasColumnType("int");

                    b.HasKey("IdTonKho");

                    b.HasIndex("IdKho");

                    b.HasIndex("IdModelSanPham");

                    b.ToTable("TonKhos");
                });

            modelBuilder.Entity("DATN_DT.Models.Voucher", b =>
                {
                    b.Property<int>("IdVoucher")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdVoucher"));

                    b.Property<string>("ApDungCho")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DanhSachId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("DonHangToiThieu")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("GiaTri")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("GiamToiDa")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("LoaiGiam")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MaVoucher")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTa")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayBatDau")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("NgayKetThuc")
                        .HasColumnType("datetime2");

                    b.Property<int>("SoLuongDaSuDung")
                        .HasColumnType("int");

                    b.Property<int?>("SoLuongMoiKhachHang")
                        .HasColumnType("int");

                    b.Property<int?>("SoLuongSuDung")
                        .HasColumnType("int");

                    b.Property<string>("TenVoucher")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdVoucher");

                    b.ToTable("Vouchers");
                });

            modelBuilder.Entity("DATN_DT.Models.VoucherSuDung", b =>
                {
                    b.Property<int>("IdVoucherSuDung")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdVoucherSuDung"));

                    b.Property<int?>("IdHoaDon")
                        .HasColumnType("int");

                    b.Property<int>("IdKhachHang")
                        .HasColumnType("int");

                    b.Property<int>("IdVoucher")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgaySuDung")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("SoTienGiam")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("IdVoucherSuDung");

                    b.HasIndex("IdHoaDon");

                    b.HasIndex("IdKhachHang");

                    b.HasIndex("IdVoucher");

                    b.ToTable("VoucherSuDungs");
                });

            modelBuilder.Entity("DATN_DT.Models.AnhSanPham", b =>
                {
                    b.HasOne("DATN_DT.Models.ModelSanPham", "ModelSanPham")
                        .WithMany("AnhSanPhams")
                        .HasForeignKey("IdModelSanPham")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("ModelSanPham");
                });

            modelBuilder.Entity("DATN_DT.Models.BaoHanh", b =>
                {
                    b.HasOne("DATN_DT.Models.Imei", "Imei")
                        .WithMany()
                        .HasForeignKey("IdImei")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.KhachHang", "KhachHang")
                        .WithMany()
                        .HasForeignKey("IdKhachHang")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.NhanVien", "NhanVien")
                        .WithMany()
                        .HasForeignKey("IdNhanVien")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Imei");

                    b.Navigation("KhachHang");

                    b.Navigation("NhanVien");
                });

            modelBuilder.Entity("DATN_DT.Models.DiaChi", b =>
                {
                    b.HasOne("DATN_DT.Models.KhachHang", "KhachHang")
                        .WithMany("Diachi")
                        .HasForeignKey("IdKhachHang")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("KhachHang");
                });

            modelBuilder.Entity("DATN_DT.Models.DonHang", b =>
                {
                    b.HasOne("DATN_DT.Models.KhachHang", "KhachHang")
                        .WithMany()
                        .HasForeignKey("IdKhachHang")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.NhanVien", "NhanVien")
                        .WithMany()
                        .HasForeignKey("IdNhanVien")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("KhachHang");

                    b.Navigation("NhanVien");
                });

            modelBuilder.Entity("DATN_DT.Models.DonHangChiTiet", b =>
                {
                    b.HasOne("DATN_DT.Models.DonHang", "DonHang")
                        .WithMany("DonHangChiTiets")
                        .HasForeignKey("IdDonHang")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DATN_DT.Models.ModelSanPham", "ModelSanPham")
                        .WithMany()
                        .HasForeignKey("IdModelSanPham")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("DonHang");

                    b.Navigation("ModelSanPham");
                });

            modelBuilder.Entity("DATN_DT.Models.GioHang", b =>
                {
                    b.HasOne("DATN_DT.Models.KhachHang", "KhachHang")
                        .WithMany()
                        .HasForeignKey("IdKhachHang")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("KhachHang");
                });

            modelBuilder.Entity("DATN_DT.Models.GioHangChiTiet", b =>
                {
                    b.HasOne("DATN_DT.Models.GioHang", "GioHang")
                        .WithMany("GioHangChiTiets")
                        .HasForeignKey("IdGioHang")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DATN_DT.Models.ModelSanPham", "ModelSanPham")
                        .WithMany()
                        .HasForeignKey("IdModelSanPham")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("GioHang");

                    b.Navigation("ModelSanPham");
                });

            modelBuilder.Entity("DATN_DT.Models.HoaDon", b =>
                {
                    b.HasOne("DATN_DT.Models.KhachHang", "KhachHang")
                        .WithMany()
                        .HasForeignKey("IdKhachHang")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.NhanVien", "NhanVien")
                        .WithMany()
                        .HasForeignKey("IdNhanVien")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("KhachHang");

                    b.Navigation("NhanVien");
                });

            modelBuilder.Entity("DATN_DT.Models.HoaDonChiTiet", b =>
                {
                    b.HasOne("DATN_DT.Models.HoaDon", "HoaDon")
                        .WithMany("HoaDonChiTiets")
                        .HasForeignKey("IdHoaDon")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DATN_DT.Models.Imei", "Imei")
                        .WithMany()
                        .HasForeignKey("IdImei")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.ModelSanPham", "ModelSanPham")
                        .WithMany()
                        .HasForeignKey("IdModelSanPham")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("HoaDon");

                    b.Navigation("Imei");

                    b.Navigation("ModelSanPham");
                });

            modelBuilder.Entity("DATN_DT.Models.Imei", b =>
                {
                    b.HasOne("DATN_DT.Models.ModelSanPham", "ModelSanPham")
                        .WithMany("Imeis")
                        .HasForeignKey("IdModelSanPham")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ModelSanPham");
                });

            modelBuilder.Entity("DATN_DT.Models.ModelSanPham", b =>
                {
                    b.HasOne("DATN_DT.Models.CameraSau", "CameraSau")
                        .WithMany()
                        .HasForeignKey("IdCameraSau")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.CameraTruoc", "CameraTruoc")
                        .WithMany()
                        .HasForeignKey("IdCameraTruoc")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.ManHinh", "ManHinh")
                        .WithMany()
                        .HasForeignKey("IdManHinh")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.Pin", "Pin")
                        .WithMany()
                        .HasForeignKey("IdPin")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.RAM", "RAM")
                        .WithMany()
                        .HasForeignKey("IdRAM")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.ROM", "ROM")
                        .WithMany()
                        .HasForeignKey("IdROM")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.SanPham", "SanPham")
                        .WithMany("ModelSanPhams")
                        .HasForeignKey("IdSanPham")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("CameraSau");

                    b.Navigation("CameraTruoc");

                    b.Navigation("ManHinh");

                    b.Navigation("Pin");

                    b.Navigation("RAM");

                    b.Navigation("ROM");

                    b.Navigation("SanPham");
                });

            modelBuilder.Entity("DATN_DT.Models.ModelSanPhamKhuyenMai", b =>
                {
                    b.HasOne("DATN_DT.Models.KhuyenMai", "KhuyenMai")
                        .WithMany()
                        .HasForeignKey("IdKhuyenMai")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.ModelSanPham", "ModelSanPham")
                        .WithMany("ModelSanPhamKhuyenMais")
                        .HasForeignKey("IdModelSanPham")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("KhuyenMai");

                    b.Navigation("ModelSanPham");
                });

            modelBuilder.Entity("DATN_DT.Models.NhanVien", b =>
                {
                    b.HasOne("DATN_DT.Models.ChucVu", "ChucVu")
                        .WithMany()
                        .HasForeignKey("IdChucVu")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ChucVu");
                });

            modelBuilder.Entity("DATN_DT.Models.SanPham", b =>
                {
                    b.HasOne("DATN_DT.Models.ThuongHieu", "ThuongHieu")
                        .WithMany()
                        .HasForeignKey("IdThuongHieu")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ThuongHieu");
                });

            modelBuilder.Entity("DATN_DT.Models.ThanhToan", b =>
                {
                    b.HasOne("DATN_DT.Models.HoaDon", "HoaDon")
                        .WithMany()
                        .HasForeignKey("IdHoaDon")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("HoaDon");
                });

            modelBuilder.Entity("DATN_DT.Models.TonKho", b =>
                {
                    b.HasOne("DATN_DT.Models.Kho", "Kho")
                        .WithMany()
                        .HasForeignKey("IdKho")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN_DT.Models.ModelSanPham", "ModelSanPham")
                        .WithMany()
                        .HasForeignKey("IdModelSanPham")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Kho");

                    b.Navigation("ModelSanPham");
                });

            modelBuilder.Entity("DATN_DT.Models.VoucherSuDung", b =>
                {
                    b.HasOne("DATN_DT.Models.HoaDon", "HoaDon")
                        .WithMany()
                        .HasForeignKey("IdHoaDon")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("DATN_DT.Models.KhachHang", "KhachHang")
                        .WithMany()
                        .HasForeignKey("IdKhachHang")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DATN_DT.Models.Voucher", "Voucher")
                        .WithMany("VoucherSuDungs")
                        .HasForeignKey("IdVoucher")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("HoaDon");

                    b.Navigation("KhachHang");

                    b.Navigation("Voucher");
                });

            modelBuilder.Entity("DATN_DT.Models.DonHang", b =>
                {
                    b.Navigation("DonHangChiTiets");
                });

            modelBuilder.Entity("DATN_DT.Models.GioHang", b =>
                {
                    b.Navigation("GioHangChiTiets");
                });

            modelBuilder.Entity("DATN_DT.Models.HoaDon", b =>
                {
                    b.Navigation("HoaDonChiTiets");
                });

            modelBuilder.Entity("DATN_DT.Models.KhachHang", b =>
                {
                    b.Navigation("Diachi");
                });

            modelBuilder.Entity("DATN_DT.Models.ModelSanPham", b =>
                {
                    b.Navigation("AnhSanPhams");

                    b.Navigation("Imeis");

                    b.Navigation("ModelSanPhamKhuyenMais");
                });

            modelBuilder.Entity("DATN_DT.Models.SanPham", b =>
                {
                    b.Navigation("ModelSanPhams");
                });

            modelBuilder.Entity("DATN_DT.Models.Voucher", b =>
                {
                    b.Navigation("VoucherSuDungs");
                });
#pragma warning restore 612, 618
        }
    }
}
