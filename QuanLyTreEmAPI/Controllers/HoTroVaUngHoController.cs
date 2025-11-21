using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.DTOs.HoTroVaPhucLoi;
using QuanLyTreEmAPI.Models;
using QuanLyTreEmOKhuPho.Models.HoTroVaUngHo;
using System.Data;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoTroVaUngHoController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;

        public HoTroVaUngHoController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet("ThongKe")]
        public async Task<ActionResult<ThongKeHoTroThangDTO>> ThongKe()
        {
            var ngayDauThang = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
            var ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);

            var soLuongHoTro = _context.HoTroPhucLois.Count();
            var soLuongTreEm = _context.HoTroPhucLois
                .Select(h => h.TreEmId)
                .Distinct()
                .Count();
            decimal tongTien = _context.UngHos.Sum(u => u.SoTien) ?? 0;

            var soDotHoTro = await _context.HoTroPhucLois
                .Where(h => h.NgayCap.HasValue
                            && h.NgayCap.Value >= ngayDauThang
                            && h.NgayCap.Value <= ngayCuoiThang)
                .Select(h => h.NgayCap)
                .Distinct()
                .CountAsync();

            var result = new ThongKeHoTroThangDTO
            {
                SoLuongHoTro = soLuongHoTro,
                SoLuongTreEm = soLuongTreEm,
                TongTien = tongTien,
                SoDotHoTro = soDotHoTro,
            };

            return Ok(result);
        }

        [HttpGet("DanhSachHoTro")]
        public async Task<IActionResult> GetDanhSachHoTro()
        {
            try
            {
                var sqlQuery = @"
            SELECT 
                qtuh.QuaTangUngHoID AS QuaTangUngHoID,
                qtuh.TenQua,
                qtuh.MoTa,
                qtuh.DoiTuongNhan,
                qtuh.LoaiHoTro,
                qtuh.SoLuongTong,
                qtuh.SoLuongConLai,
                qtuh.DonGia,
                ISNULL(COUNT(DISTINCT ppq.TreEmID), 0) AS SoLuongTreEmDuocUngHo,
                ISNULL(SUM(CASE WHEN ppq.TrangThai = N'Đã phát' THEN 1 ELSE 0 END), 0) AS TreDaNhan,
                uh.SoTien,
                uh.NgayUngHo,
                mtq.Ten AS TenManhThuongQuan
            FROM QuaTangUngHo qtuh
            LEFT JOIN PhanPhatQua ppq ON qtuh.QuaTangUngHoID = ppq.QuaTangUngHoID
            LEFT JOIN UngHo uh ON qtuh.UngHoID = uh.UngHoID
            LEFT JOIN ManhThuongQuan mtq ON uh.ManhThuongQuanID = mtq.ManhThuongQuanID
            GROUP BY 
                qtuh.QuaTangUngHoID, qtuh.TenQua, qtuh.MoTa, qtuh.DoiTuongNhan,
                qtuh.LoaiHoTro, qtuh.SoLuongTong, qtuh.SoLuongConLai, qtuh.DonGia,
                uh.SoTien, uh.NgayUngHo, mtq.Ten
            ORDER BY qtuh.QuaTangUngHoID DESC
        ";

                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = sqlQuery;

                using var reader = await command.ExecuteReaderAsync();
                var danhSach = new List<object>();

                while (await reader.ReadAsync())
                {
                    danhSach.Add(new
                    {
                        QuaTangUngHoID = reader["QuaTangUngHoID"] != DBNull.Value ? (int)reader["QuaTangUngHoID"] : 0,
                        TenQua = reader["TenQua"]?.ToString() ?? "",
                        MoTa = reader["MoTa"]?.ToString() ?? "",
                        DoiTuongNhan = reader["DoiTuongNhan"]?.ToString() ?? "",
                        LoaiHoTro = reader["LoaiHoTro"]?.ToString() ?? "",

                        SoLuongTreEmDuocUngHo = reader["SoLuongTreEmDuocUngHo"] != DBNull.Value ? Convert.ToInt32(reader["SoLuongTreEmDuocUngHo"]) : 0,
                        TreDaNhan = reader["TreDaNhan"] != DBNull.Value ? Convert.ToInt32(reader["TreDaNhan"]) : 0,

                        SoTien = reader["SoTien"] != DBNull.Value ? Convert.ToDecimal(reader["SoTien"]) : 0m,
                        NgayUngHo = reader["NgayUngHo"] != DBNull.Value ? (DateTime?)reader["NgayUngHo"] : null,
                        TenManhThuongQuan = reader["TenManhThuongQuan"]?.ToString() ?? ""
                    });
                }

                return Ok(danhSach);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi lấy danh sách hỗ trợ",
                    error = ex.Message
                });
            }
        }
        [HttpGet("ChiTietQuaTang/{id}")]
        public async Task<ActionResult<QuaTangDTO>> GetChiTietQuaTang(int id)
        {
            try
            {
                var quaTang = await _context.QuaTangUngHos
                     .Where(qt => qt.QuaTangUngHoId == id)
                     .Include(qt => qt.PhanPhatQuas)
                         .ThenInclude(ppq => ppq.TreEm)
                     .Include(qt => qt.UngHo) // Include thông tin ủng hộ
                         .ThenInclude(u => u.ManhThuongQuan) // Include mạnh thường quân
                     .FirstOrDefaultAsync();


                if (quaTang == null)
                    return NotFound(new { message = "Không tìm thấy thông tin quà tặng" });
                var danhSachTreEm = quaTang.PhanPhatQuas
                                    .Where(ppq => ppq.TreEmId != null)
                                    .ToList();
                var tongSoLuongTreEm = danhSachTreEm.Count();
                var soLuongChuaPhat = danhSachTreEm.Count(x => x.TrangThai != "Đã nhận");

                // Số trẻ em đã nhận quà
                var soTreEmDaNhan = tongSoLuongTreEm - soLuongChuaPhat;

                decimal phanTramHoanThanh = tongSoLuongTreEm > 0
                    ? Math.Round((decimal)soTreEmDaNhan * 100 / tongSoLuongTreEm, 2)
                    : 0;

                var quaTangDTO = new QuaTangDTO
                {
                    QuaTangUngHoID = quaTang.QuaTangUngHoId,
                    TenQua = quaTang.TenQua ?? "",
                    MoTa = quaTang.MoTa ?? "",
                    SoLuongTreEm = tongSoLuongTreEm,
                    SoLuongTreEmDaNhan = soTreEmDaNhan,
                    SoLuongTreEmChuaNhanQua = soLuongChuaPhat,
                    PhanTrangHoanThanh = phanTramHoanThanh,
                    SoLuongConLai = quaTang.SoLuongConLai,
                    DoiTuongNhan = quaTang.DoiTuongNhan ?? "",
                    TenManhThuongQuan = quaTang.UngHo?.ManhThuongQuan?.Ten ?? "",
                    DiaChi = quaTang.UngHo?.ManhThuongQuan?.DiaChi ?? "",
                    SDT = quaTang.UngHo?.ManhThuongQuan?.Sdt ?? "",
                    LoaiUngHo = quaTang.UngHo?.LoaiUngHo ?? "",
                    GhiChu = quaTang.UngHo?.GhiChu ?? "",
                    NgayUngHo = quaTang.UngHo?.NgayUngHo.HasValue == true
                                ? quaTang.UngHo.NgayUngHo.Value.ToDateTime(TimeOnly.MinValue)
                                : (DateTime?)null,


                    DanhSachTreNhan = quaTang.PhanPhatQuas
                        .Where(ppq => ppq.TreEm != null)
                        .Select(ppq => new TreNhanQuaDTO
                        {
                            PhanPhatId = ppq.PhanPhatId,
                            TreEmID = ppq.TreEmId,
                            HoTen = ppq.TreEm.HoTen ?? "",
                            NgaySinh = ppq.TreEm.NgaySinh.HasValue
                                ? ppq.TreEm.NgaySinh.Value.ToDateTime(TimeOnly.MinValue)
                                : DateTime.MinValue,
                            TinhTrang = ppq.TreEm.TinhTrang ?? "",
                            SoLuongNhan = ppq.SoLuongNhan,
                            NgayPhanPhat = ppq.NgayPhanPhat,
                            NguoiPhanPhat = ppq.NguoiPhanPhat ?? "",
                            TrangThai = ppq.TrangThai ?? ""
                        })
                        .ToList()
                };

                return Ok(quaTangDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi lấy chi tiết quà tặng",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        [HttpGet("DanhSachUngHo")]
        public async Task<ActionResult<List<UngHoListDTO>>> DanhSachUngHo()
        {
            var danhSachUngHo = await _context.UngHos
              .Where(uh => uh.SoLuongConLai > 0)
              .Include(uh => uh.ManhThuongQuan)
              .OrderByDescending(uh => uh.NgayUngHo)
              .Select(uh => new UngHoListDTO
              {
                  UngHoID = uh.UngHoId,
                  GhiChu = uh.GhiChu,
                  NgayUngHo = uh.NgayUngHo.HasValue
                            ? uh.NgayUngHo.Value.ToDateTime(TimeOnly.MinValue)
                            : DateTime.MinValue,
                  NgayUngHoDisplay = uh.NgayUngHo.HasValue
                ? uh.NgayUngHo.Value.ToString("dd/MM/yyyy")
                : "",
                  TenManhThuongQuan = uh.ManhThuongQuan.Ten,
                  LoaiUngHo = uh.LoaiUngHo,
                  TenVatPham = uh.TenVatPham,
                  SoLuongVatPham = uh.SoLuongVatPham ?? 0,
                  SoLuongConLai = uh.SoLuongConLai ?? 0,
                  DoiTuong = uh.DoiTuong,
                  SoTien = uh.SoTien ?? 0,
                  // Lấy khu phố từ QuaTangUngHo -> SuKien -> KhuPho (nếu có)
                  TenKhuPho = _context.QuaTangUngHos
                      .Where(qt => qt.UngHoId == uh.UngHoId)
                      .Select(qt => qt.SuKien.KhuPho.TenKhuPho)
                      .FirstOrDefault() ?? "Tất cả",
                  DiaChiKhuPho = _context.QuaTangUngHos
                      .Where(qt => qt.UngHoId == uh.UngHoId)
                      .Select(qt => qt.SuKien.KhuPho.DiaChi)
                      .FirstOrDefault(),
                  QuanHuyen = _context.QuaTangUngHos
                      .Where(qt => qt.UngHoId == uh.UngHoId)
                      .Select(qt => qt.SuKien.KhuPho.QuanHuyen)
                      .FirstOrDefault(),
                  ThanhPho = _context.QuaTangUngHos
                      .Where(qt => qt.UngHoId == uh.UngHoId)
                      .Select(qt => qt.SuKien.KhuPho.ThanhPho)
                      .FirstOrDefault()
              })
              .ToListAsync();

            return Ok(danhSachUngHo);
        }
        [HttpGet("DanhSachTreEm")]
        public async Task<ActionResult<List<DsTreEm>>> DanhSachTreEm()
        {
            try
            {
                var danhSachTreEm = await _context.TreEms
                    .Include(te => te.KhuPho)
                    .OrderBy(te => te.HoTen)
                    .Select(te => new DsTreEm
                    {
                        TreEmId = te.TreEmId,
                        TenTreEm = te.HoTen,
                        NgaySinh = te.NgaySinh.HasValue
                                    ? te.NgaySinh.Value.ToDateTime(TimeOnly.MinValue)
                                    : (DateTime?)null, // chuyển DateOnly? -> DateTime?
                        NgaySinhDisplay = te.NgaySinh.HasValue
                                    ? te.NgaySinh.Value.ToString("dd/MM/yyyy")
                                    : "N/A",
                        KhuPho = te.KhuPho != null ? te.KhuPho.TenKhuPho : "N/A",
                        TinhTrang = te.TinhTrang
                    })
                    .ToListAsync();

                return Ok(danhSachTreEm);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi lấy danh sách trẻ em", error = ex.Message });
            }
        }
        [HttpPost("TaoHoTroVaPhanPhat")]
        public async Task<ActionResult<TaoHoTroVaPhanPhatResponseDTO>> TaoHoTroVaPhanPhat(TaoHoTroVaPhanPhatDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                UngHo ungHo;
                QuaTangUngHo quaTang;

                // 1. LẤY ĐỢT ỦNG HỘ
                if (request.UngHoId <= 0)
                    return BadRequest(new { message = "Vui lòng chọn đợt ủng hộ" });

                ungHo = await _context.UngHos.FirstOrDefaultAsync(u => u.UngHoId == request.UngHoId);

                if (ungHo == null)
                    return NotFound(new { message = "Không tìm thấy thông tin ủng hộ" });

                // Tính tổng số lượng cần phát
                int tongSoLuongPhat = request.DanhSachTreEmNhan.Sum(t => t.SoLuongNhan);

                if (!ungHo.SoLuongConLai.HasValue || ungHo.SoLuongConLai < tongSoLuongPhat)
                {
                    return BadRequest(new
                    {
                        message = $"Số lượng vật phẩm không đủ! Còn lại: {ungHo.SoLuongConLai ?? 0}, Cần: {tongSoLuongPhat}"
                    });
                }

                // 2. XỬ LÝ QUÀ TẶNG
                if (request.QuaTangUngHoId.HasValue)
                {
                    quaTang = await _context.QuaTangUngHos
                        .FirstOrDefaultAsync(qt => qt.QuaTangUngHoId == request.QuaTangUngHoId.Value);

                    if (quaTang == null)
                        return NotFound(new { message = "Không tìm thấy quà tặng" });

                    if (quaTang.SoLuongConLai < tongSoLuongPhat)
                    {
                        return BadRequest(new
                        {
                            message = $"Số lượng quà tặng không đủ! Còn lại: {quaTang.SoLuongConLai}, Cần: {tongSoLuongPhat}"
                        });
                    }
                }
                else
                {
                    // TẠO QUÀ TẶNG MỚI
                    quaTang = new QuaTangUngHo
                    {
                        UngHoId = ungHo.UngHoId,
                        SuKienId = request.SuKienId.HasValue && request.SuKienId > 0 ? request.SuKienId : null,
                        TenQua = request.TenQua ?? ungHo.TenVatPham,
                        MoTa = request.MoTaQua,
                        NguoiChiuTrachNhiem = request.NguoiChiuTrachNhiemHoTro,
                        LoaiHoTro = request.LoaiHoTro,
                        SoLuongTong = tongSoLuongPhat,
                        SoLuongConLai = tongSoLuongPhat,
                        DonGia = request.DonGia ?? 0,
                        DoiTuongNhan = request.DoiTuongNhan ?? ungHo.DoiTuong,
                        Anh = request.AnhQua
                    };

                    _context.QuaTangUngHos.Add(quaTang);

                    // Trừ số lượng từ ủng hộ
                    ungHo.SoLuongConLai -= tongSoLuongPhat;
                    _context.UngHos.Update(ungHo);

                    await _context.SaveChangesAsync();
                }

                // 3. LẤY DANH SÁCH TRẺ EM
                var treEmIds = request.DanhSachTreEmNhan.Select(t => t.TreEmId).ToList();

                var danhSachTreEm = await _context.TreEms
                    .Where(t => treEmIds.Contains(t.TreEmId))
                    .ToListAsync();

                if (danhSachTreEm.Count != treEmIds.Count)
                    return BadRequest(new { message = "Một số trẻ em không tồn tại!" });

                var danhSachPhanPhat = new List<PhanPhatQuaInfo>();

                // 4. TẠO PHÂN PHÁT QUÀ CHO TỪNG TRẺ
                foreach (var treEmNhan in request.DanhSachTreEmNhan)
                {
                    var ngayPP = request.NgayPhanPhat.ToDateTime(TimeOnly.MinValue);

                    // Kiểm tra trùng phát trong ngày
                    var trungLap = await _context.PhanPhatQuas.AnyAsync(pp =>
                        pp.QuaTangUngHoId == quaTang.QuaTangUngHoId &&
                        pp.TreEmId == treEmNhan.TreEmId &&
                        pp.NgayPhanPhat.Date == ngayPP.Date
                    );

                    if (trungLap)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            message = $"Trẻ ID {treEmNhan.TreEmId} đã nhận quà ngày {request.NgayPhanPhat:dd/MM/yyyy}"
                        });
                    }

                    var phanPhat = new PhanPhatQua
                    {
                        QuaTangUngHoId = quaTang.QuaTangUngHoId,
                        TreEmId = treEmNhan.TreEmId,
                        SoLuongNhan = treEmNhan.SoLuongNhan,
                        NgayPhanPhat = ngayPP,
                        NguoiPhanPhat = request.NguoiPhanPhat,
                        TrangThai = request.TrangThaiPhat ?? "Chưa phát",
                        GhiChu = treEmNhan.GhiChu ?? request.GhiChuPhanPhat
                    };

                    _context.PhanPhatQuas.Add(phanPhat);
                    await _context.SaveChangesAsync();

                    var treEm = danhSachTreEm.First(t => t.TreEmId == treEmNhan.TreEmId);

                    danhSachPhanPhat.Add(new PhanPhatQuaInfo
                    {
                        PhanPhatId = phanPhat.PhanPhatId,
                        TreEmId = treEm.TreEmId,
                        HoTenTreEm = treEm.HoTen,
                        SoLuongNhan = treEmNhan.SoLuongNhan,
                        TrangThai = request.TrangThaiPhat ?? "Chưa phát"
                    });
                }

                // 5. CẬP NHẬT SỐ LƯỢNG QUÀ
                if (request.TrangThaiPhat == "Đã phát")
                {
                    quaTang.SoLuongConLai -= tongSoLuongPhat;
                    _context.QuaTangUngHos.Update(quaTang);
                }
                else if (request.QuaTangUngHoId.HasValue)
                {
                    ungHo.SoLuongConLai -= tongSoLuongPhat;
                    _context.UngHos.Update(ungHo);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Đã phân phát quà cho {request.DanhSachTreEmNhan.Count} trẻ",
                    UngHoId = ungHo.UngHoId,
                    QuaTangUngHoId = quaTang.QuaTangUngHoId,
                    SoTreEmDaNhan = request.DanhSachTreEmNhan.Count,
                    TongSoLuongPhat = tongSoLuongPhat,
                    SoLuongConLaiQuaTang = quaTang.SoLuongConLai,
                    SoLuongConLaiUngHo = ungHo.SoLuongConLai ?? 0,
                    DanhSachPhanPhat = danhSachPhanPhat
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi xử lý", error = ex.Message });
            }
        }

        [HttpPost("DoiTrangThai")]
        public async Task<IActionResult> DoiTrangThai(int id, string TrangThai)
        {
            if (string.IsNullOrEmpty(TrangThai))
                return BadRequest("Trạng thái không hợp lệ.");

            // Tìm bản ghi phân phát quà theo ID
            var phanPhat = await _context.PhanPhatQuas
                .FirstOrDefaultAsync(p => p.PhanPhatId == id);

            if (phanPhat == null)
                return NotFound("Không tìm thấy phân phát quà.");

            // Cập nhật trạng thái
            phanPhat.TrangThai = TrangThai;
            phanPhat.NgayPhanPhat = DateTime.Today;

            // Lưu DB
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật trạng thái thành công!",
                id = phanPhat.PhanPhatId,
                trangThai = phanPhat.TrangThai
            });
        }

        [HttpDelete("XoaTreKhoiDanhSachPhatQua/{phanPhatId}")]
        public async Task<IActionResult> XoaTreKhoiDanhSachPhatQua(int phanPhatId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var phanPhat = await _context.PhanPhatQuas
                    .Include(pp => pp.QuaTangUngHo)
                    .FirstOrDefaultAsync(pp => pp.PhanPhatId == phanPhatId);

                if (phanPhat == null)
                    return NotFound(new { message = "Không tìm thấy bản ghi phát quà" });

                // Hoàn lại số lượng quà
                if (phanPhat.QuaTangUngHo != null)
                {
                    phanPhat.QuaTangUngHo.SoLuongConLai += phanPhat.SoLuongNhan;
                    _context.QuaTangUngHos.Update(phanPhat.QuaTangUngHo);
                }

                _context.PhanPhatQuas.Remove(phanPhat);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Xóa trẻ em khỏi danh sách phát quà thành công",
                    treEmId = phanPhat.TreEmId,
                    phanPhatId = phanPhatId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi xóa",
                    error = ex.Message
                });
            }
        }
        [HttpGet("ChiTietThongTinQuaTang")]
        public async Task<IActionResult> ChiTietQuaTang(int quaTangUngHoId)
        {
            var data = await _context.QuaTangUngHos
                .Where(q => q.QuaTangUngHoId == quaTangUngHoId)
                .Include(q => q.UngHo)
                    .ThenInclude(u => u.ManhThuongQuan)
                .Include(q => q.PhanPhatQuas)
                    .ThenInclude(pp => pp.TreEm)
                .Select(q => new
                {
                    TenManhThuongQuan = q.UngHo.ManhThuongQuan.Ten,

                    UngHoId = q.UngHo.UngHoId,
                    LoaiUngHo = q.UngHo.LoaiUngHo,
                    TenVatPham = q.UngHo.TenVatPham,
                    SoLuongVatPham = q.UngHo.SoLuongVatPham,
                    SoLuongConLai = q.UngHo.SoLuongConLai,
                    NgayUngHo = q.UngHo.NgayUngHo,
                    DoiTuong = q.UngHo.DoiTuong,
                    LoaiHoTro = q.LoaiHoTro,
                    QuaTangUngHoId = q.QuaTangUngHoId,
                    TenQua = q.TenQua,
                    MoTaQua = q.MoTa,
                    DoiTuongNhan = q.DoiTuongNhan,
                    SoLuongConLaiQuaTang = q.SoLuongConLai,
                    NguoiChiuTrachNhiem = q.NguoiChiuTrachNhiem,
                    NguoiPhanPhat = q.PhanPhatQuas
                        .Select(pp => pp.NguoiPhanPhat)
                        .FirstOrDefault(),

                    // ✅ Danh sách ID phân phát
                    PhanPhatQuaIds = q.PhanPhatQuas
                        .Select(pp => pp.PhanPhatId)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return Ok(data);
        }
        [HttpPost("SuaUngHo")]
        public async Task<IActionResult> SuaUngHo(SuaUngHoDTO request)
        {
            // 1. Lấy quà tặng từ DB kèm phân phát
            var qua = await _context.QuaTangUngHos
                .Include(q => q.PhanPhatQuas)
                .FirstOrDefaultAsync(q => q.QuaTangUngHoId == request.QuaTangUngHoId);

            if (qua == null)
                return NotFound(new { message = "Không tìm thấy quà tặng để cập nhật." });

            // 2. Cập nhật thông tin chính
            qua.LoaiHoTro = request.LoaiHoTro;
            qua.NguoiChiuTrachNhiem = request.NguoiChiuTrachNhiem;
            qua.MoTa = request.MoTaQua;

            if (!string.IsNullOrEmpty(request.TenQuaTang))
                qua.TenQua = request.TenQuaTang;

            if (!string.IsNullOrEmpty(request.DoiTuongNhan))
                qua.DoiTuongNhan = request.DoiTuongNhan;

            // 3. Cập nhật NGƯỜI NHẬN PHÁT CHO CÁC PHÂN PHÁT ĐƯỢC CHỌN
            if (!string.IsNullOrEmpty(request.NguoiNhanPhat) && request.PhanPhatQuaIds?.Any() == true)
            {
                foreach (var pp in qua.PhanPhatQuas.Where(pp => request.PhanPhatQuaIds.Contains(pp.PhanPhatId)))
                {
                    pp.NguoiPhanPhat = request.NguoiNhanPhat;
                }
            }

            // 4. Lưu thay đổi
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật hỗ trợ & quà tặng thành công!",
                qua.QuaTangUngHoId,
                qua.TenQua,
                qua.NguoiChiuTrachNhiem
            });
        }
        [HttpGet("LocDanhSachTreEm")]
        public async Task<ActionResult<List<DsTreEm>>> LocDanhSachTreEm(int quaTangUngHoId)
        {
            try
            {
                var treDaNhanIds = await _context.PhanPhatQuas
                    .Where(pp => pp.QuaTangUngHoId == quaTangUngHoId)
                    .Select(pp => pp.TreEmId)
                    .ToListAsync();

                var danhSachTreEm = await _context.TreEms
                    .Include(te => te.KhuPho)
                    .Where(te => !treDaNhanIds.Contains(te.TreEmId))
                    .OrderBy(te => te.HoTen)
                    .Select(te => new DsTreEm
                    {
                        TreEmId = te.TreEmId,
                        TenTreEm = te.HoTen,
                        NgaySinh = te.NgaySinh.HasValue
                                    ? te.NgaySinh.Value.ToDateTime(TimeOnly.MinValue)
                                    : (DateTime?)null,
                        NgaySinhDisplay = te.NgaySinh.HasValue
                                    ? te.NgaySinh.Value.ToString("dd/MM/yyyy")
                                    : "N/A",
                        KhuPho = te.KhuPho != null ? te.KhuPho.TenKhuPho : "N/A",
                        TinhTrang = te.TinhTrang
                    })
                    .ToListAsync();

                return Ok(danhSachTreEm);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi lấy danh sách trẻ em",
                    error = ex.Message
                });
            }
        }
        [HttpPost("ThemTreVaoPhanPhat")]
        public async Task<IActionResult> ThemTreVaoPhanPhat([FromBody] ThemTreVaoPhanPhatDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Lấy thông tin quà
                var quaTang = await _context.QuaTangUngHos
                    .FirstOrDefaultAsync(q => q.QuaTangUngHoId == request.QuaTangUngHoId);

                if (quaTang == null)
                    return NotFound(new { message = "Không tìm thấy quà tặng ủng hộ" });

                if (request.DanhSachTreEm == null || !request.DanhSachTreEm.Any())
                    return BadRequest(new { message = "Danh sách trẻ em trống!" });

                // Ngày phát là hôm nay
                var ngayPhat = DateTime.Today;

                // 2. Tính số lượng cần phát
                int tongSoPhat = request.DanhSachTreEm.Sum(x => x.SoLuongNhan);

                if (quaTang.SoLuongConLai < tongSoPhat)
                {
                    return BadRequest(new
                    {
                        message = $"Không đủ số lượng quà. Còn lại: {quaTang.SoLuongConLai}, Cần: {tongSoPhat}"
                    });
                }

                // 3. Kiểm tra trùng – không cho trẻ nhận trùng quà cùng ngày
                var treIds = request.DanhSachTreEm.Select(t => t.TreEmId).ToList();

                var trungLap = await _context.PhanPhatQuas
                    .AnyAsync(pp =>
                        pp.QuaTangUngHoId == request.QuaTangUngHoId &&
                        treIds.Contains(pp.TreEmId) &&
                        pp.NgayPhanPhat.Date == ngayPhat.Date
                    );

                if (trungLap)
                {
                    return BadRequest(new
                    {
                        message = "Một hoặc nhiều trẻ đã nhận quà này trong ngày hôm nay!"
                    });
                }

                // 4. Thêm mới danh sách phân phát
                foreach (var tre in request.DanhSachTreEm)
                {
                    var phanPhat = new PhanPhatQua
                    {
                        QuaTangUngHoId = request.QuaTangUngHoId,
                        TreEmId = tre.TreEmId,
                        SoLuongNhan = tre.SoLuongNhan,
                        NgayPhanPhat = ngayPhat,
                        NguoiPhanPhat = request.NguoiPhat,
                        TrangThai = "Chưa phát",
                        GhiChu = tre.GhiChu
                    };

                    _context.PhanPhatQuas.Add(phanPhat);
                }

                // 5. Trừ số lượng quà còn lại
                quaTang.SoLuongConLai -= tongSoPhat;
                _context.QuaTangUngHos.Update(quaTang);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = $"Đã thêm {request.DanhSachTreEm.Count} trẻ vào phân phát quà",
                    QuaTangUngHoId = request.QuaTangUngHoId,
                    TongSoLuongPhat = tongSoPhat,
                    SoLuongConLaiMoi = quaTang.SoLuongConLai
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi xử lý", error = ex.Message });
            }
        }


        [HttpGet("SoLuongQuaConLai")]
        public async Task<IActionResult> SoLuongQuaConLai([FromQuery] int quaTangUngHoId)
        {
            var quaTang = await _context.QuaTangUngHos
                .FirstOrDefaultAsync(x => x.QuaTangUngHoId == quaTangUngHoId);

            if (quaTang == null)
                return NotFound(new { message = "Không tìm thấy quà tặng ủng hộ" });

            return Ok(new
            {
                QuaTangUngHoId = quaTangUngHoId,
                SoLuongConLai = quaTang.SoLuongConLai,
                TenQuaTang = quaTang.TenQua
            });
        }

    }
}


