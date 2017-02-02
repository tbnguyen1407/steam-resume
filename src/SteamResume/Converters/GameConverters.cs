using SteamResume.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SteamResume.Converters
{
    class GameTypeBrushConverter : IValueConverter
    {
        private static SolidColorBrush brushApp = new SolidColorBrush(Colors.DarkOrange);
        private static SolidColorBrush brushDemo = new SolidColorBrush(Colors.DarkBlue);
        private static SolidColorBrush brushGame = new SolidColorBrush(Colors.DarkGray);
        private static SolidColorBrush brushNone = new SolidColorBrush(Colors.Black);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((GameType)value)
            {
                case GameType.App: return brushApp;
                case GameType.Demo: return brushDemo;
                case GameType.Game: return brushGame;
                default: return brushNone;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class GameTimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToString("MMM dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
