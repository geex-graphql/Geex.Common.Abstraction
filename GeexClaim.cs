using System.Security.Claims;
using Geex.Common.Abstractions.Enumerations;

namespace Geex.Common.Abstractions
{
    public class GeexClaim : Claim
    {
        protected GeexClaim(Claim other) : base(other)
        {
        }

        public GeexClaim(GeexClaimType type, string value) : base(type, value)
        {
        }
    }
}