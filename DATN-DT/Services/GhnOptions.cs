using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace DATN_DT.Services.Ghn
{
    public sealed class GhnOptions
    {
        // Token GHN (anh đang dùng)
        public string Token { get; set; } = "7b4f1e5c-0700-11f0-94b6-be01e07a48b5";

        // Base GHN
        public string BaseUrl { get; set; } = "https://online-gateway.ghn.vn";

        // ShopId: JS anh đang dùng shop_id=3846066
        public int ShopId { get; set; } = 3846066;

        // Địa chỉ người gửi (shop)
        public int FromDistrictId { get; set; } = 3440;
        public string FromWardCode { get; set; } = ""; // nếu anh có thì điền

        // Optional: kích thước/khối lượng mặc định nếu chưa có dữ liệu sản phẩm
        public int DefaultWeightGram { get; set; } = 300;
        public int DefaultLengthCm { get; set; } = 20;
        public int DefaultWidthCm { get; set; } = 20;
        public int DefaultHeightCm { get; set; } = 10;
    }

    public sealed class GhnMasterResponse<T>
    {
        public int code { get; set; }
        public string message { get; set; } = "";
        public List<T>? data { get; set; }
    }

    public sealed class GhnProvince
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = "";
        public List<string>? NameExtension { get; set; }
    }

    public sealed class GhnDistrict
    {
        public int DistrictID { get; set; }
        public string DistrictName { get; set; } = "";
        public int ProvinceID { get; set; }
    }

    public sealed class GhnWard
    {
        public string WardCode { get; set; } = "";
        public string WardName { get; set; } = "";
        public int DistrictID { get; set; }
    }

    public sealed class GhnLocationNameResult
    {
        public string? ProvinceName { get; set; }
        public string? DistrictName { get; set; }
        public string? WardName { get; set; }
    }


    public record GhnServiceItem(int service_id, int service_type_id, string short_name);

    public sealed class GhnAvailableServicesResponse
    {
        public int code { get; set; }
        public string message { get; set; } = "";
        public List<GhnServiceItem>? data { get; set; }
    }

    public sealed class GhnFeeRequest
    {
        // theo JS của anh
        public int service_id { get; set; }
        public int insurance_value { get; set; }
        public string? coupon { get; set; }

        public int to_province_id { get; set; }
        public int to_district_id { get; set; }
        public string to_ward_code { get; set; } = "";

        public int weight { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public int from_district_id { get; set; }
        // from_ward_code: có thể cần trong một số shop/tuyến; nếu anh có thì truyền
        public string? from_ward_code { get; set; }
    }

    public sealed class GhnFeeData
    {
        public int total { get; set; }
    }

    public sealed class GhnFeeResponse
    {
        public int code { get; set; }
        public string message { get; set; } = "";
        public GhnFeeData? data { get; set; }
    }

    public interface IGhnClient
    {
        Task<List<GhnServiceItem>> GetAvailableServicesAsync(int fromDistrictId, int toDistrictId, CancellationToken ct);
        Task<int> CalculateFeeAsync(GhnFeeRequest req, CancellationToken ct);

        // master-data
        Task<GhnLocationNameResult> ConvertCodeToNameAsync(int provinceId, int districtId, string wardCode, CancellationToken ct);
    }

    public sealed class GhnClient : IGhnClient
    {
        private readonly HttpClient _http;
        private readonly GhnOptions _opt;
        private static readonly JsonSerializerOptions JsonOpt = new(JsonSerializerDefaults.Web);

        public GhnClient(HttpClient http, IOptions<GhnOptions> opt)
        {
            _http = http;
            _opt = opt.Value;

            _http.BaseAddress = new Uri(_opt.BaseUrl);

            // Token (bắt buộc cho master-data + shipping)
            if (_http.DefaultRequestHeaders.Contains("Token"))
                _http.DefaultRequestHeaders.Remove("Token");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Token", (_opt.Token ?? "").Trim());

            // ShopId header: thường chỉ cần cho shipping, nhưng để sẵn cũng không sao
            if (_http.DefaultRequestHeaders.Contains("ShopId"))
                _http.DefaultRequestHeaders.Remove("ShopId");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("ShopId", _opt.ShopId.ToString());
        }

        public async Task<List<GhnServiceItem>> GetAvailableServicesAsync(int fromDistrictId, int toDistrictId, CancellationToken ct)
        {
            var body = new
            {
                shop_id = _opt.ShopId,
                from_district = fromDistrictId,
                to_district = toDistrictId
            };

            using var res = await _http.PostAsJsonAsync(
                "/shiip/public-api/v2/shipping-order/available-services",
                body, ct);

            var raw = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"GHN available-services {(int)res.StatusCode} {res.ReasonPhrase}. Body: {raw}");

            var dto = JsonSerializer.Deserialize<GhnAvailableServicesResponse>(raw, JsonOpt);
            if (dto == null || dto.code != 200 || dto.data == null) return new List<GhnServiceItem>();
            return dto.data;
        }

        public async Task<int> CalculateFeeAsync(GhnFeeRequest req, CancellationToken ct)
        {
            using var res = await _http.PostAsJsonAsync(
                "/shiip/public-api/v2/shipping-order/fee",
                req, ct);

            var raw = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"GHN fee {(int)res.StatusCode} {res.ReasonPhrase}. Body: {raw}");

            var dto = JsonSerializer.Deserialize<GhnFeeResponse>(raw, JsonOpt);
            if (dto == null || dto.code != 200 || dto.data == null) return 0;
            return dto.data.total;
        }

        // =========================
        // MASTER-DATA (CODE -> NAME)
        // =========================

        public async Task<GhnLocationNameResult> ConvertCodeToNameAsync(int provinceId, int districtId, string wardCode, CancellationToken ct)
        {
            // 1) Province name
            var provinceName = await GetProvinceNameAsync(provinceId, ct);

            // 2) District name
            var districtName = await GetDistrictNameAsync(districtId, ct);

            // 3) Ward name (cần district_id)
            var wardName = await GetWardNameAsync(districtId, wardCode, ct);

            return new GhnLocationNameResult
            {
                ProvinceName = provinceName,
                DistrictName = districtName,
                WardName = wardName
            };
        }

        private async Task<string?> GetProvinceNameAsync(int provinceId, CancellationToken ct)
        {
            using var res = await _http.GetAsync("/shiip/public-api/master-data/province", ct);
            var raw = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode) return null;

            var dto = JsonSerializer.Deserialize<GhnMasterResponse<GhnProvince>>(raw, JsonOpt);
            if (dto?.code != 200 || dto.data == null) return null;

            var p = dto.data.FirstOrDefault(x => x.ProvinceID == provinceId);
            if (p == null) return null;

            // Ưu tiên ProvinceName; nếu muốn theo NameExtension giống code cũ của anh thì dùng NameExtension
            return !string.IsNullOrWhiteSpace(p.ProvinceName)
                ? p.ProvinceName
                : p.NameExtension?.FirstOrDefault();
        }

        private async Task<string?> GetDistrictNameAsync(int districtId, CancellationToken ct)
        {
            // GHN district endpoint có thể GET /district hoặc POST lọc theo province_id.
            // Ở đây dùng GET tất cả cho đơn giản (đúng như anh đang dùng ở controller).
            using var res = await _http.GetAsync("/shiip/public-api/master-data/district", ct);
            var raw = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode) return null;

            var dto = JsonSerializer.Deserialize<GhnMasterResponse<GhnDistrict>>(raw, JsonOpt);
            if (dto?.code != 200 || dto.data == null) return null;

            return dto.data.FirstOrDefault(x => x.DistrictID == districtId)?.DistrictName;
        }

        private async Task<string?> GetWardNameAsync(int districtId, string wardCode, CancellationToken ct)
        {
            // ward cần district_id -> GHN thường dùng querystring: ?district_id=xxx
            // Nếu GHN bên anh yêu cầu POST thì đổi sang PostAsJsonAsync tương tự controller cũ.
            using var res = await _http.GetAsync($"/shiip/public-api/master-data/ward?district_id={districtId}", ct);
            var raw = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode) return null;

            var dto = JsonSerializer.Deserialize<GhnMasterResponse<GhnWard>>(raw, JsonOpt);
            if (dto?.code != 200 || dto.data == null) return null;

            return dto.data.FirstOrDefault(x => x.WardCode == wardCode)?.WardName;
        }
    }
}

