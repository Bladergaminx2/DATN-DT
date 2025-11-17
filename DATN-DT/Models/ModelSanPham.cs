namespace DATN_DT.Models
{
    public class ModelSanPham
    {
        public int IdModelSanPham { get; set; }
        public string? TenModel { get; set; }
        public int? IdSanPham { get; set; }
        public int? IdManHinh { get; set; }
        public int? IdCameraTruoc { get; set; }
        public int? IdCameraSau { get; set; }
        public int? IdPin { get; set; }
        public int? IdRAM { get; set; }
        public int? IdROM { get; set; }
        public string? Mau { get; set; }
        public decimal? GiaBanModel { get; set; }
        public int TrangThai { get; set; } = 1; // 1: Active, 0: Inactive


        // Navigation properties
        public SanPham? SanPham { get; set; }
        public ManHinh? ManHinh { get; set; }
        public CameraTruoc? CameraTruoc { get; set; }
        public CameraSau? CameraSau { get; set; }
        public Pin? Pin { get; set; }
        public RAM? RAM { get; set; }
        public ROM? ROM { get; set; }
        public ICollection<AnhSanPham>? AnhSanPhams { get; set; }
        public ICollection<Imei>? Imeis { get; set; }
    }
}
