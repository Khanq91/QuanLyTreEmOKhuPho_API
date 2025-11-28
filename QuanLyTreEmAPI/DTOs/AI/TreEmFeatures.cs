using System.Text.Json.Serialization;

namespace QuanLyTreEmAPI.DTOs.AI
{
    public class TreEmFeatures
    {
        [JsonPropertyName("tre_em_id")]
        public int TreEmId { get; set; }

        [JsonPropertyName("HocTap")]
        public float HocTap { get; set; }

        [JsonPropertyName("HanhVi")]
        public float HanhVi { get; set; }

        [JsonPropertyName("TamLy")]
        public float TamLy { get; set; }

        [JsonPropertyName("GiaDinh")]
        public float GiaDinh { get; set; }

        [JsonPropertyName("NguyCoBoHoc")]
        public float NguyCoBoHoc { get; set; }
    }
    public class ClusterPrediction
    {
        [JsonPropertyName("cluster")]
        public int Cluster { get; set; }

        [JsonPropertyName("priority_level")]
        public string PriorityLevel { get; set; }

        [JsonPropertyName("priority_rank")]
        public int PriorityRank { get; set; }

        [JsonPropertyName("priority_score")]
        public float PriorityScore { get; set; }

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; set; }

        [JsonPropertyName("immediate_actions")]
        public List<string> ImmediateActions { get; set; }

        [JsonPropertyName("feature_analysis")]
        public Dictionary<string, FeatureAnalysis> FeatureAnalysis { get; set; }

        [JsonPropertyName("input_data")]
        public TreEmFeatures InputData { get; set; }
    }
    public class FeatureAnalysis
    {
        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }
    }

    public class BatchPredictionRequest
    {
        [JsonPropertyName("children")]
        public List<TreEmFeatures> Children { get; set; }
    }

    public class BatchPredictionResult
    {
        [JsonPropertyName("tre_em_id")]
        public int TreEmId { get; set; }

        [JsonPropertyName("cluster")]
        public int Cluster { get; set; }

        [JsonPropertyName("priority_level")]
        public string PriorityLevel { get; set; }

        [JsonPropertyName("priority_rank")]
        public int PriorityRank { get; set; }

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }
    }

    public class BatchPredictionResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("results")]
        public List<BatchPredictionResult> Results { get; set; }
    }
}
