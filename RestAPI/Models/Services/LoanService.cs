using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Abstractions;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services
{
    public class LoanService : AbstractionService, ICommonService<Loan, int>
    {
        private const int pageSize = 10;
        private readonly LibraryDbContext db;
        public LoanService(LibraryDbContext _db)
        {
            this.db = _db;
        }
        public bool Create(Loan model)
        {
            bool result = DoAction(delegate ()
            {
                db.Loans.Add(model);
                db.SaveChanges();
            });
            return result;
        }

        public bool Delete(int id)
        {
            Loan? res = db.Loans.FirstOrDefault(p => p.LoanId == id);
            if (res == null) return false;
            bool result = DoAction(delegate ()
            {
                db.Loans.Remove(res);
                db.SaveChanges();
            });
            return result;
        }

        public async Task<Loan> Get(int id)
        {
            Loan? loan = await db.Loans.FirstOrDefaultAsync(p => p.LoanId == id);
            return loan!;
        }

        public async Task<IEnumerable<Loan>> GetAll()
        {
            return await db.Loans.ToListAsync();
        }

        public bool Update(int id, Loan model)
        {
            Loan? res = db.Loans.FirstOrDefault(p => p.LoanId == id);
            if (res == null) return false;
            bool result = DoAction(delegate ()
            {
                res.ReaderId = model.ReaderId;
                res.BookCipher = model.BookCipher;
                res.DateOfIssue = model.DateOfIssue;
                res.Signature = model.Signature;
                db.SaveChanges();
            });
            return result;
        }
        public async Task<IEnumerable<Loan>> GetPage(int? page)
        {
            return await db.Loans.OrderBy(p=>p.LoanId).
                Skip(((page??1)-1)*pageSize).
                Take(pageSize).ToListAsync();
        }
    }
}
