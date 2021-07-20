using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Geex.Common.Abstraction
{
    public class GeexJwtSecurityTokenHandler : ISecurityTokenValidator
    {
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public GeexJwtSecurityTokenHandler()
        {
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; } = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;

        public bool CanReadToken(string securityToken)
        {
            return _tokenHandler.CanReadToken(securityToken);
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var principal = _tokenHandler.ValidateToken(securityToken, validationParameters, out validatedToken);
            //if (validatedToken.ValidFrom > DateTime.Now && validatedToken.ValidTo < DateTime.Now)
            //{
            //    throw new SecurityTokenExpiredException($"token is only valid in {validatedToken.ValidFrom} ~ {validatedToken.ValidTo}");
            //}
            //var subClaim = principal.Claims.First(c => c.Type == GeexClaimType.Sub);
            //var expireClaim = principal.Claims.First(x => x.Type == GeexClaimType.Expires);

            return principal;
        }
    }

}
