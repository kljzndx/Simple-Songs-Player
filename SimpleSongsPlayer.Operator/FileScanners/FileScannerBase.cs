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
        protected readonly QueryOptions QueryOptions;

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

            QueryOptions = new QueryOptions(CommonFileQuery.OrderByName, _targetTypes);
        }

        public virtual async Task<List<StorageFile>> ScanFiles(StorageFolder folder)
        {
            var query = folder.CreateFileQuery(CommonFileQuery.OrderByName);
            query.ApplyNewQueryOptions(QueryOptions);
            return (await query.GetFilesAsync()).ToList();
        }
    }
}