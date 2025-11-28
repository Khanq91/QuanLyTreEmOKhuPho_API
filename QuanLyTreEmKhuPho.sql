-- ==============================
-- TẠO CƠ SỞ DỮ LIỆU HOÀN CHỈNH
-- ==============================
CREATE DATABASE QuanLyTreEm;
GO
USE QuanLyTreEm;
GO

-- ============================== 
-- TẠO CÁC BẢNG
-- ==============================

-- Bảng khu phố
CREATE TABLE KhuPho (
    KhuPhoID INT PRIMARY KEY IDENTITY(1,1),
    TenKhuPho NVARCHAR(100),
    DiaChi NVARCHAR(200),
    QuanHuyen NVARCHAR(100),
    ThanhPho NVARCHAR(100)
);

-- Bảng Người dùng
CREATE TABLE NguoiDung (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100),
    SDT NVARCHAR(12) UNIQUE,
    Email NVARCHAR(100) UNIQUE,
    MatKhau NVARCHAR(100),
    VaiTro NVARCHAR(50),
    NgayTao DATE,
    Anh VARCHAR(500),
    TrangThai NVARCHAR(50)
);

-- Bảng Mạnh thường quân
CREATE TABLE ManhThuongQuan (
    ManhThuongQuanID INT PRIMARY KEY IDENTITY(1,1),
    Ten NVARCHAR(100),
    Loai NVARCHAR(50),
    DiaChi NVARCHAR(200),
    SDT NVARCHAR(15),
    Email NVARCHAR(100),
    GhiChu NVARCHAR(300)
);

-- Bảng Ủng hộ
CREATE TABLE UngHo (
    UngHoID INT PRIMARY KEY IDENTITY(1,1),
    SoTien DECIMAL(18,2),
    LoaiUngHo NVARCHAR(100),
    DoiTuong NVARCHAR(100),
    SoLuongVatPham INT ,
	SoLuongConLai INT,
    TenVatPham NVARCHAR(200),
    NgayUngHo DATE,
    GhiChu NVARCHAR(300),
    ManhThuongQuanID INT FOREIGN KEY REFERENCES ManhThuongQuan(ManhThuongQuanID)
);

-- Bảng Tình nguyện viên
CREATE TABLE TinhNguyenVien (
    TinhNguyenVienID INT PRIMARY KEY IDENTITY(1,1),
    SDT NVARCHAR(15),
    NgaySinh DATE,
    ChucVu NVARCHAR(100),
    UserID INT FOREIGN KEY REFERENCES NguoiDung(UserID) UNIQUE,
    KhuPhoID INT FOREIGN KEY REFERENCES KhuPho(KhuPhoID)
);

-- Bảng Lịch trống
CREATE TABLE LichTrong (
    LichTrongID INT PRIMARY KEY IDENTITY(1,1),
    TinhNguyenVienID INT FOREIGN KEY REFERENCES TinhNguyenVien(TinhNguyenVienID) UNIQUE
);

-- Bảng Chi tiết lịch trống
CREATE TABLE ChiTietLichTrong (
    ChiTietLichTrongID INT PRIMARY KEY IDENTITY(1,1),
    Buoi NVARCHAR(15),
    Thu NVARCHAR(15),
    LichTrongID INT FOREIGN KEY REFERENCES LichTrong(LichTrongID)
);

-- Bảng Sự kiện
CREATE TABLE SuKien (
    SuKienID INT PRIMARY KEY IDENTITY(1,1),
    TenSuKien NVARCHAR(200),
    NguoiChiuTrachNhiem NVARCHAR(200),
    MoTa NVARCHAR(MAX),
    DiaDiem NVARCHAR(200),
    NgayBatDau DATE,
    NgayKetThuc DATE,
    SoLuongTinhNguyenVien INT,
    SoLuongTreEm INT,
    UserID INT FOREIGN KEY REFERENCES NguoiDung(UserID),
    KhuPhoID INT FOREIGN KEY REFERENCES KhuPho(KhuPhoID)
);

-- Bảng Thời gian chi tiết sự kiện
CREATE TABLE ThoiGianChiTietSuKien(
    ThoiGianChiTietSuKienID INT PRIMARY KEY IDENTITY(1,1),
    MoTa NVARCHAR(MAX),
    ThoiGianBatDau DATETIME,
    ThoiGianKetThuc DATETIME,
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID)
);

-- Bảng Phân công tình nguyện viên
CREATE TABLE PhanCongTinhNguyenVien (
    PhanCongID INT PRIMARY KEY IDENTITY(1,1),
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID),
    TinhNguyenVienID INT FOREIGN KEY REFERENCES TinhNguyenVien(TinhNguyenVienID),
    CongViec NVARCHAR(200),
    GhiChu NVARCHAR(300),
    NgayPhanCong DATE DEFAULT GETDATE()
);

-- Bảng Tiết mục sự kiện
CREATE TABLE TietMucSuKien(
    TietMucID INT PRIMARY KEY IDENTITY(1,1),
    TenTietMuc NVARCHAR(100),
    NguoiThucHien NVARCHAR(100),
    ChiPhiTietMuc DECIMAL(18,2),
    ThoiGianChiTietSuKienID INT FOREIGN KEY REFERENCES ThoiGianChiTietSuKien(ThoiGianChiTietSuKienID)
);

-- Bảng Chi phí sự kiện
CREATE TABLE ChiPhiSuKien (
    ChiPhiID INT PRIMARY KEY IDENTITY(1,1),
    TenKhoanChi NVARCHAR(200),
    SoTien DECIMAL(18,2),
    NguoiPheDuyet NVARCHAR(100),
    NgayPheDuyet DATE,
    VanBanPheDuyet NVARCHAR(255),
    GhiChu NVARCHAR(300),
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID)
);

-- Bảng Chi tiết chi phí sự kiện
CREATE TABLE ChiTietChiPhiSuKien(
    ChiTietChiPhi INT PRIMARY KEY IDENTITY(1,1),
    TenPhanQua NVARCHAR(100),
    NguoiDaiDien NVARCHAR(200),
    SoLuong INT,
    DonGia DECIMAL(18,2),
    ChiPhiID INT FOREIGN KEY REFERENCES ChiPhiSuKien(ChiPhiID)
);

-- Bảng Đăng ký sự kiện
CREATE TABLE DangKySuKien (
    DangKySuKienID INT PRIMARY KEY IDENTITY(1,1),
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID),
    UserID INT FOREIGN KEY REFERENCES NguoiDung(UserID),
    NgayDangKy DATE,
    TrangThai NVARCHAR(50)
);

-- Bảng Thông báo
CREATE TABLE ThongBao (
    ThongBaoID INT PRIMARY KEY IDENTITY(1,1),
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID),
    NoiDung NVARCHAR(MAX),
    NgayThongBao DATE DEFAULT GETDATE()
);

-- Bảng Thông báo - Người dùng
CREATE TABLE ThongBao_NguoiDung (
    ThongBaoID INT FOREIGN KEY REFERENCES ThongBao(ThongBaoID),
    UserID INT FOREIGN KEY REFERENCES NguoiDung(UserID),
    DaDoc BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (ThongBaoID, UserID)
);

-- Bảng Thông tin phụ huynh
CREATE TABLE ThongTinPhuHuynh (
    PhuHuynhID INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100),
    SDT NVARCHAR(15),
    DiaChi NVARCHAR(200),
    NgheNghiep NVARCHAR(100),
    NgaySinh DATE,
    TonGiao NVARCHAR(20),
    DanToc NVARCHAR(20),
    QuocTich NVARCHAR(30),
    UserID INT FOREIGN KEY REFERENCES NguoiDung(UserID)
);

-- Bảng Trường học
CREATE TABLE TruongHoc (
    TruongID INT PRIMARY KEY IDENTITY(1,1),
    TenTruong NVARCHAR(200),
    DiaChi NVARCHAR(200),
    CapHoc NVARCHAR(50)
);

-- Bảng Trẻ em
CREATE TABLE TreEm (
    TreEmID INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100),
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    TonGiao NVARCHAR(20),
    DanToc NVARCHAR(20),
    QuocTich NVARCHAR(30),
    Anh VARCHAR(500),
    TruongID INT FOREIGN KEY REFERENCES TruongHoc(TruongID),
    TinhTrang NVARCHAR(100),
    KhuPhoID INT FOREIGN KEY REFERENCES KhuPho(KhuPhoID)
);

-- Bảng Trẻ em - Phụ huynh
CREATE TABLE TreEm_PhuHuynh (
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    PhuHuynhID INT FOREIGN KEY REFERENCES ThongTinPhuHuynh(PhuHuynhID),
    MoiQuanHe NVARCHAR(50),
    PRIMARY KEY (TreEmID, PhuHuynhID)
);

-- Bảng Lớp học
CREATE TABLE LopHoc (
    LopID INT PRIMARY KEY IDENTITY(1,1),
    TenLop NVARCHAR(50),
    TruongID INT FOREIGN KEY REFERENCES TruongHoc(TruongID)
);

-- Bảng Phiếu học tập
CREATE TABLE PhieuHocTap (
    PhieuHocTapID INT PRIMARY KEY IDENTITY(1,1),
    DiemTrungBinh FLOAT,
    XepLoai NVARCHAR(50),
    HanhKiem NVARCHAR(25),
    GhiChu NVARCHAR(200),
    NamHoc DATE,
    TruongID INT FOREIGN KEY REFERENCES TruongHoc(TruongID),
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    LopID INT FOREIGN KEY REFERENCES LopHoc(LopID)
);

-- Bảng Hoàn cảnh
CREATE TABLE HoanCanh (
    HoanCanhID INT PRIMARY KEY IDENTITY(1,1),
    LoaiHoanCanh NVARCHAR(100),
    MoTa NVARCHAR(MAX)
);

-- Bảng Trẻ em - Hoàn cảnh
CREATE TABLE TreEm_HoanCanh (
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    HoanCanhID INT FOREIGN KEY REFERENCES HoanCanh(HoanCanhID),
    NgayCapNhat DATE,
    PRIMARY KEY (TreEmID, HoanCanhID)
);

-- Bảng Trẻ em - Sự kiện
CREATE TABLE TreEm_SuKien(
    TreEmSuKienID INT PRIMARY KEY IDENTITY(1,1),
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID),
    NgayDangKy DATE DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) DEFAULT N'Đã đăng ký',
    GhiChu NVARCHAR(500),
    CONSTRAINT UQ_TreEmSuKien UNIQUE (TreEmID, SuKienID)
);

-- Bảng Vận động trẻ em
CREATE TABLE VanDongTreEm (
    VanDongID INT PRIMARY KEY IDENTITY(1,1),
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    HoanCanhID INT FOREIGN KEY REFERENCES HoanCanh(HoanCanhID),
    NguoiDungID INT FOREIGN KEY REFERENCES NguoiDung(UserID),
    SoLan INT DEFAULT 1,
    LyDo NVARCHAR(200),
    KetQua NVARCHAR(200),
    NgayVanDong DATE DEFAULT GETDATE(),
    AnhMinhChung VARCHAR(500),
    TinhTrangCapNhat NVARCHAR(50),
    GhiChuChiTiet NVARCHAR(MAX),
    NgayCapNhat DATE DEFAULT GETDATE()
);


