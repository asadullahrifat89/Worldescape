﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Worldescape
{
    public class TextToPlaceholderTextVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                if (string.IsNullOrEmpty((string)value))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            throw new InvalidOperationException("The IValueConverter expects a value of type String.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
