using System;
using System.Collections.Generic;

namespace SimpleSongsPlayer.Service
{
    public interface IObservableDataService<TData> : IDataService<TData> where TData:class
    {
        event EventHandler<IEnumerable<TData>> DataUpdated;
    }
}