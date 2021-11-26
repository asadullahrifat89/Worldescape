using System;
using System.ComponentModel;

namespace Worldescape.Data
{
    public class SignupModel : BaseModel
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

        private string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("LastName cannot be empty.");
                }
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
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Email cannot be empty.");
                }
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
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Password cannot be empty.");
                }
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
                if (value == null || DateTime.MinValue == value)
                {
                    throw new Exception("DateOfBirth cannot be empty or invalid.");
                }
                _DateOfBirth = value;
                RaisePropertyChanged("DateOfBirth");
            }
        }
    }

    public class AccountModel : SignupModel
    {
        public int Id { get; set; }
    }
}
