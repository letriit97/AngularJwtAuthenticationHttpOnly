using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API._Services
{

    public record TokenDto(string AccessToken, string RefreshToken);

    public class AuthenticationResponse
    {
        public bool IsAuthSuccessfully { get; set; }
        public string ErrorMessage { get; set; }
        public TokenDto Token { get; set; }
    }

    public class RefreshTokenResponse
    {
        public bool IsSuccess { get; set; }



        public string Message { get; set; }
        public TokenDto Token { get; set; }

        public RefreshTokenResponse(bool isSuccess, string message = null)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public RefreshTokenResponse()
        {

        }

        public RefreshTokenResponse(TokenDto token)
        {
            Token = token;
        }
    }
    public class JwtHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _jwtSetting;

        public JwtHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSetting = _configuration.GetSection("JwtSettings");
        }

        public TokenDto CreateToken(Accounts user)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = GetClaims(user);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            return new TokenDto(new JwtSecurityTokenHandler().WriteToken(tokenOptions), GenerateRefreshToken());
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rrg = RandomNumberGenerator.Create();
            rrg.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }


        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwtSetting["SecretKey"]);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256Signature);
        }

        private List<Claim> GetClaims(Accounts user)
        {
            var claims = new List<Claim>(){
                new (ClaimTypes.Name, user.UserName )
            };

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {

            var tokenOptions = new JwtSecurityToken(
                issuer: _jwtSetting["ValidIssuer"],
                audience: _jwtSetting["ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSetting["Exprired"])),
                signingCredentials: signingCredentials
            );
            return tokenOptions;
        }
    }


    public interface IAuthenticationServices
    {
        Task<AuthenticationResponse> ValidateUser(AccountDto user);
        TokenDto CreateToken(Accounts user, bool populateExp);
        Task<RefreshTokenResponse> RefreshToken(TokenDto tokenDto);

        Task RevokeToken(string userName);

        void SetTokensInsideCookie(TokenDto tokenDto, HttpContext context);
        void RemoveTokenInCookie(HttpContext context);
    }

    public class AuthenticationService : IAuthenticationServices
    {
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtHandler _jwtHandle;

        public AuthenticationService(DBContext context, IConfiguration configuration, JwtHandler jwtHandle)
        {
            _context = context;
            _configuration = configuration;
            _jwtHandle = jwtHandle;
        }

        public TokenDto CreateToken(Accounts user, bool populateExp)
        {
            // tạo token
            var token = _jwtHandle.CreateToken(user);
            // tạo refresh token
            return token;
        }

        public async Task<RefreshTokenResponse> RefreshToken(TokenDto tokenDto)
        {
            var account = new Accounts();
            // Nếu có Access Token 
            if (string.IsNullOrWhiteSpace(tokenDto.AccessToken))
            {
                // Lấy thông tin từ refresh Token
                var refr = await _context.RefreshToken.FirstOrDefaultAsync(x => x.Token == tokenDto.RefreshToken);
                if (refr == null || refr.ExpiredTime < DateTime.Now) 
                    return new RefreshTokenResponse(false, "ExpiredTimeCookie");
                account = await _context.Accounts.Where(x => x.UserName == refr.UserName).FirstOrDefaultAsync();
            }
            else
            {
                var principal = GetClaimsPrincipalFromExpiredToken(tokenDto.AccessToken);

                // Lấy thông tin Tài khoản
                account = await _context.Accounts.Where(x => x.UserName == principal.Identity.Name).FirstOrDefaultAsync();
            }

            // kiểm  tra refresh token || token còn hạn hay không ?
            if (account == null) return new RefreshTokenResponse(false, "Unthorize");

            // Lấy thông tin Token
            var token = CreateToken(account, populateExp: false);
            return new RefreshTokenResponse(token) { IsSuccess = true };
        }


        public void RemoveTokenInCookie(HttpContext context)
        {
            context.Request.Cookies.TryGetValue("X-Access-Token", out var accessToken);
            context.Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken);
            if (!string.IsNullOrWhiteSpace(accessToken))
                context.Response.Cookies.Delete("X-Access-Token");
            if (!string.IsNullOrWhiteSpace(refreshToken))
                context.Response.Cookies.Delete("X-Refresh-Token");
        }

        /// <summary>
        /// Thu hồi token
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Task RevokeToken(string userName)
        {
            throw new NotImplementedException();
        }

        public void SetTokensInsideCookie(TokenDto tokenDto, HttpContext context)
        {
            context.Response.Cookies.Append("X-Access-Token", tokenDto.AccessToken,
            new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(5),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            context.Response.Cookies.Append("X-Refresh-Token", tokenDto.RefreshToken,
            new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
        }

        public async Task<AuthenticationResponse> ValidateUser(AccountDto user)
        {
            var account = await _context.Accounts.Where(x => x.UserName == user.UserName).FirstOrDefaultAsync();
            if (account == null) return new AuthenticationResponse() { IsAuthSuccessfully = false, ErrorMessage = "Tài khoản không tồn tại" };

            var tokenDto = _jwtHandle.CreateToken(account);

            var refreshToken = new RefreshToken()
            {
                UserName = account.UserName,
                Token = tokenDto.RefreshToken,
                CreatedTime = DateTime.Now,
                ExpiredTime = DateTime.Now.AddDays(7)
            };

            _context.RefreshToken.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthenticationResponse()
            {
                IsAuthSuccessfully = true,
                ErrorMessage = "Đăng nhập thành công",
                Token = tokenDto
            };
        }


        /// <summary>
        /// Lấy thông tin Claim từ Access Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private ClaimsPrincipal GetClaimsPrincipalFromExpiredToken(string token)
        {
            var jwtSetting = _configuration.GetSection("JwtSettings");

            var TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSetting["ValidIssuer"],
                ValidAudience = jwtSetting["ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting["SecretKey"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, TokenValidationParameters, out SecurityToken securityToken);
            var JwtSecurityToken = securityToken as JwtSecurityToken;


            if (JwtSecurityToken is null || JwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invlid Token");

            return principal;
        }

    }
}