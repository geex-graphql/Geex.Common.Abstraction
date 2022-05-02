using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using HotChocolate;

using MediatR;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;

namespace Geex.Common.Abstraction.Storage
{
    public abstract class Entity : MongoDB.Entities.Entity, IModifiedOn, IHasId
    {
        protected DbContext DbContext
        {
            get => base.DbContext as DbContext;
            set => base.DbContext = value;
        }
        public DateTimeOffset ModifiedOn { get; set; }
        [GraphQLIgnore]

        public void AddDomainEvent(params INotification[] events)
        {
            foreach (var @event in events)
            {
                (this.DbContext as GeexDbContext)?.DomainEvents.Enqueue(@event);
            }
        }

        /// <summary>校验对象合法性, 会在对象被Attach后立即触发</summary>
        /// <param name="sp">依赖注入器, 等价于this.ServiceProvider</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        [GraphQLIgnore]
        public abstract Task<ValidationResult> Validate(IServiceProvider sp, CancellationToken cancellation = default);


        /// <summary>
        /// 用于校验对象的合法性, 会在对象被attach后立即触发
        /// </summary>
        /// <returns></returns>
        [GraphQLIgnore]
        [Obsolete("框架内部使用, 请勿调用.")]
        public async Task Validate()
        {
            var validationResult = (await this.Validate(this.ServiceProvider));
            if (validationResult != ValidationResult.Success)
            {
                throw new BusinessException(GeexExceptionType.ValidationFailed, null, $"{validationResult.ErrorMessage}{Environment.NewLine}{validationResult.MemberNames.JoinAsString(",")}");
            }
        }
    }
}
