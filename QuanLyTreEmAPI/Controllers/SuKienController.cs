using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;
using System.Linq;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuKienController : ControllerBase
    {
     private readonly QuanLyTreEmContext _context;
         private readonly IConfiguration _configuration;


        public SuKienController(QuanLyTreEmContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            var sk = _context.SuKiens.ToList();
            var suKiens = _context.SuKiens
       .ToList() 
       .Select(e => new
       {
           e.SuKienID,
           e.TenSuKien,
           e.NgayBatDau,
           e.AnhSuKien,
           e.NgayKetThuc,
           e.DiaDiem,
           TrangThai = GetTrangThai(
               e.NgayBatDau?.ToDateTime(TimeOnly.MinValue),
               e.NgayKetThuc?.ToDateTime(TimeOnly.MinValue)
           )
       })
        .OrderByDescending(e => e.SuKienID)
       .ToList();

            return Ok(suKiens);
        }

        [HttpGet("sapdienra")]
        public IActionResult GetUpcomingEvents()
        {
            var today = DateTime.Today;

            var result = _context.SuKiens
                .Where(e => e.NgayBatDau.HasValue && e.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue) > today)
                .Select(e => new
                {
                    e.SuKienID,
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
                .Select(e => new
                {
                    e.SuKienID,
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
                .Select(e => new
                {
                    e.SuKienID,
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {

            var sk = await _context.SuKiens
             .Include(x => x.KhuPho)
             .Include(x => x.ThoiGianChiTietSuKiens)
             .Include(x => x.ChiPhiSuKiens)
                 .ThenInclude(cp => cp.ChiTietChiPhiSuKiens)
             .AsNoTracking()
             .FirstOrDefaultAsync(x => x.SuKienId == id);

            var tietMucs = new Dictionary<int, List<TietMucSuKien>>();
            if (sk == null)
                return NotFound(new { message = "Không tìm thấy sự kiện" });

            var soTNVDaDangKy = await _context.DangKySuKiens
                .Where(x => x.SuKienId == id && x.TrangThai == "Đã duyệt")
                .CountAsync();
            var soTreEmDaDangKy = await _context.TreEmSuKiens
                .CountAsync(x => x.SuKienId == id && x.TrangThai == "Đã đăng ký");
            var connStr = _context.Database.GetDbConnection().ConnectionString;

            var tietMucDict = new Dictionary<int, List<object>>();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                var sql = @"SELECT * FROM TietMucSuKien WHERE ThoiGianChiTietSuKienID IN
            (SELECT ThoiGianChiTietSuKienID FROM ThoiGianChiTietSuKien WHERE SuKienID = @SuKienID)";


                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SuKienID", id);
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            int tgId = rd.GetInt32(rd.GetOrdinal("ThoiGianChiTietSuKienID"));


                            var obj = new
                            {
                                TietMucId = rd.GetInt32(rd.GetOrdinal("TietMucID")),
                                TenTietMuc = rd["TenTietMuc"]?.ToString(),
                                NguoiThucHien = rd["NguoiThucHien"]?.ToString(),
                                ChiPhiTietMuc = rd["ChiPhiTietMuc"] == DBNull.Value ? 0 : (decimal)rd["ChiPhiTietMuc"],
                                ThoiGianChiTietSuKienId = tgId
                            };


                            if (!tietMucDict.ContainsKey(tgId))
                                tietMucDict[tgId] = new List<object>();


                            tietMucDict[tgId].Add(obj);
                        }
                    }
                }
            }
            var quaTang = await _context.QuaTangUngHos
                .Where(x => x.SuKienId == id)
                .Select(q => new
                {
                    q.QuaTangUngHoId,
                    q.TenQua,
                    q.MoTa,
                    q.SoLuongTong,
                    q.SoLuongConLai,
                    q.DonGia,
                    q.DoiTuongNhan,
                    q.Anh,
                    SoLuongDaPhat = q.SoLuongTong - q.SoLuongConLai,
                    TongGiaTri = q.SoLuongTong * q.DonGia
                })
                .ToListAsync();

            var tongChiPhi = sk.ChiPhiSuKiens.Sum(c => c.SoTien ?? 0);
            var tongChiPhiChiTiet = sk.ChiPhiSuKiens
                .SelectMany(c => c.ChiTietChiPhiSuKiens)
                .Sum(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0));

            var result = new
            {
                // Thông tin cơ bản
                sk.SuKienID,
                sk.TenSuKien,
                sk.MoTa,
                sk.DiaDiem,
                sk.NgayBatDau,
                sk.AnhSuKien,
                sk.NgayKetThuc,
                sk.NguoiChiuTrachNhiem,
                sk.SoLuongTinhNguyenVien,
                sk.SoLuongTreEm,
                SoTNVDaDangKy = soTNVDaDangKy,
                SoTreEmDaDangKy = soTreEmDaDangKy,
                ConTrongTNV = (sk.SoLuongTinhNguyenVien ?? 0) - soTNVDaDangKy,
                ConTrongTreEm = (sk.SoLuongTreEm ?? 0) - soTreEmDaDangKy,

                // Khu phố
                KhuPho = sk.KhuPho == null ? null : new
                {
                    sk.KhuPho.KhuPhoId,
                    sk.KhuPho.TenKhuPho,
                    sk.KhuPho.DiaChi,
                    sk.KhuPho.QuanHuyen,
                    sk.KhuPho.ThanhPho
                },
                ThoiGianChiTiet = sk.ThoiGianChiTietSuKiens
                .OrderBy(t => t.ThoiGianBatDau)
                .Select(t => new
                {
                    t.ThoiGianChiTietSuKienId,
                    t.MoTa,
                    t.ThoiGianBatDau,
                    t.ThoiGianKetThuc,
                    TietMuc = tietMucDict.ContainsKey(t.ThoiGianChiTietSuKienId)
                ? tietMucDict[t.ThoiGianChiTietSuKienId]
                : new List<object>(),


                    TongChiPhiTietMuc = tietMucDict.ContainsKey(t.ThoiGianChiTietSuKienId)
                ? tietMucDict[t.ThoiGianChiTietSuKienId].Sum(tm => (decimal)tm.GetType().GetProperty("ChiPhiTietMuc").GetValue(tm))
                : 0
                }).ToList(),
                TietMuc = sk.TietMucSuKiens.ToList(),
                ChiPhi = sk.ChiPhiSuKiens.Select(c => new
                {
                    c.ChiPhiId,
                    c.TenKhoanChi,
                    c.SoTien,
                    c.NguoiPheDuyet,
                    c.NgayPheDuyet,
                    c.VanBanPheDuyet,
                    c.GhiChu,
                    ChiTiet = c.ChiTietChiPhiSuKiens.Select(ct => new
                    {
                        ct.ChiTietChiPhi,
                        ct.TenPhanQua,
                        ct.NguoiDaiDien,
                        ct.SoLuong,
                        ct.DonGia,
                        ThanhTien = (ct.SoLuong ?? 0) * (ct.DonGia ?? 0)
                    }).ToList(),
                    TongChiTietKhoanChi = c.ChiTietChiPhiSuKiens.Sum(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0))
                }).ToList(),

                QuaTang = quaTang,

                TongKet = new
                {
                    TongChiPhi = tongChiPhi,
                    TongChiPhiChiTiet = tongChiPhiChiTiet,
                    ChenhLech = tongChiPhi - tongChiPhiChiTiet,
                    TongSoQuaTang = quaTang.Count,
                    TongGiaTriQuaTang = quaTang.Sum(q => q.TongGiaTri),
                    TongChiPhiTietMuc = sk.ThoiGianChiTietSuKiens
                        .SelectMany(t => t.TietMucSuKiens)
                        .Sum(tm => tm.ChiPhiTietMuc ?? 0)
                }
            };

            return Ok(result);
        }
        #endregion

        // Thay thế toàn bộ phần xử lý EF trong phương thức Create bằng ADO
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SuKienDTOCreate dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var connString = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(connString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();


            try
            {
                // Kiểm tra tồn tại khu phố và người dùng
                if (!await _context.KhuPhos.AnyAsync(k => k.KhuPhoId == dto.KhuPhoId))
                    return BadRequest(new { message = "Khu phố không tồn tại." });
                if (!await _context.NguoiDungs.AnyAsync(u => u.UserId == dto.UserId))
                    return BadRequest(new { message = "Người dùng không tồn tại." });

                string imagePath = null;
                if (!string.IsNullOrEmpty(dto.AnhSuKien))
                {
                    if (dto.AnhSuKien.StartsWith("data:image"))
                    {
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Anh", "SuKien");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                        var fileName = $"sukien_{DateTime.Now:yyyyMMddHHmmssfff}.jpg";
                        var filePath = Path.Combine(folderPath, fileName);
                        var base64Data = dto.AnhSuKien.Substring(dto.AnhSuKien.IndexOf(",") + 1);
                        var bytes = Convert.FromBase64String(base64Data);
                        await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                        imagePath = $"/Anh/SuKien/{fileName}";
                    }
                    else
                    {
                        imagePath = dto.AnhSuKien;
                    }
                }

                var insertSuKienCmd = conn.CreateCommand();
                insertSuKienCmd.Transaction = transaction;
                insertSuKienCmd.CommandText = @"
            INSERT INTO SuKien (TenSuKien, MoTa, DiaDiem, NgayBatDau, NgayKetThuc, NguoiChiuTrachNhiem,
                                SoLuongTinhNguyenVien, SoLuongTreEm, UserID, KhuPhoID, AnhSuKien)
            OUTPUT INSERTED.SuKienID
            VALUES (@Ten, @MoTa, @DiaDiem, @NgayBatDau, @NgayKetThuc, @Nguoi, @SoTNV, @SoTreEm, @User, @KhuPho, @Anh)";

                insertSuKienCmd.Parameters.AddRange(new[]
                {
            new SqlParameter("@Ten", dto.TenSuKien ?? (object)DBNull.Value),
            new SqlParameter("@MoTa", dto.MoTa ?? (object)DBNull.Value),
            new SqlParameter("@DiaDiem", dto.DiaDiem ?? (object)DBNull.Value),
            new SqlParameter("@NgayBatDau", dto.NgayBatDau),
            new SqlParameter("@NgayKetThuc", dto.NgayKetThuc),
            new SqlParameter("@Nguoi", dto.NguoiChiuTrachNhiem ?? (object)DBNull.Value),
            new SqlParameter("@SoTNV", dto.SoLuongTinhNguyenVien),
            new SqlParameter("@SoTreEm", dto.SoLuongTreEm),
            new SqlParameter("@User", dto.UserId),
            new SqlParameter("@KhuPho", dto.KhuPhoId),
            new SqlParameter("@Anh", (object?)imagePath ?? DBNull.Value)
        });

                var suKienId = (int)(await insertSuKienCmd.ExecuteScalarAsync());

                foreach (var tg in dto.ThoiGianChiTiet)
                {
                    var insertTimeCmd = conn.CreateCommand();
                    insertTimeCmd.Transaction = transaction;
                    insertTimeCmd.CommandText = @"
                INSERT INTO ThoiGianChiTietSuKien (MoTa, ThoiGianBatDau, ThoiGianKetThuc, SuKienID)
                OUTPUT INSERTED.ThoiGianChiTietSuKienID
                VALUES (@MoTa, @BatDau, @KetThuc, @SuKienID)";

                    insertTimeCmd.Parameters.AddRange(new[]
                    {
                new SqlParameter("@MoTa", tg.MoTa ?? (object)DBNull.Value),
                new SqlParameter("@BatDau", tg.ThoiGianBatDau),
                new SqlParameter("@KetThuc", tg.ThoiGianKetThuc),
                new SqlParameter("@SuKienID", suKienId)
            });

                    var tgId = (int)(await insertTimeCmd.ExecuteScalarAsync());

                    foreach (var tm in tg.TietMuc)
                    {
                        var insertTmCmd = conn.CreateCommand();
                        insertTmCmd.Transaction = transaction;
                        insertTmCmd.CommandText = @"
                    INSERT INTO TietMucSuKien (TenTietMuc, NguoiThucHien, ChiPhiTietMuc, ThoiGianChiTietSuKienID)
                    VALUES (@Ten, @Nguoi, @ChiPhi, @TGID)";

                        insertTmCmd.Parameters.AddRange(new[]
                        {
                    new SqlParameter("@Ten", tm.TenTietMuc ?? (object)DBNull.Value),
                    new SqlParameter("@Nguoi", tm.NguoiThucHien ?? (object)DBNull.Value),
                    new SqlParameter("@ChiPhi", tm.ChiPhiTietMuc ?? (object)DBNull.Value),
                    new SqlParameter("@TGID", tgId)
                });

                        await insertTmCmd.ExecuteNonQueryAsync();
                    }
                }

                // 3. Insert ChiPhi + ChiTiet
                foreach (var cp in dto.ChiPhi)
                {
                    var insertCpCmd = conn.CreateCommand();
                    insertCpCmd.Transaction = transaction;
                    insertCpCmd.CommandText = @"
                INSERT INTO ChiPhiSuKien (SuKienID, TenKhoanChi, GhiChu, SoTien)
                OUTPUT INSERTED.ChiPhiID
                VALUES (@SuKienID, @Ten, @GhiChu, @SoTien)";

                    insertCpCmd.Parameters.AddRange(new[]
                    {
                new SqlParameter("@SuKienID", suKienId),
                new SqlParameter("@Ten", cp.TenKhoanChi ?? (object)DBNull.Value),
                new SqlParameter("@GhiChu", cp.GhiChu ?? (object)DBNull.Value),
                new SqlParameter("@SoTien", cp.SoTien)
            });

                    var chiPhiId = (int)(await insertCpCmd.ExecuteScalarAsync());

                    foreach (var ct in cp.ChiTiet)
                    {
                        var insertCtCmd = conn.CreateCommand();
                        insertCtCmd.Transaction = transaction;
                        insertCtCmd.CommandText = @"
                    INSERT INTO ChiTietChiPhiSuKien (ChiPhiID, TenPhanQua, SoLuong, DonGia, NguoiDaiDien)
                    VALUES (@ChiPhiID, @Ten, @SL, @DonGia, @Nguoi)";

                        insertCtCmd.Parameters.AddRange(new[]
                        {
                    new SqlParameter("@ChiPhiID", chiPhiId),
                    new SqlParameter("@Ten", ct.TenPhanQua ?? (object)DBNull.Value),
                    new SqlParameter("@SL", ct.SoLuong),
                    new SqlParameter("@DonGia", ct.DonGia),
                    new SqlParameter("@Nguoi", ct.NguoiDaiDien ?? (object)DBNull.Value)
                });

                        await insertCtCmd.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
                return Ok(new { message = "✅ Tạo sự kiện thành công!", suKienId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "❌ Lỗi khi tạo sự kiện.", error = ex.Message });
            }
        }

        [HttpGet("khuPho")]
        public async Task<IActionResult> GetKhuPho()
        {
            var result = await _context.KhuPhos
                .Select(k => new
                {
                    k.KhuPhoId,
                    k.TenKhuPho
                })
                .OrderBy(k => k.TenKhuPho)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("nguoiDung")]
        public async Task<IActionResult> GetNguoiDung()
        {
            var result = await _context.NguoiDungs
                .Select(u => new
                {
                    u.UserId,
                    HoTen = u.HoTen 
                })
                .OrderBy(u => u.HoTen)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("tinhNguyenVien")]
        public async Task<IActionResult> GetTinhNguyenVien()
        {
            var result = await _context.TinhNguyenViens
                .Select(t => new
                {
                    t.TinhNguyenVienId,
                    t.Sdt
                })
                .ToListAsync();

            return Ok(result);
        }

        // POST: api/SuKien/{id}/dangky
        [HttpPost("{id}/dangky")]
        public async Task<IActionResult> DangKy(int id, [FromBody] DangKySuKien dto)
         {
            var suKien = await _context.SuKiens.AnyAsync(s => s.SuKienID == id);
            if (!suKien) return NotFound("Sự kiện không tồn tại");

            var user = await _context.NguoiDungs.AnyAsync(u => u.UserId == dto.UserId);
            if (!user) return BadRequest("Người dùng không tồn tại");

            // Kiểm tra đã đăng ký chưa
            var daDangKy = await _context.DangKySuKiens
                .AnyAsync(d => d.SuKienId == id && d.UserId == dto.UserId);
            if (daDangKy) return BadRequest("Bạn đã đăng ký sự kiện này rồi!");

            var dangKy = new DangKySuKien
            {
                SuKienId = id,
                UserId = dto.UserId,
                NgayDangKy = DateOnly.FromDateTime(DateTime.Today),
                TrangThai = "Chờ duyệt"
            };

            _context.DangKySuKiens.Add(dangKy);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công! Vui lòng chờ duyệt." });
        }
      

        [HttpGet("getalldangkysukien")]
        public async Task<IActionResult> GetAllDangKySuKien()
        {
            var result = await _context.DangKySuKiens
                .Include(d => d.SuKien)
                .Include(d => d.User)
                .Select(d => new
                {
                    d.DangKySuKienId,
                    d.SuKienId,
                    TenSuKien = d.SuKien.TenSuKien,
                    d.UserId,
                    HoTenNguoiDung = d.User.HoTen,
                    d.NgayDangKy,
                    d.TrangThai
                })
                .OrderByDescending(d => d.NgayDangKy)
                .ToListAsync();

            return Ok(result);
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveDangKy(int id)
        {
            var dk = await _context.DangKySuKiens.FindAsync(id);
            if (dk == null)
                return NotFound("Không tìm thấy đăng ký.");

            dk.TrangThai = "Đã duyệt";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Duyệt đăng ký thành công!" });
        }

        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectDangKy(int id)
        {
            var dk = await _context.DangKySuKiens.FindAsync(id);
            if (dk == null)
                return NotFound("Không tìm thấy đăng ký.");

            dk.TrangThai = "Từ chối";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Từ chối đăng ký thành công!" });
        }

        [HttpDelete("deletedangky/{id}")]
        public async Task<IActionResult> DeleteDangKy(int id)
        {
            var dk = await _context.DangKySuKiens.FindAsync(id);
            if (dk == null)
                return NotFound("Không tìm thấy đăng ký.");

            _context.DangKySuKiens.Remove(dk);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa đăng ký thành công!" });
        }

        [HttpPut("{id}/updateAll")]
        public async Task<IActionResult> UpdateFullSuKien(int id, [FromBody] SuKienDTOCreate model)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                // 1. Cập nhật SuKien
                var updateSuKienCmd = connection.CreateCommand();
                updateSuKienCmd.Transaction = transaction;
                updateSuKienCmd.CommandText = @"
            UPDATE SuKien
            SET TenSuKien = @TenSuKien,
                MoTa = @MoTa,
                NgayBatDau = @NgayBatDau,
                NgayKetThuc = @NgayKetThuc,
                DiaDiem = @DiaDiem,
                NguoiChiuTrachNhiem = @NguoiChiuTrachNhiem
            WHERE SuKienID = @SuKienID";

                updateSuKienCmd.Parameters.Add(new SqlParameter("@SuKienID", id));
                updateSuKienCmd.Parameters.Add(new SqlParameter("@TenSuKien", model.TenSuKien ?? (object)DBNull.Value));
                updateSuKienCmd.Parameters.Add(new SqlParameter("@MoTa", model.MoTa ?? (object)DBNull.Value));
                updateSuKienCmd.Parameters.Add(new SqlParameter("@NgayBatDau", model.NgayBatDau));
                updateSuKienCmd.Parameters.Add(new SqlParameter("@NgayKetThuc", model.NgayKetThuc));
                updateSuKienCmd.Parameters.Add(new SqlParameter("@DiaDiem", model.DiaDiem ?? (object)DBNull.Value));
                updateSuKienCmd.Parameters.Add(new SqlParameter("@NguoiChiuTrachNhiem", model.NguoiChiuTrachNhiem ?? (object)DBNull.Value));

                await updateSuKienCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "✅ Cập nhật sự kiện thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "❌ Cập nhật thất bại! Dữ liệu đã được rollback.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("{id}/dangkytreem")]
        public async Task<IActionResult> DangKyTreEm(int id, [FromBody] int treEmId)
        {
            var suKien = await _context.SuKiens.FindAsync(id);
            if (suKien == null)
                return NotFound("Sự kiện không tồn tại.");

            var treEm = await _context.TreEms.FindAsync(treEmId);
            if (treEm == null)
                return NotFound("Trẻ em không tồn tại.");

            var daDangKy = await _context.TreEmSuKiens
                .AnyAsync(t => t.SuKienId == id && t.TreEmId == treEmId);

            if (daDangKy)
                return BadRequest("Trẻ em này đã đăng ký sự kiện này rồi!");

            var treEmSuKien = new TreEmSuKien
            {
                TreEmId = treEmId,
                SuKienId = id,
                NgayDangKy = DateTime.Now,
                TrangThai = "Đã dăng ký"
            };

            _context.TreEmSuKiens.Add(treEmSuKien);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký trẻ em cho sự kiện thành công!" });
        }
        [HttpGet("{id}/trethamgiaSK")]
        public async Task<IActionResult> GetTreEmThamGia(int id)
        {
            var suKien = await _context.SuKiens.FindAsync(id);
            if (suKien == null)
                return NotFound("Không tìm thấy sự kiện.");

            var treEmList = await _context.TreEmSuKiens
                .Where(ts => ts.SuKienId == id)
                .Include(ts => ts.TreEm)
                .Select(ts => new
                {
                    ts.TreEm.TreEmId,
                    ts.TreEm.HoTen,
                    ts.TreEm.NgaySinh,
                    ts.TreEm.GioiTinh,
                    ts.TreEm.TinhTrang,
                    ts.TreEm.Anh
                })
                .ToListAsync();

            if (!treEmList.Any())
                return Ok(new { message = "Chưa có trẻ nào tham gia sự kiện này." });

            return Ok(treEmList);
        }

        [HttpDelete("{id}/xoatreem/{treEmId}")]
        public async Task<IActionResult> XoaTreEmKhoiSuKien(int id, int treEmId)
        {
            var record = await _context.TreEmSuKiens
                .FirstOrDefaultAsync(t => t.SuKienId == id && t.TreEmId == treEmId);

            if (record == null)
                return NotFound("Trẻ em chưa đăng ký sự kiện này.");

            _context.TreEmSuKiens.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa trẻ khỏi danh sách tham gia sự kiện." });
        }

    }
}