-- ==============================
-- TẠO CƠ SỞ DỮ LIỆU
-- ==============================
CREATE DATABASE QuanLyTreEm;
GO
USE QuanLyTreEm;
GO

-- Bảng khu phố (Liên kết Trẻ em, Tình nguyện viên, và Sự kiện)
CREATE TABLE KhuPho (
    KhuPhoID INT PRIMARY KEY IDENTITY(1,1),
    TenKhuPho NVARCHAR(100),
    DiaChi NVARCHAR(200),
    QuanHuyen NVARCHAR(100),
    ThanhPho NVARCHAR(100),
);
-- Bảng Người dùng
CREATE TABLE NguoiDung (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100),
	SDT nvarchar(12) UNIQUE,
    Email NVARCHAR(100) UNIQUE,
    MatKhau NVARCHAR(100),
    VaiTro NVARCHAR(50),
    NgayTao DATE,
	Anh varchar(500),
	TrangThai nvarchar(50)
);

-- Bảng Mạnh thường quân

CREATE TABLE ManhThuongQuan (
    ManhThuongQuanID INT PRIMARY KEY IDENTITY(1,1),
    Ten NVARCHAR(100), -- Tên mạnh thường quân (cá nhân, tổ chức)
    Loai NVARCHAR(50), -- Loại mạnh thường quân (Cá nhân, Tổ chức)
    DiaChi NVARCHAR(200),
    SDT NVARCHAR(15),
    Email NVARCHAR(100),
    GhiChu NVARCHAR(300)
);
CREATE TABLE UngHo (
    UngHoID INT PRIMARY KEY IDENTITY(1,1),
    SoTien DECIMAL(18,2), -- Số tiền ủng hộ
    LoaiUngHo NVARCHAR(100), -- Loại ủng hộ (tiền mặt, hiện vật, quà tặng,...)
    NgayUngHo DATE, -- Ngày ủng hộ
    GhiChu NVARCHAR(300), -- Ghi chú thêm nếu có
    ManhThuongQuanID INT FOREIGN KEY REFERENCES ManhThuongQuan(ManhThuongQuanID), -- Liên kết với mạnh thường quân
);


-- Bảng Tình nguyện viên
CREATE TABLE TinhNguyenVien (
    TinhNguyenVienID INT PRIMARY KEY IDENTITY(1,1),
    SDT NVARCHAR(15),
    NgaySinh DATE,
    ChucVu NVARCHAR(100),
	UserID INT FOREIGN KEY REFERENCES NguoiDung(UserID) UNIQUE,
	KhuPhoID INT FOREIGN KEY REFERENCES KhuPho(KhuPhoID),
);
CREATE TABLE LichTrong (
	LichTrongID INT PRIMARY KEY IDENTITY(1,1),
	TinhNguyenVienID INT FOREIGN KEY REFERENCES TinhNguyenVien(TinhNguyenVienID) UNIQUE,	
);
CREATE TABLE ChiTietLichTrong (
	ChiTietLichTrongID INT PRIMARY KEY IDENTITY(1,1),
	Buoi NVARCHAR(15),
	Thu NVARCHAR(15),
	LichTrongID INT FOREIGN KEY REFERENCES LichTrong(LichTrongID) ,	
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
	KhuPhoID INT FOREIGN KEY REFERENCES KhuPho(KhuPhoID) ,
);
CREATE TABLE ThoiGianChiTietSuKien(
	ThoiGianChiTietSuKienID INT PRIMARY KEY IDENTITY(1,1),
	MoTa NVARCHAR(MAX),
	ThoiGianBatDau DATETIME,
	ThoiGianKetThuc DATETIME,
	SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID)
);
CREATE TABLE PhanCongTinhNguyenVien (
    PhanCongID INT PRIMARY KEY IDENTITY(1,1),
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID),
    TinhNguyenVienID INT FOREIGN KEY REFERENCES TinhNguyenVien(TinhNguyenVienID),
    CongViec NVARCHAR(200),       -- công việc được phân công
    GhiChu NVARCHAR(300),         -- ghi chú thêm nếu cần
    NgayPhanCong DATE DEFAULT GETDATE()
);

