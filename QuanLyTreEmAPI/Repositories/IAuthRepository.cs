using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface IAuthRepository
    {
        Task<NguoiDung> GetUserByEmailAsync(string email);
        Task<NguoiDung> GetUserByIdAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
    }
}
