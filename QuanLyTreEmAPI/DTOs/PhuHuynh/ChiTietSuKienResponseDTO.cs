namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class ChiTietSuKienResponseDTO
    {
        public int SuKienId { get; set; }
        public string TenSuKien { get; set; }
        public string MoTa { get; set; }
        public string DiaDiem { get; set; }
        public string NgayBatDau { get; set; } // dd/MM/yyyy
        public string NgayKetThuc { get; set; } // dd/MM/yyyy
        public int SoLuongTinhNguyenVien { get; set; }
        public int SoLuongTreEm { get; set; }
        public string NguoiChiuTrachNhiem { get; set; }
        public string TenKhuPho { get; set; }
        public string DiaChiKhuPho { get; set; }
        public bool DaDangKy { get; set; }
        public string TrangThaiDangKy { get; set; } // Chờ duyệt, Đã duyệt, Từ chối
        public List<ChuongTrinhSuKienDTO> DanhSachChuongTrinh { get; set; }
    }
}
