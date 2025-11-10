using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.DTOs.AI;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIPhanLoaiController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;
        public AIPhanLoaiController(QuanLyTreEmContext context)
        {
            _context = context;
        }
        [HttpGet("PhanTichMucDoUuTienTreEm")]
        public async Task<ActionResult<KetQuaPhanTichDto>> PhanTichTatCaTreEm()
        {
            try
            {
                // Lấy tất cả dữ liệu trẻ em và các bảng liên quan
                var danhSachTreEm = await _context.TreEms
                    .Include(t => t.TreEmHoanCanhs)
                        .ThenInclude(th => th.HoanCanh)
                    .Include(t => t.HoTroPhucLois)
                    .Include(t => t.TreEmPhuHuynhs)
                        .ThenInclude(tp => tp.PhuHuynh)
                    .Include(t => t.PhieuHocTaps)
                    .Include(t => t.VanDongTreEms)
                    .Include(t => t.UngHos)
                    .Include(t => t.KhuPho)
                    .Include(t => t.Truong)
                    .ToListAsync();

                // Phân tích từng trẻ
                var danhSachUuTien = new List<MucDoUuTienDto>();
                foreach (var treEm in danhSachTreEm)
                {
                    var mucDoUuTien = TinhToanMucDoUuTien(treEm);
                    danhSachUuTien.Add(mucDoUuTien);
                }

                // Sắp xếp từ cao đến thấp
                danhSachUuTien = danhSachUuTien
                    .OrderByDescending(m => m.DiemUuTien)
                    .ThenBy(m => m.HoTen)
                    .ToList();

                // Tạo kết quả trả về
                var ketQua = new KetQuaPhanTichDto
                {
                    TongSoTreEm = danhSachUuTien.Count,
                    SoTreEmUuTienCao = danhSachUuTien.Count(m => m.MucDoUuTien == "Cao"),
                    SoTreEmUuTienTrungBinh = danhSachUuTien.Count(m => m.MucDoUuTien == "Trung Bình"),
                    SoTreEmUuTienThap = danhSachUuTien.Count(m => m.MucDoUuTien == "Thấp"),
                    ThoiGianPhanTich = DateTime.Now,
                    DanhSachTreEm = danhSachUuTien
                };

                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi phân tích dữ liệu", error = ex.Message });
            }
        }

        private MucDoUuTienDto TinhToanMucDoUuTien(TreEm treEm)
        {
            int tongDiem = 0;
            var chiTietLyDo = new List<string>();
            var deXuat = new List<string>();
            string lyDoChinh = "";

            // Tính tuổi
            int? tuoi = null;
            if (treEm.NgaySinh.HasValue)
            {
                var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                tuoi = ngayHienTai.Year - treEm.NgaySinh.Value.Year;
            }

            // 1. HOÀN CẢNH (0-40 điểm) - Quan trọng nhất
            int diemHoanCanh = 0;
            if (treEm.TreEmHoanCanhs.Any())
            {
                foreach (var hc in treEm.TreEmHoanCanhs)
                {
                    var tenHC = hc.HoanCanh?.LoaiHoanCanh?.ToLower() ?? "";

                    if (tenHC.Contains("mồ côi") && (tenHC.Contains("cả") || tenHC.Contains("hai")))
                    {
                        diemHoanCanh = 40;
                        lyDoChinh = "Trẻ mồ côi cả cha lẫn mẹ";
                        chiTietLyDo.Add("🔴 Hoàn cảnh: Mồ côi cả cha lẫn mẹ - Cần ưu tiên cao nhất");
                        deXuat.Add("Hỗ trợ toàn diện về tài chính và chăm sóc");
                        deXuat.Add("Tìm gia đình hoặc cơ sở nuôi dưỡng");
                        break;
                    }
                    else if (tenHC.Contains("mồ côi") || tenHC.Contains("mẹ") || tenHC.Contains("cha"))
                    {
                        diemHoanCanh = Math.Max(diemHoanCanh, 30);
                        if (string.IsNullOrEmpty(lyDoChinh))
                            lyDoChinh = $"Mất {(tenHC.Contains("mẹ") ? "mẹ" : "cha")}";
                        chiTietLyDo.Add($"🟠 Hoàn cảnh: {hc.HoanCanh?.LoaiHoanCanh}");
                        deXuat.Add("Hỗ trợ phụ huynh đơn thân chăm sóc con");
                    }
                    else if (tenHC.Contains("khuyết tật") || tenHC.Contains("bệnh"))
                    {
                        diemHoanCanh = Math.Max(diemHoanCanh, 30);
                        if (string.IsNullOrEmpty(lyDoChinh))
                            lyDoChinh = "Trẻ có khuyết tật/bệnh tật";
                        chiTietLyDo.Add($"🟠 Sức khỏe: {hc.HoanCanh?.LoaiHoanCanh}");
                        deXuat.Add("Hỗ trợ y tế và phục hồi chức năng");
                    }
                    else if (tenHC.Contains("nghèo") || tenHC.Contains("khó khăn"))
                    {
                        diemHoanCanh = Math.Max(diemHoanCanh, 25);
                        if (string.IsNullOrEmpty(lyDoChinh))
                            lyDoChinh = "Hoàn cảnh gia đình khó khăn";
                        chiTietLyDo.Add($"🟡 Kinh tế: {hc.HoanCanh?.LoaiHoanCanh}");
                        deXuat.Add("Hỗ trợ tài chính định kỳ");
                    }
                    else
                    {
                        diemHoanCanh = Math.Max(diemHoanCanh, 15);
                        if (string.IsNullOrEmpty(lyDoChinh))
                            lyDoChinh = "Hoàn cảnh đặc biệt";
                        chiTietLyDo.Add($"⚪ Hoàn cảnh: {hc.HoanCanh?.LoaiHoanCanh}");
                    }
                }
            }
            else
            {
                diemHoanCanh = 5;
                chiTietLyDo.Add("⚪ Chưa có thông tin hoàn cảnh chi tiết");
            }
            tongDiem += diemHoanCanh;

            // 2. TÌNH TRẠNG HỖ TRỢ (0-25 điểm)
            int diemHoTro = 0;
            var hoTroGanDay = treEm.HoTroPhucLois
                .Where(h => h.NgayCap.HasValue &&
                       h.NgayCap.Value.ToDateTime(TimeOnly.MinValue) > DateTime.Now.AddMonths(-6))
                .ToList();

            if (!treEm.HoTroPhucLois.Any())
            {
                diemHoTro = 25;
                if (string.IsNullOrEmpty(lyDoChinh))
                    lyDoChinh = "Chưa nhận hỗ trợ nào";
                chiTietLyDo.Add("🔴 Hỗ trợ: Chưa nhận bất kỳ hỗ trợ nào");
                deXuat.Add("Khảo sát và lập kế hoạch hỗ trợ ngay");
            }
            else if (!hoTroGanDay.Any())
            {
                diemHoTro = 20;
                chiTietLyDo.Add("🟠 Hỗ trợ: Đã lâu không nhận hỗ trợ (>6 tháng)");
                deXuat.Add("Đánh giá lại và tiếp tục hỗ trợ");
            }
            else if (hoTroGanDay.Count < 2)
            {
                diemHoTro = 10;
                chiTietLyDo.Add($"🟡 Hỗ trợ: Đang nhận {hoTroGanDay.Count} hình thức hỗ trợ");
                deXuat.Add("Cân nhắc tăng cường hỗ trợ");
            }
            else
            {
                diemHoTro = 5;
                chiTietLyDo.Add($"🟢 Hỗ trợ: Đang nhận {hoTroGanDay.Count} hình thức hỗ trợ tốt");
            }

            if (!treEm.UngHos.Any())
            {
                diemHoTro += 5;
                chiTietLyDo.Add("⚪ Chưa có mạnh thường quân ủng hộ");
                if (!deXuat.Any(d => d.Contains("mạnh thường quân")))
                    deXuat.Add("Tìm kiếm nhà hảo tâm hỗ trợ dài hạn");
            }
            tongDiem += diemHoTro;

            // 3. GIA ĐÌNH (0-20 điểm)
            int diemGiaDinh = 0;
            if (!treEm.TreEmPhuHuynhs.Any())
            {
                diemGiaDinh = 20;
                chiTietLyDo.Add("🔴 Gia đình: Không có thông tin phụ huynh");
                if (!deXuat.Any())
                    deXuat.Add("Xác minh người chăm sóc trẻ");
            }
            else
            {
                var phuHuynhs = treEm.TreEmPhuHuynhs.Select(tp => tp.PhuHuynh).ToList();
                if (phuHuynhs.Count <= 1)
                {
                    diemGiaDinh = 15;
                    chiTietLyDo.Add("🟠 Gia đình: Gia đình đơn thân");
                }
                else
                {
                    chiTietLyDo.Add($"🟢 Gia đình: Có {phuHuynhs.Count} người chăm sóc");
                }

                var coNgheOnDinh = phuHuynhs.Any(p =>
                    !string.IsNullOrEmpty(p.NgheNghiep) &&
                    !p.NgheNghiep.ToLower().Contains("thất nghiệp") &&
                    !p.NgheNghiep.ToLower().Contains("không"));

                if (!coNgheOnDinh)
                {
                    diemGiaDinh += 10;
                    chiTietLyDo.Add("🟠 Kinh tế: Phụ huynh không có nghề ổn định");
                }
            }
            tongDiem += diemGiaDinh;

            // 4. HỌC TẬP (0-15 điểm)
            int diemHocTap = 0;
            if (treEm.TruongId == null)
            {
                diemHocTap = 15;
                chiTietLyDo.Add("🔴 Học tập: Trẻ chưa đi học/đã bỏ học");
                deXuat.Add("Hỗ trợ để trẻ được đến trường");
            }
            else
            {
                var phieuMoiNhat = treEm.PhieuHocTaps.LastOrDefault();
                if (phieuMoiNhat == null)
                {
                    diemHocTap = 10;
                    chiTietLyDo.Add("🟡 Học tập: Không có thông tin học tập");
                }
                else
                {
                    var hocLuc = phieuMoiNhat.XepLoai?.ToLower() ?? "";
                    if (hocLuc.Contains("yếu") || hocLuc.Contains("kém"))
                    {
                        diemHocTap = 12;
                        chiTietLyDo.Add("🟠 Học tập: Học lực yếu");
                        deXuat.Add("Hỗ trợ học bổng và dạy kèm");
                    }
                    else if (hocLuc.Contains("trung bình"))
                    {
                        diemHocTap = 6;
                        chiTietLyDo.Add("🟡 Học tập: Học lực trung bình");
                    }
                    else
                    {
                        chiTietLyDo.Add("🟢 Học tập: Học lực tốt");
                    }
                }
            }
            tongDiem += diemHocTap;

            // 5. ĐỘ TUỔI (0-10 điểm)
            int diemDoTuoi = 0;
            if (tuoi.HasValue)
            {
                if (tuoi < 6)
                {
                    diemDoTuoi = 10;
                    chiTietLyDo.Add($"🟠 Độ tuổi: Trẻ nhỏ ({tuoi} tuổi) - cần chăm sóc đặc biệt");
                }
                else if (tuoi >= 15 && tuoi < 18)
                {
                    diemDoTuoi = 8;
                    chiTietLyDo.Add($"🟡 Độ tuổi: Tuổi vị thành niên ({tuoi} tuổi) - cần định hướng");
                    if (!deXuat.Any(d => d.Contains("nghề")))
                        deXuat.Add("Hướng nghiệp và dạy nghề");
                }
                else if (tuoi >= 6 && tuoi < 11)
                {
                    diemDoTuoi = 6;
                    chiTietLyDo.Add($"⚪ Độ tuổi: Tiểu học ({tuoi} tuổi)");
                }
                else if (tuoi >= 11 && tuoi < 15)
                {
                    diemDoTuoi = 5;
                    chiTietLyDo.Add($"⚪ Độ tuổi: Trung học ({tuoi} tuổi)");
                }
            }
            tongDiem += diemDoTuoi;

            // 6. VẬN ĐỘNG (0-10 điểm)
            int diemVanDong = 0;
            var vanDongGanDay = treEm.VanDongTreEms
                .Where(v => v.NgayVanDong.HasValue &&
                       v.NgayVanDong.Value.ToDateTime(TimeOnly.MinValue) > DateTime.Now.AddMonths(-3))
                .ToList();

            if (vanDongGanDay.Any())
            {
                var soLan = vanDongGanDay.Sum(v => v.SoLan ?? 0);
                if (soLan >= 3)
                {
                    diemVanDong = 10;
                    chiTietLyDo.Add($"🔴 Vận động: Đã vận động {soLan} lần nhưng chưa có kết quả");
                }
                else if (soLan > 0)
                {
                    diemVanDong = 5;
                    chiTietLyDo.Add($"🟡 Vận động: Đang vận động hỗ trợ ({soLan} lần)");
                }
            }
            tongDiem += diemVanDong;

            // Xác định mức độ ưu tiên
            string mucDoUuTien;
            if (tongDiem >= 70)
            {
                mucDoUuTien = "Cao";
                if (string.IsNullOrEmpty(lyDoChinh))
                    lyDoChinh = "Tổng hợp nhiều yếu tố khó khăn";
                if (!deXuat.Any())
                {
                    deXuat.Add("⚠️ Ưu tiên hỗ trợ khẩn cấp");
                    deXuat.Add("Theo dõi sát sao hàng tuần");
                }
            }
            else if (tongDiem >= 40)
            {
                mucDoUuTien = "Trung Bình";
                if (string.IsNullOrEmpty(lyDoChinh))
                    lyDoChinh = "Cần hỗ trợ định kỳ";
                if (!deXuat.Any())
                {
                    deXuat.Add("Hỗ trợ định kỳ 3-6 tháng");
                    deXuat.Add("Đánh giá lại tình hình thường xuyên");
                }
            }
            else
            {
                mucDoUuTien = "Thấp";
                if (string.IsNullOrEmpty(lyDoChinh))
                    lyDoChinh = "Tình trạng ổn định";
                if (!deXuat.Any())
                {
                    deXuat.Add("Theo dõi định kỳ 6-12 tháng");
                    deXuat.Add("Khuyến khích gia đình tự lực");
                }
            }

            return new MucDoUuTienDto
            {
                TreEmId = treEm.TreEmId,
                HoTen = treEm.HoTen,
                Tuoi = tuoi,
                GioiTinh = treEm.GioiTinh,
                KhuPho = treEm.KhuPho?.TenKhuPho,
                TruongHoc = treEm.Truong?.TenTruong,
                LyDoChinh = lyDoChinh,
                MucDoUuTien = mucDoUuTien,
                DiemUuTien = tongDiem,
<<<<<<< HEAD
                LyDoChinh = lyDoChinh,
=======
>>>>>>> 1bc039af6e93ce6b5411f76df0027acee4880dd0
                ChiTietLyDo = chiTietLyDo,
                DeXuatHoTro = deXuat
            };
        }
    }
}

