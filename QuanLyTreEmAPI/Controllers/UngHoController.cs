using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UngHoController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;

        public UngHoController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int? ManhThuongQuanID)
        {
            IQueryable<UngHo> query = _context.UngHos;

            if (ManhThuongQuanID.HasValue)
            {
                query = query.Where(u => u.ManhThuongQuanId == ManhThuongQuanID.Value);
            }

            var ungHos = await query.ToListAsync();

            if (ungHos == null || !ungHos.Any())
            {
                return NotFound(new { Message = "Không tìm thấy dữ liệu ủng hộ." });
            }

            return Ok(ungHos);
        }

        [HttpGet("TongSuKienSapToi")]
        public async Task<IActionResult> GetSuKienSapToi([FromQuery] int? KhuPhoID)
        {
            var allSuKien = await _context.SuKiens.ToListAsync();

            var suKienSapToi = allSuKien
                .Where(s => s.NgayBatDau.HasValue && s.NgayBatDau.Value > DateOnly.FromDateTime(DateTime.Now)
                            && (KhuPhoID == null || s.KhuPhoId == KhuPhoID))
                .OrderBy(s => s.NgayBatDau)
                .Take(3)
                .ToList();
            return Ok(allSuKien);
        }
        [HttpGet("TongTienUngHoTrongThang")]
        public async Task<ActionResult<double>> TongTienTrongThang()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var firstDay = new DateOnly(today.Year, today.Month, 1);

            var ungHos = await _context.UngHos
                .Where(u => u.NgayUngHo.HasValue)
                .ToListAsync();

            var tongTien = ungHos
                .Where(u => u.NgayUngHo >= firstDay && u.NgayUngHo <= today)
                .Sum(u => (double?)u.SoTien) ?? 0;

            return Ok(tongTien);
        } 


    }
}
