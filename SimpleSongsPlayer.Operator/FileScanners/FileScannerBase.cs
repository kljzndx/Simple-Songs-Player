using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace SimpleSongsPlayer.Operator.FileScanners
{
    public abstract class FileScannerBase
    {
        private readonly string[] _targetTypes;

        protected FileScannerBase(params string[] targetTypes)
        {
            _targetTypes = targetTypes;
        }

        public List<StorageFile> ScanFiles(IEnumerable<StorageFile> files)
        {
            return files.Where(f => _targetTypes.Contains(f.FileType.Replace(".", String.Empty))).ToList();
        }
    }
}