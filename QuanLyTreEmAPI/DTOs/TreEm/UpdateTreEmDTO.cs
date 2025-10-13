namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class UpdateTreEmDTO
    {
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string QuocTich { get; set; }
        public string TinhTrang { get; set; }
        public int? TruongID { get; set; }
        public int? KhuPhoID { get; set; }
        public List<int> HoanCanhIDs { get; set; }
    }
}
