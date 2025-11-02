using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.DTOs.QuanLyTaiKhoan;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyTaiKhoanController : ControllerBase
    {

        private readonly QuanLyTreEmContext _context;
        public QuanLyTaiKhoanController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet("ThongKe")]
        public async Task<IActionResult> GetThongKe()
        {
            try
            {
                var tongNguoiDung = await _context.NguoiDungs.CountAsync();
                var soAdmin = await _context.NguoiDungs.CountAsync(x => x.VaiTro == "Admin");
                var soTNV = await _context.NguoiDungs.CountAsync(x => x.VaiTro == "Tình nguyện viên");
                var soPhuHuynh = await _context.NguoiDungs.CountAsync(x => x.VaiTro == "Phụ huynh");
                var dangHoatDong = await _context.NguoiDungs.CountAsync(x => x.TrangThai == "Đang hoạt động");
                var chuaKichHoat = await _context.NguoiDungs.CountAsync(x => x.TrangThai == "Chưa kích hoạt");
                var biKhoa = await _context.NguoiDungs.CountAsync(x => x.TrangThai == "Đã bị khóa");

                var result = new
                {
                    tongNguoiDung,
                    soAdmin,
                    soTNV,
                    soPhuHuynh,
                    dangHoatDong,
                    chuaKichHoat,
                    biKhoa,
                    tyLeDangHoatDong = tongNguoiDung > 0 ? (dangHoatDong * 100.0 / tongNguoiDung) : 0
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải thống kê", error = ex.Message });
            }
        }

        // GET: api/NguoiDungApi
        [HttpGet("DanhSachNguoiDung")]
        public async Task<IActionResult> GetDanhSachNguoiDung()
        {
            try
            {
                var query = _context.NguoiDungs.AsQueryable();
                var danhSach = await query
                    .Select(x => new
                    {
                        x.UserId,
                        x.HoTen,
                        x.Email,
                        x.SDT,
                        x.VaiTro,
                        x.TrangThai,
                        x.NgayTao,
                    })
                    .OrderByDescending(x => x.UserId)
                    .ToListAsync();

                return Ok(danhSach);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh sách", error = ex.Message });
            }
        }
        // GET: api/NguoiDungApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNguoiDung(int id)
        {
            try
            {
                var nguoiDung = await _context.NguoiDungs
                    .Where(x => x.UserId == id)
                    .Select(x => new
                    {
                        x.UserId,
                        x.HoTen,
                        x.Email,
                        x.SDT,
                        x.VaiTro,
                        x.TrangThai,
                        x.NgayTao,
                        x.Anh
                    })
                    .FirstOrDefaultAsync();

                if (nguoiDung == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                return Ok(nguoiDung);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải thông tin", error = ex.Message });
            }
        }

        // POST: api/NguoiDungApi
        [HttpPost("ThemTaiKhoan")]
        public async Task<IActionResult> TaoNguoiDung([FromBody] NguoiDungCreateDto dto)
        {
            var nguoiDung = new NguoiDung
            {
                HoTen = dto.HoTen,
                Email = dto.Email,
                SDT = dto.SDT,
                MatKhau = dto.MatKhau ?? "140letrongtan", // Mật khẩu mặc định
                VaiTro = dto.VaiTro,
                TrangThai = "Đang hoạt động",
                Anh = "/Anh/NguoiDung/macdinh.jpg",
                NgayTao = DateOnly.FromDateTime(DateTime.Now),
            };
            _context.NguoiDungs.Add(nguoiDung);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Tạo người dùng thành công!" });

        }
        [HttpPost("ThemNhieuTaiKhoan")]
        public async Task<IActionResult> ThemNhiuNguoiDung([FromBody] List<NguoiDungCreateDto> dsNguoiDung)
        {
            if (dsNguoiDung == null || dsNguoiDung.Count == 0)
                return BadRequest(new { message = "Danh sách người dùng trống!" });

            foreach (var dto in dsNguoiDung)
            {
                var nguoiDung = new NguoiDung
                {
                    HoTen = dto.HoTen,
                    Email = dto.Email,
                    SDT = dto.SDT,
                    MatKhau = dto.MatKhau ?? "140letrongtan",
                    VaiTro = dto.VaiTro,
                    TrangThai = "Đang hoạt động",
                    Anh = "/Anh/NguoiDung/macdinh.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now),
                };

                _context.NguoiDungs.Add(nguoiDung);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã import thành công {dsNguoiDung.Count} tài khoản!" });
        }
        [HttpPost("ValidateDanhSachTaiKhoan")]
        public async Task<IActionResult> ValidateDanhSachTaiKhoan([FromBody] List<NguoiDungCreateDto> dsNguoiDung)
        {
            if (dsNguoiDung == null || dsNguoiDung.Count == 0)
                return BadRequest(new { message = "Danh sách trống!" });

            var validRoles = new[] { "Cán bộ", "Tình nguyện viên", "Phụ huynh" };

            // Lấy data từ DB để check trùng
            var existingPhones = await _context.NguoiDungs
                .Select(x => x.SDT)
                .ToListAsync();
            var existingEmails = await _context.NguoiDungs
                .Select(x => x.Email.ToLower())
                .ToListAsync();

            var phoneSet = new HashSet<string>();
            var emailSet = new HashSet<string>();

            int validCount = 0, duplicateCount = 0, errorCount = 0;
            var result = new List<object>();

            foreach (var (dto, index) in dsNguoiDung.Select((d, i) => (d, i)))
            {
                var errors = new List<string>();
                var status = "valid";

                // Validate Họ tên
                if (string.IsNullOrWhiteSpace(dto.HoTen))
                    errors.Add("Thiếu họ tên");

                // Validate SĐT
                if (string.IsNullOrWhiteSpace(dto.SDT))
                {
                    errors.Add("Thiếu số điện thoại");
                }
                else
                {
                    var sdt = dto.SDT.Trim().Replace("'", "");
                    if (!System.Text.RegularExpressions.Regex.IsMatch(sdt, @"^\d{10,11}$"))
                    {
                        errors.Add("SĐT không hợp lệ (10-11 số)");
                    }
                    else if (existingPhones.Contains(sdt))
                    {
                        errors.Add("SĐT đã tồn tại trong hệ thống");
                        status = "duplicate";
                    }
                    else if (phoneSet.Contains(sdt))
                    {
                        errors.Add("SĐT trùng lặp trong file");
                        status = "duplicate";
                    }
                    else
                    {
                        phoneSet.Add(sdt);
                    }
                }

                // Validate Email
                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    errors.Add("Thiếu email");
                }
                else
                {
                    var email = dto.Email.Trim().ToLower();
                    if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                    {
                        errors.Add("Email không hợp lệ");
                    }
                    else if (existingEmails.Contains(email))
                    {
                        errors.Add("Email đã tồn tại trong hệ thống");
                        status = "duplicate";
                    }
                    else if (emailSet.Contains(email))
                    {
                        errors.Add("Email trùng lặp trong file");
                        status = "duplicate";
                    }
                    else
                    {
                        emailSet.Add(email);
                    }
                }

                // Validate Vai trò
                if (string.IsNullOrWhiteSpace(dto.VaiTro))
                {
                    errors.Add("Thiếu vai trò");
                }
                else
                {
                    var vaiTro = dto.VaiTro.Trim();
                    if (vaiTro == "Phu huynh") vaiTro = "Phụ huynh";

                    if (!validRoles.Contains(vaiTro))
                    {
                        errors.Add("Vai trò không hợp lệ");
                    }
                }

                // Xác định status cuối
                if (errors.Count > 0 && status != "duplicate")
                    status = "error";

                // Đếm
                if (status == "valid") validCount++;
                else if (status == "duplicate") duplicateCount++;
                else errorCount++;

                result.Add(new
                {
                    index = index,
                    hoTen = dto.HoTen,
                    sdt = dto.SDT,
                    email = dto.Email,
                    vaiTro = dto.VaiTro,
                    status = status,
                    errors = errors
                });
            }

            return Ok(new
            {
                tongSo = dsNguoiDung.Count,
                hopLe = validCount,
                trungLap = duplicateCount,
                loi = errorCount,
                chiTiet = result
            });
        }

        // DELETE: api/NguoiDungApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> XoaNguoiDung(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            _context.NguoiDungs.Remove(nguoiDung);
            await _context.SaveChangesAsync();

            return Ok();

        }

        // POST: api/NguoiDungApi/ResetMatKhau/5
        [HttpPost("ResetMatKhau/{id}")]
        public async Task<IActionResult> ResetMatKhau(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            nguoiDung.MatKhau = "140letrongtan"; // Reset về mật khẩu mặc định
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/NguoiDungApi/ResetTatCaMatKhau
        [HttpPut("DoiTrangThai/{id}")]
        public async Task<IActionResult> DoiTrangThai(int id, [FromBody] DoiTrangThaiDto dto)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            nguoiDung.TrangThai = dto.TrangThai;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
