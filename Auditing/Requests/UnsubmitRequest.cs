using MediatR;

namespace Geex.Common.Abstraction.Auditing
{
    public record UnsubmitRequest<T>(string[] Ids) : IRequest<Unit>;
}