CREATE TABLE TietMucSuKien(
	TietMucID INT PRIMARY KEY IDENTITY(1,1),
	TenTietMuc NVARCHAR(100),
	NguoiThucHien NVARCHAR(100),
	ChiPhiTietMuc DECIMAL(18,2),
	ThoiGianChiTietSuKienID INT FOREIGN KEY REFERENCES ThoiGianChiTietSuKien(ThoiGianChiTietSuKienID),
);
CREATE TABLE ChiPhiSuKien (
    ChiPhiID INT PRIMARY KEY IDENTITY(1,1),
    TenKhoanChi NVARCHAR(200),  
    SoTien DECIMAL(18,2),
    GhiChu NVARCHAR(300) NULL,
	SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID),  
);
CREATE TABLE ChiTietChiPhiSuKien(
	ChiTietChiPhi INT PRIMARY KEY IDENTITY(1,1),
	TenPhanQua NVARCHAR(20),
	NguoiDaiDien NVARCHAR(200),
	SoLuong INT,
	DonGia DECIMAL(18,2),
	ChiPhiID INT  FOREIGN KEY REFERENCES ChiPhiSuKien(ChiPhiID),
);
CREATE TABLE DangKySuKien (
    DangKySuKienID INT PRIMARY KEY IDENTITY(1,1),
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID),
    UserID INT FOREIGN KEY REFERENCES NguoiDung(UserID),
    NgayDangKy DATE,
    TrangThai NVARCHAR(50)
);
CREATE TABLE ThongBao (
    ThongBaoID INT PRIMARY KEY IDENTITY(1,1),
    SuKienID INT FOREIGN KEY REFERENCES SuKien(SuKienID),
    NoiDung NVARCHAR(MAX),
    NgayThongBao DATE DEFAULT GETDATE()
);
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
	UserID INT FOREIGN KEY REFERENCES NguoiDung(UserID),
);

-- Bảng Trường học
CREATE TABLE TruongHoc (
    TruongID INT PRIMARY KEY IDENTITY(1,1),
    TenTruong NVARCHAR(200),
    DiaChi NVARCHAR(200),
    CapHoc NVARCHAR(50),
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
	Anh varchar(500),
    TruongID INT FOREIGN KEY REFERENCES TruongHoc(TruongID),
    TinhTrang NVARCHAR(100),
	KhuPhoID INT FOREIGN KEY REFERENCES KhuPho(KhuPhoID),
);

CREATE TABLE TreEm_PhuHuynh (
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    PhuHuynhID INT FOREIGN KEY REFERENCES ThongTinPhuHuynh(PhuHuynhID),
    MoiQuanHe NVARCHAR(50), -- Bố, mẹ, ông, bà...
    PRIMARY KEY (TreEmID, PhuHuynhID)
);
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
    NgayCapNhat DATE,
    TruongID INT FOREIGN KEY REFERENCES TruongHoc(TruongID),
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),	
	LopID INT FOREIGN KEY REFERENCES LopHoc(LopID),

);


CREATE TABLE HoanCanh (
    HoanCanhID INT PRIMARY KEY IDENTITY(1,1),
    LoaiHoanCanh NVARCHAR(100),
    MoTa NVARCHAR(MAX)
);

CREATE TABLE TreEm_HoanCanh (
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    HoanCanhID INT FOREIGN KEY REFERENCES HoanCanh(HoanCanhID),
    NgayCapNhat DATE,
    PRIMARY KEY (TreEmID, HoanCanhID)
);

CREATE TABLE VanDongTreEm (
    VanDongID INT PRIMARY KEY IDENTITY(1,1),
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    HoanCanhID INT FOREIGN KEY REFERENCES HoanCanh(HoanCanhID), 
    NguoiDungID INT FOREIGN KEY REFERENCES NguoiDung(UserID),
    SoLan INT DEFAULT 1,
    LyDo NVARCHAR(200),
    KetQua NVARCHAR(200),
    NgayVanDong DATE DEFAULT GETDATE()
);

-- Bảng Hỗ trợ phúc lợi
CREATE TABLE HoTroPhucLoi (
    HoTroID INT PRIMARY KEY IDENTITY(1,1),
    LoaiHoTro NVARCHAR(100),
    MoTa NVARCHAR(MAX),
    NgayCap DATE,
    NguoiChiuTrachNhiemHoTro NVARCHAR(100),
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
);
-- Bảng Phiếu minh chứng
CREATE TABLE PhieuMinhChung (
    MinhChungID INT PRIMARY KEY IDENTITY(1,1),
    LoaiMinhChung NVARCHAR(100),
    FilePath NVARCHAR(300),
    NgayCap DATE,
	HoTroID INT FOREIGN KEY (HoTroID) REFERENCES HoTroPhucLoi(HoTroID)
);

