using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface IRAMRepo
    {
        Task Create(RAM ram);
        Task Delete(int id);
        Task<RAM?> GetRAMById(int id);
        Task<List<RAM>> GetAllRAMs();
        Task SaveChanges();
        Task Update(RAM ram);
    }
}
