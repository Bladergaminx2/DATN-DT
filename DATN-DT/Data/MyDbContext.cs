using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Kho> Khos { get; set; }
        public DbSet<ChucVu> ChucVus { get; set; }
        public DbSet<ThuongHieu> ThuongHieus { get; set; }
        public DbSet<RAM> RAMs { get; set; }
        public DbSet<ROM> ROMs { get; set; }
        public DbSet<Pin> Pins { get; set; }
        public DbSet<CameraSau> CameraSaus { get; set; }
        public DbSet<CameraTruoc> CameraTruocs { get; set; }
        public DbSet<ManHinh> ManHinhs { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<ModelSanPham> ModelSanPhams { get; set; }
        public DbSet<AnhSanPham> AnhSanPhams { get; set; }
        public DbSet<TonKho> TonKhos { get; set; }
        public DbSet<Imei> Imeis { get; set; }
        public DbSet<BaoHanh> BaoHanhs { get; set; }
        public DbSet<BaoHanhLichSu> BaoHanhLichSus { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<HoaDonChiTiet> HoaDonChiTiets { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<GioHangChiTiet> GioHangChiTiets { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<DonHangChiTiet> DonHangChiTiets { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }
        public DbSet<DiaChi> diachis { get; set; }
        public DbSet<ModelSanPhamKhuyenMai> ModelSanPhamKhuyenMais { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherSuDung> VoucherSuDungs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Primary Keys
            modelBuilder.Entity<Kho>().HasKey(e => e.IdKho);
            modelBuilder.Entity<ChucVu>().HasKey(e => e.IdChucVu);
            modelBuilder.Entity<ThuongHieu>().HasKey(e => e.IdThuongHieu);
            modelBuilder.Entity<RAM>().HasKey(e => e.IdRAM);
            modelBuilder.Entity<ROM>().HasKey(e => e.IdROM);
            modelBuilder.Entity<Pin>().HasKey(e => e.IdPin);
            modelBuilder.Entity<CameraSau>().HasKey(e => e.IdCameraSau);
            modelBuilder.Entity<CameraTruoc>().HasKey(e => e.IdCamTruoc);
            modelBuilder.Entity<ManHinh>().HasKey(e => e.IdManHinh);
            modelBuilder.Entity<KhachHang>().HasKey(e => e.IdKhachHang);
            modelBuilder.Entity<KhuyenMai>().HasKey(e => e.IdKhuyenMai);
            modelBuilder.Entity<NhanVien>().HasKey(e => e.IdNhanVien);
            modelBuilder.Entity<SanPham>().HasKey(e => e.IdSanPham);
            modelBuilder.Entity<ModelSanPham>().HasKey(e => e.IdModelSanPham);
            modelBuilder.Entity<AnhSanPham>().HasKey(e => e.IdAnh);
            modelBuilder.Entity<TonKho>().HasKey(e => e.IdTonKho);
            modelBuilder.Entity<Imei>().HasKey(e => e.IdImei);
            modelBuilder.Entity<BaoHanh>().HasKey(e => e.IdBaoHanh);
            modelBuilder.Entity<BaoHanhLichSu>().HasKey(e => e.IdBaoHanhLichSu);
            modelBuilder.Entity<HoaDon>().HasKey(e => e.IdHoaDon);
            modelBuilder.Entity<HoaDonChiTiet>().HasKey(e => e.IdHoaDonChiTiet);
            modelBuilder.Entity<GioHang>().HasKey(e => e.IdGioHang);
            modelBuilder.Entity<GioHangChiTiet>().HasKey(e => e.IdGioHangChiTiet);
            modelBuilder.Entity<DonHang>().HasKey(e => e.IdDonHang);
            modelBuilder.Entity<DonHangChiTiet>().HasKey(e => e.IdDonHangChiTiet);
            modelBuilder.Entity<ThanhToan>().HasKey(e => e.IdThanhToan);
            modelBuilder.Entity<ModelSanPhamKhuyenMai>().HasKey(e => e.IdModelSanPhamKhuyenMai);
            modelBuilder.Entity<Voucher>().HasKey(e => e.IdVoucher);
            modelBuilder.Entity<VoucherSuDung>().HasKey(e => e.IdVoucherSuDung);
            modelBuilder.Entity<HoaDon>().ToTable("HoaDon"); // hoặc "HoaDons" tùy DB


            // === QUAN HỆ CHÍNH ===

            // SanPham -> ThuongHieu
            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.ThuongHieu)
                .WithMany()
                .HasForeignKey(s => s.IdThuongHieu)
                .OnDelete(DeleteBehavior.Restrict);

            // SanPham -> ModelSanPham (1-n)
            modelBuilder.Entity<SanPham>()
                .HasMany(s => s.ModelSanPhams)
                .WithOne(m => m.SanPham)
                .HasForeignKey(m => m.IdSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            // ModelSanPham -> các component
            modelBuilder.Entity<ModelSanPham>()
                .HasOne(m => m.ManHinh)
                .WithMany()
                .HasForeignKey(m => m.IdManHinh)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModelSanPham>()
                .HasOne(m => m.CameraTruoc)
                .WithMany()
                .HasForeignKey(m => m.IdCameraTruoc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModelSanPham>()
                .HasOne(m => m.CameraSau)
                .WithMany()
                .HasForeignKey(m => m.IdCameraSau)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModelSanPham>()
                .HasOne(m => m.Pin)
                .WithMany()
                .HasForeignKey(m => m.IdPin)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModelSanPham>()
                .HasOne(m => m.RAM)
                .WithMany()
                .HasForeignKey(m => m.IdRAM)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModelSanPham>()
                .HasOne(m => m.ROM)
                .WithMany()
                .HasForeignKey(m => m.IdROM)
                .OnDelete(DeleteBehavior.Restrict);

            // ModelSanPham -> AnhSanPham (1-n)
            modelBuilder.Entity<ModelSanPham>()
                .HasMany(m => m.AnhSanPhams)
                .WithOne(a => a.ModelSanPham)
                .HasForeignKey(a => a.IdModelSanPham)
                .OnDelete(DeleteBehavior.Cascade);

            // ModelSanPham -> Imei (1-n)
            modelBuilder.Entity<ModelSanPham>()
                .HasMany(m => m.Imeis)
                .WithOne(i => i.ModelSanPham)
                .HasForeignKey(i => i.IdModelSanPham)
.OnDelete(DeleteBehavior.Restrict);

            // NhanVien -> ChucVu
            modelBuilder.Entity<NhanVien>()
                .HasOne(n => n.ChucVu)
                .WithMany()
                .HasForeignKey(n => n.IdChucVu)
                .OnDelete(DeleteBehavior.Restrict);

            // TonKho relationships
            modelBuilder.Entity<TonKho>()
                .HasOne(t => t.ModelSanPham)
                .WithMany()
                .HasForeignKey(t => t.IdModelSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TonKho>()
                .HasOne(t => t.Kho)
                .WithMany()
                .HasForeignKey(t => t.IdKho)
                .OnDelete(DeleteBehavior.Restrict);

            // AnhSanPham -> ModelSanPham
            modelBuilder.Entity<AnhSanPham>()
                .HasOne(a => a.ModelSanPham)
                .WithMany(m => m.AnhSanPhams)
                .HasForeignKey(a => a.IdModelSanPham)
                .OnDelete(DeleteBehavior.Cascade);

            // BaoHanh relationships
            modelBuilder.Entity<BaoHanh>()
                .HasOne(b => b.Imei)
                .WithMany()
                .HasForeignKey(b => b.IdImei)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BaoHanh>()
                .HasOne(b => b.KhachHang)
                .WithMany()
                .HasForeignKey(b => b.IdKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BaoHanh>()
                .HasOne(b => b.NhanVien)
                .WithMany()
                .HasForeignKey(b => b.IdNhanVien)
                .OnDelete(DeleteBehavior.Restrict);

            // BaoHanhLichSu relationships
            modelBuilder.Entity<BaoHanhLichSu>()
                .HasOne(b => b.BaoHanh)
                .WithMany()
                .HasForeignKey(b => b.IdBaoHanh)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BaoHanhLichSu>()
                .HasOne(b => b.NhanVien)
                .WithMany()
                .HasForeignKey(b => b.IdNhanVien)
                .OnDelete(DeleteBehavior.SetNull);

            // DonHang -> KhachHang
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.KhachHang)
                .WithMany()
                .HasForeignKey(d => d.IdKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            // DonHang -> NhanVien
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.NhanVien)
                .WithMany()
                .HasForeignKey(d => d.IdNhanVien)
                .OnDelete(DeleteBehavior.Restrict);

            // DonHangChiTiet relationships
            modelBuilder.Entity<DonHangChiTiet>()
                .HasOne(d => d.DonHang)
                .WithMany(d => d.DonHangChiTiets)
                .HasForeignKey(d => d.IdDonHang)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DonHangChiTiet>()
                .HasOne(d => d.ModelSanPham)
                .WithMany()
                .HasForeignKey(d => d.IdModelSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            // GioHang -> KhachHang
            modelBuilder.Entity<GioHang>()
                .HasOne(g => g.KhachHang)
                .WithMany()
                .HasForeignKey(g => g.IdKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            // GioHangChiTiet relationships
            modelBuilder.Entity<GioHangChiTiet>()
                            .HasOne(g => g.GioHang)
                            .WithMany(g => g.GioHangChiTiets)
                            .HasForeignKey(g => g.IdGioHang)
                            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GioHangChiTiet>()
                .HasOne(g => g.ModelSanPham)
                .WithMany()
                .HasForeignKey(g => g.IdModelSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            // HoaDon relationships
            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.KhachHang)
                .WithMany()
                .HasForeignKey(h => h.IdKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.NhanVien)
                .WithMany()
                .HasForeignKey(h => h.IdNhanVien)
                .OnDelete(DeleteBehavior.Restrict);

            // HoaDonChiTiet relationships
            modelBuilder.Entity<HoaDonChiTiet>()
                .HasOne(h => h.HoaDon)
                .WithMany(h => h.HoaDonChiTiets)
                .HasForeignKey(h => h.IdHoaDon)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HoaDonChiTiet>()
                .HasOne(h => h.ModelSanPham)
                .WithMany()
                .HasForeignKey(h => h.IdModelSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HoaDonChiTiet>()
                .HasOne(h => h.Imei)
                .WithMany()
                .HasForeignKey(h => h.IdImei)
                .OnDelete(DeleteBehavior.Restrict);

            // Imei -> ModelSanPham
            modelBuilder.Entity<Imei>()
                .HasOne(i => i.ModelSanPham)
                .WithMany(m => m.Imeis)
                .HasForeignKey(i => i.IdModelSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            // ThanhToan -> HoaDon
            modelBuilder.Entity<ThanhToan>()
                .HasOne(t => t.HoaDon)
                .WithMany()
                .HasForeignKey(t => t.IdHoaDon)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KhachHang>()
         .HasMany(k => k.Diachi)
         .WithOne(d => d.KhachHang)
         .HasForeignKey(d => d.IdKhachHang)
         .OnDelete(DeleteBehavior.Cascade);

            // ModelSanPhamKhuyenMai relationships
            modelBuilder.Entity<ModelSanPhamKhuyenMai>()
                .HasOne(m => m.ModelSanPham)
                .WithMany(msp => msp.ModelSanPhamKhuyenMais)
                .HasForeignKey(m => m.IdModelSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModelSanPhamKhuyenMai>()
                .HasOne(m => m.KhuyenMai)
                .WithMany()
                .HasForeignKey(m => m.IdKhuyenMai)
                .OnDelete(DeleteBehavior.Restrict);

            // Voucher relationships
            modelBuilder.Entity<VoucherSuDung>()
                .HasOne(v => v.Voucher)
                .WithMany(v => v.VoucherSuDungs)
                .HasForeignKey(v => v.IdVoucher)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VoucherSuDung>()
                .HasOne(v => v.KhachHang)
                .WithMany()
                .HasForeignKey(v => v.IdKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VoucherSuDung>()
                .HasOne(v => v.HoaDon)
                .WithMany()
                .HasForeignKey(v => v.IdHoaDon)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}