-- Bảng Hỗ trợ phúc lợi
--CREATE TABLE HoTroPhucLoi (
--    HoTroID INT PRIMARY KEY IDENTITY(1,1),
--    LoaiHoTro NVARCHAR(100),
--    MoTa NVARCHAR(MAX),
--    NgayCap DATE,
--    NguoiChiuTrachNhiemHoTro NVARCHAR(100),
--    TrangThaiPhat NVARCHAR(50),
--    NgayHenLai DATE NULL,
--    GhiChuTNV NVARCHAR(MAX),
--    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
--    NguoiDungID INT FOREIGN KEY REFERENCES NguoiDung(UserID)
--);
-- Bảng Phiếu minh chứng
CREATE TABLE PhieuMinhChung (
    MinhChungID INT PRIMARY KEY IDENTITY(1,1),
    LoaiMinhChung NVARCHAR(100),
    FilePath NVARCHAR(300),
    NgayCap DATE,
    UngHoID INT FOREIGN KEY REFERENCES UngHo(UngHoID)
);

-- Bảng Ủng hộ - Hỗ trợ phúc lợi
--CREATE TABLE UngHo_HoTroPhucLoi (
--    UngHoID INT FOREIGN KEY REFERENCES UngHo(UngHoID),
--    HoTroID INT FOREIGN KEY REFERENCES HoTroPhucLoi(HoTroID),
--    PRIMARY KEY (UngHoID, HoTroID)
--);
-- Bảng Phân bổ ủng hộ chi phí
CREATE TABLE PhanBoUngHoChiPhi (
    PhanBoID INT PRIMARY KEY IDENTITY(1,1),
    UngHoID INT FOREIGN KEY REFERENCES UngHo(UngHoID),
    ChiPhiID INT FOREIGN KEY REFERENCES ChiPhiSuKien(ChiPhiID),
    SoTienPhanBo DECIMAL(18,2),
    TyLe DECIMAL(5,2),
    NguoiPheDuyet NVARCHAR(100),
    NgayPheDuyet DATE DEFAULT GETDATE(),
    GhiChu NVARCHAR(500)
);

-- Bảng Quà tặng ủng hộ
CREATE TABLE QuaTangUngHo (
    QuaTangUngHoID INT PRIMARY KEY IDENTITY(1,1),
    UngHoID INT FOREIGN KEY REFERENCES UngHo(UngHoID),
    SuKienID INT NULL,
    TenQua NVARCHAR(200),
    MoTa NVARCHAR(500),
    NguoiChiuTrachNhiem NVARCHAR(500),
    LoaiHoTro NVARCHAR(100),
    SoLuongTong INT,
    SoLuongConLai INT,
    DonGia DECIMAL(18,2),
    DoiTuongNhan NVARCHAR(100),
    Anh VARCHAR(500),
	CONSTRAINT FK_QuaTangUngHo_SuKien FOREIGN KEY (SuKienID) REFERENCES SuKien(SuKienID),

);

-- Bảng Phân phát quà
CREATE TABLE PhanPhatQua (
    PhanPhatID INT PRIMARY KEY IDENTITY(1,1),
    QuaTangUngHoID INT FOREIGN KEY REFERENCES QuaTangUngHo(QuaTangUngHoID),
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    SoLuongNhan INT DEFAULT 1,
    NgayPhanPhat DATE DEFAULT GETDATE(),
    NguoiPhanPhat NVARCHAR(100),
    TrangThai NVARCHAR(50),
    GhiChu NVARCHAR(500),
    CONSTRAINT UQ_QuaTreNgay UNIQUE (QuaTangUngHoID, TreEmID, NgayPhanPhat)
);

GO

-- ==============================
-- THÊM DỮ LIỆU ĐẦY ĐỦ
-- ==============================

-- 1. KhuPho (8 khu phố)
INSERT INTO KhuPho (TenKhuPho, DiaChi, QuanHuyen, ThanhPho)
VALUES 
(N'Khu phố 1', N'123 Lê Lợi', N'Quận 1', N'Hồ Chí Minh'),
(N'Khu phố 2', N'45 Hai Bà Trưng', N'Quận 3', N'Hồ Chí Minh'),
(N'Khu phố 3', N'78 Nguyễn Huệ', N'Quận 1', N'Hồ Chí Minh'),
(N'Khu phố 4', N'12 CMT8', N'Quận 10', N'Hồ Chí Minh'),
(N'Khu phố 5', N'56 Nguyễn Thị Minh Khai', N'Quận 3', N'Hồ Chí Minh'),
(N'Khu phố 6', N'89 Trần Hưng Đạo', N'Quận 5', N'Hồ Chí Minh'),
(N'Khu phố 7', N'234 Võ Văn Tần', N'Quận 3', N'Hồ Chí Minh'),
(N'Khu phố 8', N'567 Lý Thường Kiệt', N'Quận 10', N'Hồ Chí Minh');

-- 2. NguoiDung (10 người dùng)
INSERT INTO NguoiDung (HoTen, SDT, Email, MatKhau, VaiTro, NgayTao, Anh, TrangThai)
VALUES
(N'Nguyễn Văn A', '0909123456', 'nguyenvana@gmail.com', 'matkhau123', N'Admin', '2024-01-15', '/Anh/NguoiDung/hinh1.jpg', N'Đang hoạt động'),
(N'Trần Thị B', '0909234567', 'tranthib@gmail.com', 'matkhau123', N'Tình nguyện viên', '2024-02-20', '/Anh/NguoiDung/hinh2.jpg', N'Đang hoạt động'),
(N'Lê Văn C', '0909345678', 'levanc@gmail.com', 'matkhau123', N'Phụ huynh', '2023-10-20', '/Anh/NguoiDung/hinh3.jpg', N'Đang hoạt động'),
(N'Phạm Thị D', '0909456789', 'phamthid@gmail.com', 'matkhau123', N'Cán bộ', '2023-05-10', '/Anh/NguoiDung/hinh4.jpg', N'Đang hoạt động'),
(N'Võ Văn E', '0909567890', 'vovane@gmail.com', 'matkhau123', N'Tình nguyện viên', '2024-03-12', '/Anh/NguoiDung/hinh5.jpg', N'Đang hoạt động'),
(N'Hoàng Thị F', '0909678901', 'hoangthif@gmail.com', 'matkhau123', N'Tình nguyện viên', '2024-04-05', '/Anh/NguoiDung/hinh6.jpg', N'Đang hoạt động'),
(N'Đặng Văn G', '0909789012', 'dangvang@gmail.com', 'matkhau123', N'Phụ huynh', '2023-08-15', '/Anh/NguoiDung/hinh7.jpg', N'Đang hoạt động'),
(N'Bùi Thị H', '0909890123', 'buithih@gmail.com', 'matkhau123', N'Cán bộ', '2024-01-20', '/Anh/NguoiDung/hinh8.jpg', N'Đang hoạt động'),
(N'Đinh Văn I', '0909901234', 'dinhvani@gmail.com', 'matkhau123', N'Tình nguyện viên', '2024-05-18', '/Anh/NguoiDung/hinh9.jpg', N'Đang hoạt động'),
(N'Dương Thị K', '0909012345', 'duongthik@gmail.com', 'matkhau123', N'Phụ huynh', '2023-11-25', '/Anh/NguoiDung/hinh10.jpg', N'Đang hoạt động');

-- 3. ManhThuongQuan (8 mạnh thường quân)
INSERT INTO ManhThuongQuan (Ten, Loai, DiaChi, SDT, Email, GhiChu)
VALUES
(N'Nguyễn Mạnh Hùng', N'Cá nhân', N'123 Nguyễn Văn Cừ, Q1, TP.HCM', '0911111111', 'hungnguyen@gmail.com', N'Thường xuyên ủng hộ học sinh nghèo vượt khó'),
(N'Công ty TNHH ABC', N'Tổ chức', N'456 Lê Duẩn, Q3, TP.HCM', '0922222222', 'lienhe@abc.com', N'Tài trợ quỹ thiếu nhi hàng năm 100 triệu'),
(N'Nguyễn Thị Lan', N'Cá nhân', N'789 Điện Biên Phủ, Q5, TP.HCM', '0933333333', 'lannguyenthi@gmail.com', N'Tài trợ quần áo đồng phục cho trẻ em khó khăn'),
(N'Công ty Cổ phần XYZ', N'Tổ chức', N'234 Ba Tháng Hai, Q10, TP.HCM', '0944444444', 'contact@xyz.com.vn', N'Ủng hộ tiền mặt và quà tặng cho các sự kiện'),
(N'Trần Văn Tâm', N'Cá nhân', N'567 Nguyễn Thị Minh Khai, Q7, TP.HCM', '0955555555', 'tamtran@gmail.com', N'Ủng hộ học bổng cho học sinh giỏi nghèo'),
(N'Quỹ Thiện Tâm', N'Tổ chức', N'890 Trần Hưng Đạo, Q1, TP.HCM', '0966666666', 'info@thientam.org', N'Hỗ trợ toàn diện cho trẻ em mồ côi'),
(N'Lê Minh Tuấn', N'Cá nhân', N'345 Võ Văn Tần, Q3, TP.HCM', '0977777777', 'tuanle@gmail.com', N'Tài trợ thiết bị học tập và đồ dùng học tập'),
(N'Tập đoàn Giáo dục DEF', N'Tổ chức', N'678 Lý Thường Kiệt, Q10, TP.HCM', '0988888888', 'support@def.edu.vn', N'Tài trợ học bổng và sách vở cho trẻ em');

-- 4. UngHo (10 đợt ủng hộ)
INSERT INTO UngHo (SoTien, LoaiUngHo, DoiTuong, SoLuongVatPham,SoLuongConLai, TenVatPham, NgayUngHo, GhiChu, ManhThuongQuanID)
VALUES
(5000000, N'Tiền mặt', N'Tất cả trẻ em khó khăn', 10,5, N'Tiền mặt', '2025-01-15', N'Ủng hộ học phí học kỳ 1', 1),
(2000000, N'Hiện vật', N'Trẻ em nghèo vượt khó', 50,10, N'Bộ sách giáo khoa lớp 1-5', '2025-02-20', N'Ủng hộ sách vở đầu năm học', 2),
(3000000, N'Quà tặng', N'Trẻ em mồ côi', 100,15, N'Bánh kẹo trung thu cao cấp', '2025-08-10', N'Quà trung thu cho các em', 3),
(10000000, N'Tiền mặt', N'Tất cả trẻ em tham gia',10,25, N'Tiền mặt', '2025-04-20', N'Tài trợ sự kiện hè 2025', 4),
(1500000, N'Hiện vật', N'Trẻ em học giỏi', 30,20, N'Bộ quần áo đồng phục', '2025-05-15', N'Ủng hộ quần áo đồng phục mới', 5),
(8000000, N'Tiền mặt', N'Trẻ em có hoàn cảnh khó khăn', 10,0, N'Tiền mặt', '2025-06-10', N'Hỗ trợ học bổng năm học mới', 1),
(4500000, N'Quà tặng', N'Trẻ em mồ côi và khuyết tật', 80,0, N'Đồ chơi giáo dục', '2025-07-15', N'Quà tặng tết thiếu nhi', 6),
(6000000, N'Tiền mặt', N'Trẻ em toàn khu vực', 10,0, N'Tiền mặt', '2025-03-25', N'Tài trợ chi phí tổ chức các hoạt động', 7),
(3500000, N'Hiện vật', N'Trẻ em cần hỗ trợ', 60,0, N'Đồ dùng học tập', '2025-09-05', N'Bộ đồ dùng học tập đầy đủ', 8),
(7000000, N'Quà tặng', N'Tất cả trẻ em', 120,0, N'Phần quà tết thiếu nhi', '2025-12-20', N'Quà tặng dịp cuối năm', 2);