CREATE TABLE UngHo_TreEm (
    UngHoID INT FOREIGN KEY REFERENCES UngHo(UngHoID),
    TreEmID INT FOREIGN KEY REFERENCES TreEm(TreEmID),
    PRIMARY KEY (UngHoID, TreEmID)
);
CREATE TABLE UngHo_HoTroPhucLoi (
    UngHoID INT FOREIGN KEY REFERENCES UngHo(UngHoID),
    HoTroID INT FOREIGN KEY REFERENCES HoTroPhucLoi(HoTroID),
    PRIMARY KEY (UngHoID, HoTroID)
);
USE QuanLyTreEm;
GO
-- ============================
-- 1. KhuPho
-- ============================
INSERT INTO KhuPho (TenKhuPho, DiaChi, QuanHuyen, ThanhPho)
VALUES 
(N'Khu phố 1', N'123 Lê Lợi', N'Quận 1', N'Hồ Chí Minh'),
(N'Khu phố 2', N'45 Hai Bà Trưng', N'Quận 3', N'Hồ Chí Minh'),
(N'Khu phố 3', N'78 Nguyễn Huệ', N'Quận 1', N'Hồ Chí Minh'),
(N'Khu phố 4', N'12 CMT8', N'Quận 10', N'Hồ Chí Minh'),
(N'Khu phố 5', N'56 Nguyễn Thị Minh Khai', N'Quận 3', N'Hồ Chí Minh');

-- ============================
-- 2. NguoiDung
-- ============================
INSERT INTO NguoiDung (HoTen, SDT, Email, MatKhau, VaiTro, NgayTao,Anh,TrangThai)
VALUES
(N'Nguyễn Văn A', '0909123456', 'a@gmail.com', '123', N'Admin', '2024-10-20','/Anh/NguoiDung/hinh1.jpg',N'Đang hoạt động'),
(N'Trần Thị B', '0909234567', 'b@gmail.com', '123', N'Tình nguyện viên', '2024-10-20','/Anh/NguoiDung/hinh2.jpg',N'Đang hoạt động'),
(N'Lê Văn C', '0909345678', 'c@gmail.com', '123', N'Phụ huynh', '2023-10-20','/Anh/NguoiDung/hinh3.jpg',N'Đang hoạt động'),
(N'Phạm Thị D', '0909456789', 'd@gmail.com', '123', N'Cán bộ', '2022-10-20','/Anh/NguoiDung/hinh4.jpg',N'Đang hoạt động'),
(N'Võ Văn E', '0909567890', 'e@gmail.com', '123', N'Tình nguyện viên', '2021-10-20','/Anh/NguoiDung/hinh5.jpg',N'Đang hoạt động');

-- ============================
-- 3. ManhThuongQuan
-- ============================
INSERT INTO ManhThuongQuan (Ten, Loai, DiaChi, SDT, Email, GhiChu)
VALUES
(N'Nguyễn Mạnh Hùng', N'Cá nhân', N'Q1, TP.HCM', '0911111111', 'hung@gmail.com', N'Hay ủng hộ học sinh nghèo'),
(N'Công ty ABC', N'Tổ chức', N'Q3, TP.HCM', '0922222222', 'abc@company.com', N'Tài trợ quỹ thiếu nhi'),
(N'Nguyễn Thị Lan', N'Cá nhân', N'Q5, TP.HCM', '0933333333', 'lan@gmail.com', N'Tài trợ quần áo'),
(N'Công ty XYZ', N'Tổ chức', N'Q10, TP.HCM', '0944444444', 'xyz@company.com', N'Ủng hộ tiền mặt'),
(N'Trần Văn Tâm', N'Cá nhân', N'Q7, TP.HCM', '0955555555', 'tam@gmail.com', N'Ủng hộ học bổng');

