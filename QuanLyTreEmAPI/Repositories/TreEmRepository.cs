using QuanLyTreEmAPI.DTOs.TreEm;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public class TreEmRepository : ITreEmRepository
    {
        private readonly QuanLyTreEmContext _context;

        public TreEmRepository(QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<(List<TreEm> items, int totalCount)> GetAllAsync(TreEmFilterDTO filter)
        {
            var query = _context.TreEms
                .Include(t => t.Truong)
                .Include(t => t.KhuPho)
                .Include(t => t.TreEmHoanCanhs)
                    .ThenInclude(h => h.HoanCanh)
                .Include(t => t.TreEmPhuHuynhs)
                    .ThenInclude(p => p.PhuHuynh)
                .AsQueryable();

            // Apply filters
            if (filter.KhuPhoID.HasValue)
                query = query.Where(t => t.KhuPhoId == filter.KhuPhoID.Value);

            if (!string.IsNullOrEmpty(filter.GioiTinh))
                query = query.Where(t => t.GioiTinh == filter.GioiTinh);

            if (filter.HoanCanhID.HasValue)
                query = query.Where(t => t.TreEmHoanCanhs.Any(h => h.HoanCanhId == filter.HoanCanhID.Value));

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(t => t.HoTen.Contains(filter.SearchTerm) ||
                                        t.TreEmId.ToString().Contains(filter.SearchTerm));
            }

            // Age filter
            if (filter.TuoiMin.HasValue || filter.TuoiMax.HasValue)
            {
                //var today = DateTime.Today;
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (filter.TuoiMin.HasValue)
                {
                    var maxDate = today.AddYears(-filter.TuoiMin.Value);
                    query = query.Where(t => t.NgaySinh <= maxDate);
                }
                if (filter.TuoiMax.HasValue)
                {
                    var minDate = today.AddYears(-filter.TuoiMax.Value - 1);
                    query = query.Where(t => t.NgaySinh > minDate);
                }
            }

            var totalCount = await query.CountAsync();

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "hoten" => filter.SortOrder == "desc" ? query.OrderByDescending(t => t.HoTen) : query.OrderBy(t => t.HoTen),
                "ngaysinh" => filter.SortOrder == "desc" ? query.OrderByDescending(t => t.NgaySinh) : query.OrderBy(t => t.NgaySinh),
                "khupho" => filter.SortOrder == "desc" ? query.OrderByDescending(t => t.KhuPho.TenKhuPho) : query.OrderBy(t => t.KhuPho.TenKhuPho),
                _ => query.OrderBy(t => t.HoTen)
            };

            // Pagination
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<TreEm> GetByIdAsync(int id)
        {
            return await _context.TreEms
                .Include(t => t.Truong)
                .Include(t => t.KhuPho)
                .Include(t => t.TreEmHoanCanhs)
                    .ThenInclude(h => h.HoanCanh)
                .Include(t => t.TreEmPhuHuynhs)
                    .ThenInclude(p => p.PhuHuynh)
                .Include(t => t.PhieuHocTaps)
                    .ThenInclude(p => p.Truong)
                .Include(t => t.PhieuHocTaps)
                    .ThenInclude(p => p.Lop)
                .Include(t => t.HoTroPhucLois)
                    .ThenInclude(h => h.PhieuMinhChungs)
                .Include(t => t.VanDongTreEms)
                    .ThenInclude(v => v.HoanCanh)
                .Include(t => t.VanDongTreEms)
                    .ThenInclude(v => v.NguoiDung)
                .FirstOrDefaultAsync(t => t.TreEmId == id);
        }

        public async Task<TreEm> CreateAsync(TreEm treEm)
        {
            _context.TreEms.Add(treEm);
            await _context.SaveChangesAsync();
            return treEm;
        }

        public async Task<bool> UpdateAsync(TreEm treEm)
        {
            _context.TreEms.Update(treEm);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var treEm = await _context.TreEms.FindAsync(id);
            if (treEm == null) return false;

            _context.TreEms.Remove(treEm);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TreEms.AnyAsync(t => t.TreEmId == id);
        }

        public async Task<List<TreEm>> GetTreEmCanUuTienAsync(int top = 10)
        {
            // Logic tính điểm ưu tiên (Có gom cụm ròi sửa lại sau)
            return await _context.TreEms
                .Include(t => t.TreEmHoanCanhs)
                    .ThenInclude(h => h.HoanCanh)
                .Include(t => t.KhuPho)
                .OrderByDescending(t => t.TreEmHoanCanhs.Count)
                .Take(top)
                .ToListAsync();
        }

        public async Task<List<TreEm>> GetTreEmNghiHocNhieuAsync()
        {
            // Logic xác định trẻ nghỉ học nhiều
            // Tạm thời return empty, cần logic phức tạp hơn
            return new List<TreEm>();
        }

        public async Task<List<TreEm>> GetTreEmChuaVaoLop1Async()
        {
            //var today = DateTime.Today;
            var today = DateOnly.FromDateTime(DateTime.Today);
            var minDate = today.AddYears(-7);
            var maxDate = today.AddYears(-6);

            return await _context.TreEms
                .Include(t => t.KhuPho)
                .Where(t => t.NgaySinh >= minDate && t.NgaySinh < maxDate && t.TruongId == null)
                .ToListAsync();
        }

        public async Task<int> GetTongSoTreEmAsync()
        {
            return await _context.TreEms.CountAsync();
        }

        public async Task<List<object>> GetTreEmTheoKhuPhoAsync()
        {
            return await _context.TreEms
                .GroupBy(t => new { t.KhuPhoId, t.KhuPho.TenKhuPho })
                .Select(g => new
                {
                    TenKhuPho = g.Key.TenKhuPho ?? "Chưa xác định",
                    SoLuong = g.Count()
                })
                .ToListAsync<object>();
        }

    }
}