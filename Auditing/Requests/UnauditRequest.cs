using MediatR;

namespace Geex.Common.Abstraction.Auditing
{
    public record UnauditRequest<T>(params string[] Ids) : IRequest<Unit>;
}