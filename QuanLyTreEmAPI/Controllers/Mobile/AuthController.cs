using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuanLyTreEmAPI.DTOs.Auth;
using QuanLyTreEmAPI.DTOs.PhuHuynh;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Controllers.Mobile
{
    [Route("api/mobile/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly QuanLyTreEmContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(QuanLyTreEmContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.SDT) || string.IsNullOrWhiteSpace(request.MatKhau))
                return BadRequest(new { message = "Số điện thoại và mật khẩu không được để trống" });

            try
            {
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.SDT == request.SDT);

                if (nguoiDung == null)
                    return Unauthorized(new { message = "Số điện thoại hoặc mật khẩu không đúng" });

                if (nguoiDung.MatKhau != request.MatKhau)
                    return Unauthorized(new { message = "Số điện thoại hoặc mật khẩu không đúng" });

                if (nguoiDung.TrangThai != "Đang hoạt động")
                    return Unauthorized(new { message = "Tài khoản đã bị vô hiệu hóa" });

                var token = GenerateJwtToken(nguoiDung);

                var response = new LoginResponseDTO
                {
                    Token = token,
                    HoTen = nguoiDung.HoTen,
                    VaiTro = nguoiDung.VaiTro,
                    UserID = nguoiDung.UserId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        private string GenerateJwtToken(NguoiDung nguoiDung)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationDays = int.Parse(jwtSettings["ExpirationDays"]);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, nguoiDung.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, nguoiDung.HoTen),
                new Claim(ClaimTypes.Role, nguoiDung.VaiTro),
                new Claim("sdt", nguoiDung.SDT),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expirationDays),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Test endpoint to verify authentication
        [HttpGet("profile")]
        //[Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                userId,
                userName,
                userRole,
                message = "Xác thực thành công!"
            });
        }
        // ============================================
        // TAB TÀI KHOẢN - ĐỔI MẬT KHẨU - ĐĂNG XUẤT
        // ============================================
        [HttpPost("DoiMatKhau")]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauRequestDTO request)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                // Validation
                if (string.IsNullOrWhiteSpace(request.MatKhauCu) || string.IsNullOrWhiteSpace(request.MatKhauMoi))
                {
                    return BadRequest(new { message = "Mật khẩu cũ và mật khẩu mới không được để trống" });
                }

                if (request.MatKhauMoi.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
                }

                var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
                if (nguoiDung == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Kiểm tra mật khẩu cũ
                if (nguoiDung.MatKhau != request.MatKhauCu)
                {
                    return BadRequest(new { message = "Mật khẩu cũ không đúng" });
                }

                // Cập nhật mật khẩu mới
                nguoiDung.MatKhau = request.MatKhauMoi;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        [HttpPost("DangXuat")]
        public IActionResult DangXuat()
        {
            // Với JWT, việc đăng xuất được xử lý ở client (xóa token)
            // API này chỉ trả về message xác nhận
            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}
