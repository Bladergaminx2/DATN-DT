namespace DATN_DT.Models
{
    public class TonKho
    {
        public int IdTonKho { get; set; }
        public int? IdModelSanPham { get; set; }
        public int? IdKho { get; set; }
        public int? SoLuong { get; set; }




        // Navigation properties
        public ModelSanPham? ModelSanPham { get; set; }
        public Kho? Kho { get; set; }
    }
}
