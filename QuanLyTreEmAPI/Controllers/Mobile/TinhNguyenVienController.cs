using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity.Data;
using QuanLyTreEmAPI.DTOs.TInhNguyenVien;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Controllers.Mobile
{
    [Route("api/Mobile/[controller]")]
    [ApiController]
    [Authorize(Roles = "Tình nguyện viên")]
    public class TinhNguyenVienController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        private readonly IWebHostEnvironment _env;

        public TinhNguyenVienController(QuanLyTreEmContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetCurrentUserId()
        {
            var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(subClaim))
                //return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                return 0;

            var userId = int.Parse(subClaim);
            return userId;
        }
        // +------------------------------------------------------------------+
        // |    TAB TRANG CHỦ                                                 |
        // +------------------------------------------------------------------+

        // TAB TRANG CHỦ API: GET api/Mobile/TinhNguyenVien/Home
        /// <summary>
        /// Lấy thông tin cho trang chủ bao gồm:
        ///     + Thông tin tài khoản
        ///     + 3 sự kiện được phân công
        ///     + Thông kê số hoạt động đã thực hiện, lấy 3 hoạt động gần nhất
        ///     + Bảng lịch trống mà tình nguyện viên đăng ký
        /// </summary>
        /// <returns>
        ///     + Thông tin tài khoản từ 'ThongTinTaiKhoanDTO'
        ///     + 3 sự kiện được phân công từ 'SuKienPhanCongDTO'
        ///     + Tổng số hoạt động, 3 hoạt động gần nhất 'SuKienDaThamGiaDTO'
        ///     + Lịch trống 7 ngày x 3 buổi (Sẽ sửa lại thành 2 buổi) 'LichTrongDTO'
        ///     => TinhNguyenVienHomeDTO
        /// </returns>
        [HttpGet("Home")]
        public async Task<IActionResult> GetHome()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                // 1. Thông tin tài khoản
                var nguoiDung = await _context.NguoiDungs
                    .Include(n => n.TinhNguyenVien)
                    .FirstOrDefaultAsync(n => n.UserId == userId);

                if (nguoiDung == null || nguoiDung.TinhNguyenVien == null)
                    return NotFound(new { message = "Không tìm thấy thông tin tình nguyện viên" });

                var tnv = nguoiDung.TinhNguyenVien;

                var thongTinTaiKhoan = new ThongTinTaiKhoanDTO
                {
                    UserId = nguoiDung.UserId,
                    TinhNguyenVienId = tnv.TinhNguyenVienId,
                    HoTen = nguoiDung.HoTen,
                    ChucVu = tnv.ChucVu,
                    SDT = nguoiDung.SDT,
                    Avatar = nguoiDung.Anh,
                    VaiTro = nguoiDung.VaiTro
                };

                // 2. Sự kiện được phân công (TOP 3, sắp xếp theo ngày gần nhất)
                var now = DateOnly.FromDateTime(DateTime.Now);
                var suKienPhanCong = await _context.PhanCongTinhNguyenViens
                    .Include(pc => pc.SuKien)
                    .Where(pc => pc.TinhNguyenVienId == tnv.TinhNguyenVienId)
                    .OrderBy(pc => pc.SuKien.NgayBatDau)
                    .Take(3)
                    .Select(pc => new SuKienPhanCongDTO
                    {
                        SuKienId = pc.SuKienId,
                        TenSuKien = pc.SuKien.TenSuKien,
                        CongViec = pc.CongViec,
                        NgayPhanCong = pc.NgayPhanCong.Value,
                        NgayBatDau = pc.SuKien.NgayBatDau.Value,
                        NgayKetThuc = pc.SuKien.NgayKetThuc.Value,
                        DiaDiem = pc.SuKien.DiaDiem,
                        TrangThai = pc.SuKien.NgayKetThuc < now ? "Đã kết thúc" :
                                   pc.SuKien.NgayBatDau > now ? "Sắp diễn ra" : "Đang diễn ra"
                    })
                    .ToListAsync();

                // 3. Thống kê hoạt động
                var tongSuKienThamGia = await _context.DangKySuKiens
                    .Where(dk => dk.UserId == userId && dk.TrangThai == "Đã duyệt")
                    .CountAsync();

                var suKienGanDay = await _context.DangKySuKiens
                    .Include(dk => dk.SuKien)
                    .Where(dk => dk.UserId == userId && dk.TrangThai == "Đã duyệt")
                    .OrderByDescending(dk => dk.SuKien.NgayBatDau)
                    .Take(3)
                    .Select(dk => new SuKienDaThamGiaDTO
                    {
                        SuKienId = dk.SuKienId,
                        TenSuKien = dk.SuKien.TenSuKien,
                        NguoiChiuTrachNhiem = dk.SuKien.NguoiChiuTrachNhiem,
                        MoTa = dk.SuKien.MoTa,
                        DiaDiem = dk.SuKien.DiaDiem,
                        NgayBatDau = dk.SuKien.NgayBatDau.Value,
                        NgayKetThuc = dk.SuKien.NgayKetThuc.Value
                    })
                    .ToListAsync();

                var thongKe = new ThongKeHoatDongDTO
                {
                    TongSuKienThamGia = tongSuKienThamGia,
                    SuKienGanDay = suKienGanDay
                };

                // 4. Lịch trống
                var lichTrong = await _context.LichTrongs
                    .Include(lt => lt.ChiTietLichTrongs)
                    .FirstOrDefaultAsync(lt => lt.TinhNguyenVienId == tnv.TinhNguyenVienId);

                var lichTrongDTO = new LichTrongDTO
                {
                    LichTrongId = lichTrong?.LichTrongId,
                    IsEmpty = lichTrong == null || !lichTrong.ChiTietLichTrongs.Any(),
                    ChiTietLichTrong = GenerateFullWeekSchedule(lichTrong)
                };

                // Response tổng hợp
                var response = new TinhNguyenVienHomeDTO
                {
                    ThongTinTaiKhoan = thongTinTaiKhoan,
                    SuKienPhanCong = suKienPhanCong,
                    ThongKe = thongKe,
                    LichTrong = lichTrongDTO
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // TAB TRANG CHỦ API: GET api/Mobile/TinhNguyenVien/LichTrong
        /// <summary>
        /// Lấy thông tin cho trang lịch trống:
        ///     + Thông tin lịch trống 7 ngày x 3 buổi (Sẽ sửa thành 2 buổi sau)
        /// </summary>
        /// <returns>
        ///     + Lịch trống 7 ngày x 3 buổi (Sẽ sửa lại thành 2 buổi) 'LichTrongDTO'
        /// </returns>
        [HttpGet("LichTrong")]
        public async Task<IActionResult> GetLichTrong()
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                var tnv = await _context.TinhNguyenViens
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                if (tnv == null)
                    return NotFound(new { message = "Không tìm thấy thông tin tình nguyện viên" });

                var lichTrong = await _context.LichTrongs
                    .Include(lt => lt.ChiTietLichTrongs)
                    .FirstOrDefaultAsync(lt => lt.TinhNguyenVienId == tnv.TinhNguyenVienId);

                var lichTrongDTO = new LichTrongDTO
                {
                    LichTrongId = lichTrong?.LichTrongId,
                    IsEmpty = lichTrong == null || !lichTrong.ChiTietLichTrongs.Any(),
                    ChiTietLichTrong = GenerateFullWeekSchedule(lichTrong)
                };

                return Ok(lichTrongDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // TAB TRANG CHỦ API: PUT api/Mobile/TinhNguyenVien/LichTrong
        /// <summary>
        /// Cập nhật thông tin cho trang lịch trống:
        ///     + Thông tin lịch trống 7 ngày x 3 buổi (Sẽ sửa thành 2 buổi sau)
        /// </summary>
        /// <returns>
        ///     + Lịch trống mới vẫn là 7 ngày x 3 buổi (Sẽ sửa lại thành 2 buổi) 'LichTrongDTO'
        /// </returns>
        [HttpPut("LichTrong")]
        public async Task<IActionResult> UpdateLichTrong([FromBody] UpdateLichTrongDTO request)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                var tnv = await _context.TinhNguyenViens
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                if (tnv == null)
                    return NotFound(new { message = "Không tìm thấy thông tin tình nguyện viên" });

                // Lấy hoặc tạo mới LichTrong
                var lichTrong = await _context.LichTrongs
                    .Include(lt => lt.ChiTietLichTrongs)
                    .FirstOrDefaultAsync(lt => lt.TinhNguyenVienId == tnv.TinhNguyenVienId);

                if (lichTrong == null)
                {
                    lichTrong = new LichTrong
                    {
                        TinhNguyenVienId = tnv.TinhNguyenVienId,
                        ChiTietLichTrongs = new List<ChiTietLichTrong>()
                    };
                    _context.LichTrongs.Add(lichTrong);
                }
                else
                {
                    // Xóa chi tiết cũ
                    _context.ChiTietLichTrongs.RemoveRange(lichTrong.ChiTietLichTrongs);
                }

                // Thêm chi tiết mới (chỉ lưu các ô được chọn = rảnh)
                foreach (var item in request.ChiTietLichTrong.Where(x => x.IsAvailable))
                {
                    lichTrong.ChiTietLichTrongs.Add(new ChiTietLichTrong
                    {
                        Thu = item.Thu,
                        Buoi = item.Buoi
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật lịch trống thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // Helper: Tạo lịch đầy đủ 7 ngày x 3 buổi
        private List<ChiTietLichTrongDTO> GenerateFullWeekSchedule(LichTrong lichTrong)
        {
            var days = new[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
            //var sessions = new[] { "Sáng", "Chiều", "Tối" };
            var sessions = new[] { "Sáng", "Chiều"};
            var result = new List<ChiTietLichTrongDTO>();

            foreach (var day in days)
            {
                foreach (var session in sessions)
                {
                    var isAvailable = lichTrong?.ChiTietLichTrongs
                        .Any(ct => ct.Thu == day && ct.Buoi == session) ?? false;

                    result.Add(new ChiTietLichTrongDTO
                    {
                        Thu = day,
                        Buoi = session,
                        IsAvailable = isAvailable
                    });
                }
            }

            return result;
        }


        // +------------------------------------------------------------------+
        // |    TAB SỰ KIỆN                                                   |
        // +------------------------------------------------------------------+
        // TAB SỰ KIỆN API GET: api/Mobile/TinhNguyenVien/DanhSachSuKien
        /// <summary>
        /// Lấy thông tin cho trang sự kiện bao gồm:
        ///     + Danh sách sự kiện có filter: CoTheDangKy, DaDangKy, DuocPhanCong, DaHetHan
        ///     + Danh sách sự kiện được tìm kiếm theo: Tên, địa điểm, người chịu trách nhiệm
        /// </summary>
        /// <returns>
        ///     + Danh sách sự kiện theo 'DanhSachSuKienDTO'
        /// </returns>
        [HttpGet("DanhSachSuKien")]
        public async Task<ActionResult<IEnumerable<DanhSachSuKienDTO>>> GetDanhSachSuKien(
            [FromQuery] string? filter = "TatCa",
            [FromQuery] string? search = null)
        {

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Không tìm thấy userId trong token" });
            }

            // Lấy TinhNguyenVienID
            var tnv = await _context.TinhNguyenViens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tnv == null)
                return BadRequest(new { message = "Không tìm thấy thông tin tình nguyện viên" });

            var query = _context.SuKiens
                .Include(s => s.KhuPho)
                .Select(s => new
                {
                    SuKien = s,
                    DangKy = _context.DangKySuKiens
                        .FirstOrDefault(dk => dk.SuKienId == s.SuKienId && dk.UserId == userId),
                    PhanCong = _context.PhanCongTinhNguyenViens
                        .FirstOrDefault(pc => pc.SuKienId == s.SuKienId && pc.TinhNguyenVienId == tnv.TinhNguyenVienId),
                    SoLuongDaDangKy = _context.DangKySuKiens
                        .Count(dk => dk.SuKienId == s.SuKienId && dk.TrangThai != "Từ chối")
                });

            // Apply filter
            query = filter switch
            {
                "CoTheDangKy" => query.Where(x =>
                    x.DangKy == null &&
                    x.SuKien.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Now) &&
                    x.SoLuongDaDangKy < x.SuKien.SoLuongTinhNguyenVien),
                "DaDangKy" => query.Where(x => x.DangKy != null),
                "DuocPhanCong" => query.Where(x => x.PhanCong != null),
                "DaHetHan" => query.Where(x => x.SuKien.NgayKetThuc < DateOnly.FromDateTime(DateTime.Now)),
                _ => query
            };

            // Apply search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.SuKien.TenSuKien.Contains(search) ||
                    x.SuKien.DiaDiem.Contains(search) ||
                    x.SuKien.NguoiChiuTrachNhiem.Contains(search));
            }

            // Order by NgayBatDau
            query = query.OrderBy(x => x.SuKien.NgayBatDau);

            var result = await query
                .Select(x => new DanhSachSuKienDTO
                {
                    SuKienId = x.SuKien.SuKienId,
                    TenSuKien = x.SuKien.TenSuKien,
                    MoTa = x.SuKien.MoTa,
                    DiaDiem = x.SuKien.DiaDiem,
                    NgayBatDau = x.SuKien.NgayBatDau,
                    NgayKetThuc = x.SuKien.NgayKetThuc,
                    SoLuongTinhNguyenVien = x.SuKien.SoLuongTinhNguyenVien,
                    SoLuongTreEm = x.SuKien.SoLuongTreEm,
                    NguoiChiuTrachNhiem = x.SuKien.NguoiChiuTrachNhiem,
                    TenKhuPho = x.SuKien.KhuPho.TenKhuPho,
                    DangKyId = x.DangKy != null ? x.DangKy.DangKySuKienId : null,
                    TrangThaiDangKy = x.DangKy != null ? x.DangKy.TrangThai : null,
                    CongViecPhanCong = x.PhanCong != null ? x.PhanCong.CongViec : null,
                    DaPhanCong = x.PhanCong != null,
                    SoLuongDaDangKy = x.SoLuongDaDangKy
                })
                .ToListAsync();

            return Ok(result);
        }

        // TAB SỰ KIỆN API GET: api/Mobile/TinhNguyenVien/5/ChiTietSuKien
        /// <summary>
        /// Lấy thông tin chi tiết cho mỗi sự kiện theo id ở trang sự kiện 
        /// </summary>
        /// <returns>
        ///     + Thông tin chi tiết theo id 'ThoiGianChiTietDTO' , 'ChiTietSuKienDTO'
        /// </returns>
        [HttpGet("{id}/ChiTietSuKien")]
        public async Task<ActionResult<ChiTietSuKienDTO>> GetChiTietSuKien(int id)
        {

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Không tìm thấy userId trong token" });
            }

            var tnv = await _context.TinhNguyenViens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tnv == null)
                return BadRequest(new { message = "Không tìm thấy thông tin tình nguyện viên" });

            var suKien = await _context.SuKiens
                .Include(s => s.KhuPho)
                .FirstOrDefaultAsync(s => s.SuKienId == id);

            if (suKien == null)
                return NotFound(new { message = "Không tìm thấy sự kiện" });

            var dangKy = await _context.DangKySuKiens
                .FirstOrDefaultAsync(dk => dk.SuKienId == id && dk.UserId == userId);

            var phanCong = await _context.PhanCongTinhNguyenViens
                .FirstOrDefaultAsync(pc => pc.SuKienId == id && pc.TinhNguyenVienId == tnv.TinhNguyenVienId);

            var thoiGianChiTiet = await _context.ThoiGianChiTietSuKiens
                .Where(t => t.SuKienId == id)
                .OrderBy(t => t.ThoiGianBatDau)
                .Select(t => new QuanLyTreEmAPI.DTOs.TInhNguyenVien.ThoiGianChiTietDtoo
                {
                    ThoiGianChiTietSuKienId = t.ThoiGianChiTietSuKienId,
                    MoTa = t.MoTa,
                    ThoiGianBatDau = t.ThoiGianBatDau,
                    ThoiGianKetThuc = t.ThoiGianKetThuc
                })
                .ToListAsync();

            var soLuongDaDangKy = await _context.DangKySuKiens
                .CountAsync(dk => dk.SuKienId == id && dk.TrangThai != "Từ chối");

            var result = new ChiTietSuKienDTO
            {
                SuKienId = suKien.SuKienId,
                TenSuKien = suKien.TenSuKien,
                MoTa = suKien.MoTa,
                DiaDiem = suKien.DiaDiem,
                NgayBatDau = suKien.NgayBatDau,
                NgayKetThuc = suKien.NgayKetThuc,
                SoLuongTinhNguyenVien = suKien.SoLuongTinhNguyenVien,
                SoLuongTreEm = suKien.SoLuongTreEm,
                NguoiChiuTrachNhiem = suKien.NguoiChiuTrachNhiem,
                TenKhuPho = suKien.KhuPho.TenKhuPho,
                DiaChiKhuPho = suKien.KhuPho.DiaChi,
                DangKyId = dangKy?.DangKySuKienId,
                TrangThaiDangKy = dangKy?.TrangThai,
                NgayDangKy = dangKy?.NgayDangKy,
                CongViecPhanCong = phanCong?.CongViec,
                GhiChuPhanCong = phanCong?.GhiChu,
                NgayPhanCong = phanCong?.NgayPhanCong,
                ThoiGianChiTiet = thoiGianChiTiet,
                SoLuongDaDangKy = soLuongDaDangKy
            };

            return Ok(result);
        }

        // TAB SỰ KIỆN API POST: api/Mobile/TinhNguyenVien/DangKySuKien
        /// <summary>
        /// TẠO thông tin đăng ký cho việc đăng ký sự kiện
        /// </summary>
        /// <returns>
        ///     + Thông tin đăng ký theo 'DangKySuKien'
        /// </returns>
        [HttpPost("DangKySuKien")]
        public async Task<IActionResult> DangKySuKien([FromBody] DangKySuKienRequestDtoo request)
        {

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Không tìm thấy userId trong token" });
            }

            // Kiểm tra sự kiện tồn tại
            var suKien = await _context.SuKiens.FindAsync(request.SuKienId);
            if (suKien == null)
                return NotFound(new { message = "Không tìm thấy sự kiện" });

            // Kiểm tra đã hết hạn
            if (suKien.NgayKetThuc < DateOnly.FromDateTime(DateTime.Now))
                return BadRequest(new { message = "Sự kiện đã kết thúc" });

            // Kiểm tra đã đăng ký chưa
            var daDangKy = await _context.DangKySuKiens
                .AnyAsync(dk => dk.SuKienId == request.SuKienId && dk.UserId == userId);

            if (daDangKy)
                return BadRequest(new { message = "Bạn đã đăng ký sự kiện này" });

            // Kiểm tra số lượng
            var soLuongDaDangKy = await _context.DangKySuKiens
                .CountAsync(dk => dk.SuKienId == request.SuKienId && dk.TrangThai != "Từ chối");

            if (soLuongDaDangKy >= suKien.SoLuongTinhNguyenVien)
                return BadRequest(new { message = "Đã đủ tình nguyện viên. Vui lòng đăng ký sự kiện khác" });

            // Tạo đăng ký mới
            var dangKy = new DangKySuKien
            {
                SuKienId = request.SuKienId,
                UserId = userId,
                NgayDangKy = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = "Chờ duyệt"
            };

            _context.DangKySuKiens.Add(dangKy);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", dangKyId = dangKy.DangKySuKienId });
        }

        // TAB SỰ KIỆN API POST: api/Mobile/TinhNguyenVien/HuyDangKySuKien
        /// <summary>
        /// HỦY thông tin đăng ký cho việc đăng ký sự kiện
        ///     + Kiểm tra đơn đăng ký đã được duyệt hay chưa, nếu được duyệt thì không hủy được
        /// </summary>
        /// <returns>
        ///     + Trả về message 
        /// </returns>
        [HttpPost("HuyDangKySuKien")]
        public async Task<IActionResult> HuyDangKySuKien([FromBody] HuyDangKySuKienRequestDTO request)
        {

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Không tìm thấy userId trong token" });
            }

            var dangKy = await _context.DangKySuKiens
                .FirstOrDefaultAsync(dk => dk.DangKySuKienId == request.DangKyId && dk.UserId == userId);

            if (dangKy == null)
                return NotFound(new { message = "Không tìm thấy đăng ký" });

            // Kiểm tra trạng thái đã duyệt
            if (dangKy.TrangThai == "Đã duyệt")
                return BadRequest(new { message = "Không thể hủy đăng ký đã được duyệt. Vui lòng liên hệ ban tổ chức" });

            // Xóa đăng ký (hoặc update trạng thái thành "Đã hủy" nếu muốn lưu lịch sử)
            _context.DangKySuKiens.Remove(dangKy);

            // Lưu lý do hủy vào log nếu cần (tùy yêu cầu)
            // Có thể tạo bảng LogHuyDangKy để lưu lý do

            await _context.SaveChangesAsync();

            return Ok(new { message = "Hủy đăng ký thành công" });
        }

        // +------------------------------------------------------------------+
        // |    TAB TRẺ EM                                                    |
        // +------------------------------------------------------------------+
        // TAB TRẺ EM API GET: api/Mobile/TinhNguyenVien/DanhSachTreEm
        /// <summary>
        /// Trả về danh sách trẻ em theo 2 loại
        ///     + Trẻ em cần vận động
        ///     + Trẻ em được phát phúc lợi
        /// </summary>
        /// <returns>
        /// Trả về danh sách trẻ em: 
        ///     + 'TreEmCanVanDongDTO'
        ///     + 'TreEmPhatHoTroDTO'
        /// </returns>
        [HttpGet("DanhSachTreEm")]
        public async Task<IActionResult> GetDanhSachTreEm([FromQuery] string? filter = "TatCa", [FromQuery] string? search = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                // Lấy thông tin TNV
                var tnv = await _context.TinhNguyenViens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                if (tnv == null)
                    return NotFound(new { message = "Không tìm thấy tình nguyện viên" });

                // 1. TRẺ CẦN VẬN ĐỘNG (GIỮ NGUYÊN)
                var allVanDong = await _context.VanDongTreEms
                    .Where(v => v.NguoiDungId == userId)
                    .OrderByDescending(v => v.NgayVanDong)
                    .ToListAsync();

                var vanDongGrouped = allVanDong
                    .GroupBy(v => v.TreEmId)
                    .Select(g => new
                    {
                        TreEmId = g.Key,
                        TinhTrangMoiNhat = g.First().TinhTrangCapNhat ?? "Chưa cập nhật",
                        TongSoLan = g.Sum(x => x.SoLan)
                    })
                    .ToList();

                var allTreEm = await _context.TreEms
                    .Include(t => t.KhuPho)
                    .ToListAsync();

                var allPhuHuynh = await _context.TreEmPhuHuynhs
                    .Include(tp => tp.PhuHuynh)
                    .ToListAsync();

                var treCanVanDong = vanDongGrouped.Select(vd =>
                {
                    var treEm = allTreEm.FirstOrDefault(t => t.TreEmId == vd.TreEmId);
                    if (treEm == null) return null;

                    var phuHuynh = allPhuHuynh
                        .FirstOrDefault(tp => tp.TreEmId == vd.TreEmId)
                        ?.PhuHuynh;

                    return new TreEmCanVanDongDTO
                    {
                        TreEmID = treEm.TreEmId,
                        HoTen = treEm.HoTen,
                        NgaySinh = treEm.NgaySinh,
                        GioiTinh = treEm.GioiTinh,
                        DiaChi = phuHuynh?.DiaChi ?? "",
                        TinhTrang = vd.TinhTrangMoiNhat,
                        SoLanVanDong = vd.TongSoLan,
                        Anh = treEm.Anh
                    };
                })
                .Where(dto => dto != null)
                .ToList();

                // 2. TRẺ PHÂN PHÁT QUÀ (MỚI)
                var tenNguoiDung = tnv.User.HoTen;

                var queryPhanPhat = _context.PhanPhatQuas
                    .Include(pp => pp.QuaTangUngHo)
                        .ThenInclude(qt => qt.SuKien)
                    .Include(pp => pp.TreEm)
                        .ThenInclude(te => te.KhuPho)
                    .Include(pp => pp.TreEm)
                        .ThenInclude(te => te.TreEmPhuHuynhs)
                            .ThenInclude(tp => tp.PhuHuynh)
                    .Where(pp => pp.NguoiPhanPhat == tenNguoiDung)
                    .AsQueryable();

                // Apply filter
                switch (filter)
                {
                    case "DaNhan":
                        queryPhanPhat = queryPhanPhat.Where(pp => pp.TrangThai == "Đã nhận");
                        break;
                    case "DangTienHanh":
                        queryPhanPhat = queryPhanPhat.Where(pp => pp.TrangThai == "Đang tiến hành");
                        break;
                        // "TatCa" - không filter
                }

                // Apply search
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    queryPhanPhat = queryPhanPhat.Where(pp =>
                        pp.TreEm.HoTen.ToLower().Contains(search) ||
                        pp.TreEm.KhuPho.TenKhuPho.ToLower().Contains(search));
                }

                var trePhanPhatQua = await queryPhanPhat
                    .OrderByDescending(pp => pp.NgayPhanPhat)
                    .Select(pp => new TreEmPhanPhatQuaDTO
                    {
                        PhanPhatID = pp.PhanPhatId,

                        TreEmID = pp.TreEmId,
                        HoTen = pp.TreEm.HoTen,
                        NgaySinh = pp.TreEm.NgaySinh,
                        GioiTinh = pp.TreEm.GioiTinh,
                        DiaChi = pp.TreEm.TreEmPhuHuynhs
                            .Select(tp => tp.PhuHuynh.DiaChi)
                            .FirstOrDefault() ?? "",
                        Anh = pp.TreEm.Anh,

                        QuaTangUngHoID = pp.QuaTangUngHoId,
                        TenQua = pp.QuaTangUngHo.TenQua,
                        MoTaQua = pp.QuaTangUngHo.MoTa,
                        AnhQua = pp.QuaTangUngHo.Anh,

                        SuKienID = pp.QuaTangUngHo.SuKienId ?? 0,
                        TenSuKien = pp.QuaTangUngHo.SuKien != null
                            ? pp.QuaTangUngHo.SuKien.TenSuKien
                            : "",
                        SoLuongNhan = pp.SoLuongNhan,
                        NgayPhanPhat = DateOnly.FromDateTime(pp.NgayPhanPhat),
                        NguoiPhanPhat = pp.NguoiPhanPhat,
                        TrangThai = pp.TrangThai ?? "Đang tiến hành",
                        GhiChu = pp.GhiChu,

                        TenKhuPho = pp.TreEm.KhuPho.TenKhuPho
                    })
                    .ToListAsync();

                var response = new DanhSachTreEmResponseDTO
                {
                    TreCanVanDong = treCanVanDong,
                    TrePhanPhatQua = trePhanPhatQua
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================================================
        // CHI TIẾT TRẺ EM PHÂN PHÁT QUÀ
        // ============================================================================
        [HttpGet("PhanPhatQua/{phanPhatId}")]
        public async Task<IActionResult> GetChiTietTreEmPhanPhatQua(int phanPhatId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                // Lấy thông tin TNV
                var tnv = await _context.TinhNguyenViens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                if (tnv == null)
                    return NotFound(new { message = "Không tìm thấy tình nguyện viên" });

                // Lấy thông tin phân phát
                var phanPhat = await _context.PhanPhatQuas
                    .Include(pp => pp.TreEm)
                        .ThenInclude(te => te.TreEmPhuHuynhs)
                            .ThenInclude(tp => tp.PhuHuynh)
                    .Include(pp => pp.QuaTangUngHo)
                        .ThenInclude(qt => qt.SuKien)
                    .FirstOrDefaultAsync(pp => pp.PhanPhatId == phanPhatId
                        && pp.NguoiPhanPhat == tnv.User.HoTen);

                if (phanPhat == null)
                    return NotFound(new { message = "Không tìm thấy thông tin phân phát quà" });

                var treEm = phanPhat.TreEm;
                var quaTang = phanPhat.QuaTangUngHo;
                var suKien = quaTang.SuKien;

                // Danh sách phụ huynh
                var phuHuynh = treEm.TreEmPhuHuynhs
                    .Select(tp => new ThongTinPhuHuynhDTO
                    {
                        PhuHuynhID = tp.PhuHuynh.PhuHuynhId,
                        HoTen = tp.PhuHuynh.HoTen,
                        SDT = tp.PhuHuynh.Sdt,
                        DiaChi = tp.PhuHuynh.DiaChi,
                        NgheNghiep = tp.PhuHuynh.NgheNghiep,
                        MoiQuanHe = tp.MoiQuanHe
                    })
                    .ToList();

                // Lịch sử phân phát quà cho trẻ này
                var lichSuPhanPhat = await _context.PhanPhatQuas
                    .Include(pp => pp.QuaTangUngHo)
                        .ThenInclude(qt => qt.SuKien)
                    .Where(pp => pp.TreEmId == treEm.TreEmId)
                    .OrderByDescending(pp => pp.NgayPhanPhat)
                    .Select(pp => new LichSuPhanPhatQuaDTO
                    {
                        PhanPhatID = pp.PhanPhatId,
                        TenQua = pp.QuaTangUngHo.TenQua,
                        TenSuKien = pp.QuaTangUngHo.SuKien != null
                            ? pp.QuaTangUngHo.SuKien.TenSuKien
                            : "",
                        SoLuongNhan = pp.SoLuongNhan ,
                        NgayPhanPhat = DateOnly.FromDateTime(pp.NgayPhanPhat),
                        TrangThai = pp.TrangThai ?? "Đang tiến hành",
                        GhiChu = pp.GhiChu,
                        AnhQua = pp.QuaTangUngHo.Anh
                    })
                    .ToListAsync();

                var response = new ChiTietTreEmPhanPhatQuaDTO
                {
                    TreEmID = treEm.TreEmId,
                    HoTen = treEm.HoTen,
                    NgaySinh = treEm.NgaySinh,
                    GioiTinh = treEm.GioiTinh,
                    TonGiao = treEm.TonGiao ?? "",
                    DanToc = treEm.DanToc ?? "",
                    Anh = treEm.Anh,

                    DanhSachPhuHuynh = phuHuynh,

                    PhanPhatID = phanPhat.PhanPhatId,
                    QuaTangUngHoID = quaTang.QuaTangUngHoId,
                    TenQua = quaTang.TenQua,
                    MoTaQua = quaTang.MoTa,
                    AnhQua = quaTang.Anh,
                    DonGia = quaTang.DonGia ,
                    SoLuongNhan = phanPhat.SoLuongNhan,
                    NgayPhanPhat = DateOnly.FromDateTime(phanPhat.NgayPhanPhat),
                    NguoiPhanPhat = phanPhat.NguoiPhanPhat,
                    TrangThai = phanPhat.TrangThai ?? "Đang tiến hành",
                    GhiChu = phanPhat.GhiChu,

                    SuKienID = suKien?.SuKienId ?? 0,
                    TenSuKien = suKien?.TenSuKien ?? "",
                    NgayBatDau = suKien?.NgayBatDau ?? DateOnly.FromDateTime(DateTime.Now),
                    NgayKetThuc = suKien?.NgayKetThuc ?? DateOnly.FromDateTime(DateTime.Now),
                    DiaDiem = suKien?.DiaDiem ?? "",

                    SoLuongTong = quaTang.SoLuongTong ,
                    SoLuongConLai = quaTang.SoLuongConLai,
                    DoiTuongNhan = quaTang.DoiTuongNhan ?? "",

                    LichSuPhanPhatQua = lichSuPhanPhat
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================================================
        // CẬP NHẬT PHÂN PHÁT QUÀ
        // ============================================================================
        [HttpPut("CapNhatPhanPhatQua")]
        public async Task<IActionResult> CapNhatPhanPhatQua([FromBody] CapNhatPhanPhatQuaRequestDTO request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                // Validate trạng thái
                if (request.TrangThai != "Đã nhận" && request.TrangThai != "Đang tiến hành")
                    return BadRequest(new { message = "Trạng thái không hợp lệ. Chỉ chấp nhận 'Đã nhận' hoặc 'Đang tiến hành'" });

                // Validate ngày phân phát <= ngày hiện tại
                if (request.NgayPhanPhat > DateOnly.FromDateTime(DateTime.Now))
                    return BadRequest(new { message = "Ngày phân phát không được lớn hơn ngày hiện tại" });

                // Lấy thông tin TNV
                var tnv = await _context.TinhNguyenViens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                if (tnv == null)
                    return NotFound(new { message = "Không tìm thấy tình nguyện viên" });

                // Lấy phân phát
                var phanPhat = await _context.PhanPhatQuas
                    .Include(pp => pp.QuaTangUngHo)
                    .FirstOrDefaultAsync(pp => pp.PhanPhatId == request.PhanPhatID
                        && pp.NguoiPhanPhat == tnv.User.HoTen);

                if (phanPhat == null)
                    return NotFound(new { message = "Không tìm thấy phân phát quà hoặc bạn không có quyền cập nhật" });

                // Kiểm tra: Không cho phép chuyển từ "Đã nhận" về "Đang tiến hành"
                if (phanPhat.TrangThai == "Đã nhận" && request.TrangThai == "Đang tiến hành")
                    return BadRequest(new { message = "Không thể chuyển từ 'Đã nhận' về 'Đang tiến hành'" });

                var quaTang = phanPhat.QuaTangUngHo;
                var soLuongNhan = phanPhat.SoLuongNhan;

                // Nếu đang chuyển sang "Đã nhận"
                if (phanPhat.TrangThai != "Đã nhận" && request.TrangThai == "Đã nhận")
                {
                    // Kiểm tra số lượng còn lại
                    if (quaTang.SoLuongConLai < soLuongNhan)
                        return BadRequest(new { message = $"Số lượng quà không đủ. Còn lại: {quaTang.SoLuongConLai}, cần: {soLuongNhan}" });

                    // Cập nhật số lượng còn lại
                    quaTang.SoLuongConLai -= soLuongNhan;
                }

                // Cập nhật thông tin phân phát
                phanPhat.TrangThai = request.TrangThai;
                phanPhat.NgayPhanPhat = request.NgayPhanPhat.ToDateTime(TimeOnly.MinValue);
                phanPhat.GhiChu = request.GhiChu;

                await _context.SaveChangesAsync();

                return Ok(new CapNhatPhanPhatQuaResponseDTO
                {
                    Success = true,
                    Message = "Cập nhật phân phát quà thành công",
                    SoLuongConLai = quaTang.SoLuongConLai 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        // ============ API: CHI TIẾT TRẺ CẦN VẬN ĐỘNG ============

        [HttpGet("VanDongTreEm/{treEmId}")]
        public async Task<IActionResult> GetChiTietTreEmVanDong(int treEmId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                // Kiểm tra quyền truy cập
                var hasAccess = await _context.VanDongTreEms
                    .AnyAsync(v => v.TreEmId == treEmId && v.NguoiDungId == userId);

                if (!hasAccess)
                    return Forbid();

                // Lấy thông tin trẻ
                var treEm = await _context.TreEms
                    .Where(t => t.TreEmId == treEmId)
                    .FirstOrDefaultAsync();

                if (treEm == null)
                    return NotFound(new { message = "Không tìm thấy trẻ em" });

                // Lấy danh sách phụ huynh
                var phuHuynh = await _context.TreEmPhuHuynhs
                    .Where(tp => tp.TreEmId == treEmId)
                    .Include(tp => tp.PhuHuynh)
                    .Select(tp => new ThongTinPhuHuynhDTO
                    {
                        PhuHuynhID = tp.PhuHuynh.PhuHuynhId,
                        HoTen = tp.PhuHuynh.HoTen,
                        SDT = tp.PhuHuynh.Sdt,
                        DiaChi = tp.PhuHuynh.DiaChi,
                        NgheNghiep = tp.PhuHuynh.NgheNghiep,
                        MoiQuanHe = tp.MoiQuanHe
                    })
                    .ToListAsync();

                // Lấy hoàn cảnh
                var hoanCanhData = await _context.TreEmHoanCanhs
                    .Where(thc => thc.TreEmId == treEmId)
                    .Include(thc => thc.HoanCanh)
                    .FirstOrDefaultAsync();

                // Lấy lịch sử vận động
                var lichSuVanDong = await _context.VanDongTreEms
                    .Where(v => v.TreEmId == treEmId && v.NguoiDungId == userId)
                    .OrderByDescending(v => v.NgayVanDong)
                    .Select(v => new LichSuVanDongDTO
                    {
                        VanDongID = v.VanDongId,
                        TinhTrangCapNhat = v.TinhTrangCapNhat ?? "",
                        GhiChuChiTiet = v.GhiChuChiTiet ?? "",
                        AnhMinhChung = v.AnhMinhChung,
                        NgayCapNhat = v.NgayCapNhat ?? v.NgayVanDong,
                        SoLan = v.SoLan
                    })
                    .ToListAsync();

                var response = new ChiTietTreEmVanDongDTO
                {
                    TreEmID = treEm.TreEmId,
                    HoTen = treEm.HoTen,
                    NgaySinh = treEm.NgaySinh,
                    GioiTinh = treEm.GioiTinh,
                    TonGiao = treEm.TonGiao ?? "",
                    DanToc = treEm.DanToc ?? "",
                    Anh = treEm.Anh,
                    DanhSachPhuHuynh = phuHuynh,
                    HoanCanhID = hoanCanhData?.HoanCanhId ?? 0,
                    LoaiHoanCanh = hoanCanhData?.HoanCanh?.LoaiHoanCanh ?? "",
                    MoTaHoanCanh = hoanCanhData?.HoanCanh?.MoTa ?? "",
                    LichSuVanDong = lichSuVanDong,
                    TinhTrangHienTai = lichSuVanDong.FirstOrDefault()?.TinhTrangCapNhat,
                    TongSoLanVanDong = lichSuVanDong.Sum(l => l.SoLan)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============ API: CHI TIẾT TRẺ HỖ TRỢ PHÚC LỢI ============

        //[HttpGet("HoTroTreEm/{hoTroId}")]
        //public async Task<IActionResult> GetChiTietTreEmHoTro(int hoTroId)
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        if (userId == 0)
        //        {
        //            return Unauthorized(new { message = "Không tìm thấy userId trong token" });
        //        }

        //        // Lấy thông tin hỗ trợ và trẻ
        //        var hoTro = await _context.HoTroPhucLois
        //            .Include(h => h.TreEm)
        //                .ThenInclude(te => te.TreEmPhuHuynhs)
        //                    .ThenInclude(tp => tp.PhuHuynh)
        //            .Where(h => h.HoTroId == hoTroId && h.NguoiDungId == userId)
        //            .FirstOrDefaultAsync();

        //        if (hoTro == null)
        //            return NotFound(new { message = "Không tìm thấy hỗ trợ phúc lợi" });

        //        var treEm = hoTro.TreEm;

        //        // Lấy danh sách phụ huynh
        //        var phuHuynh = treEm.TreEmPhuHuynhs
        //            .Select(tp => new ThongTinPhuHuynhDTO
        //            {
        //                PhuHuynhID = tp.PhuHuynh.PhuHuynhId,
        //                HoTen = tp.PhuHuynh.HoTen,
        //                SDT = tp.PhuHuynh.Sdt,
        //                DiaChi = tp.PhuHuynh.DiaChi,
        //                NgheNghiep = tp.PhuHuynh.NgheNghiep,
        //                MoiQuanHe = tp.MoiQuanHe
        //            })
        //            .ToList();

        //        // Lấy lịch sử phát hỗ trợ (minh chứng)
        //        var lichSuPhat = await _context.PhieuMinhChungs
        //            .Where(m => m.HoTroId == hoTroId)
        //            .OrderByDescending(m => m.NgayCap)
        //            .Select(m => new LichSuHoTroDTO
        //            {
        //                MinhChungID = m.MinhChungId,
        //                TrangThaiPhat = hoTro.TrangThaiPhat ?? "Lần đầu",
        //                GhiChuTNV = hoTro.GhiChuTnv,
        //                AnhMinhChung = m.FilePath,
        //                NgayCap = m.NgayCap
        //            })
        //            .ToListAsync();

        //        var response = new ChiTietTreEmHoTroDTO
        //        {
        //            TreEmID = treEm.TreEmId,
        //            HoTen = treEm.HoTen,
        //            NgaySinh = treEm.NgaySinh,
        //            GioiTinh = treEm.GioiTinh,
        //            TonGiao = treEm.TonGiao ?? "",
        //            DanToc = treEm.DanToc ?? "",
        //            Anh = treEm.Anh,
        //            HoTroID = hoTro.HoTroId,
        //            LoaiHoTro = hoTro.LoaiHoTro,
        //            MoTaHoTro = hoTro.MoTa,
        //            TrangThaiPhat = hoTro.TrangThaiPhat ?? "Lần đầu",
        //            NgayHenLai = hoTro.NgayHenLai,
        //            DanhSachPhuHuynh = phuHuynh,
        //            LichSuPhatHoTro = lichSuPhat
        //        };

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        //    }
        //}


        // ============ API: CẬP NHẬT VẬN ĐỘNG ============

        [HttpPost("CapNhatVanDong")]
        public async Task<IActionResult> CapNhatVanDong([FromForm] CapNhatVanDongRequestDTO request, IFormFile? file)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                // Validate
                if (string.IsNullOrEmpty(request.TinhTrangCapNhat))
                    return BadRequest(new { message = "Vui lòng chọn tình trạng" });

                // Upload ảnh (tùy chọn)
                string? imagePath = null;
                if (file != null)
                {
                    var uploadResult = await UploadImage(file, "KetQuaUngHo");
                    if (!uploadResult.Success)
                        return BadRequest(new { message = uploadResult.Message });
                    imagePath = uploadResult.FilePath;
                }

                // Tạo bản ghi vận động mới
                var vanDong = new VanDongTreEm
                {
                    TreEmId = request.TreEmID,
                    HoanCanhId = request.HoanCanhID,
                    NguoiDungId = userId,
                    SoLan = request.SoLan,
                    TinhTrangCapNhat = request.TinhTrangCapNhat,
                    GhiChuChiTiet = request.GhiChuChiTiet,
                    KetQua = request.GhiChuChiTiet, 
                    AnhMinhChung = imagePath,
                    NgayVanDong = DateOnly.FromDateTime(DateTime.Now),
                    NgayCapNhat = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.VanDongTreEms.Add(vanDong);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật vận động thành công",
                    vanDongId = vanDong.VanDongId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }


        // ============ API: CẬP NHẬT HỖ TRỢ PHÚC LỢI ============

        //[HttpPost("CapNhatHoTro")]
        //public async Task<IActionResult> CapNhatHoTro([FromForm] CapNhatHoTroRequestDTO request, IFormFile file)
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        if (userId == 0)
        //        {
        //            return Unauthorized(new { message = "Không tìm thấy userId trong token" });
        //        }

        //        // Validate
        //        if (file == null)
        //            return BadRequest(new { message = "Vui lòng chụp ảnh minh chứng" });

        //        if (string.IsNullOrEmpty(request.TrangThaiPhat))
        //            return BadRequest(new { message = "Vui lòng chọn trạng thái" });

        //        if (request.TrangThaiPhat == "Chưa nhận" && request.NgayHenLai == null)
        //            return BadRequest(new { message = "Vui lòng chọn ngày hẹn lại" });

        //        // Lấy thông tin hỗ trợ
        //        var hoTro = await _context.HoTroPhucLois
        //            .FirstOrDefaultAsync(h => h.HoTroId == request.HoTroID && h.NguoiDungId == userId);

        //        if (hoTro == null)
        //            return NotFound(new { message = "Không tìm thấy hỗ trợ phúc lợi" });

        //        // Upload ảnh (bắt buộc)
        //        var uploadResult = await UploadImage(file, "KetQuaHoTroPhucLoi");
        //        if (!uploadResult.Success)
        //            return BadRequest(new { message = uploadResult.Message });

        //        // Cập nhật trạng thái hỗ trợ
        //        hoTro.TrangThaiPhat = request.TrangThaiPhat;
        //        hoTro.NgayHenLai = request.NgayHenLai;
        //        hoTro.GhiChuTnv = request.GhiChuTNV;

        //        // Tạo minh chứng mới
        //        var minhChung = new PhieuMinhChung
        //        {
        //            LoaiMinhChung = "Ảnh phát hỗ trợ",
        //            FilePath = uploadResult.FilePath,
        //            NgayCap = DateOnly.FromDateTime(DateTime.Now),
        //            HoTroId = request.HoTroID
        //        };

        //        _context.PhieuMinhChungs.Add(minhChung);
        //        await _context.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            success = true,
        //            message = "Cập nhật hỗ trợ phúc lợi thành công",
        //            minhChungId = minhChung.MinhChungId
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        //    }
        //}


        // ============ HELPER: UPLOAD ẢNH ============

        private async Task<UploadImageResponseDTO> UploadImage(IFormFile file, string folderName)
        {
            try
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return new UploadImageResponseDTO
                    {
                        Success = false,
                        Message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png)"
                    };

                if (file.Length > 5 * 1024 * 1024) // 5MB
                    return new UploadImageResponseDTO
                    {
                        Success = false,
                        Message = "Kích thước file không được vượt quá 5MB"
                    };

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{extension}";
                var folderPath = Path.Combine(_env.WebRootPath, "Anh", "MinhChung", folderName);

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/Anh/MinhChung/{folderName}/{fileName}";

                return new UploadImageResponseDTO
                {
                    Success = true,
                    Message = "Upload ảnh thành công",
                    FilePath = relativePath
                };
            }
            catch (Exception ex)
            {
                return new UploadImageResponseDTO
                {
                    Success = false,
                    Message = $"Lỗi upload ảnh: {ex.Message}"
                };
            }
        }
        //==================================================
        // TAB THÔNG BÁO
        //==================================================
        [HttpGet("ThongBao")]
        public async Task<IActionResult> GetDanhSachThongBao([FromQuery] string? filter = "TatCa")
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Không xác định được người dùng" });

                // Query thông báo
                var query = from tb in _context.ThongBaos
                            join tbn in _context.ThongBaoNguoiDungs
                                on tb.ThongBaoId equals tbn.ThongBaoId
                            where tbn.UserId == userId
                            select new
                            {
                                tb,
                                tbn.DaDoc
                            };

                // Áp dụng filter
                switch (filter)
                {
                    case "ChuaDoc":
                        query = query.Where(x => !x.DaDoc);
                        break;
                    case "DaDoc":
                        query = query.Where(x => x.DaDoc);
                        break;
                    case "SuKien":
                        query = query.Where(x => x.tb.SuKienId != null);
                        break;
                    case "VanDong":
                        // Lọc theo từ khóa trong nội dung (vì chưa có cột riêng)
                        query = query.Where(x => x.tb.NoiDung.Contains("vận động")
                            || x.tb.NoiDung.Contains("trẻ em"));
                        break;
                }

                var result = await query
                    .OrderByDescending(x => x.tb.NgayThongBao)
                    .ThenBy(x => x.DaDoc) // Chưa đọc lên trước
                    .Select(x => new ThongBaoDTO
                    {
                        ThongBaoId = x.tb.ThongBaoId,
                        NoiDung = x.tb.NoiDung,
                        NgayThongBao = x.tb.NgayThongBao,
                        DaDoc = x.DaDoc,
                        SuKienId = x.tb.SuKienId,
                        // Xác định loại thông báo từ nội dung
                        LoaiThongBao = x.tb.SuKienId != null ? "SuKien" :
                            x.tb.NoiDung.Contains("vận động") ? "VanDong" :
                            x.tb.NoiDung.Contains("khẩn cấp") ? "KhanCap" : "ThongTin",
                        // Xác định mức độ ưu tiên
                        MucDoUuTien = x.tb.NoiDung.Contains("khẩn cấp") ||
                            x.tb.NoiDung.Contains("gấp") ? "URGENT" :
                            x.tb.NoiDung.Contains("quan trọng") ? "IMPORTANT" : "INFO",
                        TenSuKien = x.tb.SuKienId != null ?
                            _context.SuKiens
                                .Where(sk => sk.SuKienId == x.tb.SuKienId)
                                .Select(sk => sk.TenSuKien)
                                .FirstOrDefault() : null
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy số lượng thông báo chưa đọc
        /// </summary>
        [HttpGet("ThongBao/ChuaDoc/SoLuong")]
        public async Task<IActionResult> GetSoLuongChuaDoc()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Không xác định được người dùng" });

                var soLuong = await _context.ThongBaoNguoiDungs
                    .Where(tbn => tbn.UserId == userId && !tbn.DaDoc)
                    .CountAsync();

                return Ok(new { soLuong });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu thông báo đã đọc
        /// </summary>
        [HttpPut("ThongBao/{thongBaoId}/DaDoc")]
        public async Task<IActionResult> DanhDauDaDoc(int thongBaoId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Không xác định được người dùng" });

                var thongBaoNguoiDung = await _context.ThongBaoNguoiDungs
                    .FirstOrDefaultAsync(tbn => tbn.ThongBaoId == thongBaoId && tbn.UserId == userId);

                if (thongBaoNguoiDung == null)
                    return NotFound(new { message = "Không tìm thấy thông báo" });

                thongBaoNguoiDung.DaDoc = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã đánh dấu đọc" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo đã đọc
        /// </summary>
        [HttpPut("ThongBao/DaDoc/TatCa")]
        public async Task<IActionResult> DanhDauTatCaDaDoc()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Không xác định được người dùng" });

                await _context.ThongBaoNguoiDungs
                    .Where(tbn => tbn.UserId == userId && !tbn.DaDoc)
                    .ExecuteUpdateAsync(s => s.SetProperty(tbn => tbn.DaDoc, true));

                return Ok(new { message = "Đã đánh dấu tất cả đã đọc" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // ==================== TẠO THÔNG BÁO TỰ ĐỘNG ====================

        /// <summary>
        /// Tạo thông báo cho TNV khi có sự kiện mới
        /// Gọi từ API tạo sự kiện
        /// </summary>
        private async Task TaoThongBaoSuKienMoi(int suKienId, int khuPhoId)
        {
            try
            {
                var suKien = await _context.SuKiens
                    .FirstOrDefaultAsync(sk => sk.SuKienId == suKienId);

                if (suKien == null) return;

                // Tạo thông báo chung
                var thongBao = new ThongBao
                {
                    SuKienId = suKienId,
                    NoiDung = $"Sự kiện mới: {suKien.TenSuKien}\n" +
                              $"{suKien.DiaDiem}\n" +
                              $"{suKien.NgayBatDau:dd/MM/yyyy} - {suKien.NgayKetThuc:dd/MM/yyyy}\n" +
                              $"Cần {suKien.SoLuongTinhNguyenVien} tình nguyện viên\n" +
                              $"Đăng ký ngay!",
                    NgayThongBao = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();

                // Lấy danh sách TNV trong khu phố
                var tinhNguyenViens = await _context.TinhNguyenViens
                    .Where(tnv => tnv.KhuPhoId == khuPhoId)
                    .Select(tnv => tnv.UserId)
                    .ToListAsync();

                // Tạo bản ghi cho từng TNV
                foreach (var userId in tinhNguyenViens)
                {
                    _context.ThongBaoNguoiDungs.Add(new ThongBaoNguoiDung
                    {
                        ThongBaoId = thongBao.ThongBaoId,
                        UserId = (int)userId,
                        DaDoc = false
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Lỗi tạo thông báo sự kiện: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo thông báo khi phân công TNV
        /// </summary>
        private async Task TaoThongBaoPhanCong(int phanCongId)
        {
            try
            {
                var phanCong = await _context.PhanCongTinhNguyenViens
                    .Include(pc => pc.SuKien)
                    .Include(pc => pc.TinhNguyenVien)
                    .FirstOrDefaultAsync(pc => pc.PhanCongId == phanCongId);

                if (phanCong == null) return;

                var thongBao = new ThongBao
                {
                    SuKienId = phanCong.SuKienId,
                    NoiDung = $"Bạn được phân công công việc mới!\n" +
                              $"Công việc: {phanCong.CongViec}\n" +
                              $"Sự kiện: {phanCong.SuKien.TenSuKien}\n" +
                              $"Ngày: {phanCong.SuKien.NgayBatDau:dd/MM/yyyy}\n" +
                              $"Ghi chú: {phanCong.GhiChu ?? "Không có"}",
                    NgayThongBao = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();

                _context.ThongBaoNguoiDungs.Add(new ThongBaoNguoiDung
                {
                    ThongBaoId = thongBao.ThongBaoId,
                    UserId = (int)phanCong.TinhNguyenVien.UserId,
                    DaDoc = false
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo thông báo phân công: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo thông báo cảnh báo trẻ em nghỉ học
        /// Chạy định kỳ hoặc khi cập nhật vận động
        /// </summary>
        private async Task TaoThongBaoTreNghiHoc(int treEmId, int soNgayNghi)
        {
            try
            {
                var treEm = await _context.TreEms
                    .Include(te => te.KhuPho)
                    .FirstOrDefaultAsync(te => te.TreEmId == treEmId);

                if (treEm == null) return;

                // Lấy TNV phụ trách khu phố
                var tinhNguyenViens = await _context.TinhNguyenViens
                    .Where(tnv => tnv.KhuPhoId == treEm.KhuPhoId)
                    .Select(tnv => tnv.UserId)
                    .ToListAsync();

                var thongBao = new ThongBao
                {
                    NoiDung = $"[KHẨN CẤP] Trẻ em cần vận động!\n" +
                              $"Họ tên: {treEm.HoTen}\n" +
                              $"Khu phố: {treEm.KhuPho.TenKhuPho}\n" +
                              $"Đã nghỉ học: {soNgayNghi} ngày liên tục\n" +
                              $"Tình trạng: {treEm.TinhTrang}\n" +
                              $"Cần vận động ngay!",
                    NgayThongBao = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();

                foreach (var userId in tinhNguyenViens)
                {
                    _context.ThongBaoNguoiDungs.Add(new ThongBaoNguoiDung
                    {
                        ThongBaoId = thongBao.ThongBaoId,
                        UserId = (int)userId,
                        DaDoc = false
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo thông báo trẻ nghỉ học: {ex.Message}");
            }
        }

        /// <summary>
        /// Nhắc nhở sự kiện sắp diễn ra (chạy job hàng ngày)
        /// </summary>
        private async Task NhacNhoSuKienSapDienRa()
        {
            try
            {
                var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                var ngaySau3Ngay = ngayHienTai.AddDays(3);

                // Lấy sự kiện sắp diễn ra trong 3 ngày
                var suKienSapDienRa = await _context.SuKiens
                    .Where(sk => sk.NgayBatDau >= ngayHienTai && sk.NgayBatDau <= ngaySau3Ngay)
                    .ToListAsync();

                foreach (var suKien in suKienSapDienRa)
                {
                    // Lấy TNV đã đăng ký
                    var tinhNguyenVienDaDangKy = await _context.DangKySuKiens
                        .Where(dk => dk.SuKienId == suKien.SuKienId && dk.TrangThai == "Đã duyệt")
                        .Select(dk => dk.UserId)
                        .ToListAsync();

                    if (!tinhNguyenVienDaDangKy.Any()) continue;

                    var soNgayConLai = (suKien.NgayBatDau!.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Now).Days;

                    var thongBao = new ThongBao
                    {
                        SuKienId = suKien.SuKienId,
                        NoiDung = $"Nhắc nhở: Sự kiện sắp diễn ra!\n" +
                                  $"{suKien.TenSuKien}\n" +
                                  $"Còn {soNgayConLai} ngày nữa ({suKien.NgayBatDau:dd/MM/yyyy})\n" +
                                  $"Địa điểm: {suKien.DiaDiem}\n" +
                                  $"Hãy chuẩn bị sẵn sàng!",
                        NgayThongBao = ngayHienTai
                    };

                    _context.ThongBaos.Add(thongBao);
                    await _context.SaveChangesAsync();

                    foreach (var userId in tinhNguyenVienDaDangKy)
                    {
                        _context.ThongBaoNguoiDungs.Add(new ThongBaoNguoiDung
                        {
                            ThongBaoId = thongBao.ThongBaoId,
                            UserId = (int)userId,
                            DaDoc = false
                        });
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi nhắc nhở sự kiện: {ex.Message}");
            }
        }
        //==================================================
        // TAB TÀI KHOẢN
        //==================================================
        // File: Controllers/Mobile/TinhNguyenVienController.cs
        // Thêm các endpoint sau vào controller

        // ==========================================================================
        // LẤY DANH SÁCH KHU PHỐ
        // ==========================================================================
        [HttpGet("DanhSachKhuPho")]
        public async Task<IActionResult> GetDanhSachKhuPho()
        {
            try
            {
                var danhSach = await _context.KhuPhos
                    .Select(kp => new DanhSachKhuPhoDTO
                    {
                        KhuPhoId = kp.KhuPhoId,
                        TenKhuPho = kp.TenKhuPho,
                        DiaChi = kp.DiaChi,
                        QuanHuyen = kp.QuanHuyen,
                        ThanhPho = kp.ThanhPho
                    })
                    .OrderBy(kp => kp.TenKhuPho)
                    .ToListAsync();

                return Ok(danhSach);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách khu phố", error = ex.Message });
            }
        }

        // ==========================================================================
        // CẬP NHẬT THÔNG TIN TÀI KHOẢN
        // ==========================================================================
        [HttpPut("Profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDTO request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Lấy thông tin người dùng và TNV
                var nguoiDung = await _context.NguoiDungs
                    .Include(nd => nd.TinhNguyenVien)
                    .FirstOrDefaultAsync(nd => nd.UserId == userId);

                if (nguoiDung == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });

                var tinhNguyenVien = nguoiDung.TinhNguyenVien;
                if (tinhNguyenVien == null)
                    return NotFound(new { message = "Không tìm thấy thông tin tình nguyện viên" });

                // Validate số điện thoại
                if (!string.IsNullOrEmpty(request.Sdt))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(request.Sdt, @"^0\d{9}$"))
                        return BadRequest(new { message = "Số điện thoại không hợp lệ. Phải có 10 chữ số và bắt đầu bằng 0" });

                    // Kiểm tra trùng SDT
                    var existingSdt = await _context.NguoiDungs
                        .AnyAsync(nd => nd.SDT == request.Sdt && nd.UserId != userId);
                    if (existingSdt)
                        return BadRequest(new { message = "Số điện thoại đã được sử dụng bởi người dùng khác" });
                }

                // Validate email
                if (!string.IsNullOrEmpty(request.Email))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(request.Email,
                        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                        return BadRequest(new { message = "Email không hợp lệ" });

                    // Kiểm tra trùng email
                    var existingEmail = await _context.NguoiDungs
                        .AnyAsync(nd => nd.Email == request.Email && nd.UserId != userId);
                    if (existingEmail)
                        return BadRequest(new { message = "Email đã được sử dụng bởi người dùng khác" });
                }

                // Validate ngày sinh
                if (request.NgaySinh.HasValue)
                {
                    var age = DateTime.Now.Year - request.NgaySinh.Value.Year;

                    var compareDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-age));

                    if (request.NgaySinh.Value > compareDate) age--;

                    if (age < 15)
                        return BadRequest(new { message = "Tuổi phải từ 15 tuổi trở lên" });
                    if (age > 100)
                        return BadRequest(new { message = "Ngày sinh không hợp lệ" });
                }

                // Xử lý khu phố
                int? khuPhoId = null;
                if (request.KhuPhoId.HasValue && request.KhuPhoId.Value > 0)
                {
                    // Sử dụng khu phố có sẵn
                    khuPhoId = request.KhuPhoId.Value;

                    var khuPho = await _context.KhuPhos.FindAsync(khuPhoId);
                    if (khuPho == null)
                        return BadRequest(new { message = "Khu phố không tồn tại" });
                }
                else if (!string.IsNullOrWhiteSpace(request.TenKhuPhoMoi))
                {
                    // Tạo khu phố mới khi chọn "Khác"
                    var khuPhoMoi = new KhuPho
                    {
                        TenKhuPho = request.TenKhuPhoMoi.Trim(),
                        DiaChi = request.DiaChiKhuPho?.Trim(),
                        QuanHuyen = request.QuanHuyen?.Trim(),
                        ThanhPho = request.ThanhPho?.Trim()
                    };

                    _context.KhuPhos.Add(khuPhoMoi);
                    await _context.SaveChangesAsync();
                    khuPhoId = khuPhoMoi.KhuPhoId;
                }

                // Cập nhật thông tin NguoiDung
                nguoiDung.HoTen = request.HoTen?.Trim() ?? nguoiDung.HoTen;
                nguoiDung.SDT = request.Sdt?.Trim() ?? nguoiDung.SDT;
                nguoiDung.Email = request.Email?.Trim() ?? nguoiDung.Email;

                // Cập nhật thông tin TinhNguyenVien
                tinhNguyenVien.Sdt = request.Sdt?.Trim() ?? tinhNguyenVien.Sdt;
                tinhNguyenVien.NgaySinh = request.NgaySinh.HasValue
                    ? request.NgaySinh.Value.ToDateTime(TimeOnly.MinValue)  // chuyển DateOnly → DateTime
                    : tinhNguyenVien.NgaySinh;

                if (khuPhoId.HasValue)
                    tinhNguyenVien.KhuPhoId = khuPhoId.Value;

                await _context.SaveChangesAsync();

                // Lấy thông tin profile mới để trả về
                var updatedProfile = await GetProfileData(userId);

                return Ok(new
                {
                    message = "Cập nhật thông tin thành công",
                    profile = updatedProfile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin", error = ex.Message });
            }
        }

        // Helper method để lấy profile data
        private async Task<TinhNguyenVienProfileDTO> GetProfileData(int userId)
        {
            var result = await (from nd in _context.NguoiDungs
                                join tnv in _context.TinhNguyenViens on nd.UserId equals tnv.UserId
                                join kp in _context.KhuPhos on tnv.KhuPhoId equals kp.KhuPhoId into kpGroup
                                from kp in kpGroup.DefaultIfEmpty()
                                where nd.UserId == userId
                                select new TinhNguyenVienProfileDTO
                                {
                                    UserId = nd.UserId,
                                    HoTen = nd.HoTen,
                                    SDT = nd.SDT,
                                    Email = nd.Email,
                                    VaiTro = nd.VaiTro,
                                    Anh = nd.Anh,
                                    NgayTao = nd.NgayTao ?? DateOnly.FromDateTime(DateTime.Now),
                                    TinhNguyenVienID = tnv.TinhNguyenVienId,
                                    NgaySinh = tnv.NgaySinh.HasValue
                                            ? DateOnly.FromDateTime(tnv.NgaySinh.Value)
                                            : (DateOnly?)null,
                                    ChucVu = tnv.ChucVu,
                                    TenKhuPho = kp != null ? kp.TenKhuPho : null,
                                    DiaChiKhuPho = kp != null ? kp.DiaChi : null,
                                    QuanHuyen = kp != null ? kp.QuanHuyen : null,
                                    ThanhPho = kp != null ? kp.ThanhPho : null
                                }).FirstOrDefaultAsync();

            return result ?? new TinhNguyenVienProfileDTO();
        }


        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                var nguoiDung = await _context.NguoiDungs
                    .Include(n => n.TinhNguyenVien)
                        .ThenInclude(t => t.KhuPho)
                    .FirstOrDefaultAsync(n => n.UserId == userId);

                if (nguoiDung == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });

                var tnv = nguoiDung.TinhNguyenVien;
                if (tnv == null)
                    return NotFound(new { message = "Không tìm thấy thông tin tình nguyện viên" });

                var profile = new TinhNguyenVienProfileDTO
                {
                    UserId = nguoiDung.UserId,
                    HoTen = nguoiDung.HoTen,
                    SDT = nguoiDung.SDT,
                    Email = nguoiDung.Email,
                    VaiTro = nguoiDung.VaiTro,
                    Anh = nguoiDung.Anh,
                    NgayTao = nguoiDung.NgayTao,
                    TinhNguyenVienID = tnv.TinhNguyenVienId,
                    NgaySinh = tnv.NgaySinh.HasValue
                                ? DateOnly.FromDateTime(tnv.NgaySinh.Value)
                                : (DateOnly?)null,
                    ChucVu = tnv.ChucVu,
                    TenKhuPho = tnv.KhuPho?.TenKhuPho,
                    DiaChiKhuPho = tnv.KhuPho?.DiaChi,
                    QuanHuyen = tnv.KhuPho?.QuanHuyen,
                    ThanhPho = tnv.KhuPho?.ThanhPho
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ==========================================================================
        // GET LỊCH SỬ HOẠT ĐỘNG
        // ==========================================================================
        //[HttpGet("LichSuHoatDong")]
        //public async Task<IActionResult> GetLichSuHoatDong(
        //    [FromQuery] string? khuPho = null,
        //    [FromQuery] int page = 1,
        //    [FromQuery] int pageSize = 10)
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        if (userId == 0)
        //        {
        //            return Unauthorized(new { message = "Không tìm thấy userId trong token" });
        //        }

        //        var tnv = await _context.TinhNguyenViens
        //            .FirstOrDefaultAsync(t => t.UserId == userId);

        //        if (tnv == null)
        //            return NotFound(new { message = "Không tìm thấy tình nguyện viên" });

        //        // 1. Sự kiện đã tham gia
        //        var suKienQuery = _context.DangKySuKiens
        //            .Include(dk => dk.SuKien)
        //                .ThenInclude(sk => sk.KhuPho)
        //            .Where(dk => dk.UserId == userId
        //                && dk.TrangThai == "Đã duyệt"
        //                && dk.SuKien.NgayKetThuc < DateOnly.FromDateTime(DateTime.Now));

        //        if (!string.IsNullOrEmpty(khuPho))
        //        {
        //            suKienQuery = suKienQuery.Where(dk => dk.SuKien.KhuPho.TenKhuPho.Contains(khuPho));
        //        }

        //        var suKienList = await suKienQuery
        //            .Skip((page - 1) * pageSize)
        //            .Take(pageSize)
        //            .Select(dk => new SuKienTNVDaThamGiaDTO
        //            {
        //                SuKienID = dk.SuKienId,
        //                TenSuKien = dk.SuKien.TenSuKien,
        //                DiaDiem = dk.SuKien.DiaDiem,
        //                NgayBatDau = dk.SuKien.NgayBatDau,
        //                NgayKetThuc = dk.SuKien.NgayKetThuc,
        //                TenKhuPho = dk.SuKien.KhuPho.TenKhuPho,
        //                NgayDangKy = dk.NgayDangKy
        //            })
        //            .ToListAsync();

        //        // 2. Hỗ trợ phúc lợi đã phát
        //        var hoTroQuery = _context.HoTroPhucLois
        //            .Include(ht => ht.TreEm)
        //                .ThenInclude(te => te.KhuPho)
        //            .Where(ht => ht.NguoiDungId == userId
        //                && ht.TrangThaiPhat == "Đã phát thành công");

        //        if (!string.IsNullOrEmpty(khuPho))
        //        {
        //            hoTroQuery = hoTroQuery.Where(ht => ht.TreEm.KhuPho.TenKhuPho.Contains(khuPho));
        //        }

        //        var hoTroList = await hoTroQuery
        //            .Skip((page - 1) * pageSize)
        //            .Take(pageSize)
        //            .Select(ht => new HoTroPhucLoiDaPhatDTO
        //            {
        //                HoTroID = ht.HoTroId,
        //                LoaiHoTro = ht.LoaiHoTro,
        //                MoTa = ht.MoTa,
        //                NgayCap = ht.NgayCap,
        //                TenTreEm = ht.TreEm.HoTen,
        //                TenKhuPho = ht.TreEm.KhuPho.TenKhuPho,
        //                TrangThaiPhat = ht.TrangThaiPhat
        //            })
        //            .ToListAsync();

        //        // 3. Trẻ em đã vận động
        //        var vanDongQuery = _context.VanDongTreEms
        //            .Include(vd => vd.TreEm)
        //                .ThenInclude(te => te.KhuPho)
        //            .Include(vd => vd.HoanCanh)
        //            .Where(vd => vd.NguoiDungId == userId
        //                && vd.NgayVanDong < DateOnly.FromDateTime(DateTime.Now));

        //        if (!string.IsNullOrEmpty(khuPho))
        //        {
        //            vanDongQuery = vanDongQuery.Where(vd => vd.TreEm.KhuPho.TenKhuPho.Contains(khuPho));
        //        }

        //        var vanDongList = await vanDongQuery
        //            .Skip((page - 1) * pageSize)
        //            .Take(pageSize)
        //            .Select(vd => new TreEmDaVanDongDTO
        //            {
        //                VanDongID = vd.VanDongId,
        //                TenTreEm = vd.TreEm.HoTen,
        //                NgaySinh = vd.TreEm.NgaySinh,
        //                GioiTinh = vd.TreEm.GioiTinh,
        //                TenKhuPho = vd.TreEm.KhuPho.TenKhuPho,
        //                LoaiHoanCanh = vd.HoanCanh.LoaiHoanCanh,
        //                SoLan = vd.SoLan,
        //                LyDo = vd.LyDo,
        //                KetQua = vd.KetQua,
        //                NgayVanDong = vd.NgayVanDong,
        //                TinhTrangCapNhat = vd.TinhTrangCapNhat,
        //                GhiChuChiTiet = vd.GhiChuChiTiet
        //            })
        //            .ToListAsync();

        //        var result = new LichSuHoatDongDTO
        //        {
        //            SuKienDaThamGia = suKienList,
        //            HoTroPhucLoiDaPhat = hoTroList,
        //            TreEmDaVanDong = vanDongList,
        //            TotalSuKien = await suKienQuery.CountAsync(),
        //            TotalHoTro = await hoTroQuery.CountAsync(),
        //            TotalVanDong = await vanDongQuery.CountAsync(),
        //            Page = page,
        //            PageSize = pageSize
        //        };

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        //    }
        //}

        // ==========================================================================
        // CẬP NHẬT AVATAR
        // ==========================================================================
        [HttpPost("CapNhatAvatar")]
        public async Task<IActionResult> CapNhatAvatar( IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Vui lòng chọn file ảnh" });

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
                if (nguoiDung == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(nguoiDung.Anh))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, nguoiDung.Anh.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Lưu ảnh mới
                var uploadsFolder = Path.Combine(_env.WebRootPath, "Anh", "NguoiDung");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                nguoiDung.Anh = $"/Anh/NguoiDung/{uniqueFileName}";
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật ảnh đại diện thành công",
                    anh = nguoiDung.Anh
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }


    }
}
