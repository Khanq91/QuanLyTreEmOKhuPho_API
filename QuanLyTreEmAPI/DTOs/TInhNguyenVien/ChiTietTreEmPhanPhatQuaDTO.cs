namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class ChiTietTreEmPhanPhatQuaDTO
    {
        // Thông tin trẻ em
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string? Anh { get; set; }

        // Danh sách phụ huynh
        public List<ThongTinPhuHuynhDTO> DanhSachPhuHuynh { get; set; } = new();

        // Thông tin phân phát hiện tại
        public int PhanPhatID { get; set; }
        public int QuaTangUngHoID { get; set; }
        public string TenQua { get; set; }
        public string MoTaQua { get; set; }
        public string? AnhQua { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuongNhan { get; set; }
        public DateOnly NgayPhanPhat { get; set; }
        public string NguoiPhanPhat { get; set; }
        public string TrangThai { get; set; }
        public string? GhiChu { get; set; }

        // Thông tin sự kiện
        public int SuKienID { get; set; }
        public string TenSuKien { get; set; }
        public DateOnly NgayBatDau { get; set; }
        public DateOnly NgayKetThuc { get; set; }
        public string DiaDiem { get; set; }

        // Thông tin quà tặng
        public int SoLuongTong { get; set; }
        public int SoLuongConLai { get; set; }
        public string DoiTuongNhan { get; set; }

        // Lịch sử phân phát quà cho trẻ này
        public List<LichSuPhanPhatQuaDTO> LichSuPhanPhatQua { get; set; } = new();
    }
}
