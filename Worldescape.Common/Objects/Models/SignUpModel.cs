using System;

namespace Worldescape.Common
{
    public class SignupModel : ModelBase
    {
        private string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set
            {
                _FirstName = value;
                RaisePropertyChanged("FirstName");
            }
        }

        private string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set
            {
                _LastName = value;
                RaisePropertyChanged("LastName");
            }
        }

        private string _Email;
        public string Email
        {
            get { return _Email; }
            set
            {
                _Email = value;
                RaisePropertyChanged("Email");
            }
        }

        private string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
                RaisePropertyChanged("Password");
            }
        }

        private DateTime _DateOfBirth = DateTime.Today;
        public DateTime DateOfBirth
        {
            get { return _DateOfBirth; }
            set
            {
                _DateOfBirth = value;
                RaisePropertyChanged("DateOfBirth");
            }
        }
    }

    public class AccountModel : SignupModel
    {
        /// <summary>
        /// Id of the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Image url of the profile picture of the user.
        /// </summary>
        public string ImageUrl { get; set; }
    }
}