-- ============================
-- 4. UngHo
-- ============================
INSERT INTO UngHo (SoTien, LoaiUngHo, NgayUngHo, GhiChu, ManhThuongQuanID)
VALUES
(5000000, N'Tiền mặt', '2025-01-01', N'Ủng hộ học phí', 1),
(2000000, N'Hiện vật', '2025-02-15', N'Ủng hộ sách vở', 2),
(3000000, N'Quà tặng', '2025-03-10', N'Quà trung thu', 3),
(10000000, N'Tiền mặt', '2025-04-20', N'Tài trợ sự kiện hè', 4),
(1500000, N'Hiện vật', '2025-05-05', N'Ủng hộ quần áo', 5);

-- ============================
-- 5. TinhNguyenVien
-- ============================
INSERT INTO TinhNguyenVien (SDT, NgaySinh, ChucVu, UserID, KhuPhoID)
VALUES
('0909234567', '2000-01-01', N'Trưởng nhóm', 2, 1),
('0909567890', '1998-02-02', N'Tình nguyện viên', 5, 2),
('0909999999', '2001-03-03', N'Tình nguyện viên', 4, 3),
('0908888888', '1999-04-04', N'Phó nhóm', 3, 4),
('0907777777', '2002-05-05', N'Tình nguyện viên', 1, 5);

-- ============================
-- 6. LichTrong
-- ============================
INSERT INTO LichTrong (TinhNguyenVienID)
VALUES (1),(2),(3),(4),(5);

-- ============================
-- 7. ChiTietLichTrong
-- ============================
INSERT INTO ChiTietLichTrong (Buoi, Thu, LichTrongID)
VALUES
(N'Sáng', N'Thứ 2', 1),
(N'Chiều', N'Thứ 3', 1),
(N'Sáng', N'Thứ 4', 2),
(N'Chiều', N'Thứ 5', 3),
(N'Sáng', N'Thứ 6', 4);

-- ============================
-- 8. SuKien
-- ============================
INSERT INTO SuKien (TenSuKien, NguoiChiuTrachNhiem, MoTa, DiaDiem, NgayBatDau, NgayKetThuc, SoLuongTinhNguyenVien, SoLuongTreEm, UserID, KhuPhoID)
VALUES
(N'Sự kiện trung thu', N'Nguyễn Văn A', N'Phát quà cho trẻ em', N'Khu phố 1', '2025-10-20', '2025-10-21', 10, 100, 1, 1),
(N'Hội trại hè', N'Trần Thị B', N'Tổ chức vui chơi cho trẻ', N'Khu phố 2', '2025-10-22', '2025-10-23', 15, 80, 2, 2),
(N'Lễ phát quà tết', N'Lê Văn C', N'Phát quà tết cho trẻ khó khăn', N'Khu phố 3', '2025-10-24', '2025-10-25', 8, 50, 3, 3),
(N'Chiến dịch xanh', N'Phạm Thị D', N'Vệ sinh khu phố', N'Khu phố 4', '2025-10-26', '2025-10-27', 20, 0, 4, 4),
(N'Hội thi thiếu nhi', N'Võ Văn E', N'Thi năng khiếu trẻ em', N'Khu phố 5', '2025-10-25', '2025-10-27', 12, 60, 5, 5);

-- ============================
-- 9. ThoiGianChiTietSuKien
-- ============================
INSERT INTO ThoiGianChiTietSuKien (MoTa, ThoiGianBatDau, ThoiGianKetThuc, SuKienID)
VALUES
(N'Buổi sáng trung thu', '2025-10-20 08:00', '2025-10-20 11:00', 1),
(N'Buổi chiều trung thu', '2025-10-20 14:00', '2025-10-20 17:00', 1),
(N'Lễ khai mạc hội trại', '2025-10-22 09:00', '2025-10-22 11:00', 2),
(N'Thi năng khiếu', '2025-10-25 08:00', '2025-10-25 10:00', 5),
(N'Lễ phát quà tết', '2025-10-24 09:00', '2025-10-24 11:00', 3);

-- ============================
-- 10. PhanCongTinhNguyenVien
-- ============================
INSERT INTO PhanCongTinhNguyenVien (SuKienID, TinhNguyenVienID, CongViec, GhiChu)
VALUES
(1,1,N'Phát quà',N'Đảm nhiệm khu A'),
(1,2,N'Trông trẻ',N'Khu vực B'),
(2,3,N'Chuẩn bị sân khấu',N'Buổi sáng'),
(3,4,N'Giao lưu',N'Trò chơi dân gian'),
(5,5,N'Dẫn chương trình',N'Lễ khai mạc');

