using System;
using System.Threading.Tasks;

using Geex.Common.Abstraction.Auditing.Events;
using Geex.Common.Abstractions;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Entities;

using Entity = Geex.Common.Abstraction.Storage.Entity;

namespace Geex.Common.Abstraction.Auditing
{
    public interface IAuditEntity : IEntity
    {
        /// <summary>
        /// 对象审批状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }
        /// <summary>
        /// 审批操作备注文本
        /// </summary>
        public string AuditRemark { get; set; }

        async Task Submit<TEntity>(string? remark = default)
        {
            if (this.Submittable)
            {
                this.AuditStatus |= AuditStatus.Submitted;
                this.AuditRemark = remark;
                (this as Entity)?.AddDomainEvent(new EntitySubmittedNotification<TEntity>(this));
            }
            else
            {
                throw new BusinessException(GeexExceptionType.ValidationFailed, message: "不满足上报条件.");
            }
        }

        async Task Audit<TEntity>(string? remark = default)
        {
            if (this.AuditStatus == AuditStatus.Submitted)
            {
                this.AuditStatus |= AuditStatus.Audited;
                this.AuditRemark = remark;
                (this as Entity)?.AddDomainEvent(new EntityAuditedNotification<TEntity>(this));
            }
            else
            {
                throw new BusinessException(GeexExceptionType.ValidationFailed, message: "不满足审批条件.");
            }
        }

        async Task UnSubmit<TEntity>(string? remark = default)
        {
            if (this.AuditStatus == AuditStatus.Submitted)
            {
                this.AuditStatus ^= AuditStatus.Submitted;
                this.AuditRemark = remark;
                (this as Entity)?.AddDomainEvent(new EntityUnsubmittedNotification<TEntity>(this));
            }
            else if(this.AuditStatus == AuditStatus.Audited)
            {
                throw new BusinessException(GeexExceptionType.ValidationFailed, message: "已审核，无法取消上报.");
            }
        }

        async Task UnAudit<TEntity>(string? remark = default)
        {
            if (this.AuditStatus == AuditStatus.Audited)
            {
                this.AuditStatus ^= AuditStatus.Audited;
                this.AuditRemark = remark;
                (this as Entity)?.AddDomainEvent(new EntityUnauditedNotification<TEntity>(this));
            }
        }

        bool Submittable { get; }
    }
}
