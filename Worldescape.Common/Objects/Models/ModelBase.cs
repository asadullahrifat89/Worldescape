using System.ComponentModel;

namespace Worldescape.Common
{
    public class ModelBase : INotifyPropertyChanged 
    {
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
