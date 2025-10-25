namespace DATN_DT.Models
{
    public class HoaDonChiTiet
    {
        public int IdHoaDonChiTiet { get; set; }
        public int? IdHoaDon { get; set; }
        public int? IdModelSanPham { get; set; }
        public int? IdImei { get; set; }
        public decimal? DonGia { get; set; }
        public int? SoLuong { get; set; }
        public decimal? ThanhTien { get; set; }



        // Navigation properties
        public HoaDon? HoaDon { get; set; }
        public ModelSanPham? ModelSanPham { get; set; }
        public Imei? Imei { get; set; }
    }
}
