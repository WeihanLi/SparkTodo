using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SparkTodo.API.JWT
{
    public class TokenProvider
    {
        private readonly TokenOptions _options;
        public TokenProvider(TokenOptions options)
        {
            _options = options;
        }

        public TokenEntity GenerateToken(HttpContext context, string userName)
        {
            var identity =  GetIdentity(userName);
            if (identity == null)
                return null;
            DateTime now = DateTime.UtcNow;
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64)
            };

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.ValidFor),
                signingCredentials: _options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new TokenEntity
            {
                AccessToken = encodedJwt,
                ExpiresIn = (int)_options.ValidFor.TotalSeconds
            };
            return response;
        }
        private ClaimsIdentity GetIdentity(string username)
        {
            return new ClaimsIdentity( new System.Security.Principal.GenericIdentity(username, "Token"),
                new Claim[] { new Claim(ClaimTypes.Name, username) });
        }
        public static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