-- 5. TinhNguyenVien (8 tình nguyện viên)
INSERT INTO TinhNguyenVien (SDT, NgaySinh, ChucVu, UserID, KhuPhoID)
VALUES
('0909234567', '2000-01-15', N'Trưởng nhóm', 2, 1),
('0909567890', '1998-05-22', N'Tình nguyện viên', 5, 2),
('0909678901', '2001-03-10', N'Phó nhóm', 6, 3),
('0909789012', '1999-07-18', N'Tình nguyện viên', 9, 4),
('0909456789', '1997-11-25', N'Trưởng ban tổ chức', 4, 5),
('0909890123', '2002-02-14', N'Tình nguyện viên', 8, 6),
('0909123456', '1996-09-08', N'Phó ban chuyên môn', 1, 7),
('0909345678', '2000-06-30', N'Tình nguyện viên', 3, 8);

-- 6. LichTrong (8 lịch trống)
INSERT INTO LichTrong (TinhNguyenVienID)
VALUES (1), (2), (3), (4), (5), (6), (7), (8);

-- 7. ChiTietLichTrong (40 chi tiết lịch - mỗi TNV 5 buổi)
INSERT INTO ChiTietLichTrong (Buoi, Thu, LichTrongID)
VALUES
(N'Sáng', N'Thứ 2', 1), (N'Chiều', N'Thứ 3', 1), (N'Sáng', N'Thứ 5', 1), (N'Chiều', N'Thứ 6', 1), (N'Sáng', N'Thứ 7', 1),
(N'Chiều', N'Thứ 2', 2), (N'Sáng', N'Thứ 4', 2), (N'Chiều', N'Thứ 5', 2), (N'Sáng', N'Thứ 6', 2), (N'Chiều', N'Thứ 7', 2),
(N'Sáng', N'Thứ 2', 3), (N'Sáng', N'Thứ 3', 3), (N'Chiều', N'Thứ 4', 3), (N'Sáng', N'Thứ 6', 3), (N'Chiều', N'Chủ nhật', 3),
(N'Chiều', N'Thứ 2', 4), (N'Chiều', N'Thứ 3', 4), (N'Sáng', N'Thứ 4', 4), (N'Chiều', N'Thứ 6', 4), (N'Sáng', N'Chủ nhật', 4),
(N'Sáng', N'Thứ 3', 5), (N'Chiều', N'Thứ 4', 5), (N'Sáng', N'Thứ 5', 5), (N'Chiều', N'Thứ 7', 5), (N'Sáng', N'Chủ nhật', 5),
(N'Chiều', N'Thứ 2', 6), (N'Sáng', N'Thứ 3', 6), (N'Chiều', N'Thứ 5', 6), (N'Sáng', N'Thứ 7', 6), (N'Chiều', N'Chủ nhật', 6),
(N'Sáng', N'Thứ 2', 7), (N'Chiều', N'Thứ 3', 7), (N'Sáng', N'Thứ 4', 7), (N'Chiều', N'Thứ 5', 7), (N'Sáng', N'Thứ 6', 7),
(N'Chiều', N'Thứ 3', 8), (N'Sáng', N'Thứ 4', 8), (N'Chiều', N'Thứ 6', 8), (N'Sáng', N'Thứ 7', 8), (N'Chiều', N'Chủ nhật', 8);

-- 8. SuKien (8 sự kiện)
INSERT INTO SuKien (TenSuKien, NguoiChiuTrachNhiem, MoTa, DiaDiem, NgayBatDau, NgayKetThuc, SoLuongTinhNguyenVien, SoLuongTreEm, UserID, KhuPhoID)
VALUES
(N'Sự kiện trung thu 2025', N'Nguyễn Văn A', N'Tổ chức phát quà trung thu cho trẻ em khó khăn trong khu vực', N'Nhà văn hóa Khu phố 1', '2025-09-10', '2025-09-11', 15, 120, 1, 1),
(N'Hội trại hè thiếu nhi', N'Trần Thị B', N'Tổ chức các trò chơi, hoạt động vui chơi ngoài trời cho trẻ', N'Công viên Khu phố 2', '2025-06-15', '2025-06-17', 20, 100, 2, 2),
(N'Lễ phát quà tết Nguyên đán', N'Lê Văn C', N'Phát quà tết cho trẻ em có hoàn cảnh khó khăn', N'Trung tâm văn hóa Khu phố 3', '2025-01-20', '2025-01-21', 12, 80, 3, 3),
(N'Chiến dịch môi trường xanh', N'Phạm Thị D', N'Vệ sinh môi trường, trồng cây xanh tại khu phố', N'Khu vực công cộng Khu phố 4', '2025-04-22', '2025-04-22', 18, 60, 4, 4),
(N'Hội thi năng khiếu thiếu nhi', N'Võ Văn E', N'Thi vẽ, hát, múa, kể chuyện cho trẻ em', N'Hội trường Khu phố 5', '2025-05-25', '2025-05-26', 10, 70, 5, 5),
(N'Ngày hội đọc sách', N'Hoàng Thị F', N'Khuyến khích trẻ em đọc sách, tặng sách cho các em', N'Thư viện Khu phố 6', '2025-03-15', '2025-03-15', 8, 50, 6, 6),
(N'Chương trình từ thiện tết thiếu nhi', N'Bùi Thị H', N'Tặng quà, tổ chức vui chơi cho trẻ em nhân dịp 1/6', N'Sân vận động Khu phố 7', '2025-06-01', '2025-06-01', 16, 150, 8, 7),
(N'Lớp học kỹ năng sống', N'Đinh Văn I', N'Dạy kỹ năng sống, kỹ năng giao tiếp cho trẻ em', N'Nhà văn hóa Khu phố 8', '2025-07-10', '2025-07-20', 12, 40, 9, 8);

 
-- 9. ThoiGianChiTietSuKien (20 thời gian chi tiết)
INSERT INTO ThoiGianChiTietSuKien (MoTa, ThoiGianBatDau, ThoiGianKetThuc, SuKienID)
VALUES
(N'Buổi sáng ngày 1 - Lễ khai mạc', '2025-09-10 08:00', '2025-09-10 11:30', 1),
(N'Buổi chiều ngày 1 - Phát quà trung thu', '2025-09-10 14:00', '2025-09-10 17:00', 1),
(N'Buổi sáng ngày 2 - Trò chơi dân gian', '2025-09-11 08:00', '2025-09-11 11:00', 1),
(N'Ngày 1 - Lễ khai mạc hội trại', '2025-06-15 09:00', '2025-06-15 11:30', 2),
(N'Ngày 1 chiều - Trò chơi tập thể', '2025-06-15 14:00', '2025-06-15 17:00', 2),
(N'Ngày 2 - Hoạt động ngoài trời', '2025-06-16 08:00', '2025-06-16 17:00', 2),
(N'Ngày 3 - Lễ bế mạc', '2025-06-17 08:00', '2025-06-17 11:00', 2),
(N'Buổi sáng - Lễ phát quà', '2025-01-20 08:30', '2025-01-20 11:30', 3),
(N'Buổi chiều - Giao lưu văn nghệ', '2025-01-20 14:00', '2025-01-20 16:30', 3),
(N'Buổi sáng - Vệ sinh môi trường', '2025-04-22 07:00', '2025-04-22 10:00', 4),
(N'Buổi chiều - Trồng cây xanh', '2025-04-22 14:00', '2025-04-22 17:00', 4),
(N'Ngày 1 sáng - Vòng thi sơ khảo', '2025-05-25 08:00', '2025-05-25 12:00', 5),
(N'Ngày 1 chiều - Vòng thi bán kết', '2025-05-25 14:00', '2025-05-25 17:30', 5),
(N'Ngày 2 - Chung kết và trao giải', '2025-05-26 08:00', '2025-05-26 11:30', 5),
(N'Buổi sáng - Giới thiệu sách hay', '2025-03-15 08:30', '2025-03-15 11:00', 6),
(N'Buổi chiều - Tặng sách và giao lưu', '2025-03-15 14:00', '2025-03-15 16:30', 6),
(N'Buổi sáng - Lễ khai mạc tết thiếu nhi', '2025-06-01 08:00', '2025-06-01 11:30', 7),
(N'Buổi chiều - Phát quà và vui chơi', '2025-06-01 14:00', '2025-06-01 17:30', 7),
(N'Tuần 1 - Kỹ năng giao tiếp', '2025-07-10 14:00', '2025-07-14 17:00', 8),
(N'Tuần 2 - Kỹ năng tự bảo vệ bản thân', '2025-07-17 14:00', '2025-07-20 17:00', 8);

-- 10. PhanCongTinhNguyenVien (25 phân công)
INSERT INTO PhanCongTinhNguyenVien (SuKienID, TinhNguyenVienID, CongViec, GhiChu, NgayPhanCong)
VALUES
(1, 1, N'Điều phối chung', N'Giám sát toàn bộ sự kiện', '2025-08-25'),
(1, 2, N'Phát quà cho trẻ', N'Khu vực A', '2025-08-25'),
(1, 3, N'Trông trẻ', N'Khu vực B', '2025-08-25'),
(2, 4, N'Chuẩn bị sân khấu', N'Dựng sân khấu và âm thanh', '2025-06-01'),
(2, 5, N'Tổ chức trò chơi', N'Các trò chơi dân gian', '2025-06-01'),
(2, 6, N'Hỗ trợ ăn uống', N'Chuẩn bị bữa ăn cho trẻ', '2025-06-01'),
(3, 7, N'Phát quà tết', N'Điều phối phát quà', '2025-01-10'),
(3, 8, N'Giao lưu văn nghệ', N'Tổ chức tiết mục', '2025-01-10'),
(4, 1, N'Hướng dẫn vệ sinh', N'Chia nhóm làm việc', '2025-04-15'),
(4, 2, N'Trồng cây xanh', N'Hướng dẫn trồng cây', '2025-04-15'),
(5, 3, N'Dẫn chương trình', N'MC chính của sự kiện', '2025-05-10'),
(5, 4, N'Chấm thi', N'Hội đồng giám khảo', '2025-05-10'),
(5, 5, N'Tổ chức hậu cần', N'Chuẩn bị sân khấu, giải thưởng', '2025-05-10'),
(6, 6, N'Giới thiệu sách', N'Giới thiệu sách hay cho trẻ', '2025-03-01'),
(6, 7, N'Phát sách tặng', N'Phát sách cho các em', '2025-03-01'),
(7, 8, N'Điều phối phát quà', N'Tổ chức phát quà', '2025-05-20'),
(7, 1, N'Tổ chức trò chơi', N'Các trò chơi vui nhộn', '2025-05-20'),
(7, 2, N'Hỗ trợ ăn uống', N'Phục vụ bữa ăn', '2025-05-20'),
(8, 3, N'Giảng dạy kỹ năng giao tiếp', N'Giáo viên chính', '2025-07-01'),
(8, 4, N'Giảng dạy kỹ năng tự bảo vệ', N'Huấn luyện viên', '2025-07-01'),
(8, 5, N'Hỗ trợ giảng dạy', N'Trợ giảng', '2025-07-01'),
(1, 4, N'Nhiếp ảnh', N'Chụp ảnh sự kiện', '2025-08-25'),
(2, 7, N'Y tế', N'Hỗ trợ y tế cho trẻ', '2025-06-01'),
(3, 1, N'Đón tiếp', N'Đón tiếp khách mời', '2025-01-10'),
(7, 3, N'An ninh trật tự', N'Đảm bảo an toàn', '2025-05-20');

