using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Service
{
    public interface IGroupServiceBasicOptions<TKey, TValue>
    {
        Task AddRange(string name, IEnumerable<TValue> value);
        Task AddRange(string name, IEnumerable<TKey> keys);

        Task RemoveGroup(string name);
        Task RemoveRange(string name, IEnumerable<TValue> value);
        Task RemoveRange(string name, IEnumerable<TKey> keys);
        Task RemoveRangeInAllGroup(IEnumerable<TValue> files);

        Task RenameGroup(string oldName, string newName);
    }
}