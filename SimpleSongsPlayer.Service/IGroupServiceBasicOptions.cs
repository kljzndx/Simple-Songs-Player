using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Service
{
    public interface IGroupServiceBasicOptions<TName, TValue>
    {
        Task AddRange(TName name, IEnumerable<TValue> value);
        Task AddRange(TName name, IEnumerable<string> key);

        Task RemoveGroup(TName name);
        Task RemoveRange(TName name, IEnumerable<TValue> value);
    }
}