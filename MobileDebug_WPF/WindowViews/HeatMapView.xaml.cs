using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MobileDebug_WPF.WindowViewModel;
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
    public partial class HeatMapView : MetroTabItem
    {
        public HeatMapView()
        {
            ThemeManager.Current.ThemeChanged += Current_ThemeChanged;

            InitializeComponent();
                
        }

        private void Current_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            ((HeatMapViewModel)DataContext).ThemeChanged(e.NewTheme.BaseColorScheme == "Dark" ? true : false);
        }

        private void MetroTabItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is HeatMapViewModel vm)
            {
                var theme = ThemeManager.Current.DetectTheme();
                vm.ThemeChanged(theme.BaseColorScheme == "Dark");
            }

        }
    }
}
