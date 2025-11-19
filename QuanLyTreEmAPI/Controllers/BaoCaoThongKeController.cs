using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using QuanLyTreEmAPI.Models;

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

        private static DateTime? ToDateTime(DateOnly? dateOnly)
        {
            return dateOnly.HasValue ? dateOnly.Value.ToDateTime(TimeOnly.MinValue) : null;
        }

        // ---------------------------
        // 0) Helpers
        // ---------------------------
        private static DateTime FloorToPeriod(DateTime dt, string granularity)
        {
            granularity = (granularity ?? "month").ToLowerInvariant();
            return granularity switch
            {
                "day" => dt.Date,
                "week" => dt.Date.AddDays(-(int)(dt.Date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)dt.Date.DayOfWeek - 1)),
                "quarter" => new DateTime(dt.Year, (((dt.Month - 1) / 3) * 3) + 1, 1),
                "year" => new DateTime(dt.Year, 1, 1),
                _ => new DateTime(dt.Year, dt.Month, 1)
            };
        }

        private static DateTime AddOnePeriod(DateTime dt, string granularity)
        {
            granularity = (granularity ?? "month").ToLowerInvariant();
            return granularity switch
            {
                "day" => dt.AddDays(1),
                "week" => dt.AddDays(7),
                "quarter" => dt.AddMonths(3),
                "year" => dt.AddYears(1),
                _ => dt.AddMonths(1)
            };
        }

        private static IEnumerable<(DateTime start, DateTime end)> Periods(DateTime from, DateTime to, string granularity)
        {
            var cursor = FloorToPeriod(from, granularity);
            while (cursor < to)
            {
                var next = AddOnePeriod(cursor, granularity);
                yield return (cursor, next);
                cursor = next;
            }
        }

        // ---------------------------
        // 1) Tổng quan dashboard
        // ---------------------------
        [HttpGet("TongQuan")]
        public IActionResult GetTongQuan([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var fromDate = from ?? DateTime.Today.AddMonths(-12);   
            var toDate = to ?? DateTime.Today.AddDays(1);

            // Tổng sự kiện trong kỳ
            var soSuKien = _context.SuKiens
                .AsEnumerable()
                .Count(x => ToDateTime(x.NgayBatDau) >= fromDate && ToDateTime(x.NgayBatDau) < toDate);

            // Tổng chi phí sự kiện trong kỳ
            var tongChiPhiSuKien = _context.ChiPhiSuKiens
                .Include(x => x.SuKien)
                .AsEnumerable()
                .Where(x => ToDateTime(x.SuKien.NgayBatDau) >= fromDate && ToDateTime(x.SuKien.NgayBatDau) < toDate)
                .Sum(x => (decimal?)x.SoTien) ?? 0m;

            // Tổng ủng hộ (tiền) trong kỳ
            var tongUngHo = _context.UngHos
                .AsEnumerable()
                .Where(x => ToDateTime(x.NgayUngHo) >= fromDate && ToDateTime(x.NgayUngHo) < toDate)
                .Sum(x => (decimal?)x.SoTien) ?? 0m;
            //var tongUngHo = _context.UngHos.AsEnumerable()
            //    .Where(x => ToDateTime(x.NgayUngHo) >= fromDate && ToDateTime(x.NgayUngHo) < toDate).Sum(x => (decimal?)x.SoTien) ?? 0m;

            // Số trẻ được hỗ trợ
            var soTreDuocHoTro = _context.HoTroPhucLois
                .AsEnumerable()
                .Where(x => ToDateTime(x.NgayCap) >= fromDate && ToDateTime(x.NgayCap) < toDate)
                .Select(x => x.TreEmId)
                .Distinct()
                .Count();

            // Số Tình nguyện viên
            var soTinhNguyenVien = _context.TinhNguyenViens.Count();

            // Số thông báo trong kỳ
            var soThongBao = _context.ThongBaos
                .AsEnumerable()
                .Count(x => ToDateTime(x.NgayThongBao) >= fromDate && ToDateTime(x.NgayThongBao) < toDate);

            return Ok(new
            {
                From = fromDate,
                To = toDate,
                SoSuKien = soSuKien,
                TongChiPhiSuKien = tongChiPhiSuKien,
                TongUngHo = tongUngHo,
                SoTreDuocHoTro = soTreDuocHoTro,
                SoTinhNguyenVien = soTinhNguyenVien,
                SoThongBao = soThongBao
            });
        }

        // ---------------------------
        // 2) Sự kiện theo kỳ
        // ---------------------------
        [HttpGet("SuKienTheoKy")]
        public IActionResult GetSuKienTheoKy([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string granularity = "month")
        {
            var fromDate = from ?? DateTime.Today.AddMonths(-12);
            var toDate = to ?? DateTime.Today.AddDays(1);

            var events = _context.SuKiens
                .AsEnumerable()
                .Where(x => ToDateTime(x.NgayBatDau) >= fromDate && ToDateTime(x.NgayBatDau) < toDate)
                .Select(x => ToDateTime(x.NgayBatDau))
                .ToList();

            var series = Periods(fromDate, toDate, granularity)
                .Select(p => new
                {
                    Label = granularity.ToLowerInvariant() switch
                    {
                        "day" => p.start.ToString("dd/MM/yyyy"),
                        "week" => $"Tuần {CultureInfo.GetCultureInfo("vi-VN").Calendar.GetWeekOfYear(p.start, CalendarWeekRule.FirstDay, DayOfWeek.Monday)}/{p.start.Year}",
                        "quarter" => $"Q{((p.start.Month - 1) / 3) + 1}/{p.start.Year}",
                        "year" => p.start.ToString("yyyy"),
                        _ => p.start.ToString("MM/yyyy")
                    },
                    Count = events.Count(d => d >= p.start && d < p.end)
                });

            return Ok(series);
        }

        // ---------------------------
        // 3) Trẻ được hỗ trợ theo kỳ
        // ---------------------------
        [HttpGet("TreHoTroTheoKy")]
        public IActionResult GetTreHoTroTheoKy([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string granularity = "month")
        {
            var fromDate = from ?? DateTime.Today.AddMonths(-12);
            var toDate = to ?? DateTime.Today.AddDays(1);

            var items = _context.HoTroPhucLois
                .AsEnumerable()
                .Where(x => ToDateTime(x.NgayCap) >= fromDate && ToDateTime(x.NgayCap) < toDate)
                .Select(x => new { x.TreEmId, NgayCap = ToDateTime(x.NgayCap) })
                .ToList();

            var series = Periods(fromDate, toDate, granularity)
                .Select(p => new
                {
                    Label = p.start.ToString("MM/yyyy"),
                    CountTreDistinct = items
                        .Where(i => i.NgayCap >= p.start && i.NgayCap < p.end)
                        .Select(i => i.TreEmId)
                        .Distinct()
                        .Count()
                });

            return Ok(series);
        }

        // ---------------------------
        // 4) Ủng hộ theo loại
        // ---------------------------
        [HttpGet("UngHoTheoLoai")]
        public IActionResult GetUngHoTheoLoai([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var fromDate = from ?? DateTime.Today.AddMonths(-12);
            var toDate = to ?? DateTime.Today.AddDays(1);

            var data = _context.UngHos
                .AsEnumerable()
                .Where(x => ToDateTime(x.NgayUngHo) >= fromDate && ToDateTime(x.NgayUngHo) < toDate)
                .GroupBy(x => x.LoaiUngHo)
                .Select(g => new
                {
                    LoaiUngHo = g.Key,
                    TongSoTien = g.Sum(x => x.SoTien),
                    SoLuotUngHo = g.Count()
                })
                .OrderByDescending(x => x.TongSoTien)
                .ToList();

            return Ok(data);
        }
    }
}
