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
                // Bỏ tài khoản Admin ra (không phân biệt hoa/thường và khoảng trắng)
                var users = _context.NguoiDungs
                    .Where(x => x.VaiTro.Trim().ToLower() != "admin");

                var result = new ThongKe
                {
                    TongTaiKhoan = await users.CountAsync(),
                    SoTnv = await users.CountAsync(x => x.VaiTro.Trim().ToLower() == "tình nguyện viên"),
                    CanBo = await users.CountAsync(x => x.VaiTro.Trim().ToLower() == "cán bộ"),
                    SoPhuHuynh = await users.CountAsync(x => x.VaiTro.Trim().ToLower() == "phụ huynh"),
                    DangHoatDong = await users.CountAsync(x => x.TrangThai == "Đang hoạt động"),
                    DaBiKhoa = await users.CountAsync(x => x.TrangThai == "Đã bị khóa")
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
                var query = _context.NguoiDungs
                    .Where(x => x.VaiTro.Trim().ToLower() != "admin"); // Bỏ Admin

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
        [HttpPost("KiemTraTonTai")]
        public async Task<IActionResult> KiemTraTonTai([FromBody] KiemTraTonTaiDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.SDT))
                return BadRequest(new { message = "Email và Số điện thoại không được để trống!" });

            var emailTonTai = await _context.NguoiDungs
                .AnyAsync(x => x.Email == model.Email);

            if (emailTonTai)
                return BadRequest(new { message = "Email đã tồn tại!" });

            var sdtTonTai = await _context.NguoiDungs
                .AnyAsync(x => x.SDT.Trim() == model.SDT.Trim());

            if (sdtTonTai)
                return BadRequest(new { message = "Số điện thoại đã tồn tại!" });

            return Ok(new { message = "Dữ liệu hợp lệ, có thể tạo tài khoản." });
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
                MatKhau = dto.MatKhau,
                VaiTro = dto.VaiTro,
                TrangThai = dto.TrangThai,
                Anh = dto.Anh,
                NgayTao = dto.NgayTao,
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

            var existingPhones = await _context.NguoiDungs.Select(x => x.SDT).ToListAsync();
            var existingEmails = await _context.NguoiDungs.Select(x => x.Email.ToLower()).ToListAsync();

            var phoneSet = new HashSet<string>();
            var emailSet = new HashSet<string>();

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
                    var sdt = dto.SDT.Trim();
                    if (!System.Text.RegularExpressions.Regex.IsMatch(sdt, @"^0\d{9}$"))
                    {
                        errors.Add("SĐT không hợp lệ (10 số, bắt đầu bằng 0)");
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
                    errors.Add("Thiếu email");
                else
                {
                    var email = dto.Email.Trim().ToLower();
                    if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                        errors.Add("Email không hợp lệ");
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
                    errors.Add("Thiếu vai trò");
                else
                {
                    var vaiTro = dto.VaiTro.Trim();
                    if (vaiTro == "Phu huynh") vaiTro = "Phụ huynh";
                    if (!validRoles.Contains(vaiTro))
                        errors.Add("Vai trò không hợp lệ");
                }

                // Xác định status cuối
                if (errors.Count > 0 && status != "duplicate")
                    status = "error";

                result.Add(new
                {
                    index = index,
                    errors = errors
                });
            }

            return Ok(new { chiTiet = result });
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
        [HttpPut("SuaTaiKhoan/{id}")]
        public async Task<IActionResult> SuaTaiKhoan(int id, [FromBody] NguoiDungUpdateDto dto)
        {
            var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(x => x.UserId == id);

            if (nguoiDung == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            // Kiểm tra trùng Email (ngoại trừ chính UserId đang sửa)
            var emailTonTai = await _context.NguoiDungs
                .AnyAsync(x => x.Email == dto.Email && x.UserId != id);
            if (emailTonTai)
                return BadRequest(new { message = "Email đã tồn tại!" });

            // Kiểm tra trùng SDT (ngoại trừ chính UserId đang sửa)
            var sdtTonTai = await _context.NguoiDungs
                .AnyAsync(x => x.SDT == dto.SDT && x.UserId != id);
            if (sdtTonTai)
                return BadRequest(new { message = "Số điện thoại đã tồn tại!" });

            // Cập nhật
            nguoiDung.HoTen = dto.HoTen;
            nguoiDung.Email = dto.Email;
            nguoiDung.SDT = dto.SDT;
            nguoiDung.VaiTro = dto.VaiTro;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật tài khoản thành công!" });
        }

    }
}
