namespace API.Utilities
{
    public static class JwtCookiesSecurity
    {
        public static void SetTokensInsideCookie(string accessToken, string refreshToken, HttpContext context)
        {
            SetCookie("X-Access-Token", accessToken, context);
            SetCookie("X-Refresh-Token", refreshToken, context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <param name="accessToken"></param>
        private static void SetCookie(string key, string value, HttpContext context, bool accessToken = true)
        {
            context.Response.Cookies.Append(key, value,
            new CookieOptions
            {
                Expires = accessToken ? DateTime.Now.AddMinutes(30) : DateTime.Now.AddDays(7),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
        }
    }
}