using System.Threading.Tasks;
using Geex.Common.Abstractions;
using MongoDB.Entities;

namespace Geex.Common.Abstraction.Auditing
{
    public interface IAuditEntity : IEntity
    {
        /// <summary>
        /// 对象审批状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        Task SubmitAsync()
        {
            if (this.Submittable)
            {
                this.AuditStatus |= AuditStatus.Submitted;
            }
            else
            {
                throw new BusinessException(GeexExceptionType.ValidationFailed, message:"不满足上报条件, 无法上报.");
            }
            return Task.CompletedTask;
        }

        Task AuditAsync()
        {
            if (this.AuditStatus.HasFlag(AuditStatus.Submitted))
            {
                this.AuditStatus |= AuditStatus.Audited;
            }
            return Task.CompletedTask;
        }

        bool Submittable { get; }
    }
}
