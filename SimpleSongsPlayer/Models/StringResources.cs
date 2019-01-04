using Windows.ApplicationModel.Resources;

namespace SimpleSongsPlayer.Models
{
    public static class StringResources
    {
        public static readonly ResourceLoader ListStringResource = ResourceLoader.GetForCurrentView("MusicListPage");
        public static readonly ResourceLoader NotificationStringResource = ResourceLoader.GetForCurrentView("Notification");
        public static readonly ResourceLoader SorterMembersStringResource = ResourceLoader.GetForCurrentView("SorterMembers");
    }
}