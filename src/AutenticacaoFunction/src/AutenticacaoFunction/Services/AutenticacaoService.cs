using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutenticacaoFunction.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AutenticacaoFunction.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string _jwtSecret;
        private readonly int _jwtExpirationHours;

        public AuthenticationService(AppConfig config)
        {
            _jwtSecret = config.JwtSecret;
            _jwtExpirationHours = config.JwtExpirationHours;
        }

        public string GerarJwt(Guid clienteId, string cpf, string nome)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, clienteId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("cpf", cpf),
                new Claim(ClaimTypes.Name, nome)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtExpirationHours),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}