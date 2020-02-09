using NinjAPI.Common;
using NinjAPI.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace NinjAPI.Query
{
    public class QueryValidator
    {
        public static void Validate(QueryData queryData, ModelStateDictionary modelState)
        {
            List<KeyValuePair<string, string>> errors = new List<KeyValuePair<string, string>>();

            if (queryData.Page < 1)
                errors.Add(
                    KeyValuePair.Create("Page", ErrorHelper.Format(Resources.ArgumentMustBeGreaterThanOrEqualTo, 1))
                    );


            if (queryData.Page > 1 && queryData.PageSize < 1)
                errors.Add(
                    KeyValuePair.Create("PageSize", ErrorHelper.Format(Resources.ArgumentMustBeGreaterThanOrEqualTo, 1))
                    );

            errors.AddRange(FilterValidation(queryData));
            errors.AddRange(OrderByValidation(queryData));

            if (errors.Count > 0)
            {
                queryData.IsValid = false;
                foreach (var error in errors)
                    modelState.AddModelError(error.Key, error.Value);
            }
            else
                queryData.IsValid = true;        
        }

        private static List<KeyValuePair<string, string>> FilterValidation(QueryData queryData)
        {
            List<KeyValuePair<string, string>> errors = new List<KeyValuePair<string, string>>();
            Type entityType = queryData.EntityType;

            foreach(FilterKey key in queryData.FilterKeys.SelectMany(f => f))
            {
                if (entityType.HasProperty(key.Property))
                {
                    PropertyInfo property = entityType.GetPropertyNoCase(key.Property);
                    if (key.Value.CouldChangeType(property.PropertyType))
                    {
                        if (!property.PropertyType.ValidOperator(key.LogicalOperator))
                        {

                            errors.Add(
                                KeyValuePair.Create("Filter", ErrorHelper.Format(Resources.InvalidOperator, key.LogicalOperator, key.Alias, property.PropertyType))
                                );
                        }
                    }
                    else
                    {
                        errors.Add(
                            KeyValuePair.Create("Filter", ErrorHelper.Format(Resources.ValueWithNoMatchingType, key.Value, key.Alias, property.PropertyType))
                            );
                    }
                }
                else
                {
                    errors.Add(
                        KeyValuePair.Create("Filter", ErrorHelper.Format(Resources.PropertyDoesNotFoundInType, key.Alias, entityType))
                        );
                }
            }

            return errors;
        }

        private static List<KeyValuePair<string, string>> OrderByValidation(QueryData queryData)
        {
            List<KeyValuePair<string, string>> errors = new List<KeyValuePair<string, string>>();

            if(queryData.paginate && queryData.OrderKeys.Count() == 0)
                errors.Add(
                        KeyValuePair.Create("OrderBy", ErrorHelper.Format(Resources.OrderByCannotBeNullOrEmptyIfPagination))
                        );

            foreach (OrderKey key in queryData.OrderKeys)
            {
                if (!queryData.EntityType.HasProperty(key.Property))
                    errors.Add(
                        KeyValuePair.Create("OrderBy", ErrorHelper.Format(Resources.PropertyDoesNotFoundInType, key.Property, queryData.EntityType))
                        );
            }

            return errors;
        }
    }
}
