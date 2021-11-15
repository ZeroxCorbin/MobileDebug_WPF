using ControlzEx.Theming;
using FileSearch;
using MobileDebug_WPF.Config;
using MobileDebug_WPF.Core;
using MobileDebug_WPF.Models;
using MobileLogs;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace MobileDebug_WPF.WindowViewModel
{
    public class BatteryViewerViewModel : Core.ViewModelBase
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

        private object BatteryViewerDetailsLock = new object();
        public ObservableCollection<BatteryViewerEntry> BatteryViewerDetails { get; private set; } = new ObservableCollection<BatteryViewerEntry>();


        public PlotModel StatePlotModel { get => _StatePlotModel; private set { Set(ref _StatePlotModel, value); } }
        private PlotModel _StatePlotModel;

        public PlotModel VoltagePlotModel { get => _VoltagePlotModel; private set { Set(ref _VoltagePlotModel, value); } }
        private PlotModel _VoltagePlotModel;

        public BatteryViewerViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(BatteryViewerDetails, BatteryViewerDetailsLock);

            ViewAllCommand = new RelayCommand(ViewAllCallback, c => true);

            _StatePlotModel = CreatePlotModel("State of Charge (%)");
            _VoltagePlotModel = CreatePlotModel("Voltage");
        }

        private PlotModel CreatePlotModel(string name)
        {
            var plotModel = new PlotModel();

            var theme = ThemeManager.Current.DetectTheme();
            OxyPlot.OxyColor color;
            if (theme.BaseColorScheme.Equals("Dark"))
                color = OxyColor.FromRgb(255, 255, 255);
            else
                color = OxyColor.FromRgb(0, 0, 0);
            plotModel.TextColor = color;
            AddAxes(plotModel, name);
            return plotModel;
        }
        private static void AddAxes(PlotModel plotModel, string name)
        {

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = name
            });

            plotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time & Date",
                StringFormat = "HH:mm:ss MM/dd/yy",
                IntervalType = DateTimeIntervalType.Auto,
                IntervalLength = 90
            });
        }

        public void Load()
        {
            IsVisible = true;
            IsLoading = true;

            SetupLogs();

            IsLoading = false;
        }
        public void Reset()
        {
            IsVisible = false;
            IsLoading = false;

            BatteryViewerDetails.Clear();

            _StatePlotModel.Series.Clear();
            _VoltagePlotModel.Series.Clear();
        }
        private void SetupLogs()
        {
            DirectoryInfo dir = new DirectoryInfo(App.WorkingDirectory + "var\\robot\\logs\\");

            IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

            IEnumerable<FileInfo> res =
                from test in names
                where test.Name.StartsWith("log_")
                select test;

            IList<FileInfo> lst = res.ToList<FileInfo>();

            foreach (FileInfo log in res)
            {
                BatteryViewerEntry entry = new BatteryViewerEntry()
                {
                    Name = log.Name,
                    Path = log.FullName,
                    Log = new BatteryLogs(log.FullName),
                    DateTime = log.LastWriteTime,
                    OpenCommand = new RelayCommand(OpenCallback, c => true),
                    ViewCommand = new RelayCommand(ViewCallback, c => true),
                };

                if(entry.SearchResults.Count > 1)
                    BatteryViewerDetails.Add(entry);
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
            var logs = ((BatteryViewerEntry)parameter).SearchResults;
            BatteryLogsDisplayPlot(logs);
        }

        public ICommand ViewAllCommand { get; }
        private void ViewAllCallback(object parameter)
        {
            List<BatteryLogData> results = new List<BatteryLogData>();

            foreach (var log in BatteryViewerDetails)
                results.AddRange(log.SearchResults);

            BatteryLogsDisplayPlot(results);
        }
        private void BatteryLogsDisplayPlot(List<BatteryLogData> batteryLog)
        {
            _StatePlotModel.Series.Clear();
            _VoltagePlotModel.Series.Clear();

            LineSeries wiFiBaudLineSeries = new LineSeries
            {
                StrokeThickness = 1,
                //Title = dict.Key,
            };
            _StatePlotModel.Series.Add(wiFiBaudLineSeries);

            LineSeries wiFiDecibelsLineSeries = new LineSeries
            {
                StrokeThickness = 1,
                //Title = dict.Key,
            };
            _VoltagePlotModel.Series.Add(wiFiDecibelsLineSeries);

            IEnumerable<BatteryLogData> batterySorted = from s in batteryLog
                                                        orderby s.Time
                                                        select s;

            foreach (BatteryLogData dat in batterySorted)
            {
                wiFiDecibelsLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Voltage));
                wiFiBaudLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Level));
            }

            _StatePlotModel.ResetAllAxes();
            _VoltagePlotModel.ResetAllAxes();
            _StatePlotModel.InvalidatePlot(true);
            _VoltagePlotModel.InvalidatePlot(true);
        }
        //private void BatteryLogsDisplayPlot(List<BatteryLogs> batLogs)
        //{

        //LineSeries batteryVoltageLineSeries = new LineSeries
        //{
        //    StrokeThickness = 1,
        //};
        //VoltagePlotModel.Series.Add(batteryVoltageLineSeries);

        //    LineSeries batterySOCLineSeries = new LineSeries
        //    {
        //        StrokeThickness = 1,
        //    };
        //batterySOCPlotModel.Series.Add(batterySOCLineSeries);

        //    foreach (BatteryLogs log in batLogs)
        //    {
        //        IEnumerable<BatteryLogs.BatteryLogData> batterySorted = from s in log.Results
        //                                                                orderby s.Time
        //                                                                select s;

        //        foreach (BatteryLogs.BatteryLogData dat in batterySorted)
        //        {
        //            batterySOCLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Level));
        //            batteryVoltageLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Voltage));
        //        }
        //    }
        //    Chart_BatteryVoltage.InvalidatePlot(true);
        //    Chart_BatteryChargeState.InvalidatePlot(true);
        //}
        //private void ViewCallback(object parameter)
        //{
        //    //LogData = ((KeyValuePair<string, List<FileSearchResults>>)parameter).Value;
        //    //OnPropertyChanged("LogData");

        //    //foreach (FileSearchResults res in ((KeyValuePair<string, List<FileSearchResults>>)parameter).Value)
        //    //    LogData.Add(res);
        //    //string test = ((KeyValuePair<string, List<FileSearchResults>>)parameter).Value[0].Line;
        //}
    }
}
