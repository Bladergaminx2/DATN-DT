using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ROMService : IROMService
    {
        private readonly HttpClient _httpClient;
        public ROMService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Create(ROM rom)
        {
            await _httpClient.PostAsJsonAsync<ROM>("https://localhost:7150/api/ROM", rom);
        }

        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/ROM/{id}");
        }

        public Task<List<ROM>> GetAllROMs()
        {
            var response =  _httpClient.GetFromJsonAsync<List<ROM>>("https://localhost:7150/api/ROM");
            return response;
        }

        public Task<ROM?> GetROMById(int id)
        {
            var response =  _httpClient.GetFromJsonAsync<ROM>($"https://localhost:7150/api/ROM/{id}");
            return response;
        }

        public async Task Update(ROM rom)
        {
            await _httpClient.PutAsJsonAsync<ROM>($"https://localhost:7150/api/ROM/{rom.IdROM}", rom);
        }
    }
}
