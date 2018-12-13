using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Service
{
    public interface IGroupServiceBasicOptions<TKey>
    {
        Task AddRange(string name, IEnumerable<TKey> keys);

        Task RemoveGroup(string name);
        Task RemoveRange(string name, IEnumerable<TKey> keys);

        Task RenameGroup(string oldName, string newName);
    }
}