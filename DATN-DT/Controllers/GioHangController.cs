using DATN_DT.CustomAttribute;
using DATN_DT.Data;
using DATN_DT.Form;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DATN_DT.Controllers
{
    [Route("GioHang")]
    [AuthorizeRoleFromToken("KHACHHANG")]
    public class GioHangController : Controller
    {
        private readonly MyDbContext _context;

        public GioHangController(MyDbContext context)
        {
            _context = context;
        }

        // GET: GioHang/Cart - Hiển thị trang giỏ hàng
        [HttpGet("Cart")]
        public IActionResult Cart()
        {
            return View();
        }

        // API: Lấy thông tin giỏ hàng theo khách hàng
        [HttpGet("GetGioHangByKhachHang")]
        public async Task<IActionResult> GetGioHangByKhachHang()
        {
            try
            {
                // Lấy ID khách hàng từ token (nếu có) hoặc từ session
                var khachHangId = GetCurrentKhachHangId();

                if (khachHangId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập để xem giỏ hàng"
                    });
                }

                var gioHang = await _context.GioHangs
                    .Include(gh => gh.KhachHang)
                    .Include(gh => gh.GioHangChiTiets!)
                        .ThenInclude(ghct => ghct.ModelSanPham)
                            .ThenInclude(msp => msp.SanPham)
                                .ThenInclude(sp => sp.ThuongHieu)
                    .Include(gh => gh.GioHangChiTiets!)
                        .ThenInclude(ghct => ghct.ModelSanPham)
                            .ThenInclude(msp => msp.RAM)
                    .Include(gh => gh.GioHangChiTiets!)
                        .ThenInclude(ghct => ghct.ModelSanPham)
                            .ThenInclude(msp => msp.ROM)
                    .Include(gh => gh.GioHangChiTiets!)
                        .ThenInclude(ghct => ghct.ModelSanPham)
                            .ThenInclude(msp => msp.Pin)
                    .Include(gh => gh.GioHangChiTiets!)
                        .ThenInclude(ghct => ghct.ModelSanPham)
                            .ThenInclude(msp => msp.ManHinh)
                    .Include(gh => gh.GioHangChiTiets!)
                        .ThenInclude(ghct => ghct.ModelSanPham)
                            .ThenInclude(msp => msp.CameraTruoc)
                    .Include(gh => gh.GioHangChiTiets!)
                        .ThenInclude(ghct => ghct.ModelSanPham)
                            .ThenInclude(msp => msp.CameraSau)
                    .Include(gh => gh.GioHangChiTiets!)
                        .ThenInclude(ghct => ghct.ModelSanPham)
                            .ThenInclude(msp => msp.AnhSanPhams)
                    .FirstOrDefaultAsync(gh => gh.IdKhachHang == khachHangId);

                if (gioHang == null)
                {
                    return Ok(new
                    {
                        Success = true,
                        Data = new
                        {
                            IdGioHang = 0,
                            IdKhachHang = khachHangId,
                            KhachHang = new { },
                            GioHangChiTiets = new List<object>(),
                            TongTien = 0
                        },
                        Message = "Giỏ hàng trống"
                    });
                }

                // Format dữ liệu trả về
                var result = new
                {
                    Success = true,
                    Data = new
                    {
                        IdGioHang = gioHang.IdGioHang,
                        IdKhachHang = gioHang.IdKhachHang,
                        KhachHang = new
                        {
                            gioHang.KhachHang.IdKhachHang,
                            gioHang.KhachHang.HoTenKhachHang,
                            gioHang.KhachHang.EmailKhachHang,
                            gioHang.KhachHang.SdtKhachHang
                        },
                        GioHangChiTiets = gioHang.GioHangChiTiets?.Select(ghct => new
                        {
                            IdGioHangChiTiet = ghct.IdGioHangChiTiet,
                            IdGioHang = ghct.IdGioHang,
                            IdModelSanPham = ghct.IdModelSanPham,
                            SoLuong = ghct.SoLuong,
                            ModelSanPham = new
                            {
                                IdModelSanPham = ghct.ModelSanPham.IdModelSanPham,
                                TenModel = ghct.ModelSanPham.TenModel,
                                Mau = ghct.ModelSanPham.Mau,
                                GiaBanModel = ghct.ModelSanPham.GiaBanModel,
                                SanPham = new
                                {
                                    IdSanPham = ghct.ModelSanPham.SanPham?.IdSanPham,
                                    TenSanPham = ghct.ModelSanPham.SanPham?.TenSanPham,
                                    ThuongHieu = new
                                    {
                                        TenThuongHieu = ghct.ModelSanPham.SanPham?.ThuongHieu?.TenThuongHieu
                                    }
                                },
                                RAM = new
                                {
                                    DungLuongRAM = ghct.ModelSanPham.RAM?.DungLuongRAM
                                },
                                ROM = new
                                {
                                    DungLuongROM = ghct.ModelSanPham.ROM?.DungLuongROM
                                },
                                Pin = new
                                {
                                    DungLuongPin = ghct.ModelSanPham.Pin?.DungLuongPin
                                },
                                ManHinh = new
                                {
                                    KichThuoc = ghct.ModelSanPham.ManHinh?.KichThuoc,
                                    DoPhanGiai = ghct.ModelSanPham.ManHinh?.DoPhanGiai
                                },
                                CameraTruoc = new
                                {
                                    DoPhanGiaiCamTruoc = ghct.ModelSanPham.CameraTruoc?.DoPhanGiaiCamTruoc
                                },
                                CameraSau = new
                                {
                                    DoPhanGiaiCamSau = ghct.ModelSanPham.CameraSau?.DoPhanGiaiCamSau
                                },
                                AnhSanPhams = ghct.ModelSanPham.AnhSanPhams?.Select(a => new
                                {
                                    IdAnh = a.IdAnh,
                                    DuongDan = a.DuongDan
                                }).ToList()
                            },
                            ThanhTien = ghct.SoLuong * ghct.ModelSanPham.GiaBanModel
                        }).ToList(),
                        TongTien = gioHang.GioHangChiTiets?.Sum(ghct => ghct.SoLuong * ghct.ModelSanPham.GiaBanModel) ?? 0
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Lỗi server: " + ex.Message
                });
            }
        }

        // API: Cập nhật số lượng sản phẩm
        [HttpPost("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityModel model)
        {
            try
            {
                var cartItem = await _context.GioHangChiTiets
                    .FirstOrDefaultAsync(x => x.IdGioHangChiTiet == model.CartItemId);

                if (cartItem == null)
                    return NotFound(new { Success = false, Message = "Không tìm thấy sản phẩm trong giỏ hàng" });

                cartItem.SoLuong = model.Quantity;
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Đã cập nhật số lượng" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // API: Xóa sản phẩm khỏi giỏ hàng
        [HttpDelete("DeleteCartItem")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            try
            {
                var cartItem = await _context.GioHangChiTiets
                    .FirstOrDefaultAsync(x => x.IdGioHangChiTiet == id);

                if (cartItem == null)
                    return NotFound(new { Success = false, Message = "Không tìm thấy sản phẩm" });

                _context.GioHangChiTiets.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Đã xóa sản phẩm khỏi giỏ hàng" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // Helper: Lấy ID khách hàng hiện tại
        private int? GetCurrentKhachHangId()
        {
            // Lấy từ token JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userId, out int id))
                return id;

            // Hoặc lấy từ session (tuỳ vào implementation của bạn)
            var sessionId = HttpContext.Session.GetInt32("KhachHangId");
            return sessionId;
        }
    }
}