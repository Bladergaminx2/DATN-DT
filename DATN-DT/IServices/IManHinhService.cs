using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface IManHinhService
    {
        Task Create(ManHinh manHinh);
        Task Delete(int id);
        Task<ManHinh?> GetManHinhById(int id);
        Task<List<ManHinh>> GetAllManHinhs();
        Task Update(ManHinh manHinh);

    }
}
