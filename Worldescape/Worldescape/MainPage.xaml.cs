using Microsoft.Extensions.Logging;
using System;
using Windows.UI.Xaml.Controls;
using Worldescape.Data;

namespace Worldescape
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

        private string _ImageUrl = null;
        public string ImageUrl
        {
            get { return _ImageUrl; }
            set
            {
                //if (string.IsNullOrWhiteSpace(value))
                //{
                //    throw new Exception("ImageUrl cannot be empty.");
                //}
                _ImageUrl = value;
                RaisePropertyChanged("ImageUrl");
            }
        }

    }

    public partial class MainPage : Page
    {
        #region Fields

        private readonly ILogger<MainPage> _logger;

        #endregion

        #region Ctor

        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;

            CurrentUserHolder.DataContext = CurrentUserModel;
        }

        #endregion

        #region Properties

        public CurrentUserModel CurrentUserModel { get; set; } = new CurrentUserModel();

        #endregion

        #region Methods

        public void SetUser(User user)
        {
            CurrentUserModel.FirstName = user.FirstName;
            CurrentUserModel.ImageUrl = user.ImageUrl;
            CurrentUserHolder.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public void LogError(Exception error)
        {
            _logger.LogError(error, error.Message);
        }

        public void NavigateToPage(Page page)
        {
            // Set target to target page:            
            PageContainer.Content = page;
        }

        public void NavigateToPage(string targetUri)
        {
            // Navigate to the target page:
            Uri uri = new Uri(targetUri, UriKind.Relative);
            PageContainer.Source = uri;
        }

        #endregion
    }
}
