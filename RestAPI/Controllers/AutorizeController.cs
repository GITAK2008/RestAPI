using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestAPI.Models;
using RestAPI.Models.Data;
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
        public AutorizeController()
        {
            service = new AuthService();
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(string login, string password)
        {
            if (service.Login(login, password) != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, login) };
                var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)), // время действия 2 минуты
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                return new JwtSecurityTokenHandler().WriteToken(jwt);
            }
            return BadRequest();
        }
        [HttpPost("register")]
        public async Task<ActionResult<Client>> Register([FromBody] Client client)
        {
            if(service.Register(client)) return Ok(client);
            return BadRequest();
        }
    }
}
