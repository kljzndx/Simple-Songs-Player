namespace SimpleSongsPlayer.ViewModels.Arguments
{
    public abstract class PageArgumentsBase
    {
        protected PageArgumentsBase(string title)
        {
            Title = title;
        }

        public string Title { get; }
    }
}