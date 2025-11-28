using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyTreEmAPI.DTOs.AI;
using QuanLyTreEmAPI.Models;
using QuanLyTreEmAPI.Services;

namespace QuanLyTreEmAPI.Services
{
    public interface ITreEmAnalysisService
    {
        Task<TreEmFeatures> ExtractFeaturesAsync(int treEmId);
        Task<List<TreEmWithPriority>> AnalyzeAllChildrenAsync();
        Task<List<TreEmWithPriority>> AnalyzeByKhuPhoAsync(int khuPhoId);
        Task<Dictionary<string, int>> GetPriorityStatisticsAsync();
    }

    public class TreEmAnalysisService : ITreEmAnalysisService
    {
        private readonly QuanLyTreEmContext _context;
        private readonly IMLClusteringService _mlService;
        private readonly ILogger<TreEmAnalysisService> _logger;

        public TreEmAnalysisService(
            QuanLyTreEmContext context,
            IMLClusteringService mlService,
            ILogger<TreEmAnalysisService> logger)
        {
            _context = context;
            _mlService = mlService;
            _logger = logger;
        }

        public async Task<TreEmFeatures> ExtractFeaturesAsync(int treEmId)
        {
            try
            {
                var treEm = await _context.TreEms
                    .Include(t => t.PhieuHocTaps)
                      .Include(th => th.TreEmHoanCanhs)
                    .Include(t => t.VanDongTreEms)
                    .Include(t => t.PhanPhatQuas)
                    .Include(t => t.TreEmPhuHuynhs)
                        .ThenInclude(tp => tp.PhuHuynh)
                    .FirstOrDefaultAsync(t => t.TreEmId == treEmId);

                if (treEm == null)
                {
                    _logger.LogWarning($"TreEm not found: {treEmId}");
                    return null;
                }

                // 1. ĐIỂM HỌC TẬP (HocTap)
                float hocTap = TinhDiemHocTap(treEm);

                // 2. ĐIỂM HÀNH VI (HanhVi)
                float hanhVi = TinhDiemHanhVi(treEm);

                // 3. ĐIỂM TÂM LÝ (TamLy)
                float tamLy = TinhDiemTamLy(treEm);

                // 4. ĐIỂM GIA ĐÌNH (GiaDinh)
                float giaDinh = TinhDiemGiaDinh(treEm);

                // 5. NGUY CƠ BỎ HỌC (NguyCoBoHoc)
                var nguyCo = TinhNguyCoBoHoc(treEm, hocTap, hanhVi, tamLy, giaDinh);
                var features = new TreEmFeatures
                {
                    TreEmId = treEmId,
                    HocTap = hocTap,
                    HanhVi = hanhVi,
                    TamLy = tamLy,
                    GiaDinh = giaDinh,
                    NguyCoBoHoc = nguyCo
                };

                _logger.LogInformation($"Features extracted for TreEm {treEmId}: " +
                    $"HocTap={hocTap:F1}, HanhVi={hanhVi:F1}, TamLy={tamLy:F1}, " +
                    $"GiaDinh={giaDinh:F1}, NguyCoBoHoc={nguyCo:F1}");

                return features;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting features for TreEm {treEmId}: {ex.Message}");
                throw;
            }
        }

        private float TinhDiemHocTap(TreEm treEm)
        {
            // Lấy phiếu học tập gần nhất
            var phieuHocTap = treEm.PhieuHocTaps
                ?.OrderByDescending(p => p.NamHoc)
                .FirstOrDefault();

            if (phieuHocTap == null)
                return 50f; // Điểm trung bình nếu không có dữ liệu

            float diem = (float)(phieuHocTap.DiemTrungBinh * 10); // Chuyển về thang 100

            // Điều chỉnh theo xếp loại
            var adjustment = phieuHocTap.XepLoai?.ToLower() switch
            {
                "giỏi" => 10f,
                "khá" => 5f,
                "trung bình" => 0f,
                "yếu" => -10f,
                _ => 0f
            };

            diem += adjustment;

            return Math.Clamp(diem, 0, 100);
        }

