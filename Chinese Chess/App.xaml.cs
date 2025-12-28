using Chinese_Chess.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Chinese_Chess
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppSettings.LoadSettings();

            AppSettings.OnSettingsChanged += UpdateThemeColors;

            UpdateThemeColors();
        }

        private void UpdateThemeColors()
        {
            if (AppSettings.AppTheme == "dark")
            {
                SetColor("BackgroundBrush", "#121212");
                SetColor("CardBackgroundBrush", "#1E1E1E");
                SetColor("SurfaceBrush", "#1E1E1E");
                SetColor("TextDarkBrush", "#E0E0E0");
                SetColor("TextGrayBrush", "#B0BEC5");
                SetColor("BorderBrush", "#424242");
            }
            else
            {
                SetColor("BackgroundBrush", "#FDF8F0");
                SetColor("CardBackgroundBrush", "#FFFFFF");
                SetColor("SurfaceBrush", "#FFFFFF");
                SetColor("TextDarkBrush", "#4E342E");
                SetColor("TextGrayBrush", "#8D6E63");
                SetColor("BorderBrush", "#D7CCC8");
            }
        }

        private void SetColor(string key, string hex)
        {
            try
            {
                var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                this.Resources[key] = brush;
            }
            catch { }
        }
    }
}
