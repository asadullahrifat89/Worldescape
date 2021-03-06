using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Worldescape.Common
{
    public class AvatarMessenger : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Properties

        #region SourceAvatar

        public Avatar Avatar { get; set; }

        #endregion

        #region ActivityStatus
        //public ActivityStatus ActivityStatus
        //{
        //    get { return Avatar.ActivityStatus; }
        //    set
        //    {
        //        Avatar.ActivityStatus = value; RaisePropertyChanged("ActivityStatus");
        //    }
        //}
        #endregion

        #region IsTyping
        //private bool _IsTyping;

        //public bool IsTyping
        //{
        //    get { return _IsTyping; }
        //    set
        //    {
        //        _IsTyping = value; RaisePropertyChanged("IsTyping");
        //    }
        //}
        #endregion

        #region IsLoggedIn

        public bool IsLoggedIn { get; set; }

        #endregion

        #region HasSentNewMessage
        private bool _HasSentNewMessage;

        public bool HasSentNewMessage
        {
            get { return _HasSentNewMessage; }
            set
            {
                _HasSentNewMessage = value; RaisePropertyChanged("HasSentNewMessage");
            }
        }
        #endregion

        #region Message
        //private string _Message;

        //public string Message
        //{
        //    get { return _Message; }
        //    set
        //    {
        //        _Message = value;
        //        RaisePropertyChanged("Message");
        //    }
        //}
        #endregion

        #region UnreadMessagesCount
        //private int _UnreadMessagesCount;

        //public int UnreadMessagesCount
        //{
        //    get { return _UnreadMessagesCount; }
        //    set { _UnreadMessagesCount = value; RaisePropertyChanged("UnreadMessagesCount"); }
        //}
        #endregion

        #region Chatter
        
        private ObservableCollection<ChatMessage> _Chatter = new();

        public ObservableCollection<ChatMessage> Chatter
        {
            get { return _Chatter; }
            set { _Chatter = value; RaisePropertyChanged("Chatter"); }
        }

        #endregion

        #endregion
    }
}
