using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTreEm.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaoCaoThongKeController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;

        public BaoCaoThongKeController(QuanLyTreEmContext context)
        {
            _context = context;
        }

        // Helper method để convert DateOnly sang DateTime
        private DateTime ConvertToDateTime(DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }

        // GET: api/BaoCaoThongKe/TongQuan
        [HttpGet("TongQuan")]
        public async Task<IActionResult> GetTongQuan()
        {
            try
            {
                var tongSoSuKien = await _context.SuKiens.CountAsync();
                var treEmDuocHoTro = await _context.TreEms.CountAsync();
                var tongTienUngHo = await _context.UngHos
                    .Where(u => u.LoaiUngHo == "Tiền mặt")
                    .SumAsync(u => u.SoTien ?? 0);
                var hienVatUngHo = await _context.UngHos
                    .Where(u => u.LoaiUngHo != "Tiền mặt")
                    .CountAsync();
                var tongTinhNguyenVien = await _context.TinhNguyenViens.CountAsync();

                // Giả định tính giờ tình nguyện (có thể điều chỉnh logic)
                var gioTinhNguyen = await _context.PhanCongTinhNguyenViens.CountAsync() * 4;

                var result = new
                {
                    tongSoSuKien = tongSoSuKien,
                    treEmDuocHoTro = treEmDuocHoTro,
                    tongTienUngHo = tongTienUngHo,
                    hienVatUngHo = hienVatUngHo,
                    tongTinhNguyenVien = tongTinhNguyenVien,
                    gioTinhNguyen = gioTinhNguyen
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/SuKienTheoThang
        [HttpGet("SuKienTheoThang")]
        public async Task<IActionResult> GetSuKienTheoThang([FromQuery] int nam = 2025)
        {
            try
            {
                var allSuKien = await _context.SuKiens.ToListAsync();

                if (allSuKien == null || !allSuKien.Any())
                {
                    return Ok(Enumerable.Range(1, 12).Select(month => new
                    {
                        thang = $"Tháng {month}",
                        soLuong = 0
                    }).ToList());
                }

                var data = allSuKien
                    .Where(s => s.NgayBatDau.HasValue && ConvertToDateTime(s.NgayBatDau.Value).Year == nam)
                    .GroupBy(s => ConvertToDateTime(s.NgayBatDau.Value).Month)
                    .Select(g => new
                    {
                        thang = g.Key,
                        soLuong = g.Count()
                    })
                    .OrderBy(x => x.thang)
                    .ToList();

                // Đảm bảo có đủ 12 tháng
                var fullData = Enumerable.Range(1, 12).Select(month => new
                {
                    thang = $"Tháng {month}",
                    soLuong = data.FirstOrDefault(d => d.thang == month)?.soLuong ?? 0
                }).ToList();

                return Ok(fullData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/TreEmTheoThang
        [HttpGet("TreEmTheoThang")]
        public async Task<IActionResult> GetTreEmTheoThang([FromQuery] int nam = 2025)
        {
            try
            {
                var allSuKien = await _context.SuKiens.ToListAsync();

                var data = allSuKien
                    .Where(s => s.NgayBatDau.HasValue && ConvertToDateTime(s.NgayBatDau.Value).Year == nam)
                    .GroupBy(s => ConvertToDateTime(s.NgayBatDau.Value).Month)
                    .Select(g => new
                    {
                        thang = g.Key,
                        soLuong = g.Sum(s => s.SoLuongTreEm ?? 0)
                    })
                    .OrderBy(x => x.thang)
                    .ToList();

                // Đảm bảo có đủ 12 tháng
                var fullData = Enumerable.Range(1, 12).Select(month => new
                {
                    thang = $"Tháng {month}",
                    soLuong = data.FirstOrDefault(d => d.thang == month)?.soLuong ?? 0
                }).ToList();

                return Ok(fullData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/NguonUngHo
        [HttpGet("NguonUngHo")]
        public async Task<IActionResult> GetNguonUngHo()
        {
            try
            {
                var data = await _context.UngHos
                    .GroupBy(u => u.LoaiUngHo)
                    .Select(g => new
                    {
                        loai = g.Key,
                        soTien = g.Sum(u => u.SoTien ?? 0)
                    })
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/TinhNguyenVienTheoThang
        [HttpGet("TinhNguyenVienTheoThang")]
        public async Task<IActionResult> GetTinhNguyenVienTheoThang([FromQuery] int nam = 2025)
        {
            try
            {
                var allPhanCong = await _context.PhanCongTinhNguyenViens
                    .Include(p => p.SuKien)
                    .ToListAsync();

                var data = allPhanCong
                    .Where(p => p.SuKien != null && p.SuKien.NgayBatDau.HasValue && ConvertToDateTime(p.SuKien.NgayBatDau.Value).Year == nam)
                    .GroupBy(p => ConvertToDateTime(p.SuKien.NgayBatDau.Value).Month)
                    .Select(g => new
                    {
                        thang = g.Key,
                        soLuong = g.Select(p => p.TinhNguyenVienId).Distinct().Count()
                    })
                    .OrderBy(x => x.thang)
                    .ToList();

                // Đảm bảo có đủ 12 tháng
                var fullData = Enumerable.Range(1, 12).Select(month => new
                {
                    thang = $"Tháng {month}",
                    soLuong = data.FirstOrDefault(d => d.thang == month)?.soLuong ?? 0
                }).ToList();

                return Ok(fullData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/SoSanhTheoThang
        [HttpGet("SoSanhTheoThang")]
        public async Task<IActionResult> GetSoSanhTheoThang([FromQuery] int nam = 2025)
        {
            try
            {
                var allSuKien = await _context.SuKiens.ToListAsync();
                var allUngHo = await _context.UngHos.ToListAsync();

                // Su Kien Data
                var suKienData = allSuKien
                    .Where(s => s.NgayBatDau.HasValue && ConvertToDateTime(s.NgayBatDau.Value).Year == nam)
                    .GroupBy(s => ConvertToDateTime(s.NgayBatDau.Value).Month)
                    .Select(g => new { thang = g.Key, soLuong = g.Count() })
                    .ToList();

                // Tre Em Data
                var treEmData = allSuKien
                    .Where(s => s.NgayBatDau.HasValue && ConvertToDateTime(s.NgayBatDau.Value).Year == nam)
                    .GroupBy(s => ConvertToDateTime(s.NgayBatDau.Value).Month)
                    .Select(g => new { thang = g.Key, soLuong = g.Sum(s => s.SoLuongTreEm ?? 0) })
                    .ToList();

                // Ung Ho Data
                var ungHoData = allUngHo
                    .Where(u => u.NgayUngHo.HasValue && ConvertToDateTime(u.NgayUngHo.Value).Year == nam)
                    .GroupBy(u => ConvertToDateTime(u.NgayUngHo.Value).Month)
                    .Select(g => new { thang = g.Key, soTien = g.Sum(u => u.SoTien ?? 0) })
                    .ToList();

                var result = Enumerable.Range(1, 12).Select(month => new
                {
                    thang = $"T{month}",
                    suKien = suKienData.FirstOrDefault(d => d.thang == month)?.soLuong ?? 0,
                    treEm = treEmData.FirstOrDefault(d => d.thang == month)?.soLuong ?? 0,
                    ungHo = ungHoData.FirstOrDefault(d => d.thang == month)?.soTien ?? 0
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/PhanLoaiSuKien
        [HttpGet("PhanLoaiSuKien")]
        public async Task<IActionResult> GetPhanLoaiSuKien()
        {
            try
            {
                var allSuKien = await _context.SuKiens.ToListAsync();

                // Phân loại dựa trên tên sự kiện
                var data = allSuKien
                    .Where(s => !string.IsNullOrEmpty(s.TenSuKien))
                    .Select(s => new
                    {
                        loai = s.TenSuKien.ToLower().Contains("trung thu") ? "Lễ hội" :
                               s.TenSuKien.ToLower().Contains("hội") ? "Hội trại" :
                               s.TenSuKien.ToLower().Contains("tết") ? "Từ thiện" :
                               s.TenSuKien.ToLower().Contains("xanh") ? "Môi trường" : "Khác"
                    })
                    .GroupBy(x => x.loai)
                    .Select(g => new
                    {
                        loai = g.Key,
                        soLuong = g.Count()
                    })
                    .ToList();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/TyLeHoanThanhMucTieu
        [HttpGet("TyLeHoanThanhMucTieu")]
        public async Task<IActionResult> GetTyLeHoanThanhMucTieu()
        {
            try
            {
                var allSuKien = await _context.SuKiens.ToListAsync();
                var tongSuKien = allSuKien.Count;

                var today = DateOnly.FromDateTime(DateTime.Now);
                var suKienHoanThanh = allSuKien.Count(s => s.NgayKetThuc.HasValue && s.NgayKetThuc.Value < today);

                var tyLe = tongSuKien > 0 ? (double)suKienHoanThanh / tongSuKien * 100 : 0;

                var result = new
                {
                    tyLeHoanThanh = Math.Round(tyLe, 2),
                    chuaHoanThanh = Math.Round(100 - tyLe, 2)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/ChiTietTheoTuan
        [HttpGet("ChiTietTheoTuan")]
        public async Task<IActionResult> GetChiTietTheoTuan([FromQuery] int nam = 2025)
        {
            try
            {
                var allSuKien = await _context.SuKiens.ToListAsync();
                var allUngHo = await _context.UngHos.ToListAsync();

                var data = allSuKien
                    .Where(s => s.NgayBatDau.HasValue && ConvertToDateTime(s.NgayBatDau.Value).Year == nam)
                    .Select(s => new
                    {
                        tuan = $"Tuần {GetWeekOfYear(ConvertToDateTime(s.NgayBatDau.Value))}",
                        soSuKien = 1,
                        treThamGia = s.SoLuongTreEm ?? 0,
                        tinhNguyenVien = s.SoLuongTinhNguyenVien ?? 0
                    })
                    .ToList();

                var ungHoData = allUngHo
                    .Where(u => u.NgayUngHo.HasValue && ConvertToDateTime(u.NgayUngHo.Value).Year == nam)
                    .ToList();

                var grouped = data.GroupBy(d => d.tuan)
                    .Select(g => new
                    {
                        tuan = g.Key,
                        soSuKien = g.Sum(x => x.soSuKien),
                        treThamGia = g.Sum(x => x.treThamGia),
                        tienUngHo = ungHoData
                            .Where(u => u.NgayUngHo.HasValue && $"Tuần {GetWeekOfYear(ConvertToDateTime(u.NgayUngHo.Value))}" == g.Key && u.LoaiUngHo == "Tiền mặt")
                            .Sum(u => u.SoTien ?? 0),
                        hienVat = ungHoData
                            .Count(u => u.NgayUngHo.HasValue && $"Tuần {GetWeekOfYear(ConvertToDateTime(u.NgayUngHo.Value))}" == g.Key && u.LoaiUngHo != "Tiền mặt"),
                        tinhNguyenVien = g.Sum(x => x.tinhNguyenVien),
                        gioTinhNguyen = g.Sum(x => x.tinhNguyenVien) * 4
                    })
                    .OrderBy(x => x.tuan)
                    .ToList();

                return Ok(grouped);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/BaoCaoThongKe/TopTinhNguyenVien
        [HttpGet("TopTinhNguyenVien")]
        public async Task<IActionResult> GetTopTinhNguyenVien([FromQuery] int top = 10)
        {
            try
            {
                var data = await _context.PhanCongTinhNguyenViens
                    .Include(p => p.TinhNguyenVien)
                    .ThenInclude(t => t.User)
                    .ToListAsync();

                var result = data
                    .GroupBy(p => new
                    {
                        p.TinhNguyenVienId,
                        HoTen = p.TinhNguyenVien?.User?.HoTen ?? "Không xác định"
                    })
                    .Select(g => new
                    {
                        hoTen = g.Key.HoTen,
                        soSuKien = g.Count(),
                        gioTinhNguyen = g.Count() * 4,
                        danhGia = 5.0
                    })
                    .OrderByDescending(x => x.soSuKien)
                    .Take(top)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        private int GetWeekOfYear(DateTime date)
        {
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date,
                System.Globalization.CalendarWeekRule.FirstDay,
                DayOfWeek.Monday);
        }
    }
}