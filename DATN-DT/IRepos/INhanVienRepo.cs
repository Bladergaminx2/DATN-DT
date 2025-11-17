using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface INhanVienRepo
    {
        Task Create(NhanVien nhanvien);
        Task Delete(int id);
        Task<NhanVien?> GetNhanVienById(int id);
        Task<List<NhanVien>> GetAllNhanViens();
        Task SaveChanges();
        Task Update(NhanVien nhanvien);
    }
}
