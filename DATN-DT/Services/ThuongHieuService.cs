using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ThuongHieuService : IThuongHieuService
    {
        private readonly IThuongHieuRepo _thuongHieuRepo;
        public ThuongHieuService(IThuongHieuRepo thuongHieuRepo)
        {
            _thuongHieuRepo = thuongHieuRepo;
        }
        public async Task CreateThuongHieu(ThuongHieu thuongHieu)
        {
            await _thuongHieuRepo.Create(thuongHieu);
        }

        public async Task DeleteThuongHieu(int id)
        {
            await _thuongHieuRepo.Delete(id);
        }

        public async Task<List<ThuongHieu>> GetAllThuongHieus()
        {
           return await _thuongHieuRepo.GetAllThuongHieus();
        }

        public Task<ThuongHieu?> GetThuongHieuById(int id)
        {
            return _thuongHieuRepo.GetThuongHieuById(id);
        }

        public async Task UpdateThuongHieu(ThuongHieu thuongHieu)
        {
            await _thuongHieuRepo.Update(thuongHieu);
        }
    }
}
