using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyTreEmOKhuPho.Models.HoTroVaUngHo
{
    public class ChiTietUngHoDTO
    {
        public int UngHoID { get; set; }
        public string GhiChu { get; set; }
        public DateTime NgayUngHo { get; set; }
        public string TenManhThuongQuan { get; set; }
        public string LoaiUngHo { get; set; }
        public decimal SoTien { get; set; }
        public string DoiTuong { get; set; }
        public string TenKhuPho { get; set; }
        public string DiaChiKhuPho { get; set; }
        public string QuanHuyen { get; set; }
        public string ThanhPho { get; set; }
        public int SoLuongTreEmDuocUngHo { get; set; }
        public int TongTreEm { get; set; }
        public int TreDaNhan { get; set; }
        public int TreChuaNhan { get; set; }
        public int PercentDaPhat { get; set; }
        public List<QuaTangDTO> DanhSachQuaTang { get; set; }
        public List<MinhChungDTO> DanhSachMinhChung { get; set; }
    }
    public class QuaTangDTO
    {
        public int QuaTangUngHoID { get; set; }
        public string TenQua { get; set; }
        public string MoTa { get; set; }
        public int SoLuongTong { get; set; }
        public int SoLuongConLai { get; set; }
        public decimal DonGia { get; set; }
        public string DoiTuongNhan { get; set; }
        public string Anh { get; set; }
        public List<TreNhanQuaDTO> DanhSachTreNhan { get; set; }
    }
    public class TreNhanQuaDTO
    {
        public int TreEmID { get; set; }
        public int HoTroId { get; set; }
        public int PhanPhatId { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string TenKhuPho { get; set; }
        public string TinhTrang { get; set; }
        public int SoLuongNhan { get; set; }
        public DateTime NgayPhanPhat { get; set; }
        public string NguoiPhanPhat { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
    }
    public class MinhChungDTO
    {
        public int MinhChungID { get; set; }
        public string LoaiMinhChung { get; set; }
        public string FilePath { get; set; }
        public DateTime? NgayCap { get; set; }
    }

}