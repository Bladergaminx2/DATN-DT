using DATN_DT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.IServices
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
}