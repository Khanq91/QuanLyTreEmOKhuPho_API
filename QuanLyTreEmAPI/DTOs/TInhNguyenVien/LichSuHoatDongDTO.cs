namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class LichSuHoatDongDTO
    {
        public List<SuKienTNVDaThamGiaDTO> SuKienDaThamGia { get; set; }
        public List<HoTroPhucLoiDaPhatDTO> HoTroPhucLoiDaPhat { get; set; }
        public List<TreEmDaVanDongDTO> TreEmDaVanDong { get; set; }
        public int TotalSuKien { get; set; }
        public int TotalHoTro { get; set; }
        public int TotalVanDong { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
