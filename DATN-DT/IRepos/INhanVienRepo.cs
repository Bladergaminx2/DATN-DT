using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface INhanVienRepo
    {
        Task Create(NhanVien nhanvien);
        Task Delete(int id);
        Task<NhanVien?> GetNhanVienById(int id);
        Task<List<NhanVien>> GetAllNhanViens();
        Task<List<ChucVu>> GetChucVus();
        Task<List<ChucVu>> GetChucVusExceptAdmin();
        Task<List<NhanVien>> GetAllNhanViensIncludingAdmin();
        Task Update(NhanVien nhanvien);
        Task<bool> IsAdminRole(int idChucVu);
    }
}
