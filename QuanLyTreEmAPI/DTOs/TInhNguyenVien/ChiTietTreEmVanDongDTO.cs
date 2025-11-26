namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class ChiTietTreEmVanDongDTO
    {
        // Thông tin trẻ
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string? Anh { get; set; }

        // Thông tin phụ huynh
        public List<ThongTinPhuHuynhDTO> DanhSachPhuHuynh { get; set; }

        // Thông tin hoàn cảnh
        public int HoanCanhID { get; set; }
        public string LoaiHoanCanh { get; set; }
        public string MoTaHoanCanh { get; set; }

        // Lịch sử vận động
        public List<LichSuVanDongDTO> LichSuVanDong { get; set; }

        // Thông tin vận động mới nhất
        public string? TinhTrangHienTai { get; set; }
        public int? TongSoLanVanDong { get; set; }
    }
}
