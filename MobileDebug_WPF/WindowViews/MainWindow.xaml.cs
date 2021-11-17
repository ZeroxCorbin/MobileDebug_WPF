using MobileLogs;
using MobileMap;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Threading;
using OxyPlot.Legends;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using FileSearch;
using MobileDebug_WPF.Config;

namespace MobileDebug_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            //ThemeManager.Current.SyncTheme();

            InitializeComponent();

            DataContext = new WindowViewModel.MainWindowViewModel(MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance);

            DragDrop.DataContext =  ((WindowViewModel.MainWindowViewModel)DataContext).DragDrop;
            SystemInformationExpander.DataContext = ((WindowViewModel.MainWindowViewModel)DataContext).SystemInformation;
            TableOfContentsExpander.DataContext = ((WindowViewModel.MainWindowViewModel)DataContext).TableOfContents;
            LogViewer.DataContext = ((WindowViewModel.MainWindowViewModel)DataContext).LogViewer;
            WiFiViewer.DataContext = ((WindowViewModel.MainWindowViewModel)DataContext).WiFiViewer;
            BatteryViewer.DataContext = ((WindowViewModel.MainWindowViewModel)DataContext).BatteryViewer;
            HeatMapViewer.DataContext = ((WindowViewModel.MainWindowViewModel)DataContext).HeatMapViewer;

            _ = SetBinding(WidthProperty, new Binding("Width") { Source = DataContext, Mode = BindingMode.TwoWay });
            _ = SetBinding(HeightProperty, new Binding("Height") { Source = DataContext, Mode = BindingMode.TwoWay });

            

            //WindowSettings = App.Settings.GetValue("WindowSettings", new SimpleDataBase.WindowSettings());

            //Title = "Mobile Debug (Beta)";

            //if (WindowSettings.State == WindowState.Maximized) this.WindowState = WindowSettings.State;
            //this.Top = WindowSettings.Top;
            //this.Left = WindowSettings.Left;

            //this.Width = WindowSettings.Width;
            //this.Height = WindowSettings.Height;

            //if (!WindowSettings.IsOnScreen())
            //{
            //    this.Top = 0;
            //    this.Left = 0;
            //}

            //CheckForSearchConfigFiles();

            //CheckDxMobileMapPath();

            //MenuItem mi = new MenuItem()
            //{
            //    Header = "Log Searches",
            //    Tag = App.UserDataDirectory + "LogDetails.xml",
            //};
            //mi.Click += MenuSettings_Search_Click;
            //MenuSettings_Search.Items.Add(mi);

           // ExpTOC.IsExpanded = App.Settings.GetValue("ExpanderTOC", false);

            //tabCrashLogs.Visibility = Visibility.Collapsed;
            //tabBatteryLogs.Visibility = Visibility.Collapsed;
            ////tabWiFiLogs.Visibility = Visibility.Collapsed;
            //tabMapContents.Visibility = Visibility.Collapsed;
            //tabMapContentsRaw.Visibility = Visibility.Collapsed;
            //tabMapImage.Visibility = Visibility.Collapsed;
            //tabMapWiFiHeat.Visibility = Visibility.Collapsed;

        }
        private void btnLightTheme_Click(object sender, RoutedEventArgs e) => ThemeManager.Current.ChangeTheme(App.Current, "Light.Steel");

        private void btnDarkTheme_Click(object sender, RoutedEventArgs e) => ThemeManager.Current.ChangeTheme(App.Current, "Dark.Steel");


        //private bool CheckDxMobileMapPath()
        //{
        //    string path = App.Settings.GetValue("DxMobileMapPath");
        //    if (!string.IsNullOrEmpty(path))
        //    {
        //        if (!File.Exists($"{path}DxMobileMap_WPF.exe"))
        //            path = string.Empty;
        //        else
        //            return true;
        //    }
        //    if (string.IsNullOrEmpty(path))
        //    {
        //        string temp = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppPath, @"..\..\..\..\DxMobileMap_WPF\DxMobileMap_WPF\bin\x64\Local Release\DxMobileMap_WPF.exe"));
        //        if (File.Exists(temp))
        //            path = System.IO.Path.GetDirectoryName(temp);

        //        temp = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppPath, @"..\DxMobileMap_WPF\DxMobileMap_WPF.exe"));
        //        if (File.Exists(temp))
        //            path = System.IO.Path.GetDirectoryName(temp);
        //    }

        //    if (string.IsNullOrEmpty(path))
        //    {
        //        App.Settings.SetValue("DxMobileMapPath", string.Empty);
        //        return false;
        //    }

        //    if (!path.EndsWith("\\")) path += "\\";
        //    App.Settings.SetValue("DxMobileMapPath", path);

        //    return true;
        //}

        //private void CheckForSearchConfigFiles()
        //{
        //    string[] targetFiles = Directory.GetFiles(App.UserDataDirectory, "*.xml");
        //    foreach (string fileName in Directory.GetFiles(SearchConfigurationPath, "*.xml"))
        //    {
        //        bool found = false;
        //        foreach (string target in targetFiles)
        //            if (System.IO.Path.GetFileName(target).Equals(System.IO.Path.GetFileName(fileName)))
        //            {
        //                found = true;
        //                break;
        //            }
        //        if (!found)
        //            File.Copy(fileName, $"{App.UserDataDirectory}{System.IO.Path.GetFileName(fileName)}");
        //    }
        //}



        //public delegate void Run();

        //private void InvokeThis(Action act)
        //{
        //    Console.WriteLine("Invoke: " + act.Method.Name);
        //    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, act);
        //}
        //private void RunThread()
        //{
        //    //InvokeThis(new Action(() => { MenuFile.IsEnabled = false; }));

        //    InvokeThis(new Action(() => { ClearForm(true); }));

        //    if (!string.IsNullOrEmpty(ZipFilePath))
        //    {
        //        UpdateStatus("Extracting Files...");
        //        if (!ExtractFile())
        //        {
        //            //InvokeThis(new Action(() => { MenuFile.IsEnabled = true; }));
        //            UpdateStatus("Extraction Error!");
        //        }
        //    }

        //    try
        //    {
        //        CheckProductType();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return;
        //    }

        //   // UpdateStatus("Loading system data...");
        //    //InvokeThis(new Action(() => { LoadSystemHealth(); }));

        //    //InvokeThis(new Action(() => { LoadSystemDetails(); }));

        //    //InvokeThis(new Action(() => { LoadSystemApps(); }));

        //    //UpdateStatus("Loading Table of Contents...");
        //    //InvokeThis(new Action(() => { ReadTOC(); }));

        //    //UpdateStatus("Setting up log searches...");
        //    ////InvokeThis(new Action(() => { SetupLogs(); }));

        //    //UpdateStatus("Setting up crash logs...");
        //    //InvokeThis(new Action(() => { SetupCrashLogs(); }));

        //    if (!IsEM)
        //    {
        //        InvokeThis(new Action(() => { tabBatteryLogs.Visibility = Visibility.Visible; }));

        //        UpdateStatus("Setting up battery graphs...");
        //        InvokeThis(new Action(() => { LoadBatteryLogs(); }));
        //    }

        //    //if (!IsEM)
        //    //{
        //    //    //InvokeThis(new Action(() => { tabWiFiLogs.Visibility = Visibility.Visible; }));

        //    //    if (!IsEM)
        //    //    {
        //    //        UpdateStatus("Setting up WiFi graphs...");
        //    //        InvokeThis(new Action(() => { LoadWiFiLogs(); }));
        //    //    }
        //    //}

        //    DirectoryInfo di = new DirectoryInfo(WorkingPath + "home\\admin");
        //    HasMap = false;
        //    MapFile = null;
        //    foreach (FileInfo fi in di.GetFiles())
        //    {
        //        if (fi.Name.EndsWith(".map"))
        //        {
        //            UpdateStatus($"Map found: {fi.Name} - Loading Contents...");
        //            MapFile = new MapFile(fi.FullName, false);
        //            MapFile.LoadMapFromFile();

        //            HasMap = true;
        //            InvokeThis(new Action(() => { LoadMapContents(); }));
        //            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((path) => { LoadMapContentsRaw(path); }), MapFile.FilePath);

        //            if (!IsEM)
        //                LoadLoggedDataThread();

        //            break;
        //        }
        //    }

        //    string mapPath = HasMap ? MapFile.FilePath : string.Empty;

        //    if (HasMap)
        //    {


        //        //if (MapFile.Map.MapFile.LoggedWifi != null)
        //        //{
        //        //    UpdateStatus("Building WiFi Heatmap points...");
        //        //    System.Drawing.Rectangle r = new System.Drawing.Rectangle((int)MapFile.Map.Header.WidthOffset, (int)MapFile.Map.Header.HeightOffset, (int)MapFile.Map.Header.Width, (int)MapFile.Map.Header.Height);

        //        //    foreach (WifiLogs.WifiLogData s in MapFile.Map.MapFile.LoggedWifi)
        //        //    {
        //        //        if (r.Contains(s.Position))
        //        //        {
        //        //            //HeatPoints.Add(new HeatPoint(s.Position, (byte)Math.Abs(s.Baud)));
        //        //            if (HeatPoints.Count() == 0)
        //        //            {
        //        //                HeatPoints.Add(new HeatPoint(s.Position, (byte)Math.Abs(s.Baud)));
        //        //                continue;
        //        //            }

        //        //            int i = 0;
        //        //            bool found = false;
        //        //            foreach (HeatPoint hp in HeatPoints.ToArray())
        //        //            {
        //        //                if (new System.Drawing.Rectangle(hp.X, hp.Y, 500, 500).Contains(s.Position))
        //        //                {
        //        //                    HeatPoints[i] = new HeatPoint(hp.Point, (byte)((hp.Intensity + Math.Abs(s.Baud)) / 2));
        //        //                    found = true;
        //        //                    break;
        //        //                }
        //        //                i++;
        //        //            }
        //        //            if (!found)
        //        //            {
        //        //                HeatPoints.Add(new HeatPoint(s.Position, (byte)Math.Abs(s.Baud)));
        //        //            }
        //        //        }

        //        //    }


        //        //}

        //        UpdateStatus("Drawing Map");

        //        string bmp = MapUtils.GetBitmapString(MapFile.Map, 4096, 4096);

        //        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) => { DrawMapApplyToImage(s); }), bmp);

        //        //UpdateStatus("Setting up Map menu...");
        //       // Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) => { SetupMapMenu(s); }), mapPath);

        //        //InvokeThis(new Action(() => { MenuFile.IsEnabled = true; }));
        //    }
        //    UpdateStatus("Complete!");
        //}


        //private void UpdateStatus(string msg)
        //{
        //    Console.WriteLine(msg);

        //    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) =>
        //    {

        //        //LstStatusList.Items.Insert(0, s);
        //    }), msg);
        //    //Thread.Sleep(1);
        //}

        //public delegate Map LoadLoggedDataDelegate(Map map);

        //private void LoadLoggedDataThread()
        //{
        //    MapFile.Map.LoggedWifi = new Dictionary<string, List<WifiLogData>>();
        //    MobileLogs.MultiFile mf = new MobileLogs.MultiFile("wifiLog", WorkingPath + "var\\log");
        //    mf.FileOpened += Mf_FileOpened;

        //    var wifiFiles = mf.GetResults<MobileLogs.WifiLogs>();
        //    foreach (MobileLogs.WifiLogs logFile in wifiFiles)
        //        foreach (KeyValuePair<string, List<WifiLogData>> res in logFile.Results)
        //        {
        //            if (MapFile.Map.LoggedWifi.ContainsKey(res.Key))
        //            {
        //                MapFile.Map.LoggedWifi[res.Key].AddRange(res.Value);
        //            }
        //            else
        //            {
        //                MapFile.Map.LoggedWifi.Add(res.Key, res.Value);
        //            }
        //        }


        //    MapFile.Map.LoggedStatus = new List<MobileLogs.StatusLogs.Status>();
        //    mf = new MobileLogs.MultiFile("log_", WorkingPath + "\\var\\robot\\logs");
        //    mf.FileOpened += Mf_FileOpened;

        //    var statusFiles = mf.GetResults<MobileLogs.StatusLogs>();
        //    foreach (MobileLogs.StatusLogs status in statusFiles)
        //        foreach (MobileLogs.StatusLogs.Status dat in status.Results)
        //        {
        //            if (dat.Position.X == 0 & dat.Position.Y == 0 & dat.Heading == 0)
        //                continue;
        //            MapFile.Map.LoggedStatus.Add(dat);
        //        }
        //}

        ////private void LoadLoggedDataCallBack(IAsyncResult result)
        ////{
        ////    AsyncResult ar = (AsyncResult)result;
        ////    LoadLoggedDataDelegate bp = (LoadLoggedDataDelegate)ar.AsyncDelegate;
        ////    Map map = bp.EndInvoke(result);


        ////}



        //private bool ExtractFile()
        //{
        //    try
        //    {
        //        ZipFile.ExtractToDirectory(ZipFilePath, WorkingPath);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return false;
        //    }
        //}
        //private bool RemoveWorkDir()
        //{
        //    bool err;

        //    for (int i = 0; i < 10; i++)
        //    {
        //        try
        //        {
        //            err = false;
        //            Directory.Delete(WorkingPath, true);
        //        }
        //        catch
        //        {
        //            err = true;
        //            Thread.Sleep(50);
        //        }
        //        if (err == false) break;
        //    }

        //    if (Directory.Exists(WorkingPath) == true) return false;
        //    else return true;
        //}





        //private void SetupCrashLogs()
        //{
        //    DirectoryInfo dir = new DirectoryInfo(WorkingPath + @"\var\robot\logs\");
        //    IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);
        //    IEnumerable<FileInfo> res =
        //        from test in names
        //        where test.Name.StartsWith("crash")
        //        select test;
        //    IList<FileInfo> lst = res.ToList<FileInfo>();

        //    foreach (FileInfo file in lst)
        //    {

        //        Hyperlink hl = new Hyperlink()
        //        {
        //            Tag = file.FullName,
        //        };
        //        hl.Inlines.Add(System.IO.Path.GetFileName(file.FullName));
        //        hl.Click += CrashLogHyperLink_Click;

        //        Label lb = new Label
        //        {
        //            Tag = file.FullName,
        //            Content = hl
        //        };
        //        flpCrashLogs.Children.Add(lb);

        //        Button but = new Button
        //        {
        //            Tag = file.FullName,
        //            Content = System.IO.Path.GetFileNameWithoutExtension(file.FullName),
        //            Margin = new Thickness(3),
        //            MinWidth = 74
        //        };
        //        but.Click += CrashLogButton_Click;

        //        flpCrashLogs.Children.Add(but);

        //        tabCrashLogs.Visibility = Visibility.Visible;
        //    }
        //}
        //private void CrashLogHyperLink_Click(object sender, RoutedEventArgs e)
        //{
        //    if (((Hyperlink)sender).Tag is string path)
        //        System.Diagnostics.Process.Start(path);
        //}
        //private void CrashLogButton_Click(object sender, RoutedEventArgs e)
        //{
        //    foreach (Control c in flpCrashLogs.Children)
        //        if (typeof(Button) == c.GetType())
        //            c.Background = ButtonFace;

        //    Button btn = (Button)sender;
        //    string filePath = (string)btn.Tag;
        //    btn.Background = Brushes.LightGreen;

        //    TxtCrashLogLines.Text = File.ReadAllText(filePath);
        //}



        ////Map TABs
        //private void LoadMapContents()
        //{
        //    ParseMapHeader();
        //    ParseMapInfo();
        //    tabMapContents.Visibility = Visibility.Visible;
        //}
        //private void ParseMapHeader()
        //{
        //    TreeViewItem tviHeaderMain = new TreeViewItem()
        //    {
        //        Header = new Label() { Content = new Bold(new System.Windows.Documents.Run("Map Header") { FontSize = 20 }) }
        //    };

        //    StackPanel stkHeaderDataMain = new StackPanel()
        //    {

        //    };
        //    TreeViewItem tviHeaderData = new TreeViewItem()
        //    {
        //        Header = stkHeaderDataMain
        //    };

        //    foreach (PropertyInfo prop in MapFile.Map.Header.GetType().GetProperties())
        //    {
        //        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        //        if (type == typeof(float) | type == typeof(double) | type == typeof(int))
        //        {
        //            StackPanel stkHeaderData = new StackPanel()
        //            {
        //                Orientation = Orientation.Horizontal,
        //            };
        //            stkHeaderData.Children.Add(new Label() { Content = prop.Name });
        //            stkHeaderData.Children.Add(new TextBox() { Text = $"{prop.GetValue(MapFile.Map.Header, null).ToString()}", IsReadOnly = true, VerticalContentAlignment = System.Windows.VerticalAlignment.Center, BorderBrush = this.Background });

        //            stkHeaderDataMain.Children.Add(stkHeaderData);
        //        }
        //    }

        //    tviHeaderMain.Items.Add(tviHeaderData);
        //    TvMapContents.Items.Add(tviHeaderMain);
        //}
        //private void ParseMapInfo()
        //{
        //    TreeViewItem tviMapInfoMain = new TreeViewItem()
        //    {
        //        Header = new Label() { Content = new Bold(new System.Windows.Documents.Run("Map Info") { FontSize = 20 }) }
        //    };

        //    TvMapContents.Items.Add(tviMapInfoMain);

        //    foreach (MapInfo.InfoType mInfo in MapFile.Map.Info.List)
        //    {
        //        TreeViewItem tviMapInfoTypeMain = new TreeViewItem()
        //        {
        //            Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(mInfo.Name)) }
        //        };
        //        tviMapInfoMain.Items.Add(tviMapInfoTypeMain);

        //        StackPanel stkMapInfoTypeData = new StackPanel()
        //        {

        //        };
        //        TreeViewItem tviMapInfoTypeData = new TreeViewItem()
        //        {
        //            Header = stkMapInfoTypeData
        //        };
        //        tviMapInfoTypeMain.Items.Add(tviMapInfoTypeData);

        //        foreach (PropertyInfo prop in typeof(MapInfo.InfoType).GetProperties())
        //        {
        //            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        //            if (type == typeof(string))
        //            {
        //                object data = prop.GetValue(mInfo, null);
        //                string val = string.Empty;
        //                if (data != null)
        //                    val = data.ToString();

        //                StackPanel stkMapInfoTypeDataItem = new StackPanel()
        //                {
        //                    Orientation = Orientation.Horizontal,
        //                };
        //                stkMapInfoTypeDataItem.Children.Add(new Label() { Content = prop.Name });
        //                stkMapInfoTypeDataItem.Children.Add(new TextBox() { Text = $"{val}", IsReadOnly = true, VerticalContentAlignment = System.Windows.VerticalAlignment.Center, BorderBrush = this.Background });

        //                stkMapInfoTypeData.Children.Add(stkMapInfoTypeDataItem);
        //            }

        //            if (type == typeof(Dictionary<string, string>))
        //            {
        //                TreeViewItem tviMapInfoTypeParamDataMain = new TreeViewItem()
        //                {
        //                    Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(prop.Name)) }
        //                };
        //                tviMapInfoTypeMain.Items.Add(tviMapInfoTypeParamDataMain);

        //                StackPanel stkMapInfoTypeParamData = new StackPanel()
        //                {

        //                };
        //                TreeViewItem tviMapInfoTypeParamData = new TreeViewItem()
        //                {
        //                    Header = stkMapInfoTypeParamData
        //                };
        //                tviMapInfoTypeParamDataMain.Items.Add(tviMapInfoTypeParamData);

        //                object data = prop.GetValue(mInfo, null);
        //                Dictionary<string, string> dict;
        //                if (data != null)
        //                    dict = (Dictionary<string, string>)data;
        //                else
        //                    dict = new Dictionary<string, string>();

        //                foreach (KeyValuePair<string, string> val in dict)
        //                {
        //                    StackPanel stkMapInfoTypeDataParamItem = new StackPanel()
        //                    {
        //                        Orientation = Orientation.Horizontal,
        //                    };
        //                    stkMapInfoTypeDataParamItem.Children.Add(new Label() { Content = val.Key });
        //                    stkMapInfoTypeDataParamItem.Children.Add(new TextBox() { Text = $"{val.Value}", IsReadOnly = true, VerticalContentAlignment = System.Windows.VerticalAlignment.Center, BorderBrush = this.Background });

        //                    stkMapInfoTypeParamData.Children.Add(stkMapInfoTypeDataParamItem);
        //                }
        //            }

        //            if (type == typeof(Dictionary<string, MapArgDesc>))
        //            {
        //                TreeViewItem tviMapInfoTypeArgsMain = new TreeViewItem()
        //                {
        //                    Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(prop.Name)) }
        //                };
        //                tviMapInfoTypeMain.Items.Add(tviMapInfoTypeArgsMain);

        //                object data = prop.GetValue(mInfo, null);
        //                Dictionary<string, MapArgDesc> dict;
        //                if (data != null)
        //                    dict = (Dictionary<string, MapArgDesc>)data;
        //                else
        //                    dict = new Dictionary<string, MapArgDesc>();

        //                foreach (KeyValuePair<string, MapArgDesc> val in dict)
        //                {
        //                    TreeViewItem tviMapInfoTypeArg = new TreeViewItem()
        //                    {
        //                        Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(val.Key)) }
        //                    };
        //                    tviMapInfoTypeArgsMain.Items.Add(tviMapInfoTypeArg);

        //                    StackPanel stkMapInfoTypeArgData = new StackPanel()
        //                    {

        //                    };
        //                    TreeViewItem tviMapInfoTypeArgData = new TreeViewItem()
        //                    {
        //                        Header = stkMapInfoTypeArgData
        //                    };
        //                    tviMapInfoTypeArg.Items.Add(tviMapInfoTypeArgData);

        //                    foreach (PropertyInfo prop1 in typeof(MapArgDesc).GetProperties())
        //                    {
        //                        var type1 = Nullable.GetUnderlyingType(prop1.PropertyType) ?? prop1.PropertyType;

        //                        if (type1 == typeof(string))
        //                        {
        //                            object data1 = prop1.GetValue(val.Value, null);

        //                            string val1 = string.Empty;
        //                            if (data1 != null)
        //                                val1 = data1.ToString();

        //                            StackPanel stkMapInfoTypeArgDataItem = new StackPanel()
        //                            {
        //                                Orientation = Orientation.Horizontal,
        //                            };
        //                            stkMapInfoTypeArgDataItem.Children.Add(new Label() { Content = prop1.Name });
        //                            stkMapInfoTypeArgDataItem.Children.Add(new TextBox() { Text = $"{val1}", VerticalContentAlignment = System.Windows.VerticalAlignment.Center, IsReadOnly = true, BorderBrush = this.Background });

        //                            stkMapInfoTypeArgData.Children.Add(stkMapInfoTypeArgDataItem);
        //                        }

        //                        if (type1 == typeof(Dictionary<string, string>))
        //                        {
        //                            TreeViewItem tviMapInfoTypeArgParams = new TreeViewItem()
        //                            {
        //                                Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(prop1.Name)) }
        //                            };
        //                            tviMapInfoTypeArgData.Items.Add(tviMapInfoTypeArgParams);

        //                            StackPanel stkMapInfoTypeArgParamsData = new StackPanel()
        //                            {

        //                            };
        //                            TreeViewItem tviMapInfoTypeArgParamsData = new TreeViewItem()
        //                            {
        //                                Header = stkMapInfoTypeArgParamsData
        //                            };
        //                            tviMapInfoTypeArgParams.Items.Add(tviMapInfoTypeArgParamsData);

        //                            object data2 = prop1.GetValue(val.Value, null);

        //                            Dictionary<string, string> dict2;
        //                            if (data2 != null)
        //                                dict2 = (Dictionary<string, string>)data2;
        //                            else
        //                                dict2 = new Dictionary<string, string>();

        //                            foreach (KeyValuePair<string, string> val2 in dict2)
        //                            {
        //                                StackPanel stkMapInfoTypeArgParamsDataItem = new StackPanel()
        //                                {
        //                                    Orientation = Orientation.Horizontal,
        //                                };
        //                                stkMapInfoTypeArgParamsDataItem.Children.Add(new Label() { Content = val2.Key });
        //                                stkMapInfoTypeArgParamsDataItem.Children.Add(new TextBox() { Text = $"{val2.Value}", VerticalContentAlignment = System.Windows.VerticalAlignment.Center, IsReadOnly = true, BorderBrush = this.Background });

        //                                stkMapInfoTypeArgParamsData.Children.Add(stkMapInfoTypeArgParamsDataItem);
        //                            }
        //                        }
        //                    }
        //                    //    StkMain.Children.Add(new Label() { Content = new Bold(new Run(val.Key)), Margin = new Thickness(20, 0, 0, 0) });
        //                    //StkMain.Children.Add(new TextBox() { IsReadOnly = true, Text = $"{val.Value}", BorderBrush = this.Background, Margin = new Thickness(40, 0, 80, 0) });
        //                }
        //            }

        //            if (type == typeof(List<MapCairn>))
        //            {
        //                TreeViewItem tviMapInfoTypeChildMain = new TreeViewItem()
        //                {
        //                    Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(prop.Name)) }
        //                };
        //                tviMapInfoTypeMain.Items.Add(tviMapInfoTypeChildMain);

        //                object data = prop.GetValue(mInfo, null);

        //                List<MapCairn> dict;
        //                if (data != null)
        //                    dict = (List<MapCairn>)data;
        //                else
        //                    dict = new List<MapCairn>();

        //                if (dict.Count() > 0) tviMapInfoTypeMain.Tag = true;
        //                else tviMapInfoTypeMain.Tag = false;

        //                foreach (MapCairn mc in dict)
        //                {
        //                    TreeViewItem tviMapInfoTypeChild = new TreeViewItem()
        //                    {
        //                        Header = new Label() { Content = new Bold(new System.Windows.Documents.Run($"{prop.Name}: {mc.Type}")) }
        //                    };
        //                    tviMapInfoTypeChildMain.Items.Add(tviMapInfoTypeChild);

        //                    StackPanel stkMapInfoTypeChildData = new StackPanel()
        //                    {

        //                    };
        //                    TreeViewItem tviMapInfoTypeChildData = new TreeViewItem()
        //                    {
        //                        Header = stkMapInfoTypeChildData
        //                    };
        //                    tviMapInfoTypeChild.Items.Add(tviMapInfoTypeChildData);

        //                    foreach (PropertyInfo prop3 in typeof(MapCairn).GetProperties())
        //                    {
        //                        var type3 = Nullable.GetUnderlyingType(prop3.PropertyType) ?? prop3.PropertyType;

        //                        object data3 = prop3.GetValue(mc, null);

        //                        if (data3 == null)
        //                            data3 = string.Empty;

        //                        StackPanel stkMapInfoTypeChildDataItem = new StackPanel()
        //                        {
        //                            Orientation = Orientation.Horizontal,
        //                        };
        //                        stkMapInfoTypeChildDataItem.Children.Add(new Label() { Content = prop3.Name });
        //                        stkMapInfoTypeChildDataItem.Children.Add(new TextBox() { Text = $"{data3.ToString()}", VerticalContentAlignment = System.Windows.VerticalAlignment.Center, IsReadOnly = true, BorderBrush = this.Background });

        //                        stkMapInfoTypeChildData.Children.Add(stkMapInfoTypeChildDataItem);
        //                    }
        //                }
        //            }
        //        }

        //    }
        //}
        //private void BtnExpandAll_Click(object sender, RoutedEventArgs e)
        //{
        //    foreach (object obj in this.TvMapContents.Items)
        //        if (obj is TreeViewItem tvi)
        //            ExpandStateAll(tvi, true);
        //}
        //private void BtnCollapseAll_Click(object sender, RoutedEventArgs e)
        //{
        //    foreach (object obj in this.TvMapContents.Items)
        //        if (obj is TreeViewItem tvi)
        //            ExpandStateAll(tvi, false);
        //    foreach (object obj in this.TvMapContents.Items)
        //        if (obj is TreeViewItem tvi)
        //            tvi.IsExpanded = true;
        //}
        //private void ExpandStateAll(TreeViewItem items, bool expand)
        //{
        //    if (items.Visibility == Visibility.Collapsed) return;

        //    items.IsExpanded = expand;
        //    foreach (object obj in items.Items)
        //        if (obj is TreeViewItem tvi)
        //            ExpandStateAll(tvi, expand);
        //}
        //private void ExpandStateAllMakeVisible(TreeViewItem items, bool expand)
        //{
        //    items.Visibility = Visibility.Visible;
        //    items.IsExpanded = expand;
        //    foreach (object obj in items.Items)
        //        if (obj is TreeViewItem tvi)
        //            ExpandStateAllMakeVisible(tvi, expand);
        //}
        //private void ChkShowOnlyChildren_Click(object sender, RoutedEventArgs e)
        //{
        //    if (((CheckBox)sender).IsChecked.Value)
        //    {
        //        foreach (object obj in this.TvMapContents.Items)
        //            if (obj is TreeViewItem tviMain)
        //                foreach (object obj1 in tviMain.Items)
        //                    if (obj1 is TreeViewItem tvi)
        //                        if (tvi.Tag is bool val)
        //                            if (!val)
        //                            {
        //                                tvi.IsExpanded = false;
        //                                tvi.Visibility = Visibility.Collapsed;
        //                            }
        //    }
        //    else
        //    {
        //        foreach (object obj in this.TvMapContents.Items)
        //            if (obj is TreeViewItem tvi)
        //                ExpandStateAllMakeVisible(tvi, false);
        //        foreach (object obj in this.TvMapContents.Items)
        //            if (obj is TreeViewItem tvi)
        //                tvi.IsExpanded = true;
        //    }
        //}

        //private void LoadMapContentsRaw(string mapPath)
        //{
        //    SystemMap_Serializer.SystemMap serial = SystemMap_Serializer.Load($"{App.UserDataDirectory}SystemMap.xml");

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(rtfHead);

        //    using (StreamReader sr = new StreamReader(mapPath))
        //    {
        //        Hyperlink hl1 = new Hyperlink()
        //        {
        //            Tag = "Header",
        //            Foreground = Brushes.Black
        //        };
        //        hl1.Inlines.Add("Header");
        //        hl1.Click += MapSectorHyperLink_Click;

        //        SolidColorBrush b1 = new SolidColorBrush(System.Windows.Media.Colors.White) { Opacity = 0.0 };
        //        Label lb1 = new Label
        //        {
        //            Content = hl1,
        //            Tag = "Header",
        //            Background = b1
        //        };

        //        flpMapSections.Children.Add(lb1);

        //        string line;
        //        int lineNum = 0;
        //        while ((line = sr.ReadLine()) != null)
        //        {
        //            if (line.StartsWith("LINES"))
        //                break;

        //            lineNum++;

        //            foreach (SystemMap_Serializer.SystemMapSection sec in serial.Section)
        //            {
        //                foreach (Match m in Regex.Matches(line, sec.SearchText))
        //                {
        //                    if (sec.FirstLine == -1 && !sec.SubSection)
        //                    {
        //                        Hyperlink hl = new Hyperlink()
        //                        {
        //                            Tag = sec.SearchText,
        //                            Foreground = Brushes.Black
        //                        };
        //                        hl.Inlines.Add((string)sec.Label);
        //                        hl.Click += MapSectorHyperLink_Click;

        //                        SolidColorBrush b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(sec.HighlightColor)) { Opacity = 0.50 };
        //                        Label lb = new Label
        //                        {
        //                            Content = hl,
        //                            Tag = sec.SearchText,
        //                            Background = b
        //                        };

        //                        sec.FirstLine = lineNum;

        //                        flpMapSections.Children.Add(lb);
        //                    }

        //                    int offset = 0;
        //                    int i = (int)Enum.Parse(typeof(Colors), sec.HighlightColor);

        //                    line = line.Insert(m.Index + offset, rtfSearchFormatsColors[i]);
        //                    offset += rtfSearchFormatsColors[i].Length;
        //                    line = line.Insert(m.Index + m.Length + offset, rtfSearchFormatsColorsTail);
        //                    offset += rtfSearchFormatsColorsTail.Length;
        //                }
        //            }

        //            sb.Append(line + "\\par" + Environment.NewLine);
        //        }
        //    }
        //    sb.Append(rtfTail);

        //    //LoadRTF(sb.ToString(), rtbMap);

        //    tabMapContentsRaw.Visibility = Visibility.Visible;
        //}
        //private void MapSectorHyperLink_Click(object sender, RoutedEventArgs e)
        //{
        //    Hyperlink hyp = (Hyperlink)sender;

        //    if (hyp.Tag is string line)
        //    {
        //        if (line.Equals("Header"))
        //        {
        //            Rect r = rtbMap.Document.ContentStart.GetCharacterRect(LogicalDirection.Backward);
        //            rtbMap.ScrollToVerticalOffset(rtbMap.VerticalOffset + r.Y);
        //            return;
        //        }

        //        rtbMap.CaretPosition = rtbMap.Document.ContentStart;
        //        TextPointer txtP = rtbMap.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
        //        while (true)
        //        {
        //            char[] text = new char[line.Length];

        //            if (txtP.GetTextInRun(LogicalDirection.Forward, text, 0, line.Length) > 0)
        //            {
        //                string s = new string(text);
        //                if (s.Equals(line))
        //                {
        //                    Rect r = txtP.GetCharacterRect(LogicalDirection.Backward);
        //                    rtbMap.ScrollToVerticalOffset(rtbMap.VerticalOffset + r.Y);
        //                    return;
        //                }
        //            }

        //            TextPointer txtP1 = txtP.GetLineStartPosition(1, out int skipped).GetInsertionPosition(LogicalDirection.Forward);
        //            if (skipped == 1)
        //                txtP = txtP1;
        //            else
        //                return;
        //        }
        //    }

        //}

        ////private void MapLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        ////{


        ////    LinkLabel ll = (LinkLabel)sender;

        ////    rtbMap.SelectionStart = rtbMap.GetFirstCharIndexFromLine((int)ll.Tag - 1);
        ////    rtbMap.ScrollToCaret();

        ////}
        /// <summary>

        /// </summary>
 

        ////private void SetupMapMenu(string mapPath)
        ////{
        ////    if (HasMap)
        ////    {
        ////        MenuMap.Header = $"Map ({System.IO.Path.GetFileNameWithoutExtension(mapPath)})";
        ////        MenuMap.Visibility = Visibility.Visible;
        ////        MenuMap.Tag = mapPath;

        ////        MenuItem save = new MenuItem()
        ////        {
        ////            Header = "Save As",
        ////            Tag = mapPath
        ////        };
        ////        save.Click += SaveMapFile_Click;
        ////        MenuMap.Items.Add(save);

        ////        //if (HeatPoints.Count() > 0)
        ////        //{
        ////        //    MenuItem heat = new MenuItem()
        ////        //    {
        ////        //        Header = $"Create WiFi Heat Map: {HeatPoints.Count()} Points \u2248{(HeatPoints.Count() / 15) / 60} min.",
        ////        //        Tag = mapPath
        ////        //    };
        ////        //    heat.Click += CreateHeatMap_Click;
        ////        //    MenuMap.Items.Add(heat);
        ////        //}

        ////        //MenuItem dxMM;
        ////        //if (CheckDxMobileMapPath())
        ////        //{
        ////        //    dxMM = new MenuItem()
        ////        //    {
        ////        //        Header = "Create DXMobileMap Database",
        ////        //    };
        ////        //    dxMM.Click += MapDBCreateFile_Click;
        ////        //}
        ////        //else
        ////        //{
        ////        //    dxMM = new MenuItem()
        ////        //    {
        ////        //        Header = "Get DXMobileMap",
        ////        //    };
        ////        //    dxMM.Click += MapDBGetDxMobileMap_Click;
        ////        //}

        ////        //MenuMap.Items.Add(dxMM);
        ////    }
        ////    else
        ////    {
        ////        MenuMap.Header = "Map";
        ////        MenuMap.Visibility = Visibility.Collapsed;
        ////        MenuMap.Tag = string.Empty;

        ////        MenuMap.Items.Clear();
        ////    }
        ////}

        //private void CreateHeatMap_Click(object sender, RoutedEventArgs e)
        //{
        //    Thread thread = new Thread(() => DrawWiFiHeatMap(4096, 4096));
        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.Start();
        //}

        //private void SaveMapFile_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveMapFile();
        //}
        //private bool SaveMapFile()
        //{
        //    Microsoft.Win32.SaveFileDialog file = new Microsoft.Win32.SaveFileDialog
        //    {
        //        CheckFileExists = false,
        //        AddExtension = true,
        //        CheckPathExists = true,
        //        Filter = "Map file (*.map)|*.map",
        //        FilterIndex = 1
        //    };

        //    string filePath;
        //    if (file.ShowDialog() == true)
        //    {
        //        filePath = file.FileName;
        //    }
        //    else return false;

        //    MapFile.LoadMapFromFile();
        //    MapFile.FilePath = filePath;
        //    MapFile.WriteFileContents();

        //    return true;
        //}
        //public void MapDBGetDxMobileMap_Click(object sender, RoutedEventArgs e)
        //{
        //    Process.Start(@"https://github.com/ZeroxCorbin/DxMobileMap_WPF");
        //}
        ////public void MapDBCreateFile_Click(object sender, RoutedEventArgs e)
        ////{
        ////    Thread thread = new Thread(() => MapDBCreateFile_Thread());
        ////    thread.SetApartmentState(ApartmentState.STA);
        ////    thread.Start();
        ////}
        ////public void MapDBCreateFile_Thread()
        ////{
        ////    string dxPath = App.Settings.GetValue("DxMobileMapPath");
        ////    if (dxPath == null)
        ////    {
        ////        UpdateStatus("Please select the DxMobileMap.exe directory in the Application Settings.");
        ////        return;
        ////    }


        ////    string dxDBPath = GetDXMobileMapDatabasePath(dxPath);
        ////    if (!dxDBPath.EndsWith("\\")) dxDBPath += "\\";

        ////    if (dxDBPath == null)
        ////    {
        ////        UpdateStatus("Please run DxMobileMap.exe once to allow it's application settings to be created. Then run this command again.");
        ////        return;
        ////    }

        ////    if (!SaveMapFile())
        ////        return;

        ////    string dbPath = $"{dxDBPath}{MapFile.Map.MapFile.FileName}{MapDatabaseExtension}";

        ////    int i = 1;
        ////    while (File.Exists(dbPath))
        ////    {
        ////        dbPath = $"{dxDBPath}{MapFile.Map.MapFile.FileName}_{i++}{MapDatabaseExtension}";
        ////        if (i > 200)
        ////            return;
        ////    }

        ////    UpdateStatus($"Creating map database: {dbPath}");
        ////    using (SimpleDataBase mapDb = new SimpleDataBase().Init(dbPath, MapDatabaseTableName, true))
        ////    {
        ////        if (mapDb == null) return;

        ////        mapDb.SetValue(MapDatabaseFileKey, MapFile.Map.MapFile);
        ////        mapDb.SetValue($"{MapFile.Map.MapFile.LoadedUID}{MapDatabaseContentsDateKey}", MapFile.Map.MapFile.FileLastUpdated);
        ////        mapDb.SetValue($"{MapFile.Map.MapFile.LoadedUID}{MapDatabaseContentsHeaderKey}", MapFile.Map.Header);

        ////        MapFile.Map.MapFile.FileToContents();
        ////        mapDb.SetValue($"{MapFile.Map.MapFile.LoadedUID}{MapDatabaseContentsKey}", MapFile.Map.MapFile.Contents);
        ////        MapFile.Map.MapFile.Contents = "";

        ////        string bmp = MapUtils.GetBitmapString(Map, 220, 220);
        ////        mapDb.SetValue($"{MapFile.Map.MapFile.LoadedUID}_thumbnail", bmp);

        ////        if (MapFile.Map.LoggedStatus != null)
        ////            if (MapFile.Map.LoggedStatus.Count > 0)
        ////            {
        ////                mapDb.SetValue($"{MapDatabasePositionsKey}", MapFile.Map.LoggedStatus);
        ////                mapDb.SetValue("UseLoggedPositions", true);
        ////            }

        ////        if (MapFile.Map.LoggedWifi != null)
        ////            if (MapFile.Map.LoggedWifi.Count > 0)
        ////            {
        ////                mapDb.SetValue($"{MapDatabaseWifiKey}", MapFile.Map.LoggedWifi);
        ////                mapDb.SetValue("UseLoggedWiFi", true);
        ////            }

        ////        // MapFile.Map.MapFile.DatabasePath = dbPath;

        ////        if (File.Exists($"{dxPath}DxMobileMap_WPF.exe"))
        ////        {
        ////            UpdateStatus($"Launching DxMobileMap");

        ////            ProcessStartInfo proc = new ProcessStartInfo($"{dxPath}DxMobileMap_WPF.exe")
        ////            {
        ////                WorkingDirectory = dxPath
        ////            };
        ////            Process.Start(proc);
        ////        }

        ////        return;
        ////    }
        ////}

        ////private string GetDXMobileMapDatabasePath(string dxPath)
        ////{
        ////    if (File.Exists(dxPath + "UserData\\ApplicationSettings.sqlite"))
        ////        using (SimpleDataBase dxDb = new SimpleDataBase().Init(dxPath + "UserData\\ApplicationSettings.sqlite", false))
        ////        {
        ////            if (dxDb == null)
        ////                return null;

        ////            return dxDb.GetValue<string>("MapDatabaseDirectory", null);
        ////        }
        ////    else
        ////        return null;
        ////}

        //private void Mdd_FileDownloaded(string filePath)
        //{
        //    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) =>
        //    {
        //        WorkingPath = s + ".temp\\";
        //        ZipFilePath = s;
        //        //AddToHistory(ZipFilePath);

        //        Thread thread = new Thread(() => RunThread());
        //        thread.SetApartmentState(ApartmentState.STA);
        //        thread.Start();

        //    }), filePath);
        //}

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //rtbLogLines.SelectionChanged += RtbLogLines_SelectionChanged;

        //    //MenuItem men = new MenuItem()
        //    //{
        //    //    Header = "Open Zip File"
        //    //};
        //    //men.Click += MenuOpenZipFile_Click;
        //    //MenuFile.Items.Add(men);

        //    //MenuItem menF = new MenuItem()
        //    //{
        //    //    Header = "Open Folder"
        //    //};
        //    //menF.Click += MenuOpenFolder_Click;
        //    //MenuFile.Items.Add(menF);

        //    //MenuItem menR = new MenuItem()
        //    //{
        //    //    Header = "Open From LD/EM"
        //    //};
        //    //menR.Click += MenuOpenFromRobot_Click;
        //    //MenuFile.Items.Add(menR);

        //    //Separator sep = new Separator()
        //    //{
        //    //    Height = 3,
        //    //    Width = double.NaN,
        //    //    BorderThickness = new Thickness(1)
        //    //};
        //    //MenuFile.Items.Add(sep);

        //    //UpdateHistoryMenuItems(App.Settings.GetValue("FileHistory", new List<FileHistory>()));
        //}
        //private void Window_LocationChanged(object sender, EventArgs e)
        //{
        //    //if (FormLoading) return;
        //    //WindowSettings.Update(this.Top, this.Left, this.Width, this.Height, this.WindowState);
        //}
        //private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    //if (FormLoading) return;
        //    //WindowSettings.Update(this.Top, this.Left, this.Width, this.Height, this.WindowState);
        //}
        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    //App.Settings.SetValue("WindowSettings", WindowSettings);
        //}

        //private void Mf_FileOpened(string filePath)
        //{
        //    //string msg = $"Parsing file: {filePath}";
        //    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) =>
        //    //{
        //    //    //lblStatus.Content = s; 
        //    //    LstStatusList.Items.Insert(0, s);
        //    //}), msg);
        //    //Thread.Sleep(1);
        //}



        ////private void MenuOpenZipFile_Click(object sender, RoutedEventArgs e)
        ////{
        ////    Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog
        ////    {
        ////        CheckFileExists = true,
        ////        CheckPathExists = true,
        ////        Filter = "Zip file (*.zip)|*.zip",
        ////        FilterIndex = 1
        ////    };

        ////    if ((bool)file.ShowDialog())
        ////    {
        ////        WorkingPath = file.FileName + ".temp\\";
        ////        ZipFilePath = file.FileName;
        ////        AddToHistory(ZipFilePath);

        ////        Thread thread = new Thread(() => RunThread());
        ////        thread.SetApartmentState(ApartmentState.STA);
        ////        thread.Start();
        ////    }
        ////}
        ////private void MenuOpenFolder_Click(object sender, EventArgs e)
        ////{
        ////    //var fol = new System.Windows.Forms.FolderBrowserDialog
        ////    //{
        ////    //    ShowNewFolderButton = false
        ////    //};

        ////    //if (fol.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        ////    //{
        ////    //    WorkingPath = fol.SelectedPath + "\\";
        ////    //    ZipFilePath = null;
        ////    //    AddToHistory(WorkingPath);

        ////    //    Thread thread = new Thread(() => RunThread());
        ////    //    thread.SetApartmentState(ApartmentState.STA);
        ////    //    thread.Start();
        ////    //}
        ////}
        ////private void MenuOpenFromRobot_Click(object sender, RoutedEventArgs e)
        ////{
        ////    MobileDebugDownload mdd = new MobileDebugDownload
        ////    {
        ////        Owner = this,
        ////        WindowStartupLocation = WindowStartupLocation.CenterOwner
        ////    };
        ////    mdd.FileDownloaded += Mdd_FileDownloaded;
        ////    mdd.Show();
        ////}
        ////private void MenuSettings_Search_Click(object sender, RoutedEventArgs e)
        ////{
        ////    LogDetailsEditor win = new LogDetailsEditor
        ////    {
        ////        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        ////        Owner = this
        ////    };
        ////    win.Show();
        ////}
        ////private void MenuSettings_Application_Click(object sender, RoutedEventArgs e)
        ////{
        ////    ApplicationSettingsEditor appEd = new ApplicationSettingsEditor
        ////    {
        ////        Owner = this,
        ////        WindowStartupLocation = WindowStartupLocation.CenterOwner
        ////    };
        ////    appEd.Show();
        ////}

        ////private class FileHistory
        ////{
        ////    public string Path { get; set; }
        ////    public bool IsDirectory { get; set; } = false;
        ////}
        ////private void AddToHistory(string filePath)
        ////{
        ////    List<FileHistory> history = App.Settings.GetValue("FileHistory", new List<FileHistory>());

        ////    FileHistory fhFile;
        ////    if (!File.Exists(filePath) && !Directory.Exists(filePath))
        ////    {
        ////        IEnumerable<FileHistory> his = history.Where(s => s.Path.Equals(filePath));

        ////        foreach (FileHistory fh in history.ToList())
        ////            history.Remove(fh);
        ////    }
        ////    else
        ////    {
        ////        fhFile = new FileHistory()
        ////        {
        ////            Path = filePath,
        ////        };
        ////        if (File.Exists(filePath))
        ////            fhFile.IsDirectory = false;
        ////        else
        ////            fhFile.IsDirectory = true;

        ////        if (!history.Contains(fhFile))
        ////            history.Add(fhFile);
        ////    }

        ////    App.Settings.SetValue("FileHistory", history);

        ////    UpdateHistoryMenuItems(history);
        ////}
        ////private void UpdateHistoryMenuItems(List<FileHistory> history)
        ////{
        ////    int i = MenuFile.Items.Count - 1;

        ////    for (; i > 0; i--)
        ////    {
        ////        if (MenuFile.Items[i] is MenuItem mi)
        ////        {
        ////            mi.Click -= HistoryMenuItem_Click;
        ////            MenuFile.Items.Remove(mi);
        ////        }
        ////        else if (MenuFile.Items[i] is Separator)
        ////            break;
        ////    }

        ////    foreach (FileHistory filePath in history)
        ////    {
        ////        MenuItem tsm = new MenuItem
        ////        {
        ////            Header = filePath.IsDirectory ? System.IO.Path.GetDirectoryName(filePath.Path) : System.IO.Path.GetFileName(filePath.Path),
        ////            Tag = filePath
        ////        };
        ////        tsm.Click += HistoryMenuItem_Click;
        ////        MenuFile.Items.Add(tsm);
        ////    }
        ////}
        ////private void HistoryMenuItem_Click(object sender, RoutedEventArgs e)
        ////{
        ////    if (sender is MenuItem mi)
        ////    {
        ////        if (mi.Tag is FileHistory filePath)
        ////        {
        ////            if (!filePath.IsDirectory)
        ////            {
        ////                if (!File.Exists(filePath.Path))
        ////                {
        ////                    AddToHistory(filePath.Path);
        ////                    return;
        ////                }

        ////                WorkingPath = filePath.Path + ".temp\\";
        ////                ZipFilePath = filePath.Path;

        ////                Thread thread = new Thread(() => RunThread());
        ////                thread.SetApartmentState(ApartmentState.STA);
        ////                thread.Start();
        ////            }
        ////            else
        ////            {
        ////                if (!Directory.Exists(filePath.Path))
        ////                {
        ////                    AddToHistory(filePath.Path);
        ////                    return;
        ////                }

        ////                WorkingPath = filePath.Path;
        ////                ZipFilePath = null;

        ////                Thread thread = new Thread(() => RunThread());
        ////                thread.SetApartmentState(ApartmentState.STA);
        ////                thread.Start();
        ////            }
        ////        }
        ////    }
        ////}

        //private void ExpTOC_Collapsed(object sender, RoutedEventArgs e) => App.Settings.SetValue("ExpanderTOC", ((Expander)sender).IsExpanded);
        //private void ExpTOC_Expanded(object sender, RoutedEventArgs e) => App.Settings.SetValue("ExpanderTOC", ((Expander)sender).IsExpanded);

        //private void ExpSystemInfo_Collapsed(object sender, RoutedEventArgs e) => App.Settings.SetValue("ExpanderSystemInfo", ((Expander)sender).IsExpanded);
        //private void ExpSystemInfo_Expanded(object sender, RoutedEventArgs e) => App.Settings.SetValue("ExpanderSystemInfo", ((Expander)sender).IsExpanded);

        //private void ExpMapPointsOffMap_Collapsed(object sender, RoutedEventArgs e)
        //{

        //}

        //private void ExpMapPointsOffMap_Expanded(object sender, RoutedEventArgs e)
        //{

        //}

        //private void BtnSaveMapImage_Click(object sender, RoutedEventArgs e)
        //{
        //    //SaveBMPtoJPGFile((BitmapSource)((Image)ImgMapBorder.Child).Source);
        //}

        //private void BtnSaveWiFiHeatMapImage_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveBMPtoJPGFile((BitmapSource)((Image)ImgMapWiFiHeatBorder.Child).Source);
        //}

        //private void SaveBMPtoJPGFile(BitmapSource source)
        //{
        //    Microsoft.Win32.SaveFileDialog file = new Microsoft.Win32.SaveFileDialog
        //    {
        //        CheckFileExists = false,
        //        AddExtension = true,
        //        CheckPathExists = true,
        //        Filter = "Image File (*.jpg)|*.jpg",
        //        FilterIndex = 1
        //    };

        //    string filePath;
        //    if (file.ShowDialog() == true)
        //        filePath = file.FileName;
        //    else return;

        //    var encoder = new PngBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(source));
        //    using (FileStream stream = new FileStream(filePath, FileMode.Create))
        //        encoder.Save(stream);
        //}

        ////private void BtnSaveWiFiDecibelsGraphImage_Click(object sender, RoutedEventArgs e)
        ////{
        ////    SaveBMPtoJPGFile(Chart_WiFiDecibels.ToBitmap());
        ////}

        ////private void BtnSaveWiFiBaudGraphImage_Click(object sender, RoutedEventArgs e)
        ////{
        ////    SaveBMPtoJPGFile(Chart_WiFiBaud.ToBitmap());
        ////}

        //private void BtnSaveBatteryVoltageGraphImage_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveBMPtoJPGFile(Chart_BatteryVoltage.ToBitmap());
        //}

        //private void BtnSaveBatteryChargeStateGraphImage_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveBMPtoJPGFile(Chart_BatteryChargeState.ToBitmap());
        //}




    }
}
