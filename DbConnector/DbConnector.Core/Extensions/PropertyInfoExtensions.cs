using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace System.Reflection
{
    public static class PropertyInfoExtensions
    {
        public static bool IsNullable(this PropertyInfo pInfo)
        {
            return !pInfo.PropertyType.IsValueType || (Nullable.GetUnderlyingType(pInfo.PropertyType) != null);
        }

        public static string GetColumnAttributeName(this PropertyInfo pInfo)
        {
            ColumnAttribute cAttr = pInfo.GetCustomAttribute<ColumnAttribute>();

            if (cAttr != null)
            {
                return cAttr.Name;
            }
            else
            {
                return pInfo.Name;
            }
        }
    }
}
