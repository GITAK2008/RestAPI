using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RestAPI.Models;
using RestAPI.Models.Data;
using RestAPI.Models.Services;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> logger;
        private readonly LibraryDbContext db;
        private readonly LoanService service;
        private readonly IMemoryCache memoryCache;
        private const string LoansPageCacheKey = "LoansPage";

        public LoanController(LibraryDbContext _db, ILogger<LoanController> _logger, IMemoryCache _memoryCache)
        {
            this.db = _db;
            this.service = new LoanService(this.db);
            logger = _logger;
            memoryCache = _memoryCache;
        }

        [HttpGet]
        [Produces(typeof(Loan[]))]
        public async Task<ActionResult<IEnumerable<Loan>>> GetPage(int? page)
        {
            int pageNumber = page ?? 1;
            string cacheKey = $"{LoansPageCacheKey}_{pageNumber}";
            if (!memoryCache.TryGetValue(cacheKey, out IEnumerable<Loan>? cachedValue))
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
            return Ok(cachedValue ?? Enumerable.Empty<Loan>());
        }

        [HttpGet("{id:int}")]
        [ResponseCache(Duration = 5,
            Location = ResponseCacheLocation.Any,
            VaryByHeader = "User-Agent")]
        public async Task<ActionResult<Loan>> GetById(int id)
        {
            var res = await service.Get(id);
            return res == null ? NotFound(new { message = "Loan not found" }) : Ok(res);
        }

        [HttpPost]
        public ActionResult<Loan> Create([FromBody] Loan loan)
        {
            if (service.Create(loan))
                return CreatedAtAction(nameof(GetById), new { id = loan.LoanId }, loan);
            return BadRequest();
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            if (service.Delete(id))
                return NoContent();
            return NotFound();
        }

        [HttpPut("{id:int}")]
        public ActionResult<Loan> Update(int id, [FromBody] Loan loan)
        {
            if (loan.LoanId != id) return BadRequest();
            if (service.Update(id, loan))
                return Ok(loan);
            return NotFound();
        }
    }
}
