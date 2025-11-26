namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class ChiTietTreEmHoTroDTO
    {
        // Thông tin trẻ
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string? Anh { get; set; }

        // Thông tin hỗ trợ
        public int HoTroID { get; set; }
        public string LoaiHoTro { get; set; }
        public string MoTaHoTro { get; set; }
        public string TrangThaiPhat { get; set; }
        public DateOnly? NgayHenLai { get; set; }

        // Thông tin phụ huynh
        public List<ThongTinPhuHuynhDTO> DanhSachPhuHuynh { get; set; }

        // Lịch sử phát hỗ trợ
        public List<LichSuHoTroDTO> LichSuPhatHoTro { get; set; }
    }
}
