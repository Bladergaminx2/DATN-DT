namespace DATN_DT.Models
{
    public class BaoHanhLichSu
    {
        public int IdBaoHanhLichSu { get; set; }
        public int IdBaoHanh { get; set; }
        public int? IdNhanVien { get; set; } // Người thực hiện
        public string? ThaoTac { get; set; } // "Tạo mới", "Cập nhật trạng thái", "Thêm chi phí", etc.
        public string? TrangThaiCu { get; set; }
        public string? TrangThaiMoi { get; set; }
        public string? MoTa { get; set; } // Mô tả chi tiết thay đổi
        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Navigation properties
        public BaoHanh? BaoHanh { get; set; }
        public NhanVien? NhanVien { get; set; }
    }
}

