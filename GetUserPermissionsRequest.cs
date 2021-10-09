using System.Collections.Generic;
using MediatR;

namespace Geex.Common.Abstraction
{
    public class GetUserPermissionsRequest : IRequest<IEnumerable<string>>
    {
        public string UserId { get; }

        public GetUserPermissionsRequest(string userId)
        {
            UserId = userId;
        }
    }
}
