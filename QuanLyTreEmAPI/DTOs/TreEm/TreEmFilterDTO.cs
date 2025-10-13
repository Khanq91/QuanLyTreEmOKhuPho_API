namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class TreEmFilterDTO
    {
        public int? KhuPhoID { get; set; }
        public int? TuoiMin { get; set; }
        public int? TuoiMax { get; set; }
        public string GioiTinh { get; set; }
        public int? HoanCanhID { get; set; }
        public string MucUuTien { get; set; }
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "HoTen";
        public string SortOrder { get; set; } = "asc";
    }
}