-- 11. TietMucSuKien (30 tiết mục)
INSERT INTO TietMucSuKien (TenTietMuc, NguoiThucHien, ChiPhiTietMuc, ThoiGianChiTietSuKienID)
VALUES
(N'Múa lân khai mạc', N'Đội múa lân Thanh niên', 800000, 1),
(N'Ca múa hát chào mừng', N'Nhóm thiếu nhi Khu phố 1', 500000, 1),
(N'Phát biểu khai mạc', N'Ban tổ chức', 0, 1),
(N'Phát quà trung thu', N'Tình nguyện viên', 0, 2),
(N'Trò chơi dân gian', N'Các em thiếu nhi', 300000, 3),
(N'Lễ khai mạc hội trại', N'Ban tổ chức', 0, 4),
(N'Trò chơi tập thể', N'Tình nguyện viên', 400000, 5),
(N'Cắm trại và nướng BBQ', N'Đội hậu cần', 1200000, 6),
(N'Lễ bế mạc và trao giải', N'Ban tổ chức', 600000, 7),
(N'Lễ phát quà tết', N'Tình nguyện viên', 0, 8),
(N'Tiểu phẩm hài tết', N'Nhóm thiếu nhi', 350000, 9),
(N'Vệ sinh đường phố', N'Tình nguyện viên và trẻ em', 200000, 10),
(N'Trồng cây xanh', N'Tình nguyện viên và trẻ em', 500000, 11),
(N'Thi vẽ tranh', N'Thí sinh dự thi', 300000, 12),
(N'Thi hát', N'Thí sinh dự thi', 400000, 12),
(N'Thi múa', N'Thí sinh dự thi', 450000, 13),
(N'Thi kể chuyện', N'Thí sinh dự thi', 250000, 13),
(N'Chung kết và trao giải', N'Ban giám khảo', 800000, 14),
(N'Giới thiệu sách thiếu nhi hay', N'Giáo viên', 200000, 15),
(N'Hoạt động đọc sách cùng nhau', N'Trẻ em và tình nguyện viên', 150000, 15),
(N'Tặng sách cho các em', N'Ban tổ chức', 0, 16),
(N'Lễ khai mạc tết thiếu nhi', N'Ban tổ chức', 700000, 17),
(N'Ca múa hát', N'Nhóm thiếu nhi', 600000, 17),
(N'Trò chơi và phát quà', N'Tình nguyện viên', 500000, 18),
(N'Bài giảng kỹ năng giao tiếp', N'Giảng viên', 1000000, 19),
(N'Thực hành giao tiếp', N'Học viên', 300000, 19),
(N'Bài giảng tự bảo vệ bản thân', N'Huấn luyện viên', 1200000, 20),
(N'Thực hành kỹ năng', N'Học viên', 400000, 20),
(N'Thi kéo co', N'Các đội thi', 200000, 5),
(N'Thi đố vui', N'Các đội thi', 250000, 3);

-- 12. ChiPhiSuKien (10 chi phí sự kiện)
INSERT INTO ChiPhiSuKien (TenKhoanChi, SoTien, NguoiPheDuyet, NgayPheDuyet, VanBanPheDuyet, GhiChu, SuKienID)
VALUES
(N'Thuê sân khấu và âm thanh', 3000000, N'Ban Giám đốc', '2025-08-20', N'CV-001/2025-SK1', N'Sự kiện trung thu', 1),
(N'Mua quà trung thu', 5000000, N'Phó Giám đốc', '2025-08-20', N'CV-002/2025-SK1', N'100 phần quà bánh trung thu', 1),
(N'Chi phí tổ chức hội trại', 8000000, N'Ban Giám đốc', '2025-06-01', N'CV-003/2025-SK2', N'Cắm trại, ăn uống, đồ dùng', 2),
(N'Mua quà tết', 4000000, N'Trưởng phòng TC-HC', '2025-01-05', N'CV-004/2025-SK3', N'80 phần quà tết', 3),
(N'Mua cây xanh và dụng cụ', 1500000, N'Ban Giám đốc', '2025-04-15', N'CV-005/2025-SK4', N'Cây xanh và dụng cụ làm vườn', 4),
(N'Chi phí tổ chức thi', 2500000, N'Phó Giám đốc', '2025-05-10', N'CV-006/2025-SK5', N'Sân khấu, giải thưởng', 5),
(N'Mua sách tặng', 3000000, N'Trưởng phòng TC-HC', '2025-03-01', N'CV-007/2025-SK6', N'50 bộ sách thiếu nhi', 6),
(N'Tổ chức tết thiếu nhi', 7000000, N'Ban Giám đốc', '2025-05-20', N'CV-008/2025-SK7', N'Quà tặng, ăn uống, tổ chức', 7),
(N'Chi phí giảng dạy', 3500000, N'Phó Giám đốc', '2025-07-01', N'CV-009/2025-SK8', N'Thù lao giảng viên, tài liệu', 8),
(N'Chi phí trang trí và tổ chức', 2000000, N'Trưởng phòng', '2025-08-20', N'CV-010/2025-SK1', N'Trang trí địa điểm sự kiện', 1);

-- 13. ChiTietChiPhiSuKien (30 chi tiết chi phí)
INSERT INTO ChiTietChiPhiSuKien (TenPhanQua, NguoiDaiDien, SoLuong, DonGia, ChiPhiID)
VALUES
(N'Bánh trung thu', N'Nguyễn Văn A', 100, 50000, 2),
(N'Lồng đèn', N'Nguyễn Văn A', 100, 30000, 2),
(N'Lều cắm trại', N'Trần Thị B', 20, 150000, 3),
(N'Bếp ga và nồi nấu', N'Trần Thị B', 5, 200000, 3),
(N'Thực phẩm BBQ', N'Trần Thị B', 100, 50000, 3),
(N'Túi quà tết', N'Lê Văn C', 80, 50000, 4),
(N'Cây xanh', N'Phạm Thị D', 50, 20000, 5),
(N'Dụng cụ làm vườn', N'Phạm Thị D', 20, 25000, 5),
(N'Giải nhất', N'Võ Văn E', 3, 500000, 6),
(N'Giải nhì', N'Võ Văn E', 6, 300000, 6),
(N'Giải ba', N'Võ Văn E', 9, 200000, 6),
(N'Giải khuyến khích', N'Võ Văn E', 15, 100000, 6),
(N'Bộ sách thiếu nhi', N'Hoàng Thị F', 50, 60000, 7),
(N'Quà tặng 1/6', N'Bùi Thị H', 150, 40000, 8),
(N'Nước ngọt', N'Bùi Thị H', 200, 10000, 8),
(N'Giáo trình kỹ năng', N'Đinh Văn I', 40, 50000, 9),
(N'Thù lao giảng viên', N'Đinh Văn I', 2, 1000000, 9),
(N'Hoa trang trí', N'Nguyễn Văn A', 50, 20000, 10),
(N'Bóng bay', N'Nguyễn Văn A', 200, 5000, 10),
(N'Băng rôn', N'Nguyễn Văn A', 10, 50000, 10),
(N'Áo đồng phục TNV', N'Trần Thị B', 20, 80000, 3),
(N'Nón bảo hiểm', N'Phạm Thị D', 50, 60000, 5),
(N'Micro không dây', N'Võ Văn E', 4, 300000, 6),
(N'Loa di động', N'Bùi Thị H', 2, 800000, 8),
(N'Máy chiếu', N'Đinh Văn I', 1, 1500000, 9),
(N'Bàn ghế', N'Nguyễn Văn A', 50, 30000, 1),
(N'Thuê xe đưa đón', N'Trần Thị B', 2, 1000000, 3),
(N'Khăn mặt in logo', N'Lê Văn C', 80, 25000, 4),
(N'Dây đeo thẻ', N'Hoàng Thị F', 50, 10000, 7),
(N'Túi xách canvas', N'Bùi Thị H', 150, 35000, 8);

-- 14. DangKySuKien (25 đăng ký sự kiện)
INSERT INTO DangKySuKien (SuKienID, UserID, NgayDangKy, TrangThai)
VALUES
(1, 3, '2025-08-15', N'Đã duyệt'),
(1, 7, '2025-08-16', N'Đã duyệt'),
(1, 10, '2025-08-17', N'Đã duyệt'),
(2, 3, '2025-06-05', N'Đã duyệt'),
(2, 7, '2025-06-06', N'Đã duyệt'),
(2, 10, '2025-06-07', N'Chờ duyệt'),
(3, 3, '2025-01-10', N'Đã duyệt'),
(3, 7, '2025-01-11', N'Đã duyệt'),
(4, 10, '2025-04-10', N'Đã duyệt'),
(5, 3, '2025-05-15', N'Từ chối'),
(5, 7, '2025-05-16', N'Đã duyệt'),
(5, 10, '2025-05-17', N'Đã duyệt'),
(6, 3, '2025-03-05', N'Đã duyệt'),
(6, 7, '2025-03-06', N'Đã duyệt'),
(7, 3, '2025-05-22', N'Đã duyệt'),
(7, 7, '2025-05-23', N'Đã duyệt'),
(7, 10, '2025-05-24', N'Đã duyệt'),
(8, 3, '2025-07-02', N'Đã duyệt'),
(8, 7, '2025-07-03', N'Chờ duyệt'),
(1, 1, '2025-08-18', N'Đã duyệt'),
(2, 2, '2025-06-08', N'Đã duyệt'),
(3, 4, '2025-01-12', N'Đã duyệt'),
(4, 5, '2025-04-11', N'Đã duyệt'),
(6, 6, '2025-03-07', N'Đã duyệt'),
(7, 8, '2025-05-25', N'Đã duyệt');

-- 15. ThongBao (10 thông báo)
INSERT INTO ThongBao (SuKienID, NoiDung, NgayThongBao)
VALUES
(1, N'Thông báo tổ chức sự kiện trung thu 2025. Kính mời phụ huynh và các em tham gia.', '2025-08-25'),
(2, N'Hội trại hè thiếu nhi sẽ diễn ra từ 15-17/6. Đăng ký tham gia trước 10/6.', '2025-06-01'),
(3, N'Lễ phát quà tết Nguyên đán cho trẻ em khó khăn. Mọi người hãy đến tham gia.', '2025-01-10'),
(4, N'Chiến dịch môi trường xanh - Cùng nhau bảo vệ môi trường.', '2025-04-15'),
(5, N'Hội thi năng khiếu thiếu nhi - Tìm kiếm tài năng nhí. Đăng ký ngay!', '2025-05-10'),
(6, N'Ngày hội đọc sách - Khuyến khích văn hóa đọc trong cộng đồng.', '2025-03-01'),
(7, N'Chương trình từ thiện tết thiếu nhi 1/6 - Cùng tạo niềm vui cho các em.', '2025-05-20'),
(8, N'Lớp học kỹ năng sống - Trang bị kỹ năng cho trẻ em. Số lượng có hạn.', '2025-07-01'),
(1, N'Nhắc nhở: Sự kiện trung thu sẽ diễn ra vào 10-11/9. Hãy chuẩn bị đầy đủ.', '2025-09-05'),
(2, N'Thông báo thay đổi địa điểm hội trại do thời tiết. Vui lòng theo dõi.', '2025-06-12');

-- 16. ThongBao_NguoiDung (30 quan hệ thông báo - người dùng)
INSERT INTO ThongBao_NguoiDung (ThongBaoID, UserID, DaDoc)
VALUES
(1, 3, 1), (1, 7, 1), (1, 10, 0),
(2, 3, 1), (2, 7, 0), (2, 10, 1),
(3, 3, 1), (3, 7, 1), (3, 10, 1),
(4, 10, 0), (4, 5, 1),
(5, 3, 1), (5, 7, 1), (5, 10, 0),
(6, 3, 1), (6, 7, 0),
(7, 3, 1), (7, 7, 1), (7, 10, 1),
(8, 3, 0), (8, 7, 0),
(9, 3, 1), (9, 7, 1), (9, 10, 1), (9, 1, 1),
(10, 3, 0), (10, 7, 0), (10, 10, 0), (10, 2, 1), (10, 5, 0);

