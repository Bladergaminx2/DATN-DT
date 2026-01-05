using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface IChucVuService
    {
        Task Create(ChucVu chucVu);
        Task Delete(int id);
        Task<ChucVu?> GetChucVuById(int id);
        Task<List<ChucVu>> GetAllChucVus();
        Task Update(ChucVu chucVu);
    }
}

