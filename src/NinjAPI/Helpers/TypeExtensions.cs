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
    }
}
