using System;
using System.Collections.Generic;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public static class FavoriteAdditionNotification
    {
        public static event EventHandler<IEnumerable<MusicFileDTO>> FavoriteAdditionRequested;

        public static void RequestFavoriteAddition(IEnumerable<MusicFileDTO> source)
        {
            FavoriteAdditionRequested?.Invoke(null, source);
        }
    }
}