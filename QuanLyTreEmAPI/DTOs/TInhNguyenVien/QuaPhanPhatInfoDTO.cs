namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class QuaPhanPhatInfoDTO
    {
        public int PhanPhatID { get; set; }
        public string TenQua { get; set; }
        public string MoTa { get; set; }
        public int SoLuongNhan { get; set; }
        public DateTime NgayPhanPhat { get; set; }
        public string NguoiPhanPhat { get; set; }
        public string TrangThai { get; set; }
        public string Anh { get; set; }
        public int? SuKienID { get; set; }
        public string? TenSuKien { get; set; }
        public string TenTreEm { get; set; }
        public int TreEmId { get; set; }

    }
}
