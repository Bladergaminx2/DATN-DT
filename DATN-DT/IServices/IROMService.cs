using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface IROMService
    {
        Task Create(ROM rom);
        Task Delete(int id);
        Task<ROM?> GetROMById(int id);
        Task<List<ROM>> GetAllROMs();
        Task Update(ROM rom);
    }
}
