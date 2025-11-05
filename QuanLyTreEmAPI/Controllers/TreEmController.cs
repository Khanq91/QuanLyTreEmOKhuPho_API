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
                .Include(te => te.TreEmHoanCanhs).ThenInclude(h => h.HoanCanh)
                .Include(te => te.TreEmPhuHuynhs).ThenInclude(ph => ph.PhuHuynh)
                .Include(te => te.PhieuHocTaps)
                .Include(te => te.HoTroPhucLois).ThenInclude(h => h.PhieuMinhChungs)
                .Include(te => te.VanDongTreEms)
                .FirstOrDefaultAsync(te => te.TreEmId == id);

            if (tre == null) return NotFound();

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
                Anh = tre.Anh != null
                    ? $"{baseUrl}{tre.Anh.Replace("\\", "/")}"  // ✅ full URL
                    : $"{baseUrl}/Anh/default-avatar.png",
                HoanCanh = tre.TreEmHoanCanhs.Select(h => h.HoanCanh.LoaiHoanCanh).ToList(),
                PhuHuynh = tre.TreEmPhuHuynhs.Select(p => new
                {
                    p.PhuHuynh.HoTen,
                    p.PhuHuynh.Sdt,
                    p.PhuHuynh.NgheNghiep,
                    p.MoiQuanHe
                }),
                HocTap = tre.PhieuHocTaps.Select(p => new
                {
                    p.NgayCapNhat,
                    p.DiemTrungBinh,
                    p.XepLoai,
                    p.HanhKiem,
                    p.GhiChu
                }),
                HoTro = tre.HoTroPhucLois.Select(ht => new
                {
                    ht.HoTroId,
                    ht.LoaiHoTro,
                    ht.NgayCap,
                    ht.MoTa,
                    File = ht.PhieuMinhChungs.Select(f => $"{baseUrl}{f.FilePath.Replace("\\", "/")}")
                }),
                VanDong = tre.VanDongTreEms.Select(v => new
                {
                    v.NgayVanDong,
                    v.LyDo,
                    v.KetQua,
                    v.SoLan
                })
            };

            return Ok(detail);
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
                            NgayCapNhat = DateOnly.FromDateTime(DateTime.Now),
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
            var tre = await _context.TreEms.FindAsync(id);
            if (tre == null)
                return NotFound("Không tìm thấy trẻ em.");

            // Cập nhật thông tin cơ bản
            tre.HoTen = model.HoTen;
            tre.NgaySinh = DateOnly.FromDateTime(model.NgaySinh);
            tre.GioiTinh = model.GioiTinh;
            tre.TonGiao = model.TonGiao;
            tre.DanToc = model.DanToc;
            tre.QuocTich = model.QuocTich;
            tre.TruongId = model.TruongId;
            tre.KhuPhoId = model.KhuPhoId;
            tre.TinhTrang = model.TinhTrang;
            tre.Anh = model.Anh;
            await _context.SaveChangesAsync();

            // Cập nhật hoàn cảnh
            var oldHC = _context.TreEmHoanCanhs.Where(h => h.TreEmId == id);
            _context.TreEmHoanCanhs.RemoveRange(oldHC);
            if (model.HoanCanhIds != null)
            {
                foreach (var hc in model.HoanCanhIds)
                {
                    _context.TreEmHoanCanhs.Add(new TreEmHoanCanh
                    {
                        TreEmId = id,
                        HoanCanhId = hc,
                        NgayCapNhat = DateOnly.FromDateTime(DateTime.Now)
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật hồ sơ trẻ thành công" });
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
