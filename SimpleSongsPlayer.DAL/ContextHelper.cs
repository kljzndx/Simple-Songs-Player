using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace SimpleSongsPlayer.DAL
{
    /// <summary>
    /// 数据库上下文帮助类
    /// </summary>
    /// <typeparam name="C">上下文类型</typeparam>
    public static class ContextHelper<C> where C : DbContext, new()
    {
        private static readonly object TableGetting_Locker = new object();

        private static readonly Dictionary<Type, PropertyInfo> AllTables = new Dictionary<Type, PropertyInfo>();
        
        public static void Add<M>(M data) where M : class => AddRange(new[] {data});

        public static void AddRange<M>(IEnumerable<M> data) where M : class
        {
            var prop = GetTableInfo<M>();

            using (var db = Activator.CreateInstance<C>())
            {
                var table = (DbSet<M>) prop.GetValue(db);
                table.AddRange(data);
                db.SaveChanges();
            }
        }

        public static void Remove<M>(M data) where M : class => RemoveRange(new[] {data});

        public static void RemoveRange<M>(IEnumerable<M> data) where M : class
        {
            var prop = GetTableInfo<M>();

            using (var db = Activator.CreateInstance<C>())
            {
                var table = (DbSet<M>) prop.GetValue(db);
                table.RemoveRange(data);
                db.SaveChanges();
            }
            
        }

        private static PropertyInfo GetTableInfo<M>() where M : class
        {
            if (!AllTables.ContainsKey(typeof(M)))
                lock (TableGetting_Locker)
                    if (!AllTables.ContainsKey(typeof(M)))
                    {
                        var prop = typeof(C).GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x is DbSet<M>);
                        if (prop is null)
                            throw new Exception("上下文中没有该表");

                        AllTables.Add(typeof(M), prop);
                    }

            return AllTables[typeof(M)];
        }
    }
}