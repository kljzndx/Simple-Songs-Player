using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicGroupers
{
    public class MusicPathGrouper : IMusicGrouper
    {
        public IEnumerable<MusicFileGroup> Group(IEnumerable<MusicFileDTO> source)
        {
            var groups = source.GroupBy(fileDto =>
            {
                List<string> relative = fileDto.FilePath.Replace(fileDto.LibraryFolderPath, String.Empty).Split('\\').ToList();
                string folderName = fileDto.LibraryFolderPath.Split('\\').Last();
                relative.Remove(relative.Last());
                return $"{folderName}{String.Join("\\", relative)}";
            });
            foreach (var group in groups)
                yield return new MusicFileGroup(group.Key, group);
        }
    }
}