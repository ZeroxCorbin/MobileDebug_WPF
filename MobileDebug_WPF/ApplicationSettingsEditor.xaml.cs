using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MobileDebug_WPF
{
    /// <summary>
    /// Interaction logic for ApplicationSettings.xaml
    /// </summary>
    public partial class ApplicationSettingsEditor : Window
    {
        private string AppPath => System.AppDomain.CurrentDomain.BaseDirectory;

        public ApplicationSettingsEditor()
        {
            InitializeComponent();

            CheckDxMobileMapPath();

            TxtDxMobileMapPath.Text = App.Settings.GetValue("DxMobileMapPath");
        }

        private void TxtDxMobileMapPath_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            //App.Settings.SetValue("DxMobileMapPath", TxtDxMobileMapPath.Text);
        }

        private void BtnSelectDxMobileMapPath_Click(object sender, RoutedEventArgs e)
        {
            string filePath = null;
            if (CheckDxMobileMapPath())
                filePath = $"{App.Settings.GetValue("DxMobileMapPath")}DxMobileMap_WPF.exe";

            Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = (filePath == null) ? string.Empty : System.IO.Path.GetDirectoryName(filePath),
                Filter = "DxMobileMap_WPF.exe|DxMobileMap_WPF.exe",
                FilterIndex = 1
            };

            if ((bool)file.ShowDialog())
            {
                string path = System.IO.Path.GetDirectoryName(file.FileName);
                if (!path.EndsWith("\\")) path += "\\";
                App.Settings.SetValue("DxMobileMapPath", path);

            }
            TxtDxMobileMapPath.Text = App.Settings.GetValue("DxMobileMapPath");
        }

        private bool CheckDxMobileMapPath()
        {
            string path = App.Settings.GetValue("DxMobileMapPath");
            if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists($"{path}DxMobileMap_WPF.exe"))
                    path = string.Empty;
                else
                    return true;
            }
            if (string.IsNullOrEmpty(path))
            {
                string temp = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppPath, @"..\..\..\..\DxMobileMap_WPF\DxMobileMap_WPF\bin\x64\Local Release\DxMobileMap_WPF.exe"));
                if (File.Exists(temp))
                    path = System.IO.Path.GetDirectoryName(temp);

                temp = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppPath, @"..\DxMobileMap_WPF\DxMobileMap_WPF.exe"));
                if (File.Exists(temp))
                    path = System.IO.Path.GetDirectoryName(temp);
            }

            if (string.IsNullOrEmpty(path))
            {
                App.Settings.SetValue("DxMobileMapPath", string.Empty);
                return false;
            }

            if (!path.EndsWith("\\")) path += "\\";
            App.Settings.SetValue("DxMobileMapPath", path);
            return true;
        }

    }
}
