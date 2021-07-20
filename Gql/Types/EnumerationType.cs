using System;
using System.Linq;
using Geex.Common.Abstractions;

using HotChocolate.Configuration;
using HotChocolate.Language;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;

namespace Geex.Common.Gql.Types
{
    public class EnumerationType<TEnum, TValue> : EnumType<TEnum> where TEnum : IEnumeration
        where TValue : IEquatable<TValue>, IComparable<TValue>
    {
        //protected override EnumTypeDefinition CreateDefinition(ITypeDiscoveryContext context)
        //{
        //    var definition = base.CreateDefinition(context);

        //    if (!definition.Values.Any())
        //    {
        //        definition.Values.Push(new EnumValueDefinition()
        //        {
        //            Name = "_",
        //            Value = null,
        //            DeprecationReason = "_"
        //        });
        //    }

        //    return definition;
        //}
    }

    //public class EnumerationType<TEnum, TValue> : ScalarType<TEnum> where TEnum : IEnumeration where TValue : IEquatable<TValue>, IComparable<TValue>
    //{
    //    public EnumerationType() : base(typeof(TEnum).Name)
    //    {
    //    }

    //    public override bool IsInstanceOfType(IValueNode literal)
    //    {
    //        if (literal == null)
    //        {
    //            throw new ArgumentNullException(nameof(literal));
    //        }

    //        return literal is StringValueNode
    //            || literal is IntValueNode
    //            || literal is NullValueNode;
    //    }

    //    public override object? ParseLiteral(IValueNode valueSyntax, bool withDefaults = true)
    //    {
    //        if (valueSyntax == null)
    //        {
    //            throw new ArgumentNullException(nameof(valueSyntax));
    //        }

    //        if (valueSyntax is StringValueNode stringLiteral)
    //        {
    //            dynamic value = stringLiteral.Value;
    //            return Enumeration.FromValue(value);
    //        }

    //        throw new SerializationException("", new EnumerationType<TEnum, TValue>());
    //    }

    //    public override IValueNode ParseValue(object value)
    //    {
    //        if (value is TEnum s)
    //        {
    //            return new StringValueNode(s.ToString());
    //        }

    //        throw new SerializationException("", new EnumerationType<TEnum, TValue>());
    //    }

    //    public override IValueNode ParseResult(object? resultValue)
    //    {
    //        return this.ParseValue(resultValue);
    //    }

    //    public override object Serialize(object value)
    //    {
    //        if (value is TEnum s)
    //        {
    //            return s;
    //        }

    //        throw new SerializationException("", new EnumerationType<TEnum, TValue>());
    //    }

    //    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    //    {
    //        try
    //        {
    //            resultValue = this.Serialize(runtimeValue);
    //            return true;
    //        }
    //        catch (Exception)
    //        {
    //            resultValue = null;
    //            return false;
    //        }
    //    }

    //    public override bool TryDeserialize(object? serialized, out object value)
    //    {
    //        dynamic dynamicValue = serialized;
    //        if (serialized is string str)
    //        {
    //            value = Enumeration.FromValue(dynamicValue);
    //            return true;
    //        }

    //        if (serialized is TEnum)
    //        {
    //            value = serialized;
    //            return true;
    //        }

    //        value = null;
    //        return false;
    //    }
    //}
}
