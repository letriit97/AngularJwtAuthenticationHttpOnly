using System.IdentityModel.Tokens.Jwt;
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
            using (var rrg = RandomNumberGenerator.Create())
            {
                rrg.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
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
                new Claim(ClaimTypes.Name, user.UserName )
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
        Task<TokenDto> RefreshToken(TokenDto tokenDto);
        void SetTokensInsideCookie(TokenDto tokenDto, HttpContext context);
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

        public async Task<TokenDto> RefreshToken(TokenDto tokenDto)
        {

            var principal = GetClaimsPrincipalFromExpiredToken(tokenDto.AccessToken);

            // Lấy thông tin Tài khoản
            var account = await _context.Accounts.Where(x => x.UserName == principal.Identity.Name).FirstOrDefaultAsync();

            // kiểm  tra refresh token || token còn hạn hay không ? 

            return CreateToken(account, populateExp: false);
        }



        public void SetTokensInsideCookie(TokenDto tokenDto, HttpContext context)
        {
            context.Response.Cookies.Append("accessToken", tokenDto.AccessToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            context.Response.Cookies.Append("refreshToken", tokenDto.RefreshToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
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

            return new AuthenticationResponse()
            {
                IsAuthSuccessfully = true,
                ErrorMessage = "Đăng nhập thành công",
                Token = _jwtHandle.CreateToken(account)
            };
        }


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
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, TokenValidationParameters, out securityToken);
            var JwtSecurityToken = securityToken as JwtSecurityToken;


            if (JwtSecurityToken is null || JwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invlid Token");
            }

            return principal;
        }

    }
}