using System;
using System.Reflection;
using HotChocolate;
using HotChocolate.Data.Filters;
using HotChocolate.Types.Descriptors;

namespace Geex.Common
{
    internal class GeexFilterConvention : FilterConvention
    {
        /// <inheritdoc />
        public override NameString GetTypeName(Type runtimeType)
        {
            return base.GetTypeName(runtimeType);
        }
    }
}