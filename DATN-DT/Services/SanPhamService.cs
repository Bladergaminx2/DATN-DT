using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class SanPhamService : ISanPhamService
    {
        private readonly ISanPhamRepo _ISanPhamRepo;
        public SanPhamService(ISanPhamRepo ISanPhamRepo)
        {
            _ISanPhamRepo = ISanPhamRepo;
        }
        public async Task Create(SanPham sanpham)
        {
            await _ISanPhamRepo.Create(sanpham);
        }

        public async Task Delete(int id)
        {
            await _ISanPhamRepo.Delete(id);
        }

        public Task<List<SanPham>> GetAllSanPhams()
        {
            var response = _ISanPhamRepo.GetAllSanPhams();
            return response;
        }

        public Task<SanPham?> GetSanPhamById(int id)
        {
            var response = _ISanPhamRepo.GetSanPhamById(id);
            return response;
        }

        public async Task Update(SanPham sanpham)
        {
            await _ISanPhamRepo.Update(sanpham);
        }
    }
}
