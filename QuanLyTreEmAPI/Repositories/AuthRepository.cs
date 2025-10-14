using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly QuanLyTreEmContext _context;

        public AuthRepository(QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<NguoiDung> GetUserByEmailAsync(string email)
        {
            return await _context.NguoiDungs
                .Include(n => n.TinhNguyenVien)
                    .ThenInclude(t => t.KhuPho)
                .Include(n => n.ThongTinPhuHuynhs)
                .FirstOrDefaultAsync(n => n.Email == email);
        }

        public async Task<NguoiDung> GetUserByIdAsync(int userId)
        {
            return await _context.NguoiDungs
                .Include(n => n.TinhNguyenVien)
                    .ThenInclude(t => t.KhuPho)
                .Include(n => n.ThongTinPhuHuynhs)
                .FirstOrDefaultAsync(n => n.UserId == userId);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return false;

            user.MatKhau = newPassword;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}