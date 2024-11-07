using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PFMS_MI04.Models.Authentication
{
    public class JwtService
    {
        private string secureKey = "your_very_long_and_secure_key_here_at_least_16_chars";

        public string Generate(string id, string accRole)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secureKey));
            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var header = new JwtHeader(credentials);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, id),
                new Claim(JwtRegisteredClaimNames.Sid, accRole)  // Add the role as a claim
            };

            var payload = new JwtPayload(
                issuer: null,
                audience: null,
                claims: claims,
                notBefore: null,
                expires: DateTime.UtcNow.AddHours(2),
                issuedAt: DateTime.UtcNow
            );

            var securityToken = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }
        public JwtSecurityToken Verify(string jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secureKey);
            tokenHandler.ValidateToken(jwt, new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false
            }, out SecurityToken validatedToken);

            return (JwtSecurityToken)validatedToken;
        }

        public string GetClaimFromToken(string jwt, string claimType)
        {
            try
            {
                if (string.IsNullOrEmpty(jwt))
                {
                    return null;
                }
                var token = this.Verify(jwt);
                if (token == null)
                {
                    return null;
                }
                var claim = token.Claims.FirstOrDefault(c => c.Type == claimType);
                if (claim == null)
                {
                    return null;
                }
                return claim.Value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Helper methods to get specific claims
        public string getToken(string jwt) => GetClaimFromToken(jwt, JwtRegisteredClaimNames.Sub);
        public string GetRole(string jwt) => GetClaimFromToken(jwt, JwtRegisteredClaimNames.Sid);
    }
}
