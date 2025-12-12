using DATN_DT.Form;
using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;
using DATN_DT.Repos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DATN_DT.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly INhanVienRepo _INhanVienRepo;

        public NhanVienService(INhanVienRepo nhanVienRepo)
        {
            _INhanVienRepo = nhanVienRepo;
        }

        public async Task Create(NhanVienFormSystemCreate nhanvien)
        {
            var data = new NhanVien();
            data.EmailNhanVien = nhanvien.EmailNhanVien;
            data.Password = nhanvien.Password;
            data.TenTaiKhoanNV = nhanvien?.TenTaiKhoanNV;
            data.HoTenNhanVien = nhanvien?.HoTenNhanVien;
            data.SdtNhanVien = nhanvien?.SdtNhanVien;
            data.IdChucVu = nhanvien?.IdChucVu;
            data.DiaChiNV = nhanvien?.DiaChiNV;
            data.NgayVaoLam = DateTime.Now;
            data.TrangThaiNV = 1;
            await _INhanVienRepo.Create(data);
        }

        public async Task Delete(int id)
        {
            await _INhanVienRepo.Delete(id);
        }

        public async Task<List<NhanVien>> GetAllNhanViensIncludingAdmin()
        {
            return await _INhanVienRepo.GetAllNhanViensIncludingAdmin();
        }

        public async Task<bool> IsAdminRole(int idChucVu)
        {
            return await _INhanVienRepo.IsAdminRole(idChucVu);
        }

        public Task<List<NhanVien>> GetAllNhanViens()
        {
            var response = _INhanVienRepo.GetAllNhanViens();
            return response;
        }

        public Task<NhanVien?> GetNhanVienById(int id)
        {
            var response = _INhanVienRepo.GetNhanVienById(id);
            return response;
        }

        public async Task<List<ChucVu>> GetChucVus()
        {
            return await _INhanVienRepo.GetChucVus();
        }

        public async Task<List<ChucVu>> GetChucVusExceptAdmin()
        {
            return await _INhanVienRepo.GetChucVusExceptAdmin();
        }

        public async Task Update(int id, NhanVienFormSystem nhanvien)
        {
            if (nhanvien.IdChucVu.HasValue && await _INhanVienRepo.IsAdminRole(nhanvien.IdChucVu.Value))
            {
                throw new InvalidOperationException("Không thể cập nhật nhân viên thành role Admin");
            }

            var data = await _INhanVienRepo.GetNhanVienById(id);
            if (data == null)
            {
                throw new InvalidOperationException("Không tìm thấy nhân viên");
            }

            data.EmailNhanVien = nhanvien.EmailNhanVien;
            data.TenTaiKhoanNV = nhanvien?.TenTaiKhoanNV;
            data.HoTenNhanVien = nhanvien?.HoTenNhanVien;
            data.SdtNhanVien = nhanvien?.SdtNhanVien;
            data.IdChucVu = nhanvien?.IdChucVu;
            data.DiaChiNV = nhanvien?.DiaChiNV;
            data.TrangThaiNV = nhanvien?.TrangThaiNV;

            await _INhanVienRepo.Update(data);
        }
    }
}