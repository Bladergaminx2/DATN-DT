using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface IRAMService
    {
        Task Create(RAM ram);
        Task Delete(int id);
        Task<RAM?> GetRAMById(int id);
        Task<List<RAM>> GetAllRAMs();
        Task Update(RAM ram);
    }
}
