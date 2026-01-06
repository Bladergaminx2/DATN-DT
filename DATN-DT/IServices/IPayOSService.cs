namespace DATN_DT.IServices
{
    public interface IPayOSService
    {
        /// <summary>
        /// Tạo payment link từ PayOS
        /// </summary>
        /// <param name="orderCode">Mã đơn hàng (phải là số nguyên dương, unique)</param>
        /// <param name="amount">Số tiền (VND)</param>
        /// <param name="description">Mô tả đơn hàng</param>
        /// <param name="returnUrl">URL redirect sau khi thanh toán thành công</param>
        /// <param name="cancelUrl">URL redirect khi hủy thanh toán</param>
        /// <param name="items">Danh sách sản phẩm (optional)</param>
        /// <returns>Payment link URL</returns>
        Task<string> CreatePaymentLinkAsync(
            long orderCode,
            int amount,
            string description,
            string returnUrl,
            string cancelUrl,
            List<PayOSItem>? items = null);

        /// <summary>
        /// Xác thực webhook signature từ PayOS
        /// </summary>
        /// <param name="webhookBody">Body của webhook request</param>
        /// <param name="signature">Signature từ header</param>
        /// <returns>True nếu signature hợp lệ</returns>
        bool VerifyWebhookSignature(string webhookBody, string signature);

        /// <summary>
        /// Lấy thông tin payment từ PayOS
        /// </summary>
        /// <param name="orderCode">Mã đơn hàng</param>
        /// <returns>Thông tin payment</returns>
        Task<PayOSPaymentInfo?> GetPaymentInfoAsync(long orderCode);

        /// <summary>
        /// Hủy payment link (nếu cần)
        /// </summary>
        /// <param name="orderCode">Mã đơn hàng</param>
        Task<bool> CancelPaymentLinkAsync(long orderCode);
    }

    public class PayOSPaymentInfo
    {
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Description { get; set; }
    }

    public class PayOSItem
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}

