using API.DTOs;
using API.Extensions;
using API.Services;
using Application.Profiles;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [EnableRateLimiting("auth")]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private const string InvalidCredentials = "Email or password is incorrect";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService _tokenService;
        private readonly IMediator _mediator;
        private readonly IMemoryCache _cache;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, TokenService tokenService, IMediator mediator, IMemoryCache cache)
        {
            _cache = cache;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (user == null)
                return Unauthorized(new { message = InvalidCredentials });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
                return Unauthorized(new { message = "This account is locked. Try again later or contact support." });
            if (!result.Succeeded)
                return Unauthorized(new { message = InvalidCredentials });

            return await CreateUserObject(user);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email.Trim()) != null)
                return BadRequest(new { message = "That email is already registered. Try signing in." });
            if (await _userManager.FindByNameAsync(dto.Username.Trim()) != null)
                return BadRequest(new { message = "That username is taken." });

            var user = new ApplicationUser
            {
                DisplayName = dto.DisplayName.Trim(),
                Email = dto.Email.Trim(),
                UserName = dto.Username.Trim()
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Errors.FirstOrDefault()?.Description ?? "Could not create the account" });

            return await CreateUserObject(user);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
            if (user == null) return Unauthorized();
            return await CreateUserObject(user);
        }

        /// <summary>Changes the password, signs out every other session and returns a fresh token.</summary>
        [Authorize]
        [HttpPost("password")]
        public async Task<ActionResult<UserDto>> ChangePassword(ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var mismatch = result.Errors.Any(e => e.Code == "PasswordMismatch");
                return BadRequest(new { message = mismatch ? "Your current password is incorrect" : result.Errors.First().Description });
            }

            _cache.InvalidateSessionCache(user.Id);
            return await CreateUserObject(user);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new DeleteAccount.Command());
            if (userId != null) _cache.InvalidateSessionCache(userId);
            if (result == null) return NotFound(new { message = "Account not found" });
            return result.IsSuccess ? NoContent() : BadRequest(new { message = result.Error });
        }

        private async Task<UserDto> CreateUserObject(ApplicationUser user)
        {
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                IsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
            };
        }
    }
}
