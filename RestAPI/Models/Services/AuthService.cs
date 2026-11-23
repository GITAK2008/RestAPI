using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services
{
    public class AuthService
    {
        private readonly LibraryDbContext db;

        public AuthService(LibraryDbContext _db)
        {
            db = _db;
        }

        public async Task<User?> Login(string login, string password)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.Login == login && u.Password == password);
        }

        public async Task<bool> Register(User user)
        {
            if (await db.Users.AnyAsync(u => u.Login == user.Login))
                return false;

            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO \"User\" (\"Login\", \"Password\") VALUES ({user.Login}, {user.Password})");
            return true;
        }
    }
}
