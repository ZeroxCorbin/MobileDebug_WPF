using ControlzEx.Theming;
using MahApps.Metro.Controls;
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
    /// Interaction logic for BatteryViewerView.xaml
    /// </summary>
    public partial class BatteryViewerView : MetroTabItem
    {
        public BatteryViewerView()
        {
            ThemeManager.Current.ThemeChanged += Current_ThemeChanged;

            InitializeComponent();
                
        }

        private void Current_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            OxyPlot.OxyColor color;
            if (e.NewTheme.BaseColorScheme.Equals("Dark"))
                color = OxyPlot.OxyColor.FromRgb(255, 255, 255);
            else
                color = OxyPlot.OxyColor.FromRgb(0, 0, 0);

            DecibelPlot.Model.TextColor = color;
            BaudPlot.Model.TextColor = color;
        }
    }
}
