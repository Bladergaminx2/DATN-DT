using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class PinService : IPinService
    {
        private readonly IPinRepo _IPinRepo;
        public PinService(IPinRepo Ipinrepo)
        {
            _IPinRepo = Ipinrepo;
        }
        public async Task Create(Pin pin)
        {
            await _IPinRepo.Create(pin);
        }

        public async Task Delete(int id)
        {
            await _IPinRepo.Delete(id);
        }

        public Task<List<Pin>> GetAllPins()
        {
            var response =  _IPinRepo.GetAllPins();
            return response;
        }

        public Task<Pin?> GetPinById(int id)
        {
            var response =  _IPinRepo.GetPinById(id);
            return response;
        }

        public async Task Update(Pin pin)
        {
            await _IPinRepo.Update(pin);
        }
    }
}
