using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RestAPI.Models;
using RestAPI.Models.Data;
using RestAPI.Models.Services;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReaderController : ControllerBase
    {
        private readonly ILogger<ReaderController> logger;
        private readonly LibraryDbContext db;
        private readonly ReaderService service;
        private readonly IMemoryCache memoryCache;
        private const string ReadersPageCacheKey = "ReadersPage";

        public ReaderController(LibraryDbContext _db, ILogger<ReaderController> _logger, IMemoryCache _memoryCache)
        {
            this.db = _db;
            this.service = new ReaderService(this.db);
            logger = _logger;
            memoryCache = _memoryCache;
        }

        [HttpGet]
        [Produces(typeof(Reader[]))]
        public async Task<ActionResult<IEnumerable<Reader>>> GetPage(int? page)
        {
            int pageNumber = page ?? 1;
            string cacheKey = $"{ReadersPageCacheKey}_{pageNumber}";
            if (!memoryCache.TryGetValue(cacheKey, out IEnumerable<Reader>? cachedValue))
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
            return Ok(cachedValue ?? Enumerable.Empty<Reader>());
        }

        [HttpGet("{id:int}")]
        [ResponseCache(Duration = 5,
            Location = ResponseCacheLocation.Any,
            VaryByHeader = "User-Agent")]
        public async Task<ActionResult<Reader>> GetById(int id)
        {
            var res = await service.Get(id);
            return res == null ? NotFound(new { message = "Reader not found" }) : Ok(res);
        }

        [HttpPost]
        public ActionResult<Reader> Create([FromBody] Reader reader)
        {
            if (service.Create(reader))
                return CreatedAtAction(nameof(GetById), new { id = reader.ReaderId }, reader);
            return BadRequest();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (await service.HasLoans(id))
                return Conflict(new { message = "Reader has loans and cannot be deleted" });
            if (service.Delete(id))
                return NoContent();
            return NotFound();
        }

        [HttpPut("{id:int}")]
        public ActionResult<Reader> Update(int id, [FromBody] Reader reader)
        {
            if (reader.ReaderId != id) return BadRequest();
            if (service.Update(id, reader))
                return Ok(reader);
            return NotFound();
        }
    }
}
