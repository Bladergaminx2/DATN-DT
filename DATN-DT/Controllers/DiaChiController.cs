using DATN_DT.Data;
using DATN_DT.Migrations;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DATN_DT.Controllers
{
    public class DiaChiController : Controller
    {
        private readonly MyDbContext _context;
        private readonly string _ghnApiKey = "7b4f1e5c-0700-11f0-94b6-be01e07a48b5";
        private readonly string _ghnApiUrl = "https://online-gateway.ghn.vn/shiip/public-api/master-data";

        public DiaChiController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Giả sử ID khách hàng là 1 (bạn có thể thay đổi theo logic của mình)
            ViewBag.IdKhachHang = 1;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetByKhachHang(int idKhachHang)
        {
            try
            {
                var diaChiList = await _context.diachis
                    .Where(dc => dc.IdKhachHang == idKhachHang)
                    .ToListAsync();

                // Lấy thông tin tỉnh/thành, quận/huyện, phường/xã từ GHN API
                var provincesTask = GetProvincesFromGHN();
                var districtsTask = GetDistrictsFromGHN();

                await Task.WhenAll(provincesTask, districtsTask);

                var provinces = provincesTask.Result;
                var districts = districtsTask.Result;

                var result = new List<object>();

                foreach (var dc in diaChiList)
                {
                    var wardName = await GetWardName(dc.Quanhuyen, dc.Phuongxa);
                    result.Add(new
                    {
                        dc.Id,
                        dc.IdKhachHang,
                        dc.Tennguoinhan,
                        dc.sdtnguoinhan,
                        dc.Thanhpho,
                        dc.Quanhuyen,
                        dc.Phuongxa,
                        dc.Diachicuthe,
                        dc.trangthai,
                        ProvinceName = provinces.ContainsKey(dc.Thanhpho) ? provinces[dc.Thanhpho] : "Không xác định",
                        DistrictName = districts.ContainsKey(dc.Quanhuyen) ? districts[dc.Quanhuyen] : "Không xác định",
                        WardName = wardName
                    });
                }

                return Ok(result.OrderBy(dc => ((dynamic)dc).trangthai).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DiaChi diaChi)
        {
            try
            {
                // Kiểm tra số lượng địa chỉ hiện có
                var existingAddresses = await _context.diachis
                    .Where(dc => dc.IdKhachHang == diaChi.IdKhachHang)
.CountAsync();

                if (existingAddresses >= 5)
                {
                    return BadRequest("Khách hàng này đã có quá 5 địa chỉ. Không thể thêm mới!");
                }

                // Nếu là địa chỉ đầu tiên, đặt làm mặc định
                if (existingAddresses == 0)
                {
                    diaChi.trangthai = 0;
                }
                else
                {
                    diaChi.trangthai = 1;
                }

                _context.diachis.Add(diaChi);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm địa chỉ thành công!", id = diaChi.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] DiaChi diaChi)
        {
            try
            {
                var existingDiaChi = await _context.diachis.FindAsync(id);
                if (existingDiaChi == null)
                {
                    return NotFound("Không tìm thấy địa chỉ");
                }

                existingDiaChi.Tennguoinhan = diaChi.Tennguoinhan;
                existingDiaChi.sdtnguoinhan = diaChi.sdtnguoinhan;
                existingDiaChi.Thanhpho = diaChi.Thanhpho;
                existingDiaChi.Quanhuyen = diaChi.Quanhuyen;
                existingDiaChi.Phuongxa = diaChi.Phuongxa;
                existingDiaChi.Diachicuthe = diaChi.Diachicuthe;
                existingDiaChi.trangthai = diaChi.trangthai;

                await _context.SaveChangesAsync();

                return Ok("Cập nhật địa chỉ thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var diaChi = await _context.diachis.FindAsync(id);
                if (diaChi == null)
                {
                    return NotFound("Không tìm thấy địa chỉ");
                }

                // Kiểm tra nếu là địa chỉ mặc định
                if (diaChi.trangthai == 0)
                {
                    return BadRequest("Không thể xóa địa chỉ mặc định");
                }

                _context.diachis.Remove(diaChi);
                await _context.SaveChangesAsync();

                return Ok("Xóa địa chỉ thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetDefaultAddress(int idDiaChi, int idKhachHang)
        {
            try
            {
                // Reset tất cả địa chỉ về không mặc định
                var allAddresses = await _context.diachis
                                    .Where(dc => dc.IdKhachHang == idKhachHang)
                                    .ToListAsync();

                foreach (var address in allAddresses)
                {
                    address.trangthai = 1; // Không mặc định
                }

                // Đặt địa chỉ được chọn làm mặc định
                var defaultAddress = allAddresses.FirstOrDefault(dc => dc.Id == idDiaChi);
                if (defaultAddress != null)
                {
                    defaultAddress.trangthai = 0; // Mặc định
                }

                await _context.SaveChangesAsync();

                return Ok("Đã cập nhật địa chỉ mặc định!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        // API methods for GHN - ĐÃ ĐỔI TÊN ĐỂ TRÁNH TRÙNG LẶP
        [HttpGet]
        public async Task<IActionResult> GetProvinces()
        {
            try
            {
                var provinces = await GetProvincesFromGHN();
                return Ok(provinces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetDistricts([FromBody] int provinceId)
        {
            try
            {
                var districts = await GetDistrictsByProvince(provinceId);
                return Ok(districts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetWards([FromBody] int districtId)
        {
            try
            {
                var wards = await GetWardsByDistrict(districtId);
                return Ok(wards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        // Helper methods for GHN API - ĐÃ ĐỔI TÊN
        private async Task<Dictionary<string, string>> GetProvincesFromGHN()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Token", _ghnApiKey);

            var response = await client.PostAsync($"{_ghnApiUrl}/province", null);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GhnResponse<GhnProvince>>(content);

                if (result?.code == 200)
                {
                    return result.data.ToDictionary(p => p.ProvinceID.ToString(), p => p.NameExtension[1]);
                }
            }
            return new Dictionary<string, string>();
        }

        private async Task<Dictionary<string, string>> GetDistrictsFromGHN()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Token", _ghnApiKey);

            var response = await client.PostAsync($"{_ghnApiUrl}/district", null);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GhnResponse<GhnDistrict>>(content);

                if (result?.code == 200)
                {
                    return result.data.ToDictionary(d => d.DistrictID.ToString(), d => d.DistrictName);
                }
            }
            return new Dictionary<string, string>();
        }

        private async Task<Dictionary<string, string>> GetDistrictsByProvince(int provinceId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Token", _ghnApiKey);

            var content = new StringContent(JsonSerializer.Serialize(new { province_id = provinceId }),
                System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_ghnApiUrl}/district", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GhnResponse<GhnDistrict>>(responseContent);

                if (result?.code == 200)
                {
                    return result.data.ToDictionary(d => d.DistrictID.ToString(), d => d.DistrictName);
                }
            }
            return new Dictionary<string, string>();
        }

        private async Task<Dictionary<string, string>> GetWardsByDistrict(int districtId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Token", _ghnApiKey);

            var content = new StringContent(JsonSerializer.Serialize(new { district_id = districtId }),
                System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_ghnApiUrl}/ward", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GhnResponse<GhnWard>>(responseContent);

                if (result?.code == 200)
                {
                    return result.data.ToDictionary(w => w.WardCode, w => w.WardName);
                }
            }
            return new Dictionary<string, string>();
        }

        private async Task<string> GetWardName(string districtId, string wardCode)
        {
            if (int.TryParse(districtId, out int districtIdInt))
            {
                var wards = await GetWardsByDistrict(districtIdInt);
                return wards.ContainsKey(wardCode) ? wards[wardCode] : "Không xác định";
            }
            return "Không xác định";
        }
    }

    // GHN API Models
    public class GhnResponse<T>
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<T> data { get; set; }
    }

    public class GhnProvince
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }
        public List<string> NameExtension { get; set; }
    }

    public class GhnDistrict
    {
        public int DistrictID { get; set; }
        public string DistrictName { get; set; }
    }

    public class GhnWard
    {
        public string WardCode { get; set; }
        public string WardName { get; set; }
    }
}