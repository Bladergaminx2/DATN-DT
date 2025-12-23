using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Services
{
    public class SanPhamService : ISanPhamService
    {
        private readonly ISanPhamRepo _sanPhamRepo;
        public SanPhamService(ISanPhamRepo sanPhamRepo)
        {
            _sanPhamRepo = sanPhamRepo;
        }

        public async Task Create(SanPham sanPham)
        {
            await _sanPhamRepo.Create(sanPham);
        }

        public async Task Delete(int id)
        {
            await _sanPhamRepo.Delete(id);
        }

        public async Task<List<SanPham>> GetAllSanPhams()
        {
            return await _sanPhamRepo.GetAllSanPhams();
        }

        public async Task<SanPham?> GetSanPhamById(int id)
        {
            return await _sanPhamRepo.GetSanPhamById(id);
        }

        public async Task Update(SanPham sanPham)
        {
            await _sanPhamRepo.Update(sanPham);
        }

        public async Task<List<ThuongHieu>> GetAllThuongHieu()
        {
            // Gọi Repository để lấy danh sách thương hiệu
            return await _sanPhamRepo.GetAllThuongHieu();
        }
    }
}