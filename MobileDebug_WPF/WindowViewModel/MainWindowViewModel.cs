
using FileSearch;
using MahApps.Metro.Controls.Dialogs;
using MobileDebug_WPF.Config;
using MobileDebug_WPF.Core;
using MobileMap;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MobileDebug_WPF.WindowViewModel
{
    public class MainWindowViewModel : Core.ViewModelBase
    {
        public string Title => "Main Window";
        public double Left { get => App.Settings.GetValue("MainWindowView.Left", 0.0); set { if (WindowState == WindowState.Normal) { App.Settings.SetValue("MainWindowView.Left", value); OnPropertyChanged(); } } }
        public double Top { get => App.Settings.GetValue("MainWindowView.Top", 0.0); set { if (WindowState == WindowState.Normal) { App.Settings.SetValue("MainWindowView.Top", value); OnPropertyChanged(); } } }
        public double Width { get => App.Settings.GetValue("MainWindowView.Width", 1024.0); set { if (WindowState == WindowState.Normal) { App.Settings.SetValue("MainWindowView.Width", value); OnPropertyChanged(); } } }
        public double Height { get => App.Settings.GetValue("MainWindowView.Height", 768.0); set { if (WindowState == WindowState.Normal) { App.Settings.SetValue("MainWindowView.Height", value); OnPropertyChanged(); } } }
        public WindowState WindowState
        {
            get => App.Settings.GetValue("MainWindowView.State", WindowState.Normal);
            set
            {

                if (value != WindowState.Minimized)
                    App.Settings.SetValue("MainWindowView.State", value);

                OnPropertyChanged();
            }
        }

        public SystemInformationViewModel SystemInformation { get; }
        public TableOfContentsViewModel TableOfContents { get; }
        public LogViewerViewModel LogViewer { get; }
        public WiFiViewerViewModel WiFiViewer { get; }
        public BatteryViewerViewModel BatteryViewer { get; }

        public ICommand OpenCommand { get; }
        private void OpenCallback(object parameter)
        {
            switch ((string)parameter)
            {
                case "Open Zip File":
                    Task.Run(() => OpenZipFile());
                    break;
                case "Open Folder":
                    break;
                case "Open From LD/EM":
                    break;
                default:
                    break;
            }
        }

        private void ResetAll()
        {
            SystemInformation.Reset();
            TableOfContents.Reset();
            LogViewer.Reset();
            WiFiViewer.Reset();
            BatteryViewer.Reset();
        }
        private void OpenZipFile()
        {
            Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Zip file (*.zip)|*.zip",
                FilterIndex = 1
            };

            if ((bool)file.ShowDialog())
            {

                if (ExtractFile(file.FileName))
                {
                    ResetAll();

                    // AddToHistory(file.FileName);
                    SystemInformation.Load();
                    TableOfContents.Load();
                    LogViewer.Load(SystemInformation.IsEM);
                    if (!SystemInformation.IsEM)
                    {
                        WiFiViewer.Load();
                        BatteryViewer.Load();
                    }

                }
                //Thread thread = new Thread(() => RunThread());
                //thread.SetApartmentState(ApartmentState.STA);
                //thread.Start();
            }
        }
        private bool ExtractFile(string fileName)
        {
            try
            {
                if (Directory.Exists(App.WorkingDirectory))
                    Directory.Delete(App.WorkingDirectory, true);

                ZipFile.ExtractToDirectory(fileName, App.WorkingDirectory);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private class FileHistory
        {
            public string Path { get; set; }
            public bool IsDirectory { get; set; } = false;
        }
        private void AddToHistory(string filePath)
        {
            Dictionary<string, FileHistory> history = GetOpenHistory();

            FileHistory fhFile;
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                var his = history.Where(s => s.Value.Path.Equals(filePath));

                foreach (var fh in history.ToList())
                    history.Remove(fh.Key);
            }
            else
            {
                fhFile = new FileHistory()
                {
                    Path = filePath,
                };
                if (File.Exists(filePath))
                    fhFile.IsDirectory = false;
                else
                    fhFile.IsDirectory = true;

                if (!history.ContainsKey(Path.GetFileName(fhFile.Path)))
                    history.Add(Path.GetFileName(fhFile.Path), fhFile);
            }

            App.Settings.SetValue("FileHistory", history);

            //BuildOpenMenu();
            //UpdateHistoryMenuItems(history);
        }
        private Dictionary<string, FileHistory> GetOpenHistory() => App.Settings.GetValue("FileHistory", new Dictionary<string, FileHistory>());

        IDialogCoordinator _DialogCoordinator;

        public MainWindowViewModel(IDialogCoordinator controller)
        {
            _DialogCoordinator = controller;

            SystemInformation = new SystemInformationViewModel();
            TableOfContents = new TableOfContentsViewModel();
            LogViewer = new LogViewerViewModel();
            WiFiViewer = new WiFiViewerViewModel();
            BatteryViewer = new BatteryViewerViewModel();

            OpenCommand = new RelayCommand(OpenCallback, c => true);
        }

    }
}
