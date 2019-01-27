using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        public async Task CustomOption(Action<DbSet<TableModel>> action)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                action.Invoke(table);
                await db.SaveChangesAsync();
            }
        }

        public async Task CustomOption(Func<DbSet<TableModel>, Task> action)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                await action.Invoke(table);
                await db.SaveChangesAsync();
            }
        }

        public Task<TableModel> Find(params object[] primaryKeyValues)
        {
            TableModel result = null;

            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                result = table.Find(primaryKeyValues);
            }

            return Task.FromResult(result);
        }

        public async Task<List<TableModel>> ToList()
        {
            List<TableModel> result = null;

            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                result = await Task.Run((Func<List<TableModel>>) table.ToList);
            }

            return result;
        }
        
        public async Task Add(TableModel data) => await AddRange(new[] {data});

        public async Task AddRange(IEnumerable<TableModel> data)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                await table.AddRangeAsync(data);
                await db.SaveChangesAsync();
            }
        }

        public async Task Remove(TableModel data) => await RemoveRange(new[] {data});

        public async Task RemoveRange(IEnumerable<TableModel> data)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                table.RemoveRange(data);
                await db.SaveChangesAsync();
            }
        }

        public async Task Update(TableModel data) => await UpdateRange(new[] {data});

        public async Task UpdateRange(IEnumerable<TableModel> data)
        {
            using (var db = Activator.CreateInstance<Context>())
            {
                var table = (DbSet<TableModel>) TableInfo.GetValue(db);
                table.UpdateRange(data);
                await db.SaveChangesAsync();
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