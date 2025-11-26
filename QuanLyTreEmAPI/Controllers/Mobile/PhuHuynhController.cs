using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyTreEmAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.DTOs.PhuHuynh;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

namespace QuanLyTreEmAPI.Controllers.Mobile
{
    [ApiController]
    [Route("api/Mobile/[controller]")]
    public class PhuHuynhController : Controller
    {
        private readonly QuanLyTreEmContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        public PhuHuynhController(QuanLyTreEmContext context, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _context = context;
            _environment = environment;
        }

        // ============================================
        // API: LOAD DASHBOARD
        // ============================================
        [HttpGet("Home")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> GetPhuHuynhHome([FromQuery] int? treEmId = null)
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                // 1 Lấy PhuHuynhID từ UserID
                var phuHuynh = await _context.ThongTinPhuHuynhs
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (phuHuynh == null)
                    return NotFound(new { message = "Không tìm thấy thông tin phụ huynh" });

                var phuHuynhId = phuHuynh.PhuHuynhId;

                var response = new PhuHuynhHomeResponseDTO
                {
                    DanhSachCon = new List<TreEmDropdownDTO>(),
                    HoTroDaNhan = new List<HoTroInfoDTO>(),
                    SuKienSapToi = new List<SuKienInfoDTO>(),
                    ThongBaoChuaDoc = new List<ThongBaoInfoDTO>()
                };

                // 2 Lấy danh sách con (tuổi tăng dần, con nhỏ nhất trước)
                var danhSachTre = await _context.TreEms
                    .Where(te => te.TreEmPhuHuynhs.Any(tp => tp.PhuHuynhId == phuHuynhId))
                    .Select(te => new TreEmDropdownDTO
                    {
                        TreEmID = te.TreEmId,
                        HoTen = te.HoTen,
                        Anh = te.Anh,
                        NgaySinh = te.NgaySinh,
                    })
                    .OrderByDescending(te => te.NgaySinh) // Con nhỏ nhất (sinh sau) đầu tiên
                    .ToListAsync();
                foreach (var tre in danhSachTre)
                {
                    if (tre.NgaySinh.HasValue)
                    {
                        var ngaySinh = tre.NgaySinh.Value.ToDateTime(TimeOnly.MinValue);
                        var today = DateTime.Today;

                        tre.Tuoi = today.Year - ngaySinh.Year -
                                   (today < ngaySinh.AddYears(today.Year - ngaySinh.Year) ? 1 : 0);
                    }
                    else
                    {
                        tre.Tuoi = -1;
                    }
                }

                response.DanhSachCon = danhSachTre;
                if (response.DanhSachCon.Count == 0)
                    return NotFound(new { message = "Không tìm thấy thông tin con của phụ huynh" });

                // Xác định TreEmID cần hiển thị
                int selectedTreEmId = treEmId ?? response.DanhSachCon.First().TreEmID;
                response.TreEmMacDinhId = selectedTreEmId;

                if (!response.DanhSachCon.Any(c => c.TreEmID == selectedTreEmId))
                    return BadRequest(new { message = "Trẻ em không thuộc quyền quản lý của phụ huynh này" });

                // 3 Lấy thông tin học tập mới nhất của con
                response.ThongTinHocTap = await _context.PhieuHocTaps
                    .Where(p => p.TreEmId == selectedTreEmId)
                    .OrderByDescending(p => p.NamHoc)
                    .Select(p => new ThongTinHocTapDTO
                    {
                        PhieuHocTapID = p.PhieuHocTapId,
                        TenTreEm = p.TreEm.HoTen,
                        TenTruong = p.Truong.TenTruong,
                        TenLop = p.Lop.TenLop,
                        DiemTrungBinh = p.DiemTrungBinh,
                        XepLoai = p.XepLoai,
                        HanhKiem = p.HanhKiem,
                        GhiChu = p.GhiChu,
                        NamHoc = p.NamHoc
                    })
                    .FirstOrDefaultAsync();

                // 4 Lấy 3 hỗ trợ mới nhất trong tháng hiện tại

                var now = DateTime.Now;
                var startOfMonth = new DateOnly(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                //response.HoTroDaNhan = await _context.HoTroPhucLois
                //    .Where(h => h.TreEmId == selectedTreEmId &&
                //                h.NgayCap >= startOfMonth &&
                //                h.NgayCap <= endOfMonth)
                //    .OrderByDescending(h => h.NgayCap)
                //    .Take(3)
                //    .Select(h => new HoTroInfoDTO
                //    {
                //        HoTroID = h.HoTroId,
                //        LoaiHoTro = h.LoaiHoTro,
                //        MoTa = h.MoTa,
                //        NgayCap = h.NgayCap,
                //        NguoiChiuTrachNhiemHoTro = h.NguoiChiuTrachNhiemHoTro
                //    })
                //    .ToListAsync();

                // 5 Lấy 3 sự kiện sắp tới ở khu phố của con

                response.SuKienSapToi = await _context.SuKiens
                    .Where(sk => sk.KhuPhoId == _context.TreEms
                                    .Where(te => te.TreEmId == selectedTreEmId)
                                    .Select(te => te.KhuPhoId)
                                    .FirstOrDefault()
                                 && sk.NgayBatDau >= DateOnly.FromDateTime(DateTime.Today))
                    .OrderBy(sk => sk.NgayBatDau)
                    .Take(3)
                    .Select(sk => new SuKienInfoDTO
                    {
                        SuKienID = sk.SuKienID,
                        TenSuKien = sk.TenSuKien,
                        MoTa = sk.MoTa,
                        DiaDiem = sk.DiaDiem,
                        NgayBatDau = sk.NgayBatDau,
                        NgayKetThuc = sk.NgayKetThuc,
                        TenKhuPho = sk.KhuPho.TenKhuPho
                    })
                    .ToListAsync();

                // 6 Đếm thông báo chưa đọc
                response.SoThongBaoChuaDoc = await _context.ThongBaoNguoiDungs
                    .CountAsync(t => t.UserId == userId && !t.DaDoc);

                // 7 Lấy 3 thông báo chưa đọc mới nhất
                response.ThongBaoChuaDoc = await _context.ThongBaos
                    .Join(_context.ThongBaoNguoiDungs,
                        tb => tb.ThongBaoID,
                        tbnd => tbnd.ThongBaoID,
                        (tb, tbnd) => new { tb, tbnd })
                    .Where(x => x.tbnd.UserId == userId && !x.tbnd.DaDoc)
                    .OrderByDescending(x => x.tb.NgayThongBao)
                    .Take(3)
                    .Select(x => new ThongBaoInfoDTO
                    {
                        ThongBaoID = x.tb.ThongBaoID,
                        NoiDung = x.tb.NoiDung,
                        NgayThongBao = x.tb.NgayThongBao,
                        TenSuKien = x.tb.SuKien != null ? x.tb.SuKien.TenSuKien : null
                    })
                    .ToListAsync();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        // ============================================
        // API: DANH SÁCH CON (CAROUSEL)
        // ============================================
        [HttpGet("DanhSachCon")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> GetDanhSachCon()
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                var danhSachCon = await _context.TreEms
                    .Include(te => te.Truong)
                    .Include(te => te.TreEmPhuHuynhs)
                    .ThenInclude(teph => teph.PhuHuynh)
                    .Where(te => te.TreEmPhuHuynhs.Any(teph => teph.PhuHuynh.UserId == userId))
                    .OrderByDescending(te => te.NgaySinh)
                    .Select(te => new TreEmBasicInfoDTO
                    {
                        TreEmID = te.TreEmId,
                        HoTen = te.HoTen,
                        NgaySinh = te.NgaySinh.HasValue ? te.NgaySinh.Value.ToString("dd/MM/yyyy") : "",
                        GioiTinh = te.GioiTinh,
                        Anh = te.Anh,
                        TenTruong = te.Truong != null ? te.Truong.TenTruong : "Chưa có thông tin",
                        CapHoc = te.Truong != null ? te.Truong.CapHoc : "Chưa có thông tin"
                    })
                    .ToListAsync();

                return Ok(new DanhSachConResponseDTO { DanhSachCon = danhSachCon });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // API: TAB HỌC TẬP - PHIẾU HỌC TẬP (SLIDE BANNER)
        // ============================================
        [HttpGet("HocTap/{treEmId}")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> GetTabHocTap(int treEmId)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                // Kiểm tra quyền truy cập
                if (!await KiemTraQuyenTruyCap(userId, treEmId))
                {
                    return Forbid("Bạn không có quyền xem thông tin trẻ này");
                }

                var namHienTai = DateTime.Now.Year;

                var danhSachPhieuHocTap = await _context.PhieuHocTaps
                    .Include(pht => pht.Lop)
                    .Include(pht => pht.Truong)
                    //.Where(pht => pht.TreEmId == treEmId && pht.NamHoc.HasValue && pht.NamHoc.Value.Year == namHienTai)
                    .Where(pht => pht.TreEmId == treEmId && pht.NamHoc.HasValue)
                    .OrderByDescending(pht => pht.NamHoc)
                    .Select(pht => new PhieuHocTapInfoDTO
                    {
                        PhieuHocTapID = pht.PhieuHocTapId,
                        DiemTrungBinh = pht.DiemTrungBinh ?? 0,
                        XepLoai = pht.XepLoai ?? "Chưa xếp loại",
                        HanhKiem = pht.HanhKiem ?? "Chưa đánh giá",
                        NhanXet = pht.GhiChu ?? "",
                        NgayCapNhat = pht.NamHoc.HasValue ? pht.NamHoc.Value.ToString("dd/MM/yyyy") : "",
                        TenLop = pht.Lop != null ? pht.Lop.TenLop : "Chưa có thông tin",
                        TenTruong = pht.Truong != null ? pht.Truong.TenTruong : "Chưa có thông tin"
                    })
                    .ToListAsync();

                return Ok(new TabHocTapResponseDTO { DanhSachPhieuHocTap = danhSachPhieuHocTap });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // API: TAB HỖ TRỢ - DANH SÁCH HỖ TRỢ VÀ ỦNG HỘ
        // ============================================
        //[HttpGet("HoTro/{treEmId}")]
        //[Authorize(Roles = "Phụ huynh")]
        //public async Task<IActionResult> GetTabHoTro(int treEmId)
        //{
        //    try
        //    {
        //        //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        //            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        //        if (string.IsNullOrEmpty(subClaim))
        //            return Unauthorized(new { message = "Không tìm thấy userId trong token" });

        //        var userId = int.Parse(subClaim);

        //        // Kiểm tra quyền truy cập
        //        if (!await KiemTraQuyenTruyCap(userId, treEmId))
        //        {
        //            return Forbid("Bạn không có quyền xem thông tin trẻ này");
        //        }

        //        // Lấy danh sách Hỗ trợ phúc lợi
        //        var danhSachHoTro = await _context.HoTroPhucLois
        //            .Include(htpl => htpl.PhieuMinhChungs)
        //            .Where(htpl => htpl.TreEmId == treEmId)
        //            .OrderByDescending(htpl => htpl.NgayCap)
        //            .Select(htpl => new HoTroPhucLoiInfoDTO
        //            {
        //                HoTroID = htpl.HoTroId,
        //                LoaiHoTro = htpl.LoaiHoTro,
        //                MoTa = htpl.MoTa ?? "",
        //                NgayCap = htpl.NgayCap.HasValue ? htpl.NgayCap.Value.ToString("dd/MM/yyyy") : "",
        //                NguoiChiuTrachNhiem = htpl.NguoiChiuTrachNhiemHoTro ?? "",
        //                DanhSachMinhChung = htpl.PhieuMinhChungs
        //                    .OrderByDescending(mc => mc.NgayCap)
        //                    .Select(mc => new MinhChungInfoDTO
        //                    {
        //                        MinhChungID = mc.MinhChungId,
        //                        LoaiMinhChung = mc.LoaiMinhChung,
        //                        FilePath = mc.FilePath ?? "",
        //                        NgayCap = mc.NgayCap.HasValue ? mc.NgayCap.Value.ToString("dd/MM/yyyy") : ""
        //                    })
        //                    .ToList()
        //            })
        //            .ToListAsync();

        //        var danhSachUngHo = await _context.UngHos
        //            .Include(uh => uh.ManhThuongQuan)
        //            .Where(uh => uh.TreEms.Any(te => te.TreEmId == treEmId))
        //            .OrderByDescending(uh => uh.NgayUngHo)
        //            .Select(uh => new UngHoInfoDTO
        //            {
        //                UngHoID = uh.UngHoId,
        //                SoTien = uh.SoTien,
        //                LoaiUngHo = uh.LoaiUngHo,
        //                NgayUngHo = uh.NgayUngHo.HasValue ? uh.NgayUngHo.Value.ToString("dd/MM/yyyy") : "",
        //                GhiChu = uh.GhiChu ?? "",
        //                TenManhThuongQuan = uh.ManhThuongQuan != null ? uh.ManhThuongQuan.Ten : "Ẩn danh",
        //                LoaiManhThuongQuan = uh.ManhThuongQuan != null ? uh.ManhThuongQuan.Loai : ""
        //            })
        //            .ToListAsync();

        //        return Ok(new TabHoTroResponseDTO
        //        {
        //            DanhSachHoTro = danhSachHoTro,
        //            DanhSachUngHo = danhSachUngHo
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        //    }
        //}


        // ============================================
        // API: TAB QUÀ ĐÃ NHẬN - DANH SÁCH QUÀ PHÂN PHÁT
        // ============================================
        [HttpGet("QuaDaNhan/{treEmId}")]
        public async Task<IActionResult> GetQuaDaNhan(int treEmId, [FromQuery] string filter = "all")
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                // Kiểm tra quyền truy cập
                if (!await KiemTraQuyenTruyCap(userId, treEmId))
                {
                    return Forbid("Bạn không có quyền xem thông tin trẻ này");
                }

                // Query cơ bản
                var query = _context.PhanPhatQuas
                    .Include(ppq => ppq.QuaTangUngHo)
                        .ThenInclude(qtu => qtu.SuKien)
                    .Where(ppq => ppq.TreEmId == treEmId);

                // Áp dụng filter theo trạng thái
                switch (filter.ToLower())
                {
                    case "da-nhan":
                        query = query.Where(ppq => ppq.TrangThai == "Đã nhận");
                        break;

                    case "dang-tien-hanh":
                        query = query.Where(ppq => ppq.TrangThai == "Đang tiến hành" ||
                                                  ppq.TrangThai == "Chờ nhận");
                        break;

                    case "all":
                    default:
                        // Lấy tất cả, không filter
                        break;
                }

                // Lấy dữ liệu và sắp xếp theo ngày mới nhất
                var danhSachQua = await query
                    .OrderByDescending(ppq => ppq.NgayPhanPhat)
                    .Select(ppq => new QuaPhanPhatInfoDTO
                    {
                        PhanPhatID = ppq.PhanPhatId,
                        TenQua = ppq.QuaTangUngHo.TenQua ?? "",
                        MoTa = ppq.QuaTangUngHo.MoTa ?? "",
                        SoLuongNhan = ppq.SoLuongNhan ,
                        NgayPhanPhat = ppq.NgayPhanPhat.ToString("dd/MM/yyyy") ?? "",
                        NguoiPhanPhat = ppq.NguoiPhanPhat ?? "",
                        TrangThai = ppq.TrangThai ?? "",
                        Anh = ppq.QuaTangUngHo.Anh ?? "",

                        // Thông tin sự kiện (để navigate)
                        SuKienID = ppq.QuaTangUngHo.SuKienId,
                        TenSuKien = ppq.QuaTangUngHo.SuKien != null
                            ? ppq.QuaTangUngHo.SuKien.TenSuKien
                            : null
                    })
                    .ToListAsync();

                return Ok(new TabQuaDaNhanResponseDTO
                {
                    DanhSachQua = danhSachQua,
                    TongSoQua = danhSachQua.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        // ============================================
        // HELPER: Kiểm tra quyền truy cập
        // ============================================
        private async Task<bool> KiemTraQuyenTruyCap(int userId, int treEmId)
        {
            return await _context.TreEmPhuHuynhs
                .Include(teph => teph.PhuHuynh)
                .AnyAsync(teph => teph.PhuHuynh.UserId == userId && teph.TreEmId == treEmId);
        }

        // ============================================
        // API: DANH SÁCH SỰ KIỆN VỚI BỘ LỌC
        // ============================================

        [HttpGet("DanhSachSuKien")]
        public async Task<IActionResult> GetDanhSachSuKien([FromQuery] string filter = "sap-dien-ra")
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);
                var today = DateOnly.FromDateTime(DateTime.Now.Date);
                // Lấy tất cả sự kiện
                var query = _context.SuKiens
                    .Include(sk => sk.KhuPho)
                    .AsQueryable();

                // Áp dụng bộ lọc
                switch (filter.ToLower())
                {
                    case "sap-dien-ra":
                        query = query.Where(sk => sk.NgayBatDau >= today);
                        break;

                    case "da-dang-ky":
                        query = query.Where(sk => sk.DangKySuKiens.Any(dk =>
                            dk.UserId == userId &&
                            dk.TrangThai == "Đã duyệt"));
                        break;

                    case "da-ket-thuc":
                        query = query.Where(sk => sk.NgayKetThuc < today);
                        break;

                    default:
                        query = query.Where(sk => sk.NgayBatDau >= today);
                        break;
                }

                // Lấy dữ liệu
                var suKiens = await query
                    .OrderBy(sk => sk.NgayBatDau)
                    .Select(sk => new
                    {
                        sk.SuKienID,
                        sk.TenSuKien,
                        sk.NgayBatDau,
                        sk.NgayKetThuc,
                        sk.DiaDiem,
                        TenKhuPho = sk.KhuPho.TenKhuPho ?? "",
                        DangKy = sk.DangKySuKiens.FirstOrDefault(dk => dk.UserId == userId)
                    })
                    .ToListAsync();

                var danhSachSuKien = suKiens.Select(sk => new DanhSachSuKienDTO
                {
                    SuKienId = sk.SuKienID,
                    TenSuKien = sk.TenSuKien,
                    NgayBatDau = sk.NgayBatDau?.ToString("dd/MM/yyyy") ?? "",
                    NgayKetThuc = sk.NgayKetThuc?.ToString("dd/MM/yyyy") ?? "",
                    DiaDiem = sk.DiaDiem ?? "",
                    TenKhuPho = sk.TenKhuPho,
                    DaDangKy = sk.DangKy != null,
                    TrangThaiDangKy = sk.DangKy?.TrangThai ?? ""
                }).ToList();

                return Ok(new DanhSachSuKienResponseDTO { DanhSachSuKien = danhSachSuKien });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // API: CHI TIẾT SỰ KIỆN
        // ============================================
        [HttpGet("ChiTietSuKien/{suKienId}")]
        public async Task<IActionResult> GetChiTietSuKien(int suKienId)
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                var userId = int.Parse(subClaim);

                var suKien = await _context.SuKiens
                    .Include(sk => sk.KhuPho)
                    .Include(sk => sk.DangKySuKiens)
                    .FirstOrDefaultAsync(sk => sk.SuKienID == suKienId);

                if (suKien == null)
                {
                    return NotFound(new { message = "Không tìm thấy sự kiện" });
                }

                var thoiGianList = await _context.ThoiGianChiTietSuKiens
                    .Where(tg => tg.SuKienId == suKienId)
                    .OrderBy(tg => tg.ThoiGianBatDau)
                    .ToListAsync();

                var tietMucDict = new Dictionary<int, List<TietMucInfoDTO>>();

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = @"
                SELECT tm.TietMucID, tm.TenTietMuc, tm.NguoiThucHien, tm.ThoiGianChiTietSuKienID
                FROM TietMucSuKien tm
                INNER JOIN ThoiGianChiTietSuKien tg ON tm.ThoiGianChiTietSuKienID = tg.ThoiGianChiTietSuKienID
                WHERE tg.SuKienID = @SuKienID";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@SuKienID";
                    parameter.Value = suKienId;
                    command.Parameters.Add(parameter);

                    await _context.Database.OpenConnectionAsync();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (await result.ReadAsync())
                        {
                            var thoiGianId = result.GetInt32(result.GetOrdinal("ThoiGianChiTietSuKienID"));

                            if (!tietMucDict.ContainsKey(thoiGianId))
                            {
                                tietMucDict[thoiGianId] = new List<TietMucInfoDTO>();
                            }

                            tietMucDict[thoiGianId].Add(new TietMucInfoDTO
                            {
                                TietMucId = result.GetInt32(result.GetOrdinal("TietMucID")),
                                TenTietMuc = result.IsDBNull(result.GetOrdinal("TenTietMuc"))
                                    ? ""
                                    : result.GetString(result.GetOrdinal("TenTietMuc")),
                                NguoiThucHien = result.IsDBNull(result.GetOrdinal("NguoiThucHien"))
                                    ? ""
                                    : result.GetString(result.GetOrdinal("NguoiThucHien"))
                            });
                        }
                    }
                }

                var dangKy = suKien.DangKySuKiens.FirstOrDefault(dk => dk.UserId == userId);

                var danhSachChuongTrinh = thoiGianList.Select(tg => new ChuongTrinhSuKienDTO
                {
                    ThoiGianChiTietSuKienId = tg.ThoiGianChiTietSuKienId,
                    MoTa = tg.MoTa ?? "",
                    ThoiGianBatDau = tg.ThoiGianBatDau?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    ThoiGianKetThuc = tg.ThoiGianKetThuc?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    DanhSachTietMuc = tietMucDict.ContainsKey(tg.ThoiGianChiTietSuKienId)
                        ? tietMucDict[tg.ThoiGianChiTietSuKienId]
                        : new List<TietMucInfoDTO>()
                }).ToList();

                var response = new ChiTietSuKienResponseDTO
                {
                    SuKienId = suKien.SuKienID,
                    TenSuKien = suKien.TenSuKien,
                    MoTa = suKien.MoTa ?? "",
                    DiaDiem = suKien.DiaDiem ?? "",
                    NgayBatDau = suKien.NgayBatDau?.ToString("dd/MM/yyyy") ?? "",
                    NgayKetThuc = suKien.NgayKetThuc?.ToString("dd/MM/yyyy") ?? "",
                    SoLuongTinhNguyenVien = suKien.SoLuongTinhNguyenVien ?? 0,
                    SoLuongTreEm = suKien.SoLuongTreEm ?? 0,
                    NguoiChiuTrachNhiem = suKien.NguoiChiuTrachNhiem ?? "",
                    TenKhuPho = suKien.KhuPho?.TenKhuPho ?? "",
                    DiaChiKhuPho = suKien.KhuPho?.DiaChi ?? "",
                    DaDangKy = dangKy != null,
                    TrangThaiDangKy = dangKy?.TrangThai ?? "",
                    DanhSachChuongTrinh = danhSachChuongTrinh
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // API: ĐĂNG KÝ THAM GIA SỰ KIỆN
        // ============================================
        [HttpPost("DangKySuKien")]
        public async Task<IActionResult> DangKySuKien([FromBody] DangKySuKienRequestDTO request)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                // Kiểm tra sự kiện có tồn tại không
                var suKien = await _context.SuKiens
                    .FirstOrDefaultAsync(sk => sk.SuKienID == request.SuKienId);

                if (suKien == null)
                {
                    return NotFound(new { message = "Không tìm thấy sự kiện" });
                }

                // Kiểm tra đã đăng ký chưa
                var dangKyCu = await _context.DangKySuKiens
                    .FirstOrDefaultAsync(dk => dk.SuKienId == request.SuKienId && dk.UserId == userId);

                if (dangKyCu != null)
                {
                    return BadRequest(new DangKySuKienResponseDTO
                    {
                        Success = false,
                        Message = "Bạn đã đăng ký sự kiện này rồi",
                        TrangThai = dangKyCu.TrangThai
                    });
                }

                // Tạo đăng ký mới
                var dangKyMoi = new DangKySuKien
                {
                    SuKienId = request.SuKienId,
                    UserId = userId,
                    NgayDangKy = DateOnly.FromDateTime(DateTime.Now),
                    TrangThai = "Chờ duyệt"
                };

                _context.DangKySuKiens.Add(dangKyMoi);
                await _context.SaveChangesAsync();

                return Ok(new DangKySuKienResponseDTO
                {
                    Success = true,
                    Message = "Đăng ký sự kiện thành công. Vui lòng chờ duyệt!",
                    TrangThai = "Chờ duyệt"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // API: HỦY ĐĂNG KÝ SỰ KIỆN (Bonus)
        // ============================================
        [HttpDelete("HuyDangKySuKien/{suKienId}")]
        public async Task<IActionResult> HuyDangKySuKien(int suKienId)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                var dangKy = await _context.DangKySuKiens
                    .FirstOrDefaultAsync(dk => dk.SuKienId == suKienId && dk.UserId == userId);

                if (dangKy == null)
                {
                    return NotFound(new { message = "Không tìm thấy đăng ký" });
                }

                // Chỉ cho phép hủy nếu đang "Chờ duyệt"
                if (dangKy.TrangThai != "Chờ duyệt")
                {
                    return BadRequest(new { message = $"Không thể hủy đăng ký đã {dangKy.TrangThai.ToLower()}" });
                }

                _context.DangKySuKiens.Remove(dangKy);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Hủy đăng ký thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }

        }
        // ============================================
        // API: TAB THÔNG BÁO - DANH SÁCH THÔNG BÁO
        // ============================================
        [HttpGet("ThongBao")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> GetDanhSachThongBao([FromQuery] string? filter = "all")
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                // Thêm logging để debug
                Console.WriteLine($"SubClaim value: '{subClaim}'");

                // Trim whitespace và validate
                subClaim = subClaim.Trim();

                if (!int.TryParse(subClaim, out int userId))
                {
                    return BadRequest(new
                    {
                        message = "UserId không hợp lệ",
                        receivedValue = subClaim
                    });
                }

                var query = _context.ThongBaoNguoiDungs
                    .Include(tbnd => tbnd.ThongBao)
                        .ThenInclude(tb => tb.SuKien)
                    .Where(tbnd => tbnd.UserId == userId);

                // Filter: "all" hoặc "unread"
                if (filter == "unread")
                {
                    query = query.Where(tbnd => !tbnd.DaDoc);
                }

                var danhSachThongBao = await query
                    .OrderByDescending(tbnd => tbnd.ThongBao.NgayThongBao)
                    .Select(tbnd => new TabThongBaoInfoDTO
                    {
                        ThongBaoID = tbnd.ThongBaoID,
                        NoiDung = tbnd.ThongBao.NoiDung,
                        NgayThongBao = tbnd.ThongBao.NgayThongBao.HasValue 
                            ? tbnd.ThongBao.NgayThongBao.Value.ToString("dd/MM/yyyy") 
                            : "",
                        DaDoc = tbnd.DaDoc,
                        SuKienID = tbnd.ThongBao.SuKienId,
                        TenSuKien = tbnd.ThongBao.SuKien != null ? tbnd.ThongBao.SuKien.TenSuKien : null
                    })
                    .ToListAsync();

                var soLuongChuaDoc = await _context.ThongBaoNguoiDungs
                    .Where(tbnd => tbnd.UserId == userId && !tbnd.DaDoc)
                    .CountAsync();

                return Ok(new TabThongBaoResponseDTO
                {
                    SoLuongChuaDoc = soLuongChuaDoc,
                    DanhSachThongBao = danhSachThongBao
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // TAB THÔNG BÁO - ĐÁNH DẤU ĐÃ ĐỌC
        // ============================================
        [HttpPost("ThongBao/DanhDauDaDoc")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> DanhDauDaDoc([FromBody] DanhDauDaDocRequestDTO request)
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                subClaim = subClaim.Trim();

                if (!int.TryParse(subClaim, out int userId))
                {
                    return BadRequest(new
                    {
                        message = "UserId không hợp lệ",
                        receivedValue = subClaim
                    });
                }

                var thongBaoNguoiDung = await _context.ThongBaoNguoiDungs
                    .FirstOrDefaultAsync(tbnd => tbnd.ThongBaoID == request.ThongBaoID && tbnd.UserId == userId);

                if (thongBaoNguoiDung == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông báo" });
                }

                thongBaoNguoiDung.DaDoc = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã đánh dấu đã đọc thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        // ============================================
        // TAB TÀI KHOẢN - LẤY THÔNG TIN TÀI KHOẢN USER
        // ============================================
        [HttpGet("ThongTinTaiKhoan")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> GetThongTinTaiKhoan()
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(nd => nd.UserId == userId);

                if (nguoiDung == null)
                    return NotFound(new { message = "Không tìm thấy thông tin tài khoản" });

                var response = new ThongTinTaiKhoanResponseDTO
                {
                    UserID = nguoiDung.UserId,
                    HoTen = nguoiDung.HoTen ?? "",
                    Email = nguoiDung.Email ?? "",
                    SDT = nguoiDung.SDT ?? "",
                    Anh = nguoiDung.Anh ?? ""
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // TAB TÀI KHOẢN - UPLOAD ẢNH TÀI KHOẢN USER
        // ============================================
        [HttpPost("UploadAnhTaiKhoan")]
        [Authorize(Roles = "Phụ huynh")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAnhTaiKhoan( IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File không hợp lệ" });

                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
                if (nguoiDung == null)
                    return NotFound(new { message = "Không tìm thấy tài khoản" });

                // Tạo thư mục nếu chưa tồn tại
                var uploadFolder = Path.Combine(_environment.WebRootPath, "Anh", "NguoiDung");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(nguoiDung.Anh))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, nguoiDung.Anh.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Tạo tên file unique
                var fileName = $"{userId}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Cập nhật database
                var relativePath = $"/Anh/NguoiDung/{fileName}";
                nguoiDung.Anh = relativePath;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Upload ảnh thành công", filePath = relativePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        //// ============================================
        //// TAB TÀI KHOẢN - LẤY THÔNG TIN PHỤ HUYNH (BỎ)
        //// ============================================
        //[HttpGet("ThongTin")]
        //[Authorize(Roles = "Phụ huynh")]
        //public async Task<IActionResult> GetThongTinPhuHuynh()
        //{
        //    try
        //    {
        //        //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        //            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        //        if (string.IsNullOrEmpty(subClaim))
        //            return Unauthorized(new { message = "Không tìm thấy userId trong token" });

        //        var userId = int.Parse(subClaim);

        //        var thongTin = await _context.ThongTinPhuHuynhs
        //            .Where(ph => ph.UserId == userId)
        //            .Select(ph => new ThongTinPhuHuynhResponseDTO
        //            {
        //                PhuHuynhID = ph.PhuHuynhId,
        //                HoTen = ph.HoTen,
        //                SDT = ph.Sdt,
        //                Email = ph.User.Email,
        //                DiaChi = ph.DiaChi ?? "",
        //                NgheNghiep = ph.NgheNghiep ?? "",
        //                NgaySinh = ph.NgaySinh.HasValue ? ph.NgaySinh.Value.ToString("dd/MM/yyyy") : "",
        //                TonGiao = ph.TonGiao ?? "",
        //                DanToc = ph.DanToc ?? "",
        //                QuocTich = ph.QuocTich ?? "",
        //                Anh = ph.User.Anh ?? ""
        //            })
        //            .FirstOrDefaultAsync();

        //        if (thongTin == null)
        //        {
        //            return NotFound(new { message = "Không tìm thấy thông tin phụ huynh" });
        //        }

        //        return Ok(thongTin);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        //    }
        //}

        // ============================================
        // TAB TÀI KHOẢN - LẤY DANH SÁCH PHỤ HUYNH CỦA USER
        // ============================================
        [HttpGet("DanhSachPhuHuynh")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> GetDanhSachPhuHuynh()
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                // Lấy tất cả phụ huynh liên kết với userId
                var danhSachPhuHuynh = await _context.ThongTinPhuHuynhs
                    .Include(ph => ph.TreEmPhuHuynhs)
                        .ThenInclude(teph => teph.TreEm)
                    .Include(ph => ph.User)
                    .Where(ph => ph.UserId == userId)
                    .OrderBy(ph => ph.PhuHuynhId)
                    .Select(ph => new PhuHuynhVoiMoiQuanHeDTO
                    {
                        PhuHuynhID = ph.PhuHuynhId,
                        HoTen = ph.HoTen ?? "",
                        SDT = ph.Sdt ?? "",
                        DiaChi = ph.DiaChi ?? "",
                        NgheNghiep = ph.NgheNghiep ?? "",
                        NgaySinh = ph.NgaySinh.HasValue ? ph.NgaySinh.Value.ToString("dd/MM/yyyy") : "",
                        TonGiao = ph.TonGiao ?? "",
                        DanToc = ph.DanToc ?? "",
                        QuocTich = ph.QuocTich ?? "",
                        Anh = ph.User.Anh ?? "",
                        DanhSachMoiQuanHe = ph.TreEmPhuHuynhs
                            .OrderBy(teph => teph.TreEm.NgaySinh)
                            .Select(teph => new MoiQuanHeVoiTreEmDTO
                            {
                                TreEmID = teph.TreEmId,
                                TenTreEm = teph.TreEm.HoTen ?? "",
                                MoiQuanHe = teph.MoiQuanHe ?? "",
                                Anh = teph.TreEm.Anh ?? ""
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(new DanhSachPhuHuynhResponseDTO { DanhSachPhuHuynh = danhSachPhuHuynh });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        // ============================================
        // TAB TÀI KHOẢN - LẤY CHI TIẾT MỘT PHỤ HUYNH
        // ============================================
        [HttpGet("ChiTietPhuHuynh/{phuHuynhId}")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> GetChiTietPhuHuynh(int phuHuynhId)
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                var phuHuynh = await _context.ThongTinPhuHuynhs
                    .Include(ph => ph.TreEmPhuHuynhs)
                        .ThenInclude(teph => teph.TreEm)
                    .Include(ph => ph.User)
                    .FirstOrDefaultAsync(ph => ph.PhuHuynhId == phuHuynhId && ph.UserId == userId);

                if (phuHuynh == null)
                    return NotFound(new { message = "Không tìm thấy phụ huynh hoặc bạn không có quyền truy cập" });

                var response = new PhuHuynhVoiMoiQuanHeDTO
                {
                    PhuHuynhID = phuHuynh.PhuHuynhId,
                    HoTen = phuHuynh.HoTen ?? "",
                    SDT = phuHuynh.Sdt ?? "",
                    DiaChi = phuHuynh.DiaChi ?? "",
                    NgheNghiep = phuHuynh.NgheNghiep ?? "",
                    NgaySinh = phuHuynh.NgaySinh.HasValue ? phuHuynh.NgaySinh.Value.ToString("dd/MM/yyyy") : "",
                    TonGiao = phuHuynh.TonGiao ?? "",
                    DanToc = phuHuynh.DanToc ?? "",
                    QuocTich = phuHuynh.QuocTich ?? "",
                    Anh = phuHuynh.User.Anh ?? "",
                    DanhSachMoiQuanHe = phuHuynh.TreEmPhuHuynhs
                        .OrderBy(teph => teph.TreEm.NgaySinh)
                        .Select(teph => new MoiQuanHeVoiTreEmDTO
                        {
                            TreEmID = teph.TreEmId,
                            TenTreEm = teph.TreEm.HoTen ?? "",
                            MoiQuanHe = teph.MoiQuanHe ?? "",
                            Anh = teph.TreEm.Anh ?? ""
                        })
                        .ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // TAB TÀI KHOẢN - CẬP NHẬT THÔNG TIN PHỤ HUYNH CỤ THỂ
        // ============================================
        [HttpPut("CapNhatPhuHuynh")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> CapNhatPhuHuynh([FromBody] CapNhatThongTinPhuHuynhRequestDTO request)
        {
            try
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                // Kiểm tra phụ huynh có thuộc user này không
                var phuHuynh = await _context.ThongTinPhuHuynhs
                    .FirstOrDefaultAsync(ph => ph.PhuHuynhId == request.PhuHuynhID && ph.UserId == userId);

                if (phuHuynh == null)
                    return NotFound(new { message = "Không tìm thấy phụ huynh hoặc bạn không có quyền cập nhật" });

                // Cập nhật thông tin
                phuHuynh.HoTen = request.HoTen;
                phuHuynh.Sdt = request.SDT;
                phuHuynh.DiaChi = request.DiaChi;
                phuHuynh.NgheNghiep = request.NgheNghiep;
                phuHuynh.TonGiao = request.TonGiao;
                phuHuynh.DanToc = request.DanToc;
                phuHuynh.QuocTich = request.QuocTich;

                // Parse ngày sinh
                if (!string.IsNullOrEmpty(request.NgaySinh))
                {
                    phuHuynh.NgaySinh = DateOnly.FromDateTime(
                        DateTime.ParseExact(request.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    );
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật thông tin phụ huynh thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        //// ============================================
        //// TAB TÀI KHOẢN - CẬP NHẬT THÔNG TIN PHỤ HUYNH (BỎ)
        //// ============================================
        //[HttpPut("CapNhatThongTin")]
        //[Authorize(Roles = "Phụ huynh")]
        //public async Task<IActionResult> CapNhatThongTinPhuHuynh([FromBody] CapNhatThongTinPhuHuynhRequestDTO request)
        //{
        //    try
        //    {
        //        //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        //            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        //        if (string.IsNullOrEmpty(subClaim))
        //            return Unauthorized(new { message = "Không tìm thấy userId trong token" });

        //        var userId = int.Parse(subClaim);

        //        var phuHuynh = await _context.ThongTinPhuHuynhs
        //            .Include(ph => ph.User)
        //            .FirstOrDefaultAsync(ph => ph.UserId == userId);

        //        if (phuHuynh == null)
        //        {
        //            return NotFound(new { message = "Không tìm thấy thông tin phụ huynh" });
        //        }

        //        // Cập nhật thông tin
        //        phuHuynh.HoTen = request.HoTen;
        //        phuHuynh.Sdt = request.SDT;
        //        phuHuynh.DiaChi = request.DiaChi;
        //        phuHuynh.NgheNghiep = request.NgheNghiep;
        //        phuHuynh.TonGiao = request.TonGiao;
        //        phuHuynh.DanToc = request.DanToc;
        //        phuHuynh.QuocTich = request.QuocTich;

        //        // Parse ngày sinh
        //        if (!string.IsNullOrEmpty(request.NgaySinh))
        //        {
        //            phuHuynh.NgaySinh = DateOnly.FromDateTime(DateTime.ParseExact(request.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture));
        //        }

        //        // Cập nhật email trong bảng NguoiDung
        //        phuHuynh.User.Email = request.Email;
        //        phuHuynh.User.HoTen = request.HoTen;
        //        phuHuynh.User.SDT = request.SDT;

        //        await _context.SaveChangesAsync();

        //        return Ok(new { message = "Cập nhật thông tin thành công" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        //    }
        //}

        // ============================================
        // TAB TÀI KHOẢN - UPLOAD ẢNH PHỤ HUYNH CỤ THỂ 
        // ============================================
        [HttpPost("UploadAnhPhuHuynh/{phuHuynhId}")]
        [Authorize(Roles = "Phụ huynh")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAnhPhuHuynhCuThe(int phuHuynhId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File không hợp lệ" });
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                // Kiểm tra phụ huynh có thuộc user này không
                var phuHuynh = await _context.ThongTinPhuHuynhs
                    .Include(ph => ph.User)
                    .FirstOrDefaultAsync(ph => ph.PhuHuynhId == phuHuynhId && ph.UserId == userId);

                if (phuHuynh == null)
                    return NotFound(new { message = "Không tìm thấy phụ huynh hoặc bạn không có quyền upload" });

                // Tạo thư mục nếu chưa tồn tại
                var uploadFolder = Path.Combine(_environment.WebRootPath, "Anh", "NguoiDung");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                // Tạo tên file unique
                var fileName = $"{userId}_{phuHuynhId}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Cập nhật database (lưu vào bảng NguoiDung vì Anh được lưu ở đó)
                var relativePath = $"/Anh/NguoiDung/{fileName}";
                phuHuynh.User.Anh = relativePath;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Upload ảnh thành công", filePath = relativePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
        //// ============================================
        //// TAB TÀI KHOẢN - UPLOAD ẢNH PHỤ HUYNH (BỎ)
        //// ============================================
        //[HttpPost("UploadAnh")]
        //[Authorize(Roles = "Phụ huynh")]
        //public async Task<IActionResult> UploadAnhPhuHuynh([FromForm] IFormFile file)
        //{
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //        {
        //            return BadRequest(new { message = "File không hợp lệ" });
        //        }

        //        //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        //            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        //        if (string.IsNullOrEmpty(subClaim))
        //            return Unauthorized(new { message = "Không tìm thấy userId trong token" });

        //        var userId = int.Parse(subClaim);

        //        // Tạo thư mục nếu chưa tồn tại
        //        var uploadFolder = Path.Combine(_environment.WebRootPath, "Anh", "NguoiDung");
        //        if (!Directory.Exists(uploadFolder))
        //        {
        //            Directory.CreateDirectory(uploadFolder);
        //        }

        //        // Tạo tên file unique
        //        var fileName = $"{userId}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
        //        var filePath = Path.Combine(uploadFolder, fileName);

        //        // Lưu file
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }

        //        // Cập nhật database
        //        var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
        //        if (nguoiDung != null)
        //        {
        //            nguoiDung.Anh = $"/Anh/NguoiDung/{fileName}";
        //            await _context.SaveChangesAsync();
        //        }

        //        return Ok(new { message = "Upload ảnh thành công", filePath = $"/Anh/NguoiDung/{fileName}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        //    }
        //}

        // ============================================
        // TAB TÀI KHOẢN - LẤY THÔNG TIN TRẺ EM CHI TIẾT
        // ============================================
        [HttpGet("ThongTinTreEm/{treEmId}")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> GetThongTinTreEmChiTiet(int treEmId)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                if (!await KiemTraQuyenTruyCap(userId, treEmId))
                {
                    return Forbid("Bạn không có quyền xem thông tin trẻ này");
                }

                var thongTin = await _context.TreEms
                    .Where(te => te.TreEmId == treEmId)
                    .Select(te => new ThongTinTreEmChiTietResponseDTO
                    {
                        TreEmID = te.TreEmId,
                        HoTen = te.HoTen,
                        NgaySinh = te.NgaySinh.HasValue ? te.NgaySinh.Value.ToString("dd/MM/yyyy") : "",
                        GioiTinh = te.GioiTinh,
                        TonGiao = te.TonGiao ?? "",
                        DanToc = te.DanToc ?? "",
                        QuocTich = te.QuocTich ?? "",
                        Anh = te.Anh ?? "",
                        TruongID = te.TruongId,
                        TenTruong = te.Truong != null ? te.Truong.TenTruong : "",
                        CapHoc = te.Truong != null ? te.Truong.CapHoc : "",
                        LopID = te.PhieuHocTaps.OrderByDescending(p => p.NamHoc).FirstOrDefault() != null
                            ? te.PhieuHocTaps.OrderByDescending(p => p.NamHoc).FirstOrDefault().LopId
                            : null,
                        TenLop = te.PhieuHocTaps.OrderByDescending(p => p.NamHoc).FirstOrDefault() != null
                            && te.PhieuHocTaps.OrderByDescending(p => p.NamHoc).FirstOrDefault().Lop != null
                            ? te.PhieuHocTaps.OrderByDescending(p => p.NamHoc).FirstOrDefault().Lop.TenLop
                            : ""
                    })
                    .FirstOrDefaultAsync();

                if (thongTin == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin trẻ em" });
                }

                return Ok(thongTin);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // TAB TÀI KHOẢN - CẬP NHẬT THÔNG TIN TRẺ EM
        // ============================================
        [HttpPut("CapNhatTreEm")]
        [Authorize(Roles = "Phụ huynh")]
        public async Task<IActionResult> CapNhatThongTinTreEm([FromBody] CapNhatThongTinTreEmRequestDTO request)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                if (!await KiemTraQuyenTruyCap(userId, request.TreEmID))
                {
                    return Forbid("Bạn không có quyền cập nhật thông tin trẻ này");
                }

                var treEm = await _context.TreEms.FindAsync(request.TreEmID);
                if (treEm == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin trẻ em" });
                }

                // Cập nhật thông tin cá nhân
                treEm.HoTen = request.HoTen;
                treEm.NgaySinh = DateOnly.Parse(request.NgaySinh, CultureInfo.InvariantCulture);
                treEm.GioiTinh = request.GioiTinh;
                treEm.TonGiao = request.TonGiao;
                treEm.DanToc = request.DanToc;
                treEm.QuocTich = request.QuocTich;
                treEm.TruongId = request.TruongID;

                await _context.SaveChangesAsync();

                // Cập nhật LopID trong PhieuHocTap mới nhất (nếu có)
                if (request.LopID.HasValue)
                {
                    var phieuMoiNhat = await _context.PhieuHocTaps
                        .Where(p => p.TreEmId == request.TreEmID)
                        .OrderByDescending(p => p.NamHoc)
                        .FirstOrDefaultAsync();

                    if (phieuMoiNhat != null)
                    {
                        phieuMoiNhat.LopId = request.LopID;
                        phieuMoiNhat.TruongId = request.TruongID;
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok(new { message = "Cập nhật thông tin trẻ em thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ============================================
        // TAB TÀI KHOẢN - UPLOAD ẢNH TRẺ EM
        // ============================================
        [HttpPost("UploadAnhTreEm/{treEmId}")]
        [Authorize(Roles = "Phụ huynh")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAnhTreEm(int treEmId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File không hợp lệ" });
                }

                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(subClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });

                var userId = int.Parse(subClaim);

                if (!await KiemTraQuyenTruyCap(userId, treEmId))
                {
                    return Forbid("Bạn không có quyền upload ảnh cho trẻ này");
                }

                // Tạo thư mục nếu chưa tồn tại
                var uploadFolder = Path.Combine(_environment.WebRootPath, "Anh", "TreEm");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Tạo tên file unique
                var fileName = $"{treEmId}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Cập nhật database
                var treEm = await _context.TreEms.FindAsync(treEmId);
                if (treEm != null)
                {
                    treEm.Anh = $"/Anh/TreEm/{fileName}";
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Upload ảnh thành công", filePath = $"/Anh/TreEm/{fileName}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
    }
}
