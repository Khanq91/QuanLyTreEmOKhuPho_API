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
       .Select(e => new
       {
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
                .Select(e => new
                {
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
                .Select(e => new
                {
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
                .Select(e => new
                {
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
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SuKienId == id);

            if (sk == null)
                return NotFound("Không tìm thấy sự kiện");

            // 🔹 Load tiết mục theo SuKienId (độc lập với thời gian chi tiết)
            var tietMucs = await _context.TietMucSuKiens
                .Where(t => t.SuKienId == id)
                .Select(t => new
                {
                    t.TietMucId,
                    t.TenTietMuc,
                    t.NguoiThucHien,
                    t.ChiPhiTietMuc,
                    t.ThoiGianChiTietSuKienId
                })
                .ToListAsync();

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
                ThoiGianChiTiet = sk.ThoiGianChiTietSuKiens.Select(t => new
                {
                    t.ThoiGianChiTietSuKienId,
                    t.MoTa,
                    t.ThoiGianBatDau,
                    t.ThoiGianKetThuc
                }),
                TietMuc = tietMucs,
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
                        ct.DonGia,
                        ThanhTien = ct.SoLuong * ct.DonGia
                    })
                })
            };

            return Ok(result);
        }


        #endregion

        // POST: api/SuKien
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SuKienDTOCreate dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra tồn tại
            if (!await _context.KhuPhos.AnyAsync(k => k.KhuPhoId == dto.KhuPhoId))
                return BadRequest(new { message = "Khu phố không tồn tại." });

            if (!await _context.NguoiDungs.AnyAsync(u => u.UserId == dto.UserId))
                return BadRequest(new { message = "Người dùng không tồn tại." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo SuKien
                var suKien = new SuKien
                {
                    TenSuKien = dto.TenSuKien,
                    MoTa = dto.MoTa,
                    DiaDiem = dto.DiaDiem,
                    NgayBatDau = dto.NgayBatDau,
                    NgayKetThuc = dto.NgayKetThuc,
                    NguoiChiuTrachNhiem = dto.NguoiChiuTrachNhiem,
                    SoLuongTinhNguyenVien = dto.SoLuongTinhNguyenVien,
                    SoLuongTreEm = dto.SoLuongTreEm,
                    UserId = dto.UserId,
                    KhuPhoId = dto.KhuPhoId
                };

                _context.SuKiens.Add(suKien);
                await _context.SaveChangesAsync(); // Lấy ID

                // 2. Thêm Thời gian chi tiết + Tiết mục
                foreach (var tgDto in dto.ThoiGianChiTiet)
                {
                    var tg = new ThoiGianChiTietSuKien
                    {
                        MoTa = tgDto.MoTa,
                        ThoiGianBatDau = tgDto.ThoiGianBatDau,
                        ThoiGianKetThuc = tgDto.ThoiGianKetThuc,
                        SuKienId = suKien.SuKienId
                    };
                    _context.ThoiGianChiTietSuKiens.Add(tg);
                    await _context.SaveChangesAsync();

                    foreach (var tmDto in tgDto.TietMuc)
                    {
                        var tm = new TietMucSuKien
                        {
                            TenTietMuc = tmDto.TenTietMuc,
                            NguoiThucHien = tmDto.NguoiThucHien,
                            ChiPhiTietMuc = tmDto.ChiPhiTietMuc,
                            ThoiGianChiTietSuKienId = tg.ThoiGianChiTietSuKienId,
                        };
                        _context.TietMucSuKiens.Add(tm);
                    }
                }

                // 3. Thêm Chi phí + Chi tiết
                foreach (var cpDto in dto.ChiPhi)
                {
                    var cp = new ChiPhiSuKien
                    {
                        TenKhoanChi = cpDto.TenKhoanChi,
                        SoTien = cpDto.SoTien,
                        GhiChu = cpDto.GhiChu,
                        SuKienId = suKien.SuKienId
                    };
                    _context.ChiPhiSuKiens.Add(cp);
                    await _context.SaveChangesAsync();

                    foreach (var ctDto in cpDto.ChiTiet)
                    {
                        var ct = new ChiTietChiPhiSuKien
                        {
                            TenPhanQua = ctDto.TenPhanQua,
                            NguoiDaiDien = ctDto.NguoiDaiDien,
                            SoLuong = ctDto.SoLuong,
                            DonGia = ctDto.DonGia,
                            ChiPhiId = cp.ChiPhiId
                        };
                        _context.ChiTietChiPhiSuKiens.Add(ct);
                    }
                }

                // 4. Thêm Phân công
                foreach (var pcDto in dto.PhanCong)
                {
                    if (!await _context.TinhNguyenViens.AnyAsync(t => t.TinhNguyenVienId == pcDto.TinhNguyenVienId))
                        return BadRequest(new { message = $"Tình nguyện viên ID {pcDto.TinhNguyenVienId} không tồn tại." });

                    var pc = new PhanCongTinhNguyenVien
                    {
                        SuKienId = suKien.SuKienId,
                        TinhNguyenVienId = pcDto.TinhNguyenVienId,
                        CongViec = pcDto.CongViec,
                        GhiChu = pcDto.GhiChu,
                        NgayPhanCong = DateOnly.FromDateTime(DateTime.Today)
                    };
                    _context.PhanCongTinhNguyenViens.Add(pc);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Trả về đúng định dạng GetDetail
                return await GetDetail(suKien.SuKienId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi khi tạo sự kiện.", error = ex.Message });
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
            var suKien = await _context.SuKiens.AnyAsync(s => s.SuKienId == id);
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
        // =============================================================
        // QUẢN LÝ ĐĂNG KÝ SỰ KIỆN
        // =============================================================

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
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var suKien = await _context.SuKiens
                    .Include(x => x.TietMucSuKiens)
                    .Include(x => x.ChiPhiSuKiens)
                        .ThenInclude(cp => cp.ChiTietChiPhiSuKiens)
                    .FirstOrDefaultAsync(x => x.SuKienId == id);

                if (suKien == null)
                    return NotFound("Không tìm thấy sự kiện.");

                // === Cập nhật phần chính ===
                suKien.TenSuKien = model.TenSuKien;
                suKien.MoTa = model.MoTa;
                suKien.NgayBatDau = model.NgayBatDau;
                suKien.NgayKetThuc = model.NgayKetThuc;
                suKien.DiaDiem = model.DiaDiem;
                suKien.NguoiChiuTrachNhiem = model.NguoiChiuTrachNhiem;

                // === Cập nhật TIẾT MỤC ===
                var oldTietMuc = suKien.TietMucSuKiens.ToList();
                foreach (var old in oldTietMuc)
                {
                    var newTm = model.TietMuc.FirstOrDefault(x => x.TietMucId == old.TietMucId);
                    if (newTm == null)
                    {
                        _context.TietMucSuKiens.Remove(old);
                    }
                    else
                    {
                        old.TenTietMuc = newTm.TenTietMuc;
                        old.NguoiThucHien = newTm.NguoiThucHien;
                        old.ChiPhiTietMuc = newTm.ChiPhiTietMuc;
                    }
                }

                // Thêm mới tiết mục nếu có
                foreach (var tm in model.TietMuc.Where(x => x.TietMucId == 0))
                {
                    suKien.TietMucSuKiens.Add(new TietMucSuKien
                    {
                        TenTietMuc = tm.TenTietMuc,
                        ThoiGianChiTietSuKienId = 1,
                        NguoiThucHien = tm.NguoiThucHien,
                        ChiPhiTietMuc = tm.ChiPhiTietMuc
                    });
                }

                // === Cập nhật CHI PHÍ ===
                foreach (var oldCp in suKien.ChiPhiSuKiens.ToList())
                {
                    var newCp = model.ChiPhi.FirstOrDefault(x => x.ChiPhiId == oldCp.ChiPhiId);
                    if (newCp == null)
                    {
                        _context.ChiPhiSuKiens.Remove(oldCp);
                        continue;
                    }

                    // Cập nhật phần chính
                    oldCp.TenKhoanChi = newCp.TenKhoanChi;
                    oldCp.GhiChu = newCp.GhiChu;
                    oldCp.SoTien = newCp.SoTien;

                    // === Cập nhật CHI TIẾT CHI PHÍ ===
                    foreach (var oldCt in oldCp.ChiTietChiPhiSuKiens.ToList())
                    {
                        var newCt = newCp.ChiTiet.FirstOrDefault(c => c.ChiTietId == oldCt.ChiTietChiPhi);
                        if (newCt == null)
                            _context.ChiTietChiPhiSuKiens.Remove(oldCt);
                        else
                        {
                            oldCt.TenPhanQua = newCt.TenPhanQua;
                            oldCt.SoLuong = newCt.SoLuong;
                            oldCt.DonGia = newCt.DonGia;
                            oldCt.NguoiDaiDien = newCt.NguoiDaiDien;
                        }
                    }

                    // Thêm chi tiết mới
                    foreach (var addCt in newCp.ChiTiet.Where(c => c.ChiTietId == 0))
                    {
                        oldCp.ChiTietChiPhiSuKiens.Add(new ChiTietChiPhiSuKien
                        {
                            TenPhanQua = addCt.TenPhanQua,
                            SoLuong = addCt.SoLuong,
                            DonGia = addCt.DonGia,
                            NguoiDaiDien = addCt.NguoiDaiDien
                        });
                    }
                }

                // === Thêm khoản chi phí mới nếu có ===
                foreach (var addCp in model.ChiPhi.Where(x => x.ChiPhiId == 0))
                {
                    suKien.ChiPhiSuKiens.Add(new ChiPhiSuKien
                    {
                        TenKhoanChi = addCp.TenKhoanChi,
                        GhiChu = addCp.GhiChu,
                        SoTien = addCp.SoTien,
                        ChiTietChiPhiSuKiens = addCp.ChiTiet.Select(c => new ChiTietChiPhiSuKien
                        {
                            TenPhanQua = c.TenPhanQua,
                            SoLuong = c.SoLuong,
                            DonGia = c.DonGia,
                            NguoiDaiDien = c.NguoiDaiDien
                        }).ToList()
                    });
                }

                await _context.SaveChangesAsync();
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

        // tre em dang ky tham gia su kien
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
                SuKienId = id
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