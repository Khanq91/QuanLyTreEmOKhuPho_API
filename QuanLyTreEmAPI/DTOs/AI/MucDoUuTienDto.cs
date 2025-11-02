namespace QuanLyTreEmAPI.DTOs.AI
{
    public class MucDoUuTienDto
    {
        public int TreEmId { get; set; }
        public string? HoTen { get; set; }
        public int? Tuoi { get; set; }
        public string? GioiTinh { get; set; }
        public string? KhuPho { get; set; }
        public string? TruongHoc { get; set; }
        public string MucDoUuTien { get; set; } = null!; 
        public int DiemUuTien { get; set; }
        public string? LyDoChinh { get; set; }
        public List<string> ChiTietLyDo { get; set; } = new();
        public List<string> DeXuatHoTro { get; set; } = new();
    }
    public class KetQuaPhanTichDto
    {
        public int TongSoTreEm { get; set; }
        public int SoTreEmUuTienCao { get; set; }
        public int SoTreEmUuTienTrungBinh { get; set; }
        public int SoTreEmUuTienThap { get; set; }
        public DateTime ThoiGianPhanTich { get; set; }
        public List<MucDoUuTienDto> DanhSachTreEm { get; set; } = new();
    }

}
