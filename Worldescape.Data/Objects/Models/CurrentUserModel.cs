using System;

namespace Worldescape.Data
{
    public class CurrentUserModel : BaseModel
    {
        private string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("FirstName cannot be empty.");
                }
                _FirstName = value;
                RaisePropertyChanged("FirstName");
            }
        }

        private string _ProfileImageUrl;
        public string ProfileImageUrl
        {
            get { return _ProfileImageUrl; }
            set
            {
                _ProfileImageUrl = value;
                RaisePropertyChanged("ProfileImageUrl");
            }
        }

        //private string _AvatarImageUrl;
        //public string AvatarImageUrl
        //{
        //    get { return _AvatarImageUrl; }
        //    set
        //    {
        //        _AvatarImageUrl = value;
        //        RaisePropertyChanged("AvatarImageUrl");
        //    }
        //}
    }
}
