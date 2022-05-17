using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<ApplicationUser> userManager,
         SignInManager<ApplicationUser> signInManager, TokenService tokenService)
        {
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto dto)
        {
            var user = await _userManager.Users.Include(p => p.Photos)
        .FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Username or password is incorrect" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Username or password is incorrect" });

            return CreateUserObject(user);

        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {

            if (await _userManager.Users.AnyAsync(x => x.Email == dto.Email))
            {
                ModelState.AddModelError("email", "Email already in use");
                return ValidationProblem();
            }
            if (await _userManager.Users.AnyAsync(x => x.UserName == dto.Username))
            {
                ModelState.AddModelError("username", "Username already in use");
                return ValidationProblem();
            }

            var user = new ApplicationUser
            {
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                UserName = dto.Username
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = "Problem creating user" });

            return CreateUserObject(user);

        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.Users.Include(p => p.Photos)
             .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

            return CreateUserObject(user);
        }
        private UserDto CreateUserObject(ApplicationUser user)
        {
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                Image = user?.Photos?.FirstOrDefault(x => x.IsMain)?.Url
            };
        }
    }
}