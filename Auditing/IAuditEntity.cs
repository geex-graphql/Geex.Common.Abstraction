using System;
using System.Threading.Tasks;

using Geex.Common.Abstraction.Auditing.Events;
using Geex.Common.Abstractions;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Entities;

namespace Geex.Common.Abstraction.Auditing
{
    public interface IAuditEntity : IEntity
    {
        /// <summary>
        /// 对象审批状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        async Task SubmitAsync<TEntity>()
        {
            if (this.Submittable)
            {
                this.AuditStatus |= AuditStatus.Submitted;
                await this.DbContext.ServiceProvider.GetRequiredService<IMediator>().Publish(new EntitySubmittedNotification<TEntity>(this));
            }
            else
            {
                throw new BusinessException(GeexExceptionType.ValidationFailed, message: "不满足上报条件, 无法上报.");
            }
        }

        async Task AuditAsync<TEntity>()
        {
            if (this.AuditStatus.HasFlag(AuditStatus.Submitted))
            {
                this.AuditStatus |= AuditStatus.Audited;
                await this.DbContext.ServiceProvider.GetRequiredService<IMediator>().Publish(new EntityAuditedNotification<TEntity>(this));
            }
        }

        async Task UnsubmitAsync<TEntity>()
        {
            if (this.AuditStatus.HasFlag(AuditStatus.Submitted))
            {
                this.AuditStatus ^= AuditStatus.Submitted;
                await this.DbContext.ServiceProvider.GetRequiredService<IMediator>().Publish(new EntityUnsubmittedNotification<TEntity>(this));
            }
            else
            {
                throw new BusinessException(GeexExceptionType.ValidationFailed, message: "不满足取消上报条件, 无法上报.");
            }
        }

        async Task UnauditAsync<TEntity>()
        {
            if (this.AuditStatus.HasFlag(AuditStatus.Audited))
            {
                this.AuditStatus ^= AuditStatus.Audited;
                await this.DbContext.ServiceProvider.GetRequiredService<IMediator>().Publish(new EntityUnauditedNotification<TEntity>(this));
            }
        }

        bool Submittable { get; }
    }
}
