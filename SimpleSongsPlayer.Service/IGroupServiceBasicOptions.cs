using System.Collections.Generic;

namespace SimpleSongsPlayer.Service
{
    public interface IGroupServiceBasicOptions<TName, TValue>
    {
        void AddRange(TName name, IEnumerable<TValue> value);

        void RemoveGroup(TName name);
        void RemoveRange(TName name, IEnumerable<TValue> value);
    }
}