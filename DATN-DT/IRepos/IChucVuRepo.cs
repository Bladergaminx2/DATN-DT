using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface IChucVuRepo
    {
        Task Create(ChucVu chucVu);
        Task Delete(int id);
        Task<ChucVu?> GetChucVuById(int id);
        Task<List<ChucVu>> GetAllChucVus();
        Task Update(ChucVu chucVu);
    }
}

