using DATN_DT.Data;
using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Controllers
{
    public class DanhSachSPController : Controller
    {
        private readonly ISanPhamService _sanPhamService;
        private readonly MyDbContext _context;
        
        public DanhSachSPController(ISanPhamService sanPhamService,MyDbContext context)
        {
            _sanPhamService = sanPhamService;
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
           
                var list = _sanPhamService.GetAllSanPhams();
                // Load thương hiệu cho ViewBag
                var thuongHieus =  _context.ThuongHieus
                    .Where(t => t.TrangThaiThuongHieu == "co")
                    .Select(th => new {
                        IdThuongHieu = th.IdThuongHieu,
                        TenThuongHieu = th.TenThuongHieu
                    })
                    .OrderBy(th => th.TenThuongHieu)
                    .ToListAsync();
                ViewBag.ThuongHieus = thuongHieus;
                return View(list);

        }
    }
}
