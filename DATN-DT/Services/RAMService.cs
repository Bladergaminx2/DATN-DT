using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class RAMService : IRAMService
    {
        private readonly IRAMRepo _IRamrepo;
        public RAMService(IRAMRepo Iramrepo)
        {
            _IRamrepo = Iramrepo;
        }
        public async Task Create(RAM ram)
        {
            await _IRamrepo.Create(ram);
        }

        public async Task Delete(int id)
        {
            await _IRamrepo.Delete(id);
        }

        public Task<List<RAM>> GetAllRAMs()
        {
            var response = _IRamrepo.GetAllRAMs();
            return response;
        }

        public Task<RAM?> GetRAMById(int id)
        {
            var response = _IRamrepo.GetRAMById(id);
            return response;
        }

        public async Task Update(RAM ram)
        {
            await _IRamrepo.Update(ram);
        }
    }
}
