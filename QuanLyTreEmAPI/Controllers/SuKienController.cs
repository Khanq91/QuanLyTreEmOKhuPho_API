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
            var sk = _context.SuKiens.ToList();
            var suKiens = _context.SuKiens
       .ToList() // ⚠️ Chuyển về memory
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
                    .ThenInclude(t => t.TietMucSuKiens)
                .Include(x => x.ChiPhiSuKiens)
                    .ThenInclude(cp => cp.ChiTietChiPhiSuKiens)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SuKienID == id);


            if (sk == null)
                return NotFound(new { message = "Không tìm thấy sự kiện" });

            var soTNVDaDangKy = await _context.DangKySuKiens
                .Where(x => x.SuKienId == id && x.TrangThai == "Đã duyệt")
                .CountAsync();
            var soTreEmDaDangKy = await _context.TreEmSuKiens
                .CountAsync(x => x.SuKienId == id && x.TrangThai == "Đã đăng ký");

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

            // ✅ Tính tổng chi phí
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
                // Thời gian chi tiết và tiết mục
                ThoiGianChiTiet = sk.ThoiGianChiTietSuKiens
                    .OrderBy(t => t.ThoiGianBatDau)
                    .Select(t => new
                    {
                        t.ThoiGianChiTietSuKienId,
                        t.MoTa,
                        t.ThoiGianBatDau,
                        t.ThoiGianKetThuc,
                        TietMuc = t.TietMucSuKiens.Select(tm => new
                        {
                            tm.TietMucId,
                            tm.TenTietMuc,
                            tm.NguoiThucHien,
                            tm.ChiPhiTietMuc
                        }).ToList(),
                        TongChiPhiTietMuc = t.TietMucSuKiens.Sum(tm => tm.ChiPhiTietMuc ?? 0)
                    }).ToList(),
                TietMuc = sk.TietMucSuKiens.ToList(),
                // Chi phí sự kiện
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

                // Quà tặng
                QuaTang = quaTang,

                // Tổng hợp
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

                string imagePath = null;

                if (!string.IsNullOrEmpty(dto.AnhSuKien))
                {
                    // Nếu FE gửi base64
                    if (dto.AnhSuKien.StartsWith("data:image"))
                    {
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                                      "wwwroot", "Anh", "SuKien");
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

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
                    KhuPhoId = dto.KhuPhoId,
                    AnhSuKien = imagePath       
                };

                _context.SuKiens.Add(suKien);
                await _context.SaveChangesAsync();

                foreach (var tgDto in dto.ThoiGianChiTiet)
                {
                    var tg = new ThoiGianChiTietSuKien
                    {
                        MoTa = tgDto.MoTa,
                        ThoiGianBatDau = tgDto.ThoiGianBatDau,
                        ThoiGianKetThuc = tgDto.ThoiGianKetThuc,
                        SuKienId = suKien.SuKienID
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
                        SuKienID = suKien.SuKienID
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
                        SuKienId = suKien.SuKienID,
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
                return await GetDetail(suKien.SuKienID);
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
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var suKien = await _context.SuKiens
                    .Include(x => x.TietMucSuKiens)
                    .Include(x => x.ChiPhiSuKiens)
                        .ThenInclude(cp => cp.ChiTietChiPhiSuKiens)
                    .FirstOrDefaultAsync(x => x.SuKienID == id);

                if (suKien == null)
                    return NotFound("Không tìm thấy sự kiện.");

                suKien.TenSuKien = model.TenSuKien;
                suKien.MoTa = model.MoTa;
                suKien.NgayBatDau = model.NgayBatDau;
                suKien.NgayKetThuc = model.NgayKetThuc;
                suKien.DiaDiem = model.DiaDiem;
                suKien.NguoiChiuTrachNhiem = model.NguoiChiuTrachNhiem;
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
                        old.ThoiGianChiTietSuKienId = newTm.ThoiGianChiTietSuKienId;
                    }
                }

                foreach (var tm in model.TietMuc.Where(x => x.TietMucId == 0))
                {
                    _context.TietMucSuKiens.Add(new TietMucSuKien
                    {
                        TenTietMuc = tm.TenTietMuc,
                        NguoiThucHien = tm.NguoiThucHien,
                        ChiPhiTietMuc = tm.ChiPhiTietMuc,
                        ThoiGianChiTietSuKienId = tm.ThoiGianChiTietSuKienId,
                        SuKienID = suKien.SuKienID // ✅ Gán đúng SuKienId cho tiết mục mới
                    });
                }
                foreach (var oldCp in suKien.ChiPhiSuKiens.ToList())
                {
                    var newCp = model.ChiPhi.FirstOrDefault(x => x.ChiPhiId == oldCp.ChiPhiId);

                    if (newCp == null)
                    {
                        var phanBo = _context.PhanBoUngHoChiPhis.Where(p => p.ChiPhiId == oldCp.ChiPhiId).ToList();
                        if (phanBo.Any()) _context.PhanBoUngHoChiPhis.RemoveRange(phanBo);

                        var ct = oldCp.ChiTietChiPhiSuKiens.ToList();
                        if (ct.Any()) _context.ChiTietChiPhiSuKiens.RemoveRange(ct);

                        _context.ChiPhiSuKiens.Remove(oldCp);
                        continue;
                    }

                    oldCp.TenKhoanChi = newCp.TenKhoanChi;
                    oldCp.GhiChu = newCp.GhiChu;
                    oldCp.SoTien = newCp.SoTien;

                    foreach (var oldCt in oldCp.ChiTietChiPhiSuKiens.ToList())
                    {
                        var newCt = newCp.ChiTiet.FirstOrDefault(c => c.ChiTietId == oldCt.ChiPhiId);

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

                foreach (var addCp in model.ChiPhi.Where(x => x.ChiPhiId == 0))
                {
                    suKien.ChiPhiSuKiens.Add(new ChiPhiSuKien
                    {
                        TenKhoanChi = addCp.TenKhoanChi,
                        GhiChu = addCp.GhiChu,
                        SoTien = addCp.SoTien,
                        SuKienID = suKien.SuKienID, 
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