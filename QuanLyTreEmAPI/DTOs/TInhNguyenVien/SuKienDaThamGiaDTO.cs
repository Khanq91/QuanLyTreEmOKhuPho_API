namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class SuKienDaThamGiaDTO
    {
        public int? SuKienId { get; set; }
        public string TenSuKien { get; set; }
        public string NguoiChiuTrachNhiem { get; set; }
        public string MoTa { get; set; }
        public string DiaDiem { get; set; }
        public DateOnly NgayBatDau { get; set; }
        public DateOnly NgayKetThuc { get; set; }
    }
}
