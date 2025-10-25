namespace DATN_DT.Models
{
    public class NhanVien
    {
        public int IdNhanVien { get; set; }
        public string? TenTaiKhoanNV { get; set; }
        public string? Password { get; set; }
        public string? HoTenNhanVien { get; set; }
        public int? IdChucVu { get; set; }
        public string? SdtNhanVien { get; set; }
        public string? EmailNhanVien { get; set; }
        public string? DiaChiNV { get; set; }
        public DateTime? NgayVaoLam { get; set; }
        public string? TrangThaiNV { get; set; }



        // Navigation property
        public ChucVu? ChucVu { get; set; }
    }
}
