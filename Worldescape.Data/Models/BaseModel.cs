using System.ComponentModel;

namespace Worldescape.Data
{
    public class BaseModel : INotifyPropertyChanged 
    {
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
