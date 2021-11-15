using FileSearch;
using MobileDebug_WPF.Config;
using MobileDebug_WPF.Core;
using MobileDebug_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace MobileDebug_WPF.WindowViewModel
{
    public class LogViewerViewModel : Core.ViewModelBase
    {
        public bool IsLoading
        {
            get { return _IsLoading; }
            set { Set(ref _IsLoading, value); }
        }
        private bool _IsLoading;

        public bool IsVisible
        {
            get { return _IsVisible; }
            set { Set(ref _IsVisible, value); }
        }
        private bool _IsVisible;

        public bool IsEM { get; private set; }

        private object LogViewerDetailsLock = new object();
        public ObservableCollection<LogViewerEntry> LogViewerDetails { get; private set; } = new ObservableCollection<LogViewerEntry>();

        private object LogDataLock = new object();
        public List<FileSearchResults> LogData { get => _LogData; private set => Set(ref _LogData, value); }
        private List<FileSearchResults> _LogData;

        private object BufferDataLock = new object();
        public ObservableCollection<FileSearchResults> BufferData { get; } = new ObservableCollection<FileSearchResults>();

        public FileSearchResults SelectedLogData
        {
            get => _SelectedLogData;
            set
            {
                Set(ref _SelectedLogData, value);

                if (value == null)
                {
                    BufferData.Clear();
                    OnPropertyChanged("BufferData");
                    return;
                }
                else
                {
                    var sortedStr = from item in value.Buffer.Raw
                                    orderby (item != null) ? item.LineNumber : 0
                                    select item;

                    BufferData.Clear();
                    foreach (var s in sortedStr)
                        BufferData.Add(s);
                    OnPropertyChanged("BufferData");
                }
            }
        }
        private FileSearchResults _SelectedLogData;

        public LogViewerViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(LogViewerDetails, LogViewerDetailsLock);

            BindingOperations.EnableCollectionSynchronization(BufferData, BufferDataLock);
        }
        public void Load(bool isEM)
        {
            IsVisible = true;
            IsLoading = true;

            IsEM = isEM;

            SetupLogs();

            IsLoading = false;
        }
        public void Reset()
        {
            IsVisible = false;
            IsLoading = false;

            SelectedLogData = null;

            LogViewerDetails.Clear();

            if (LogData != null)
                LogData.Clear();
            LogData = null;
        }

        private void SetupLogs()
        {
            LogDetails serial = LogDetails_Serializer.Load(Path.Join(App.UserDataDirectory, "Config\\", "LogDetails.xml"));

            foreach (LogDetailsLog log in serial.Log)
            {
                if (IsEM && !log.isEM) continue;
                if (!IsEM && !log.isLD) continue;

                IList<FileInfo> lst = new List<FileInfo>();
                if (log.MultiLog)
                {
                    DirectoryInfo dir = new DirectoryInfo(App.WorkingDirectory + log.FilePath);
                    IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);
                    IEnumerable<FileInfo> res =
                        from test in names
                        where test.Name.StartsWith(log.FileName)
                        select test;
                    lst = res.ToList<FileInfo>();
                }
                else lst.Add(new FileInfo(App.WorkingDirectory + log.FilePath + log.FileName));

                foreach (FileInfo file in lst)
                {
                    LogViewerEntry logViewerEntry = new LogViewerEntry
                    {
                        Log = log,
                        Name = log.MultiLog ? file.Name : log.DisplayName,
                        Path = file.FullName,
                        DateTime = file.LastWriteTime,
                        OpenCommand = new RelayCommand(OpenCallback, c => true),
                        ViewCommand = new RelayCommand(ViewCallback, c => true),
                    };

                    foreach (LogDetailsLogSearch ser in log.Search)
                    {
                        if (IsEM && !ser.isEM) continue;
                        if (!IsEM && !ser.isLD) continue;

                        IEnumerable<FileSearchResults> searchRes = FileSearch.FileSearch.Find(file.FullName, ser.RegEx2Match, false);

                        List<FileSearchResults> temp = new List<FileSearchResults>();
                        foreach (FileSearchResults res in searchRes)
                        {
                            string name = res.Line;
                            temp.Add(new FileSearchResults() { Buffer = res.Buffer, Line = res.Line, LineNumber = res.LineNumber });
                        }

                        if (temp.Count > 0)
                        {
                            logViewerEntry.SearchResults.Add(ser.DisplayName, temp);
                        }
                    }

                    LogViewerDetails.Add(logViewerEntry);
                }
            }
        }

        private void OpenCallback(object parameter)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(((LogViewerEntry)parameter).Path)
            {
                UseShellExecute = true
            };
            p.Start();
        }
        private void ViewCallback(object parameter)
        {
            LogData = ((KeyValuePair<string, List<FileSearchResults>>)parameter).Value;
            BindingOperations.EnableCollectionSynchronization(LogData, LogDataLock);
            //OnPropertyChanged("LogData");

            //foreach (FileSearchResults res in ((KeyValuePair<string, List<FileSearchResults>>)parameter).Value)
            //    LogData.Add(res);
            //string test = ((KeyValuePair<string, List<FileSearchResults>>)parameter).Value[0].Line;
        }
    }
}
