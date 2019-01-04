using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public interface IDataServer<TOutput, TOption>
    {
        bool IsInit { get; }
        ObservableCollection<TOutput> Data { get; }

        event EventHandler<IEnumerable<TOption>> DataAdded;
        event EventHandler<IEnumerable<TOption>> DataRemoved;
    }
}