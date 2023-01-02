using Kuari.AuthServer.Core.DTOs;
using Kuari.AuthServer.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kuari.AuthServer.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost]
        public async  Task<IActionResult> CreateToken(LoginDto loginDto)
        {
            var result = await _authenticationService.CreateToken(loginDto);

        }
    }
}
