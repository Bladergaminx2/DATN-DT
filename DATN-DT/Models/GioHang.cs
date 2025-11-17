namespace DATN_DT.Models
{
    using DATN_DT.Models;

    public class GioHang
    {
        public int IdGioHang { get; set; }
        public int? IdKhachHang { get; set; }

        // Navigation properties
        public KhachHang? KhachHang { get; set; }
        public ICollection<GioHangChiTiet>? GioHangChiTiets { get; set; }
    }

}