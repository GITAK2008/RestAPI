using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RestAPI.Models;
using RestAPI.Models.Data;
using RestAPI.Models.Services;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly ILogger<BookController> logger;
        private readonly LibraryDbContext db;
        private readonly BookService service;
        private readonly IMemoryCache memoryCache;
        private const string BooksPageCacheKey = "BooksPage";

        public BookController(LibraryDbContext _db, ILogger<BookController> _logger, IMemoryCache _memoryCache)
        {
            this.db = _db;
            this.service = new BookService(this.db);
            logger = _logger;
            memoryCache = _memoryCache;
        }

        [HttpGet]
        [Produces(typeof(Book[]))]
        public async Task<ActionResult<IEnumerable<Book>>> GetPage(int? page)
        {
            int pageNumber = page ?? 1;
            string cacheKey = $"{BooksPageCacheKey}_{pageNumber}";
            if (!memoryCache.TryGetValue(cacheKey, out IEnumerable<Book>? cachedValue))
            {
                cachedValue = await service.GetPage(pageNumber);
                MemoryCacheEntryOptions cacheOptions = new()
                {
                    SlidingExpiration = TimeSpan.FromSeconds(5),
                    Size = cachedValue.ToArray().Length
                };
                memoryCache.Set(cacheKey, cachedValue, cacheOptions);
            }
            MemoryCacheStatistics? stats = memoryCache.GetCurrentStatistics();
            logger.LogInformation("Memory cache. Total hits: {TotalHits}. Estimated size: {EstimatedSize}.",
                stats?.TotalHits, stats?.CurrentEstimatedSize);
            return Ok(cachedValue ?? Enumerable.Empty<Book>());
        }

        [HttpGet("{id}")]
        [ResponseCache(Duration = 5,
            Location = ResponseCacheLocation.Any,
            VaryByHeader = "User-Agent")]
        public async Task<ActionResult<Book>> GetById(string id)
        {
            var res = await service.Get(id);
            return res == null ? NotFound(new { message = "Book not found" }) : Ok(res);
        }

        [HttpPost]
        public ActionResult<Book> Create([FromBody] Book book)
        {
            if (service.Create(book))
                return CreatedAtAction(nameof(GetById), new { id = book.BookCipher }, book);
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            if (service.Delete(id))
                return NoContent();
            return NotFound();
        }

        [HttpPut("{id}")]
        public ActionResult<Book> Update(string id, [FromBody] Book book)
        {
            if (book.BookCipher != id) return BadRequest();
            if (service.Update(id, book))
                return Ok(book);
            return NotFound();
        }
    }
}
