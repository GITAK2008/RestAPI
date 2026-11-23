using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Abstractions;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services
{
    public class PublisherService : AbstractionService, ICommonService<Publisher, int>
    {
        private const int pageSize = 10;
        private readonly LibraryDbContext db;
        public PublisherService(LibraryDbContext _db)
        {
            this.db = _db;
        }
        public bool Create(Publisher model)
        {
            bool result = DoAction(delegate ()
            {
                db.Publishers.Add(model);
                db.SaveChanges();
            });
            return result;
        }

        public bool Delete(int id)
        {
            Publisher? res = db.Publishers.FirstOrDefault(p => p.PublisherId == id);
            if (res == null) return false;
            bool result = DoAction(delegate ()
            {
                db.Publishers.Remove(res);
                db.SaveChanges();
            });
            return result;
        }

        public async Task<Publisher> Get(int id)
        {
            Publisher? publisher = await db.Publishers.FirstOrDefaultAsync(p => p.PublisherId == id);
            return publisher!;
        }

        public async Task<IEnumerable<Publisher>> GetAll()
        {
            return await db.Publishers.ToListAsync();
        }

        public bool Update(int id, Publisher model)
        {
            Publisher? res = db.Publishers.FirstOrDefault(p => p.PublisherId == id);
            if (res == null) return false;
            bool result = DoAction(delegate ()
            {
                res.PublisherName = model.PublisherName;
                res.PublisherCity = model.PublisherCity;
                db.SaveChanges();
            });
            return result;
        }

        public async Task<IEnumerable<Publisher>> GetPage(int? page)
        {
            return await db.Publishers.OrderBy(p=>p.PublisherId).
                Skip(((page??1)-1)*pageSize).
                Take(pageSize).ToListAsync();
        }

        public async Task<bool> HasBooks(int publisherId)
        {
            return await db.Books.AnyAsync(b => b.PublisherId == publisherId);
        }

        public async Task AddWithBook(Publisher publisher, Book book)
        {
            publisher.Books.Add(book);
            db.Publishers.Add(publisher);
            await db.SaveChangesAsync();
        }
    }
}
