using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public class QueryParser<TEntity> where TEntity : class
    {
        public Expression ParseFilter(string query)
        {
            throw new NotImplementedException();
        }
    }
}
