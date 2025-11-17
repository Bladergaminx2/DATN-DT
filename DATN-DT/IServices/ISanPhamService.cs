using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface ISanPhamService
    {
        Task Create(SanPham sanpham);
        Task Delete(int id);
        Task<SanPham?> GetSanPhamById(int id);
        Task<List<SanPham>> GetAllSanPhams();
        Task Update(SanPham sanpham);
    }
}
