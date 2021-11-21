using System;
using Worldescape.Data;

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

        private string _ProfileImageUrl = "ms-appx:///Images/StoreLogo.png";
        public string ProfileImageUrl
        {
            get { return _ProfileImageUrl; }
            set
            {
                _ProfileImageUrl = value;
                RaisePropertyChanged("ProfileImageUrl");
            }
        }

        private string _AvatarImageUrl = "ms-appx:///Images/StoreLogo.png";
        public string AvatarImageUrl
        {
            get { return _AvatarImageUrl; }
            set
            {
                _AvatarImageUrl = value;
                RaisePropertyChanged("AvatarImageUrl");
            }
        }
    }
}
