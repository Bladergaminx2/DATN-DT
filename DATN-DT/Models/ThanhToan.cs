namespace DATN_DT.Models
{
    public class ThanhToan
    {
        public int IdThanhToan { get; set; }
        public int? IdHoaDon { get; set; }
        public string? HinhThuc { get; set; }
        public decimal? SoTien { get; set; }
        public DateTime? NgayThanhToan { get; set; }



        // Navigation properties
        public HoaDon? HoaDon { get; set; }
    }
}
