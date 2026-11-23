using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Abstractions;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services
{
    public class BookService : AbstractionService, ICommonService<Book, string>
    {
        private const int pageSize = 10;
        private readonly LibraryDbContext db;
        public BookService(LibraryDbContext _db)
        {
            this.db = _db;
        }
        public bool Create(Book model)
        {
            bool result = DoAction(delegate ()
            {
                db.Books.Add(model);
                db.SaveChanges();
            });
            return result;
        }

        public bool Delete(string id)
        {
            Book? res = db.Books.FirstOrDefault(p => p.BookCipher == id);
            if (res == null) return false;
            bool result = DoAction(delegate ()
            {
                db.Books.Remove(res);
                db.SaveChanges();
            });
            return result;
        }

        public async Task<Book> Get(string id)
        {
            Book? book = await db.Books.FirstOrDefaultAsync(p => p.BookCipher == id);
            return book!;
        }

        public async Task<IEnumerable<Book>> GetAll()
        {
            return await db.Books.ToListAsync();
        }

        public bool Update(string id, Book model)
        {
            Book? res = db.Books.FirstOrDefault(p => p.BookCipher == id);
            if (res == null) return false;
            bool result = DoAction(delegate ()
            {
                res.FirstAuthor = model.FirstAuthor;
                res.BookName = model.BookName;
                res.YearOfPublication = model.YearOfPublication;
                res.BookPrice = model.BookPrice;
                res.NumberOfCopies = model.NumberOfCopies;
                res.PublisherId = model.PublisherId;
                db.SaveChanges();
            });
            return result;
        }
        public async Task<IEnumerable<Book>> GetPage(int? page)
        {
            return await db.Books.OrderBy(p=>p.BookCipher).
                Skip(((page??1)-1)*pageSize).
                Take(pageSize).ToListAsync();
        }
    }
}
