//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QuanLyTreEmAPI.DTOs.AI;
//using QuanLyTreEmAPI.Models;

//namespace QuanLyTreEmAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AIPhanCumController : ControllerBase
//    {
//        private readonly QuanLyTreEmContext _context;

//        public AIPhanCumController(QuanLyTreEmContext context)
//        {
//            _context = context;
//        }

//        [HttpGet("PhanCumDoUuTienCapBach")]
//        public async Task<ActionResult<KetQuaPhanCumDto>> PhanCumTreEmTheoDoCapBach()
//        {
//            try
//            {
//                // Lấy toàn bộ dữ liệu trẻ em
//                var danhSachTreEm = await _context.TreEms
//                    .Include(t => t.TreEmHoanCanhs)
//                        .ThenInclude(th => th.HoanCanh)
//                    .Include(t => t.HoTroPhucLois)
//                    .Include(t => t.TreEmPhuHuynhs)
//                        .ThenInclude(tp => tp.PhuHuynh)
//                    .Include(t => t.PhieuHocTaps)
//                    .Include(t => t.KhuPho)
//                    .Include(t => t.Truong)
//                    .ToListAsync();

//                // Phân tích và phân cụm từng trẻ
//                var danhSachPhanTich = new List<TreEmDonGianDto>();

//                foreach (var treEm in danhSachTreEm)
//                {
//                    var phanTich = PhanTichDonGian(treEm);
//                    danhSachPhanTich.Add(phanTich);
//                }

//                // Sắp xếp theo điểm cấp bách
//                var danhSachSapXep = danhSachPhanTich
//                    .OrderByDescending(t => t.DiemCapBach)
//                    .ToList();

//                var ketQua = new KetQuaPhanCumDto
//                {
//                    TongSoTreEm = danhSachSapXep.Count,
//                    DanhSachTreEm = danhSachSapXep
//                };

//                return Ok(ketQua);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new
//                {
//                    message = "Lỗi khi phân cụm dữ liệu",
//                    error = ex.Message
//                });
//            }
//        }

//        private TreEmDonGianDto PhanTichDonGian(TreEm treEm)
//        {
//            // Tính tuổi
//            int? tuoi = null;
//            if (treEm.NgaySinh.HasValue)
//            {
//                var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
//                tuoi = ngayHienTai.Year - treEm.NgaySinh.Value.Year;
//            }

//            // Tính điểm rủi ro
//            double diemRuiRo = 0;
//            var lyDoChinhList = new List<string>();
//            var huongGiaiQuyet = new List<string>();

//            // === PHÂN TÍCH RỦI RO ===

//            // 1. Hoàn cảnh nguy hiểm
//            foreach (var hc in treEm.TreEmHoanCanhs)
//            {
//                var tenHC = hc.HoanCanh?.LoaiHoanCanh?.ToLower() ?? "";

//                if (tenHC.Contains("mồ côi") && (tenHC.Contains("cả") || tenHC.Contains("hai")))
//                {
//                    diemRuiRo += 50;
//                    lyDoChinhList.Add("Mồ côi cả cha lẫn mẹ");
//                    huongGiaiQuyet.Add("Tìm gia đình/cơ sở nuôi dưỡng KHẨN CẤP");
//                }
//                else if (tenHC.Contains("lang thang") || tenHC.Contains("vô gia cư"))
//                {
//                    diemRuiRo += 45;
//                    lyDoChinhList.Add("Sống lang thang/vô gia cư");
//                    huongGiaiQuyet.Add("Can thiệp cứu trợ và tái hòa nhập xã hội");
//                }
//                else if (tenHC.Contains("bạo hành") || tenHC.Contains("xâm hại"))
//                {
//                    diemRuiRo += 40;
//                    lyDoChinhList.Add("Bị bạo hành/xâm hại");
//                    huongGiaiQuyet.Add("Báo cơ quan chức năng và bảo vệ trẻ");
//                }
//                else if (tenHC.Contains("bệnh") && tenHC.Contains("hiểm nghèo"))
//                {
//                    diemRuiRo += 45;
//                    lyDoChinhList.Add("Mắc bệnh hiểm nghèo");
//                    huongGiaiQuyet.Add("Hỗ trợ y tế và chi phí điều trị");
//                }
//                else if (tenHC.Contains("khuyết tật") && tenHC.Contains("nặng"))
//                {
//                    diemRuiRo += 40;
//                    lyDoChinhList.Add("Khuyết tật nặng");
//                    huongGiaiQuyet.Add("Hỗ trợ phục hồi chức năng");
//                }
//                else if (tenHC.Contains("suy dinh dưỡng"))
//                {
//                    diemRuiRo += 30;
//                    lyDoChinhList.Add("Suy dinh dưỡng");
//                    huongGiaiQuyet.Add("Hỗ trợ dinh dưỡng và theo dõi sức khỏe");
//                }
//                else if (tenHC.Contains("mồ côi"))
//                {
//                    diemRuiRo += 25;
//                    lyDoChinhList.Add("Mồ côi cha hoặc mẹ");
//                    huongGiaiQuyet.Add("Hỗ trợ phụ huynh còn lại và tư vấn tâm lý");
//                }
//            }

//            // 2. Không có người giám hộ
//            if (!treEm.TreEmPhuHuynhs.Any())
//            {
//                diemRuiRo += 30;
//                lyDoChinhList.Add("Không có người giám hộ");
//                huongGiaiQuyet.Add("Xác minh và đăng ký người giám hộ");
//            }