-- 17. ThongTinPhuHuynh (10 phụ huynh)
INSERT INTO ThongTinPhuHuynh (HoTen, SDT, DiaChi, NgheNghiep, NgaySinh, TonGiao, DanToc, QuocTich, UserID)
VALUES
(N'Lê Văn C', '0909345678', N'123 Lê Lợi, Q1, TP.HCM', N'Công nhân', '1985-03-15', N'Không', N'Kinh', N'Việt Nam', 3),
(N'Đặng Văn G', '0909789012', N'45 Hai Bà Trưng, Q3, TP.HCM', N'Buôn bán', '1982-07-20', N'Phật giáo', N'Kinh', N'Việt Nam', 7),
(N'Dương Thị K', '0909012345', N'78 Nguyễn Huệ, Q1, TP.HCM', N'Nội trợ', '1990-11-10', N'Thiên chúa giáo', N'Kinh', N'Việt Nam', 10),
(N'Nguyễn Thị Lan', '0909111111', N'12 CMT8, Q10, TP.HCM', N'Giáo viên', '1988-05-25', N'Không', N'Kinh', N'Việt Nam', NULL),
(N'Trần Văn Minh', '0909222222', N'56 Nguyễn Thị Minh Khai, Q3, TP.HCM', N'Xe ôm', '1980-09-12', N'Phật giáo', N'Kinh', N'Việt Nam', NULL),
(N'Phạm Thị Hoa', '0909333333', N'89 Trần Hưng Đạo, Q5, TP.HCM', N'Bán hàng rong', '1986-02-28', N'Không', N'Kinh', N'Việt Nam', NULL),
(N'Hoàng Văn Nam', '0909444444', N'234 Võ Văn Tần, Q3, TP.HCM', N'Thợ xây', '1983-12-05', N'Phật giáo', N'Kinh', N'Việt Nam', NULL),
(N'Võ Thị Mai', '0909555555', N'567 Lý Thường Kiệt, Q10, TP.HCM', N'Nội trợ', '1992-04-18', N'Thiên chúa giáo', N'Kinh', N'Việt Nam', NULL),
(N'Bùi Văn Tuấn', '0909666666', N'345 Lê Duẩn, Q1, TP.HCM', N'Bảo vệ', '1981-08-22', N'Không', N'Kinh', N'Việt Nam', NULL),
(N'Đỗ Thị Hương', '0909777777', N'678 Điện Biên Phủ, Q3, TP.HCM', N'Giúp việc', '1987-06-30', N'Phật giáo', N'Kinh', N'Việt Nam', NULL);

-- 18. TruongHoc (8 trường học)
INSERT INTO TruongHoc (TenTruong, DiaChi, CapHoc)
VALUES
(N'Trường Tiểu học Nguyễn Du', N'123 Nguyễn Du, Q1, TP.HCM', N'Tiểu học'),
(N'Trường Tiểu học Lý Tự Trọng', N'456 Lý Tự Trọng, Q3, TP.HCM', N'Tiểu học'),
(N'Trường THCS Lê Quý Đôn', N'789 Lê Quý Đôn, Q3, TP.HCM', N'THCS'),
(N'Trường Tiểu học Trần Đại Nghĩa', N'234 Trần Đại Nghĩa, Q5, TP.HCM', N'Tiểu học'),
(N'Trường THCS Nguyễn Trãi', N'567 Nguyễn Trãi, Q5, TP.HCM', N'THCS'),
(N'Trường Tiểu học Võ Thị Sáu', N'890 Võ Thị Sáu, Q3, TP.HCM', N'Tiểu học'),
(N'Trường THCS Hai Bà Trưng', N'345 Hai Bà Trưng, Q1, TP.HCM', N'THCS'),
(N'Trường Tiểu học Đinh Tiên Hoàng', N'678 Đinh Tiên Hoàng, Q1, TP.HCM', N'Tiểu học');

-- 19. TreEm (20 trẻ em)
INSERT INTO TreEm (HoTen, NgaySinh, GioiTinh, TonGiao, DanToc, QuocTich, Anh, TruongID, TinhTrang, KhuPhoID)
VALUES
(N'Lê Văn An', '2015-03-10', N'Nam', N'Không', N'Kinh', N'Việt Nam', '/Anh/TreEm/em1.jpg', 1, N'Mồ côi cha', 1),
(N'Nguyễn Thị Bình', '2014-07-15', N'Nữ', N'Phật giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em2.jpg', 1, N'Hoàn cảnh khó khăn', 1),
(N'Trần Văn Chiến', '2016-11-20', N'Nam', N'Không', N'Kinh', N'Việt Nam', '/Anh/TreEm/em3.jpg', 2, N'Gia đình nghèo', 2),
(N'Phạm Thị Dung', '2013-05-25', N'Nữ', N'Thiên chúa giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em4.jpg', 3, N'Khuyết tật', 2),
(N'Võ Văn Em', '2015-09-12', N'Nam', N'Phật giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em5.jpg', 2, N'Hoàn cảnh khó khăn', 3),
(N'Hoàng Thị Hoa', '2014-02-28', N'Nữ', N'Không', N'Kinh', N'Việt Nam', '/Anh/TreEm/em6.jpg', 4, N'Mồ côi mẹ', 3),
(N'Đặng Văn Khánh', '2016-12-05', N'Nam', N'Phật giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em7.jpg', 4, N'Gia đình nghèo', 4),
(N'Bùi Thị Lan', '2015-04-18', N'Nữ', N'Thiên chúa giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em8.jpg', 5, N'Hoàn cảnh khó khăn', 4),
(N'Đinh Văn Minh', '2014-08-22', N'Nam', N'Không', N'Kinh', N'Việt Nam', '/Anh/TreEm/em9.jpg', 5, N'Khuyết tật', 5),
(N'Dương Thị Nga', '2013-06-30', N'Nữ', N'Phật giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em10.jpg', 3, N'Mồ côi cả cha lẫn mẹ', 5),
(N'Lý Văn Phong', '2016-10-15', N'Nam', N'Không', N'Kinh', N'Việt Nam', '/Anh/TreEm/em11.jpg', 6, N'Gia đình nghèo', 6),
(N'Phan Thị Quỳnh', '2015-01-08', N'Nữ', N'Phật giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em12.jpg', 6, N'Hoàn cảnh khó khăn', 6),
(N'Trương Văn Sơn', '2014-03-22', N'Nam', N'Thiên chúa giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em13.jpg', 7, N'Mồ côi cha', 7),
(N'Hồ Thị Tâm', '2016-07-17', N'Nữ', N'Không', N'Kinh', N'Việt Nam', '/Anh/TreEm/em14.jpg', 7, N'Gia đình nghèo', 7),
(N'Mai Văn Uy', '2015-11-30', N'Nam', N'Phật giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em15.jpg', 8, N'Khuyết tật', 8),
(N'Cao Thị Vân', '2013-09-14', N'Nữ', N'Không', N'Kinh', N'Việt Nam', '/Anh/TreEm/em16.jpg', 8, N'Hoàn cảnh khó khăn', 8),
(N'Lưu Văn Xuân', '2014-12-25', N'Nam', N'Phật giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em17.jpg', 1, N'Mồ côi mẹ', 1),
(N'Tô Thị Yến', '2016-05-19', N'Nữ', N'Thiên chúa giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em18.jpg', 2, N'Gia đình nghèo', 2),
(N'Quách Văn Đạt', '2015-08-11', N'Nam', N'Không', N'Kinh', N'Việt Nam', '/Anh/TreEm/em19.jpg', 3, N'Hoàn cảnh khó khăn', 3),
(N'Hứa Thị Ái', '2014-02-05', N'Nữ', N'Phật giáo', N'Kinh', N'Việt Nam', '/Anh/TreEm/em20.jpg', 4, N'Khuyết tật', 4);

-- 20. TreEm_PhuHuynh (25 quan hệ)
INSERT INTO TreEm_PhuHuynh (TreEmID, PhuHuynhID, MoiQuanHe)
VALUES
(1, 1, N'Cha'), (2, 1, N'Cha'),
(3, 2, N'Mẹ'), (4, 2, N'Mẹ'),
(5, 3, N'Cha'), (6, 3, N'Cha'),
(7, 4, N'Mẹ'), (8, 5, N'Cha'),
(9, 6, N'Mẹ'), (10, 6, N'Mẹ'),
(11, 7, N'Cha'), (12, 8, N'Mẹ'),
(13, 9, N'Cha'), (14, 9, N'Cha'),
(15, 10, N'Mẹ'), (16, 10, N'Mẹ'),
(17, 1, N'Cha'), (18, 2, N'Mẹ'),
(19, 4, N'Mẹ'), (20, 5, N'Cha'),
(1, 4, N'Bác'), (3, 7, N'Chú'),
(5, 8, N'Cô'), (11, 9, N'Ông'),
(15, 6, N'Bà');

-- 21. LopHoc (24 lớp học - mỗi trường 3 lớp)
INSERT INTO LopHoc (TenLop, TruongID)
VALUES
(N'Lớp 1A', 1), (N'Lớp 2B', 1), (N'Lớp 3C', 1),
(N'Lớp 1A', 2), (N'Lớp 2A', 2), (N'Lớp 3B', 2),
(N'Lớp 6A', 3), (N'Lớp 7B', 3), (N'Lớp 8C', 3),
(N'Lớp 1B', 4), (N'Lớp 2C', 4), (N'Lớp 3A', 4),
(N'Lớp 6B', 5), (N'Lớp 7A', 5), (N'Lớp 8B', 5),
(N'Lớp 1C', 6), (N'Lớp 2A', 6), (N'Lớp 3B', 6),
(N'Lớp 6C', 7), (N'Lớp 7C', 7), (N'Lớp 8A', 7),
(N'Lớp 1A', 8), (N'Lớp 2B', 8), (N'Lớp 3C', 8);

