namespace SimpleSongsPlayer.DAL
{
    public interface ILibraryFile
    {
        string LibraryFolder { get; set; }
        string Path { get; set; }
    }
}