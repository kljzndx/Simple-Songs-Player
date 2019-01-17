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
                if (String.IsNullOrWhiteSpace(fileDto.ParentFolderPath))
                {
                    List<string> relativePaths = fileDto.FilePath.Replace(fileDto.LibraryFolderPath, String.Empty).Split('\\').ToList();
                    relativePaths.Remove(relativePaths.Last());
                    return String.Format("{0}{1}", fileDto.LibraryFolderName, String.Join("\\", relativePaths));
                }

                return String.Format("{0}{1}", fileDto.LibraryFolderName, fileDto.ParentFolderPath.Replace(fileDto.LibraryFolderPath, String.Empty));

            });
            foreach (var group in groups)
                yield return new MusicFileGroup(group.Key, group);
        }
    }
}