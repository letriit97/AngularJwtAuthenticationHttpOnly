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

            _services.SetTokensInsideCookie(result.Token, HttpContext);
            return Ok(result);
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Xoá cookies
            _services.RemoveTokenInCookie(HttpContext);
            return Ok();
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            HttpContext.Request.Cookies.TryGetValue("X-Access-Token", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken);

            var tokenDto = new TokenDto(accessToken, refreshToken);
            var tokenDtoToReturn = await _services.RefreshToken(tokenDto);

            if (!tokenDtoToReturn.IsSuccess)
                return Unauthorized(tokenDtoToReturn.Message);

            _services.SetTokensInsideCookie(tokenDtoToReturn.Token, HttpContext);
            return Ok();
        }
    }
}