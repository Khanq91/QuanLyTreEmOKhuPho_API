using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.DTOs.ManhThuongQuan;
using QuanLyTreEmAPI.Models;
using System.Drawing;
using System.Globalization;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManhThuongQuanController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        public ManhThuongQuanController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet("ThongKeManhThuongQuan")]
        public async Task<IActionResult> GetThongKeManhThuongQuan()
        {
            var tongSo = await _context.ManhThuongQuans.CountAsync();
            var soToChuc = await _context.ManhThuongQuans.CountAsync(x => x.Loai == "Tổ chức");
            var soCaNhan = await _context.ManhThuongQuans.CountAsync(x => x.Loai == "Cá nhân");

            var tongTienThangNay = await _context.UngHos
                .Where(u => u.NgayUngHo.HasValue &&
                            u.NgayUngHo.Value.Month == DateTime.Now.Month &&
                            u.NgayUngHo.Value.Year == DateTime.Now.Year)
                .SumAsync(u => (decimal?)u.SoTien) ?? 0m;

            var tongTienTongCong = await _context.UngHos
                .SumAsync(u => (decimal?)u.SoTien) ?? 0m;

            var tongNhaTaiTroThuongXuyen = (await _context.UngHos
                .GroupBy(u => u.ManhThuongQuanId)
                .Select(g => new { SoLan = g.Count() })
                .ToListAsync())
                .Count(x => x.SoLan >= 3);

            var result = new ThongKeManhThuongQuanDTO
            {
                TongSoManhThuongQuan = tongSo,
                SoToChuc = soToChuc,
                SoCaNhan = soCaNhan,
                TongTienUngHoThangNay = tongTienThangNay,
                TongTienUngHo = tongTienTongCong,   // <-- trường mới
                TongNhaTaiTroThuongXuyen = tongNhaTaiTroThuongXuyen
            };

            return Ok(result);
        }
        [HttpGet("ThongTinhManhThuongQuan")]
        public async Task<IActionResult> GetThongKeManhThuongQuan_Lambda()
        {
            var result = await _context.ManhThuongQuans
                .GroupJoin(
                    _context.UngHos,
                    mtq => mtq.ManhThuongQuanId,
                    uh => uh.ManhThuongQuanId,
                    (mtq, uhs) => new
                    {
                        mtq.ManhThuongQuanId,
                        mtq.Ten,
                        mtq.Loai,
                        mtq.Sdt,
                        mtq.Email,
                        mtq.DiaChi,
                        TongUngHo = uhs.Sum(x => (decimal?)x.SoTien) ?? 0,
                        SoLanUngHo = uhs.Count()
                    }
                )
                .OrderBy(x => x.ManhThuongQuanId)
                .Select(x => new ThongTinManhThuongQuanDTO
                {
                    ManhThuongQuanId = x.ManhThuongQuanId,
                    Ten = x.Ten,
                    Loai = x.Loai,
                    SDT = x.Sdt,
                    Email = x.Email,
                    DiaChi = x.DiaChi,
                    TongUngHo = x.TongUngHo,
                    SoLanUngHo = x.SoLanUngHo
                })
                .ToListAsync();

            return Ok(result);
        }
        [HttpPost("LuuThongTinUngHo")]
        public async Task<IActionResult> LuuThongTinUngHo([FromBody] ThongTinUngHo dto)
        {
            try
            {
                var ungHo = new UngHo
                {
                    ManhThuongQuanId = dto.ManhThuongQuanId.Value,
                    SoTien = dto.SoTien,
                    NgayUngHo = dto.NgayUngHo,
                    LoaiUngHo = dto.HinhThuc,
                    GhiChu = dto.GhiChu
                };

                _context.UngHos.Add(ungHo);
                await _context.SaveChangesAsync();

                return Ok(new { MessageSuccess = "Lưu thông tin ủng hộ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { MessageError = "Lỗi khi lưu: " + ex.Message });
            }
        }
        [HttpPost("GhiNhanManhThuongQuan")]
        public async Task<IActionResult> GhiNhanManhThuongQuan([FromBody] GhiNhanManhThuongQuan model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }
            var mtq = new ManhThuongQuan
            {
                Ten = model.Ten,
                Loai = model.Loai,
                DiaChi = model.DiaChi,
                Sdt = model.SDT,
                Email = model.Email,
                GhiChu = model.GhiChu
            };

            _context.ManhThuongQuans.Add(mtq);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = " Lưu mạnh thường quân thành công!",
                data = mtq
            });
        }
        [HttpGet("GetThongTinChiTietManhThuongQuan")]
        public async Task<IActionResult> GetThongTinChiTietManhThuongQuan([FromQuery] int ManhThuongQuanID)
        {
            var query = from mtq in _context.ManhThuongQuans
                        join uh in _context.UngHos
                            on mtq.ManhThuongQuanId equals uh.ManhThuongQuanId into ungHoGroup
                        from uh in ungHoGroup.DefaultIfEmpty()
                        where mtq.ManhThuongQuanId == ManhThuongQuanID
                        select new { mtq, uh };

            var result = await query
                .GroupBy(g => new
                {
                    g.mtq.ManhThuongQuanId,
                    g.mtq.Ten,
                    g.mtq.Loai,
                    g.mtq.Sdt,
                    g.mtq.Email,
                    g.mtq.DiaChi,
                    g.mtq.GhiChu
                })
                .Select(g => new ChiTietManhThuongQuan
                {
                    ManhThuongQuanID = g.Key.ManhThuongQuanId,
                    Ten = g.Key.Ten,
                    Loai = g.Key.Loai,
                    SDT = g.Key.Sdt,
                    Email = g.Key.Email,
                    DiaChi = g.Key.DiaChi,
                    GhiChu = g.Key.GhiChu,
                    TongTienUngHo = g.Where(x => x.uh != null)
                                     .Sum(x => (decimal?)x.uh.SoTien) ?? 0m,
                    SoLanUngHo = g.Where(x => x.uh != null)
                                  .Count(x => x.uh.UngHoId != 0),
                    NgayUngHoGanNhat = g
                        .Where(x => x.uh != null && x.uh.NgayUngHo.HasValue)
                        .Max(x => x.uh.NgayUngHo.Value)
                        .ToString("dd/MM/yyyy")
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound(new { Message = "Không tìm thấy Mạnh Thường Quân với ID này." });

            return Ok(result);
        }
        [HttpGet("ThongTinUngHoCuaManhThuongQuan")]
        public async Task<IActionResult> ThongTinUngHoCuaManhThuongQuan([FromQuery] int UngHoID)
        {
            var result = await (from uh in _context.UngHos
                                join mtq in _context.ManhThuongQuans
                                on uh.ManhThuongQuanId equals mtq.ManhThuongQuanId
                                where uh.UngHoId == UngHoID
                                select new UngHoDTO
                                {
                                    UngHoId = uh.UngHoId,
                                    SoTien = uh.SoTien,
                                    LoaiUngHo = uh.LoaiUngHo,
                                    NgayUngHo = uh.NgayUngHo.HasValue ? uh.NgayUngHo.Value.ToString("yyyy-MM-dd") : null,
                                    GhiChu = uh.GhiChu,
                                    TenManhThuongQuan = mtq.Ten,
                                    ManhThuongQuanId = mtq.ManhThuongQuanId,
                                })
                                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound(new { Message = "Không tìm thấy dữ liệu ủng hộ." });

            return Ok(result);
        }

        [HttpPost("SuaUngHoCuaManhThuongQuan")]
        public async Task<IActionResult> SuaUngHoCuaManhThuongQuan([FromBody] UngHoDTO updatedUngHo)
        {
            var ungHo = await _context.UngHos
                                      .FirstOrDefaultAsync(u => u.UngHoId == updatedUngHo.UngHoId);

            if (ungHo == null)
                return NotFound(new { Message = "Không tìm thấy dữ liệu ủng hộ." });

            ungHo.SoTien = updatedUngHo.SoTien ?? 0;

            // Chuyển string sang DateOnly
            if (!string.IsNullOrEmpty(updatedUngHo.NgayUngHo) &&
                DateOnly.TryParse(updatedUngHo.NgayUngHo, out DateOnly parsedDate))
            {
                ungHo.NgayUngHo = parsedDate;
            }

            ungHo.LoaiUngHo = updatedUngHo.LoaiUngHo;
            ungHo.GhiChu = updatedUngHo.GhiChu;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật thành công", Data = ungHo });
        }
        [HttpDelete("XoaUngHo")]
        public async Task<IActionResult> XoaUngHo([FromQuery] int UngHoId)
        {
            var ungHo = await _context.UngHos.FirstOrDefaultAsync(u => u.UngHoId == UngHoId);

            if (ungHo == null)
                return NotFound(new { Message = "Không tìm thấy dữ liệu ủng hộ." });

            _context.UngHos.Remove(ungHo);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("ThongTinManhThuongQuan")]
        public async Task<IActionResult> ThongTinManhThuongQuan([FromQuery] int ManhThuongQuanID)
        {
            var manhThuongQuan = await _context.ManhThuongQuans
                .Where(m => m.ManhThuongQuanId == ManhThuongQuanID)
                .Select(m => new
                {
                    m.ManhThuongQuanId,
                    m.Ten,
                    m.Loai,
                    m.DiaChi,
                    m.Sdt,
                    m.Email,
                    m.GhiChu
                })
                .FirstOrDefaultAsync();

            if (manhThuongQuan == null)
            {
                return NotFound(new { message = "Không tìm thấy mạnh thường quân." });
            }

            // Trả về thông tin mạnh thường quân tìm được
            return Ok(manhThuongQuan);
        }

        [HttpPut("SuaManhThuongQuan")]
        public async Task<IActionResult> SuaManhThuongQuan([FromBody] GhiNhanManhThuongQuan model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }

            var mtq = await _context.ManhThuongQuans
                                    .FirstOrDefaultAsync(m => m.ManhThuongQuanId == model.ManhThuongQuanId);

            if (mtq == null)
            {
                return NotFound(new { message = "Mạnh Thường Quân không tìm thấy." });
            }

            mtq.Ten = model.Ten;
            mtq.Loai = model.Loai;
            mtq.DiaChi = model.DiaChi;
            mtq.Sdt = model.SDT;
            mtq.Email = model.Email;
            mtq.GhiChu = model.GhiChu;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thông tin mạnh thường quân thành công!",
                data = mtq
            });
        }
    }
}
