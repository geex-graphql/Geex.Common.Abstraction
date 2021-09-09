using MediatR;

namespace Geex.Common.Abstraction.Auditing
{
    public record UnsubmitRequest<T>(params string[] Ids) : IRequest<Unit>;
}