-- 22. PhieuHocTap (30 phiếu học tập)
INSERT INTO PhieuHocTap (DiemTrungBinh, XepLoai, HanhKiem, GhiChu, NamHoc, TruongID, TreEmID, LopID)
VALUES
(8.5, N'Giỏi', N'Tốt', N'Học sinh ngoan, chăm học', '2024-01-01', 1, 1, 1),
(7.2, N'Khá', N'Khá', N'Cần cố gắng hơn', '2024-01-01', 1, 2, 1),
(9.0, N'Giỏi', N'Tốt', N'Học sinh xuất sắc', '2024-01-01', 2, 3, 4),
(6.5, N'Trung bình', N'Tốt', N'Cần bổ sung kiến thức', '2024-01-01', 3, 4, 7),
(8.0, N'Giỏi', N'Tốt', N'Học sinh giỏi toàn diện', '2024-01-01', 2, 5, 5),
(7.8, N'Khá', N'Tốt', N'Tiến bộ đáng kể', '2024-01-01', 4, 6, 10),
(6.0, N'Trung bình', N'Khá', N'Cần quan tâm nhiều hơn', '2024-01-01', 4, 7, 11),
(8.8, N'Giỏi', N'Tốt', N'Học sinh ưu tú', '2024-01-01', 5, 8, 13),
(7.5, N'Khá', N'Tốt', N'Đạt kết quả khá', '2024-01-01', 5, 9, 14),
(9.2, N'Giỏi', N'Tốt', N'Học sinh xuất sắc nhất lớp', '2024-01-01', 3, 10, 8),
(7.0, N'Khá', N'Tốt', N'Phát triển tốt', '2024-01-01', 6, 11, 16),
(8.3, N'Giỏi', N'Tốt', N'Học sinh giỏi', '2024-01-01', 6, 12, 17),
(6.8, N'Trung bình', N'Khá', N'Cần nỗ lực thêm', '2024-01-01', 7, 13, 19),
(7.6, N'Khá', N'Tốt', N'Tiến bộ tốt', '2024-01-01', 7, 14, 20),
(8.9, N'Giỏi', N'Tốt', N'Học sinh giỏi toàn diện', '2024-01-01', 8, 15, 22),
(7.4, N'Khá', N'Tốt', N'Đạt kết quả khá', '2024-01-01', 8, 16, 23),
(8.1, N'Giỏi', N'Tốt', N'Học sinh tích cực', '2024-01-01', 1, 17, 2),
(6.9, N'Trung bình', N'Khá', N'Cần cố gắng hơn', '2024-01-01', 2, 18, 5),
(8.6, N'Giỏi', N'Tốt', N'Học sinh xuất sắc', '2024-01-01', 3, 19, 8),
(7.1, N'Khá', N'Tốt', N'Đạt kết quả tốt', '2024-01-01', 4, 20, 11),
(8.4, N'Giỏi', N'Tốt', N'Học sinh ngoan', '2023-01-01', 1, 1, 1),
(7.0, N'Khá', N'Khá', N'Tiến bộ rõ rệt', '2023-01-01', 1, 2, 1),
(8.7, N'Giỏi', N'Tốt', N'Học sinh giỏi', '2023-01-01', 2, 3, 4),
(6.3, N'Trung bình', N'Khá', N'Cần học thêm', '2023-01-01', 3, 4, 7),
(7.9, N'Khá', N'Tốt', N'Phát triển tốt', '2023-01-01', 2, 5, 5),
(7.5, N'Khá', N'Tốt', N'Đạt kết quả khá', '2023-01-01', 4, 6, 10),
(5.8, N'Trung bình', N'Khá', N'Cần quan tâm', '2023-01-01', 4, 7, 11),
(8.5, N'Giỏi', N'Tốt', N'Học sinh giỏi', '2023-01-01', 5, 8, 13),
(7.3, N'Khá', N'Tốt', N'Kết quả khá', '2023-01-01', 5, 9, 14),
(9.0, N'Giỏi', N'Tốt', N'Học sinh xuất sắc', '2023-01-01', 3, 10, 8);

-- 23. HoanCanh (8 loại hoàn cảnh)
INSERT INTO HoanCanh (LoaiHoanCanh, MoTa)
VALUES
(N'Mồ côi cha', N'Trẻ em có cha đã qua đời, mẹ nuôi dưỡng một mình'),
(N'Mồ côi mẹ', N'Trẻ em có mẹ đã qua đời, cha nuôi dưỡng một mình'),
(N'Mồ côi cả cha lẫn mẹ', N'Trẻ em mất cả cha và mẹ, sống với người thân'),
(N'Gia đình nghèo', N'Gia đình có hoàn cảnh kinh tế rất khó khăn'),
(N'Khuyết tật', N'Trẻ em có khuyết tật về thể chất hoặc tinh thần'),
(N'Hoàn cảnh khó khăn', N'Gia đình gặp khó khăn tạm thời về kinh tế'),
(N'Bị bạo lực gia đình', N'Trẻ em bị bạo lực hoặc ngược đãi trong gia đình'),
(N'Cha mẹ ly hôn', N'Cha mẹ đã ly hôn, trẻ sống với một trong hai người');

-- 24. TreEm_HoanCanh (25 quan hệ)
INSERT INTO TreEm_HoanCanh (TreEmID, HoanCanhID, NgayCapNhat)
VALUES
(1, 1, '2024-01-15'), (2, 6, '2024-02-20'),
(3, 4, '2024-03-10'), (4, 5, '2023-12-05'),
(5, 6, '2024-04-12'), (6, 2, '2023-11-18'),
(7, 4, '2024-05-08'), (8, 6, '2024-06-22'),
(9, 5, '2023-10-30'), (10, 3, '2023-09-15'),
(11, 4, '2024-07-05'), (12, 6, '2024-08-14'),
(13, 1, '2024-01-28'), (14, 4, '2024-09-03'),
(15, 5, '2023-08-20'), (16, 6, '2024-10-11'),
(17, 2, '2024-02-17'), (18, 4, '2024-03-25'),
(19, 6, '2024-04-30'), (20, 5, '2023-12-22'),
(1, 4, '2023-06-10'), (4, 7, '2024-01-05'),
(10, 4, '2024-05-18'), (15, 7, '2024-07-20'),
(19, 8, '2024-08-30');

