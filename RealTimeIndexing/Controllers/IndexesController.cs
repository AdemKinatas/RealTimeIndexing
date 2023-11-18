using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeIndexing.Services.ElasticSearch;

namespace RealTimeIndexing.Controllers
{
    [Route("api/v1/indexes")]
    [ApiController]
    public class IndexesController : ControllerBase
    {
        private readonly NorthwindContext _context;
        private readonly IElasticsearchService _elasticsearchService;

        public IndexesController(NorthwindContext context, IElasticsearchService elasticsearchService)
        {
            _context = context;
            _elasticsearchService = elasticsearchService;
        }

        [HttpGet("indexproducts")]
        public async Task IndexProducts()
        {
            await _elasticsearchService.CheckIndex("products");

            var indexProducts = await _elasticsearchService.GetDocuments("products");

            if (!indexProducts.Any())
            {
                var products = await _context.Products.ToListAsync();

                await _elasticsearchService.InsertDocuments("products", products);
            }
        }
    }
}