-- ============================
-- 11. TietMucSuKien
-- ============================
INSERT INTO TietMucSuKien (TenTietMuc, NguoiThucHien, ChiPhiTietMuc, ThoiGianChiTietSuKienID)
VALUES
(N'Múa lân',N'Nhóm thiếu nhi',500000,1),
(N'Hát đơn ca',N'Bé Mai',300000,2),
(N'Tiểu phẩm hài',N'Nhóm 5B',400000,3),
(N'Tham gia trò chơi',N'Trẻ khu 3',200000,4),
(N'Hát tập thể',N'Tập thể lớp 4A',250000,5);

-- ============================
-- 12. ChiPhiSuKien
-- ============================
INSERT INTO ChiPhiSuKien (TenKhoanChi, SoTien, GhiChu, SuKienID)
VALUES
(N'Thuê sân khấu',3000000,N'Ngày chính',1),
(N'Mua quà',2000000,N'Tặng trẻ',2),
(N'Mua nước uống',500000,N'Phục vụ tình nguyện viên',3),
(N'Thuê âm thanh',1500000,N'Sự kiện hè',4),
(N'Mua bánh kẹo',800000,N'Trung thu',5);

-- ============================
-- 13. ChiTietChiPhiSuKien
-- ============================
INSERT INTO ChiTietChiPhiSuKien (TenPhanQua, NguoiDaiDien, SoLuong, DonGia, ChiPhiID)
VALUES
(N'Bánh trung thu',N'Nguyễn Văn A',100,20000,1),
(N'Sách giáo khoa',N'Trần Thị B',50,30000,2),
(N'Nước suối',N'Lê Văn C',200,5000,3),
(N'Mic không dây',N'Phạm Thị D',2,500000,4),
(N'Bánh kẹo',N'Võ Văn E',150,8000,5);

-- ============================
-- 14. DangKySuKien
-- ============================
INSERT INTO DangKySuKien (SuKienID, UserID, NgayDangKy, TrangThai)
VALUES
(1,1,GETDATE(),N'Đã duyệt'),
(2,2,GETDATE(),N'Đã duyệt'),
(3,3,GETDATE(),N'Chờ duyệt'),
(4,4,GETDATE(),N'Đã duyệt'),
(5,5,GETDATE(),N'Từ chối');

-- ============================
-- 15. ThongBao
-- ============================
INSERT INTO ThongBao (SuKienID, NoiDung)
VALUES
(1,N'Thông báo phát quà trung thu'),
(2,N'Thông báo hội trại hè'),
(3,N'Thông báo lễ phát quà tết'),
(4,N'Thông báo chiến dịch xanh'),
(5,N'Thông báo hội thi thiếu nhi');

-- ============================
-- 16. ThongBao_NguoiDung
-- ============================
INSERT INTO ThongBao_NguoiDung (ThongBaoID, UserID)
VALUES
(1,1),(1,2),(2,3),(3,4),(4,5);

-- ============================
-- 17. ThongTinPhuHuynh
-- ============================
INSERT INTO ThongTinPhuHuynh (HoTen, SDT, DiaChi, NgheNghiep, NgaySinh, TonGiao, DanToc, QuocTich, UserID)
VALUES
(N'Lê Văn Bình','0901122334',N'Q1, HCM',N'Công nhân','1980-05-05',N'Không',N'Kinh',N'Việt Nam',3),
(N'Trần Thị Hoa','0902233445',N'Q2, HCM',N'Giáo viên','1985-03-03',N'Phật giáo',N'Kinh',N'Việt Nam',4),
(N'Nguyễn Văn Long','0903344556',N'Q3, HCM',N'Lái xe','1978-04-04',N'Thiên chúa',N'Kinh',N'Việt Nam',5),
(N'Lý Thị Mai','0904455667',N'Q4, HCM',N'Kế toán','1988-06-06',N'Không',N'Kinh',N'Việt Nam',1),
(N'Phạm Văn An','0905566778',N'Q5, HCM',N'Thợ điện','1975-07-07',N'Phật giáo',N'Kinh',N'Việt Nam',2);

