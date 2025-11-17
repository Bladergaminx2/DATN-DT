using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface IModelSanPhamService
    {
        Task Create(ModelSanPham modelSanPham);
        Task Delete(int id);
        Task<ModelSanPham?> GetModelSanPhamById(int id);
        Task<List<ModelSanPham>> GetAllModelSanPhams();
        Task Update(ModelSanPham modelSanPham);
    }
}
