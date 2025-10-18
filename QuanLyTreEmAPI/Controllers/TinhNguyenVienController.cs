using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinhNguyenVienController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        public TinhNguyenVienController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tnvs = await _context.TinhNguyenViens.ToListAsync();
            return Ok(tnvs);
        }
        //[HttpGet("ThongTinChiTiet")]
        //public async Task<IActionResult> GetThongTinChiTiet()
        //{
        //    var tnvs = await _context.TinhNguyenViens
        //        .Include(t => t.NguoiDungs)
        //        .Include(t => t.KhuPho)
        //        .Select(tnv => new
        //        {
        //            ID = tnv.TinhNguyenVienID,
        //            TenTinhNguyenVien = tnv.NguoiDung.HoTen,
        //            ChucVu = tnv.ChucVu,
        //            KhuPho = tnv.KhuPho.TenKhuPho,
        //            SDT = tnv.SDT,
        //            CaRanh = tnv.LichTrongs
        //                .SelectMany(l => l.ChiTietLichTrongs)
        //                .Count(),
        //            SuKien = _context.PhanCongTinhNguyenViens
        //                .Where(p => p.TinhNguyenVienId == tnv.TinhNguyenVienId)
        //                .Select(p => p.SuKienId)
        //                .Distinct()
        //                .Count(),
        //            TrangThai = "Hoạt động"
        //        })
        //        .ToListAsync();

        //    return Ok(tnvs);
        //}
    }
}
