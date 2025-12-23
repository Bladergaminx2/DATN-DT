using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Services
{
    public class ManHinhService : IManHinhService
    {
        private readonly IManHinhRepo _manHinhRepo;

        public ManHinhService(IManHinhRepo manHinhRepo)
        {
            _manHinhRepo = manHinhRepo;
        }

        public async Task Create(ManHinh manHinh)
        {
            await _manHinhRepo.Create(manHinh);
        }

        public async Task Delete(int id)
        {
            await _manHinhRepo.Delete(id);
        }

        public async Task<List<ManHinh>> GetAllManHinhs()
        {
            return await _manHinhRepo.GetAllManHinhs();
        }

        public async Task<ManHinh?> GetManHinhById(int id)
        {
            return await _manHinhRepo.GetManHinhById(id);
        }

        public async Task Update(ManHinh manHinh)
        {
            await _manHinhRepo.Update(manHinh);
        }
    }
}