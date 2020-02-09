using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjAPI.Query.Expressions;

namespace NinjAPI.Query
{
    public class QueryData
    {
        public bool paginate = true;

        /// <summary>
        /// 
        /// </summary>
        public Type EntityType;

        /// <summary>
        /// Default page to show
        /// </summary>
        public int Page = 1;

        /// <summary>
        /// Default page size
        /// </summary>
        public int PageSize = 0;

        /// <summary>
        /// Default page to show
        /// </summary>
        public string OrderBy;

        /// <summary>
        /// Default page size
        /// </summary>
        public string Filter;

        /// <summary>
        /// Order info
        /// </summary>
        public IEnumerable<OrderKey> OrderKeys;

        /// <summary>
        /// Filter info
        /// </summary>
        public IEnumerable<FilterKey[]> FilterKeys;

        /// <summary>
        /// Is a valid Query
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// Total Count
        /// </summary>
        public long? TotalResultCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public IQueryable ApplyTo(IQueryable queryable)
        {
            //where clause
            IQueryable result = queryable.Where(this.EntityType, this.FilterKeys);
            this.TotalResultCount = result.Count(this.EntityType)();

            //order clause
            if (OrderKeys.Count() > 0)
                result = result.OrderBy(this.EntityType, this.OrderKeys.ToArray());
     
            //pagination
           if (this.paginate)
           {
                if(this.Page > 1)
                {
                    result = result.Skip(this.EntityType, (this.Page - 1) * this.PageSize);
                }
                result = result.Take(this.EntityType, this.PageSize);
            }

            return result;
        }

        /// <summary>
        /// Create pagination links for set in ResponseHeaders
        /// </summary>
        /// <param name="request"></param>
        /// <param name="totalResults"></param>
        /// <returns></returns>
        public List<string> CreatePagingLinks(string Uri, long totalResults)
        {
            string linkFormat = "<" + Uri
               + "?page={0}&pageSize=" + this.PageSize
               + (this.OrderBy != null ? "&orderby=" + this.OrderBy : "")
               + (this.Filter != null ? "&filter=" + this.Filter : "")
               + ">; rel=\"{1}\"";

            var paginationLinks = new List<string>
            {
                string.Format(linkFormat, 1, "first")
            };
            //
            if (this.Page > 1)
                paginationLinks.Add(string.Format(linkFormat, this.Page - 1, "prev"));
            //
            if (totalResults > ((this.Page + 1) * this.PageSize))
                paginationLinks.Add(string.Format(linkFormat, this.Page + 1, "next"));
            //
            paginationLinks.Add(string.Format(linkFormat, Math.Ceiling((decimal)totalResults / this.PageSize), "last"));

            return paginationLinks;
        }

    }
}
