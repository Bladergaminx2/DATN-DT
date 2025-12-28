namespace DATN_DT.DTO
{
    public class hoadondtotest
    {
        // ViewModels
        public class ThanhToanViewModel
        {
            public List<CartItemViewModel> SelectedCartItems { get; set; } = new();
            public decimal TongTien { get; set; }
            public string SelectedItemsString { get; set; }
            public DiaChiDTO DiaChi { get; set; }

            public int PhiVanChuyen { get; set; } // NEW (đơn vị VND)
        }


        public class CartItemViewModel
        {
            public int IdGioHangChiTiet { get; set; }
            public int IdModelSanPham { get; set; }
            public int SoLuong { get; set; }
            public decimal ThanhTien { get; set; }
            public ModelSanPhamViewModel ModelSanPham { get; set; }
        }

        public class ModelSanPhamViewModel
        {
            public string TenModel { get; set; }
            public string Mau { get; set; }
            public decimal GiaBanModel { get; set; }
            public SanPhamViewModel SanPham { get; set; }
            public RAMViewModel RAM { get; set; }
            public ROMViewModel ROM { get; set; }
            public List<AnhSanPhamViewModel> AnhSanPhams { get; set; } = new List<AnhSanPhamViewModel>();
        }

        public class SanPhamViewModel
        {
            public string TenSanPham { get; set; }
            public ThuongHieuViewModel ThuongHieu { get; set; }
        }

        public class ThuongHieuViewModel
        {
            public string TenThuongHieu { get; set; }
        }

        public class RAMViewModel
        {
            public string DungLuongRAM { get; set; }
        }

        public class ROMViewModel
        {
            public string DungLuongROM { get; set; }
        }

        public class AnhSanPhamViewModel
        {
            public string DuongDan { get; set; }
        }

        public class UpdateQuantityModel
        {
            public int CartItemId { get; set; }
            public int Quantity { get; set; }
        }

        public class AddToCartModel
        {
            public int IdModelSanPham { get; set; }
            public int Quantity { get; set; } = 1;
        }
    }
}
