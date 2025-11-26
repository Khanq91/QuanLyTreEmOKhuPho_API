namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class TinhNguyenVienProfileDTO
    {
        public int UserId { get; set; }
        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string VaiTro { get; set; }
        public string Anh { get; set; }
        public DateOnly? NgayTao { get; set; }
        public int TinhNguyenVienID { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string ChucVu { get; set; }
        public string TenKhuPho { get; set; }
        public string DiaChiKhuPho { get; set; }
        public string QuanHuyen { get; set; }
        public string ThanhPho { get; set; }
    }
}
