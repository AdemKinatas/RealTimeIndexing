using ElasticNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeIndexing.Entities;
using RealTimeIndexing.Services.ElasticSearch;

namespace RealTimeIndexing.Controllers
{
    [Route("api/v1/products")]
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

        [HttpGet("addmanyproducts")]
        public async Task AddManyProducts()
        {
            var productList = new List<Product>()
            {
                new Product { CategoryId = 2, ProductName = "Product1", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product2", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product3", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product4", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product5", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product6", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product7", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product8", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product9", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product10", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product11", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product12", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product13", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product14", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 },
                new Product { CategoryId = 2, ProductName = "Product15", QuantityPerUnit = "11 boxes", UnitsInStock = 50, UnitPrice = 1111 }
            };
            await _context.Products.AddRangeAsync(productList);
            await _context.SaveChangesAsync();
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
            //var product = await _productElasticsearchService.GetByIdAsync(id, "products");
            var product = await _productElasticsearchService.GetByFieldAsync(p => p.ProductId, id, "products");


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
