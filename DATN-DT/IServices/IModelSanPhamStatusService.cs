using System.Threading.Tasks;

namespace DATN_DT.IServices
{
    /// <summary>
    /// Service tự động cập nhật trạng thái ModelSanPham dựa trên số lượng tồn kho
    /// </summary>
    public interface IModelSanPhamStatusService
    {
        /// <summary>
        /// Tự động cập nhật trạng thái cho một ModelSanPham dựa trên số lượng tồn kho
        /// </summary>
        /// <param name="idModelSanPham">ID của ModelSanPham cần cập nhật</param>
        /// <returns>Số lượng sản phẩm đã được cập nhật (0 hoặc 1)</returns>
        Task<int> UpdateStatusBasedOnStock(int idModelSanPham);

        /// <summary>
        /// Tự động cập nhật trạng thái cho tất cả ModelSanPham
        /// </summary>
        /// <returns>Số lượng sản phẩm đã được cập nhật</returns>
        Task<int> UpdateAllStatusesBasedOnStock();
    }
}

