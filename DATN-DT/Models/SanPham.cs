namespace DATN_DT.Models
{
    public class SanPham
    {
        public int IdSanPham { get; set; }
        public string? MaSanPham { get; set; }
        public string? TenSanPham { get; set; }
        public int? IdThuongHieu { get; set; }
        public string? MoTa { get; set; }
        public decimal? GiaNiemYet { get; set; }
        public string? TrangThaiSP { get; set; }
        public decimal? VAT { get; set; }



        // Navigation property
        public ThuongHieu? ThuongHieu { get; set; }
    }
}
