namespace DATN_DT.Models
{
    public class VoucherSuDung
    {
        public int IdVoucherSuDung { get; set; }
        public int IdVoucher { get; set; }
        public int IdKhachHang { get; set; }
        public int? IdHoaDon { get; set; }
        public decimal SoTienGiam { get; set; }
        public DateTime NgaySuDung { get; set; }
        
        // Navigation properties
        public Voucher? Voucher { get; set; }
        public KhachHang? KhachHang { get; set; }
        public HoaDon? HoaDon { get; set; }
    }
}

