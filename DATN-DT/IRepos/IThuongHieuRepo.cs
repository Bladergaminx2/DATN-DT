using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface IThuongHieuRepo
    {
        Task Create(ThuongHieu thuongHieu);
        Task Delete(int id);
        Task<ThuongHieu?> GetThuongHieuById(int id);
        Task<List<ThuongHieu>> GetAllThuongHieus();
        Task SaveChanges();
        Task Update(ThuongHieu thuongHieu);
    }
}
