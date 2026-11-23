using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Abstractions;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services
{
    public class ReaderService : AbstractionService, ICommonService<Reader, int>
    {
        private const int pageSize = 10;
        private readonly LibraryDbContext db;
        public ReaderService(LibraryDbContext _db)
        {
            this.db = _db;
        }
        public bool Create(Reader model)
        {
            bool result = DoAction(delegate ()
            {
                db.Readers.Add(model);
                db.SaveChanges();
            });
            return result;
        }

        public bool Delete(int id)
        {
            Reader? res = db.Readers.FirstOrDefault(p => p.ReaderId == id);
            if (res == null) return false;
            bool result = DoAction(delegate ()
            {
                db.Readers.Remove(res);
                db.SaveChanges();
            });
            return result;
        }

        public async Task<Reader> Get(int id)
        {
            Reader? reader = await db.Readers.FirstOrDefaultAsync(p => p.ReaderId == id);
            return reader!;
        }

        public async Task<IEnumerable<Reader>> GetAll()
        {
            return await db.Readers.ToListAsync();
        }

        public bool Update(int id, Reader model)
        {
            Reader? res = db.Readers.FirstOrDefault(p => p.ReaderId == id);
            if (res == null) return false;
            bool result = DoAction(delegate ()
            {
                res.ReaderFullName = model.ReaderFullName;
                res.ReaderAddress = model.ReaderAddress;
                res.ReaderPhone = model.ReaderPhone;
                db.SaveChanges();
            });
            return result;
        }

        public async Task<IEnumerable<Reader>> GetPage(int? page)
        {
            return await db.Readers.OrderBy(p=>p.ReaderId).
                Skip(((page??1)-1)*pageSize).
                Take(pageSize).ToListAsync();
        }

        public async Task<bool> HasLoans(int readerId)
        {
            return await db.Loans.AnyAsync(l => l.ReaderId == readerId);
        }
    }
}
