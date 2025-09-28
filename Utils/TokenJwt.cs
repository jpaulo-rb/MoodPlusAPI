using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MoodPlusAPI.Usuarios;
using MoodPlusAPI.Usuarios.DTOs;

namespace MoodPlusAPI.Utils
{
    public class TokenJwt
    {
        private readonly string _JwtKey;
        private readonly string _JwtIssuer;
        private readonly string _JwtAudience;
        private readonly int _ExpirationMinutes;

        public TokenJwt(IConfiguration configuration)
        {
            _JwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt", "Key não encontrada");
            _JwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt", "Issuer não encontrado");
            _JwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt", "Audience não encontrado");

            var expiration = configuration.GetValue<int>("Jwt:ExpirationMinutes");
            if (expiration == 0) throw new InvalidOperationException("Tempo de expiração do token precisa ser maior que 0");

            _ExpirationMinutes = expiration;
        }

        public UsuarioTokenDTO GerarTokenDTO(Usuario usuario)
        {
            var token = GerarToken(usuario);
            return new UsuarioTokenDTO { Type = "bearer", Token = token, Expiration = DateTime.UtcNow.AddMinutes(_ExpirationMinutes) };
        }

        public string GerarToken(Usuario usuario)
        {
            List<Claim> claims = [
                new Claim("Id", usuario.Id),
                new Claim("Email", usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Regra.ToString())
            ];

            if (usuario.EmpresaId != null)
            {
                claims.Add(new Claim("EmpresaId", usuario.EmpresaId));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(_ExpirationMinutes);

            var securityToken = new JwtSecurityToken(
                issuer: _JwtIssuer,
                audience: _JwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }
    }
}
