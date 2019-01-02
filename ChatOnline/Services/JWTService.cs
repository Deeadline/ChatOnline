using ChatOnline.Interface;
using ChatOnline.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatOnline.Services
{
    public class JWTService : IJWTService
    {
        public string Generate(User user)
        {
            var claims = new[] { new Claim("user", JsonConvert.SerializeObject(user)) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("babababababababababababa"));
            var signInCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var header = new JwtHeader(signInCredentials);

            var payload = new JwtPayload("", "", claims, null, DateTime.Now.AddHours(5));

            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(secToken);
        }
    }
}
