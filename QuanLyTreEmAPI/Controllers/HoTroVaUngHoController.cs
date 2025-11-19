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

        // ============================================
        // API 1: DANH SÁCH ỦNG HỘ - CẢI TIẾN
        // ============================================
        [HttpGet("DanhSachHoTro")]
        public async Task<IActionResult> GetDanhSachHoTro()
        {
            try
            {
                var sqlQuery = @"
            SELECT 
                uh.UngHoID as HoTroID,
                uh.LoaiUngHo,
                uh.GhiChu as MoTa,
                uh.NgayUngHo as NgayUngHo,
                m.Ten as TenManhThuongQuan,
                ISNULL(uh.SoTien, 0) as SoTien,
                ISNULL(COUNT(DISTINCT ht.TreEmID), 0) as SoLuongTreEmDuocUngHo,
                ISNULL(SUM(CASE WHEN ht.TrangThaiPhat = N'Đã phát' THEN 1 ELSE 0 END), 0) as TreDaNhan,
                ISNULL(k.TenKhuPho, N'Hỗ trợ tổng quát') as TenKhuPho,
                k.DiaChi as DiaChiKhuPho,
                k.QuanHuyen,
                k.ThanhPho
            FROM UngHo uh
            LEFT JOIN ManhThuongQuan m ON uh.ManhThuongQuanID = m.ManhThuongQuanID
            LEFT JOIN UngHo_HoTroPhucLoi uhht ON uh.UngHoID = uhht.UngHoID
            LEFT JOIN HoTroPhucLoi ht ON uhht.HoTroID = ht.HoTroID
            LEFT JOIN TreEm te ON ht.TreEmID = te.TreEmID
            LEFT JOIN KhuPho k ON te.KhuPhoID = k.KhuPhoID
            GROUP BY 
                uh.UngHoID, uh.LoaiUngHo, uh.GhiChu, uh.NgayUngHo, 
                m.Ten, uh.SoTien, k.TenKhuPho, k.DiaChi, k.QuanHuyen, k.ThanhPho
            ORDER BY uh.NgayUngHo DESC";

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sqlQuery;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var danhSach = new List<object>();

                            while (await reader.ReadAsync())
                            {
                                var soLuongTreEm = reader.GetInt32(6);
                                var treDaNhan = reader.GetInt32(7);

                                danhSach.Add(new
                                {
                                    HoTroID = reader.GetInt32(0),
                                    LoaiUngHo = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    MoTa = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    NgayUngHo = reader.IsDBNull(3)
                                        ? DateTime.MinValue
                                        : reader.GetDateTime(3),
                                    TenManhThuongQuan = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    SoTien = reader.GetDecimal(5),
                                    SoLuongTreEmDuocUngHo = soLuongTreEm,
                                    TreDaNhan = treDaNhan,
                                    TreChuaNhan = soLuongTreEm - treDaNhan,
                                    TenKhuPho = reader.GetString(8),
                                    DiaChiKhuPho = reader.IsDBNull(9) ? "" : reader.GetString(9),
                                    QuanHuyen = reader.IsDBNull(10) ? "" : reader.GetString(10),
                                    ThanhPho = reader.IsDBNull(11) ? "" : reader.GetString(11)
                                });
                            }

                            return Ok(danhSach);
                        }
                    }
                }
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
        //============================================
        //API 2: CHI TIẾT ỦNG HỘ - CẢI TIẾN
        //============================================
        [HttpGet("ChiTiet/{id}")]
        public async Task<ActionResult> GetChiTietHoTro(int id)
        {
            try
            {
                // Lấy thông tin UngHo + ManhThuongQuan
                var ungHo = await _context.UngHos
                    .Include(u => u.ManhThuongQuan)
                    .Where(u => u.UngHoId == id)
                    .Select(u => new
                    {
                        u.UngHoId,
                        u.GhiChu,
                        u.NgayUngHo,
                        u.LoaiUngHo,
                        u.SoTien,
                        u.DoiTuong,
                        TenManhThuongQuan = u.ManhThuongQuan != null ? u.ManhThuongQuan.Ten : "",
                        DiaChiMTQ = u.ManhThuongQuan != null ? u.ManhThuongQuan.DiaChi : "",
                        HoTroIds = u.HoTros.Select(h => h.HoTroId).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (ungHo == null)
                    return NotFound(new { message = "Không tìm thấy thông tin ủng hộ" });

                // Lấy thông tin KhuPho từ SuKien (ưu tiên cao nhất)
                var khuPhoTuSuKien = await _context.QuaTangUngHos
                    .Include(qt => qt.SuKien)
                        .ThenInclude(sk => sk.KhuPho)
                    .Where(qt => qt.UngHoId == id && qt.SuKienId != null)
                    .Select(qt => new
                    {
                        qt.SuKien.DiaDiem,
                        TenKhuPho = qt.SuKien.KhuPho != null ? qt.SuKien.KhuPho.TenKhuPho : "",
                        DiaChiKhuPho = qt.SuKien.KhuPho != null ? qt.SuKien.KhuPho.DiaChi : "",
                        QuanHuyen = qt.SuKien.KhuPho != null ? qt.SuKien.KhuPho.QuanHuyen : "",
                        ThanhPho = qt.SuKien.KhuPho != null ? qt.SuKien.KhuPho.ThanhPho : ""
                    })
                    .FirstOrDefaultAsync();

                // Lấy danh sách quà tặng
                var danhSachQuaTang = await _context.QuaTangUngHos
                    .Include(qt => qt.SuKien)
                        .ThenInclude(sk => sk.KhuPho)
                    .Include(qt => qt.PhanPhatQuas)
                        .ThenInclude(ppq => ppq.TreEm)
                            .ThenInclude(te => te.KhuPho)
                    .Where(qt => qt.UngHoId == id)
                    .ToListAsync();

                var quaTangDTOs = danhSachQuaTang.Select(qt => new QuaTangDTO
                {
                    QuaTangUngHoID = qt.QuaTangUngHoId,
                    TenQua = qt.TenQua ?? "",
                    MoTa = qt.MoTa ?? "",
                    SoLuongTong = qt.SoLuongTong,
                    SoLuongConLai = qt.SoLuongConLai,
                    DonGia = qt.DonGia,
                    DoiTuongNhan = qt.DoiTuongNhan ?? "",
                    Anh = qt.Anh ?? "",
                    DanhSachTreNhan = qt.PhanPhatQuas
                        .Where(ppq => ppq.TreEm != null)
                        .Select(ppq => new TreNhanQuaDTO
                        {
                            PhanPhatId = ppq.PhanPhatId,
                            TreEmID = ppq.TreEmId,
                            HoTen = ppq.TreEm.HoTen ?? "",
                            NgaySinh = ppq.TreEm.NgaySinh.HasValue
                                ? ppq.TreEm.NgaySinh.Value.ToDateTime(TimeOnly.MinValue)
                                : DateTime.MinValue,
                            TenKhuPho = qt.SuKien?.KhuPho?.TenKhuPho ?? ppq.TreEm.KhuPho?.TenKhuPho ?? "",
                            TinhTrang = ppq.TreEm.TinhTrang ?? "",
                            SoLuongNhan = ppq.SoLuongNhan,
                            NgayPhanPhat = ppq.NgayPhanPhat,
                            NguoiPhanPhat = ppq.NguoiPhanPhat ?? "",
                            TrangThai = ppq.TrangThai ?? "",
                            GhiChu = ppq.GhiChu ?? "",
                            HoTroId = ppq.TreEm.HoTroPhucLois
                .Where(h => h.UngHos.Any(u => u.UngHoId == qt.UngHoId))
                .Select(h => (int?)h.HoTroId)  // Chuyển sang nullable int
                .FirstOrDefault() ?? 0
                        })
                        .ToList()
                }).ToList();

                // Lấy minh chứng
                var danhSachMinhChung = await _context.PhieuMinhChungs
                    .Include(mc => mc.HoTro)
                        .ThenInclude(ht => ht.UngHos)
                    .Where(mc => mc.HoTro.UngHos.Any(u => u.UngHoId == id))
                    .Select(mc => new MinhChungDTO
                    {
                        MinhChungID = mc.MinhChungId,
                        LoaiMinhChung = mc.LoaiMinhChung ?? "",
                        FilePath = mc.FilePath ?? "",
                        NgayCap = mc.NgayCap.HasValue
                            ? mc.NgayCap.Value.ToDateTime(TimeOnly.MinValue)
                            : (DateTime?)null
                    })
                    .ToListAsync();

                // Thống kê
                int tongTreEm = quaTangDTOs.Sum(qt => qt.DanhSachTreNhan?.Count ?? 0);
                int treDaNhan = quaTangDTOs
                    .SelectMany(qt => qt.DanhSachTreNhan ?? new List<TreNhanQuaDTO>())
                    .Count(tn => tn.TrangThai == "Đã nhận");

                // Xác định thông tin khu phố (ưu tiên: SuKien → TreEm → MTQ)
                string tenKhuPho = "";
                string diaChiKhuPho = "";
                string quanHuyen = "";
                string thanhPho = "";

                if (khuPhoTuSuKien != null)
                {
                    tenKhuPho = khuPhoTuSuKien.TenKhuPho;
                    diaChiKhuPho = khuPhoTuSuKien.DiaChiKhuPho ?? khuPhoTuSuKien.DiaDiem;
                    quanHuyen = khuPhoTuSuKien.QuanHuyen;
                    thanhPho = khuPhoTuSuKien.ThanhPho;
                }
                else
                {
                    // Từ TreEm
                    var khuPhoTuTreEm = await _context.TreEms
                        .Include(te => te.KhuPho)
                        .Where(te => _context.PhanPhatQuas
                            .Any(ppq => ppq.TreEmId == te.TreEmId &&
                                        ppq.QuaTangUngHo.UngHoId == id))
                        .GroupBy(te => new
                        {
                            te.KhuPho.TenKhuPho,
                            te.KhuPho.DiaChi,
                            te.KhuPho.QuanHuyen,
                            te.KhuPho.ThanhPho
                        })
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefaultAsync();

                    if (khuPhoTuTreEm != null)
                    {
                        tenKhuPho = khuPhoTuTreEm.TenKhuPho ?? "";
                        diaChiKhuPho = khuPhoTuTreEm.DiaChi ?? "";
                        quanHuyen = khuPhoTuTreEm.QuanHuyen ?? "";
                        thanhPho = khuPhoTuTreEm.ThanhPho ?? "";
                    }
                    else
                    {
                        diaChiKhuPho = ungHo.DiaChiMTQ;
                    }
                }

                // DTO response
                var chiTietUngHo = new ChiTietUngHoDTO
                {
                    UngHoID = ungHo.UngHoId,
                    GhiChu = ungHo.GhiChu ?? "",
                    NgayUngHo = ungHo.NgayUngHo?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    LoaiUngHo = ungHo.LoaiUngHo ?? "",
                    SoTien = ungHo.SoTien ?? 0,
                    DoiTuong = ungHo.DoiTuong ?? "",
                    TenManhThuongQuan = ungHo.TenManhThuongQuan,
                    TenKhuPho = tenKhuPho,
                    DiaChiKhuPho = diaChiKhuPho,
                    QuanHuyen = quanHuyen,
                    ThanhPho = thanhPho,
                    SoLuongTreEmDuocUngHo = tongTreEm,
                    TongTreEm = tongTreEm,
                    TreDaNhan = treDaNhan,
                    TreChuaNhan = tongTreEm - treDaNhan,
                    PercentDaPhat = tongTreEm > 0 ? (int)Math.Round((treDaNhan * 100.0) / tongTreEm) : 0,
                    DanhSachQuaTang = quaTangDTOs,
                    DanhSachMinhChung = danhSachMinhChung
                };

                return Ok(chiTietUngHo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi lấy chi tiết hỗ trợ",
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

                // BƯỚC 1: LẤY ỦNG HỘ (BẮT BUỘC PHẢI CÓ)
                if (request.UngHoId <= 0)
                {
                    return BadRequest(new { message = "Vui lòng chọn đợt ủng hộ" });
                }

                ungHo = await _context.UngHos
                    .FirstOrDefaultAsync(u => u.UngHoId == request.UngHoId);

                if (ungHo == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin ủng hộ" });
                }

                System.Diagnostics.Debug.WriteLine($"✅ Tìm thấy ủng hộ: {ungHo.TenVatPham}");

                // TÍNH TỔNG SỐ LƯỢNG CẦN PHÁT
                int tongSoLuongPhat = request.DanhSachTreEmNhan.Sum(t => t.SoLuongNhan);
                System.Diagnostics.Debug.WriteLine($"Tổng số lượng phát: {tongSoLuongPhat}");

                // KIỂM TRA SỐ LƯỢNG CÒN LẠI TRONG ỦNG HỘ
                if (!ungHo.SoLuongConLai.HasValue || ungHo.SoLuongConLai < tongSoLuongPhat)
                {
                    return BadRequest(new
                    {
                        message = $"Số lượng vật phẩm trong ủng hộ không đủ! Còn lại: {ungHo.SoLuongConLai ?? 0}, Cần: {tongSoLuongPhat}"
                    });
                }

                // BƯỚC 2: XỬ LÝ QUÀ TẶNG
                if (request.QuaTangUngHoId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"Sử dụng quà tặng có sẵn ID: {request.QuaTangUngHoId.Value}");

                    quaTang = await _context.QuaTangUngHos
                        .FirstOrDefaultAsync(qt => qt.QuaTangUngHoId == request.QuaTangUngHoId.Value);

                    if (quaTang == null)
                    {
                        return NotFound(new { message = "Không tìm thấy thông tin quà tặng" });
                    }

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
                        SuKienId = request.SuKienId.HasValue && request.SuKienId.Value > 0 ? request.SuKienId.Value : (int?)null,  // ✅ FIX: Để null thay vì 0
                        TenQua = request.TenQua ?? ungHo.TenVatPham,
                        MoTa = request.MoTaQua,
                        SoLuongTong = tongSoLuongPhat,
                        SoLuongConLai = tongSoLuongPhat,
                        DonGia = request.DonGia ?? 0,
                        DoiTuongNhan = request.DoiTuongNhan ?? ungHo.DoiTuong,
                        Anh = request.AnhQua
                    };

                    System.Diagnostics.Debug.WriteLine($"Thông tin quà tặng:");
                    System.Diagnostics.Debug.WriteLine($"  - TenQua: {quaTang.TenQua}");
                    System.Diagnostics.Debug.WriteLine($"  - SoLuongTong: {quaTang.SoLuongTong}");
                    System.Diagnostics.Debug.WriteLine($"  - UngHoId: {quaTang.UngHoId}");
                    System.Diagnostics.Debug.WriteLine($"  - SuKienId: {quaTang.SuKienId}");

                    _context.QuaTangUngHos.Add(quaTang);

                    // TRỪ SỐ LƯỢNG TỪ ỦNG HỘ
                    ungHo.SoLuongConLai -= tongSoLuongPhat;
                    _context.UngHos.Update(ungHo);

                    try
                    {
                        await _context.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"✅ Đã lưu quà tặng ID: {quaTang.QuaTangUngHoId}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ LỖI khi lưu quà tặng:");
                        System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException?.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                        throw new Exception($"Lỗi tạo quà tặng: {ex.InnerException?.Message ?? ex.Message}", ex);
                    }
                }

                // BƯỚC 3: KIỂM TRA TRẺ EM
                var danhSachTreEmId = request.DanhSachTreEmNhan.Select(t => t.TreEmId).ToList();
                var danhSachTreEm = await _context.TreEms
                    .Where(t => danhSachTreEmId.Contains(t.TreEmId))
                    .ToListAsync();

                if (danhSachTreEm.Count != danhSachTreEmId.Count)
                {
                    return BadRequest(new { message = "Một số trẻ em không tồn tại trong hệ thống" });
                }

                System.Diagnostics.Debug.WriteLine($"✅ Đã xác thực {danhSachTreEm.Count} trẻ em");

                var danhSachPhanPhat = new List<PhanPhatQuaInfo>();
                var danhSachHoTroPhucLoi = new List<HoTroPhucLoiInfo>();

                // BƯỚC 4: TẠO HỖ TRỢ PHÚC LỢI VÀ PHÂN PHÁT CHO TỪNG TRẺ
                foreach (var treEmNhan in request.DanhSachTreEmNhan)
                {

                    // Kiểm tra trùng lặp
                    var ngayPP = request.NgayPhanPhat.ToDateTime(TimeOnly.MinValue);

                    var daPhatTrongNgay = await _context.PhanPhatQuas
                        .AnyAsync(pp =>
                            pp.QuaTangUngHoId == quaTang.QuaTangUngHoId &&
                            pp.TreEmId == treEmNhan.TreEmId &&
                            pp.NgayPhanPhat.Date == ngayPP.Date
                        );

                    if (daPhatTrongNgay)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            message = $"Trẻ em ID {treEmNhan.TreEmId} đã nhận quà này trong ngày {request.NgayPhanPhat:dd/MM/yyyy}"
                        });
                    }

                    // A. TẠO HỖ TRỢ PHÚC LỢI
                    var hoTroPhucLoi = new HoTroPhucLoi
                    {
                        LoaiHoTro = request.LoaiHoTro,
                        MoTa = request.MoTa,
                        NgayCap = request.NgayCap,
                        NguoiChiuTrachNhiemHoTro = request.NguoiChiuTrachNhiemHoTro,
                        TrangThaiPhat = request.TrangThaiPhat ?? "Chưa phát",
                        NgayHenLai = request.NgayHenLai,
                        GhiChuTNV = request.GhiChuTNV,
                        TreEmId = treEmNhan.TreEmId,
                        NguoiDungId = request.NguoiDungID
                    };
                    _context.HoTroPhucLois.Add(hoTroPhucLoi);

                    // B. LIÊN KẾT ỦNG HỘ - HỖ TRỢ PHÚC LỢI
                    hoTroPhucLoi.UngHos.Add(ungHo);

                    try
                    {
                        await _context.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"✅ Đã lưu hỗ trợ phúc lợi ID: {hoTroPhucLoi.HoTroId}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ LỖI khi lưu hỗ trợ phúc lợi:");
                        System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException?.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                        throw new Exception($"Lỗi tạo hỗ trợ phúc lợi cho trẻ {treEmNhan.TreEmId}: {ex.InnerException?.Message ?? ex.Message}", ex);
                    }

                    // C. TẠO PHÂN PHÁT QUÀ
                    var phanPhat = new PhanPhatQua
                    {
                        QuaTangUngHoId = quaTang.QuaTangUngHoId,
                        TreEmId = treEmNhan.TreEmId,
                        SoLuongNhan = treEmNhan.SoLuongNhan,
                        NgayPhanPhat = request.NgayPhanPhat.ToDateTime(TimeOnly.MinValue),
                        NguoiPhanPhat = request.NguoiPhanPhat,
                        TrangThai = request.TrangThaiPhat ?? "Chưa phát",
                        GhiChu = treEmNhan.GhiChu ?? request.GhiChuPhanPhat
                    };

                    System.Diagnostics.Debug.WriteLine($"Thông tin phân phát:");
                    System.Diagnostics.Debug.WriteLine($"  - QuaTangUngHoId: {phanPhat.QuaTangUngHoId}");
                    System.Diagnostics.Debug.WriteLine($"  - TreEmId: {phanPhat.TreEmId}");
                    System.Diagnostics.Debug.WriteLine($"  - SoLuongNhan: {phanPhat.SoLuongNhan}");

                    _context.PhanPhatQuas.Add(phanPhat);

                    try
                    {
                        await _context.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"✅ Đã lưu phân phát ID: {phanPhat.PhanPhatId}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ LỖI khi lưu phân phát:");
                        System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException?.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                        throw new Exception($"Lỗi tạo phân phát cho trẻ {treEmNhan.TreEmId}: {ex.InnerException?.Message ?? ex.Message}", ex);
                    }

                    var treEm = danhSachTreEm.First(t => t.TreEmId == treEmNhan.TreEmId);

                    danhSachHoTroPhucLoi.Add(new HoTroPhucLoiInfo
                    {
                        HoTroId = hoTroPhucLoi.HoTroId,
                        TreEmId = treEm.TreEmId,
                        HoTenTreEm = treEm.HoTen,
                        LoaiHoTro = request.LoaiHoTro,
                        TrangThaiPhat = request.TrangThaiPhat ?? "Chưa phát"
                    });

                    danhSachPhanPhat.Add(new PhanPhatQuaInfo
                    {
                        PhanPhatId = phanPhat.PhanPhatId,
                        TreEmId = treEm.TreEmId,
                        HoTenTreEm = treEm.HoTen,
                        SoLuongNhan = treEmNhan.SoLuongNhan,
                        TrangThai = request.TrangThaiPhat ?? "Chưa phát"
                    });
                }

                // BƯỚC 5: CẬP NHẬT SỐ LƯỢNG KHI ĐÃ PHÁT
                if (request.TrangThaiPhat == "Đã phát")
                {
                    quaTang.SoLuongConLai -= tongSoLuongPhat;
                    _context.QuaTangUngHos.Update(quaTang);
                }
                else
                {
                    if (request.QuaTangUngHoId.HasValue)
                    {
                        ungHo.SoLuongConLai -= tongSoLuongPhat;
                        _context.UngHos.Update(ungHo);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                System.Diagnostics.Debug.WriteLine("✅ HOÀN TẤT - Commit transaction thành công");

                return Ok(new TaoHoTroVaPhanPhatResponseDTO
                {
                    Success = true,
                    Message = $"Đã tạo hỗ trợ phúc lợi và phân phát quà thành công cho {request.DanhSachTreEmNhan.Count} trẻ em",
                    UngHoId = ungHo.UngHoId,
                    QuaTangUngHoId = quaTang.QuaTangUngHoId,
                    SoTreEmDaNhan = request.DanhSachTreEmNhan.Count,
                    TongSoLuongPhat = tongSoLuongPhat,
                    SoLuongConLaiQuaTang = quaTang.SoLuongConLai,
                    SoLuongConLaiUngHo = ungHo.SoLuongConLai ?? 0,
                    DanhSachHoTroPhucLoi = danhSachHoTroPhucLoi,
                    DanhSachPhanPhat = danhSachPhanPhat
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                System.Diagnostics.Debug.WriteLine("❌ EXCEPTION TOÀN CỤC:");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tạo hỗ trợ và phân phát quà",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        [HttpDelete("XoaTreKhoiDanhSachPhatQua/{phanPhatId}")]
        public async Task<IActionResult> XoaTreKhoiDanhSachPhatQua(int phanPhatId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy thông tin phân phát
                var phanPhat = await _context.PhanPhatQuas
                    .Include(pp => pp.QuaTangUngHo)
                    .FirstOrDefaultAsync(pp => pp.PhanPhatId == phanPhatId);

                if (phanPhat == null)
                    return NotFound(new { message = "Không tìm thấy bản ghi phát quà" });

                // Hoàn lại số lượng quà
                if (phanPhat.QuaTangUngHo != null)
                {
                    phanPhat.QuaTangUngHo.SoLuongConLai += phanPhat.SoLuongNhan;
                }

                // Xóa bản ghi phân phát
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
    }
}