        private float TinhDiemHanhVi(TreEm treEm)
        {
            var phieuHocTap = treEm.PhieuHocTaps
                ?.OrderByDescending(p => p.NamHoc)
                .FirstOrDefault();

            if (phieuHocTap == null)
                return 70f;

            var diem = phieuHocTap.HanhKiem?.ToLower() switch
            {
                "tốt" => 90f,
                "khá" => 75f,
                "trung bình" => 60f,
                "yếu" => 40f,
                _ => 70f
            };

            // Điều chỉnh nếu có số lần vận động nhiều (có vấn đề hành vi)
            int soLanVanDong = treEm.VanDongTreEms?.Count ?? 0;
            if (soLanVanDong > 3)
                diem -= 10;

            return Math.Clamp(diem, 0, 100);
        }

        private float TinhDiemTamLy(TreEm treEm)
        {
            float diem = 70f; // Điểm gốc

            // Giảm điểm nếu có hoàn cảnh khó khăn
            int soHoanCanh = treEm.TreEmHoanCanhs?.Count ?? 0;
            diem -= soHoanCanh * 10;

            // Giảm điểm nếu mồ côi hoặc thiếu phụ huynh
            bool coMeCha = treEm.TreEmPhuHuynhs?.Any() ?? false;
            if (!coMeCha)
                diem -= 20;

            // Giảm điểm nếu có hoàn cảnh đặc biệt khó khăn
            var hoanCanhList = treEm.TreEmHoanCanhs?
             .Where(th => th.HoanCanh != null && th.HoanCanh.LoaiHoanCanh != null)
             .Select(th => th.HoanCanh.LoaiHoanCanh!.ToLower())
             .ToList() ?? new List<string>();

            if (hoanCanhList != null)
            {
                if (hoanCanhList.Any(h => h.Contains("mồ côi")))
                    diem -= 15;
                if (hoanCanhList.Any(h => h.Contains("khuyết tật") || h.Contains("bệnh")))
                    diem -= 10;
            }

            return Math.Clamp(diem, 0, 100);
        }

        private float TinhDiemGiaDinh(TreEm treEm)
        {
            float diem = 50f; // Điểm gốc

            // Có phụ huynh
            bool coMeCha = treEm.TreEmPhuHuynhs?.Any() ?? false;
            if (coMeCha)
                diem += 20;
            else
                diem -= 30; // Mồ côi

            // Đánh giá hoàn cảnh
            var hoanCanhList = treEm.TreEmHoanCanhs?
         .Where(th => th.HoanCanh != null && th.HoanCanh.LoaiHoanCanh != null)
         .Select(th => th.HoanCanh.LoaiHoanCanh!.ToLower())
         .ToList() ?? new List<string>();


            if (hoanCanhList != null)
            {
                if (hoanCanhList.Any(h => h.Contains("nghèo") || h.Contains("khó khăn")))
                    diem -= 20;
                if (hoanCanhList.Any(h => h.Contains("mồ côi")))
                    diem -= 25;
                if (hoanCanhList.Any(h => h.Contains("bạo lực") || h.Contains("ly hôn")))
                    diem -= 15;
            }

            // Số lần nhận hỗ trợ (nhiều = hoàn cảnh khó khăn)
            int soLanNhanHoTro = treEm.PhanPhatQuas?.Count ?? 0;
            if (soLanNhanHoTro > 5)
                diem -= 10;

            return Math.Clamp(diem, 0, 100);
        }

