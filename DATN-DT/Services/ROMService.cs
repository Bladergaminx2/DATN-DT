using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ROMService : IROMService
    {
        private readonly IROMRepo _IRomrepo;
        public ROMService(IROMRepo Iromrepo)
        {
            _IRomrepo = Iromrepo;
        }
        public async Task Create(ROM rom)
        {
            await _IRomrepo.Create(rom);
        }

        public async Task Delete(int id)
        {
            await _IRomrepo.Delete(id);
        }

        public Task<List<ROM>> GetAllROMs()
        {
            var response =  _IRomrepo.GetAllROMs();
            return response;
        }

        public Task<ROM?> GetROMById(int id)
        {
            var response = _IRomrepo.GetROMById(id);
            return response;
        }

        public async Task Update(ROM rom)
        {
            await _IRomrepo.Update(rom);
        }
    }
}
