using Identity.API.Models;
using Identity.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JWTTokenService _jwtTokenService;
        public AuthController(JWTTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }
        [HttpPost]
        public IActionResult Login([FromBody] LoginModel user)
        {
            var loginResult = _jwtTokenService.GenerateAuthToken(user);
            return loginResult is null ? Unauthorized() : Ok(loginResult);
        }
    }
}