using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyTreEmAPI.DTOs.AI;
using QuanLyTreEmAPI.Services;
using System.Text;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClusteringController : ControllerBase
    {
        private readonly ITreEmAnalysisService _analysisService;
        private readonly IMLClusteringService _mlService;
        private readonly ILogger<ClusteringController> _logger;
        public ClusteringController(
          ITreEmAnalysisService analysisService,
          IMLClusteringService mlService,
          ILogger<ClusteringController> logger)
        {
            _analysisService = analysisService;
            _mlService = mlService;
            _logger = logger;
        }

        /// <summary>
        /// Kiểm tra trạng thái ML Service
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> CheckHealth()
        {
            var isHealthy = await _mlService.IsHealthyAsync();

            if (isHealthy)
                return Ok(new { status = "healthy", service = "ML Clustering Service" });
            else
                return StatusCode(503, new { status = "unhealthy", message = "ML Service không hoạt động" });
        }

        /// <summary>
        /// Phân tích 1 trẻ em
        /// GET: api/clustering/analyze/5
        /// </summary>
        [HttpGet("analyze/{treEmId}")]
        public async Task<ActionResult<ClusterPrediction>> AnalyzeChild(int treEmId)
        {
            try
            {
                _logger.LogInformation($"Analyzing child: {treEmId}");

                var features = await _analysisService.ExtractFeaturesAsync(treEmId);

                if (features == null)
                    return NotFound(new { message = $"Không tìm thấy trẻ em với ID: {treEmId}" });

                var prediction = await _mlService.PredictAsync(features);

                return Ok(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error analyzing child {treEmId}: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Phân tích tất cả trẻ em
        /// GET: api/clustering/analyze-all
        /// </summary>
        [HttpGet("analyze-all")]
        public async Task<ActionResult<List<TreEmWithPriority>>> AnalyzeAllChildren()
        {
            try
            {
                var results = await _analysisService.AnalyzeAllChildrenAsync();

                return Ok(new
                {
                    total = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error analyzing all children: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Phân tích trẻ em theo khu phố
        /// GET: api/clustering/analyze-by-khupho/3
        /// </summary>
        [HttpGet("analyze-by-khupho/{khuPhoId}")]
        public async Task<ActionResult<List<TreEmWithPriority>>> AnalyzeByKhuPho(int khuPhoId)
        {
            try
            {
                var results = await _analysisService.AnalyzeByKhuPhoAsync(khuPhoId);

                return Ok(new
                {
                    khu_pho_id = khuPhoId,
                    total = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error analyzing khu pho {khuPhoId}: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách theo mức độ ưu tiên
        /// GET: api/clustering/priority-groups
        /// </summary>
        [HttpGet("priority-groups")]
        public async Task<ActionResult> GetPriorityGroups()
        {
            try
            {
                var results = await _analysisService.AnalyzeAllChildrenAsync();

                var grouped = results
                    .GroupBy(r => r.PriorityLevel)
                    .Select(g => new
                    {
                        priority_level = g.Key,
                        count = g.Count(),
                        percentage = Math.Round((double)g.Count() / results.Count * 100, 2),
                        children = g.Select(c => new
                        {
                            tre_em_id = c.TreEmId,
                            cluster = c.Cluster,
                            confidence = c.Confidence,
                            features = c.Features
                        }).ToList()
                    })
                    .OrderBy(g => g.priority_level switch
                    {
                        "CAO NHẤT" => 1,
                        "CAO" => 2,
                        "TRUNG BÌNH" => 3,
                        "THẤP" => 4,
                        _ => 5
                    })
                    .ToList();

                return Ok(new
                {
                    total = results.Count,
                    groups = grouped
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting priority groups: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê tổng quan
        /// GET: api/clustering/statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetStatistics()
        {
            try
            {
                var stats = await _analysisService.GetPriorityStatisticsAsync();

                var total = stats.Values.Sum();

                var statsWithPercentage = stats.Select(s => new
                {
                    priority_level = s.Key,
                    count = s.Value,
                    percentage = total > 0 ? Math.Round((double)s.Value / total * 100, 2) : 0
                })
                .OrderBy(s => s.priority_level switch
                {
                    "CAO NHẤT" => 1,
                    "CAO" => 2,
                    "TRUNG BÌNH" => 3,
                    "THẤP" => 4,
                    _ => 5
                })
                .ToList();

                return Ok(new
                {
                    total_children = total,
                    priority_distribution = statsWithPercentage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting statistics: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách trẻ cần ưu tiên cao nhất
        /// GET: api/clustering/high-priority?limit=20
        /// </summary>
        [HttpGet("high-priority")]
        public async Task<ActionResult> GetHighPriorityChildren([FromQuery] int limit = 20)
        {
            try
            {
                var results = await _analysisService.AnalyzeAllChildrenAsync();

                var highPriority = results
                    .Where(r => r.PriorityLevel == "CAO NHẤT" || r.PriorityLevel == "CAO")
                    .OrderBy(r => r.PriorityRank)
                    .Take(limit)
                    .Select(r => new
                    {
                        tre_em_id = r.TreEmId,
                        priority_level = r.PriorityLevel,
                        priority_rank = r.PriorityRank,
                        confidence = r.Confidence,
                        color = r.Color,
                        features = r.Features,
                        recommendations = r.Recommendations
                    })
                    .ToList();

                return Ok(new
                {
                    total = highPriority.Count,
                    limit = limit,
                    data = highPriority
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting high priority children: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm trẻ em theo features
        /// POST: api/clustering/search
        /// Body: { "min_hoctap": 0, "max_hoctap": 50, "priority_levels": ["CAO NHẤT"] }
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult> SearchChildren([FromBody] SearchCriteria criteria)
        {
            try
            {
                var results = await _analysisService.AnalyzeAllChildrenAsync();

                // Lọc theo criteria
                var filtered = results.AsQueryable();

                if (criteria.PriorityLevels != null && criteria.PriorityLevels.Any())
                {
                    filtered = filtered.Where(r => criteria.PriorityLevels.Contains(r.PriorityLevel));
                }

                if (criteria.MinHocTap.HasValue)
                {
                    filtered = filtered.Where(r => r.Features.HocTap >= criteria.MinHocTap.Value);
                }

                if (criteria.MaxHocTap.HasValue)
                {
                    filtered = filtered.Where(r => r.Features.HocTap <= criteria.MaxHocTap.Value);
                }

                if (criteria.MinNguyCoBoHoc.HasValue)
                {
                    filtered = filtered.Where(r => r.Features.NguyCoBoHoc >= criteria.MinNguyCoBoHoc.Value);
                }

                var finalResults = filtered.ToList();

                return Ok(new
                {
                    total = finalResults.Count,
                    criteria = criteria,
                    data = finalResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching children: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Export dữ liệu phân cụm ra CSV
        /// GET: api/clustering/export-csv
        /// </summary>
        [HttpGet("export-csv")]
        public async Task<IActionResult> ExportCsv()
        {
            try
            {
                var results = await _analysisService.AnalyzeAllChildrenAsync();

                var csv = new StringBuilder();
                csv.AppendLine("TreEmID,PriorityLevel,Cluster,Confidence,HocTap,HanhVi,TamLy,GiaDinh,NguyCoBoHoc");

                foreach (var result in results)
                {
                    csv.AppendLine($"{result.TreEmId},{result.PriorityLevel},{result.Cluster}," +
                        $"{result.Confidence},{result.Features.HocTap},{result.Features.HanhVi}," +
                        $"{result.Features.TamLy},{result.Features.GiaDinh},{result.Features.NguyCoBoHoc}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"tre_em_clustering_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error exporting CSV: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // DTO cho tìm kiếm
    public class SearchCriteria
    {
        public List<string> PriorityLevels { get; set; }
        public float? MinHocTap { get; set; }
        public float? MaxHocTap { get; set; }
        public float? MinHanhVi { get; set; }
        public float? MaxHanhVi { get; set; }
        public float? MinTamLy { get; set; }
        public float? MaxTamLy { get; set; }
        public float? MinGiaDinh { get; set; }
        public float? MaxGiaDinh { get; set; }
        public float? MinNguyCoBoHoc { get; set; }
        public float? MaxNguyCoBoHoc { get; set; }
    }
}
