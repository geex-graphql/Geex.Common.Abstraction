using MediatR;

namespace Geex.Common.Abstraction.Auditing
{
    public class AuditRequest<T> : IRequest<Unit>
    {
        public AuditRequest(string[] ids)
        {
            this.Ids = ids;
        }
        public AuditRequest(string id)
        {
            this.Ids = new[] { id };
        }

        public string[] Ids { get; set; }
    }
}