namespace DATN_DT.Models
{
    public class ModelSanPhamKhuyenMai
    {
        public int IdModelSanPhamKhuyenMai { get; set; }
        public int? IdModelSanPham { get; set; }
        public int? IdKhuyenMai { get; set; }
        public DateTime? NgayTao { get; set; }

        // Navigation properties
        public ModelSanPham? ModelSanPham { get; set; }
        public KhuyenMai? KhuyenMai { get; set; }
    }
}

