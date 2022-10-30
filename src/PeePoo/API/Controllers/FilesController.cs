using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class FilesController : BaseApiController
    {
        private readonly IStorageManager _storageManager;

        public FilesController(IStorageManager storageManager)
        {
            _storageManager = storageManager;
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFilesToStorage([FromForm] IList<IFormFile> files)
        {
            var result = await _storageManager.UploadBlobAsync(files );
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFileToStorage([FromForm] IFormFile file)
        {
            var result = await _storageManager.UploadBlobAsync(file );
            return Ok(result);
        }
    }
}