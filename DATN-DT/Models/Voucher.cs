namespace DATN_DT.Models
{
    public class Voucher
    {
        public int IdVoucher { get; set; }
        public string MaVoucher { get; set; } = string.Empty; // VD: "SALE2024", "WELCOME10"
        public string TenVoucher { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        
        // Loại giảm giá
        public string LoaiGiam { get; set; } = "PhanTram"; // "PhanTram" hoặc "SoTien"
        public decimal GiaTri { get; set; } // % hoặc số tiền
        
        // Điều kiện áp dụng
        public decimal? DonHangToiThieu { get; set; } // Đơn hàng tối thiểu
        public decimal? GiamToiDa { get; set; } // Giảm tối đa (nếu giảm %)
        
        // Thời gian
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        
        // Giới hạn sử dụng
        public int? SoLuongSuDung { get; set; } // Tổng số lần sử dụng
        public int SoLuongDaSuDung { get; set; } = 0; // Đã sử dụng bao nhiêu
        public int? SoLuongMoiKhachHang { get; set; } // Mỗi khách hàng dùng tối đa bao nhiêu lần
        
        // Trạng thái
        public string TrangThai { get; set; } = "HoatDong"; // "HoatDong", "TamDung", "HetHan"
        
        // Áp dụng cho
        public string? ApDungCho { get; set; } = "TatCa"; // "TatCa", "SanPham", "DanhMuc", "ThuongHieu"
        public string? DanhSachId { get; set; } // JSON array IDs nếu áp dụng cho sản phẩm/danh mục cụ thể
        
        // Navigation properties
        public ICollection<VoucherSuDung> VoucherSuDungs { get; set; } = new List<VoucherSuDung>();
    }
}

