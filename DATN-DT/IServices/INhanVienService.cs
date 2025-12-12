using DATN_DT.Form;
using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface INhanVienService
    {
        Task Create(NhanVienFormSystemCreate nhanvien);
        Task Delete(int id);
        Task<List<NhanVien>> GetAllNhanViens();
        Task<List<NhanVien>> GetAllNhanViensIncludingAdmin();
        Task<NhanVien?> GetNhanVienById(int id);
        Task<List<ChucVu>> GetChucVus();
        Task<List<ChucVu>> GetChucVusExceptAdmin();
        Task Update(int id,NhanVienFormSystem nhanvien);
        Task<bool> IsAdminRole(int idChucVu);
    }
}
