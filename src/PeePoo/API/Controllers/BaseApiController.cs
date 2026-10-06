using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        /// <summary>
        /// Maps a handler result to HTTP: null → 404, success → 200 (204 for Unit), failure → 400 with { message }.
        /// </summary>
        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null)
                return NotFound(new { message = "Not found." });
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            if (result.Value is Unit)
                return NoContent();
            if (result.Value == null)
                return NotFound(new { message = "Not found." });
            return Ok(result.Value);
        }
    }
}
