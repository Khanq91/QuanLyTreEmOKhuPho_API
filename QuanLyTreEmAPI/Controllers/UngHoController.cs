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
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var Unghos = await _context.UngHos.ToListAsync();
            return Ok(Unghos);
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
            var today = DateTime.Today;
            var firstDay = new DateTime(today.Year, today.Month, 1);

            var tongTien = await _context.UngHos
                .Where(u => u.NgayUngHo.HasValue
                            && u.NgayUngHo.Value.ToDateTime(TimeOnly.MinValue) >= firstDay
                            && u.NgayUngHo.Value.ToDateTime(TimeOnly.MinValue) <= today)
                .SumAsync(u => (double?)u.SoTien) ?? 0;

            return Ok(tongTien);
        }

    }
}
