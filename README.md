# LiteDB Services

This project is a .NET Core that demonstrates CRUD (Create, Read, Update, Delete) operations using LiteDB, a lightweight, serverless database for .NET.
It also includes file storage capabilities, allowing users to upload, download, and manage files within the LiteDB database, asynchronous operations and validations.

## Features

- Generic repository pattern for managing entities
- File storage management, upload and download functionality
- In-memory caching
- Asynchronous operations
- Validation using data annotations

## Technologies Used

- .NET 9.0
- LiteDB

## Program.cs

```
// Add LiteDb Services
builder.Services.AddLiteDBServices(Path.Combine("Data", "products.db"));
```
## Add Lite Entities Models: 

extends **LiteEntity<ObjectId | Guid | int | long>**
 
```
public class Product : LiteEntity<Guid>
 {
     [Required]
     [StringLength(100, MinimumLength = 3)]
     public string Name { get; set; } = string.Empty;

     [Range(0.01, double.MaxValue)]
     public decimal Price { get; set; }

     [EnsureMinimumElements(1)]
     public List<string> Colors { get; set; } = []
 }
```

##  ILiteDBService<T, TKey>

```
**Methods:**

- GetAllAsync(): Gets all entities.
- CreateAsync(T entity): Creates a new entity.
- UpdateAsync(T entity): Updates an existing entity.
- DeleteAsync(TKey id): Deletes an entity by its ID.
- ExistsAsync(TKey id): Checks if an entity exists by its ID.
- CountAsync(): Counts the number of entities.
- GetPagedAsync(int pageNumber, int pageSize): Gets a paginated list of entities.
- FindAsync(Expression<Func<T, bool>> predicate): Finds entities based on a predicate.
- FirstOrDefaultAsync(Expression<Func<T, bool>> predicate): Gets the first entity that matches a predicate.
- SingleOrDefaultAsync(Expression<Func<T, bool>> predicate): Gets a single entity that matches a predicate.
- CreateBulkAsync(IEnumerable<T> entities): Creates multiple entities.
- UpdateBulkAsync(IEnumerable<T> entities): Updates multiple entities.
- DeleteBulkAsync(IEnumerable<TKey> ids): Deletes multiple entities by their IDs.
- EnsureIndexAsync(Expression<Func<T, object>> indexExpression, bool unique): Ensures an index on a field.
- IncludeAsync<TInclude>(Expression<Func<T, TInclude>> includeExpression): Includes related entities.
- QueryAsync(): Queries the collection.
```

##  IFileService interface:
```
**Methods:**

- ListFilesAsync(): Lists all files using the repository.
- ListFilesAsync(Expression<Func<LiteFileInfo<string>, bool>> predicate): Lists files based on a predicate using the repository.
- GetFileInfoAsync(string fileName): Gets information about a specific file using the repository.
- UploadFileAsync(IFormFile formFile): Uploads a file using the repository.
- DownloadFileAsync(string fileName): Downloads a file using the repository.
- DeleteFileAsync(string fileName): Deletes a file using the repository.
- Storage : IFileStorage object
```
## IFileService Example:
```
 [ApiController]
 [Route("api/[controller]")]
 public class FilesController : ControllerBase
 {
     private readonly IFileService _fileService;

     public FilesController(IFileService fileService)
     {
         _fileService = fileService;
     }

     [HttpGet]
     public async Task<ActionResult<IEnumerable<FileInfo>>> GetAll()
     {
          var files =  await _fileService.ListFilesAsync();
         return Ok(files);
     }
      
     [HttpGet("{fileName}")]
     public async Task<ActionResult<FileInfo>> GetById(string fileName)
     {
         try
         {
             var file = await _fileService.GetFileInfoAsync(fileName);
             return Ok(file);
         }
         catch (EntityNotFoundException)
         {
             return NotFound();
         }
     }

     [HttpPost]
     public async Task<ActionResult<int>> Upload(IFormFile file)
     {
         if (file == null || file.Length == 0)
         {
             return BadRequest("No file uploaded.");
         }

         try
         {

             var fileInfo = await _fileService.UploadFileAsync(file);
             return CreatedAtAction(nameof(GetById), new { fileName = fileInfo.Filename }, fileInfo);
         }
         catch (ValidationException ex)
         {
             return BadRequest(ex.Message);
         }
     }

     [HttpGet("download/{fileName}")]
     public async Task<IActionResult> Download(string fileName)
     {
         try
         {
             var (contents, contentType, name) = await _fileService.DownloadFileAsync(fileName);
             return File(contents, contentType, name);
         }
         catch (EntityNotFoundException)
         {
             return NotFound();
         }
     }

     [HttpDelete("{fileName}")]
     public async Task<IActionResult> Delete(string fileName)
     {
         try
         {
             await _fileService.DeleteFileAsync(fileName);
             return NoContent();
         }
         catch (EntityNotFoundException)
         {
             return NotFound();
         }
     }
 }

```

## ILiteDBService Example:

```
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
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
```
