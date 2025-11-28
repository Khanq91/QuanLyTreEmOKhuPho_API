using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ManhThuongQuanController(QuanLyTreEmContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;

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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!DateOnly.TryParseExact(dto.NgayUngHo, "yyyy-MM-dd", out DateOnly ngayUngHo))
                {
                    return BadRequest(new { MessageError = "Định dạng ngày không hợp lệ" });
                }

                // 1. Lưu UngHo
                var ungHo = new UngHo
                {
                    ManhThuongQuanId = dto.ManhThuongQuanId.Value,
                    SoTien = dto.SoTien,
                    SoLuongVatPham = dto.SoLuongVatPham,
                    SoLuongConLai = dto.SoLuongVatPham,
                    NgayUngHo = ngayUngHo,
                    DoiTuong = dto.DoiTuong,
                    TenVatPham = dto.TenVatPham,
                    LoaiUngHo = dto.LoaiUngHo,
                    GhiChu = dto.GhiChu
                };

                _context.UngHos.Add(ungHo);
                await _context.SaveChangesAsync();

                // 2. Xử lý từng file với loại minh chứng riêng
                if (dto.Files != null && dto.Files.Count > 0)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "MinhChung");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    foreach (var fileDto in dto.Files)
                    {
                        if (string.IsNullOrWhiteSpace(fileDto.FileData))
                            continue;

                        try
                        {
                            byte[] fileBytes = Convert.FromBase64String(fileDto.FileData);
                            string fileExtension = Path.GetExtension(fileDto.FileName);
                            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileDto.FileName);
                            string fileName = $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}{fileExtension}";
                            string filePath = Path.Combine(uploadFolder, fileName);

                            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                            // 3. Lưu PhieuMinhChung cho từng file
                            var phieuMinhChung = new PhieuMinhChung
                            {
                                UngHoId = ungHo.UngHoId,
                                FilePath = $"/MinhChung/{fileName}",
                                LoaiMinhChung = fileDto.LoaiMinhChung, // Lấy loại từ file DTO
                                NgayCap = DateOnly.FromDateTime(DateTime.Now)
                            };

                            _context.PhieuMinhChungs.Add(phieuMinhChung);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error saving file {fileDto.FileName}: {ex.Message}");
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { MessageSuccess = "Lưu thông tin ủng hộ thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
                        join pmc in _context.PhieuMinhChungs
                      on uh.UngHoId equals pmc.UngHoId into phieuGroup
                            from pmc in phieuGroup.DefaultIfEmpty()
                        where mtq.ManhThuongQuanId == ManhThuongQuanID
                        select new { mtq, uh,pmc };

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
                        .ToString("dd/MM/yyyy"),
                    PhieuMinhChung = g
                    .Where(x => x.pmc != null)
                    .Select(x => new PhieuMinhChungDto
                    {
                        PhieuMinhChungID = x.pmc.MinhChungId,
                        LoaiMinhChung = x.pmc.LoaiMinhChung,
                        FilePath = x.pmc.FilePath,
                        NgayCap = x.pmc.NgayCap.HasValue
                        ? x.pmc.NgayCap.Value.ToDateTime(TimeOnly.MinValue)
                        : null
                    })
                    .ToList()

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
                                    SoLuongVatPham = uh.SoLuongVatPham,
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

            // ====== XỬ LÝ SỐ LƯỢNG VẬT PHẨM ======
            if (updatedUngHo.SoLuongVatPham.HasValue)
            {
                int oldValue = ungHo.SoLuongVatPham ?? 0;
                int newValue = updatedUngHo.SoLuongVatPham.Value;

                if (newValue != oldValue)
                {
                    int diff = newValue - oldValue;

                    // Nếu tăng => SoLuongConLai tăng theo
                    // Nếu giảm => SoLuongConLai giảm theo
                    ungHo.SoLuongConLai += diff;

                    // Đảm bảo không âm
                    if (ungHo.SoLuongConLai < 0)
                        ungHo.SoLuongConLai = 0;

                    // Gán giá trị mới
                    ungHo.SoLuongVatPham = newValue;
                }
            }

            // ====== CẬP NHẬT SỐ TIỀN ======
            ungHo.SoTien = updatedUngHo.SoTien ?? 0;

            // ====== CẬP NHẬT NGÀY ======
            if (!string.IsNullOrEmpty(updatedUngHo.NgayUngHo) &&
                DateOnly.TryParse(updatedUngHo.NgayUngHo, out DateOnly parsedDate))
            {
                ungHo.NgayUngHo = parsedDate;
            }

            // ====== CẬP NHẬT LOẠI & GHI CHÚ ======
            ungHo.LoaiUngHo = updatedUngHo.LoaiUngHo;
            ungHo.GhiChu = updatedUngHo.GhiChu;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật thành công", Data = ungHo });
        }

        [HttpDelete("XoaUngHo")]
        public async Task<IActionResult> XoaUngHo([FromQuery] int UngHoId)
        {
            // Lấy bản ghi UngHo cùng các bảng liên quan
            var ungHo = await _context.UngHos
                .Include(u => u.PhanBoUngHoChiPhis)
                .Include(u => u.QuaTangUngHos)
                    .ThenInclude(q => q.PhanPhatQuas)
                .Include(u => u.PhieuMinhChungs)  // ✅ THÊM PhieuMinhChung
                .Include(u => u.TreEms)            // ✅ THÊM TreEms (many-to-many)
                .FirstOrDefaultAsync(u => u.UngHoId == UngHoId);

            if (ungHo == null)
                return NotFound("Không tìm thấy bản ghi Ủng hộ.");

            // 1. Xóa PhanPhatQua (cascade từ QuaTangUngHo)
            foreach (var qua in ungHo.QuaTangUngHos)
            {
                if (qua.PhanPhatQuas.Any())
                    _context.PhanPhatQuas.RemoveRange(qua.PhanPhatQuas);
            }

            // 2. Xóa QuaTangUngHo
            if (ungHo.QuaTangUngHos.Any())
                _context.QuaTangUngHos.RemoveRange(ungHo.QuaTangUngHos);

            // 3. Xóa PhanBoUngHoChiPhi
            if (ungHo.PhanBoUngHoChiPhis.Any())
                _context.PhanBoUngHoChiPhis.RemoveRange(ungHo.PhanBoUngHoChiPhis);

            // 4. ✅ Xóa PhieuMinhChung
            if (ungHo.PhieuMinhChungs.Any())
                _context.PhieuMinhChungs.RemoveRange(ungHo.PhieuMinhChungs);

            // 5. ✅ Xóa liên kết UngHo_TreEm (many-to-many, chỉ xóa liên kết)
            ungHo.TreEms.Clear();

            // 6. Xóa bản ghi cha UngHo
            _context.UngHos.Remove(ungHo);

            await _context.SaveChangesAsync();

            return Ok("Xóa thành công.");
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
