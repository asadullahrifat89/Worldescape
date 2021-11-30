using System.ComponentModel;
using Windows.UI.Xaml;

namespace Worldescape
{
    public class PageNumber : INotifyPropertyChanged
    {
        #region Ctor
        
        public PageNumber(string number)
        {
            Number = number;
        } 

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Properties

        #region BorderThickness
        private Thickness _BorderThickness = new Thickness(0);

        public Thickness BorderThickness
        {
            get { return _BorderThickness; }
            set
            {
                _BorderThickness = value; RaisePropertyChanged("BorderThickness");
            }
        }
        #endregion

        #region Number
        private string _Number;

        public string Number
        {
            get { return _Number; }
            set
            {
                _Number = value;
                RaisePropertyChanged("Number");
            }
        }
        #endregion

        #endregion
    }
}
