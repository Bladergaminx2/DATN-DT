using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class RAMService : IRAMService
    {
        private readonly HttpClient _httpClient;
        public RAMService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Create(RAM ram)
        {
            await _httpClient.PostAsJsonAsync<RAM>("https://localhost:7150/api/RAM", ram);
        }

        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/RAM/{id}");
        }

        public Task<List<RAM>> GetAllRAMs()
        {
            var response =  _httpClient.GetFromJsonAsync<List<RAM>>("https://localhost:7150/api/RAM");
            return response;
        }

        public Task<RAM?> GetRAMById(int id)
        {
            var response =  _httpClient.GetFromJsonAsync<RAM>($"https://localhost:7150/api/RAM/{id}");
            return response;
        }

        public async Task Update(RAM ram)
        {
            await _httpClient.PutAsJsonAsync<RAM>($"https://localhost:7150/api/RAM/{ram.IdRAM}", ram);
        }
    }
}
