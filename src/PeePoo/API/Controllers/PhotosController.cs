using Application.Core;
using Application.Photos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>Extra photos for a place.</summary>
    public class PhotosController : BaseApiController
    {
        [HttpPost]
        [RequestSizeLimit(PhotoRules.MaxBytes + 1024 * 1024)]
        public async Task<IActionResult> Add([FromForm] Add.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }
    }
}
