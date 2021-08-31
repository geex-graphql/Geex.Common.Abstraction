using MediatR;

namespace Geex.Common.Abstraction.Auditing
{
    public record UnauditRequest<T>(string[] Ids) : IRequest<Unit>;
}