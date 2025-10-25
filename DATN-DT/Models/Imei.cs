namespace DATN_DT.Models
{
    public class Imei
    {
        public int IdImei { get; set; }
        public string? MaImei { get; set; }
        public int? IdModelSanPham { get; set; }
        public string? MoTa { get; set; }
        public string? TrangThai { get; set; }



        // Navigation property
        public ModelSanPham? ModelSanPham { get; set; }
    }
}