-- ============================
-- 18. TruongHoc
-- ============================
INSERT INTO TruongHoc (TenTruong, DiaChi, CapHoc)
VALUES
(N'Trường Tiểu học A',N'Q1, TP.HCM',N'Tiểu học'),
(N'Trường THCS B',N'Q3, TP.HCM',N'THCS'),
(N'Trường Mầm non C',N'Q5, TP.HCM',N'Mầm non'),
(N'Trường THPT D',N'Q10, TP.HCM',N'THPT'),
(N'Trường Mầm non E',N'Q7, TP.HCM',N'Mầm non');

-- ============================
-- 19. TreEm
-- ============================
INSERT INTO TreEm (HoTen, NgaySinh, GioiTinh, TonGiao, DanToc, QuocTich, TruongID, TinhTrang, KhuPhoID,Anh)
VALUES
(N'Bé Lan','2015-03-15',N'Nữ',N'Không',N'Kinh',N'Việt Nam',1,N'Khó khăn',1,'/Anh/TreEm\/inh1.jpg'),
(N'Bé Tí','2013-02-20',N'Nam',N'Không',N'Kinh',N'Việt Nam',2,N'Bình thường',2,'/Anh/TreEm/hinh1.jpg'),
(N'Bé Hoa','2016-04-10',N'Nữ',N'Phật giáo',N'Kinh',N'Việt Nam',3,N'Mồ côi',3,'/Anh/TreEm/hinh1.jpg'),
(N'Bé Nam','2012-06-25',N'Nam',N'Không',N'Kinh',N'Việt Nam',4,N'Khó khăn',4,'/Anh/TreEm/hinh1.jpg'),
(N'Bé Bình','2014-08-30',N'Nam',N'Không',N'Kinh',N'Việt Nam',5,N'Bình thường',5,'/Anh/TreEm/hinh1.jpg');

-- ============================
-- 20. TreEm_PhuHuynh
-- ============================
INSERT INTO TreEm_PhuHuynh (TreEmID, PhuHuynhID, MoiQuanHe)
VALUES
(1,1,N'Cha'),
(1,2,N'Mẹ'),
(2,3,N'Cha'),
(3,4,N'Mẹ'),
(4,5,N'Cha');

-- ============================
-- 21. LopHoc
-- ============================
INSERT INTO LopHoc (TenLop, TruongID)
VALUES
(N'Lớp 1A',1),
(N'Lớp 2B',2),
(N'Lớp 3C',3),
(N'Lớp 4D',4),
(N'Lớp 5E',5);

-- ============================
-- 22. PhieuHocTap
-- ============================
INSERT INTO PhieuHocTap (DiemTrungBinh, XepLoai, HanhKiem, GhiChu, NgayCapNhat, TruongID, TreEmID, LopID)
VALUES
(8.5,N'Giỏi',N'Tốt',N'Hoàn thành tốt',GETDATE(),1,1,1),
(7.2,N'Khá',N'Tốt',N'Tích cực',GETDATE(),2,2,2),
(6.8,N'Trung bình',N'Khá',N'Cần cố gắng',GETDATE(),3,3,3),
(9.0,N'Xuất sắc',N'Tốt',N'Gương mẫu',GETDATE(),4,4,4),
(8.0,N'Giỏi',N'Tốt',N'Rất chăm học',GETDATE(),5,5,5);

-- ============================
-- 23. HoanCanh
-- ============================
INSERT INTO HoanCanh (LoaiHoanCanh, MoTa)
VALUES
(N'Hộ nghèo',N'Gia đình thuộc diện hộ nghèo'),
(N'Mồ côi cha',N'Mất cha từ nhỏ'),
(N'Mồ côi mẹ',N'Mất mẹ sớm'),
(N'Khuyết tật',N'Cần hỗ trợ đặc biệt'),
(N'Bình thường',N'Không thuộc diện khó khăn');

-- ============================
-- 24. TreEm_HoanCanh
-- ============================
INSERT INTO TreEm_HoanCanh (TreEmID, HoanCanhID, NgayCapNhat)
VALUES
(1,1,GETDATE()),
(2,5,GETDATE()),
(3,2,GETDATE()),
(4,3,GETDATE()),
(5,5,GETDATE());

-- ============================
-- 25. VanDongTreEm
-- ============================
INSERT INTO VanDongTreEm (TreEmID, HoanCanhID, NguoiDungID, SoLan, LyDo, KetQua)
VALUES
(1,1,1,1,N'Hỗ trợ học phí',N'Đã cấp học bổng'),
(2,5,2,1,N'Tham gia trại hè',N'Hoàn thành tốt'),
(3,2,3,2,N'Ủng hộ quần áo',N'Đã nhận'),
(4,3,4,1,N'Tặng quà tết',N'Hoàn tất'),
(5,5,5,1,N'Tặng sách',N'Đã trao');

