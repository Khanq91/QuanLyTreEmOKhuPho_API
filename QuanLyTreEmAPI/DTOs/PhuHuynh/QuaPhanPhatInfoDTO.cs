namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class QuaPhanPhatInfoDTO
    {
        public int PhanPhatID { get; set; }
        public string TenQua { get; set; } = "";
        public string MoTa { get; set; } = "";
        public int SoLuongNhan { get; set; }
        public string NgayPhanPhat { get; set; } = "";
        public string NguoiPhanPhat { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public string Anh { get; set; } = "";
        public int? SuKienID { get; set; }
        public string? TenSuKien { get; set; }
    }
}
