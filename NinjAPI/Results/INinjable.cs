using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Results
{
    public interface INinjable
    {
        Func<IQueryable, IQueryable> Mapper { get; }
        IQueryable Queryable { get; }
    }
    public interface INinjable<T> : INinjable
    {
        new IQueryable<T> Queryable { get;}
    }
}
