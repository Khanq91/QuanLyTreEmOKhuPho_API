using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.DTOs.SuKien;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuKienController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        public SuKienController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var SuKiens = await _context.SuKiens.ToListAsync();
            return Ok(SuKiens);
        }
        [HttpGet("SapToi")]
        public async Task<IActionResult> GetSuKienSapToi([FromQuery] int? KhuPhoID)
        {
            var allSuKien = await _context.SuKiens.ToListAsync();

            var suKienSapToi = allSuKien
                .Where(s => s.NgayBatDau.HasValue
                            && s.NgayBatDau.Value > DateOnly.FromDateTime(DateTime.Now)
                            && (KhuPhoID == null || s.KhuPhoId == KhuPhoID))
                .OrderBy(s => s.NgayBatDau)
                .Take(3)
                .ToList();

            return Ok(suKienSapToi);
        }
        [HttpGet("TTCB_ChiTietSuKien")]
        public async Task<IActionResult> TTCB_ChiTietSuKien([FromQuery] int? SuKienID)
        {
           var TTCB_ChiTietSuKien=_context.SuKiens.Where(s=>s.SuKienId== SuKienID).FirstOrDefault();
            return Ok(TTCB_ChiTietSuKien); 
        }
        [HttpGet("TTCT_ChiTietSuKien")]
        public async Task<IActionResult> TTCT_ChiTietSuKien([FromQuery] int? SuKienID)
        {
            if (SuKienID == null)
                return BadRequest("Thiếu SuKienID");

            var result = from tg in _context.ThoiGianChiTietSuKiens
                         join tm in _context.TietMucSuKiens
                             on tg.ThoiGianChiTietSuKienId equals tm.ThoiGianChiTietSuKienId
                         where tg.SuKienId == SuKienID
                         select new TTCT_ChiTietSuKien
                         {
                             Ngay = tg.ThoiGianBatDau.HasValue
                                 ? tg.ThoiGianBatDau.Value.ToString("dd/MM/yyyy")
                                 : "",
                             GioBatDau = tg.ThoiGianBatDau.HasValue
                                 ? tg.ThoiGianBatDau.Value.ToString("HH:mm")
                                 : "",
                             GioKetThuc = tg.ThoiGianKetThuc.HasValue
                                 ? tg.ThoiGianKetThuc.Value.ToString("HH:mm")
                                 : "",
                             NoiDung = tm.TenTietMuc,
                             NguoiThucHien = tm.NguoiThucHien
                         };

            return Ok(await result.ToListAsync());
        }
        [HttpGet("TT_ThanhVienSuKien_TNV")]
        public async Task<IActionResult> TT_ThanhVienSuKien_TNV([FromQuery] int? SuKienID)
        {
            if (SuKienID == null)
                return BadRequest("Thiếu SuKienID");

            var result = from dk in _context.DangKySuKiens
                         join nd in _context.NguoiDungs
                             on dk.UserId equals nd.UserId
                         where dk.SuKienId == SuKienID && nd.VaiTro == "Tình nguyện viên"
                         select new TT_DangKySuKien
                         {
                             SuKienID=dk.SuKienId,
                             userID = nd.UserId,
                             HoTen = nd.HoTen,
                             SDT = nd.SDT,
                             VaiTro=nd.VaiTro,
                             TrangThai = dk.TrangThai
                         };

            return Ok(await result.ToListAsync());
        }
       

        [HttpGet("TT_ThanhVienSuKienPhuHuynh")]
        public async Task<IActionResult> TT_ThanhVienSuKienPhuHuynh([FromQuery] int? SuKienID)
        {
            if (SuKienID == null)
                return BadRequest("Thiếu SuKienID");

            var query =
                from nd in _context.NguoiDungs
                join dk in _context.DangKySuKiens on nd.UserId equals dk.UserId
                join tp in _context.ThongTinPhuHuynhs on nd.UserId equals tp.UserId
                join tphe in _context.TreEmPhuHuynhs on tp.PhuHuynhId equals tphe.PhuHuynhId
                join tre in _context.TreEms on tphe.TreEmId equals tre.TreEmId
                where nd.VaiTro == "Phụ Huynh" && dk.SuKienId == SuKienID
                select new TT_DangKySuKien
                {
                    SuKienID = dk.SuKienId,
                    userID = nd.UserId,
                    HoTen = nd.HoTen,
                    TenTreEm = tre.HoTen,
                    SDT = nd.SDT,
                    TrangThai = dk.TrangThai,
                    VaiTro = nd.VaiTro
                };

            return Ok(await query.ToListAsync());
        }
        [HttpPut("CapNhatTrangThai")]
        public async Task<IActionResult> CapNhatTrangThaiDangKySuKien([FromQuery] int? SuKienID, [FromQuery] int? UserID, [FromQuery] string TrangThai)
        {
            var dangKy = await _context.DangKySuKiens
                     .Where(dk => dk.SuKienId == SuKienID && dk.UserId == UserID)
                     .FirstOrDefaultAsync();

            dangKy.TrangThai = TrangThai;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Cập nhật trạng thái thành công.",
                SuKienID,
                UserID,
                TrangThai
            });
        }
    }

}