-- ============================
-- 26. HoTroPhucLoi
-- ============================
INSERT INTO HoTroPhucLoi (LoaiHoTro, MoTa, NgayCap, NguoiChiuTrachNhiemHoTro, TreEmID)
VALUES
(N'Học bổng',N'Hỗ trợ học tập',GETDATE(),N'Nguyễn Văn A',1),
(N'Trang phục',N'Phát quần áo',GETDATE(),N'Trần Thị B',2),
(N'Quà tết',N'Tặng bánh kẹo',GETDATE(),N'Lê Văn C',3),
(N'Học phẩm',N'Tặng sách vở',GETDATE(),N'Phạm Thị D',4),
(N'Học bổng',N'Tặng tiền học',GETDATE(),N'Võ Văn E',5);

-- ============================
-- 27. PhieuMinhChung
-- ============================
INSERT INTO PhieuMinhChung (LoaiMinhChung, FilePath, NgayCap, HoTroID)
VALUES
(N'Hóa đơn',N'/files/hoa_don1.pdf',GETDATE(),1),
(N'Ảnh',N'/files/anh1.jpg',GETDATE(),2),
(N'Giấy biên nhận',N'/files/biennhan1.pdf',GETDATE(),3),
(N'Video',N'/files/video1.mp4',GETDATE(),4),
(N'Giấy chứng nhận',N'/files/chungnhan1.pdf',GETDATE(),5);

-- ============================
-- 28. UngHo_TreEm
-- ============================
INSERT INTO UngHo_TreEm (UngHoID, TreEmID)
VALUES
(1,1),(2,2),(3,3),(4,4),(5,5);

-- ============================
-- 29. UngHo_HoTroPhucLoi
-- ============================
INSERT INTO UngHo_HoTroPhucLoi (UngHoID, HoTroID)
VALUES
(1,1),(2,2),(3,3),(4,4),(5,5);

-- ============================
-- SHOW TOÀN BỘ DỮ LIỆU 29 BẢNG
-- ============================

SELECT * FROM KhuPho;
SELECT * FROM NguoiDung;
SELECT * FROM ManhThuongQuan;
SELECT * FROM UngHo;
SELECT * FROM TinhNguyenVien;
SELECT * FROM LichTrong;
SELECT * FROM ChiTietLichTrong;
SELECT * FROM SuKien;
SELECT * FROM ThoiGianChiTietSuKien;
SELECT * FROM PhanCongTinhNguyenVien;
SELECT * FROM TietMucSuKien;
SELECT * FROM ChiPhiSuKien;
SELECT * FROM ChiTietChiPhiSuKien;
SELECT * FROM DangKySuKien;
SELECT * FROM ThongBao;
SELECT * FROM ThongBao_NguoiDung;
SELECT * FROM ThongTinPhuHuynh;
SELECT * FROM TruongHoc;
SELECT * FROM TreEm;
SELECT * FROM TreEm_PhuHuynh;
SELECT * FROM LopHoc;
SELECT * FROM PhieuHocTap;
SELECT * FROM HoanCanh;
SELECT * FROM TreEm_HoanCanh;
SELECT * FROM VanDongTreEm;
SELECT * FROM HoTroPhucLoi;
SELECT * FROM PhieuMinhChung;
SELECT * FROM UngHo_TreEm;
SELECT * FROM UngHo_HoTroPhucLoi;
SELECT 
    nd.HoTen AS TenTinhNguyenVien,
    te.HoTen AS TenTreEm,
    hc.LoaiHoanCanh,
    hc.MoTa AS MoTaHoanCanh,
    vdt.SoLan,
    vdt.LyDo,
    vdt.KetQua,
    vdt.NgayVanDong
FROM VanDongTreEm vdt
JOIN NguoiDung nd ON vdt.NguoiDungID = nd.UserID
JOIN TreEm te ON vdt.TreEmID = te.TreEmID
JOIN HoanCanh hc ON vdt.HoanCanhID = hc.HoanCanhID
WHERE vdt.NguoiDungID = 2;

update NguoiDung
set TrangThai=N'Ngưng hoạt động'
where UserID=2