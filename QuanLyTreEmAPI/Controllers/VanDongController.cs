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
        public async Task<IActionResult> GhiNhanVanDong([FromBody] GhiNhanVanDongDTO input)
        {
            if (input == null || input.TreEmId <= 0)
                return BadRequest("Thiếu dữ liệu trẻ.");

            if (input.UserID <= 0)
                return BadRequest("Thiếu thông tin người vận động.");

            var tre = await _context.TreEms.FindAsync(input.TreEmId);
            if (tre == null)
                return NotFound("Không tìm thấy trẻ.");

            var nguoiVanDong = await _context.NguoiDungs
                .FirstOrDefaultAsync(x => x.UserId == input.UserID);
            if (nguoiVanDong == null)
                return BadRequest("Người vận động không hợp lệ.");

            // Lấy hoàn cảnh mới nhất của trẻ
            var hoanCanhId = await _context.TreEmHoanCanhs
                .Where(h => h.TreEmId == input.TreEmId)
                .OrderByDescending(h => h.NgayCapNhat)
                .Select(h => h.HoanCanhId)
                .FirstOrDefaultAsync();

            // Đếm số lần vận động của trẻ này
            var soLan = await _context.VanDongTreEms
                .CountAsync(v => v.TreEmId == input.TreEmId) + 1;

            var vd = new VanDongTreEm
            {
                TreEmId = input.TreEmId,
                HoanCanhId = hoanCanhId > 0 ? hoanCanhId : (int?)null,
                NguoiDungId = input.UserID, // ← QUAN TRỌNG: Gán đúng UserID
                SoLan = soLan,
                LyDo = input.LyDo,
                KetQua = input.KetQua,
                NgayVanDong = DateOnly.FromDateTime(DateTime.Now),
                TinhTrangCapNhat = "Mới ghi nhận"
            };

            _context.VanDongTreEms.Add(vd);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ghi nhận vận động thành công!",
                vanDongId = vd.VanDongId,
                soLan = soLan
            });
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
        public class GhiNhanVanDongDTO
        {
            public int TreEmId { get; set; }
            public int UserID { get; set; }
            public string? LyDo { get; set; }
            public string? KetQua { get; set; }
        }

    }
}
