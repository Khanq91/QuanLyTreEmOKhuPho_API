namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class DanhSachTreEmResponseDTO
    {
        public List<TreEmCanVanDongDTO> TreCanVanDong { get; set; } = new();
        public List<TreEmPhanPhatQuaDTO> TrePhanPhatQua { get; set; } = new();
    }
}
