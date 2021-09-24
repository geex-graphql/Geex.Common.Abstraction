﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstraction;
using Geex.Common.Abstraction.Auditing;
using Geex.Common.Abstraction.Bson;
using Geex.Common.Abstraction.Gql;
using Geex.Common.Abstractions;
using Geex.Common.Gql;
using Geex.Common.Gql.Roots;
using Geex.Common.Gql.Types;

using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Data;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using HotChocolate.Server;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Pagination;
using HotChocolate.Utilities;
using HotChocolate.Validation;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using StackExchange.Redis.Extensions.Core;

using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;

namespace Geex.Common
{
    public class GeexCoreModule : GeexModule<GeexCoreModule>
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient(typeof(LazyFactory<>));
            context.Services.AddTransient<ClaimsPrincipal>(x =>
                x.GetService<IHttpContextAccessor>()?.HttpContext?.User);
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSupportingBsonDateTimeSerializer());
            base.PreConfigureServices(context);
        }

        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            base.PostConfigureServices(context);
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddStorage();
            var schemaBuilder = context.Services.AddGraphQLServer();
            context.Services.AddStackExchangeRedisExtensions();
            context.Services.AddInMemorySubscriptions();
            context.Services.AddSingleton(schemaBuilder);
            context.Services.AddHttpResultSerializer<GeexResultSerializerWithCustomStatusCodes>();
            schemaBuilder.AddConvention<ITypeInspector>(typeof(ClassEnumTypeConvention))
                .AddTypeConverter((Type source, Type target, out ChangeType? converter) =>
                {
                    converter = o => o;
                    return source.GetBaseClasses(false).Intersect(target.GetBaseClasses(false)).Any();
                })
                .AddValidationVisitor<ExtraArgsTolerantValidationVisitor>()
                .AddTransactionScopeHandler<GeexTransactionScopeHandler>()
                .AddFiltering()
                .AddConvention<IFilterConvention>(new FilterConventionExtension(x => x.Provider(new GeexQueryablePostFilterProvider(y => y.AddDefaultFieldHandlers()))))
                .AddSorting()
                .AddProjections()
                .AddQueryType()
                .AddMutationType()
                .AddSubscriptionType()
                .AddCommonTypes()
                .OnSchemaError((ctx, err) => { throw new Exception("schema error", err); });
            context.Services.AddHttpContextAccessor();
            context.Services.AddObjectAccessor<IApplicationBuilder>();

            context.Services.AddHealthChecks();
            base.ConfigureServices(context);
        }
    }
}
