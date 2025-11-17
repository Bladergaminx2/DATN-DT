using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN_DT.Data;
using DATN_DT.Models;
using System.Security.Claims;
using DATN_DT.DTO;

namespace DATN_DT.Controllers
{
    [Route("GioHang")]
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
                // Lấy ID khách hàng từ JWT token
                var khachHangEmail = GetCurrentKhachHangEmail();

                if (string.IsNullOrEmpty(khachHangEmail))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập để xem giỏ hàng"
                    });
                }

                // Tìm khách hàng theo email từ token
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.EmailKhachHang == khachHangEmail);

                if (khachHang == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng"
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
                    .FirstOrDefaultAsync(gh => gh.IdKhachHang == khachHang.IdKhachHang);

                if (gioHang == null)
                {
                    // Nếu chưa có giỏ hàng, tạo mới
                    gioHang = new GioHang
                    {
                        IdKhachHang = khachHang.IdKhachHang,
                        GioHangChiTiets = new List<GioHangChiTiet>()
                    };
                    _context.GioHangs.Add(gioHang);
                    await _context.SaveChangesAsync();
                }

                // Lấy thông tin tồn kho cho từng model sản phẩm
                var tonKhoData = await _context.TonKhos
                    .Where(tk => gioHang.GioHangChiTiets.Select(ghct => ghct.IdModelSanPham).Contains(tk.IdModelSanPham))
                    .GroupBy(tk => tk.IdModelSanPham)
                    .Select(g => new
                    {
                        IdModelSanPham = g.Key,
                        SoLuongTon = g.Sum(tk => tk.SoLuong)
                    })
                    .ToDictionaryAsync(x => x.IdModelSanPham, x => x.SoLuongTon);

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
                            IdSanPham = ghct.ModelSanPham.SanPham?.IdSanPham,
                            SoLuong = ghct.SoLuong,
                            SoLuongTon = tonKhoData.ContainsKey(ghct.IdModelSanPham.Value) ? tonKhoData[ghct.IdModelSanPham.Value] : 0,
                            ModelSanPham = new
                            {
                                IdModelSanPham = ghct.ModelSanPham.IdModelSanPham,
                                TenModel = ghct.ModelSanPham.TenModel,
                                Mau = ghct.ModelSanPham.Mau,
                                GiaBanModel = ghct.ModelSanPham.GiaBanModel,
                                TrangThai = ghct.ModelSanPham.TrangThai, // Thêm trạng thái
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
        public async Task<IActionResult> UpdateQuantity([FromBody] hoadondtotest.UpdateQuantityModel model)
        {
            try
            {
                var cartItem = await _context.GioHangChiTiets
                    .Include(ghct => ghct.ModelSanPham)
                    .FirstOrDefaultAsync(x => x.IdGioHangChiTiet == model.CartItemId);

                if (cartItem == null)
                    return NotFound(new { Success = false, Message = "Không tìm thấy sản phẩm trong giỏ hàng" });

                // Kiểm tra trạng thái sản phẩm
                if (cartItem.ModelSanPham.TrangThai != 1)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Sản phẩm này đã ngừng kinh doanh"
                    });
                }

                // Kiểm tra số lượng tồn kho
                var tonKho = await _context.TonKhos
                    .Where(tk => tk.IdModelSanPham == cartItem.IdModelSanPham)
                    .SumAsync(tk => tk.SoLuong);

                if (model.Quantity > tonKho)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"Số lượng vượt quá tồn kho. Chỉ còn {tonKho} sản phẩm"
                    });
                }

                if (model.Quantity < 1)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Số lượng phải lớn hơn 0"
                    });
                }

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

        // API: Thêm sản phẩm vào giỏ hàng
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] hoadondtotest.AddToCartModel model)
        {
            try
            {
                var khachHangEmail = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(khachHangEmail))
                {
                    return Unauthorized(new { Success = false, Message = "Vui lòng đăng nhập" });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.EmailKhachHang == khachHangEmail);

                if (khachHang == null)
                {
                    return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin khách hàng" });
                }

                // Tìm hoặc tạo giỏ hàng
                var gioHang = await _context.GioHangs
                    .Include(gh => gh.GioHangChiTiets)
                    .FirstOrDefaultAsync(gh => gh.IdKhachHang == khachHang.IdKhachHang);

                if (gioHang == null)
                {
                    gioHang = new GioHang
                    {
                        IdKhachHang = khachHang.IdKhachHang,
                        GioHangChiTiets = new List<GioHangChiTiet>()
                    };
                    _context.GioHangs.Add(gioHang);
                    await _context.SaveChangesAsync();
                }

                // Kiểm tra model sản phẩm
                var modelSanPham = await _context.ModelSanPhams
                    .FirstOrDefaultAsync(msp => msp.IdModelSanPham == model.IdModelSanPham);

                if (modelSanPham == null)
                {
                    return NotFound(new { Success = false, Message = "Không tìm thấy sản phẩm" });
                }

                if (modelSanPham.TrangThai != 1)
                {
                    return BadRequest(new { Success = false, Message = "Sản phẩm này đã ngừng kinh doanh" });
                }

                // Kiểm tra tồn kho
                var tonKho = await _context.TonKhos
                    .Where(tk => tk.IdModelSanPham == model.IdModelSanPham)
                    .SumAsync(tk => tk.SoLuong);

                if (tonKho <= 0)
                {
                    return BadRequest(new { Success = false, Message = "Sản phẩm đã hết hàng" });
                }

                // Kiểm tra nếu sản phẩm đã có trong giỏ hàng
                var existingItem = gioHang.GioHangChiTiets
                    .FirstOrDefault(ghct => ghct.IdModelSanPham == model.IdModelSanPham);

                if (existingItem != null)
                {
                    // Cập nhật số lượng
                    var newQuantity = existingItem.SoLuong + model.Quantity;
                    if (newQuantity > tonKho)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = $"Số lượng vượt quá tồn kho. Chỉ còn {tonKho} sản phẩm"
                        });
                    }
                    existingItem.SoLuong = newQuantity;
                }
                else
                {
                    // Thêm mới vào giỏ hàng
                    var cartItem = new GioHangChiTiet
                    {
                        IdGioHang = gioHang.IdGioHang,
                        IdModelSanPham = model.IdModelSanPham,
                        SoLuong = model.Quantity
                    };
                    _context.GioHangChiTiets.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Đã thêm sản phẩm vào giỏ hàng",
                    Data = new { IdGioHang = gioHang.IdGioHang }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // Helper: Lấy email khách hàng từ JWT token
        private string GetCurrentKhachHangEmail()
        {
            // Lấy từ JWT token qua HttpContext.User
            var email = User.FindFirstValue(ClaimTypes.Name);
            return email;
        }

        // GET: GioHang/ThanhToan - Trang thanh toán
        [HttpGet("ThanhToan")]
        public async Task<IActionResult> ThanhToan(string selectedItems)
        {
            try
            {
                if (string.IsNullOrEmpty(selectedItems))
                {
                    return RedirectToAction("Cart");
                }

                // Chuyển đổi chuỗi selectedItems thành list IdGioHangChiTiet
                var selectedIds = selectedItems.Split(',')
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Select(id => int.Parse(id))
                    .ToList();

                // Lấy thông tin giỏ hàng chi tiết đã chọn
                var selectedCartItems = await _context.GioHangChiTiets
                    .Include(ghct => ghct.ModelSanPham)
                        .ThenInclude(msp => msp.SanPham)
                            .ThenInclude(sp => sp.ThuongHieu)
                    .Include(ghct => ghct.ModelSanPham)
                        .ThenInclude(msp => msp.RAM)
                    .Include(ghct => ghct.ModelSanPham)
                        .ThenInclude(msp => msp.ROM)
                    .Include(ghct => ghct.ModelSanPham)
                        .ThenInclude(msp => msp.AnhSanPhams) // Đảm bảo include AnhSanPhams
                    .Where(ghct => selectedIds.Contains(ghct.IdGioHangChiTiet))
                    .ToListAsync();

                if (!selectedCartItems.Any())
                {
                    return RedirectToAction("Cart");
                }

                // Tính tổng tiền
                var tongTien = selectedCartItems.Sum(item => item.SoLuong.GetValueOrDefault() * item.ModelSanPham.GiaBanModel.GetValueOrDefault());

                // Tạo ViewModel với kiểu dữ liệu rõ ràng
                var viewModel = new hoadondtotest.ThanhToanViewModel
                {
                    SelectedCartItems = selectedCartItems.Select(item => new hoadondtotest.CartItemViewModel
                    {
                        IdGioHangChiTiet = item.IdGioHangChiTiet,
                        IdModelSanPham = item.IdModelSanPham.Value,
                        SoLuong = item.SoLuong.Value,
                        ThanhTien = item.SoLuong.Value * item.ModelSanPham.GiaBanModel.Value,
                        ModelSanPham = new hoadondtotest.ModelSanPhamViewModel
                        {
                            TenModel = item.ModelSanPham.TenModel,
                            Mau = item.ModelSanPham.Mau,
                            GiaBanModel = item.ModelSanPham.GiaBanModel.Value,
                            SanPham = new hoadondtotest.SanPhamViewModel
                            {
                                TenSanPham = item.ModelSanPham.SanPham?.TenSanPham,
                                ThuongHieu = new hoadondtotest.ThuongHieuViewModel
                                {
                                    TenThuongHieu = item.ModelSanPham.SanPham?.ThuongHieu?.TenThuongHieu
                                }
                            },
                            RAM = new hoadondtotest.RAMViewModel
                            {
                                DungLuongRAM = item.ModelSanPham.RAM?.DungLuongRAM
                            },
                            ROM = new hoadondtotest.ROMViewModel
                            {
                                DungLuongROM = item.ModelSanPham.ROM?.DungLuongROM
                            },
                            AnhSanPhams = item.ModelSanPham.AnhSanPhams?.Select(a => new hoadondtotest.AnhSanPhamViewModel
                            {
                                DuongDan = a.DuongDan
                            }).ToList() ?? new List<hoadondtotest.AnhSanPhamViewModel>()
                        }
                    }).ToList(),
                    TongTien = tongTien,
                    SelectedItemsString = selectedItems
                };

                ViewBag.SelectedItems = selectedItems;
                ViewBag.TongTien = tongTien;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log lỗi và redirect về giỏ hàng
                return RedirectToAction("Cart");
            }
        }

        // API: Xác nhận thanh toán
        [HttpPost("XacNhanThanhToan")]
        public async Task<IActionResult> XacNhanThanhToan([FromBody] ThongTinThanhToanModel model)
        {
            try
            {
                var khachHangEmail = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(khachHangEmail))
                {
                    return Unauthorized(new { Success = false, Message = "Vui lòng đăng nhập" });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.EmailKhachHang == khachHangEmail);

                if (khachHang == null)
                {
                    return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin khách hàng" });
                }

                // Lấy thông tin giỏ hàng chi tiết đã chọn
                var selectedCartItems = await _context.GioHangChiTiets
                    .Include(ghct => ghct.ModelSanPham)
                    .Where(ghct => model.SelectedCartItems.Contains(ghct.IdGioHangChiTiet))
                    .ToListAsync();

                if (!selectedCartItems.Any())
                {
                    return BadRequest(new { Success = false, Message = "Không tìm thấy sản phẩm đã chọn" });
                }

                // Kiểm tra lại trạng thái và số lượng tồn kho
                var errorMessages = new List<string>();

                foreach (var cartItem in selectedCartItems)
                {
                    if (cartItem.ModelSanPham.TrangThai != 1)
                    {
                        errorMessages.Add($"Sản phẩm {cartItem.ModelSanPham.SanPham?.TenSanPham} đã ngừng kinh doanh");
                        continue;
                    }

                    var tonKho = await _context.TonKhos
                        .Where(tk => tk.IdModelSanPham == cartItem.IdModelSanPham)
                        .SumAsync(tk => tk.SoLuong);

                    if (tonKho < cartItem.SoLuong)
                    {
                        errorMessages.Add($"Sản phẩm {cartItem.ModelSanPham.SanPham?.TenSanPham} chỉ còn {tonKho} sản phẩm");
                    }
                }

                if (errorMessages.Any())
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Có lỗi xảy ra với một số sản phẩm:",
                        Errors = errorMessages
                    });
                }

                // Tạo đơn hàng mới
                var donHang = new DonHang
                {
                    IdKhachHang = khachHang.IdKhachHang,
                    MaDon = GenerateOrderCode(),
                    NgayDat = DateTime.Now,
                    DiaChiGiaoHang = model.DiaChi,
                    TrangThaiDH = "Chờ xác nhận",
                    TrangThaiHoaDon = "Chờ thanh toán",
                    PhuongThucThanhToan = model.PhuongThucTT,
                    GhiChu = model.GhiChu,
                    DonHangChiTiets = new List<DonHangChiTiet>()
                };

                // Thêm chi tiết đơn hàng
                foreach (var cartItem in selectedCartItems)
                {
                    var donHangChiTiet = new DonHangChiTiet
                    {
                        IdModelSanPham = cartItem.IdModelSanPham,
                        SoLuong = cartItem.SoLuong,
                        DonGia = cartItem.ModelSanPham.GiaBanModel,
                        ThanhTien = cartItem.SoLuong * cartItem.ModelSanPham.GiaBanModel
                    };
                    donHang.DonHangChiTiets.Add(donHangChiTiet);

                    // Xóa sản phẩm khỏi giỏ hàng
                    _context.GioHangChiTiets.Remove(cartItem);
                }

                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Đặt hàng thành công",
                    Data = new
                    {
                        MaDon = donHang.MaDon,
                        IdDonHang = donHang.IdDonHang
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // Helper: Tạo mã đơn hàng
        private string GenerateOrderCode()
        {
            return "DH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
        }

        public class CheckoutModel
        {
            public List<int> SelectedCartItems { get; set; } = new List<int>();
            public decimal TotalAmount { get; set; }
            public List<CheckoutProduct> Products { get; set; } = new List<CheckoutProduct>();
        }

        public class CheckoutProduct
        {
            public int IdGioHangChiTiet { get; set; }
            public int IdModelSanPham { get; set; }
            public string TenSanPham { get; set; }
            public string TenModel { get; set; }
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
        }

        public class ThongTinThanhToanModel
        {
            public string HoTen { get; set; }
            public string SoDienThoai { get; set; }
            public string Email { get; set; }
            public string DiaChi { get; set; }
            public string GhiChu { get; set; }
            public string PhuongThucTT { get; set; }
            public List<int> SelectedCartItems { get; set; } = new List<int>();
            public decimal TongTien { get; set; }
        }


        
    }

}