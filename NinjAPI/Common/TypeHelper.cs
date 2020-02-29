using NinjAPI.Properties;
using NinjAPI.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace NinjAPI.Common
{
    public static class TypeHelper
    {
        /// <summary>
        /// Indicates whether a Type contains a property by its name
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        public static bool HasProperty(this Type obj, string propertyName)
        {
            return obj.GetPropertyNoCase(propertyName) != null;
        }

        /// <summary>
        /// Return a property by nameb in IgnoreCase mode
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        public static PropertyInfo GetPropertyNoCase(this Type obj, string propertyName)
        {
            return obj.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Indicates if an object can be casted to another type
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        public static bool CouldChangeType(this object obj, Type type)
        {
            try
            {
                Type t = Nullable.GetUnderlyingType(type) ?? type;
                var o = Convert.ChangeType(obj, t);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Indicates if an object can be casted to another type
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        public static bool TryCast(object obj, Type type, out object result)
        {
            try
            {
                Type t = Nullable.GetUnderlyingType(type) ?? type;
                result = Convert.ChangeType(obj, t);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Common available operators for filter in a QueryData
        /// </summary>
        private static readonly string[] commonOperators = new string[] { "eq", "ne", "gt", "ge", "lt", "le" };

        /// <summary>
        /// Allowed operators for queries in a QueryData
        /// </summary>
        private static Dictionary<Type, string[]> operatorsByType = new Dictionary<Type, string[]>()
            {
                { typeof(decimal), commonOperators },
                { typeof(int), commonOperators },
                { typeof(long), commonOperators },
                { typeof(double), commonOperators },
                { typeof(byte), commonOperators },
                { typeof(DateTime),commonOperators },
                { typeof(bool), new string[] { "eq", "ne"} },
                { typeof(string), new string[] { "eq", "ne", "lk"} },
            };

        /// <summary>
        /// Indicates if an operator is valid for a type (applied for filter in QueryData)
        /// </summary>
        /// <param name="op"></param>
        /// <param name="type"></param>
        public static bool ValidOperator(this Type type, string op)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;
            return operatorsByType.Any(x => x.Key == t && x.Value.Contains(op));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetPropValue(this object src, string propName)
        {
            return src.GetType()?.GetPropertyNoCase(propName)?.GetValue(src, null);
        }

        /// <summary>
        /// Determine if a type is a generic type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a generic type; false otherwise.</returns>
        public static bool IsGenericType(this Type clrType)
        {
            return clrType.IsGenericType;
        }

        /// <summary>
        /// Determine if a type is an interface.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an interface; false otherwise.</returns>
        public static bool IsInterface(Type clrType)
        {
            return clrType.IsInterface;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        private static Type GetInnerGenericType(Type interfaceType)
        {
            // Getting the type T definition if the returning type implements IEnumerable<T>
            Type[] parameterTypes = interfaceType.GetGenericArguments();

            if (parameterTypes.Length == 1)
            {
                return parameterTypes[0];
            }

            return null;
        }

        /// <summary>
        /// Returns type of T if the type implements IEnumerable of T, otherwise, return null.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetImplementedIEnumerableType(Type type)
        {
            // get inner type from Task<T>
            if (TypeHelper.IsGenericType(type))
            {
                Type genericType = type.GetGenericTypeDefinition();
                if(genericType == typeof(Task<>))
                {
                    type = type.GetGenericArguments().First();
                } 
            }
                
            if (TypeHelper.IsGenericType(type) && TypeHelper.IsInterface(type) &&
                (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 type.GetGenericTypeDefinition() == typeof(IQueryable<>) ||
                 type.GetGenericTypeDefinition() == typeof(INinjable<>)))
            {
                // special case the IEnumerable<T>
                return GetInnerGenericType(type);
            }
            else
            {
                // for the rest of interfaces and strongly Type collections
                Type[] interfaces = type.GetInterfaces();
                foreach (Type interfaceType in interfaces)
                {
                    if (TypeHelper.IsGenericType(interfaceType) &&
                        (interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                         interfaceType.GetGenericTypeDefinition() == typeof(IQueryable<>)))
                    {
                        // special case the IEnumerable<T>
                        return GetInnerGenericType(interfaceType);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public static Type GetEntityTypeFromActionReturnType(HttpActionDescriptor actionDescriptor)
        {
            // It is a developer programming error to use this binding attribute
            // on actions that return void.
            if (actionDescriptor.ReturnType == null)
            {
                throw ErrorHelper.InvalidOperation(
                                Resources.FailedToBuildQueryBecauseReturnTypeIsNull,
                                actionDescriptor.ActionName,
                                actionDescriptor.ControllerDescriptor.ControllerName);
            }

            Type entityClrType = TypeHelper.GetImplementedIEnumerableType(actionDescriptor.ReturnType);

            if (entityClrType == null)
            {
                // It is a developer programming error to use this binding attribute
                // on actions that return a collection whose element type cannot be
                // determined, such as a non-generic IQueryable or IEnumerable.
                throw ErrorHelper.InvalidOperation(
                                Resources.FailedToRetrieveTypeToBuildModel,
                                actionDescriptor.ActionName,
                                actionDescriptor.ControllerDescriptor.ControllerName,
                                actionDescriptor.ReturnType.FullName);
            }

            return entityClrType;
        }
    }
}
