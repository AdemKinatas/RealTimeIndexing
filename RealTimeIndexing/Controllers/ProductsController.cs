using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeIndexing.Entities;
using RealTimeIndexing.Services.ElasticSearch;

namespace RealTimeIndexing.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly NorthwindContext _context;
        private readonly IElasticsearchService<Product> _productElasticsearchService;

        public ProductsController(NorthwindContext context, IElasticsearchService<Product> productElasticsearchService)
        {
            _context = context;
            _productElasticsearchService = productElasticsearchService;
        }

        [HttpGet("getproducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productElasticsearchService.GetAllAsync("products");
            return products.ToList();
        }

        [HttpGet("getproductbyid/{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _productElasticsearchService.GetByIdAsync(id, "products");

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPost("add")]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
