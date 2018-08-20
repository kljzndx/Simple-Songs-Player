using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleSongsPlayer.Operator.Pickers
{
    public abstract class FileOpenPickerBase
    {
        private readonly FileOpenPicker picker;

        protected FileOpenPickerBase(PickerLocationId defaultPosition, params string[] fileTypes)
        {
            picker = new FileOpenPicker {SuggestedStartLocation = defaultPosition};

            foreach (var fileType in fileTypes)
                picker.FileTypeFilter.Add("." + fileType);
        }

        public async Task<StorageFile> PickFileAsync()
        {
            return await picker.PickSingleFileAsync();
        }
    }
}