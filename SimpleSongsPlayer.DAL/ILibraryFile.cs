﻿using System;

namespace SimpleSongsPlayer.DAL
{
    public interface ILibraryFile
    {
        string FileName { get; set; }
        string LibraryFolder { get; set; }
        string Path { get; set; }
        DateTime ChangeDate { get; set; }
    }
}