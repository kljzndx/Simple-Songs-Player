using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Service
{
    public interface IDataService<TData> where TData : class
    {
        event EventHandler<IEnumerable<TData>> DataAdded;
        event EventHandler<IEnumerable<TData>> DataRemoved;

        Task<List<TData>> GetData();
    }
}