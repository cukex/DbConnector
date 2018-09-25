using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbConnector.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNumeric(this Type tType)
        {
            return (tType.IsPrimitive && !(
                   tType == typeof(bool)
                || tType == typeof(char)
                || tType == typeof(IntPtr)
                || tType == typeof(UIntPtr))) || (tType == typeof(decimal));
        }

        public static bool IsNullable(this Type tType)
        {
            return !tType.IsValueType || (Nullable.GetUnderlyingType(tType) != null);
        }

        public static TValue GetAttributeValue<TAttribute, TValue>(
        this Type type,
        Func<TAttribute, TValue> valueSelector)
        where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() as TAttribute;

            if (att != null)
            {
                return valueSelector(att);
            }

            return default(TValue);
        }
    }
}
