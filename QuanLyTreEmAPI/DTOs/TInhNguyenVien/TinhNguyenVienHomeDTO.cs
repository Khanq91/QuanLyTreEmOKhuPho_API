namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class TinhNguyenVienHomeDTO
    {
        public ThongTinTaiKhoanDTO ThongTinTaiKhoan { get; set; }
        public List<SuKienPhanCongDTO> SuKienPhanCong { get; set; }
        public ThongKeHoatDongDTO ThongKe { get; set; }
        public LichTrongDTO LichTrong { get; set; }
    }
}
