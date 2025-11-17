using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface INhanVienService
    {
        Task Create(NhanVien nhanvien);
        Task Delete(int id);
        Task<NhanVien?> GetNhanVienById(int id);
        Task<List<NhanVien>> GetAllNhanViens();
        Task Update(NhanVien nhanvien);
    }
}
