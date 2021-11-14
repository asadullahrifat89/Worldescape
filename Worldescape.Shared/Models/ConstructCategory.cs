using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Worldescape.Shared.Entities;

namespace Worldescape.Shared.Models
{
    public class ConstructCategory : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("Name"); }
        }

        string _imageUrl;

        public string ImageUrl
        {
            get { return _imageUrl; }
            set { _imageUrl = value; RaisePropertyChanged("ImageUrl"); }
        }
    }
}
