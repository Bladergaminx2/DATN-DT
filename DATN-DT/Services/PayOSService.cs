using DATN_DT.IServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;
using Net.payOS;
using Net.payOS.Types;

namespace DATN_DT.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PayOSService> _logger;
        private readonly PayOS _payOS;

        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private readonly string _baseUrl;
        private readonly string _webhookUrl;

        public PayOSService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<PayOSService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _clientId = _configuration["PayOS:ClientId"] ?? throw new ArgumentNullException("PayOS:ClientId");
            _apiKey = _configuration["PayOS:ApiKey"] ?? throw new ArgumentNullException("PayOS:ApiKey");
            _checksumKey = _configuration["PayOS:ChecksumKey"] ?? throw new ArgumentNullException("PayOS:ChecksumKey");

            // Khởi tạo PayOS SDK (theo code mẫu)
            _payOS = new PayOS(_clientId, _apiKey, _checksumKey);

            var environment = _configuration["PayOS:Environment"]?.ToLower() ?? "sandbox";
            _baseUrl = environment == "production"
                ? "https://api.payos.vn/v2"
                : "https://api-merchant.payos.vn/v2";

            _webhookUrl = _configuration["PayOS:WebhookUrl"] ?? "";
        }

        public async Task<string> CreatePaymentLinkAsync(
            long orderCode,
            int amount,
            string description,
            string returnUrl,
            string cancelUrl,
            List<DATN_DT.IServices.PayOSItem>? items = null)
        {
            try
            {
                // PayOS yêu cầu orderCode phải là số nguyên dương và unique
                if (orderCode <= 0)
                {
                    throw new ArgumentException("orderCode must be a positive integer", nameof(orderCode));
                }

                // PayOS yêu cầu amount phải > 0 và >= 1000 VND
                if (amount <= 0)
                {
                    throw new ArgumentException("amount must be greater than 0", nameof(amount));
                }
                if (amount < 1000)
                {
                    throw new ArgumentException("amount must be at least 1000 VND", nameof(amount));
                }

                // Validate URLs
                if (string.IsNullOrWhiteSpace(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute))
                {
                    throw new ArgumentException("returnUrl must be a valid absolute URL", nameof(returnUrl));
                }
                if (string.IsNullOrWhiteSpace(cancelUrl) || !Uri.IsWellFormedUriString(cancelUrl, UriKind.Absolute))
                {
                    throw new ArgumentException("cancelUrl must be a valid absolute URL", nameof(cancelUrl));
                }

                // Validate description
                var cleanDescription = description?.Trim() ?? $"Thanh toán đơn hàng #{orderCode}";
                if (cleanDescription.Length > 255)
                {
                    cleanDescription = cleanDescription.Substring(0, 255);
                }

                // Tạo danh sách items từ tham số hoặc tạo mặc định (sử dụng ItemData từ PayOS SDK)
                List<ItemData> itemsList = items != null && items.Any()
                    ? items.Select(item => new ItemData(
                        item.Name?.Trim() ?? "Sản phẩm",
                        item.Quantity > 0 ? item.Quantity : 1,
                        item.Price > 0 ? item.Price : 0
                    )).ToList()
                    : new List<ItemData>
                    {
                        new ItemData(cleanDescription, 1, amount)
                    };

                // Validate: Tổng tiền của items phải bằng amount (PayOS yêu cầu chính xác)
                var totalItemsAmount = itemsList.Sum(item => item.price * item.quantity);
                if (totalItemsAmount != amount)
                {
                    _logger.LogWarning("Items total ({Total}) does not match amount ({Amount}), adjusting...", 
                        totalItemsAmount, amount);
                    
                    // Điều chỉnh item cuối cùng để tổng khớp chính xác
                    if (itemsList.Count > 0)
                    {
                        var lastItem = itemsList[itemsList.Count - 1];
                        var otherItemsTotal = totalItemsAmount - (lastItem.price * lastItem.quantity);
                        var lastItemTotal = amount - otherItemsTotal;
                        
                        // Đảm bảo giá đơn vị là số nguyên
                        var newPrice = lastItemTotal / lastItem.quantity;
                        if (newPrice <= 0)
                        {
                            throw new Exception($"Cannot adjust items: last item price would be {newPrice}");
                        }
                        
                        itemsList[itemsList.Count - 1] = new ItemData(
                            lastItem.name,
                            lastItem.quantity,
                            (int)newPrice
                        );
                        
                        // Verify lại
                        var newTotal = itemsList.Sum(item => item.price * item.quantity);
                        if (newTotal != amount)
                        {
                            _logger.LogError("After adjustment, total ({NewTotal}) still does not match amount ({Amount})", 
                                newTotal, amount);
                        }
                    }
                    else
                    {
                        throw new Exception($"Items total ({totalItemsAmount}) does not match amount ({amount}) and no items to adjust");
                    }
                }

                // Tạo PaymentData theo đúng format PayOS SDK (theo code mẫu)
                PaymentData paymentData = new PaymentData(
                    (int)orderCode,  // PayOS yêu cầu int, không phải long
                    amount,
                    cleanDescription,
                    itemsList,
                    cancelUrl,  // cancelUrl trước
                    returnUrl   // returnUrl sau
                );

                _logger.LogInformation("PayOS CreatePaymentLink request: OrderCode={OrderCode}, Amount={Amount}, Items={ItemsCount}", 
                    orderCode, amount, itemsList.Count);

                // Gọi PayOS SDK để tạo payment link (theo code mẫu)
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                if (createPayment == null || string.IsNullOrEmpty(createPayment.checkoutUrl))
                {
                    throw new Exception("PayOS returned null or empty checkoutUrl");
                }

                _logger.LogInformation("PayOS CreatePaymentLink success: CheckoutUrl={Url}", createPayment.checkoutUrl);

                return createPayment.checkoutUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link");
                throw;
            }
        }

        public bool VerifyWebhookSignature(string webhookBody, string signature)
        {
            try
            {
                // PayOS sử dụng HMAC SHA256 để tạo signature
                // Signature = HMAC_SHA256(webhookBody + checksumKey)
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey));
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(webhookBody));
                var computedSignature = Convert.ToHexString(hashBytes).ToLower();

                // So sánh signature (case-insensitive)
                return string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying webhook signature");
                return false;
            }
        }

        public async Task<PayOSPaymentInfo?> GetPaymentInfoAsync(long orderCode)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
                httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                var response = await httpClient.GetAsync($"{_baseUrl}/payment-requests/{orderCode}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PayOS GetPaymentInfo failed: {StatusCode} - {Content}",
                        response.StatusCode, responseContent);
                    return null;
                }

                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (result.TryGetProperty("data", out var data))
                {
                    // Xử lý orderCode có thể là string hoặc number
                    long parsedOrderCode = orderCode;
                    if (data.TryGetProperty("orderCode", out var oc))
                    {
                        if (oc.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            parsedOrderCode = oc.GetInt64();
                        }
                        else if (oc.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            if (long.TryParse(oc.GetString(), out long parsed))
                            {
                                parsedOrderCode = parsed;
                            }
                        }
                    }
                    
                    // Xử lý amount có thể là string hoặc number
                    int parsedAmount = 0;
                    if (data.TryGetProperty("amount", out var amt))
                    {
                        if (amt.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            parsedAmount = amt.GetInt32();
                        }
                        else if (amt.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            if (int.TryParse(amt.GetString(), out int parsedAmt))
                            {
                                parsedAmount = parsedAmt;
                            }
                        }
                    }
                    
                    return new PayOSPaymentInfo
                    {
                        OrderCode = parsedOrderCode,
                        Amount = parsedAmount,
                        Status = data.TryGetProperty("status", out var status) ? status.GetString() : null,
                        Description = data.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        CreatedAt = data.TryGetProperty("createdAt", out var createdAt) && createdAt.TryGetInt64(out var ca)
                            ? DateTimeOffset.FromUnixTimeSeconds(ca).DateTime
                            : null,
                        PaidAt = data.TryGetProperty("paidAt", out var paidAt) && paidAt.TryGetInt64(out var pa)
                            ? DateTimeOffset.FromUnixTimeSeconds(pa).DateTime
                            : null
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayOS payment info");
                return null;
            }
        }

        public async Task<bool> CancelPaymentLinkAsync(long orderCode)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
                httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                var response = await httpClient.DeleteAsync($"{_baseUrl}/payment-requests/{orderCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling PayOS payment link");
                return false;
            }
        }
    }
}

