using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ManHinhService : IManHinhService
    {
        private readonly IManHinhRepo _IManHinhRepo;
        public ManHinhService(IManHinhRepo Imanhinhrepo)
        {
            _IManHinhRepo = Imanhinhrepo;
        }
        public async Task Create(ManHinh manHinh)
        {
            await _IManHinhRepo.Create(manHinh);
        }

        public async Task Delete(int id)
        {
            await _IManHinhRepo.Delete(id);
        }

        public Task<List<ManHinh>> GetAllManHinhs()
        {
            var response = _IManHinhRepo.GetAllManHinhs();
            return response;
        }

        public async Task<ManHinh?> GetManHinhById(int id)
        {
            var response = await _IManHinhRepo.GetManHinhById(id);
            return response;
        }

        public async Task Update(ManHinh manHinh)
        {
            await _IManHinhRepo.Update(manHinh);
        }
    }
}
