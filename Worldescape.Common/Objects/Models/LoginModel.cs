using System;

namespace Worldescape.Common
{
    public class LoginModel : ModelBase
    {
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
    }
}
