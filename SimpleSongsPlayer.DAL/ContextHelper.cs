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
    /// <typeparam name="M">表模型</typeparam>
    public static class ContextHelper<C, M> where C : DbContext, new() where M : class, new()
    {
        private static readonly object TableGetting_Locker = new object();

        private static readonly Dictionary<Type, Dictionary<Type, PropertyInfo>> AllTables = new Dictionary<Type, Dictionary<Type, PropertyInfo>>();

        static ContextHelper()
        {
            if (!AllTables.ContainsKey(typeof(C)))
                AllTables.Add(typeof(C), new Dictionary<Type, PropertyInfo>());
        }

        public static void Add(M data) => AddRange(new[] {data});

        public static void AddRange(IEnumerable<M> data)
        {
            var prop = GetTableInfo();

            using (var db = Activator.CreateInstance<C>())
            {
                var table = (DbSet<M>) prop.GetValue(db);
                table.AddRange(data);
                db.SaveChanges();
            }
        }

        public static void Remove(M data) => RemoveRange(new[] {data});

        public static void RemoveRange(IEnumerable<M> data)
        {
            var prop = GetTableInfo();

            using (var db = Activator.CreateInstance<C>())
            {
                var table = (DbSet<M>) prop.GetValue(db);
                table.RemoveRange(data);
                db.SaveChanges();
            }
            
        }

        private static PropertyInfo GetTableInfo()
        {
            if (!AllTables[typeof(C)].ContainsKey(typeof(M)))
                lock (TableGetting_Locker)
                    if (!AllTables[typeof(C)].ContainsKey(typeof(M)))
                    {
                        var prop = typeof(C).GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x is DbSet<M>);
                        if (prop is null)
                            throw new Exception("上下文中没有该表");

                        AllTables[typeof(C)].Add(typeof(M), prop);
                    }

            return AllTables[typeof(C)][typeof(M)];
        }
    }
}