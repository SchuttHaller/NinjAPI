using NinjAPI.Common;
using NinjAPI.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace NinjAPI.Query
{
    public class QueryParser
    {
        /// <summary>
        /// Maps properties names from the entity in List request's 'orderby' field
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static IEnumerable<OrderKey> ParseOrderBy(string source)
        {
            // Examples: 
            // orderby=Rating
            // orderby=Rating asc
            // orderby=Rating/Name desc
            var result = new List<OrderKey>();

            if (string.IsNullOrWhiteSpace(source))
            {
                 return result;
            }
            // split order options '/'
            string[] options = source.Split('/');

            foreach (var option in options)
            {
                string[] keyDir = option.Trim().Split(' ');
                string key = keyDir[0];
                string dir = keyDir.Length > 1 ? keyDir[1] : string.Empty;
                result.Add(new OrderKey { Property = key, Direction = dir, Alias = key });
            }

            return result;
        }

        /// <summary>
        /// Maps properties names from the entity in List request's 'filter' field
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static IEnumerable<FilterKey[]> ParseFilter(string source)
        {

            // Examples:
            // filter=Price gt 20
            // filter=Address eq 'Redmond'
            // Price le 200 and Price gt 3.5

            var result = new List<FilterKey[]>();
            if (string.IsNullOrWhiteSpace(source)) return result;

            Regex regex = new Regex(@"(.+?)\s+(eq|ne|gt|ge|lt|le|lk)\s+('?(.+?)+'?)");
            // split by 'and' then by 'or'
            result = source.Split(new string[] { " and " }, StringSplitOptions.None)
                .Select(x => x.Split(new string[] { " or " }, StringSplitOptions.None))
                .Select(x => x.Select(y => regex.Match(y))
                   .Where(match => match.Groups[1].Length > 0 && match.Groups[2].Length > 0 && match.Groups[3].Length > 0)
                   .Select(match => new FilterKey()
                   {
                       Property = match.Groups[1].Value,
                       LogicalOperator = match.Groups[2].Value,
                       Value = match.Groups[3].Value,
                       Alias = match.Groups[1].Value
                   })
                   .ToArray())
                .Where(x => x.Length > 0) // avoid empty expression.
                .ToList();

            return result;
        }
        
        /// <summary>
        /// Maps property names from the entity in List request's 'orderby' field in case of needed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyMap"></param>
        /// <returns></returns>
        private static IEnumerable<OrderKey> MapProps(IEnumerable<OrderKey> source, Dictionary<string, string> propertyMap)
        {
            foreach (OrderKey key in source) key.Property = propertyMap.ContainsKey(key.Property) ? propertyMap[key.Property] : key.Property;
            return source;
        }

        /// <summary>
        /// Maps property names from the entity from List request's 'filter' field in case of needed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyMap"></param>
        /// <returns></returns>
        private static IEnumerable<FilterKey[]> MapProps(IEnumerable<FilterKey[]> source, Dictionary<string, string> propertyMap)
        {
            foreach (FilterKey[] keys in source)
                foreach (FilterKey key in keys)
                    key.Property = key.Property = propertyMap.ContainsKey(key.Property) ? propertyMap[key.Property] : key.Property;
            return source;
        }


        /// <summary>
        /// Create and validate a new instance of <see cref="QueryData"/> from a query and context.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <returns></returns>
        public static QueryData CreateAndValidateQuery(HttpActionContext actionContext, string defaultOrder)
        {
            return CreateAndValidateQuery(actionContext, defaultOrder, TypeHelper.GetEntityTypeFromActionReturnType(actionContext.ActionDescriptor));
        }

        /// <summary>
        /// Create and validate a new instance of <see cref="QueryData"/> from a query and context.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <returns></returns>
        public static QueryData CreateAndValidateQuery(HttpActionContext actionContext, string defaultOrder, Type EntityType)
        {
            var queryParameters = actionContext.Request.GetQueryStrings();

            QueryData queryData = new QueryData
            {
                Filter = queryParameters["Filter"],
                FilterKeys = ParseFilter(queryParameters["Filter"]), // add alias
                OrderBy = queryParameters["Orderby"],
                OrderKeys = ParseOrderBy(queryParameters["Orderby"]),
                EntityType = EntityType
            };

            if (int.TryParse(queryParameters["PageSize"], out int pageSize))
            {
                queryData.PageSize = pageSize;
            }

            if (int.TryParse(queryParameters["Page"], out int page))
            {
                queryData.Page = page;
            }

            queryData.paginate = queryData.PageSize > 0;

            if (queryData.paginate && queryData.OrderKeys.Count() == 0)
                queryData.OrderKeys = ParseOrderBy(GenerateSafeDafaultOrder(defaultOrder, queryData.EntityType));

            QueryValidator.Validate(queryData, actionContext.ModelState);

            return queryData;
        }

        private static string GenerateSafeDafaultOrder(string defaultOrder, Type type)
        {

            //let default order provided in attr pass, if is not valid later in validation the error will be returned
            if (!string.IsNullOrWhiteSpace(defaultOrder))
                return defaultOrder;

            PropertyInfo key;

            // By Convention
            key = type.GetProperties().FirstOrDefault(p =>
                p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase)
                || p.Name.Equals(type.Name + "ID", StringComparison.OrdinalIgnoreCase)
                || p.Name.Equals("ID" + type.Name, StringComparison.OrdinalIgnoreCase));

            if (key != null)
                return key.Name;

            //By [KeyAttribute]
            key = type.GetProperties().FirstOrDefault(p =>
                p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)));

            return key?.Name;
        }
    }
}
