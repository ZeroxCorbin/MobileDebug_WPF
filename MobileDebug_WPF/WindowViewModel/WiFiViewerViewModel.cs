﻿using ControlzEx.Theming;
using FileSearch;
using MobileDebug_WPF.Config;
using MobileDebug_WPF.Core;
using MobileDebug_WPF.Models;
using MobileLogs;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MobileDebug_WPF.WindowViewModel
{
    public class WiFiViewerViewModel : Core.ViewModelBase
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

        private object WiFiViewerDetailsLock = new object();
        public ObservableCollection<WiFiViewerEntry> WiFiViewerDetails { get; private set; } = new ObservableCollection<WiFiViewerEntry>();

        private object WiFiSSIDDetailsLock = new object();
        public ObservableCollection<string> WiFiSSIDDetails { get; private set; } = new ObservableCollection<string>();

        public PlotModel BaudPlotModel { get => _BaudPlotModel; private set { Set(ref _BaudPlotModel, value); } }
        private PlotModel _BaudPlotModel;

        public PlotModel DecibelPlotModel { get => _DecibelPlotModel; private set { Set(ref _DecibelPlotModel, value); } }
        private PlotModel _DecibelPlotModel;

        public ICommand SaveBaudPlotCommand { get; }
        private void SaveBaudPlotCallback(object parameter)
        {
            var pngExporter = new PngExporter { Width = 4096, Height = 768 };

            if(((string)parameter) == "Save")
            {
                string file = null;
                if (App.SaveFile(ref file, "PNG file (*.png)|*.png", "WiFiBaud.png", true, false, true))
                {
                    pngExporter.ExportToFile(_BaudPlotModel, file);
                }
                return;
            }

            var bitmap = pngExporter.ExportToBitmap(_BaudPlotModel);
            Clipboard.SetImage(bitmap);

        }

        public ICommand SaveDecibelPlotCommand { get; }
        private void SaveDecibelPlotCallback(object parameter)
        {
            var pngExporter = new PngExporter { Width = 4096, Height = 768 };

            if (((string)parameter) == "Save")
            {
                string file = null;
                if (App.SaveFile(ref file, "PNG file (*.png)|*.png", "WiFiDecibels.png", true, false, true))
                {
                    pngExporter.ExportToFile(_DecibelPlotModel, file);
                }
                return;
            }

            var bitmap = pngExporter.ExportToBitmap(_DecibelPlotModel);
            Clipboard.SetImage(bitmap);
        }

        public WiFiViewerViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(WiFiViewerDetails, WiFiViewerDetailsLock);
            BindingOperations.EnableCollectionSynchronization(WiFiSSIDDetails, WiFiSSIDDetailsLock);

            ViewAllCommand = new RelayCommand(ViewAllCallback);

            SaveBaudPlotCommand = new RelayCommand(SaveBaudPlotCallback);
            SaveDecibelPlotCommand = new RelayCommand(SaveDecibelPlotCallback);

            _BaudPlotModel = CreatePlotModel("Baud");
            _DecibelPlotModel = CreatePlotModel("Decibels");
        }

        private PlotModel CreatePlotModel(string name)
        {
            var plotModel = new PlotModel();

            var theme = ThemeManager.Current.DetectTheme();

            OxyPlot.OxyColor foreColor;
            OxyPlot.OxyColor backColor;

            if (theme.BaseColorScheme.Equals("Dark"))
            {
                foreColor = OxyColor.FromRgb(255, 255, 255);
                backColor = OxyColor.FromRgb(37, 37, 37);
            }

            else
            {
                foreColor = OxyColor.FromRgb(0, 0, 0);
                backColor = OxyColor.FromRgb(255, 255, 255);
            }

            plotModel.TextColor = foreColor;
            plotModel.Background = backColor;

            AddAxes(plotModel, name);
            return plotModel;
        }
        private static void AddAxes(PlotModel plotModel, string name)
        {
            //plotModel.Legends.Add(new OxyPlot.Legends.Legend()
            //{
            //    LegendTitle = "SSID Names",
            //    LegendOrientation = LegendOrientation.Vertical,
            //    LegendPlacement = LegendPlacement.Outside,
            //    LegendPosition = LegendPosition.LeftTop
            //});

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

            WiFiViewerDetails.Clear();
            WiFiSSIDDetails.Clear();

            _BaudPlotModel.Series.Clear();
            _DecibelPlotModel.Series.Clear();
        }
        private void SetupLogs()
        {
            IList<FileInfo> lst = new List<FileInfo>();

            DirectoryInfo dir = new DirectoryInfo(App.WorkingDirectory + "var\\log\\");

            IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

            IEnumerable<FileInfo> res =
                from test in names
                where test.Name.StartsWith("wifiLog")
                select test;

            lst = res.ToList<FileInfo>();

            foreach (FileInfo log in lst)
            {
                WiFiViewerEntry entry = new WiFiViewerEntry()
                {
                    Name = log.Name,
                    Path = log.FullName,
                    Log = new WifiLogs(log.FullName),
                    DateTime = log.LastWriteTime,
                    OpenCommand = new RelayCommand(OpenCallback, c => true),
                    ViewCommand = new RelayCommand(ViewCallback, c => true),
                };

                WiFiViewerDetails.Add(entry);
            }


            foreach (var ent in WiFiViewerDetails)
                foreach (var sres in ent.SearchResults)
                    if (!WiFiSSIDDetails.Contains(sres.Key))
                        WiFiSSIDDetails.Add(sres.Key);
        }
        private void OpenCallback(object parameter)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(((WiFiViewerEntry)parameter).Path)
            {
                UseShellExecute = true
            };
            p.Start();
        }
        private void ViewCallback(object parameter)
        {
            Dictionary<string, List<WifiLogData>> results = new Dictionary<string, List<WifiLogData>>();
            var pair = (KeyValuePair<string, List<WifiLogData>>)parameter;

            results.Add(pair.Key, pair.Value);

            WiFiLogsDisplayPlot(results);
        }

        public ICommand ViewAllCommand { get; }
        private void ViewAllCallback(object parameter)
        {
            Dictionary<string, List<WifiLogData>> results = new Dictionary<string, List<WifiLogData>>();
            results.Add((string)parameter, new List<WifiLogData>());

            foreach (var log in WiFiViewerDetails)
            {
                if (log.SearchResults.ContainsKey((string)parameter))
                {
                    results[(string)parameter].AddRange(log.SearchResults[((string)parameter)]);
                }
            }

            WiFiLogsDisplayPlot(results);
        }
        public Dictionary<string, List<WifiLogData>> GetAllEntries()
        {
            Dictionary<string, List<WifiLogData>> results = new Dictionary<string, List<WifiLogData>>();


            foreach (var log in WiFiViewerDetails)
            {
                foreach (var res in log.SearchResults)
                {
                    if (!results.ContainsKey(res.Key))
                        results.Add(res.Key, res.Value);
                    else
                        results[res.Key].AddRange(res.Value);
                }
            }

            return results;
        }
        private void WiFiLogsDisplayPlot(Dictionary<string, List<WifiLogData>> wifiLogs)
        {
            _BaudPlotModel.Series.Clear();
            _DecibelPlotModel.Series.Clear();

            foreach (KeyValuePair<string, List<WifiLogData>> dict in wifiLogs)
            {
                LineSeries wiFiBaudLineSeries = new LineSeries
                {
                    StrokeThickness = 1,
                    Title = dict.Key,
                };
                _BaudPlotModel.Series.Add(wiFiBaudLineSeries);

                LineSeries wiFiDecibelsLineSeries = new LineSeries
                {
                    StrokeThickness = 1,
                    Title = dict.Key,
                };
                _DecibelPlotModel.Series.Add(wiFiDecibelsLineSeries);

                IEnumerable<WifiLogData> ssidSorted = from s in dict.Value
                                                      orderby s.Time
                                                      select s;

                foreach (WifiLogData dat in ssidSorted)
                {
                    wiFiDecibelsLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Decibels));
                    wiFiBaudLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Baud));
                }
            }
            _BaudPlotModel.ResetAllAxes();
            _DecibelPlotModel.ResetAllAxes();
            _BaudPlotModel.InvalidatePlot(true);
            _DecibelPlotModel.InvalidatePlot(true);
        }
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
