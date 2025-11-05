using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuKienController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        private readonly IWebHostEnvironment _env;

        public SuKienController(QuanLyTreEmContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suKiens = await _context.SuKiens.ToListAsync();
            return Ok(suKiens);
        }

        [HttpGet("all")]
        public IActionResult GetAllEvents()
        {
            var suKiens = _context.SuKiens
       .ToList() // ⚠️ Chuyển về memory
       .Select(e => new {
           e.SuKienId,
           e.TenSuKien,
           e.NgayBatDau,
           e.NgayKetThuc,
           e.DiaDiem,
           TrangThai = GetTrangThai(
               e.NgayBatDau?.ToDateTime(TimeOnly.MinValue),
               e.NgayKetThuc?.ToDateTime(TimeOnly.MinValue)
           )
       })
       .OrderBy(e => e.NgayBatDau)
       .ToList();

            return Ok(suKiens);
        }

        [HttpGet("sapdienra")]
        public IActionResult GetUpcomingEvents()
        {
            var today = DateTime.Today;

            var result = _context.SuKiens
                .Where(e => e.NgayBatDau.HasValue && e.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue) > today)
                .Select(e => new {
                    e.SuKienId,
                    e.TenSuKien,
                    e.NgayBatDau,
                    e.NgayKetThuc,
                    e.DiaDiem,
                    TrangThai = "Sắp diễn ra"
                })
                .OrderBy(e => e.NgayBatDau)
                .ToList();

            return Ok(result);
        }

        [HttpGet("dangdienra")]
        public IActionResult GetOngoingEvents()
        {
            var today = DateTime.Today;

            var result = _context.SuKiens
                .Where(e =>
                    e.NgayBatDau.HasValue && e.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue) <= today &&
                    e.NgayKetThuc.HasValue && e.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) >= today
                )
                .Select(e => new {
                    e.SuKienId,
                    e.TenSuKien,
                    e.NgayBatDau,
                    e.NgayKetThuc,
                    e.DiaDiem,
                    TrangThai = "Đang diễn ra"
                })
                .OrderBy(e => e.NgayBatDau)
                .ToList();

            return Ok(result);
        }

        [HttpGet("daketthuc")]
        public IActionResult GetPastEvents()
        {
            var today = DateTime.Today;

            var result = _context.SuKiens
                .Where(e => e.NgayKetThuc.HasValue && e.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) < today)
                .Select(e => new {
                    e.SuKienId,
                    e.TenSuKien,
                    e.NgayBatDau,
                    e.NgayKetThuc,
                    e.DiaDiem,
                    TrangThai = "Đã kết thúc"
                })
                .OrderByDescending(e => e.NgayKetThuc)
                .ToList();

            return Ok(result);
        }

        private string GetTrangThai(DateTime? start, DateTime? end)
        {
            var today = DateTime.Today;

            if (start.HasValue && start.Value > today) return "Sắp diễn ra";
            if (end.HasValue && end.Value < today) return "Đã kết thúc";
            return "Đang diễn ra";
        }
        #region TAB 1: THÔNG TIN CHUNG
        // ===========================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var sk = await _context.SuKiens
           .Include(x => x.KhuPho)
           .Include(x => x.ThoiGianChiTietSuKiens)
           .Include(x => x.ChiPhiSuKiens)
               .ThenInclude(cp => cp.ChiTietChiPhiSuKiens)
           .FirstOrDefaultAsync(x => x.SuKienId == id);

            if (sk == null)
                return NotFound("Không tìm thấy sự kiện");

            var result = new
            {
                sk.SuKienId,
                sk.TenSuKien,
                sk.MoTa,
                sk.DiaDiem,
                sk.NgayBatDau,
                sk.NgayKetThuc,
                sk.NguoiChiuTrachNhiem,
                KhuPho = sk.KhuPho?.TenKhuPho,
                ChiPhi = sk.ChiPhiSuKiens.Select(c => new
                {
                    c.TenKhoanChi,
                    c.SoTien,
                    c.GhiChu,
                    ChiTiet = c.ChiTietChiPhiSuKiens.Select(ct => new
                    {
                        ct.TenPhanQua,
                        ct.NguoiDaiDien,
                        ct.SoLuong,
                        ct.DonGia
                    })
                })
            };

            return Ok(result);
        }

        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file hợp lệ.");

            var suKien = await _context.SuKiens.FindAsync(id);
            if (suKien == null)
                return NotFound("Không tìm thấy sự kiện.");

            var uploadsRoot = Path.Combine(_env.ContentRootPath, "wwwroot", "Uploads", "SuKien", id.ToString());

            if (!Directory.Exists(uploadsRoot))
                Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var relativePath = $"/Uploads/SuKien/{id}/{fileName}";

            var minhChung = new PhieuMinhChung
            {
                LoaiMinhChung = Path.GetExtension(fileName),
                FilePath = relativePath,
                NgayCap = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.PhieuMinhChungs.Add(minhChung);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Tệp đã được tải lên thành công.",
                FileName = fileName,
                Path = relativePath
            });
        }

        #endregion

        // ===========================================================
        #region TAB 2: ĐĂNG KÝ THAM GIA
        // ===========================================================

        [HttpGet("{id}/dangky")]
        public async Task<IActionResult> GetDangKy(int id)
        {
            var list = await _context.DangKySuKiens
                .Include(x => x.User)
                .Where(x => x.SuKienId == id)
                .Select(x => new
                {
                    x.DangKySuKienId,
                    x.UserId,
                    HoTen = x.User.HoTen,
                    VaiTro = x.User.VaiTro,
                    x.NgayDangKy,
                    x.TrangThai
                }).ToListAsync();

            return Ok(list);
        }

        [HttpPost("{id}/duyet/{userId}")]
        public async Task<IActionResult> DuyetDangKy(int id, int userId, [FromQuery] string trangThai)
        {
            var dk = await _context.DangKySuKiens
                .FirstOrDefaultAsync(x => x.SuKienId == id && x.UserId == userId);

            if (dk == null)
                return NotFound("Không tìm thấy đăng ký");

            dk.TrangThai = trangThai;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Cập nhật trạng thái '{trangThai}' thành công" });
        }

        #endregion

        // ===========================================================
        #region TAB 3: PHÂN CÔNG TÌNH NGUYỆN VIÊN
        // ===========================================================

        [HttpGet("{id}/phancong")]
        public async Task<IActionResult> GetPhanCong(int id)
        {
            var list = await _context.PhanCongTinhNguyenViens
                .Include(x => x.TinhNguyenVien)
                    .ThenInclude(t => t.User)
                .Where(x => x.SuKienId == id)
                .Select(x => new
                {
                    x.PhanCongId,
                    x.SuKienId,
                    x.TinhNguyenVienId,
                    TenTNV = x.TinhNguyenVien.User.HoTen,
                    x.CongViec,
                    x.GhiChu,
                    x.NgayPhanCong
                }).ToListAsync();

            return Ok(list);
        }

        [HttpPost("{id}/phancong")]
        public async Task<IActionResult> AddPhanCong(int id, [FromBody] PhanCongTinhNguyenVien input)
        {
            input.SuKienId = id;
            input.NgayPhanCong = DateOnly.FromDateTime(DateTime.Now);

            _context.PhanCongTinhNguyenViens.Add(input);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Phân công thành công" });
        }

        [HttpGet("/api/TinhNguyenVien/{id}/lichtrong")]
        public async Task<IActionResult> GetLichTrong(int id)
        {
            var lich = await _context.LichTrongs
                .Include(x => x.ChiTietLichTrongs)
                .FirstOrDefaultAsync(x => x.TinhNguyenVienId == id);

            if (lich == null) return NotFound("Không có lịch trống");

            var result = lich.ChiTietLichTrongs.Select(c => new { c.Thu, c.Buoi });
            return Ok(result);
        }

        #endregion

        // ===========================================================
        #region TAB 4: CHI PHÍ
        // ===========================================================

        [HttpGet("{id}/chiphi")]
        public async Task<IActionResult> GetChiPhi(int id)
        {
            var list = await _context.ChiPhiSuKiens
                .Include(x => x.ChiTietChiPhiSuKiens)
                .Where(x => x.SuKienId == id)
                .Select(x => new
                {
                    x.ChiPhiId,
                    x.TenKhoanChi,
                    x.SoTien,
                    x.GhiChu,
                    ChiTiet = x.ChiTietChiPhiSuKiens.Select(ct => new
                    {
                        ct.TenPhanQua,
                        ct.SoLuong,
                        ct.DonGia,
                        ThanhTien = ct.SoLuong * ct.DonGia
                    })
                }).ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id}/chiphi/tong")]
        public async Task<IActionResult> GetTongChiPhi(int id)
        {
            var tong = await _context.ChiPhiSuKiens
                .Where(x => x.SuKienId == id)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0;

            return Ok(new { TongChiPhi = tong });
        }

        #endregion

        // ===========================================================
        #region TAB 5: THÔNG BÁO
        // ===========================================================

        [HttpGet("{id}/thongbao")]
        public async Task<IActionResult> GetThongBao(int id)
        {
            var list = await _context.ThongBaos
                .Where(x => x.SuKienId == id)
                .OrderByDescending(x => x.NgayThongBao)
                .Select(x => new
                {
                    x.ThongBaoId,
                    x.NoiDung,
                    x.NgayThongBao
                }).ToListAsync();

            return Ok(list);
        }

        [HttpPost("{id}/thongbao/guithongbao")]
        public async Task<IActionResult> GuiThongBao(int id, [FromBody] ThongBao input)
        {
            input.SuKienId = id;
            input.NgayThongBao = DateOnly.FromDateTime(DateTime.Now);

            _context.ThongBaos.Add(input);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Gửi thông báo thành công", input.NoiDung });
        }

        #endregion
    }
}