using DATN_DT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.IRepos
{
    public interface ISanPhamRepo
    {
        Task Create(SanPham sanPham);
        Task Delete(int id);
        Task<List<SanPham>> GetAllSanPhams();
        Task<SanPham?> GetSanPhamById(int id);
        Task SaveChanges();
        Task Update(SanPham sanPham);
        Task<List<ThuongHieu>> GetAllThuongHieu(); // Thêm phương thức này
    }
}