namespace Worldescape.Common
{
    /// <summary>
    /// Represents a construct asset.
    /// </summary>
    public class ConstructAsset //: INotifyPropertyChanged
    {
        #region PropertyChanged

        //public event PropertyChangedEventHandler PropertyChanged;

        //protected void RaisePropertyChanged([CallerMemberName] string name = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        //}

        #endregion

        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }

        //string _name;

        //public string Name
        //{
        //    get { return _name; }
        //    set { _name = value; RaisePropertyChanged("Name"); }
        //}

        //string _imageUrl;

        //public string ImageUrl
        //{
        //    get { return _imageUrl; }
        //    set { _imageUrl = value; RaisePropertyChanged("ImageUrl"); }
        //}

        //string _category;

        //public string Category
        //{
        //    get { return _category; }
        //    set { _category = value; RaisePropertyChanged("Category"); }
        //}
    }
}
