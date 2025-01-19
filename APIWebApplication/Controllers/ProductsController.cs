using APIWebApplication.Models;
using LiteDbServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace APIWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController

        : ControllerBase
    {
        private readonly ILiteDBService<Product, Guid> _productService;
        public ProductsController(ILiteDBService<Product, Guid> productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Product product)
        {
            // Validate the product and return any validation errors
            if (product.Validate().Any())
            {
                return BadRequest(product.Validate());
            }

            // Product name is unique
            try
            {
                await _productService.EnsureIndexAsync(product => product.Name, true);
                await _productService.CreateAsync(product);
            }
            catch (LiteDB.LiteException ex) when (ex.Message.Contains("duplicate key"))
            {
                return Conflict(new { message = "A product with the same name already exists." });
            }

            return Ok(product);
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var products = await _productService.GetAllAsync();
            return products;
        }



        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("{name:alpha}")]
        public async Task<IActionResult> GetProductsByNameAsync(string name)
        {
            var product = await _productService.FindAsync(q => q.Name == name);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("search/{productName}")]
        public async Task<IActionResult> GetProductByNameAsync(string productName)
        {
            var product = await _productService.SingleOrDefaultAsync(q => q.Name == productName);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] Product product)
        {
           

            if (product.Validate().Any())
            {
                return BadRequest(product.Validate());
            }
            var updated = new Product()
            {
                Id = id,
                Colors = product.Colors,
                Name = product.Name,
                Price = product.Price
            };

            await _productService.UpdateAsync(updated);
            return NoContent();
        }

       

        [HttpGet("count")]
        public async Task<IActionResult> CountAsync()
        {
            var count = await _productService.CountAsync();
            return Ok(count);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync(int pageNumber, int pageSize)
        {
            var pagedProducts = await _productService.GetPagedAsync(pageNumber, pageSize);
            return Ok(pagedProducts);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkAsync([FromBody] IEnumerable<Product> products)
        {
            var createdProducts = await _productService.CreateBulkAsync(products);
            return Ok(createdProducts);
        }

        [HttpPut("bulk")]
        public async Task<IActionResult> UpdateBulkAsync([FromBody] IEnumerable<Product> products)
        {
            await _productService.UpdateBulkAsync(products);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var exists = await _productService.ExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            await _productService.DeleteAsync(id);
            return NoContent();
        }
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAsync()
        {
            var deleted = await _productService.ClearAsync();
            if (deleted > 0)
            {
                return Ok(deleted);
            }
             
            return BadRequest();
        }
        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteBulkAsync([FromBody] IEnumerable<Guid> ids)
        {
            await _productService.DeleteBulkAsync(ids);
            return NoContent();
        }
    }
}
