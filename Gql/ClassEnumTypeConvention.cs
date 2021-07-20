using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Geex.Common.Abstractions;
using HotChocolate.Types.Descriptors;

namespace Geex.Common.Gql
{
    public class ClassEnumTypeConvention : DefaultTypeInspector
    {
        protected override void Initialize(IConventionContext context)
        {
            base.Initialize(context);
        }

        public override IEnumerable<MemberInfo> GetMembers(Type type)
        {
            return base.GetMembers(type);
        }

        public override IEnumerable<object> GetEnumValues(Type enumType)
        {
            if ((object)enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            if (enumType != typeof(object) && enumType.IsEnum)
            {
                return Enum.GetValues(enumType).Cast<object>();
            }

            if (enumType.IsAssignableTo<IEnumeration>())
            {
                var genericImplementation = enumType.GetBaseClasses().FirstOrDefault(x => x.Name == (typeof(Enumeration<,>).Name));
                var values = ((System.Collections.IEnumerable)genericImplementation?.GetProperty(nameof(Enumeration.List))?.GetValue(null)).Cast<object>();
                if (!values.Any())
                {
                    Console.WriteLine("enum no values");
                }
                return values;
            }
            return Enumerable.Empty<object>();
        }

        protected override void Complete(IConventionContext context)
        {
            base.Complete(context);
        }
    }
}
