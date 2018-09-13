using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace SimpleSongsPlayer.Operator.FileScanners
{
    public abstract class FileScannerBase
    {
        private readonly List<string> _targetTypes;

        protected FileScannerBase(params string[] targetTypes)
        {
            _targetTypes = new List<string>();

            foreach (string targetType in targetTypes)
            {
                if (targetType[0] == '.')
                    _targetTypes.Add(targetType);
                else
                    _targetTypes.Add("." + targetType);
            }
        }

        public async Task<List<StorageFile>> ScanFiles(StorageFolder folder)
        {
            var query = folder.CreateFileQuery(CommonFileQuery.OrderByName);
            query.ApplyNewQueryOptions(new QueryOptions(CommonFileQuery.OrderByName, _targetTypes));
            return (await query.GetFilesAsync()).ToList();
        }
    }
}