using DATN_DT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.IRepos
{
    public interface IVoucherRepo
    {
        Task<List<Voucher>> GetAllAsync();
        Task<Voucher?> GetByIdAsync(int id);
        Task<Voucher?> GetByCodeAsync(string maVoucher);
        Task<Voucher> CreateAsync(Voucher voucher);
        Task<bool> UpdateAsync(Voucher voucher);
        Task<bool> DeleteAsync(int id);

        // Helpers
        Task<int> CountUsageByCustomerAsync(int voucherId, int idKhachHang);
        Task SaveChangesAsync();
    }
}