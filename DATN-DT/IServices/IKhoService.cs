using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface IKhoService
    {
        Task<List<Kho>> GetAllKhos();
        Task<Kho?> GetKhoById(int id);
        Task<bool> CheckDuplicate(string tenKho, int excludeId);
        Task Create(Kho kho);
        Task Update(Kho kho);
        Task Delete(int id);
    }
}