using System;

namespace Worldescape.Data
{
    public class LoginModel : BaseModel
    {
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
    }
}
