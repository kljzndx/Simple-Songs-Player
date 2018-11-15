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
    /// <typeparam name="Context">上下文类型</typeparam>
    /// <typeparam name="TableModel">表模型</typeparam>
    public class ContextHelper<Context, TableModel> where Context : DbContext, new() where TableModel : class
    {
        private static readonly PropertyInfo TableInfo = GetTableInfo();

        static ContextHelper()
        {
            using (var db = Activator.CreateInstance<Context>())
                db.Database.Migrate();
        }

        public TableModel Find(params object[] primaryKeyValues)
        {
            TableModel result = null;

            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                result = table.Find(primaryKeyValues);
            }

            return result;
        }

        public List<TableModel> ToList()
        {
            List<TableModel> result = null;

            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                result = table.ToList();
            }

            return result;
        }
        
        public void Add(TableModel data) => AddRange(new[] {data});

        public void AddRange(IEnumerable<TableModel> data)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                table.AddRange(data);
                db.SaveChanges();
            }
        }

        public void Remove(TableModel data) => RemoveRange(new[] {data});

        public void RemoveRange(IEnumerable<TableModel> data)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                table.RemoveRange(data);
                db.SaveChanges();
            }
            
        }

        public void Update(TableModel data) => UpdateRange(new[] {data});

        public void UpdateRange(IEnumerable<TableModel> data)
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
            var tableInfo = typeof(Context).GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x.PropertyType == typeof(DbSet<TableModel>));
            if (tableInfo is null)
                throw new Exception("当前上下文类型中没有该表");

            return tableInfo;
        }
    }
}