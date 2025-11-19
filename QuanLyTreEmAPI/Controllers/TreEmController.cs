using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TreEmController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        public TreEmController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var treEms = await _context.TreEms.ToListAsync();
            return Ok(treEms);
        }
        [HttpGet("TongTreEmTheoKhuPho")]
        public async Task<IActionResult> TongTreEmTheoKhuPho()
        {
            var tongTreEm = await _context.TreEms
               .Include(te => te.KhuPho)
               .GroupBy(te => te.KhuPhoId)
               .Select(g => new
               {
                   KhuPhoID = g.Key,
                   TenKhuPho = g.FirstOrDefault().KhuPho.TenKhuPho,
                   SoLuongTreEm = g.Count()
               })
               .ToListAsync();


            return Ok(tongTreEm);
        }
        [HttpGet("DanhSach")]
        public async Task<IActionResult> GetDanhSachTreEm()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; // 👉 tự động lấy https://localhost:44389

            var data = await _context.TreEms
                .Include(te => te.KhuPho)
                .Include(te => te.Truong)
                .Include(te => te.TreEmHoanCanhs)
                    .ThenInclude(tehc => tehc.HoanCanh)
                .Select(te => new
                {
                    te.TreEmId,
                    te.HoTen,
                    te.NgaySinh,
                    te.GioiTinh,
                    te.TinhTrang,
                    Truong = te.Truong.TenTruong,
                    KhuPho = te.KhuPho.TenKhuPho,
                    HoanCanh = te.TreEmHoanCanhs
                        .Select(h => h.HoanCanh.LoaiHoanCanh)
                        .ToList(),
                    Anh = te.Anh != null
                        ? $"{baseUrl}{te.Anh.Replace("\\", "/")}" // 👉 ghép URL tuyệt đối
                        : $"{baseUrl}/Anh/default-avatar.png"
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var tre = await _context.TreEms
                .Include(te => te.KhuPho)
                .Include(te => te.Truong)
                .Include(te => te.TreEmHoanCanhs)
                    .ThenInclude(h => h.HoanCanh)
                .Include(te => te.TreEmPhuHuynhs)
                    .ThenInclude(ph => ph.PhuHuynh)
                .Include(te => te.PhieuHocTaps)
                    .ThenInclude(p => p.Lop)
                .Include(te => te.PhieuHocTaps)
                    .ThenInclude(p => p.Truong)
                .Include(te => te.HoTroPhucLois)
                    .ThenInclude(h => h.PhieuMinhChungs)
                .Include(te => te.VanDongTreEms)
                    .ThenInclude(v => v.HoanCanh)
                .Include(te => te.VanDongTreEms)
                    .ThenInclude(v => v.NguoiDung)
                .FirstOrDefaultAsync(te => te.TreEmId == id);

            if (tre == null)
                return NotFound(new { message = "Không tìm thấy trẻ em" });

            var detail = new
            {
                tre.TreEmId,
                tre.HoTen,
                tre.NgaySinh,
                tre.GioiTinh,
                tre.TinhTrang,
                tre.TonGiao,
                tre.DanToc,
                tre.QuocTich,
                KhuPho = tre.KhuPho?.TenKhuPho,
                Truong = tre.Truong?.TenTruong,
                Anh = !string.IsNullOrEmpty(tre.Anh)
                    ? $"{baseUrl}{tre.Anh.Replace("\\", "/")}"
                    : $"{baseUrl}/Anh/default-avatar.png",

                // ✅ Hoàn cảnh - BỎ ?? new List<object>()
                HoanCanh = tre.TreEmHoanCanhs
                    .Where(h => h.HoanCanh != null)
                    .Select(h => new
                    {
                        h.HoanCanh.HoanCanhId,
                        h.HoanCanh.LoaiHoanCanh,
                        h.HoanCanh.MoTa,
                        h.NgayCapNhat
                    })
                    .ToList(),

                // ✅ Phụ huynh - BỎ ?? new List<object>()
                PhuHuynh = tre.TreEmPhuHuynhs
                    .Where(p => p.PhuHuynh != null)
                    .Select(p => new
                    {
                        p.PhuHuynh.PhuHuynhId,
                        p.PhuHuynh.HoTen,
                        SDT = p.PhuHuynh.Sdt,
                        p.PhuHuynh.NgheNghiep,
                        p.PhuHuynh.DiaChi,
                        p.PhuHuynh.NgaySinh,
                        p.PhuHuynh.TonGiao,
                        p.PhuHuynh.DanToc,
                        p.PhuHuynh.QuocTich,
                        p.MoiQuanHe
                    })
                    .ToList(),

                // ✅ Học tập - BỎ ?? new List<object>()
                HocTap = tre.PhieuHocTaps
                    .OrderByDescending(p => p.NamHoc)
                    .Select(p => new
                    {
                        p.PhieuHocTapId,
                        p.NamHoc,
                        p.DiemTrungBinh,
                        p.XepLoai,
                        p.HanhKiem,
                        p.GhiChu,
                        TenLop = p.Lop?.TenLop,
                        TenTruong = p.Truong?.TenTruong
                    })
                    .ToList(),

                // ✅ Hỗ trợ phúc lợi - BỎ ?? new List<object>()
                HoTro = tre.HoTroPhucLois
                    .OrderByDescending(ht => ht.NgayCap)
                    .Select(ht => new
                    {
                        ht.HoTroId,
                        ht.LoaiHoTro,
                        ht.NgayCap,
                        ht.MoTa,
                        ht.TrangThaiPhat,
                        ht.NguoiChiuTrachNhiemHoTro,
                        ht.NgayHenLai,
                        ht.GhiChuTNV,
                        // ✅ Files - BỎ ?? new List<object>()
                        Files = ht.PhieuMinhChungs
                            .Where(f => !string.IsNullOrEmpty(f.FilePath))
                            .Select(f => new
                            {
                                f.MinhChungId,
                                f.LoaiMinhChung,
                                f.NgayCap,
                                Url = $"{baseUrl}{f.FilePath.Replace("\\", "/")}"
                            })
                            .ToList()
                    })
                    .ToList(),

                // ✅ Vận động - BỎ ?? new List<object>()
                VanDong = tre.VanDongTreEms
                    .OrderByDescending(v => v.NgayVanDong)
                    .Select(v => new
                    {
                        v.VanDongId,
                        v.NgayVanDong,
                        v.LyDo,
                        v.KetQua,
                        v.SoLan,
                        v.TinhTrangCapNhat,
                        v.GhiChuChiTiet,
                        v.NgayCapNhat,
                        HoanCanh = v.HoanCanh != null ? new
                        {
                            v.HoanCanh.HoanCanhId,
                            v.HoanCanh.LoaiHoanCanh
                        } : null,
                        NguoiVanDong = v.NguoiDung != null ? new
                        {
                            v.NguoiDung.UserId,
                            v.NguoiDung.HoTen
                        } : null,
                        AnhMinhChung = !string.IsNullOrEmpty(v.AnhMinhChung)
                            ? $"{baseUrl}{v.AnhMinhChung.Replace("\\", "/")}"
                            : null
                    })
                    .ToList()
            };

            return Ok(detail);
        }

        //[HttpGet("TongTreEmTheoKhuPho")]
        //public async Task<IActionResult> TongTreEmTheoKhuPho()
        //{
        //    var tongTreEm = await _context.TreEms
        //       .Include(te => te.KhuPho)
        //       .GroupBy(te => te.KhuPhoId)
        //       .Select(g => new
        //       {
        //           KhuPhoID = g.Key,
        //           TenKhuPho = g.FirstOrDefault().KhuPho.TenKhuPho,
        //           SoLuongTreEm = g.Count()
        //       })
        //       .ToListAsync();

        //    return Ok(tongTreEm);
        //}

        [HttpPost("Create")]
        public async Task<IActionResult> CreateTreEm([FromBody] TreEmCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 🔹 Xử lý ảnh: nếu chuỗi là base64 thì lưu ra file vật lý
                string imagePath = null;
                if (!string.IsNullOrEmpty(model.Anh))
                {
                    if (model.Anh.StartsWith("data:image"))
                    {
                        // Tạo thư mục nếu chưa có
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Anh", "TreEm");
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        // Tạo tên file duy nhất
                        var fileName = $"treem_{DateTime.Now:yyyyMMddHHmmssfff}.jpg";
                        var filePath = Path.Combine(folderPath, fileName);

                        // Cắt bỏ tiền tố "data:image/jpeg;base64,"
                        var base64Data = model.Anh.Substring(model.Anh.IndexOf(",") + 1);
                        var bytes = Convert.FromBase64String(base64Data);
                        await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                        // Lưu đường dẫn tương đối
                        imagePath = $"/Anh/TreEm/{fileName}";
                    }
                    else
                    {
                        // Trường hợp frontend chỉ gửi đường dẫn
                        imagePath = model.Anh;
                    }
                }

                var tre = new TreEm
                {
                    HoTen = model.HoTen,
                    NgaySinh = DateOnly.FromDateTime(model.NgaySinh),
                    GioiTinh = model.GioiTinh,
                    TonGiao = model.TonGiao,
                    DanToc = model.DanToc,
                    QuocTich = model.QuocTich,
                    TruongId = model.TruongId,
                    KhuPhoId = model.KhuPhoId,
                    TinhTrang = model.TinhTrang,
                    Anh = imagePath
                };

                _context.TreEms.Add(tre);
                await _context.SaveChangesAsync();

                // 🔹 Thêm hoàn cảnh
                if (model.HoanCanhIds != null)
                {
                    foreach (var hc in model.HoanCanhIds)
                    {
                        _context.TreEmHoanCanhs.Add(new TreEmHoanCanh
                        {
                            TreEmId = tre.TreEmId,
                            HoanCanhId = hc,
                            NgayCapNhat = DateOnly.FromDateTime(DateTime.Now)
                        });
                    }
                }

                // 🔹 Thêm phụ huynh
                if (model.PhuHuynhs != null)
                {
                    foreach (var ph in model.PhuHuynhs)
                    {
                        var phuHuynh = new ThongTinPhuHuynh
                        {
                            HoTen = ph.HoTen,
                            Sdt = ph.SDT,
                            DiaChi = ph.DiaChi,
                            NgheNghiep = ph.NgheNghiep,
                            TonGiao = ph.TonGiao,
                            DanToc = ph.DanToc,
                            QuocTich = ph.QuocTich
                        };
                        _context.ThongTinPhuHuynhs.Add(phuHuynh);
                        await _context.SaveChangesAsync();

                        _context.TreEmPhuHuynhs.Add(new TreEmPhuHuynh
                        {
                            TreEmId = tre.TreEmId,
                            PhuHuynhId = phuHuynh.PhuHuynhId,
                            MoiQuanHe = ph.MoiQuanHe
                        });
                    }
                }

                // 🔹 Thêm phiếu học tập
                if (model.HocTaps != null)
                {
                    foreach (var ht in model.HocTaps)
                    {
                        _context.PhieuHocTaps.Add(new PhieuHocTap
                        {
                            DiemTrungBinh = ht.DiemTrungBinh,
                            XepLoai = ht.XepLoai,
                            HanhKiem = ht.HanhKiem,
                            GhiChu = ht.GhiChu,
                            NamHoc = DateOnly.FromDateTime(DateTime.Now),
                            TruongId = model.TruongId,
                            TreEmId = tre.TreEmId,
                            LopId = ht.LopId
                        });
                    }
                }

                // 🔹 Lưu tất cả 1 lần
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Thêm trẻ mới thành công", tre.TreEmId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi khi lưu dữ liệu", detail = ex.Message });
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateTreEm(int id, [FromBody] TreEmCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tre = await _context.TreEms
                .Include(te => te.TreEmPhuHuynhs)
                .Include(te => te.TreEmHoanCanhs)
                .Include(te => te.PhieuHocTaps)
                .Include(te => te.HoTroPhucLois)
                    .ThenInclude(ht => ht.PhieuMinhChungs)
                .Include(te => te.VanDongTreEms)
                .FirstOrDefaultAsync(te => te.TreEmId == id);

            if (tre == null)
                return NotFound($"Không tìm thấy trẻ với ID = {id}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ============================
                // 🔹 Cập nhật thông tin cơ bản
                // ============================
                tre.HoTen = model.HoTen;
                tre.NgaySinh = DateOnly.FromDateTime(model.NgaySinh);
                tre.GioiTinh = model.GioiTinh;
                tre.DanToc = model.DanToc;
                tre.TonGiao = model.TonGiao;
                tre.QuocTich = model.QuocTich;
                tre.KhuPhoId = model.KhuPhoId;
                tre.TruongId = model.TruongId;
                tre.TinhTrang = model.TinhTrang;

                // 🔹 Nếu người dùng upload ảnh mới (base64)
                if (!string.IsNullOrEmpty(model.Anh))
                {
                    if (model.Anh.StartsWith("data:image"))
                    {
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Anh", "TreEm");
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        var fileName = $"treem_{DateTime.Now:yyyyMMddHHmmssfff}.jpg";
                        var filePath = Path.Combine(folderPath, fileName);
                        var base64Data = model.Anh.Substring(model.Anh.IndexOf(",") + 1);
                        var bytes = Convert.FromBase64String(base64Data);
                        await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                        tre.Anh = $"/Anh/TreEm/{fileName}";
                    }
                    else
                    {
                        tre.Anh = model.Anh; // dùng lại ảnh cũ
                    }
                }

          

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Cập nhật thông tin trẻ thành công" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi khi cập nhật", detail = ex.Message });
            }
        }


        public class TreEmCreateDto
        {
            public string HoTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string TonGiao { get; set; }
            public string DanToc { get; set; }
            public string QuocTich { get; set; }
            public string Anh { get; set; }
            public int TruongId { get; set; }
            public int KhuPhoId { get; set; }
            public string TinhTrang { get; set; }
            public List<int>? HoanCanhIds { get; set; }
            public List<PhuHuynhDto>? PhuHuynhs { get; set; }
            public List<HocTapDto>? HocTaps { get; set; }
            public List<HoTroDto>? HoTros { get; set; }
            public List<VanDongDto>? VanDongs { get; set; }
        }

        public class PhuHuynhDto
        {
            public string HoTen { get; set; }
            public string SDT { get; set; }
            public string DiaChi { get; set; }
            public string NgheNghiep { get; set; }
            public DateTime NgaySinh { get; set; }
            public string TonGiao { get; set; }
            public string DanToc { get; set; }
            public string QuocTich { get; set; }
            public string MoiQuanHe { get; set; }
        }

        public class HocTapDto
        {
            public double DiemTrungBinh { get; set; }
            public string XepLoai { get; set; }
            public string HanhKiem { get; set; }
            public string GhiChu { get; set; }
            public int LopId { get; set; }
        }

        public class HoTroDto
        {
            public string LoaiHoTro { get; set; }
            public string MoTa { get; set; }
            public string NguoiChiuTrachNhiemHoTro { get; set; }
            public List<string>? FilePaths { get; set; }
        }

        public class VanDongDto
        {
            public int HoanCanhId { get; set; }
            public int NguoiDungId { get; set; }
            public int SoLan { get; set; }
            public string LyDo { get; set; }
            public string KetQua { get; set; }
        }
    }
}
