using MediatR;

namespace Geex.Common.Abstraction.Auditing
{
    public class SubmitRequest<T> : IRequest<Unit>
    {
        public SubmitRequest(string[] ids)
        {
            this.Ids = ids;
        }

        public SubmitRequest(string id)
        {
            this.Ids = new[] { id };
        }

        public string[] Ids { get; set; }
    }
}