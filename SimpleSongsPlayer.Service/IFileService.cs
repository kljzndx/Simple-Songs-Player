using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Service
{
    public interface IFileService<TFile> where TFile : class
    {
        event EventHandler<IEnumerable<TFile>> FilesAdded;
        event EventHandler<IEnumerable<TFile>> FilesRemoved;
        event EventHandler<IEnumerable<TFile>> FilesUpdated;


        Task<List<TFile>> GetFiles();
    }
}