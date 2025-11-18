using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly INhanVienRepo _INhanVienRepo;
        public NhanVienService(INhanVienRepo nhanVienRepo)
        {
            _INhanVienRepo = nhanVienRepo;
        }
        public async Task Create(NhanVien nhanvien)
        {
            await _INhanVienRepo.Create(nhanvien);
        }

        public async Task Delete(int id)
        {
            await _INhanVienRepo.Delete(id);
        }

        public Task<List<NhanVien>> GetAllNhanViens()
        {
            var response =  _INhanVienRepo.GetAllNhanViens();
            return response;
        }

        public Task<NhanVien?> GetNhanVienById(int id)
        {
            var response =  _INhanVienRepo.GetNhanVienById(id);
            return response;
        }

        public async Task Update(NhanVien nhanvien)
        {
            await _INhanVienRepo.Update(nhanvien);
        }
    }
}
