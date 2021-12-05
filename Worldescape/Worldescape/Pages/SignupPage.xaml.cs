using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Worldescape.Common;
using Worldescape.Service;

namespace Worldescape
{
    public partial class SignupPage : Page
    {
        #region Fields

        readonly UserRepository _userRepository;
        readonly MainPage _mainPage;

        #endregion

        #region Ctor

        public SignupPage(
            //MainPage mainPage
            )
        {
            InitializeComponent();

            //_httpServiceHelper = httpServiceHelper;
            //_mainPage = mainPage;

            SignUpModelHolder.DataContext = SignUpModel;

            _userRepository = App.ServiceProvider.GetService(typeof(UserRepository)) as UserRepository;
            _mainPage = App.ServiceProvider.GetService(typeof(MainPage)) as MainPage;

        }

        #endregion

        #region Properties

        public SignupModel SignUpModel { get; set; } = new SignupModel();

        #endregion

        #region Methods

        #region Functionality

        private bool CheckIfModelValid()
        {
            if (!SignUpModel.FirstName.IsNullOrBlank()
                && !SignUpModel.LastName.IsNullOrBlank()
                && !SignUpModel.Email.IsNullOrBlank()
                && !SignUpModel.Password.IsNullOrBlank() && SignUpModel.Password.Length <= 12
                && SignUpModel.DateOfBirth != null && SignUpModel.DateOfBirth != DateTime.MinValue
                && (RadioButton_Male.IsChecked.GetValueOrDefault() || RadioButton_Female.IsChecked.GetValueOrDefault() || RadioButton_Other.IsChecked.GetValueOrDefault()))
                Button_Signup.IsEnabled = true;
            else
                Button_Signup.IsEnabled = false;

            return Button_Signup.IsEnabled;
        }

        private void NavigateToLoginPage()
        {
            _mainPage.NavigateToPage(Constants.Page_LoginPage);
        }

        private async Task SignUp()
        {
            _mainPage.SetIsBusy(true, "Creating your account...");

            var response = await _userRepository.AddUser(
                email: SignUpModel.Email,
                password: SignUpModel.Password,
                dateofbirth: SignUpModel.DateOfBirth,
                gender: RadioButton_Male.IsChecked.Value ? Gender.Male : RadioButton_Female.IsChecked.Value ? Gender.Female : RadioButton_Other.IsChecked.Value ? Gender.Other : Gender.Other,
                firstname: SignUpModel.FirstName,
                lastname: SignUpModel.LastName);

            if (!response.Success)
            {
                var contentDialogue = new ContentDialogueWindow(title: "Error!", message: response.Error);
                contentDialogue.Show();

                _mainPage.SetIsBusy(false);
            }
            else
            {
                NavigateToLoginPage();
                _mainPage.SetIsBusy(false);
            }
        }

        #endregion

        #region Page Events

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }

        #endregion

        #region UX Events

        private void Control_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            CheckIfModelValid();
        }

        private void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfModelValid();
        }

        private void PasswordBox_Pasword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }

        private void DatePicker_DateOfBirth_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckIfModelValid();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            CheckIfModelValid();
        }

        #endregion

        #region Button Events

        private async void Button_Signup_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIfModelValid())
                return;

            await SignUp();
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            NavigateToLoginPage();
        }

        #endregion

        #endregion      
    }
}
