using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ChucVuService : IChucVuService
    {
        private readonly IChucVuRepo _chucVuRepo;
        public ChucVuService(IChucVuRepo chucVuRepo)
        {
            _chucVuRepo = chucVuRepo;
        }
        public async Task Create(ChucVu chucVu)
        {
            await _chucVuRepo.Create(chucVu);
        }

        public async Task Delete(int id)
        {
            await _chucVuRepo.Delete(id);
        }

        public Task<List<ChucVu>> GetAllChucVus()
        {
            var response = _chucVuRepo.GetAllChucVus();
            return response;
        }

        public Task<ChucVu?> GetChucVuById(int id)
        {
            var response = _chucVuRepo.GetChucVuById(id);
            return response;
        }

        public async Task Update(ChucVu chucVu)
        {
            await _chucVuRepo.Update(chucVu);
        }
    }
}

