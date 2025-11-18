using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface IThuongHieuService
    {
        Task CreateThuongHieu(ThuongHieu thuongHieu);
        Task DeleteThuongHieu(int id);
        Task<List<ThuongHieu>> GetAllThuongHieus();
        Task<ThuongHieu?> GetThuongHieuById(int id);
        Task UpdateThuongHieu(ThuongHieu thuongHieu);
    }
}
