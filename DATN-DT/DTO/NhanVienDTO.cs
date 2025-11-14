namespace DATN_DT.DTO
{
    public class NhanVienDTO
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
        public int? TrangThaiNV { get; set; }
    }
}
