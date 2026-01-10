using DATN_DT.Data;
using DATN_DT.DTO;
using DATN_DT.Models;
using DATN_DT.Services;
using DATN_DT.Services.Ghn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Drawing.Drawing2D;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using DATN_DT.IServices;

namespace DATN_DT.Controllers
{
    [Route("GioHang")]
    public class GioHangController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IGhnClient _ghnClient;
        private readonly IOptions<GhnOptions> _ghnOpt;
        private readonly ITonKhoService _tonKhoService;
        private readonly IModelSanPhamStatusService _statusService;

        public GioHangController(
            MyDbContext context, 
            IGhnClient ghnClient, 
            IOptions<GhnOptions> ghnOpt,
            ITonKhoService tonKhoService,
            IModelSanPhamStatusService statusService)
        {
            _context = context;
            _ghnClient = ghnClient;
            _ghnOpt = ghnOpt;
            _tonKhoService = tonKhoService;
            _statusService = statusService;
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


        // API: Lấy số lượng sản phẩm trong giỏ hàng
        [HttpGet("GetCartCount")]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var khachHangEmail = GetCurrentKhachHangEmail();

                if (string.IsNullOrEmpty(khachHangEmail))
                {
                    // Nếu chưa đăng nhập, trả về 0
                    return Ok(new { Success = true, Count = 0 });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.EmailKhachHang == khachHangEmail);

                if (khachHang == null)
                {
                    return Ok(new { Success = true, Count = 0 });
                }

                var gioHang = await _context.GioHangs
                    .Include(gh => gh.GioHangChiTiets)
                    .FirstOrDefaultAsync(gh => gh.IdKhachHang == khachHang.IdKhachHang);

                if (gioHang == null || gioHang.GioHangChiTiets == null)
                {
                    return Ok(new { Success = true, Count = 0 });
                }

                // Tính tổng số lượng sản phẩm
                var totalCount = gioHang.GioHangChiTiets.Sum(ghct => ghct.SoLuong);

                return Ok(new { Success = true, Count = totalCount });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Count = 0, Message = ex.Message });
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
        public async Task<IActionResult> ThanhToan(string selectedItems, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrEmpty(selectedItems))
                    return RedirectToAction("Cart");

                var khachHangEmail = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(khachHangEmail))
                    return RedirectToAction("Login", "Account"); // MVC nên redirect

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.EmailKhachHang == khachHangEmail, ct);

                if (khachHang == null)
                    return RedirectToAction("Login", "Account");

                var selectedIds = selectedItems.Split(',')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(int.Parse)
                    .ToList();

                var selectedCartItems = await _context.GioHangChiTiets
                    .Include(ghct => ghct.ModelSanPham).ThenInclude(msp => msp.SanPham).ThenInclude(sp => sp.ThuongHieu)
                    .Include(ghct => ghct.ModelSanPham).ThenInclude(msp => msp.RAM)
                    .Include(ghct => ghct.ModelSanPham).ThenInclude(msp => msp.ROM)
                    .Include(ghct => ghct.ModelSanPham).ThenInclude(msp => msp.AnhSanPhams)
                    .Where(ghct => selectedIds.Contains(ghct.IdGioHangChiTiet))
                    .ToListAsync(ct);

                if (!selectedCartItems.Any())
                    return RedirectToAction("Cart");

                var tongTien = selectedCartItems.Sum(item =>
                    item.SoLuong * item.ModelSanPham.GiaBanModel.GetValueOrDefault());

                var diaChiEntity = await _context.diachis
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dc => dc.IdKhachHang == khachHang.IdKhachHang && dc.trangthai == 0, ct);

                DiaChiDTO? diaChiDto = null;
                if (diaChiEntity != null)
                {
                    diaChiDto = new DiaChiDTO
                    {
                        Id = diaChiEntity.Id,
                        IdKhachHang = diaChiEntity.IdKhachHang,
                        Tennguoinhan = diaChiEntity.Tennguoinhan,
                        sdtnguoinhan = diaChiEntity.sdtnguoinhan,
                        Thanhpho = diaChiEntity.Thanhpho,
                        Quanhuyen = diaChiEntity.Quanhuyen,
                        Phuongxa = diaChiEntity.Phuongxa,
                        Diachicuthe = diaChiEntity.Diachicuthe,
                        trangthai = diaChiEntity.trangthai
                    };

                }

                // ===== CODE -> NAME qua GHN CLIENT =====
                if (diaChiDto != null)
                {
                    // Thanhpho = "249" (province_id)  => int
                    // Quanhuyen = "1767" (district_id)=> int
                    // Phuongxa = "190607" (ward_code) => string
                    if (int.TryParse(diaChiDto.Thanhpho?.Trim(), out var provinceId) &&
                        int.TryParse(diaChiDto.Quanhuyen?.Trim(), out var districtId) &&
                        !string.IsNullOrWhiteSpace(diaChiDto.Phuongxa))
                    {
                        try
                        {
                            var nameDto = await _ghnClient.ConvertCodeToNameAsync(
                                provinceId: provinceId,
                                districtId: districtId,
                                wardCode: diaChiDto.Phuongxa.Trim(),
                                ct: ct);

                            diaChiDto.ThanhphoName = nameDto.ProvinceName;
                            diaChiDto.QuanhuyenName = nameDto.DistrictName;
                            diaChiDto.PhuongxaName = nameDto.WardName;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("GHN master-data error: " + ex.Message);
                        }
                    }
                }


                // ====== TÍNH PHÍ VẬN CHUYỂN GHN ======
                int phiVanChuyen = 0;

                if (diaChiDto != null)
                {
                    var parsed = TryParseGhnLocationCodes(diaChiDto);
                    if (parsed.HasValue)
                    {
                        var (wardCode, toDistrictId, toProvinceId) = parsed.Value;

                        var totalQty = selectedCartItems.Sum(x => x.SoLuong);
                        var weightGram = Math.Max(200, totalQty * 300);

                        try
                        {
                            // 1) Lấy danh sách service khả dụng (giống JS)
                            var services = await _ghnClient.GetAvailableServicesAsync(
                                fromDistrictId: _ghnOpt.Value.FromDistrictId,
                                toDistrictId: toDistrictId,
                                ct: ct);

                            if (services.Count > 0)
                            {
                                var selectedServiceId = services[0].service_id;

                                // 2) Tính phí (giống JS)
                                phiVanChuyen = await _ghnClient.CalculateFeeAsync(new DATN_DT.Services.Ghn.GhnFeeRequest
                                {
                                    service_id = selectedServiceId,
                                    insurance_value = (int)Math.Min(tongTien, 50_000_000m),
                                    coupon = null,

                                    to_province_id = toProvinceId,
                                    to_district_id = toDistrictId,
                                    to_ward_code = wardCode,

                                    weight = weightGram,
                                    length = 20,
                                    width = 20,
                                    height = 10,

                                    from_district_id = _ghnOpt.Value.FromDistrictId,
                                    from_ward_code = string.IsNullOrWhiteSpace(_ghnOpt.Value.FromWardCode) ? null : _ghnOpt.Value.FromWardCode
                                }, ct);
                            }
                        }
                        catch (Exception ex)
                        {
                            // không để crash trang thanh toán
                            Console.WriteLine("GHN error: " + ex.Message);
                            phiVanChuyen = 0;
                        }
                    }
                }


                var viewModel = new hoadondtotest.ThanhToanViewModel
                {
                    SelectedCartItems = selectedCartItems.Select(item => new hoadondtotest.CartItemViewModel
                    {
                        IdGioHangChiTiet = item.IdGioHangChiTiet,
                        IdModelSanPham = item.IdModelSanPham!.Value,
                        SoLuong = item.SoLuong,
                        ThanhTien = item.SoLuong * item.ModelSanPham.GiaBanModel!.Value,
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
                            RAM = new hoadondtotest.RAMViewModel { DungLuongRAM = item.ModelSanPham.RAM?.DungLuongRAM },
                            ROM = new hoadondtotest.ROMViewModel { DungLuongROM = item.ModelSanPham.ROM?.DungLuongROM },
                            AnhSanPhams = item.ModelSanPham.AnhSanPhams?.Select(a => new hoadondtotest.AnhSanPhamViewModel
                            {
                                DuongDan = a.DuongDan
                            }).ToList() ?? new List<hoadondtotest.AnhSanPhamViewModel>()
                        }
                    }).ToList(),
                    TongTien = tongTien,
                    SelectedItemsString = selectedItems,
                    DiaChi = diaChiDto!,
                    PhiVanChuyen = phiVanChuyen
                };

                return View(viewModel);
            }
            catch
            {
                return RedirectToAction("Cart");
            }
        }

        // API: Xác nhận thanh toán
        [HttpPost("XacNhanThanhToan")]
        public async Task<IActionResult> XacNhanThanhToan([FromBody] ThongTinThanhToanModel model, CancellationToken ct)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1) Auth + customer
                var khachHangEmail = GetCurrentKhachHangEmail();
                if (string.IsNullOrEmpty(khachHangEmail))
                    return Unauthorized(new { Success = false, Message = "Vui lòng đăng nhập" });

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.EmailKhachHang == khachHangEmail, ct);

                if (khachHang == null)
                    return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin khách hàng" });

                // 2) Load cart items đã chọn (kèm model + sản phẩm để báo lỗi)
                var selectedCartItems = await _context.GioHangChiTiets
                    .Include(ghct => ghct.ModelSanPham)
                        .ThenInclude(ms => ms.SanPham)
                    .Where(ghct => model.SelectedCartItems.Contains(ghct.IdGioHangChiTiet))
                    .ToListAsync(ct);

                if (!selectedCartItems.Any())
                    return BadRequest(new { Success = false, Message = "Không tìm thấy sản phẩm đã chọn" });

                // 3) Validate trạng thái + tồn kho + IMEI đủ
                var errors = new List<string>();

                foreach (var cartItem in selectedCartItems)
                {
                    var modelSp = cartItem.ModelSanPham;
                    if (modelSp == null || modelSp.TrangThai != 1)
                    {
                        errors.Add($"Sản phẩm {modelSp?.SanPham?.TenSanPham} đã ngừng kinh doanh");
                        continue;
                    }

                    // Kiểm tra số lượng tồn kho > 0
                    var tonKho = await _context.TonKhos
                        .Where(tk => tk.IdModelSanPham == cartItem.IdModelSanPham)
                        .SumAsync(tk => tk.SoLuong, ct);

                    // Kiểm tra IMEI còn hàng > 0
                    var availableImei = await _context.Imeis
                        .Where(i => i.IdModelSanPham == cartItem.IdModelSanPham && i.TrangThai == "Còn hàng")
                        .CountAsync(ct);

                    // Kiểm tra cả 2 điều kiện: số lượng > 0 và IMEI > 0
                    if (tonKho <= 0)
                    {
                        errors.Add($"Sản phẩm {modelSp.SanPham?.TenSanPham} đã hết hàng (tồn kho: {tonKho})");
                        continue; // Dừng xử lý sản phẩm này
                    }

                    if (availableImei <= 0)
                    {
                        errors.Add($"Sản phẩm {modelSp.SanPham?.TenSanPham} không còn IMEI khả dụng (số IMEI: {availableImei})");
                        continue; // Dừng xử lý sản phẩm này
                    }

                    // Kiểm tra số lượng đủ để đáp ứng yêu cầu
                    if (tonKho < cartItem.SoLuong)
                    {
                        errors.Add($"Sản phẩm {modelSp.SanPham?.TenSanPham} chỉ còn {tonKho} sản phẩm (yêu cầu: {cartItem.SoLuong})");
                    }

                    if (availableImei < cartItem.SoLuong)
                    {
                        errors.Add($"Sản phẩm {modelSp.SanPham?.TenSanPham} không đủ IMEI khả dụng (còn {availableImei}, yêu cầu: {cartItem.SoLuong})");
                    }
                }

                if (errors.Any())
                {
                    await tx.RollbackAsync(ct);
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không thể tạo đơn hàng. Vui lòng kiểm tra lại:",
                        Errors = errors
                    });
                }

                // 4) Lấy địa chỉ giao hàng (nếu cần lưu; HoaDon hiện chưa có field địa chỉ => bạn có thể bổ sung hoặc bỏ)
                // Nếu bạn muốn lưu địa chỉ vào HoaDon thì hãy thêm field DiaChiGiaoHang vào class HoaDon.
                // Ở đây tôi vẫn lấy để bạn có thể dùng về sau.
                var diaChi = await _context.diachis
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dc => dc.Id == model.DiaChiId && dc.IdKhachHang == khachHang.IdKhachHang, ct);

                if (diaChi == null)
                    return BadRequest(new { Success = false, Message = "Không tìm thấy địa chỉ giao hàng" });

                var diaChiGiaoHangText = diaChi.Diachicuthe ?? string.Empty;

                // (Nếu bạn cần GHN convert giống trước thì gắn lại đoạn convert ở đây)

                // 5) Tạo HoaDon
                // Lấy ID nhân viên nếu có (nếu nhân viên/admin đang xử lý đơn hàng)
                var idNhanVien = GetCurrentNhanVienId();
                
                // Phân biệt COD và chuyển khoản
                var isCOD = model.PhuongThucTT?.ToLower().Contains("cod") == true || 
                           model.PhuongThucTT?.ToLower().Contains("tiền mặt") == true;
                var isBankTransfer = model.PhuongThucTT?.ToLower().Contains("chuyển khoản") == true ||
                                    model.PhuongThucTT?.ToLower().Contains("bank") == true;
                
                var hoaDon = new HoaDon
                {
                    IdKhachHang = khachHang.IdKhachHang,
                    HoTenNguoiNhan = khachHang.HoTenKhachHang,     // hoặc model.HoTen nếu bạn nhập ở FE
                    SdtKhachHang = khachHang.SdtKhachHang,         // hoặc model.SoDienThoai
                    IdNhanVien = idNhanVien,                       // Lưu ID nhân viên bán hàng (null nếu khách tự đặt)
                    TrangThaiHoaDon = "Chờ xác nhận",              // Cả COD và chuyển khoản đều bắt đầu ở "Chờ xác nhận"
                    NgayLapHoaDon = DateTime.Now,
                    PhuongThucThanhToan = model.PhuongThucTT,
                    TongTien = 0m,
                    HoaDonChiTiets = new List<HoaDonChiTiet>()
                };

                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync(ct); // để có IdHoaDon

                decimal tongTien = 0m;
                var modelSanPhamIdsToRefresh = new HashSet<int>();

                // 6) Tạo HoaDonChiTiet theo từng IMEI (SoLuong=1 mỗi dòng)
                foreach (var cartItem in selectedCartItems)
                {
                    var modelSp = cartItem.ModelSanPham!;
                    var qty = cartItem.SoLuong;

                    var donGia = modelSp.GiaBanModel ?? 0m;
                    
                    // Tính giá khuyến mãi nếu có
                    var idModelSanPham = cartItem.IdModelSanPham ?? 0;
                    var giaKhuyenMai = await CalculatePromotionPrice(idModelSanPham, donGia, ct);
                    var finalPrice = giaKhuyenMai ?? donGia; // Nếu có khuyến mãi thì dùng giá khuyến mãi, không thì dùng giá gốc

                    // 🔹 NGHIỆP VỤ: COD trừ IMEI ngay, chuyển khoản chờ thanh toán thành công
                    if (isCOD)
                    {
                        // 6.1 COD: Lấy đủ IMEI còn hàng và set "Đã bán"
                        var imeis = await _context.Imeis
                            .Where(i => i.IdModelSanPham == cartItem.IdModelSanPham && i.TrangThai == "Còn hàng")
                            .OrderBy(i => i.IdImei)
                            .Take(qty)
                            .ToListAsync(ct);

                        if (imeis.Count < qty)
                        {
                            await tx.RollbackAsync(ct);
                            return BadRequest(new
                            {
                                Success = false,
                                Message = $"IMEI thay đổi. Sản phẩm {modelSp.SanPham?.TenSanPham} không đủ IMEI khả dụng."
                            });
                        }

                        // 6.2 COD: Mỗi IMEI = 1 dòng HoaDonChiTiet với IdImei
                        foreach (var imei in imeis)
                        {
                            var ctRow = new HoaDonChiTiet
                            {
                                IdHoaDon = hoaDon.IdHoaDon,
                                IdModelSanPham = cartItem.IdModelSanPham,
                                IdImei = imei.IdImei, // COD: Gán IMEI ngay
                                GiaKhuyenMai = giaKhuyenMai,
                                DonGia = finalPrice,
                                SoLuong = 1,
                                ThanhTien = finalPrice
                            };

                            _context.HoaDonChiTiets.Add(ctRow);

                            // Set IMEI đã bán (COD: Trừ ngay)
                            imei.TrangThai = "Đã bán";

                            tongTien += finalPrice;
                        }

                        // Thêm vào danh sách cần refresh tồn kho
                        if (cartItem.IdModelSanPham.HasValue)
                        {
                            modelSanPhamIdsToRefresh.Add(cartItem.IdModelSanPham.Value);
                        }
                    }
                    else if (isBankTransfer)
                    {
                        // 🔹 CHUYỂN KHOẢN: KHÔNG trừ IMEI và TonKho, chỉ lưu thông tin sản phẩm
                        // Tạo HoaDonChiTiet với IdImei = null (sẽ gán sau khi thanh toán thành công)
                        for (int i = 0; i < qty; i++)
                        {
                            var ctRow = new HoaDonChiTiet
                            {
                                IdHoaDon = hoaDon.IdHoaDon,
                                IdModelSanPham = cartItem.IdModelSanPham,
                                IdImei = null, // Chuyển khoản: Chưa gán IMEI, chờ thanh toán thành công
                                GiaKhuyenMai = giaKhuyenMai,
                                DonGia = finalPrice,
                                SoLuong = 1,
                                ThanhTien = finalPrice
                            };

                            _context.HoaDonChiTiets.Add(ctRow);
                            tongTien += finalPrice;
                        }
                    }
                    else
                    {
                        // Phương thức thanh toán khác: Xử lý như COD (an toàn)
                        var need = qty;
                        var tonKhoRows = await _context.TonKhos
                            .Where(tk => tk.IdModelSanPham == cartItem.IdModelSanPham && tk.SoLuong > 0)
                            .OrderBy(tk => tk.IdTonKho)
                            .ToListAsync(ct);

                        foreach (var row in tonKhoRows)
                        {
                            if (need <= 0) break;
                            var take = Math.Min(row.SoLuong, need);
                            row.SoLuong -= take;
                            need -= take;
                        }

                        if (need > 0)
                        {
                            await tx.RollbackAsync(ct);
                            return BadRequest(new
                            {
                                Success = false,
                                Message = $"Tồn kho thay đổi. Sản phẩm {modelSp.SanPham?.TenSanPham} không đủ số lượng."
                            });
                        }

                        // Trạng thái sẽ được tự động cập nhật bởi service sau khi thanh toán

                        var imeis = await _context.Imeis
                            .Where(i => i.IdModelSanPham == cartItem.IdModelSanPham && i.TrangThai == "Còn hàng")
                            .OrderBy(i => i.IdImei)
                            .Take(qty)
                            .ToListAsync(ct);

                        if (imeis.Count < qty)
                        {
                            await tx.RollbackAsync(ct);
                            return BadRequest(new
                            {
                                Success = false,
                                Message = $"IMEI thay đổi. Sản phẩm {modelSp.SanPham?.TenSanPham} không đủ IMEI khả dụng."
                            });
                        }

                        foreach (var imei in imeis)
                        {
                            var ctRow = new HoaDonChiTiet
                            {
                                IdHoaDon = hoaDon.IdHoaDon,
                                IdModelSanPham = cartItem.IdModelSanPham,
                                IdImei = imei.IdImei,
                                GiaKhuyenMai = giaKhuyenMai,
                                DonGia = finalPrice,
                                SoLuong = 1,
                                ThanhTien = finalPrice
                            };

                            _context.HoaDonChiTiets.Add(ctRow);
                            imei.TrangThai = "Đã bán";
                            tongTien += finalPrice;
                        }
                    }

                    // 6.4 Xóa cart item đã thanh toán
                    _context.GioHangChiTiets.Remove(cartItem);
                }

                // 6.5 Refresh tồn kho và cập nhật trạng thái ModelSanPham cho COD
                if (isCOD && modelSanPhamIdsToRefresh.Any())
                {
                    // Lưu thay đổi IMEI trước
                    await _context.SaveChangesAsync(ct);

                    foreach (var idModelSanPham in modelSanPhamIdsToRefresh)
                    {
                        // Refresh tồn kho và tự động cập nhật trạng thái (service sẽ xử lý)
                        await _tonKhoService.RefreshTonKhoForModel(idModelSanPham);
                    }
                }

                // 7) Xử lý voucher nếu có
                if (model.IdVoucher.HasValue && model.SoTienGiamVoucher > 0)
                {
                    var voucherService = HttpContext.RequestServices.GetRequiredService<IVoucherService>();
                    await voucherService.UseVoucherAsync(
                        model.IdVoucher.Value,
                        khachHang.IdKhachHang,
                        hoaDon.IdHoaDon,
                        model.SoTienGiamVoucher
                    );
                }

                // 8) Update tổng tiền hóa đơn (đã trừ voucher)
                hoaDon.TongTien = tongTien - model.SoTienGiamVoucher;

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // Tạo mã đơn hàng từ IdHoaDon
                var maDon = GenerateOrderCode();

                return Ok(new
                {
                    Success = true,
                    Message = "Đặt hàng thành công",
                    Data = new
                    {
                        IdHoaDon = hoaDon.IdHoaDon,
                        MaDon = maDon, // Tạo mã đơn hàng từ IdHoaDon
                        TongTien = hoaDon.TongTien
                    }
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDiaChiCuaToi(CancellationToken ct)
        {
            var email = GetCurrentKhachHangEmail();
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            var kh = await _context.KhachHangs.FirstOrDefaultAsync(x => x.EmailKhachHang == email, ct);
            if (kh == null)
                return Unauthorized(new { success = false, message = "Không tìm thấy khách hàng" });

            var ds = await _context.diachis
                .AsNoTracking()
                .Where(x => x.IdKhachHang == kh.IdKhachHang)
                // nếu bạn có cột này
                .Select(x => new
                {
                    id = x.Id,
                    tenNguoiNhan = x.Tennguoinhan,
                    sdt = x.sdtnguoinhan,
                    diaChiCuThe = x.Diachicuthe,
                    phuongxaName = x.Phuongxa,
                    quanhuyenName = x.Quanhuyen,
                    thanhphoName = x.Thanhpho,
                    trangthai = x.trangthai
                })
                .ToListAsync(ct);

            return Json(new { success = true, data = ds });
        }



        // Helper method: Lấy ID nhân viên từ JWT token
        private int? GetCurrentNhanVienId()
        {
            try
            {
                var token = Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(token)) return null;

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                var nhanVienIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "IdNhanVien");
                
                if (nhanVienIdClaim != null && int.TryParse(nhanVienIdClaim.Value, out int nhanVienId))
                {
                    return nhanVienId;
                }
            }
            catch { }
            return null;
        }

        // Helper method: Tính giá khuyến mãi cho sản phẩm
        private async Task<decimal?> CalculatePromotionPrice(int idModelSanPham, decimal originalPrice, CancellationToken ct = default)
        {
            try
            {
                var now = DateTime.Now.Date;

                // Tìm khuyến mãi đang hoạt động cho sản phẩm này (bỏ kiểm tra ngày kết thúc để cho phép khuyến mãi hết hạn vẫn hoạt động)
                var activePromotion = await (from mspkm in _context.ModelSanPhamKhuyenMais
                                           join km in _context.KhuyenMais on mspkm.IdKhuyenMai equals km.IdKhuyenMai
                                           where mspkm.IdModelSanPham == idModelSanPham
                                              && km.NgayBatDau.HasValue
                                              && km.NgayBatDau.Value.Date <= now
                                              && (km.TrangThaiKM == "Đang diễn ra" || km.TrangThaiKM == "Đã kết thúc")
                                           orderby km.NgayKetThuc descending // Lấy khuyến mãi gần nhất
                                           select km)
                                          .FirstOrDefaultAsync(ct);

                if (activePromotion == null)
                    return null;

                // Tính giá sau giảm
                decimal discountedPrice = 0;

                if (activePromotion.LoaiGiam == "Phần trăm")
                {
                    var percent = Math.Min(100, Math.Max(0, activePromotion.GiaTri ?? 0));
                    discountedPrice = originalPrice * (1 - percent / 100);
                }
                else if (activePromotion.LoaiGiam == "Số tiền")
                {
                    var discountAmount = Math.Min(originalPrice, Math.Max(0, activePromotion.GiaTri ?? 0));
                    discountedPrice = originalPrice - discountAmount;
                }
                else
                {
                    return null;
                }

                // Làm tròn đến 1000 VNĐ (làm tròn xuống)
                discountedPrice = Math.Floor(discountedPrice / 1000) * 1000;

                // Đảm bảo giá không âm
                discountedPrice = Math.Max(0, discountedPrice);

                return discountedPrice;
            }
            catch
            {
                return null;
            }
        }

        // Helper: Tạo mã đơn hàng
        private string GenerateOrderCode()
        {
            return "DH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
        }

        private (string wardCode, int districtId, int provinceId)? TryParseGhnLocationCodes(DiaChiDTO diaChi)
        {
            // Giả định: diaChi.Phuongxa = "190607", diaChi.Quanhuyen = "1767", diaChi.Thanhpho = "249"
            // Nếu anh đang lưu cả 3 trong 1 field (vd Diachicuthe) thì đổi nguồn parse sang field đó.

            if (diaChi == null) return null;

            var ward = (diaChi.Phuongxa ?? "").Trim();
            var districtStr = (diaChi.Quanhuyen ?? "").Trim();
            var provinceStr = (diaChi.Thanhpho ?? "").Trim();

            if (string.IsNullOrWhiteSpace(ward)) return null;
            if (!int.TryParse(districtStr, out var districtId)) return null;
            if (!int.TryParse(provinceStr, out var provinceId)) return null;

            return (ward, districtId, provinceId);
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
            public string? GhiChu { get; set; }
            public string PhuongThucTT { get; set; }
            public List<int> SelectedCartItems { get; set; } = new();
            public decimal TongTien { get; set; }

            public int DiaChiId { get; set; } // NEW
            public int? IdVoucher { get; set; }
            public decimal SoTienGiamVoucher { get; set; } = 0;
        }

        // Tạo payment link và QR code cho thanh toán chuyển khoản
        [HttpPost("CreatePaymentLink")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkModel model, CancellationToken ct)
        {
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(hd => hd.KhachHang)
                    .FirstOrDefaultAsync(hd => hd.IdHoaDon == model.IdHoaDon, ct);

                if (hoaDon == null)
                    return BadRequest(new { success = false, message = "Không tìm thấy hóa đơn" });

                // Tạo mã đơn hàng
                var maDon = GenerateOrderCode();
                
                // Tạo payment URL (mock - trong thực tế sẽ tích hợp với VNPay, MoMo, etc.)
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var paymentUrl = $"{baseUrl}/GioHang/PaymentCallback?idHoaDon={hoaDon.IdHoaDon}&maDon={maDon}&tongTien={model.TongTien}";
                
                // Tạo QR code từ payment URL (sử dụng API QR code generator)
                var qrCodeApiUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(paymentUrl)}";
                
                return Ok(new
                {
                    success = true,
                    paymentUrl = paymentUrl,
                    qrCodeUrl = qrCodeApiUrl,
                    maDon = maDon,
                    tongTien = model.TongTien
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi tạo payment link: " + ex.Message });
            }
        }

        // Kiểm tra trạng thái thanh toán
        [HttpGet("CheckPaymentStatus")]
        public async Task<IActionResult> CheckPaymentStatus(int idHoaDon, CancellationToken ct)
        {
            try
            {
                var hoaDon = await _context.HoaDons
                    .FirstOrDefaultAsync(hd => hd.IdHoaDon == idHoaDon, ct);

                if (hoaDon == null)
                    return BadRequest(new { success = false, message = "Không tìm thấy hóa đơn" });

                var isPaid = hoaDon.TrangThaiHoaDon == "Đã thanh toán" || hoaDon.TrangThaiHoaDon == "Đang giao hàng";
                var maDon = GenerateOrderCode();

                return Ok(new
                {
                    success = true,
                    isPaid = isPaid,
                    maDon = maDon,
                    trangThai = hoaDon.TrangThaiHoaDon
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi kiểm tra trạng thái: " + ex.Message });
            }
        }

        // Gửi email hóa đơn
        private async Task SendInvoiceEmail(HoaDon hoaDon, CancellationToken ct)
        {
            try
            {
                var khachHang = hoaDon.KhachHang;
                if (khachHang == null || string.IsNullOrEmpty(khachHang.EmailKhachHang))
                    return;

                // Tạo nội dung email HTML
                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .invoice-details {{ background: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .product-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        .product-table th, .product-table td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
        .product-table th {{ background: #667eea; color: white; }}
        .total {{ text-align: right; font-size: 18px; font-weight: bold; color: #dc3545; margin-top: 15px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>HÓA ĐƠN ĐIỆN TỬ</h1>
            <p>Tech Phone Store</p>
        </div>
        <div class='content'>
            <h2>Cảm ơn bạn đã mua hàng!</h2>
            <div class='invoice-details'>
                <p><strong>Mã đơn hàng:</strong> {GenerateOrderCode()}</p>
                <p><strong>Ngày đặt:</strong> {hoaDon.NgayLapHoaDon:dd/MM/yyyy HH:mm}</p>
                <p><strong>Ngày thanh toán:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                <p><strong>Khách hàng:</strong> {khachHang.HoTenKhachHang}</p>
                <p><strong>Email:</strong> {khachHang.EmailKhachHang}</p>
                <p><strong>Số điện thoại:</strong> {khachHang.SdtKhachHang}</p>
                <p><strong>Phương thức thanh toán:</strong> {hoaDon.PhuongThucThanhToan}</p>
            </div>
            <h3>Chi tiết đơn hàng:</h3>
            <table class='product-table'>
                <thead>
                    <tr>
                        <th>Sản phẩm</th>
                        <th>Số lượng</th>
                        <th>Đơn giá</th>
                        <th>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>";

                foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                {
                    var modelSp = chiTiet.ModelSanPham;
                    var tenSp = modelSp?.SanPham?.TenSanPham ?? "N/A";
                    emailBody += $@"
                    <tr>
                        <td>{tenSp}</td>
                        <td>{chiTiet.SoLuong}</td>
                        <td>{chiTiet.DonGia:N0} ₫</td>
                        <td>{chiTiet.ThanhTien:N0} ₫</td>
                    </tr>";
                }

                emailBody += $@"
                </tbody>
            </table>
            <div class='total'>
                <p>Tổng cộng: {hoaDon.TongTien:N0} ₫</p>
            </div>
            <p>Đơn hàng của bạn đang được xử lý. Chúng tôi sẽ giao hàng đến bạn trong thời gian sớm nhất.</p>
        </div>
        <div class='footer'>
            <p>Tech Phone Store - Điện thoại công nghệ cao</p>
            <p>Hotline: 1900 1000 | Email: support@techphone.com</p>
        </div>
    </div>
</body>
</html>";

                // TODO: Tích hợp với email service thực tế (SendGrid, SMTP, etc.)
                // Ở đây tôi sẽ tạo một service đơn giản để gửi email
                // Bạn cần cài đặt MailKit hoặc System.Net.Mail
                
                // Mock: Log email để test
                Console.WriteLine($"=== EMAIL INVOICE ===");
                Console.WriteLine($"To: {khachHang.EmailKhachHang}");
                Console.WriteLine($"Subject: Hóa đơn điện tử - Đơn hàng {GenerateOrderCode()}");
                Console.WriteLine($"Body: {emailBody}");
                Console.WriteLine($"======================");

                // Trong production, uncomment và sử dụng email service thực tế:
                // await _emailService.SendEmailAsync(
                //     khachHang.EmailKhachHang,
                //     $""Hóa đơn điện tử - Đơn hàng {GenerateOrderCode()}"",
                //     emailBody
                // );

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendInvoiceEmail: {ex.Message}");
                throw;
            }
        }

        // Model cho CreatePaymentLink
        public class CreatePaymentLinkModel
        {
            public int IdHoaDon { get; set; }
            public decimal TongTien { get; set; }
        }

    }

}