        private float TinhNguyCoBoHoc(TreEm treEm, float hocTap, float hanhVi,
                                      float tamLy, float giaDinh)
        {
            // Công thức tính nguy cơ bỏ học dựa trên các yếu tố
            float nguyCoCoSo = 0;

            // Học tập kém = nguy cơ cao
            if (hocTap < 50)
                nguyCoCoSo += (50 - hocTap) * 0.5f;

            // Hành vi kém = nguy cơ cao
            if (hanhVi < 60)
                nguyCoCoSo += (60 - hanhVi) * 0.3f;

            // Tâm lý kém = nguy cơ cao
            if (tamLy < 60)
                nguyCoCoSo += (60 - tamLy) * 0.4f;

            // Gia đình khó khăn = nguy cơ cao
            if (giaDinh < 50)
                nguyCoCoSo += (50 - giaDinh) * 0.6f;

            // Số lần vận động nhiều = dấu hiệu cảnh báo
            int soLanVanDong = treEm.VanDongTreEms?.Count ?? 0;
            nguyCoCoSo += soLanVanDong * 5;

            // Tuổi (trẻ lớn hơn có nguy cơ cao hơn)
            int tuoi = 0;
            if (treEm.NgaySinh.HasValue)
            {
                DateTime ngaySinh = treEm.NgaySinh.Value.ToDateTime(TimeOnly.MinValue);
                tuoi = DateTime.Now.Year - ngaySinh.Year;
            }

            if (tuoi >= 14)
                nguyCoCoSo += 10;

            return Math.Clamp(nguyCoCoSo, 0, 100);
        }

        public async Task<List<TreEmWithPriority>> AnalyzeAllChildrenAsync()
        {
            var allTreEm = await _context.TreEms
                .Select(t => new { t.TreEmId, t.HoTen, t.NgaySinh, t.GioiTinh }) // Thêm các field này
                .ToListAsync();

            var results = new List<TreEmWithPriority>();

            foreach (var treEm in allTreEm)
            {
                try
                {
                    var features = await ExtractFeaturesAsync(treEm.TreEmId);
                    if (features != null)
                    {
                        var prediction = await _mlService.PredictAsync(features);

                        results.Add(new TreEmWithPriority
                        {
                            TreEmId = treEm.TreEmId,
                            TenTre = treEm.HoTen,           
                            NgaySinh = treEm.NgaySinh?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                            GioiTinh = treEm.GioiTinh,      
                            Cluster = prediction.Cluster,
                            PriorityLevel = prediction.PriorityLevel,
                            PriorityRank = prediction.PriorityRank,
                            Confidence = prediction.Confidence,
                            Color = prediction.Color,
                            Recommendations = prediction.Recommendations,
                            Features = features
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error analyzing TreEm {treEm.TreEmId}: {ex.Message}");
                }
            }

            return results.OrderBy(r => r.PriorityRank).ToList();
        }

        public async Task<List<TreEmWithPriority>> AnalyzeByKhuPhoAsync(int khuPhoId)
        {
            var treEmList = await _context.TreEms
                .Where(t => t.KhuPhoId == khuPhoId)
                .Select(t => new { t.TreEmId, t.HoTen, t.NgaySinh, t.GioiTinh })
                .ToListAsync();

            var results = new List<TreEmWithPriority>();

            foreach (var treEm in treEmList)
            {
                var features = await ExtractFeaturesAsync(treEm.TreEmId);
                if (features != null)
                {
                    var prediction = await _mlService.PredictAsync(features);
                    results.Add(new TreEmWithPriority
                    {
                        TreEmId = treEm.TreEmId,
                        TenTre = treEm.HoTen,
                        NgaySinh = treEm.NgaySinh?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                        GioiTinh = treEm.GioiTinh,
                        Cluster = prediction.Cluster,
                        PriorityLevel = prediction.PriorityLevel,
                        PriorityRank = prediction.PriorityRank,
                        Confidence = prediction.Confidence,
                        Features = features
                    });
                }
            }

            return results.OrderBy(r => r.PriorityRank).ToList();
        }

        public async Task<Dictionary<string, int>> GetPriorityStatisticsAsync()
        {
            var all = await AnalyzeAllChildrenAsync();

            return all.GroupBy(a => a.PriorityLevel)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }

    public class TreEmWithPriority
    {
        public int TreEmId { get; set; }
        public string TenTre { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public int Cluster { get; set; }
        public string PriorityLevel { get; set; }
        public int PriorityRank { get; set; }
        public float Confidence { get; set; }
        public string Color { get; set; }
        public List<string> Recommendations { get; set; }
        public TreEmFeatures Features { get; set; }
    }
}