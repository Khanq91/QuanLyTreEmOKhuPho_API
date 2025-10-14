using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Repositories
{
    public class HoTroPhucLoiRepository : IHoTroPhucLoiRepository
    {
        private readonly QuanLyTreEmContext _context;

        public HoTroPhucLoiRepository(QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<HoTroPhucLoi> CreateAsync(HoTroPhucLoi hoTro)
        {
            _context.HoTroPhucLois.Add(hoTro);
            await _context.SaveChangesAsync();
            return hoTro;
        }

        public async Task<List<HoTroPhucLoi>> GetByTreEmIdAsync(int treEmId)
        {
            return await _context.HoTroPhucLois
                .Include(h => h.PhieuMinhChungs)
                .Where(h => h.TreEmId == treEmId)
                .OrderByDescending(h => h.NgayCap)
                .ToListAsync();
        }
    }
}
