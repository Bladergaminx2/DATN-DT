namespace DATN_DT.Models
{
    public class AnhSanPham
    {
        public int IdAnh { get; set; }
        public int? IdModelSanPham { get; set; }
        public string? DuongDan { get; set; }



        // Navigation properties
        public ModelSanPham? ModelSanPham { get; set; }
    }
}
