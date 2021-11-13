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

        public bool IsEM { get; private set; }

        private object LogViewerDetailsLock = new object();
        public ObservableCollection<LogViewerEntry> LogViewerDetails { get; private set; } = new ObservableCollection<LogViewerEntry>();
        public List<FileSearchResults> LogData { get => _LogData; private set => Set(ref _LogData, value); }
        private List<FileSearchResults> _LogData;

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
                    BufferData.Clear();
                    foreach (FileSearchResults s in value.Buffer.Raw)
                        BufferData.Add(s);
                    OnPropertyChanged("BufferData");
                }
            }
        }
        private FileSearchResults _SelectedLogData;

        public ObservableCollection<FileSearchResults> BufferData { get; } = new ObservableCollection<FileSearchResults>();

        public LogViewerViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(LogViewerDetails, LogViewerDetailsLock);
        }
        public void Load(bool isEM)
        {
            IsLoading = true;

            IsEM = isEM;

            SetupLogs();

            IsLoading = false;
        }

        private void SetupLogs()
        {
            LogViewerDetails.Clear();

            LogDetails serial = LogDetails_Serializer.Load($"{App.UserDataDirectory}LogDetails.xml");

            int i = -1;
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
                    LogIndices ind = new LogIndices();
                    bool first = true;
                    int ii = -1;

                    LogViewerEntry logViewerEntry = new LogViewerEntry
                    {
                        Log = log,
                        LogFileName = log.MultiLog ? file.Name : log.DisplayName,
                        OpenCommand = new RelayCommand(OpenCallback, c => true),
                        ViewCommand = new RelayCommand(ViewCallback, c => true),
                    };

                    foreach (LogDetailsLogSearch ser in log.Search)
                    {
                        ii++;

                        if (IsEM && !ser.isEM) continue;
                        if (!IsEM && !ser.isLD) continue;

                        //UpdateStatus("Processing log for (" + ser.RegEx2Match + "): " + file.Name);



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
                        //if (searchRes.Count() == 0) continue;


                        //logViewerEntry.SearchResults.Add(ser.DisplayName, (List<FileSearchResults>)searchRes.ToList());

                        i++;

                        ind.log = i;
                        ind.search = ii;

                        //if (first)
                        //{
                        //    Hyperlink hl = new Hyperlink()
                        //    {
                        //        Tag = log.MultiLog ? file.Name : ld.Log.DisplayName,
                        //    };
                        //    hl.Inlines.Add((string)hl.Tag);
                        //    hl.Click += LogHyperLink_Click;

                        //    Label lb = new Label
                        //    {
                        //        Tag = ind,
                        //        Content = hl
                        //    };
                        //    flpLogs.Children.Add(lb);

                        //    first = false;
                        //}

                        //Button but = new Button
                        //{
                        //    Tag = ind,
                        //    Content = log.Search[ii].DisplayName,
                        //    Margin = new Thickness(3)
                        //};
                        //but.Click += LogButton_Click;

                        //flpLogs.Children.Add(but);
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
            //OnPropertyChanged("LogData");

            //foreach (FileSearchResults res in ((KeyValuePair<string, List<FileSearchResults>>)parameter).Value)
            //    LogData.Add(res);
            //string test = ((KeyValuePair<string, List<FileSearchResults>>)parameter).Value[0].Line;
        }
    }
}
