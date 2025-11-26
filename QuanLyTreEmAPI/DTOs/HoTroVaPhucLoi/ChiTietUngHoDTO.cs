using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyTreEmOKhuPho.Models.HoTroVaUngHo
{
    public class QuaTangDTO
    {
        public int QuaTangUngHoID { get; set; }
        public int? SoLuongTreEm { get; set; }
        public decimal PhanTrangHoanThanh { get; set; }
        public int SoLuongTreEmChuaNhanQua { get; set; }
        public string TenQua { get; set; }
        public string MoTa { get; set; }
        public int SoLuongTreEmDaNhan { get; set; }
        public int SoLuongTong { get; set; }
        public int SoLuongConLai { get; set; }
        public string DoiTuongNhan { get; set; }
        public string TenSuKien { get; set; }
        public string TenManhThuongQuan { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }
        public string LoaiUngHo { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgayUngHo { get; set; }
        public List<TreNhanQuaDTO> DanhSachTreNhan { get; set; }
    }

    public class TreNhanQuaDTO
    {
        public int TreEmID { get; set; }
        public int PhanPhatId { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string TinhTrang { get; set; }
        public int SoLuongNhan { get; set; }

        public DateTime? NgayPhanPhat { get; set; }
        public string NguoiPhanPhat { get; set; }
        public string TrangThai { get; set; }
    }
    //public class MinhChungDTO
    //{
    //    public int MinhChungID { get; set; }
    //    public string LoaiMinhChung { get; set; }
    //    public string FilePath { get; set; }
    //    public DateTime? NgayCap { get; set; }
    //}

}