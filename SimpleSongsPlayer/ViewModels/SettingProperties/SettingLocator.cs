namespace SimpleSongsPlayer.ViewModels.SettingProperties
{
    public class SettingLocator
    {
        public static SettingLocator Current = new SettingLocator();

        private SettingLocator()
        {
            
        }

        public PlayerSettingProperties Player => PlayerSettingProperties.Current;
        public ViewSettingProperties View => ViewSettingProperties.Current;
        public OtherSettingProperties Other => OtherSettingProperties.Current;
    }
}