using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using RestAPI.Models;
using RestAPI.Models.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutorizeController : ControllerBase
    {
        private readonly AuthService service;

        public AutorizeController(AuthService service)
        {
            this.service = service;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> Login(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LoginRequest? request,
            [FromQuery] string? login,
            [FromQuery] string? password)
        {
            string? userLogin = request?.Login ?? login;
            string? userPassword = request?.Password ?? password;

            if (string.IsNullOrWhiteSpace(userLogin) || string.IsNullOrWhiteSpace(userPassword))
                return BadRequest(new { message = "Login and password are required" });

            var user = await service.Login(userLogin, userPassword);
            if (user == null)
                return Unauthorized(new { message = "Invalid login or password" });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(JwtRegisteredClaimNames.Sub, user.Login)
            };

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)), // время действия 2 минуты
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Login) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest(new { message = "Login and password are required" });

            if (await service.Register(user))
                return Ok(new { message = "User registered" });

            return Conflict(new { message = "User already exists" });
        }
    }
}
