using ElasticNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeIndexing.Entities;
using RealTimeIndexing.Services.ElasticSearch;

namespace RealTimeIndexing.Controllers
{
    [Route("api/v1/indexes")]
    [ApiController]
    public class IndexesController : ControllerBase
    {
        private readonly NorthwindContext _context;
        private readonly IElasticsearchService<Product> _productElasticsearchService;

        public IndexesController(NorthwindContext context, IElasticsearchService<Product> productElasticsearchService)
        {
            _context = context;
            _productElasticsearchService = productElasticsearchService;
        }

        [HttpGet("indexproducts")]
        public async Task IndexProducts()
        {
            await _productElasticsearchService.CheckIndex("products");

            var indexProducts = await _productElasticsearchService.GetAllAsync("products");

            if (!indexProducts.Any())
            {
                var products = await _context.Products.ToListAsync();

                await _productElasticsearchService.AddManyAsync(products, "products");
            }
        }
    }
}
