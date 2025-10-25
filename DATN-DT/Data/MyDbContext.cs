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
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<HoaDonChiTiet> HoaDonChiTiets { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<GioHangChiTiet> GioHangChiTiets { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<DonHangChiTiet> DonHangChiTiets { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

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
            modelBuilder.Entity<HoaDon>().HasKey(e => e.IdHoaDon);
            modelBuilder.Entity<HoaDonChiTiet>().HasKey(e => e.IdHoaDonChiTiet);
            modelBuilder.Entity<GioHang>().HasKey(e => e.IdGioHang);
            modelBuilder.Entity<GioHangChiTiet>().HasKey(e => e.IdGioHangChiTiet);
            modelBuilder.Entity<DonHang>().HasKey(e => e.IdDonHang);
            modelBuilder.Entity<DonHangChiTiet>().HasKey(e => e.IdDonHangChiTiet);
            modelBuilder.Entity<ThanhToan>().HasKey(e => e.IdThanhToan);



            base.OnModelCreating(modelBuilder);
        }
    }
}
