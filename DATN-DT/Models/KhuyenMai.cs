namespace DATN_DT.Models
{
    public class KhuyenMai
    {
        public int IdKhuyenMai { get; set; }
        public string? MaKM { get; set; }
        public string? MoTaKhuyenMai { get; set; }
        public string? LoaiGiam { get; set; }
        public string? ApDungVoi { get; set; }
        public decimal? GiaTri { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? TrangThaiKM { get; set; }
    }
}
