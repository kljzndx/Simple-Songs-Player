using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public interface IFileDataServer<T> : IDataServer<T, T>
    {
    }
}