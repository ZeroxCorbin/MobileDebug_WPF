
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

        public class LogDetails_class
        {
            public string LogFileFullName { get; set; }
            public IList<IEnumerable<FileSearchResults>> SearchResults { get; set; } = new List<IEnumerable<FileSearchResults>>();

            public LogDetailsLog Log { get; set; }
        }

        //private Brush ButtonFace { get; set; }

        //Set by CheckProductType() if the debug data is for an EM.
        public bool IsEM { get; private set; }
        public bool HasMap { get; private set; }
        private MapFile MapFile { get; set; }

        //If a zip file is opened this is the path and name.
        public string ZipFilePath { get; private set; }

        //Log details from the LogDetails.xml file.
        private List<LogDetails_class> LogDetails { get; set; } = new List<LogDetails_class>();
        //When a button is clicked to view a logs search results these are the Log and Search Indexes.
        private LogIndices LogIndices { get; set; }

        //Log level enum. Returns a value based on the name. Used to select correct RTF color.
        public enum Levels
        {
            DEBUG,
            INFO,
            WARN,
            ERROR,
            FATAL
        }

        public enum Colors
        {
            BLACK,
            GREEN,
            ORANGE,
            RED,
            BLUE,
            YELLOW
        }

        //The head of the RTF text. Defines the colors fo highlighting.
        private readonly string rtfHead = "{\\rtf1\\ansi\\deff0\\nouicompat{\\fonttbl{\\f0\\fnil\\fcharset0 Segoe UI;}}\r\n" +
                                "{\\colortbl;" +
            "\\red0\\green0\\blue0;" + //Black
            "\\red0\\green153\\blue0;" + //Green
            "\\red255\\green192\\blue0;" + //Orange
            "\\red255\\green0\\blue0;" + //Red
            "\\red18\\green164\\blue239;" + //Light Blue
            "\\red255\\green250\\blue18;" + //Light Blue
            "}\r\n{\\*\\generator Riched20 10.0.17134}\\viewkind4\\uc1 \r\n\\pard\\f0\\fs18\\lang1033 ";
        //The tail of the RTF text.
        private readonly string rtfTail = "\\par\r\n}\r\n";
        //RTF Colors to use for highlighting search results based on Level set for the log.
        private readonly string[] rtfSearchFormatsLevel = { "\\ulwave\\cf2 ", "\\ulwave\\cf2 ", "\\ulwave\\cf3 ", "\\ulwave\\cf4 ", "\\ulwave\\cf4 " };
        //RTF Colors to use for highlighting search results based on Level set for the log.
        private readonly string[] rtfSearchFormatsColors = { "\\cf1 ", "\\cf2 ", "\\cf3 ", "\\cf4 ", "\\cf5 ", "\\cf6 " };
        //RTF highlight tail. Resets to Black text.
        private readonly string rtfSearchFormatsLevelTail = "\\ul0\\cf1 ";
        //RTF highlight tail. Resets to Black text.
        private readonly string rtfSearchFormatsColorsTail = "\\cf1 ";

        private const string MapDatabaseExtension = ".sqlite";
        private const string MapDatabaseTableName = "Map";
        private const string MapDatabaseFileKey = "File";
        private const string MapDatabaseContentsDateKey = "_changedate";
        private const string MapDatabaseContentsHeaderKey = "_header";
        private const string MapDatabaseContentsKey = "_contents";
        private const string MapDatabaseWifiKey = "WiFi";
        private const string MapDatabasePositionsKey = "Positions";

        private string SearchConfigurationPath => System.AppDomain.CurrentDomain.BaseDirectory + "Config\\";

        public ObservableCollection<string> OpenCommands { get; set; } = new ObservableCollection<string>() { "Open Zip File" };

        public ICommand OpenCommand { get; }
        private void OpenCallback(object parameter)
        {
            switch ((string)parameter)
            {
                case "Open Zip File":
                   Task.Run(()=> OpenZipFile());
                    break;
                case "Open Folder":
                    break;
                case "Open From LD/EM":
                    break;
                default:
                    break;
            }
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
                    // AddToHistory(file.FileName);
                    SystemInformation.Load();
                    TableOfContents.Load();
                    LogViewer.Load(SystemInformation.IsEM);
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

        //private class FileHistory
        //{
        //    public string Path { get; set; }
        //    public bool IsDirectory { get; set; } = false;
        //}
        //private void AddToHistory(string filePath)
        //{
        //    Dictionary<string, FileHistory> history = GetOpenHistory();

        //    FileHistory fhFile;
        //    if (!File.Exists(filePath) && !Directory.Exists(filePath))
        //    {
        //        var his = history.Where(s => s.Value.Path.Equals(filePath));

        //        foreach (var fh in history.ToList())
        //            history.Remove(fh.Key);
        //    }
        //    else
        //    {
        //        fhFile = new FileHistory()
        //        {
        //            Path = filePath,
        //        };
        //        if (File.Exists(filePath))
        //            fhFile.IsDirectory = false;
        //        else
        //            fhFile.IsDirectory = true;

        //        if (!history.ContainsKey(Path.GetFileName(fhFile.Path)))
        //            history.Add(Path.GetFileName(fhFile.Path), fhFile);
        //    }

        //    App.Settings.SetValue("FileHistory", history);

        //    //BuildOpenMenu();
        //    //UpdateHistoryMenuItems(history);
        //}
        //private Dictionary<string, FileHistory> GetOpenHistory() => App.Settings.GetValue("FileHistory", new Dictionary<string, FileHistory>());

        IDialogCoordinator _DialogCoordinator;

        public MainWindowViewModel(IDialogCoordinator controller)
        {
            _DialogCoordinator = controller;

            SystemInformation = new SystemInformationViewModel();
            TableOfContents = new TableOfContentsViewModel();
            LogViewer = new LogViewerViewModel();

            OpenCommand = new RelayCommand(OpenCallback, c => true);
        }

    }
}
