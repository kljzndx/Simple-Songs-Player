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
    public static class ContextHelper<Context, TableModel> where Context : DbContext, new() where TableModel : class
    {
        private static readonly PropertyInfo TableInfo = GetTableInfo();
        
        public static void Add(TableModel data) => AddRange(new[] {data});

        public static void AddRange(IEnumerable<TableModel> data)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                table.AddRange(data);
                db.SaveChanges();
            }
        }

        public static void Remove(TableModel data) => RemoveRange(new[] {data});

        public static void RemoveRange(IEnumerable<TableModel> data)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                table.RemoveRange(data);
                db.SaveChanges();
            }
            
        }

        public static void Update(TableModel data) => UpdateRange(new[] {data});

        public static void UpdateRange(IEnumerable<TableModel> data)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                table.UpdateRange(data);
                db.SaveChanges();
            }
        }

        private static PropertyInfo GetTableInfo()
        {
            var tableInfo = typeof(Context).GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x is DbSet<TableModel>);
            if (tableInfo is null)
                throw new Exception("上下文中没有该表");

            return tableInfo;
        }
    }
}