-- 25. TreEm_SuKien (40 đăng ký trẻ em tham gia sự kiện)
INSERT INTO TreEm_SuKien (TreEmID, SuKienID, NgayDangKy, TrangThai, GhiChu)
VALUES
(1, 1, '2025-08-20', N'Đã đăng ký', N'Đã xác nhận tham gia'),
(2, 1, '2025-08-21', N'Đã đăng ký', N'Đã xác nhận'),
(3, 1, '2025-08-22', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(4, 1, '2025-08-23', N'Đã hủy', N'Gia đình có việc đột xuất'),
(5, 2, '2025-06-10', N'Đã đăng ký', N'Đã đóng phí'),
(6, 2, '2025-06-11', N'Đã đăng ký', N'Đã đóng phí'),
(7, 2, '2025-06-12', N'Chờ xác nhận', N'Đang chờ phụ huynh xác nhận'),
(8, 3, '2025-01-15', N'Đã đăng ký', N'Đã nhận quà'),
(9, 3, '2025-01-16', N'Đã đăng ký', N'Đã nhận quà'),
(10, 3, '2025-01-17', N'Đã đăng ký', N'Đã nhận quà'),
(11, 4, '2025-04-18', N'Đã đăng ký', N'Tham gia vệ sinh môi trường'),
(12, 4, '2025-04-19', N'Đã đăng ký', N'Tham gia trồng cây'),
(13, 5, '2025-05-18', N'Đã đăng ký', N'Thi múa'),
(14, 5, '2025-05-19', N'Đã đăng ký', N'Thi hát'),
(15, 5, '2025-05-20', N'Đã đăng ký', N'Thi vẽ'),
(16, 6, '2025-03-10', N'Đã đăng ký', N'Nhận sách tặng'),
(17, 6, '2025-03-11', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(18, 7, '2025-05-25', N'Đã đăng ký', N'Đã nhận quà 1/6'),
(19, 7, '2025-05-26', N'Đã đăng ký', N'Đã nhận quà 1/6'),
(20, 7, '2025-05-27', N'Đã đăng ký', N'Đã nhận quà 1/6'),
(1, 7, '2025-05-28', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(2, 7, '2025-05-29', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(3, 8, '2025-07-05', N'Đã đăng ký', N'Học kỹ năng giao tiếp'),
(4, 8, '2025-07-06', N'Đã đăng ký', N'Học kỹ năng tự bảo vệ'),
(5, 8, '2025-07-07', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(6, 1, '2025-08-24', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(7, 1, '2025-08-25', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(8, 2, '2025-06-13', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(9, 2, '2025-06-14', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(10, 5, '2025-05-21', N'Đã đăng ký', N'Thi kể chuyện'),
(11, 5, '2025-05-22', N'Chờ xác nhận',  N'Đã xác nhận tham gia'),
(12, 6, '2025-03-12', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(13, 7, '2025-05-30', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(14, 7, '2025-05-31', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(15, 3, '2025-01-18', N'Đã đăng ký', N'Đã nhận quà'),
(16, 3, '2025-01-19', N'Đã đăng ký', N'Đã nhận quà'),
(17, 4, '2025-04-20', N'Đã đăng ký',  N'Đã xác nhận tham gia'),
(18, 4, '2025-04-21', N'Đã đăng ký',  N'Đã xác nhận tham gia');

-- 26. VanDongTreEm (15 hoạt động vận động)
INSERT INTO VanDongTreEm (TreEmID, HoanCanhID, NguoiDungID, SoLan, LyDo, KetQua, NgayVanDong, AnhMinhChung, TinhTrangCapNhat, GhiChuChiTiet, NgayCapNhat)
VALUES
(1, 1, 2, 1, N'Cha mất, hoàn cảnh khó khăn', N'Đã tiếp nhận hỗ trợ', '2024-01-20', '/Anh/VanDong/vd1.jpg', N'Đã hoàn thành', N'Đã hỗ trợ học phí và sách vở', '2024-01-25'),
(2, 6, 2, 2, N'Gia đình gặp khó khăn tài chính', N'Đang theo dõi', '2024-02-15', '/Anh/VanDong/vd2.jpg', N'Đang xử lý', N'Đang xem xét hỗ trợ thêm', '2024-02-20'),
(3, 4, 5, 1, N'Gia đình nghèo, cần hỗ trợ', N'Đã hỗ trợ', '2024-03-10', '/Anh/VanDong/vd3.jpg', N'Đã hoàn thành', N'Đã cấp học bổng', '2024-03-15'),
(4, 5, 6, 3, N'Trẻ khuyết tật cần hỗ trợ', N'Đang điều trị', '2024-01-05', '/Anh/VanDong/vd4.jpg', N'Đang xử lý', N'Đang hỗ trợ chi phí y tế', '2024-04-10'),
(5, 6, 2, 1, N'Hoàn cảnh khó khăn', N'Đã tiếp nhận', '2024-04-18', '/Anh/VanDong/vd5.jpg', N'Đã hoàn thành', N'Hỗ trợ quần áo và học tập', '2024-04-22'),
(6, 2, 5, 2, N'Mất mẹ, cha nuôi khó khăn', N'Đang hỗ trợ', '2024-02-25', '/Anh/VanDong/vd6.jpg', N'Đang xử lý', N'Hỗ trợ định kỳ hàng tháng', '2024-05-01'),
(7, 4, 6, 1, N'Gia đình nghèo', N'Đã hỗ trợ', '2024-05-10', '/Anh/VanDong/vd7.jpg', N'Đã hoàn thành', N'Cấp học bổng năm học', '2024-05-15'),
(8, 6, 9, 1, N'Hoàn cảnh khó khăn', N'Đã tiếp nhận', '2024-06-20', '/Anh/VanDong/vd8.jpg', N'Đã hoàn thành', N'Hỗ trợ học phí', '2024-06-25'),
(9, 5, 2, 2, N'Khuyết tật, cần hỗ trợ dài hạn', N'Đang theo dõi', '2024-03-15', '/Anh/VanDong/vd9.jpg', N'Đang xử lý', N'Hỗ trợ điều trị và học tập', '2024-07-01'),
(10, 3, 5, 4, N'Mồ côi cả cha lẫn mẹ', N'Đã nhận hỗ trợ', '2023-10-20', '/Anh/VanDong/vd10.jpg', N'Đã hoàn thành', N'Hỗ trợ toàn diện', '2024-08-01'),
(11, 4, 6, 1, N'Gia đình nghèo', N'Đã hỗ trợ', '2024-07-08', '/Anh/VanDong/vd11.jpg', N'Đã hoàn thành', N'Cấp học bổng', '2024-07-12'),
(12, 6, 9, 1, N'Hoàn cảnh khó khăn', N'Đã tiếp nhận', '2024-08-15', '/Anh/VanDong/vd12.jpg', N'Đã hoàn thành', N'Hỗ trợ học phí và sách', '2024-08-20'),
(13, 1, 2, 2, N'Mồ côi cha, cần hỗ trợ', N'Đang hỗ trợ', '2024-02-10', '/Anh/VanDong/vd13.jpg', N'Đang xử lý', N'Hỗ trợ định kỳ', '2024-09-01'),
(15, 5, 5, 3, N'Khuyết tật, cần điều trị', N'Đang điều trị', '2023-09-15', '/Anh/VanDong/vd14.jpg', N'Đang xử lý', N'Hỗ trợ chi phí y tế', '2024-09-15'),
(19, 6, 9, 1, N'Hoàn cảnh khó khăn', N'Đã tiếp nhận', '2024-05-05', '/Anh/VanDong/vd15.jpg', N'Đã hoàn thành', N'Hỗ trợ học phí', '2024-05-10');

-- 1. HoTroPhucLoi (25 hỗ trợ phúc lợi - KHÔNG CÓ DotHoTroID)
--INSERT INTO HoTroPhucLoi (LoaiHoTro, MoTa, NgayCap, NguoiChiuTrachNhiemHoTro, TrangThaiPhat, NgayHenLai, GhiChuTNV, TreEmID, NguoiDungID)
--VALUES
--(N'Học bổng', N'Học bổng học kỳ 1 năm 2024', '2024-01-25', N'Trần Thị B', N'Đã phát', NULL, N'Đã nhận đầy đủ', 1, 2),
--(N'Hỗ trợ học phí', N'Hỗ trợ học phí toàn phần', '2024-02-20', N'Trần Thị B', N'Đã phát', NULL, N'Đã chuyển khoản', 2, 2),
--(N'Học bổng', N'Học bổng năm học 2024-2025', '2024-03-15', N'Võ Văn E', N'Đã phát', NULL, N'Đã nhận', 3, 5),
--(N'Hỗ trợ y tế', N'Chi phí điều trị khuyết tật', '2024-04-10', N'Hoàng Thị F', N'Đang xử lý', '2024-12-01', N'Đang theo dõi điều trị', 4, 6),
--(N'Hỗ trợ học phí', N'Hỗ trợ học phí học kỳ 2', '2024-04-22', N'Trần Thị B', N'Đã phát', NULL, NULL, 5, 2),
--(N'Hỗ trợ sinh hoạt', N'Hỗ trợ tiền ăn hàng tháng', '2024-05-01', N'Võ Văn E', N'Đã phát', '2024-11-01', N'Hỗ trợ định kỳ', 6, 5),
--(N'Học bổng', N'Học bổng học sinh giỏi', '2024-05-15', N'Hoàng Thị F', N'Đã phát', NULL, N'Đã nhận', 7, 6),
--(N'Hỗ trợ học phí', N'Hỗ trợ học phí toàn phần', '2024-06-25', N'Đinh Văn I', N'Đã phát', NULL, NULL, 8, 9),
--(N'Hỗ trợ y tế', N'Chi phí điều trị và phục hồi', '2024-07-01', N'Trần Thị B', N'Đang xử lý', '2025-01-01', N'Đang điều trị', 9, 2),
--(N'Hỗ trợ toàn diện', N'Hỗ trợ học phí, sinh hoạt, y tế', '2024-08-01', N'Võ Văn E', N'Đã phát', '2024-11-01', N'Hỗ trợ định kỳ hàng tháng', 10, 5),
--(N'Học bổng', N'Học bổng học kỳ 1', '2024-07-12', N'Hoàng Thị F', N'Đã phát', NULL, NULL, 11, 6),
--(N'Hỗ trợ học phí', N'Hỗ trợ học phí và sách vở', '2024-08-20', N'Đinh Văn I', N'Đã phát', NULL, N'Đã nhận đầy đủ', 12, 9),
--(N'Hỗ trợ sinh hoạt', N'Hỗ trợ tiền ăn định kỳ', '2024-09-01', N'Trần Thị B', N'Đã phát', '2024-12-01', N'Hỗ trợ hàng tháng', 13, 2),
--(N'Hỗ trợ y tế', N'Chi phí điều trị bệnh', '2024-09-15', N'Võ Văn E', N'Đang xử lý', '2025-02-01', N'Đang theo dõi', 15, 5),
--(N'Hỗ trợ học phí', N'Hỗ trợ học phí học kỳ 1', '2024-05-10', N'Đinh Văn I', N'Đã phát', NULL, NULL, 19, 9),
--(N'Quà tặng', N'Quà trung thu 2024', '2024-09-10', N'Trần Thị B', N'Đã phát', NULL, N'Đã nhận quà', 1, 2),
--(N'Quà tặng', N'Quà tết thiếu nhi', '2024-06-01', N'Bùi Thị H', N'Đã phát', NULL, N'Đã nhận', 2, 8),
--(N'Đồ dùng học tập', N'Bộ đồ dùng học tập đầy đủ', '2024-08-25', N'Hoàng Thị F', N'Đã phát', NULL, NULL, 3, 6),
--(N'Quần áo', N'Bộ quần áo đồng phục', '2024-05-20', N'Võ Văn E', N'Đã phát', NULL, N'Đã nhận', 4, 5),
--(N'Quà tặng', N'Quà tết Nguyên đán', '2024-01-20', N'Lê Văn C', N'Đã phát', NULL, NULL, 8, 3),
--(N'Đồ dùng học tập', N'Sách giáo khoa và vở', '2024-08-15', N'Trần Thị B', N'Đã phát', NULL, N'Đã nhận đầy đủ', 9, 2),
--(N'Quần áo', N'Quần áo mùa đông', '2024-11-05', N'Hoàng Thị F', N'Chờ phát', '2024-11-15', N'Chờ nhận hàng', 10, 6),
--(N'Quà tặng', N'Quà trung thu 2024', '2024-09-11', N'Nguyễn Văn A', N'Đã phát', NULL, NULL, 6, 1),
--(N'Hỗ trợ học phí', N'Học phí học kỳ 2', '2024-01-15', N'Trần Thị B', N'Đã phát', NULL, N'Đã chuyển khoản', 14, 2),
--(N'Đồ dùng học tập', N'Bộ sách tham khảo', '2024-03-20', N'Hoàng Thị F', N'Đã phát', NULL, N'Đã nhận', 16, 6);

-- 3. PhieuMinhChung
INSERT INTO PhieuMinhChung (LoaiMinhChung, FilePath, NgayCap, UngHoID)
VALUES
(N'Biên nhận học phí', N'/MinhChung/biennhan_hocphi_1.pdf', '2024-01-25', 1),
(N'Xác nhận chuyển khoản', N'/MinhChung/chuyenkhoan_2.pdf', '2024-02-20', 2),
(N'Tặng Quà cho trẻ em', N'/MinhChung/AnhTangQua.jpg', '2024-02-20', 2),
(N'Tặng Quà cho trẻ em', N'/MinhChung/AnhTangQua1.jpg', '2024-02-20', 2),
(N'Biên nhận học bổng', N'/MinhChung/hocbong_3.pdf', '2024-03-15', 3),
(N'Hóa đơn y tế', N'/MinhChung/hoadon_yte_4.pdf', '2024-04-10', 4),
(N'Biên nhận học phí', N'/MinhChung/biennhan_hocphi_5.pdf', '2024-04-22', 5),
(N'Biên nhận tiền hỗ trợ', N'/MinhChung/biennhan_hotro_6.pdf', '2024-05-01', 6),
(N'Biên nhận học bổng', N'/MinhChung/hocbong_7.pdf', '2024-05-15', 7),
(N'Xác nhận chuyển khoản', N'/MinhChung/chuyenkhoan_8.pdf', '2024-06-25', 8),
(N'Hóa đơn điều trị', N'/MinhChung/hoadon_dieutri_9.pdf', '2024-07-01', 9),
(N'Biên nhận toàn diện', N'/MinhChung/biennhan_toandien_10.pdf', '2024-08-01', 10);
--(N'Biên nhận học bổng', N'/MinhChung/hocbong_11.pdf', '2024-07-12', 11),
--(N'Biên nhận học phí', N'/MinhChung/biennhan_hocphi_12.pdf', '2024-08-20', 12),
--(N'Biên nhận tiền sinh hoạt', N'/MinhChung/biennhan_sinhoat_13.pdf', '2024-09-01', 13),
--(N'Hóa đơn y tế', N'/MinhChung/hoadon_yte_14.pdf', '2024-09-15', 14),
--(N'Biên nhận học phí', N'/MinhChung/biennhan_hocphi_15.pdf', '2024-05-10', 15),
--(N'Biên nhận quà tặng', N'/MinhChung/biennhan_qua_16.pdf', '2024-09-10', 16),
--(N'Biên nhận quà tặng', N'/MinhChung/biennhan_qua_17.pdf', '2024-06-01', 17),
--(N'Biên nhận đồ dùng', N'/MinhChung/biennhan_dodung_18.pdf', '2024-08-25', 18),
--(N'Biên nhận quần áo', N'/MinhChung/biennhan_quanao_19.pdf', '2024-05-20', 19),
--(N'Biên nhận quà tết', N'/MinhChung/biennhan_quatet_20.pdf', '2024-01-20', 20);


--INSERT INTO UngHo_HoTroPhucLoi (UngHoID, HoTroID)
--VALUES
---- Ủng hộ ID 1 (Tiền mặt học phí) -> Học bổng và hỗ trợ học phí
--(1, 1), (1, 2), (1, 5),
---- Ủng hộ ID 4 (Tài trợ sự kiện) -> Hỗ trợ y tế
--(4, 4), (4, 9), (4, 14),
---- Ủng hộ ID 6 (Học bổng) -> Hỗ trợ sinh hoạt
--(6, 6), (6, 10), (6, 13),
---- Ủng hộ ID 2 (Sách giáo khoa) -> Học bổng học sinh giỏi
--(2, 3), (2, 7), (2, 11),
---- Ủng hộ ID 3 (Quà trung thu) -> Quà tặng
--(3, 16), (3, 23),
---- Ủng hộ ID 7 (Đồ chơi giáo dục) -> Quà tặng
--(7, 17),
---- Ủng hộ ID 9 (Đồ dùng học tập) -> Đồ dùng học tập
--(9, 18), (9, 21), (9, 25),
---- Ủng hộ ID 5 (Quần áo) -> Quần áo
--(5, 19), (5, 22),
---- Ủng hộ ID 8 (Tiền mặt) -> Hỗ trợ học phí
--(8, 8), (8, 12), (8, 15), (8, 24),
---- Ủng hộ ID 10 (Quà tết) -> Quà tặng
--(10, 20);
-- 30. PhanBoUngHoChiPhi (15 phân bổ)
INSERT INTO PhanBoUngHoChiPhi (UngHoID, ChiPhiID, SoTienPhanBo, TyLe, NguoiPheDuyet, NgayPheDuyet, GhiChu)
VALUES
(1, 1, 1500000, 30.00, N'Nguyễn Văn A', '2024-08-20', N'Phân bổ cho thuê sân khấu'),
(1, 2, 2000000, 40.00, N'Nguyễn Văn A', '2024-08-20', N'Phân bổ mua quà trung thu'),
(1, 10, 1500000, 30.00, N'Nguyễn Văn A', '2024-08-20', N'Phân bổ chi phí trang trí'),
(4, 3, 5000000, 50.00, N'Trần Thị B', '2024-06-01', N'Phân bổ cho hội trại hè'),
(4, 8, 3000000, 30.00, N'Bùi Thị H', '2024-05-20', N'Phân bổ tết thiếu nhi'),
(4, 9, 2000000, 20.00, N'Đinh Văn I', '2024-07-01', N'Phân bổ lớp kỹ năng sống'),
(6, 1, 1500000, 18.75, N'Nguyễn Văn A', '2024-08-20', N'Bổ sung chi phí sự kiện'),
(6, 3, 3000000, 37.50, N'Trần Thị B', '2024-06-01', N'Bổ sung hội trại'),
(6, 8, 3500000, 43.75, N'Bùi Thị H', '2024-05-20', N'Bổ sung tết thiếu nhi'),
(8, 4, 2500000, 41.67, N'Lê Văn C', '2024-01-05', N'Phân bổ lễ tết'),
(8, 5, 1500000, 25.00, N'Phạm Thị D', '2024-04-15', N'Phân bổ môi trường xanh'),
(8, 7, 2000000, 33.33, N'Hoàng Thị F', '2024-03-01', N'Phân bổ ngày hội sách'),
(2, 2, 2000000, 100.00, N'Nguyễn Văn A', '2024-08-20', N'Ủng hộ toàn bộ cho quà trung thu'),
(5, 6, 1500000, 100.00, N'Võ Văn E', '2024-05-10', N'Ủng hộ toàn bộ cho hội thi'),
(9, 9, 3500000, 100.00, N'Đinh Văn I', '2024-07-01', N'Ủng hộ toàn bộ cho lớp kỹ năng');

-- 31. QuaTangUngHo (15 quà tặng)
INSERT INTO QuaTangUngHo (UngHoID, SuKienID, TenQua, MoTa, SoLuongTong, SoLuongConLai, DonGia, DoiTuongNhan, Anh,NguoiChiuTrachNhiem,LoaiHoTro)
VALUES
(3, 1, N'Bánh trung thu cao cấp', N'Bánh trung thu nhân thập cẩm', 100, 5, 50000, N'Trẻ em tham gia sự kiện', '/Anh/QuaTang/qua1.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(3, 1, N'Lồng đèn trung thu', N'Lồng đèn hình ngôi sao', 100, 8, 30000, N'Trẻ em tham gia sự kiện', '/Anh/QuaTang/qua2.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(7, 7, N'Bộ đồ chơi giáo dục', N'Đồ chơi phát triển trí tuệ', 80, 12, 56250, N'Trẻ em 6-12 tuổi', '/Anh/QuaTang/qua3.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(10, 7, N'Phần quà tết thiếu nhi', N'Gồm bánh kẹo, đồ chơi, sách', 120, 20, 58333, N'Tất cả trẻ em', '/Anh/QuaTang/qua4.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(2, 2, N'Bộ sách giáo khoa', N'Sách giáo khoa lớp 1-5', 50, 10, 40000, N'Trẻ em nghèo vượt khó', '/Anh/QuaTang/qua5.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(5, 5, N'Bộ quần áo đồng phục', N'Quần áo đồng phục mới', 30, 5, 50000, N'Trẻ em học giỏi', '/Anh/QuaTang/qua6.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(9, 6, N'Bộ đồ dùng học tập', N'Bút, vở, thước, hộp bút', 60, 8, 58333, N'Trẻ em cần hỗ trợ', '/Anh/QuaTang/qua7.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(4, 3, N'Túi quà tết', N'Bánh kẹo, mứt tết', 80, 0, 50000, N'Trẻ em khó khăn', '/Anh/QuaTang/qua8.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(4, 4, N'Cây xanh tặng gia đình', N'Cây cảnh nhỏ', 50, 15, 20000, N'Gia đình trẻ em', '/Anh/QuaTang/qua9.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(6, 1, N'Túi xách trung thu', N'Túi xách đựng quà', 100, 10, 25000, N'Trẻ em tham gia', '/Anh/QuaTang/qua10.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(7, 7, N'Gấu bông', N'Gấu bông size nhỏ', 80, 18, 56250, N'Trẻ em dưới 10 tuổi', '/Anh/QuaTang/qua11.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(10, 7, N'Bóng đá mini', N'Bóng đá size 3', 50, 12, 58333, N'Trẻ em nam', '/Anh/QuaTang/qua12.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(9, 8, N'Giáo trình kỹ năng sống', N'Sách kỹ năng cho trẻ', 40, 8, 50000, N'Học viên khóa học', '/Anh/QuaTang/qua13.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(3, 1, N'Kẹo trung thu', N'Kẹo các loại', 100, 15, 15000, N'Trẻ em tham gia', '/Anh/QuaTang/qua14.jpg',N'Nguyễn Văn A',N'Quà tặng'),
(10, 7, N'Vở viết', N'Combo 10 quyển vở', 120, 25, 25000, N'Tất cả trẻ em', '/Anh/QuaTang/qua15.jpg',N'Nguyễn Văn A',N'Quà tặng');

-- 32. PhanPhatQua (50 phân phát quà)
INSERT INTO PhanPhatQua (QuaTangUngHoID, TreEmID, SoLuongNhan, NgayPhanPhat, NguoiPhanPhat, TrangThai, GhiChu)
VALUES
(1, 1, 1, '2025-09-10', N'Trần Thị B', N'Đã nhận', N'Đã ký nhận'),
(1, 2, 1, '2025-09-10', N'Trần Thị B', N'Đã nhận', N'Đã ký nhận'),
(1, 3, 1, '2025-09-10', N'Trần Thị B', N'Đã nhận', N'Đã ký nhận'),
(2, 2, 1, '2025-09-10', N'Trần Thị B', N'Đã nhận', N'Đã ký nhận'),
(2, 3, 1, '2025-09-10', N'Trần Thị B', N'Đã nhận', N'Phụ huynh đến nhận'),
(14, 1, 1, '2025-09-10', N'Trần Thị B', N'Đã nhận', N'Đã ký nhận'),
(14, 2, 1, '2025-09-10', N'Trần Thị B', N'Đã nhận', N'Đã ký nhận'),
(14, 3, 1, '2025-09-10', N'Trần Thị B', N'Đã nhận', N'Phụ huynh đại diện nhận'),
(10, 1, 1, '2025-09-10', N'Nguyễn Văn A', N'Đã nhận', N'Đã ký nhận'),
(10, 2, 1, '2025-09-10', N'Nguyễn Văn A', N'Đã nhận', N'Đã ký nhận'),
(10, 6, 1, '2025-09-10', N'Nguyễn Văn A', N'Đã nhận', N'Phụ huynh nhận hộ'),
(10, 7, 1, '2025-09-10', N'Nguyễn Văn A', N'Đã nhận', N'Đã ký nhận'),
(3, 18, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận đầy đủ'),
(3, 19, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận đầy đủ'),
(3, 20, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Phụ huynh ký nhận'),
(3, 1, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(3, 2, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(11, 18, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Trẻ em rất thích'),
(11, 19, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(11, 20, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Phụ huynh đại diện'),
(11, 1, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(11, 2, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(4, 18, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Quà phong phú'),
(4, 19, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(4, 20, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Gia đình hài lòng'),
(4, 1, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(4, 2, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(12, 18, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Trẻ rất thích chơi bóng'),
(12, 19, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(12, 13, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Phụ huynh đại diện'),
(12, 14, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(15, 18, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Vở đẹp, chất lượng tốt'),
(15, 19, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(15, 20, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(15, 1, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Đã ký nhận'),
(15, 2, 1, '2025-06-01', N'Bùi Thị H', N'Đã nhận', N'Phụ huynh ký nhận'),
(5, 3, 1, '2025-06-16', N'Trần Thị B', N'Đã nhận', N'Sách mới, đầy đủ'),
(5, 5, 1, '2025-06-16', N'Trần Thị B', N'Đã nhận', N'Đã ký nhận'),
(5, 18, 1, '2025-06-16', N'Trần Thị B', N'Đã nhận', N'Gia đình cảm ơn'),
(5, 8, 1, '2025-06-17', N'Trần Thị B', N'Đã nhận', N'Đã ký nhận'),
(5, 9, 1, '2025-06-17', N'Trần Thị B', N'Đã nhận', N'Phụ huynh đại diện'),
(6, 13, 1, '2025-05-26', N'Võ Văn E', N'Đã nhận', N'Quần áo vừa vặn'),
(6, 14, 1, '2025-05-26', N'Võ Văn E', N'Đã nhận', N'Đã ký nhận'),
(6, 15, 1, '2025-05-26', N'Võ Văn E', N'Đã nhận', N'Chất lượng tốt'),
(7, 11, 1, '2025-09-05', N'Đinh Văn I', N'Đã nhận', N'Bộ đồ dùng đầy đủ'),
(7, 12, 1, '2025-09-05', N'Đinh Văn I', N'Đã nhận', N'Đã ký nhận'),
(7, 9, 1, '2025-09-05', N'Đinh Văn I', N'Đã nhận', N'Phụ huynh hài lòng'),
(7, 19, 1, '2025-09-05', N'Đinh Văn I', N'Đã nhận', N'Đã ký nhận'),
(8, 8, 1, '2025-01-20', N'Lê Văn C', N'Đã nhận', N'Quà tết phong phú'),
(8, 9, 1, '2025-01-20', N'Lê Văn C', N'Đã nhận', N'Đã ký nhận'),
(8, 10, 1, '2025-01-20', N'Lê Văn C', N'Đã nhận', N'Gia đình cảm ơn nhiều');


SELECT * FROM KhuPho;
SELECT * FROM ManhThuongQuan;
SELECT * FROM TinhNguyenVien;
SELECT * FROM LichTrong;
SELECT * FROM ChiTietLichTrong;
SELECT * FROM SuKien;
SELECT * FROM ThoiGianChiTietSuKien;
SELECT * FROM TietMucSuKien;
SELECT * FROM ChiPhiSuKien;
SELECT * FROM ChiTietChiPhiSuKien;
SELECT * FROM PhanCongTinhNguyenVien;
SELECT * FROM DangKySuKien;
SELECT * FROM NguoiDung;
SELECT * FROM TreEm;
SELECT * FROM TreEm_PhuHuynh;
SELECT * FROM ThongTinPhuHuynh;
SELECT * FROM ThongBao_NguoiDung;
SELECT * FROM TruongHoc;
SELECT * FROM LopHoc;
SELECT * FROM ThongBao;
SELECT * FROM PhieuHocTap;
SELECT * FROM HoanCanh;
SELECT * FROM TreEm_HoanCanh;
SELECT * FROM VanDongTreEm;
-- SELECT * FROM HoTroPhucLoi;
SELECT * FROM PhieuMinhChung;
-- SELECT * FROM UngHo_HoTroPhucLoi;
SELECT * FROM UngHo;
SELECT * FROM ThongBao_NguoiDung
SELECT * FROM TreEm_SuKien
SELECT * FROM PhanBoUngHoChiPhi
SELECT * FROM QuaTangUngHo
SELECT * FROM PhanPhatQua

ALTER TABLE TietMucSuKien
ADD SuKienID INT NULL;
ALTER TABLE SuKien
ADD AnhSuKien NVARCHAR(500) NULL;
ALTER TABLE TreEm
ADD Useyn BIT DEFAULT 1;

select * from SuKien
update SuKien
set NgayBatDau='2025-12-20'
where SuKienID = 3
update SuKien
set NgayKetThuc='2025-12-21'
where SuKienID = 3
select * from UngHo,QuaTangUngHo where UngHo.UngHoID=QuaTangUngHo.QuaTangUngHoID and UngHo.UngHoID=2
select * from QuaTangUngHo where QuaTangUngHoID=24
select * from UngHo where UngHoID=18