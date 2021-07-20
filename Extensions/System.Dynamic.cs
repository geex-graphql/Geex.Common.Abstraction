using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    public static class SystemTypeExtensions
    {
        public static bool IsDynamic(this Type type) => typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);
        public static List<TPropertyType> GetPropertiesOfType<TPropertyType>(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(p => type.IsAssignableFrom(p.PropertyType))
                .Select(pi => (TPropertyType)pi.GetValue(null))
                .ToList();
        }
        private static Dictionary<Type, PropertyInfo[]> _typeCache = new();
        /// <summary>
        /// Create a dictionary from the given object (<paramref name="obj"/>).
        /// </summary>
        /// <param name="obj">Source object.</param>
        /// <returns>Created dictionary.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (obj is IDictionary<string, object> already)
            {
                return already;
            }

            var type = obj.GetType();
            if (!_typeCache.TryGetValue(type, out var props))
            {
                props = type.GetProperties();
            }

            return props.ToDictionary(x => x.Name, x => x.GetValue(obj, null));
        }
    }

}
