using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DATN_DT.Services
{
    public interface IVoucherService
    {
        Task<List<Voucher>> GetAllVouchersAsync();
        Task<Voucher?> GetVoucherByIdAsync(int id);
        Task<Voucher?> GetVoucherByCodeAsync(string maVoucher);
        Task<bool> ValidateVoucherAsync(string maVoucher, int idKhachHang, decimal tongTienDonHang);
        Task<decimal> CalculateDiscountAsync(Voucher voucher, decimal tongTienDonHang, List<int>? danhSachIdSanPham = null);
        Task<bool> UseVoucherAsync(int idVoucher, int idKhachHang, int? idHoaDon, decimal soTienGiam);
        Task<Voucher> CreateVoucherAsync(Voucher voucher);
        Task<bool> UpdateVoucherAsync(Voucher voucher);
        Task<bool> DeleteVoucherAsync(int id);
        Task UpdateVoucherStatusAsync();
    }

    public class VoucherService : IVoucherService
    {
        private readonly MyDbContext _context;

        public VoucherService(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            return await _context.Vouchers
                .OrderByDescending(v => v.NgayBatDau)
                .ToListAsync();
        }

        public async Task<Voucher?> GetVoucherByIdAsync(int id)
        {
            return await _context.Vouchers.FindAsync(id);
        }

        public async Task<Voucher?> GetVoucherByCodeAsync(string maVoucher)
        {
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.MaVoucher == maVoucher);
        }

        public async Task<bool> ValidateVoucherAsync(string maVoucher, int idKhachHang, decimal tongTienDonHang)
        {
            var voucher = await GetVoucherByCodeAsync(maVoucher);
            if (voucher == null)
                return false;

            var now = DateTime.Now;

            // Kiểm tra trạng thái
            if (voucher.TrangThai != "HoatDong")
                return false;

            // Kiểm tra thời gian
            if (now < voucher.NgayBatDau || now > voucher.NgayKetThuc)
                return false;

            // Kiểm tra đơn hàng tối thiểu
            if (voucher.DonHangToiThieu.HasValue && tongTienDonHang < voucher.DonHangToiThieu.Value)
                return false;

            // Kiểm tra số lượng sử dụng tổng
            if (voucher.SoLuongSuDung.HasValue && voucher.SoLuongDaSuDung >= voucher.SoLuongSuDung.Value)
                return false;

            // Kiểm tra số lượng sử dụng mỗi khách hàng
            // Chỉ kiểm tra nếu SoLuongMoiKhachHang > 0 (0 hoặc null = không giới hạn)
            if (voucher.SoLuongMoiKhachHang.HasValue && voucher.SoLuongMoiKhachHang.Value > 0)
            {
                var soLanDaSuDung = await _context.VoucherSuDungs
                    .CountAsync(v => v.IdVoucher == voucher.IdVoucher && v.IdKhachHang == idKhachHang);
                
                if (soLanDaSuDung >= voucher.SoLuongMoiKhachHang.Value)
                    return false;
            }

            return true;
        }

        public async Task<decimal> CalculateDiscountAsync(Voucher voucher, decimal tongTienDonHang, List<int>? danhSachIdSanPham = null)
        {
            // Kiểm tra đơn hàng tối thiểu
            if (voucher.DonHangToiThieu.HasValue && tongTienDonHang < voucher.DonHangToiThieu.Value)
                return 0;

            decimal discount = 0;

            if (voucher.LoaiGiam == "PhanTram")
            {
                discount = tongTienDonHang * (voucher.GiaTri / 100);
                
                // Áp dụng giảm tối đa nếu có
                if (voucher.GiamToiDa.HasValue && discount > voucher.GiamToiDa.Value)
                    discount = voucher.GiamToiDa.Value;
            }
            else if (voucher.LoaiGiam == "SoTien")
            {
                discount = voucher.GiaTri;
                
                // Không giảm quá tổng tiền
                if (discount > tongTienDonHang)
                    discount = tongTienDonHang;
            }

            return Math.Round(discount, 0);
        }

        public async Task<bool> UseVoucherAsync(int idVoucher, int idKhachHang, int? idHoaDon, decimal soTienGiam)
        {
            var voucher = await GetVoucherByIdAsync(idVoucher);
            if (voucher == null)
                return false;

            // Tạo bản ghi sử dụng
            var voucherSuDung = new VoucherSuDung
            {
                IdVoucher = idVoucher,
                IdKhachHang = idKhachHang,
                IdHoaDon = idHoaDon,
                SoTienGiam = soTienGiam,
                NgaySuDung = DateTime.Now
            };

            _context.VoucherSuDungs.Add(voucherSuDung);

            // Cập nhật số lượng đã sử dụng
            voucher.SoLuongDaSuDung++;

            // Kiểm tra nếu hết số lượng thì cập nhật trạng thái
            if (voucher.SoLuongSuDung.HasValue && voucher.SoLuongDaSuDung >= voucher.SoLuongSuDung.Value)
            {
                voucher.TrangThai = "HetHan";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Voucher> CreateVoucherAsync(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        public async Task<bool> UpdateVoucherAsync(Voucher voucher)
        {
            try
            {
                _context.Vouchers.Update(voucher);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteVoucherAsync(int id)
        {
            var voucher = await GetVoucherByIdAsync(id);
            if (voucher == null)
                return false;

            // Kiểm tra xem có đang được sử dụng không
            var hasUsage = await _context.VoucherSuDungs
                .AnyAsync(v => v.IdVoucher == id);

            if (hasUsage)
            {
                // Không xóa, chỉ đổi trạng thái
                voucher.TrangThai = "TamDung";
                await _context.SaveChangesAsync();
                return true;
            }

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateVoucherStatusAsync()
        {
            var now = DateTime.Now;
            var vouchers = await _context.Vouchers
                .Where(v => v.TrangThai == "HoatDong" || v.TrangThai == "SapDienRa")
                .ToListAsync();

            foreach (var voucher in vouchers)
            {
                if (now > voucher.NgayKetThuc)
                {
                    voucher.TrangThai = "HetHan";
                }
                else if (now >= voucher.NgayBatDau && now <= voucher.NgayKetThuc)
                {
                    if (voucher.TrangThai == "SapDienRa")
                        voucher.TrangThai = "HoatDong";
                }
                else if (now < voucher.NgayBatDau)
                {
                    voucher.TrangThai = "SapDienRa";
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

