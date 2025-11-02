using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.DTOs.AI;
public class AIPriorityService : IAIPriorityService
{
    private readonly QuanLyTreEmContext _context;

    // Trọng số
    private const int TRONG_SO_HOAN_CANH = 40;
    private const int TRONG_SO_HOC_TAP = 20;
    private const int TRONG_SO_TINH_TRANG = 25;
    private const int TRONG_SO_THOI_GIAN = 15;

    public AIPriorityService(QuanLyTreEmContext context)
    {
        _context = context;
    }

    public async Task<List<PhanLoaiUuTienResponse>> PhanLoaiTatCa()
    {
        var danhSachTreEm = await LayDuLieuTreEm();
        var ketQua = new List<PhanLoaiUuTienResponse>();

        foreach (var treEm in danhSachTreEm)
        {
            ketQua.Add(PhanLoaiUuTien(treEm));
        }

        return ketQua.OrderByDescending(x => x.DiemUuTien).ToList();
    }

    public async Task<Top5Response> LayTop5UuTien()
    {
        var tatCa = await PhanLoaiTatCa();
        var top5 = tatCa.Take(5).ToList();

        return new Top5Response
        {
            TotalCount = tatCa.Count,
            Top5UuTien = top5,
            NgayPhanLoai = DateTime.Now,
            ThongKe = new ThongKeTongQuat
            {
                TongSoTreEm = tatCa.Count,
                SoKhanCap = tatCa.Count(x => x.MucDoUuTien == "Khẩn cấp"),
                SoCao = tatCa.Count(x => x.MucDoUuTien == "Cao"),
                SoTrungBinh = tatCa.Count(x => x.MucDoUuTien == "Trung bình"),
                SoThap = tatCa.Count(x => x.MucDoUuTien == "Thấp")
            }
        };
    }

    public async Task<PhanLoaiUuTienResponse> PhanLoaiMotTreEm(int treEmId)
    {
        var danhSach = await LayDuLieuTreEm();
        var treEm = danhSach.FirstOrDefault(x => x.TreEmID == treEmId);

        if (treEm == null)
            return null;

        return PhanLoaiUuTien(treEm);
    }

    public async Task<List<PhanLoaiUuTienResponse>> PhanLoaiTheoMucDo(string mucDo)
    {
        var tatCa = await PhanLoaiTatCa();
        return tatCa.Where(x => x.MucDoUuTien == mucDo).ToList();
    }

    // ===== PRIVATE METHODS =====

    private async Task<List<TreEmAIModel>> LayDuLieuTreEm()
    {
        var queryRaw = await (from te in _context.TreEms
                              select new
                              {
                                  TreEm = te,
                                  HoTroGanNhat = _context.HoTroPhucLois
                                      .Where(ht => ht.TreEmId == te.TreEmId)
                                      .OrderByDescending(ht => ht.NgayCap)
                                      .Select(ht => ht.NgayCap)
                                      .FirstOrDefault()
                              }).ToListAsync();

        var query = queryRaw.Select(x => new TreEmAIModel
        {
            TreEmID = x.TreEm.TreEmId,
            HoTen = x.TreEm.HoTen,
            NgaySinh = x.TreEm.NgaySinh,
            GioiTinh = x.TreEm.GioiTinh,
            TinhTrang = x.TreEm.TinhTrang,
            TenKhuPho = x.TreEm.KhuPho.TenKhuPho,
            TenTruong = x.TreEm.Truong.TenTruong,

            DanhSachHoanCanh = _context.TreEmHoanCanhs
                .Where(tehc => tehc.TreEmId == x.TreEm.TreEmId)
                .Join(_context.HoanCanhs,
                      tehc => tehc.HoanCanhId,
                      hc => hc.HoanCanhId,
                      (tehc, hc) => hc.LoaiHoanCanh)
                .ToList(),

            DiemTrungBinh = (float?)_context.PhieuHocTaps
                .Where(pht => pht.TreEmId == x.TreEm.TreEmId)
                .OrderByDescending(pht => pht.NgayCapNhat)
                .Select(pht => pht.DiemTrungBinh)
                .FirstOrDefault(),

            XepLoai = _context.PhieuHocTaps
                .Where(pht => pht.TreEmId == x.TreEm.TreEmId)
                .OrderByDescending(pht => pht.NgayCapNhat)
                .Select(pht => pht.XepLoai)
                .FirstOrDefault(),

            HanhKiem = _context.PhieuHocTaps
                .Where(pht => pht.TreEmId == x.TreEm.TreEmId)
                .OrderByDescending(pht => pht.NgayCapNhat)
                .Select(pht => pht.HanhKiem)
                .FirstOrDefault(),

            SoLanHoTro = _context.HoTroPhucLois.Count(ht => ht.TreEmId == x.TreEm.TreEmId),

            // Chuyển đổi DateOnly -> DateTime ở đây, trong C#
            NgayHoTroGanNhat = x.HoTroGanNhat.HasValue
                ? x.HoTroGanNhat.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null
        }).ToList();

        return query;
    }


