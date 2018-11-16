using System;
using System.Collections.Generic;

namespace SimpleSongsPlayer.Service
{
    public interface IFileService<TFile> where TFile : class
    {
        event EventHandler<IEnumerable<TFile>> FilesAdded;
        event EventHandler<IEnumerable<TFile>> FilesRemoved;


        List<TFile> GetFiles();
    }
}