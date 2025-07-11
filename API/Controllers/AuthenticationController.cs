using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API._Services;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationServices _services;

        public AuthenticationController(IAuthenticationServices services)
        {
            _services = services;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Authenticate([FromBody] AccountDto user)
        {
            var result = await _services.ValidateUser(user);
            if (!result.IsAuthSuccessfully)
                return Unauthorized();
            return Ok(result);
        }



        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            HttpContext.Request.Cookies.TryGetValue("accessToken", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
            
            var tokenDto = new TokenDto(accessToken, refreshToken);
            var tokenDtoToReturn = await _services.RefreshToken(tokenDto);
            _services.SetTokensInsideCookie(tokenDtoToReturn, HttpContext);

            return Ok();
        }
    }
}