    private PhanLoaiUuTienResponse PhanLoaiUuTien(TreEmAIModel treEm)
    {
        var ketQua = new PhanLoaiUuTienResponse
        {
            TreEmID = treEm.TreEmID,
            HoTen = treEm.HoTen,
            Tuoi = treEm.Tuoi,
            TinhTrang = treEm.TinhTrang,
            TenKhuPho = treEm.TenKhuPho,
            TenTruong = treEm.TenTruong,
            HoanCanh = treEm.DanhSachHoanCanh,
            DiemTrungBinh = treEm.DiemTrungBinh,
            XepLoai = treEm.XepLoai,
            SoLanHoTro = treEm.SoLanHoTro,
            NgayHoTroGanNhat = treEm.NgayHoTroGanNhat,
            ChiTietDiem = new ChiTietDiem()
        };

        // 1. Tính điểm hoàn cảnh
        int diemHoanCanh = TinhDiemHoanCanh(treEm, ketQua);
        ketQua.ChiTietDiem.DiemHoanCanh = diemHoanCanh;

        // 2. Tính điểm học tập
        int diemHocTap = TinhDiemHocTap(treEm, ketQua);
        ketQua.ChiTietDiem.DiemHocTap = diemHocTap;

        // 3. Tính điểm tình trạng
        int diemTinhTrang = TinhDiemTinhTrang(treEm, ketQua);
        ketQua.ChiTietDiem.DiemTinhTrang = diemTinhTrang;

        // 4. Tính điểm thời gian
        int diemThoiGian = TinhDiemThoiGian(treEm, ketQua);
        ketQua.ChiTietDiem.DiemThoiGian = diemThoiGian;

        // Tổng điểm
        ketQua.DiemUuTien = diemHoanCanh + diemHocTap + diemTinhTrang + diemThoiGian;
        ketQua.ChiTietDiem.TongDiem = ketQua.DiemUuTien;

        // Xác định mức độ
        ketQua.MucDoUuTien = XacDinhMucDoUuTien(ketQua.DiemUuTien);

        // Tạo đề xuất
        TaoDeXuat(ketQua, treEm);

        return ketQua;
    }

    private int TinhDiemHoanCanh(TreEmAIModel treEm, PhanLoaiUuTienResponse ketQua)
    {
        int diem = 0;

        foreach (var hoanCanh in treEm.DanhSachHoanCanh)
        {
            switch (hoanCanh.ToLower().Trim())
            {
                case "mồ côi cha":
                case "mồ côi mẹ":
                    diem += 15;
                    ketQua.LyDo.Add($"Mồ côi - cần hỗ trợ tâm lý và vật chất");
                    break;
                case "mồ côi cả cha và mẹ":
                case "mồ côi":
                    diem += 25;
                    ketQua.LyDo.Add($"Mồ côi cả cha mẹ - CẦN HỖ TRỢ KHẨN CẤP");
                    break;
                case "hộ nghèo":
                    diem += 12;
                    ketQua.LyDo.Add($"Gia đình hộ nghèo");
                    break;
                case "khuyết tật":
                    diem += 18;
                    ketQua.LyDo.Add($"Khuyết tật - cần hỗ trợ đặc biệt");
                    break;
                case "cha mẹ ly hôn":
                    diem += 8;
                    ketQua.LyDo.Add($"Gia đình đổ vỡ");
                    break;
                case "cha/mẹ bệnh nặng":
                    diem += 14;
                    ketQua.LyDo.Add($"Phụ huynh bệnh nặng");
                    break;
            }
        }

        return Math.Min(diem, TRONG_SO_HOAN_CANH);
    }

    private int TinhDiemHocTap(TreEmAIModel treEm, PhanLoaiUuTienResponse ketQua)
    {
        int diem = 0;

        if (treEm.DiemTrungBinh.HasValue)
        {
            if (treEm.DiemTrungBinh < 5.0)
            {
                diem += 12;
                ketQua.LyDo.Add($"Học lực yếu (ĐTB: {treEm.DiemTrungBinh:F1}) - cần hỗ trợ học tập");
            }
            else if (treEm.DiemTrungBinh < 6.5)
            {
                diem += 8;
                ketQua.LyDo.Add($"Học lực trung bình (ĐTB: {treEm.DiemTrungBinh:F1})");
            }
            else if (treEm.DiemTrungBinh < 8.0)
            {
                diem += 4;
            }
        }

        if (!string.IsNullOrEmpty(treEm.HanhKiem))
        {
            switch (treEm.HanhKiem.ToLower())
            {
                case "yếu":
                    diem += 8;
                    ketQua.LyDo.Add($"Hạnh kiểm yếu - cần tư vấn");
                    break;
                case "trung bình":
                    diem += 5;
                    break;
                case "khá":
                    diem += 2;
                    break;
            }
        }

        return Math.Min(diem, TRONG_SO_HOC_TAP);
    }

