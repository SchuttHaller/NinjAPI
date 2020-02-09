using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;

namespace NinjAPI.Common
{
    public static class DBSetExtensions
    {
        public static PropertyInfo GetEntityPrimaryKey<TEntity>(this DbSet<TEntity> dbSet, DbContext db) where TEntity : class
        {
            var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
            ObjectSet<TEntity> set = objectContext.CreateObjectSet<TEntity>();
            var entityType = set.EntitySet.ElementType;

            // By EFKey
            var keyName = entityType.KeyMembers.FirstOrDefault()?.Name;
            if(keyName != null)
                return typeof(TEntity).GetPropertyNoCase(keyName);
         
            PropertyInfo key;
            Type type = typeof(TEntity);
            //By [KeyAttribute]
            key = type.GetProperties().FirstOrDefault(p =>
                p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)));


            // By Convention
            key = type.GetProperties().FirstOrDefault(p =>
                p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase)
                || p.Name.Equals(type.Name + "ID", StringComparison.OrdinalIgnoreCase)
                || p.Name.Equals("ID" + type.Name, StringComparison.OrdinalIgnoreCase));

            if (key != null)
                return key;

            // Last option is return first primitive so it helps to create default order
            // but gonna fails to find a single element(?)
            keyName = entityType.Properties.FirstOrDefault(p => p.IsPrimitiveType)?.Name;
            return typeof(TEntity).GetPropertyNoCase(keyName);
        }
    }
}
