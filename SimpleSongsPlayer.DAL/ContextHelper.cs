using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace SimpleSongsPlayer.DAL
{
    /// <summary>
    /// 数据库上下文帮助类
    /// </summary>
    /// <typeparam name="Context">上下文类型</typeparam>
    public static class ContextHelper<Context> where Context : DbContext, new()
    {
        private static readonly object TableGetting_Locker = new object();

        private static readonly Dictionary<Type, PropertyInfo> AllTables = new Dictionary<Type, PropertyInfo>();
        
        public static void Add<TableModel>(TableModel data) where TableModel : class => AddRange(new[] {data});

        public static void AddRange<TableModel>(IEnumerable<TableModel> data) where TableModel : class
        {
            var prop = GetTableInfo<TableModel>();

            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) prop.GetValue(db);
                table.AddRange(data);
                db.SaveChanges();
            }
        }

        public static void Remove<TableModel>(TableModel data) where TableModel : class => RemoveRange(new[] {data});

        public static void RemoveRange<TableModel>(IEnumerable<TableModel> data) where TableModel : class
        {
            var prop = GetTableInfo<TableModel>();

            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) prop.GetValue(db);
                table.RemoveRange(data);
                db.SaveChanges();
            }
            
        }

        public static void Update<TableModel>(TableModel data) where TableModel : class => UpdateRange(new[] {data});

        public static void UpdateRange<TableModel>(IEnumerable<TableModel> data) where TableModel : class
        {
            var prop = GetTableInfo<TableModel>();

            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) prop.GetValue(db);
                table.UpdateRange(data);
                db.SaveChanges();
            }
        }

        private static PropertyInfo GetTableInfo<TableModel>() where TableModel : class
        {
            if (!AllTables.ContainsKey(typeof(TableModel)))
                lock (TableGetting_Locker)
                    if (!AllTables.ContainsKey(typeof(TableModel)))
                    {
                        var prop = typeof(Context).GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x is DbSet<TableModel>);
                        if (prop is null)
                            throw new Exception("上下文中没有该表");

                        AllTables.Add(typeof(TableModel), prop);
                    }

            return AllTables[typeof(TableModel)];
        }
    }
}