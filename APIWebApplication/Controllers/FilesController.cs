using LiteDB;
using LiteDbServices.Exceptions;
using LiteDbServices.Services.Files;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace APIWebApplication.Controllers
{
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
}
