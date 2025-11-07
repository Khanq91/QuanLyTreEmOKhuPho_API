using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VanDongController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        public VanDongController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet("vandong/vaolop1")]
        public async Task<IActionResult> GetTreEmVaoLop1()
        {
            var today = DateTime.Today;
            var treList = await _context.TreEms
                .Include(t => t.TreEmHoanCanhs)
                .ThenInclude(h => h.HoanCanh)
                .Include(t => t.VanDongTreEms)
                .Where(t => today.Year - t.NgaySinh.Value.Year == 6)
                .Select(t => new {
                    t.TreEmId,
                    t.HoTen,
                    t.NgaySinh,
                    Tuoi = today.Year - t.NgaySinh.Value.Year,
                    HoanCanh = t.TreEmHoanCanhs
                        .Select(h => h.HoanCanh.LoaiHoanCanh).FirstOrDefault(),
                    LanCuoi = t.VanDongTreEms
                        .OrderByDescending(v => v.NgayVanDong)
                        .Select(v => new {
                            v.SoLan,
                            v.LyDo,
                            v.KetQua,
                            v.NgayVanDong
                        }).FirstOrDefault(),
                    TrangThai = t.VanDongTreEms.Any()
                        ? (t.VanDongTreEms.OrderByDescending(v => v.NgayVanDong)
                            .Select(v => v.KetQua).FirstOrDefault() ?? "Đang vận động")
                        : "Chưa vận động"
                })
                .ToListAsync();

            return Ok(treList);
        }
        [HttpPost("ghinhan")]
        public async Task<IActionResult> GhiNhanVanDong([FromBody] VanDongTreEm model)
        {
            if (model == null || model.TreEmId <= 0)
                return BadRequest("Thiếu dữ liệu trẻ.");

            var tre = await _context.TreEms.FindAsync(model.TreEmId);
            if (tre == null)
                return NotFound("Không tìm thấy trẻ.");

            // Tự động xác định HoanCanhID, Người dùng giả định (Admin)
            model.NgayVanDong = DateOnly.FromDateTime(DateTime.Now);
            model.SoLan = await _context.VanDongTreEms
                            .CountAsync(v => v.TreEmId == model.TreEmId) + 1;
            model.NguoiDungId ??= 1;
            model.HoanCanhId ??= _context.TreEmHoanCanhs
                .Where(h => h.TreEmId == model.TreEmId)
                .Select(h => h.HoanCanhId).FirstOrDefault();

            _context.VanDongTreEms.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã ghi nhận vận động thành công!" });
        }
        [HttpGet("vandong/chitiet/{treEmId}")]
        public async Task<IActionResult> GetChiTietVanDong(int treEmId)
        {
            var tre = await _context.TreEms
                .Include(t => t.VanDongTreEms)
                .ThenInclude(v => v.HoanCanh)
                .FirstOrDefaultAsync(t => t.TreEmId == treEmId);

            if (tre == null)
                return NotFound("Không tìm thấy trẻ.");

            var result = new
            {
                tre.TreEmId,
                tre.HoTen,
                tre.NgaySinh,
                LichSu = tre.VanDongTreEms
                    .OrderByDescending(v => v.NgayVanDong)
                    .Select(v => new
                    {
                        v.VanDongId,
                        v.SoLan,
                        v.LyDo,
                        v.KetQua,
                        v.NgayVanDong,
                        HoanCanh = v.HoanCanh.LoaiHoanCanh
                    })
                    .ToList()
            };

            return Ok(result);
        }

    }
}
