using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ModelSanPhamService : IModelSanPhamService
    {
        private readonly IModelSanPhamRepo _IModelSanPhamRepo;
        public ModelSanPhamService(IModelSanPhamRepo Imodelsprepo)
        {
            _IModelSanPhamRepo = Imodelsprepo;
        }
        public async Task Create(ModelSanPham modelSanPham)
        {
            await _IModelSanPhamRepo.Create(modelSanPham);
        }

        public async Task Delete(int id)
        {
            await _IModelSanPhamRepo.Delete(id);
        }

        public Task<List<ModelSanPham>> GetAllModelSanPhams()
        {
            var response =  _IModelSanPhamRepo.GetAllModelSanPhams();
            return response;
        }

        public async Task<ModelSanPham?> GetModelSanPhamById(int id)
        {
            var response = await _IModelSanPhamRepo.GetModelSanPhamById(id);
            return response;
        }

        public async Task Update(ModelSanPham modelSanPham)
        {
            await _IModelSanPhamRepo.Update(modelSanPham);
        }
    }
}
