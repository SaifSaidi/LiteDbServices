# LiteDB Services with File Storage

This project is a .NET Core that demonstrates CRUD (Create, Read, Update, Delete) operations using LiteDB, a lightweight, serverless database for .NET.
It also includes file storage capabilities, allowing users to upload, download, and manage files within the LiteDB database.

## Features

- Generic CRUD operations for entities
- File upload and download functionality
- In-memory caching for improved performance
- Asynchronous operations for better scalability
- Validation using data annotations

## Technologies Used

- .NET 9.0
- LiteDB

## Program.cs

```
// Add LiteDb Services
builder.Services.AddLiteDBServices(Path.Combine("Data", "products.db"));
```

## Controllers:
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
     public async Task<ActionResult<IEnumerable<File>>> GetAll()
     {
         var files = await _fileService.GetAllAsync();
         return Ok(files);
     }

     [HttpGet("{id}")]
     public async Task<ActionResult<File>> GetById(string id)
     {
         try
         {
             var nid = new ObjectId(id);
             var file = await _fileService.GetByIdAsync(nid);
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
             var objectId = await _fileService.UploadFileAsync(file);

             //var objectId = fileInfo.Metadata.AsObjectId.ToString();
             //Console.WriteLine(fileInfo.Metadata.AsObjectId.ToString());
             return CreatedAtAction(nameof(GetById), new { id = objectId.ToString() }, objectId.ToString());
         }
         catch (ValidationException ex)
         {
             return BadRequest(ex.Message);
         }
     }

     [HttpGet("download/{id}")]
     public async Task<IActionResult> Download(string id)
     {
         try
         {
             var nid = new ObjectId(id);
             var (fileContents, contentType, fileName) = await _fileService.DownloadFileAsync(nid);
             return File(fileContents, contentType, fileName);
         }
         catch (EntityNotFoundException)
         {
             return NotFound();
         }
     }

     [HttpDelete("{id}")]
     public async Task<IActionResult> Delete(string id)
     {
         try
         {
             await _fileService.DeleteAsync(new ObjectId(id));
             return NoContent();
         }
         catch (EntityNotFoundException)
         {
             return NotFound();
         }
     }
 }
```
```
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILiteDBService<Product> _productService;

    public ProductsController(ILiteDBService<Product> productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    { 
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(string id)
    {
        var nid = new ObjectId(id);
        try
        {
            var product = await _productService.GetByIdAsync(nid);
            return Ok(product);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("name/{name}")]
    public async Task<ActionResult<Product>> Search(string name)
    {
        try
        {
            var product = await _productService.GetOneByQueryAsync(q=>q.Name.Contains(name));
            return Ok(product);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<ObjectId>> Create(ProductDto input)
    { 
        try
        {
            var product = new Product()
            {
                Colors = input.Colors,
                Name = input.Name,
                Price = input.Price
            };
               var id = await _productService.CreateAsync(product);
     
            return  CreatedAtAction(nameof(GetById), new { id }, id.ToString());
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] Product product)
    {
        var nid = new ObjectId(id);
        if (nid != product.Id)
        {
            return BadRequest("Id mismatch");
        }

        try
        {
            await _productService.UpdateAsync(product);
            return NoContent();
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var nid = new ObjectId(id);
        try
        {
            await _productService.DeleteAsync(nid);
            return NoContent();
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
    }
}
