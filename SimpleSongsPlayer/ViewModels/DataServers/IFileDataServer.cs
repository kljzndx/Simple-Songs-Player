using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public interface IFileDataServer<T>
    {
        bool IsInit { get; }
        ObservableCollection<T> Data { get; }

        event EventHandler<IEnumerable<T>> DataAdded;
        event EventHandler<IEnumerable<T>> DataRemoved;
    }
}