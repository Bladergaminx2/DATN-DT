using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface IPinService
    {
        Task Create(Pin pin);
        Task Delete(int id);
        Task<Pin?> GetPinById(int id);
        Task<List<Pin>> GetAllPins();
        Task Update(Pin pin);
    }
}
