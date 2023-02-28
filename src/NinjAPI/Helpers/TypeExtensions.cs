using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Helpers
{
    internal static class TypeExtensions
    {
        internal static bool TryChangeType(this object obj, Type newType, out object newTypedObj)
        {
            newTypedObj = default!;
            newType = Nullable.GetUnderlyingType(newType) ?? newType;

            try
            {
                if(newType == typeof(Guid))
                {
                    if(Guid.TryParse(obj.ToString(), out Guid guid))
                    {
                        newTypedObj = guid;
                        return true;
                    };
                    return false;
                }
                newTypedObj = Convert.ChangeType(obj, newType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static Type GetNullableType(this Type type)
        {
            // Use Nullable.GetUnderlyingType() to remove the Nullable<T> wrapper if type is already nullable.
            type = Nullable.GetUnderlyingType(type) ?? type; // avoid type becoming null
            if (type.IsValueType)
                return typeof(Nullable<>).MakeGenericType(type);
            else
                return type;
        }

        internal static object? GetNullValueForType(this Type type)
        {
            var targetType = type.GetNullableType();
            return Convert.ChangeType(null, targetType);
        }
    }
}
