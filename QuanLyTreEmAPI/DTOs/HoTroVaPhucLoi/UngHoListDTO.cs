namespace QuanLyTreEmAPI.DTOs.HoTroVaPhucLoi
{
    public class UngHoListDTO
    {
        public int UngHoID { get; set; }
        public string GhiChu { get; set; }

        public DateTime NgayUngHo { get; set; }
        public string NgayUngHoDisplay { get; set; }
        public string TenManhThuongQuan { get; set; }

        public string LoaiUngHo { get; set; }
        public string TenVatPham { get; set; }
        public int SoLuongVatPham { get; set; }
        public int SoLuongConLai { get; set; }
        public string DoiTuong { get; set; }
        public decimal SoTien { get; set; }
        public string TenKhuPho { get; set; }
        public string DiaChiKhuPho { get; set; }
        public string QuanHuyen { get; set; }
        public string ThanhPho { get; set; }
    }
}
