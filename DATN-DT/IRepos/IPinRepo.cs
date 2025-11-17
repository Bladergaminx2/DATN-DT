using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface IPinRepo
    {
        Task Create(Pin pin);
        Task Delete(int id);
        Task<Pin?> GetPinById(int id);
        Task<List<Pin>> GetAllPins();
        Task SaveChanges();
        Task Update(Pin pin);
    }
}
