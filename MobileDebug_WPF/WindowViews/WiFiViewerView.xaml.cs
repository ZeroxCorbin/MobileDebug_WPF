using ControlzEx.Theming;
using MahApps.Metro.Controls;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MobileDebug_WPF.WindowViews
{
    /// <summary>
    /// Interaction logic for WiFiViewerView.xaml
    /// </summary>
    public partial class WiFiViewerView : MetroTabItem
    {
        public WiFiViewerView()
        {
            ThemeManager.Current.ThemeChanged += Current_ThemeChanged;

            InitializeComponent();
                
        }

        private void Current_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {

            OxyPlot.OxyColor foreColor;
            OxyPlot.OxyColor backColor;

            if (e.NewTheme.BaseColorScheme.Equals("Dark"))
            {
                foreColor = OxyColor.FromRgb(255, 255, 255);
                backColor = OxyColor.FromRgb(37, 37, 37);
            }

            else
            {
                foreColor = OxyColor.FromRgb(0, 0, 0);
                backColor = OxyColor.FromRgb(255, 255, 255);
            }

            DecibelPlot.Model.TextColor = foreColor;
            DecibelPlot.Model.Background = backColor;

            BaudPlot.Model.TextColor = foreColor;
            BaudPlot.Model.Background = backColor;
        }
    }
}
