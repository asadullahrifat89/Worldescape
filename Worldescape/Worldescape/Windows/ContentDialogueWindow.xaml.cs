using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Worldescape
{
    public partial class ContentDialogueWindow : ChildWindow
    {
        readonly Action<bool> _result;

        public ContentDialogueWindow(string title, string message, Action<bool> result = null)
        {
            InitializeComponent();
            _result = result;
            Title = title;
            TextBlock_Message.Text = message;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _result?.Invoke(true);
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _result!.Invoke(false);
            this.DialogResult = false;
        }
    }
}

