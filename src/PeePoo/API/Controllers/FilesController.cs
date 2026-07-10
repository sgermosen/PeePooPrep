using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class FilesController : BaseApiController
    {
        private const long MaxFileSize = 10 * 1024 * 1024;
        private readonly IStorageManager _storageManager;

        public FilesController(IStorageManager storageManager)
        {
            _storageManager = storageManager;
        }
        [HttpPost("[action]")]
        [RequestSizeLimit(MaxFileSize * 10)]
        public async Task<IActionResult> UploadFilesToStorage([FromForm] IList<IFormFile> files)
        {
            if (files == null || files.Count == 0 || files.Any(f => !IsValid(f)))
                return BadRequest(new { message = "One or more files are empty or exceed the 10 MB limit" });

            var result = await _storageManager.UploadBlobAsync(files);
            return Ok(result);
        }

        [HttpPost("[action]")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<IActionResult> UploadFileToStorage([FromForm] IFormFile file)
        {
            if (!IsValid(file))
                return BadRequest(new { message = "File is empty or exceeds the 10 MB limit" });

            var result = await _storageManager.UploadBlobAsync(file);
            return Ok(result);
        }

        private static bool IsValid(IFormFile file)
        {
            return file != null && file.Length > 0 && file.Length <= MaxFileSize;
        }
    }
}