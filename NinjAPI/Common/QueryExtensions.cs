using NinjAPI.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Common
{
    public static class QueryExtensions
    {
        public static INinjable<T> AsNinjable<T>(this IQueryable<T> source, Func<IQueryable, IQueryable> mapper)
        {
            return new Ninjable<T>(source, mapper);
        }
    }
}
