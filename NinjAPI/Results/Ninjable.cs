using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Results
{
    public class Ninjable<T> : INinjable<T>
    {
        public Ninjable(IQueryable<T> queryable, Func<IQueryable, IQueryable> mapper)
        {
            Queryable = queryable;
            Mapper = mapper;
        }

        public Func<IQueryable, IQueryable> Mapper { get; }

        public IQueryable<T> Queryable { get; }

        IQueryable INinjable.Queryable => this.Queryable;
    }
}
