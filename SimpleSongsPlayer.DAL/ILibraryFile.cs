using System;

namespace SimpleSongsPlayer.DAL
{
    public interface ILibraryFile
    {
        string LibraryFolder { get; set; }
        string Path { get; set; }
        DateTime ChangeDate { get; set; }
    }
}