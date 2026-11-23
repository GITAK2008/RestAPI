using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RestAPI.Models;
using RestAPI.Models.Data;
using RestAPI.Models.Services;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublisherController : ControllerBase
    {
        private readonly ILogger<PublisherController> logger;
        private readonly LibraryDbContext db;
        private readonly PublisherService service;
        private readonly IMemoryCache memoryCache;
        private const string PublishersPageCacheKey = "PublishersPage";

        public PublisherController(LibraryDbContext _db, ILogger<PublisherController> _logger, IMemoryCache _memoryCache)
        {
            this.db = _db;
            this.service = new PublisherService(this.db);
            logger = _logger;
            memoryCache = _memoryCache;
        }

        [HttpGet]
        [Produces(typeof(Publisher[]))]
        public async Task<ActionResult<IEnumerable<Publisher>>> GetPage(int? page)
        {
            int pageNumber = page ?? 1;
            string cacheKey = $"{PublishersPageCacheKey}_{pageNumber}";
            if (!memoryCache.TryGetValue(cacheKey, out IEnumerable<Publisher>? cachedValue))
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
            return Ok(cachedValue ?? Enumerable.Empty<Publisher>());
        }

        [HttpGet("{id:int}")]
        [ResponseCache(Duration = 5,
            Location = ResponseCacheLocation.Any,
            VaryByHeader = "User-Agent")]
        public async Task<ActionResult<Publisher>> GetById(int id)
        {
            var res = await service.Get(id);
            return res == null ? NotFound(new { message = "Publisher not found" }) : Ok(res);
        }

        [HttpPost]
        public ActionResult<Publisher> Create([FromBody] Publisher publisher)
        {
            if (service.Create(publisher))
                return CreatedAtAction(nameof(GetById), new { id = publisher.PublisherId }, publisher);
            return BadRequest();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (await service.HasBooks(id))
                return Conflict(new { message = "Publisher has books and cannot be deleted" });
            if (service.Delete(id))
                return NoContent();
            return NotFound();
        }

        [HttpPut("{id:int}")]
        public ActionResult<Publisher> Update(int id, [FromBody] Publisher publisher)
        {
            if (publisher.PublisherId != id) return BadRequest();
            if (service.Update(id, publisher))
                return Ok(publisher);
            return NotFound();
        }
    }
}
