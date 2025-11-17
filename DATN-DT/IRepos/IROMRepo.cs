using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface IROMRepo
    {
        Task Create(ROM rom);
        Task Delete(int id);
        Task<ROM?> GetROMById(int id);
        Task<List<ROM>> GetAllROMs();
        Task SaveChanges();
        Task Update(ROM rom);
    }
}