    private int TinhDiemTinhTrang(TreEmAIModel treEm, PhanLoaiUuTienResponse ketQua)
    {
        int diem = 0;

        if (!string.IsNullOrEmpty(treEm.TinhTrang))
        {
            switch (treEm.TinhTrang.ToLower())
            {
                case "khó khăn":
                    diem = 20;
                    ketQua.LyDo.Add($"Tình trạng khó khăn");
                    break;
                case "mồ côi":
                    diem = 25;
                    ketQua.LyDo.Add($"Trẻ mồ côi");
                    break;
                case "nguy cơ bỏ học":
                    diem = 22;
                    ketQua.LyDo.Add($"⚠️ CÓ NGUY CƠ BỎ HỌC");
                    break;
                case "bình thường":
                    diem = 5;
                    break;
            }
        }

        return diem;
    }

    private int TinhDiemThoiGian(TreEmAIModel treEm, PhanLoaiUuTienResponse ketQua)
    {
        int diem = 0;

        // Độ tuổi
        if (treEm.Tuoi <= 6)
        {
            diem += 8;
            ketQua.LyDo.Add($"Độ tuổi nhỏ ({treEm.Tuoi} tuổi) - giai đoạn phát triển quan trọng");
        }
        else if (treEm.Tuoi <= 10)
        {
            diem += 5;
        }
        else if (treEm.Tuoi <= 14)
        {
            diem += 3;
        }

        // Thời gian chưa hỗ trợ
        if (treEm.NgayHoTroGanNhat.HasValue)
        {
            var thangChuaHoTro = treEm.SoThangChuaHoTro ?? 0;
            if (thangChuaHoTro >= 12)
            {
                diem += 7;
                ketQua.LyDo.Add($"Đã {thangChuaHoTro} tháng chưa được hỗ trợ");
            }
            else if (thangChuaHoTro >= 6)
            {
                diem += 5;
                ketQua.LyDo.Add($"Đã {thangChuaHoTro} tháng chưa được hỗ trợ");
            }
            else if (thangChuaHoTro >= 3)
            {
                diem += 3;
            }
        }
        else if (treEm.SoLanHoTro == 0)
        {
            diem += 7;
            ketQua.LyDo.Add($"⚠️ CHƯA TỪNG ĐƯỢC HỖ TRỢ");
        }

        return Math.Min(diem, TRONG_SO_THOI_GIAN);
    }

    private string XacDinhMucDoUuTien(int diem)
    {
        if (diem >= 75) return "Khẩn cấp";
        if (diem >= 55) return "Cao";
        if (diem >= 35) return "Trung bình";
        return "Thấp";
    }

    private void TaoDeXuat(PhanLoaiUuTienResponse ketQua, TreEmAIModel treEm)
    {
        if (ketQua.ChiTietDiem.DiemHoanCanh >= 15)
        {
            ketQua.DeXuat.Add("💰 Hỗ trợ tài chính khẩn cấp");
            ketQua.DeXuat.Add("🧠 Tư vấn tâm lý định kỳ");
        }

        if (ketQua.ChiTietDiem.DiemHocTap >= 10)
        {
            ketQua.DeXuat.Add("📚 Hỗ trợ học tập (gia sư, sách vở)");
            ketQua.DeXuat.Add("🎓 Tham gia lớp học bổ trợ");
        }

        if (ketQua.ChiTietDiem.DiemTinhTrang >= 20)
        {
            ketQua.DeXuat.Add("👀 Theo dõi sát sao tình trạng");
            ketQua.DeXuat.Add("🎁 Tăng cường hỗ trợ vật chất");
        }

        if (treEm.SoLanHoTro == 0)
        {
            ketQua.DeXuat.Add("⭐ Ưu tiên trong đợt hỗ trợ tiếp theo");
        }

        if (ketQua.MucDoUuTien == "Khẩn cấp")
        {
            ketQua.DeXuat.Add("🚨 CẦN HỖ TRỢ NGAY LẬP TỨC");
            ketQua.DeXuat.Add("📋 Lập kế hoạch hỗ trợ dài hạn");
        }
    }
}
