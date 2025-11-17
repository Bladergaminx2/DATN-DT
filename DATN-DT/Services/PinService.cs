using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class PinService : IPinService
    {
        private readonly HttpClient _httpClient;
        public PinService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Create(Pin pin)
        {
            await _httpClient.PostAsJsonAsync<Pin>("https://localhost:7150/api/Pins", pin);
        }

        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/Pins/{id}");
        }

        public Task<List<Pin>> GetAllPins()
        {
            var response =  _httpClient.GetFromJsonAsync<List<Pin>>("https://localhost:7150/api/Pins");
            return response;
        }

        public Task<Pin?> GetPinById(int id)
        {
            var response =  _httpClient.GetFromJsonAsync<Pin>($"https://localhost:7150/api/Pins/{id}");
            return response;
        }

        public async Task Update(Pin pin)
        {
            await _httpClient.PutAsJsonAsync<Pin>($"https://localhost:7150/api/Pins/{pin.IdPin}", pin);
        }
    }
}
