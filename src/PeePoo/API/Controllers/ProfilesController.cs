using Application.Moderation;
using Application.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class ProfilesController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Username = username }));
        }

        [HttpPut]
        public async Task<IActionResult> EditProfile(Edit.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }

        /// <summary>Usernames the signed-in user has blocked.</summary>
        [HttpGet("blocked")]
        public async Task<IActionResult> Blocked()
        {
            return HandleResult(await Mediator.Send(new ListBlocked.Query()));
        }

        [HttpPost("{username}/block")]
        public async Task<IActionResult> Block(string username)
        {
            return HandleResult(await Mediator.Send(new BlockUser.Command { Username = username }));
        }

        [HttpDelete("{username}/block")]
        public async Task<IActionResult> Unblock(string username)
        {
            return HandleResult(await Mediator.Send(new UnblockUser.Command { Username = username }));
        }
    }
}
