using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.DTOs.TInhNguyenVien;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinhNguyenVienController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        public TinhNguyenVienController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tnvs = await _context.TinhNguyenViens.ToListAsync();
            return Ok(tnvs);
        }
        [HttpGet("GetThongKeTinhNguyenVien")]
        public async Task<IActionResult> GetThongKeTinhNguyenVien([FromQuery] int? KhuPhoId)
        {
            var query = from t in _context.TinhNguyenViens
                        join u in _context.NguoiDungs on t.UserId equals u.UserId
                        join k in _context.KhuPhos on t.KhuPhoId equals k.KhuPhoId
                        join l in _context.LichTrongs on t.TinhNguyenVienId equals l.TinhNguyenVienId
                        join c in _context.ChiTietLichTrongs on l.LichTrongId equals c.LichTrongId
                        join p in _context.PhanCongTinhNguyenViens on t.TinhNguyenVienId equals p.TinhNguyenVienId into pcg
                        from p in pcg.DefaultIfEmpty()
                        join s in _context.SuKiens on p.SuKienId equals s.SuKienId into sg
                        from s in sg.DefaultIfEmpty()
                        select new { t, u, k, c, s };

            if (KhuPhoId.HasValue)
                query = query.Where(x => x.k.KhuPhoId == KhuPhoId);

            var result = await query
                .GroupBy(g => new
                {
                    g.k.KhuPhoId,
                    g.t.TinhNguyenVienId,
                    g.t.UserId,
                    g.u.HoTen,
                    g.u.TrangThai,
                    g.u.NgayTao,
                    g.t.NgaySinh,
                    g.u.Anh,
                    g.k.TenKhuPho,
                    g.u.SDT,
                    g.t.ChucVu

                })
                .Select(g => new ThongKeTinhNguyenVienDTO
                {
                    KhuPhoID = g.Key.KhuPhoId,
                    UserID = g.Key.UserId,
                    TinhNguyenVienID = g.Key.TinhNguyenVienId,
                    TenTinhNguyenVien = g.Key.HoTen,
                    TrangThai = g.Key.TrangThai,
                    NgaySinh = g.Key.NgaySinh,
                    NgayTao = g.Key.NgayTao,
                    Anh =g.Key.Anh,
                    TenKhuPho = g.Key.TenKhuPho,
                    SDT = g.Key.SDT,
                    ChucVu = g.Key.ChucVu,
                    SoLuongCaRanh = g.Select(x => x.c.ChiTietLichTrongId).Distinct().Count(),
                    TongSuKienThamGia = g.Select(x => x.s.SuKienId).Distinct().Count(sid => sid != null)
                })
                .OrderBy(x => x.TinhNguyenVienID)
                .ToListAsync();

            return Ok(result);
        }
        [HttpGet("ChiTietThongTinTinhNguyenVien")]
        public async Task<IActionResult> ChiTietThongTinTinhNguyenVien([FromQuery] int? UserID)
        {
            var query = from t in _context.TinhNguyenViens
                        join u in _context.NguoiDungs on t.UserId equals u.UserId
                        join k in _context.KhuPhos on t.KhuPhoId equals k.KhuPhoId
                        join l in _context.LichTrongs on t.TinhNguyenVienId equals l.TinhNguyenVienId
                        join c in _context.ChiTietLichTrongs on l.LichTrongId equals c.LichTrongId
                        join p in _context.PhanCongTinhNguyenViens on t.TinhNguyenVienId equals p.TinhNguyenVienId into pcg
                        from p in pcg.DefaultIfEmpty()
                        join s in _context.SuKiens on p.SuKienId equals s.SuKienId into sg
                        from s in sg.DefaultIfEmpty()
                        select new { t, u, k, c, s };

            if (UserID.HasValue)
                query = query.Where(x => x.t.UserId == UserID.Value);

            var result = await query
                .GroupBy(g => new
                {
                    g.k.KhuPhoId,
                    g.t.TinhNguyenVienId,
                    g.t.UserId,
                    g.u.HoTen,
                    g.t.NgaySinh,
                    g.k.TenKhuPho,
                    g.u.SDT,
                    g.t.ChucVu
                })
                .Select(g => new ThongKeTinhNguyenVienDTO
                {
                    KhuPhoID = g.Key.KhuPhoId,
                    UserID = g.Key.UserId,
                    TinhNguyenVienID = g.Key.TinhNguyenVienId,
                    TenTinhNguyenVien = g.Key.HoTen,
                    NgaySinh = g.Key.NgaySinh,
                    TenKhuPho = g.Key.TenKhuPho,
                    SDT = g.Key.SDT,
                    ChucVu = g.Key.ChucVu,
                    SoLuongCaRanh = g.Select(x => x.c.ChiTietLichTrongId).Distinct().Count(),
                    TongSuKienThamGia = g.Select(x => x.s.SuKienId).Distinct().Count(sid => sid != null)
                })
                .OrderBy(x => x.TinhNguyenVienID)
                .FirstOrDefaultAsync(); // Lấy 1 phần tử đầu tiên (hoặc null nếu không có)

            if (result == null)
                return NotFound(); // Không tìm thấy UserID

            return Ok(result); // Trả về 1 object thay vì List
        }

        [HttpGet("GetLichRanhTinhNguyenVien")]
        public async Task<IActionResult> GetLichRanhTinhNguyenVien([FromQuery] int UserID)
        {
            var lichRanh = await (from t in _context.TinhNguyenViens
                                  join l in _context.LichTrongs on t.TinhNguyenVienId equals l.TinhNguyenVienId
                                  join c in _context.ChiTietLichTrongs on l.LichTrongId equals c.LichTrongId
                                  where t.UserId == UserID
                                  select new
                                  {
                                      c.Thu,
                                      c.Buoi
                                  }).ToListAsync();

            if (!lichRanh.Any())
                return NotFound();

            return Ok(lichRanh);
        }
        [HttpGet("GetSuKienDaThamGia")]
        public async Task<IActionResult> GetSuKienDaThamGia([FromQuery] int UserID)
        {
            var suKienDaThamGia = await (
                from dk in _context.DangKySuKiens
                join sk in _context.SuKiens on dk.SuKienId equals sk.SuKienId
                join tnv in _context.TinhNguyenViens on dk.UserId equals tnv.UserId
                join pc in _context.PhanCongTinhNguyenViens on sk.SuKienId equals pc.SuKienId
                where tnv.UserId == UserID
                select new
                {
                    sk.TenSuKien,
                    sk.NgayBatDau,
                    pc.CongViec
                }
            ).ToListAsync();

            if (!suKienDaThamGia.Any())
                return NotFound("Không có sự kiện nào.");

            return Ok(suKienDaThamGia);
        }
        [HttpGet("GetHoTroVanDong")]
        public IActionResult GetHoTroVanDong(int userId)
        {
            var ketQua = (from vdt in _context.VanDongTreEms
                          join nd in _context.NguoiDungs on vdt.NguoiDungId equals nd.UserId
                          join te in _context.TreEms on vdt.TreEmId equals te.TreEmId
                          join hc in _context.HoanCanhs on vdt.HoanCanhId equals hc.HoanCanhId
                          where vdt.NguoiDungId == userId
                          select new
                          {
                              TenTinhNguyenVien = nd.HoTen,
                              TenTreEm = te.HoTen,
                              LoaiHoanCanh = hc.LoaiHoanCanh,
                              MoTaHoanCanh = hc.MoTa,
                              vdt.SoLan,
                              vdt.LyDo,
                              vdt.KetQua,
                              vdt.NgayVanDong
                          }).ToList();

            return Ok(ketQua);
        }


    }
}
