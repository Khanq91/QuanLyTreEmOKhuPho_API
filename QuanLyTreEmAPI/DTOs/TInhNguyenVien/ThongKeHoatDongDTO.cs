namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class ThongKeHoatDongDTO
    {
        public int TongSuKienThamGia { get; set; }
        public List<SuKienDaThamGiaDTO> SuKienGanDay { get; set; } // 3 sự kiện gần nhất
    }
}
