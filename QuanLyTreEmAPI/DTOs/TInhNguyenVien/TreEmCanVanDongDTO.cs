namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class TreEmCanVanDongDTO
    {
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; } // Từ phụ huynh
        public string TinhTrang { get; set; } // Hiển thị nổi bật
        public int? SoLanVanDong { get; set; } // Hiển thị nổi bật
        public string? Anh { get; set; }
    }
}
