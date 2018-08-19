using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleSongsPlayer.DataModel
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (field.Equals(value))
                return;

            field = value;

            OnPropertyChanged(propertyName);
        }
    }
}