//            // 3. Không đi học
//            if (treEm.TruongId == null)
//            {
//                if (tuoi.HasValue && tuoi >= 6)
//                {
//                    diemRuiRo += 35;
//                    lyDoChinhList.Add("Không đi học (đã đến tuổi)");
//                    huongGiaiQuyet.Add("Hỗ trợ học phí và tìm trường học");
//                }
//            }
//            else
//            {
//                // Kiểm tra học lực
//                var phieuMoiNhat = treEm.PhieuHocTaps.OrderByDescending(p => p.NamHoc).FirstOrDefault();
//                if (phieuMoiNhat != null)
//                {
//                    var hocLuc = phieuMoiNhat.XepLoai?.ToLower() ?? "";
//                    if (hocLuc.Contains("yếu") || hocLuc.Contains("kém"))
//                    {
//                        diemRuiRo += 20;
//                        lyDoChinhList.Add("Học lực yếu kém");
//                        huongGiaiQuyet.Add("Hỗ trợ học bổng và dạy kèm");
//                    }
//                }
//            }

//            // 4. Kiểm tra hỗ trợ
//            var hoTroGanDay = treEm.HoTroPhucLois
//                .Where(h => h.NgayCap.HasValue &&
//                       h.NgayCap.Value.ToDateTime(TimeOnly.MinValue) > DateTime.Now.AddMonths(-6))
//                .ToList();

//            if (!hoTroGanDay.Any() && diemRuiRo > 30)
//            {
//                // Chưa nhận hỗ trợ dù có hoàn cảnh khó khăn -> TĂNG điểm rủi ro
//                diemRuiRo += 15;
//                lyDoChinhList.Add("Chưa nhận hỗ trợ dù có hoàn cảnh khó khăn");
//                huongGiaiQuyet.Add("Khảo sát và triển khai hỗ trợ ngay");
//            }
//            else if (hoTroGanDay.Any())
//            {
//                // Đã nhận hỗ trợ gần đây -> GIẢM điểm rủi ro
//                int soHoTro = hoTroGanDay.Count;
//                double giamDiem = 0;

//                if (soHoTro >= 3)
//                {
//                    giamDiem = 20; // Nhận nhiều hỗ trợ
//                }
//                else if (soHoTro == 2)
//                {
//                    giamDiem = 15;
//                }
//                else
//                {
//                    giamDiem = 10; // Nhận 1 hỗ trợ
//                }

//                diemRuiRo = Math.Max(0, diemRuiRo - giamDiem);

//                // Không thêm vào lý do chính, nhưng ghi nhận trong hướng giải quyết
//                if (diemRuiRo > 0)
//                {
//                    huongGiaiQuyet.Add($"Tiếp tục duy trì và mở rộng hỗ trợ (đã có {soHoTro} hỗ trợ)");
//                }
//            }

//            // 5. Trẻ nhỏ
//            if (tuoi.HasValue && tuoi < 3)
//            {
//                diemRuiRo += 10;
//                lyDoChinhList.Add($"Trẻ nhỏ ({tuoi} tuổi)");
//                huongGiaiQuyet.Add("Theo dõi sát sức khỏe và tiêm chủng");
//            }

//            // Giới hạn điểm tối đa
//            diemRuiRo = Math.Min(diemRuiRo, 100);

//            // Xác định mức độ
//            string mucDo;
//            if (diemRuiRo >= 80)
//                mucDo = "Cao";
//            else if (diemRuiRo >= 50)
//                mucDo = "Trung bình";
//            else if (diemRuiRo >= 30)
//                mucDo = "Thấp";
//            else
//                mucDo = "Ổn định";

//            // Lấy lý do chính (tối đa 3)
//            var lyDoChinh = lyDoChinhList.Take(3).ToList();
//            if (!lyDoChinh.Any())
//            {
//                lyDoChinh.Add("Không có yếu tố rủi ro đặc biệt");
//            }

//            // Lấy hướng giải quyết (tối đa 3)
//            var huongGiaiQuyetChinh = huongGiaiQuyet.Distinct().Take(3).ToList();
//            if (!huongGiaiQuyetChinh.Any())
//            {
//                huongGiaiQuyetChinh.Add("Theo dõi định kỳ");
//            }

//            return new TreEmDonGianDto
//            {
//                TreEmId = treEm.TreEmId,
//                TenTreEm = treEm.HoTen,
//                Tuoi = tuoi,
//                KhuPho = treEm.KhuPho?.TenKhuPho ?? "Chưa xác định",
//                MucDo = mucDo,
//                DiemCapBach = Math.Round(diemRuiRo, 2),
//                LyDoChinh = lyDoChinh,
//                HuongGiaiQuyet = huongGiaiQuyetChinh
//            };
//        }
//    }

//    // ===================== DTOs =====================

//    public class KetQuaPhanCumDto
//    {
//        public int TongSoTreEm { get; set; }
//        public List<TreEmDonGianDto> DanhSachTreEm { get; set; } = new();
//    }

//    public class TreEmDonGianDto
//    {
//        public int TreEmId { get; set; }
//        public string TenTreEm { get; set; }
//        public int? Tuoi { get; set; }
//        public string KhuPho { get; set; }
//        public string MucDo { get; set; } // Cao, Trung bình, Thấp, Ổn định
//        public double DiemCapBach { get; set; }
//        public List<string> LyDoChinh { get; set; } = new();
//        public List<string> HuongGiaiQuyet { get; set; } = new();
//    }
//}