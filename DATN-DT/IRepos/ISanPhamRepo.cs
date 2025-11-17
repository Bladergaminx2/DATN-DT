using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface ISanPhamRepo
    {
        Task Create(SanPham sanpham);
        Task Delete(int id);
        Task<SanPham?> GetSanPhamById(int id);
        Task<List<SanPham>> GetAllSanPhams();
        Task SaveChanges();
        Task Update(SanPham sanpham);
    }
}
