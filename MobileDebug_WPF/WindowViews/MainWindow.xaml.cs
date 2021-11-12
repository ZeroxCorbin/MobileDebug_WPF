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
using static FileSearch.FileSearch;
using OxyPlot.Legends;
using ControlzEx.Theming;
using MahApps.Metro.Controls;

namespace MobileDebug_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public class LogDetails_class
        {
            public string LogFileFullName { get; set; }
            public IList<IEnumerable<FileSearchResults>> SearchResults { get; set; } = new List<IEnumerable<FileSearchResults>>();

            public LogDetails_Serializer.LogDetailsLog Log { get; set; }
        }

        private Brush ButtonFace { get; set; }

        //Set by CheckProductType() if the debug data is for an EM.
        public bool IsEM { get; private set; }
        public bool HasMap { get; private set; }
        private MapFile MapFile { get; set; }

        //If a zip file is opened this is the path and name.
        public string ZipFilePath { get; private set; }
        //This is either the user selected folder or the extracted folder of the ZIP file.
        private string WorkingPath { get; set; }
        private string AppPath => System.AppDomain.CurrentDomain.BaseDirectory;

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
        public MainWindow()
        {
            //ThemeManager.Current.ThemeChanged += Current_ThemeChanged;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();

            InitializeComponent();

            DataContext = new WindowViewModel.MainWindowViewModel(MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance);
            SystemInformationExpander.DataContext = ((WindowViewModel.MainWindowViewModel)DataContext).SystemInformation;
            TableOfContentsExpander.DataContext = ((WindowViewModel.MainWindowViewModel)DataContext).TableOfContents;

            _ = SetBinding(WidthProperty, new Binding("Width") { Source = DataContext, Mode = BindingMode.TwoWay });
            _ = SetBinding(HeightProperty, new Binding("Height") { Source = DataContext, Mode = BindingMode.TwoWay });

            ButtonFace = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));

            //WindowStartupLocation = WindowStartupLocation.Manual;

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

            CheckForSearchConfigFiles();

            CheckDxMobileMapPath();

            //MenuItem mi = new MenuItem()
            //{
            //    Header = "Log Searches",
            //    Tag = App.UserDataDirectory + "LogDetails.xml",
            //};
            //mi.Click += MenuSettings_Search_Click;
            //MenuSettings_Search.Items.Add(mi);

           // ExpTOC.IsExpanded = App.Settings.GetValue("ExpanderTOC", false);

            tabCrashLogs.Visibility = Visibility.Collapsed;
            tabBatteryLogs.Visibility = Visibility.Collapsed;
            tabWiFiLogs.Visibility = Visibility.Collapsed;
            tabMapContents.Visibility = Visibility.Collapsed;
            tabMapContentsRaw.Visibility = Visibility.Collapsed;
            tabMapImage.Visibility = Visibility.Collapsed;
            tabMapWiFiHeat.Visibility = Visibility.Collapsed;

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

        private void CheckForSearchConfigFiles()
        {
            string[] targetFiles = Directory.GetFiles(App.UserDataDirectory, "*.xml");
            foreach (string fileName in Directory.GetFiles(SearchConfigurationPath, "*.xml"))
            {
                bool found = false;
                foreach (string target in targetFiles)
                    if (System.IO.Path.GetFileName(target).Equals(System.IO.Path.GetFileName(fileName)))
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    File.Copy(fileName, $"{App.UserDataDirectory}{System.IO.Path.GetFileName(fileName)}");
            }
        }



        public delegate void Run();

        private void InvokeThis(Action act)
        {
            Console.WriteLine("Invoke: " + act.Method.Name);
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, act);
        }
        private void RunThread()
        {
            //InvokeThis(new Action(() => { MenuFile.IsEnabled = false; }));

            InvokeThis(new Action(() => { ClearForm(true); }));

            if (!string.IsNullOrEmpty(ZipFilePath))
            {
                UpdateStatus("Extracting Files...");
                if (!ExtractFile())
                {
                    //InvokeThis(new Action(() => { MenuFile.IsEnabled = true; }));
                    UpdateStatus("Extraction Error!");
                }
            }

            try
            {
                CheckProductType();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

           // UpdateStatus("Loading system data...");
            //InvokeThis(new Action(() => { LoadSystemHealth(); }));

            //InvokeThis(new Action(() => { LoadSystemDetails(); }));

            //InvokeThis(new Action(() => { LoadSystemApps(); }));

            //UpdateStatus("Loading Table of Contents...");
            //InvokeThis(new Action(() => { ReadTOC(); }));

            UpdateStatus("Setting up log searches...");
            InvokeThis(new Action(() => { SetupLogs(); }));

            UpdateStatus("Setting up crash logs...");
            InvokeThis(new Action(() => { SetupCrashLogs(); }));

            if (!IsEM)
            {
                InvokeThis(new Action(() => { tabBatteryLogs.Visibility = Visibility.Visible; }));

                UpdateStatus("Setting up battery graphs...");
                InvokeThis(new Action(() => { LoadBatteryLogs(); }));
            }

            if (!IsEM)
            {
                InvokeThis(new Action(() => { tabWiFiLogs.Visibility = Visibility.Visible; }));

                if (!IsEM)
                {
                    UpdateStatus("Setting up WiFi graphs...");
                    InvokeThis(new Action(() => { LoadWiFiLogs(); }));
                }
            }

            DirectoryInfo di = new DirectoryInfo(WorkingPath + "home\\admin");
            HasMap = false;
            MapFile = null;
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name.EndsWith(".map"))
                {
                    UpdateStatus($"Map found: {fi.Name} - Loading Contents...");
                    MapFile = new MapFile(fi.FullName, false);
                    MapFile.LoadMapFromFile();

                    HasMap = true;
                    InvokeThis(new Action(() => { LoadMapContents(); }));
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((path) => { LoadMapContentsRaw(path); }), MapFile.FilePath);

                    if (!IsEM)
                        LoadLoggedDataThread();

                    break;
                }
            }

            string mapPath = HasMap ? MapFile.FilePath : string.Empty;

            if (HasMap)
            {


                //if (MapFile.Map.MapFile.LoggedWifi != null)
                //{
                //    UpdateStatus("Building WiFi Heatmap points...");
                //    System.Drawing.Rectangle r = new System.Drawing.Rectangle((int)MapFile.Map.Header.WidthOffset, (int)MapFile.Map.Header.HeightOffset, (int)MapFile.Map.Header.Width, (int)MapFile.Map.Header.Height);

                //    foreach (WifiLogs.WifiLogData s in MapFile.Map.MapFile.LoggedWifi)
                //    {
                //        if (r.Contains(s.Position))
                //        {
                //            //HeatPoints.Add(new HeatPoint(s.Position, (byte)Math.Abs(s.Baud)));
                //            if (HeatPoints.Count() == 0)
                //            {
                //                HeatPoints.Add(new HeatPoint(s.Position, (byte)Math.Abs(s.Baud)));
                //                continue;
                //            }

                //            int i = 0;
                //            bool found = false;
                //            foreach (HeatPoint hp in HeatPoints.ToArray())
                //            {
                //                if (new System.Drawing.Rectangle(hp.X, hp.Y, 500, 500).Contains(s.Position))
                //                {
                //                    HeatPoints[i] = new HeatPoint(hp.Point, (byte)((hp.Intensity + Math.Abs(s.Baud)) / 2));
                //                    found = true;
                //                    break;
                //                }
                //                i++;
                //            }
                //            if (!found)
                //            {
                //                HeatPoints.Add(new HeatPoint(s.Position, (byte)Math.Abs(s.Baud)));
                //            }
                //        }

                //    }


                //}

                UpdateStatus("Drawing Map");

                string bmp = MapUtils.GetBitmapString(MapFile.Map, 4096, 4096);

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) => { DrawMapApplyToImage(s); }), bmp);

                //UpdateStatus("Setting up Map menu...");
               // Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) => { SetupMapMenu(s); }), mapPath);

                //InvokeThis(new Action(() => { MenuFile.IsEnabled = true; }));
            }
            UpdateStatus("Complete!");
        }


        private void UpdateStatus(string msg)
        {
            Console.WriteLine(msg);

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) =>
            {

                //LstStatusList.Items.Insert(0, s);
            }), msg);
            //Thread.Sleep(1);
        }

        public delegate Map LoadLoggedDataDelegate(Map map);

        private void LoadLoggedDataThread()
        {
            MapFile.Map.LoggedWifi = new Dictionary<string, List<MobileLogs.WifiLogs.WifiLogData>>();
            MobileLogs.MultiFile mf = new MobileLogs.MultiFile("wifiLog", WorkingPath + "var\\log");
            mf.FileOpened += Mf_FileOpened;

            var wifiFiles = mf.GetResults<MobileLogs.WifiLogs>();
            foreach (MobileLogs.WifiLogs logFile in wifiFiles)
                foreach (KeyValuePair<string, List<MobileLogs.WifiLogs.WifiLogData>> res in logFile.Results)
                {
                    if (MapFile.Map.LoggedWifi.ContainsKey(res.Key))
                    {
                        MapFile.Map.LoggedWifi[res.Key].AddRange(res.Value);
                    }
                    else
                    {
                        MapFile.Map.LoggedWifi.Add(res.Key, res.Value);
                    }
                }


            MapFile.Map.LoggedStatus = new List<MobileLogs.StatusLogs.Status>();
            mf = new MobileLogs.MultiFile("log_", WorkingPath + "\\var\\robot\\logs");
            mf.FileOpened += Mf_FileOpened;

            var statusFiles = mf.GetResults<MobileLogs.StatusLogs>();
            foreach (MobileLogs.StatusLogs status in statusFiles)
                foreach (MobileLogs.StatusLogs.Status dat in status.Results)
                {
                    if (dat.Position.X == 0 & dat.Position.Y == 0 & dat.Heading == 0)
                        continue;
                    MapFile.Map.LoggedStatus.Add(dat);
                }
        }

        //private void LoadLoggedDataCallBack(IAsyncResult result)
        //{
        //    AsyncResult ar = (AsyncResult)result;
        //    LoadLoggedDataDelegate bp = (LoadLoggedDataDelegate)ar.AsyncDelegate;
        //    Map map = bp.EndInvoke(result);


        //}

        private void ClearForm(bool removeOldFiles)
        {
            //TvSystemData.Items.Clear();

            //stkTOC.Children.Clear();

            flpLogs.Children.Clear();
            rtbLogLines.Document.Blocks.Clear();
            TxtLogLinesPrev.Text = string.Empty;
            LogDetails = new List<LogDetails_class>();
            LogIndices = new LogIndices();

            tabCrashLogs.Visibility = Visibility.Collapsed;
            flpCrashLogs.Children.Clear();
            TxtCrashLogLines.Text = string.Empty;

            tabBatteryLogs.Visibility = Visibility.Collapsed;
            flpBatteryLogs.Children.Clear();
            LblPlotBatteryVoltageTitle.Content = string.Empty;
            BtnSaveBatteryVoltageGraphImage.Visibility = Visibility.Collapsed;
            LblPlotBatteryChargeStateTitle.Content = string.Empty;
            BtnSaveBatteryChargeStateGraphImage.Visibility = Visibility.Collapsed;
            BtnBatteryLogsViewAll.Background = ButtonFace;
            Chart_BatteryChargeState.Model = null;
            Chart_BatteryVoltage.Model = null;
            foreach (Control c in flpBatteryLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            tabWiFiLogs.Visibility = Visibility.Collapsed;
            flpWiFiLogs.Children.Clear();
            LblWiFiPlotDecibelsTitle.Content = string.Empty;
            //BtnSaveWiFiDecibelsGraphImage.Visibility = Visibility.Collapsed;
            LblWiFiPlotBaudTitle.Content = string.Empty;
            //BtnSaveWiFiBaudGraphImage.Visibility = Visibility.Collapsed;
            BtnWiFiLogsViewAll.Background = ButtonFace;
            Chart_WiFiBaud.Model = null;
            Chart_WiFiDecibels.Model = null;
            foreach (Control c in flpWiFiLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            tabMapContents.Visibility = Visibility.Collapsed;
            TvMapContents.Items.Clear();

            tabMapContentsRaw.Visibility = Visibility.Collapsed;
            rtbMap.Document.Blocks.Clear();

            tabMapImage.Visibility = Visibility.Collapsed;
            //ImgMapBorder.Child = null;

            tabMapWiFiHeat.Visibility = Visibility.Collapsed;
            ImgMapWiFiHeatBorder.Child = null;
            HeatPoints.Clear();

            //MenuMap.Visibility = Visibility.Collapsed;
            //MenuMap.Header = "Map";
            //MenuMap.Tag = string.Empty;
            //MenuMap.Items.Clear();

            IsEM = false;

            if (removeOldFiles)
                if (ZipFilePath != null)
                    if (WorkingPath != "")
                        if (!RemoveWorkDir())
                            UserMessage.Show(this, "The extraction directory already exists and could not be removed.", "Ooops!", new List<string>() { "Ok" });


            GC.Collect();
        }

        private bool ExtractFile()
        {
            try
            {
                ZipFile.ExtractToDirectory(ZipFilePath, WorkingPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private bool RemoveWorkDir()
        {
            bool err;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    err = false;
                    Directory.Delete(WorkingPath, true);
                }
                catch
                {
                    err = true;
                    Thread.Sleep(50);
                }
                if (err == false) break;
            }

            if (Directory.Exists(WorkingPath) == true) return false;
            else return true;
        }

        private string GetLineFromFile(string filePath)
        {
            try
            {
                using (StreamReader file = new StreamReader(filePath))
                {
                    return file.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        private void CheckProductType()
        {
            if (GetLineFromFile(WorkingPath + "mnt\\status\\platform\\productType").Contains("EM")) IsEM = true;
            else IsEM = false;
        }
        //private void LoadSystemHealth()
        //{
        //    SystemHealth_Serializer.SystemHealth serial = SystemHealth_Serializer.Load($"{App.UserDataDirectory}SystemHealth.xml");

        //    foreach (SystemHealth_Serializer.SystemHealthHeading head in serial.Heading)
        //    {
        //        if (IsEM && !head.isEM) continue;//If the data is from an EM and the config file indicates the data is not relevant to an EM, the section is ignored.
        //        if (!IsEM && !head.isLD) continue;

        //        TreeViewItem tviHeading = new TreeViewItem()
        //        {
        //            Header = head.Name,
        //            ToolTip = new ToolTip() { Visibility = Visibility.Collapsed },
        //        };
        //        TvSystemData.Items.Add(tviHeading);

        //        foreach (SystemHealth_Serializer.SystemHealthHeadingLabel label in head.Label)
        //        {
        //            if (IsEM && !label.isEM) continue;
        //            if (!IsEM && !label.isLD) continue;

        //            string line = GetLineFromFile(WorkingPath + label.FilePath);
        //            double res = 0;
        //            if (line != null)
        //                res = Convert.ToDouble(line) * Convert.ToDouble(label.Multiplier);

        //            TextBox txtLabel = new TextBox()
        //            {
        //                Text = label.Name + res.ToString() + label.Tail,
        //                ToolTip = new ToolTip() { Visibility = Visibility.Collapsed },
        //                IsReadOnly = true,
        //                BorderThickness = new Thickness(0),
        //                Margin = new Thickness(3)
        //            };
        //            TreeViewItem tviLabel = new TreeViewItem()
        //            {
        //                Header = txtLabel,
        //                ToolTip = new ToolTip() { Visibility = Visibility.Collapsed },
        //            };
        //            tviHeading.Items.Add(tviLabel);

        //            double thres;

        //            if (IsEM) thres = Convert.ToDouble(label.Threshold_em);
        //            else thres = Convert.ToDouble(label.Threshold_ld);

        //            if (label.Greater)
        //            {
        //                if (res > thres)
        //                    txtLabel.Background = Brushes.LightYellow;
        //                else
        //                    txtLabel.Background = Brushes.LightGreen;
        //            }
        //            else
        //            {
        //                if (res < thres)
        //                    txtLabel.Background = Brushes.LightYellow;
        //                else
        //                    txtLabel.Background = Brushes.LightGreen;
        //            }

        //        }

        //        tviHeading.IsExpanded = true;
        //    }
        //}//Updated
        //private void LoadSystemDetails()
        //{
        //    SystemDetails_Serializer.SystemDetails serial = SystemDetails_Serializer.Load($"{App.UserDataDirectory}SystemDetails.xml");

        //    foreach (SystemDetails_Serializer.SystemDetailsHeading head in serial.Heading)
        //    {
        //        if (IsEM && !head.isEM) continue;//If the data is from an EM and the config file indicates the data is not relevant to an EM, the section is ignored.
        //        if (!IsEM && !head.isLD) continue;

        //        TreeViewItem tviHeading = new TreeViewItem()
        //        {
        //            Header = head.Name,
        //            ToolTip = new ToolTip() { Visibility = Visibility.Collapsed },
        //        };
        //        TvSystemData.Items.Add(tviHeading);

        //        foreach (SystemDetails_Serializer.SystemDetailsHeadingLabel label in head.Label)
        //        {
        //            if (IsEM && !label.isEM) continue;
        //            if (!IsEM && !label.isLD) continue;

        //            string line = GetLineFromFile(WorkingPath + label.FilePath);
        //            TextBox txtLabel = new TextBox()
        //            {
        //                IsReadOnly = true,
        //                ToolTip = new ToolTip() { Visibility = Visibility.Collapsed },
        //                BorderThickness = new Thickness(0),
        //                Margin = new Thickness(3)
        //            };

        //            if (line != null)
        //                txtLabel.Text = label.Name + line.Replace("\t", " , ");
        //            else
        //                txtLabel.Text = "File Not Found";

        //            TreeViewItem tviLabel = new TreeViewItem()
        //            {
        //                Header = txtLabel,
        //                ToolTip = new ToolTip() { Visibility = Visibility.Collapsed },
        //            };
        //            tviHeading.Items.Add(tviLabel);
        //        }

        //        tviHeading.IsExpanded = true;
        //    }
        //}//Updated
        //private void LoadSystemApps()
        //{
        //    SystemApps_Serializer.SystemApps serial = SystemApps_Serializer.Load($"{App.UserDataDirectory}SystemApps.xml");

        //    DirectoryInfo di = new DirectoryInfo(WorkingPath + serial.Path);

        //    TreeViewItem tviHeading = new TreeViewItem()
        //    {
        //        Header = serial.Title,
        //    };
        //    TvSystemData.Items.Add(tviHeading);

        //    foreach (DirectoryInfo dir in di.GetDirectories())
        //    {
        //        foreach (FileInfo fi in dir.GetFiles())
        //        {
        //            if (fi.Name == serial.FileName)
        //            {
        //                using (StreamReader file = new System.IO.StreamReader(fi.FullName))
        //                {
        //                    string line = file.ReadLine();

        //                    TextBox txtLabel = new TextBox() { Text = line, IsReadOnly = true, BorderThickness = new Thickness(0), Margin = new Thickness(3) };
        //                    TreeViewItem tviLabel = new TreeViewItem()
        //                    {
        //                        Header = txtLabel,
        //                    };
        //                    tviHeading.Items.Add(tviLabel);

        //                    while ((line = file.ReadLine()) != null)
        //                    {
        //                        TextBox txtLabel1 = new TextBox() { Text = line, IsReadOnly = true, BorderThickness = new Thickness(0), Margin = new Thickness(3) };
        //                        TreeViewItem tviLabel1 = new TreeViewItem()
        //                        {
        //                            Header = txtLabel1,
        //                        };
        //                        tviLabel.Items.Add(tviLabel1);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    tviHeading.IsExpanded = true;
        //}//Updated

        //Table of Contents TAB
        //private void ReadTOC()
        //{
        //    using (StreamReader file = new StreamReader(WorkingPath + "toc.txt"))
        //    {
        //        file.ReadLine();
        //        file.ReadLine();
        //        file.ReadLine();


        //        string line;
        //        while ((line = file.ReadLine()) != null)
        //        {
        //            string[] row = new string[3];

        //            int i = line.LastIndexOf(' ');
        //            row[0] = line.Substring(i + 1);

        //            if (row[0].ToString().EndsWith("/")) continue;
        //            if (row[0].ToString().StartsWith("-")) break;

        //            row[1] = line.Substring(i - 18, 10);
        //            row[2] = line.Substring(i - 7, 5);

        //            Hyperlink hyp = new Hyperlink()
        //            {
        //                Tag = new Uri(WorkingPath + row[0]).ToString(),
        //            };
        //            hyp.Inlines.Add(row[0]);
        //            hyp.Click += TOCHyperlink_Click;

        //            Label lbl = new Label()
        //            {
        //                Content = hyp,
        //            };
        //            stkTOC.Children.Add(lbl);

        //        }
        //    }
        //}//Updated
        //private void TOCHyperlink_Click(object sender, RoutedEventArgs e)
        //{
        //    Hyperlink hl = (Hyperlink)sender;
        //    if (hl.Tag is string s)
        //        System.Diagnostics.Process.Start(s);
        //}

        //private void tsbSearchTOC_Click(object sender, EventArgs e)
        //{


        //    foreach (DataGridViewRow dr in dgvTOC.Items)
        //    {
        //        if (Regex.IsMatch((string)dr.Cells[0].Value, tstbSearchTOC.Text, RegexOptions.IgnoreCase))
        //        {
        //            dr.Visible = true;
        //        }
        //        else
        //        {
        //            dr.Visible = false;
        //        }
        //    }
        //}

        //private void tsbSearchTOCReset_Click(object sender, EventArgs e)
        //{


        //    foreach (DataGridViewRow dr in dgvTOC.Items)
        //    {
        //        dr.Visible = true;
        //    }
        //}

        //private void tstbSearchTOC_KeyDown(object sender, KeyEventArgs e)
        //{


        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        tsbSearchTOC_Click(sender, new EventArgs());
        //    }
        //}


        //Logs TAB

        private void SetupLogs()
        {
            LogDetails_Serializer.LogDetails serial = LogDetails_Serializer.Load($"{App.UserDataDirectory}LogDetails.xml");

            int i = -1;
            foreach (LogDetails_Serializer.LogDetailsLog log in serial.Log)
            {
                if (IsEM && !log.isEM) continue;
                if (!IsEM && !log.isLD) continue;

                IList<FileInfo> lst = new List<FileInfo>();
                if (log.MultiLog)
                {
                    DirectoryInfo dir = new DirectoryInfo(WorkingPath + log.FilePath);
                    IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);
                    IEnumerable<FileInfo> res =
                        from test in names
                        where test.Name.StartsWith(log.FileName)
                        select test;
                    lst = res.ToList<FileInfo>();
                }
                else lst.Add(new FileInfo(WorkingPath + log.FilePath + log.FileName));

                foreach (FileInfo file in lst)
                {
                    LogIndices ind = new LogIndices();
                    bool first = true;
                    int ii = -1;

                    IEnumerable<FileSearchResults> searchRes = Enumerable.Empty<FileSearchResults>();

                    foreach (LogDetails_Serializer.LogDetailsLogSearch ser in log.Search)
                    {
                        ii++;

                        if (IsEM && !ser.isEM) continue;
                        if (!IsEM && !ser.isLD) continue;

                        //UpdateStatus("Processing log for (" + ser.RegEx2Match + "): " + file.Name);

                        searchRes = FileSearch.FileSearch.Find(file.FullName, ser.RegEx2Match, true);

                        if (searchRes.Count() == 0) continue;

                        i++;

                        LogDetails_class ld = new LogDetails_class
                        {
                            Log = log,
                            LogFileFullName = file.FullName
                        };
                        LogDetails.Add(ld);

                        ind.log = i;
                        ind.search = ii;

                        if (first)
                        {
                            Hyperlink hl = new Hyperlink()
                            {
                                Tag = log.MultiLog ? file.Name : ld.Log.DisplayName,
                            };
                            hl.Inlines.Add((string)hl.Tag);
                            hl.Click += LogHyperLink_Click;

                            Label lb = new Label
                            {
                                Tag = ind,
                                Content = hl
                            };
                            flpLogs.Children.Add(lb);

                            first = false;
                        }

                        Button but = new Button
                        {
                            Tag = ind,
                            Content = log.Search[ii].DisplayName,
                            Margin = new Thickness(3)
                        };
                        but.Click += LogButton_Click;

                        flpLogs.Children.Add(but);
                    }
                }
            }
        }
        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Control c in flpLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            Button btn = (Button)sender;
            LogIndices = (LogIndices)btn.Tag;
            btn.Background = Brushes.LightGreen;

            IList<string> data = new List<string>();

            IEnumerable<FileSearchResults> searchRes = FileSearch.FileSearch.Find(LogDetails[LogIndices.log].LogFileFullName, LogDetails[LogIndices.log].Log.Search[LogIndices.search].RegEx2Match);
            LogDetails[LogIndices.log].SearchResults.Clear();
            LogDetails[LogIndices.log].SearchResults.Add(searchRes);

            foreach (FileSearchResults res in searchRes)
            {
                string str = res.LineNumber.ToString() + "\\tab " + res.Line;

                int i = (int)Enum.Parse(typeof(Levels), LogDetails[LogIndices.log].Log.Search[LogIndices.search].Level);

                int offset = 0;
                foreach (Match m in Regex.Matches(str, LogDetails[LogIndices.log].Log.Search[LogIndices.search].RegEx2Match))
                {
                    str = str.Insert(m.Index + offset, rtfSearchFormatsLevel[i]);
                    offset += rtfSearchFormatsLevel[i].Length;
                    str = str.Insert(m.Index + m.Length + offset, rtfSearchFormatsLevelTail);
                    offset += rtfSearchFormatsLevelTail.Length;
                }
                data.Add(str);
            }

            string rtf = rtfHead;
            foreach (string ln in data)
                rtf += ln + "\\par" + Environment.NewLine;
            rtf += rtfTail;

            LoadRTF(rtf, rtbLogLines);
        }
        private void LoadRTF(string rtf, RichTextBox richTextBox)
        {
            TxtLogLinesPrev.Text = string.Empty;

            if (string.IsNullOrEmpty(rtf))
                throw new ArgumentNullException();

            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

            //Create a MemoryStream of the Rtf content
            using (MemoryStream rtfMemoryStream = new MemoryStream())
            {
                using (StreamWriter rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                {
                    rtfStreamWriter.Write(rtf);
                    rtfStreamWriter.Flush();
                    rtfMemoryStream.Seek(0, SeekOrigin.Begin);

                    //Load the MemoryStream into TextRange ranging from start to end of RichTextBox.
                    textRange.Load(rtfMemoryStream, DataFormats.Rtf);
                }
            }
        }
        private void RtbLogLines_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextPointer txtP = rtbLogLines.CaretPosition.GetLineStartPosition(0).GetInsertionPosition(LogicalDirection.Forward);
            int logLineNumber = -1;
            bool found = false;
            while (!found)
            {
                char[] text = new char[10];

                if (txtP.GetTextInRun(LogicalDirection.Forward, text, 0, 10) > 0)
                {
                    foreach (char c in text)
                        if (c.Equals('\t'))
                        {
                            found = true;
                            break;
                        }
                    if (found)
                    {
                        string output = new string(text.TakeWhile(char.IsDigit).ToArray());
                        int.TryParse(output, out logLineNumber);
                        break;
                    }

                }
                else
                    return;

                TextPointer txtP1 = txtP.GetLineStartPosition(-1, out int skipped).GetInsertionPosition(LogicalDirection.Forward);
                if (skipped == -1)
                    txtP = txtP1;
                else
                    return;
            }

            //int currentLineNumber;
            //txtP.GetLineStartPosition(-int.MaxValue, out int lineMoved);
            //currentLineNumber = -lineMoved;
            bool found1 = false;
            FileSearchResults res = null;
            foreach (FileSearchResults res1 in LogDetails[LogIndices.log].SearchResults[0])
                if (res1.LineNumber == logLineNumber)
                {
                    found1 = true;
                    res = res1;
                    break;
                }
            if (!found1) return;

            TxtLogLinesPrev.Text = res.Buffer.GetHead();
            for (int i = 0; i != 4; i++)
                TxtLogLinesPrev.Text += res.Buffer.GetNext();
        }
        private void LogHyperLink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;

            if (hl.Parent is Label lbl)
            {
                if (lbl.Tag is LogIndices li)
                {
                    string fileName;
                    if (LogDetails[li.log].Log.MultiLog) fileName = (string)hl.Tag;
                    else fileName = LogDetails[li.log].Log.FileName;

                    System.Diagnostics.Process.Start(WorkingPath + LogDetails[li.log].Log.FilePath + fileName);
                }
            }
        }

        //private void HighlightResults(Match m, System.Drawing.Color c)
        //{
        //    rtbLogLines.Select(m.Index, m.Length);
        //    rtbLogLines.SelectionColor = c;
        //}

        //private void rtbLogLines_SelectionChanged(object sender, EventArgs e)
        //{


        //    FileSearchResults res;

        //    try
        //    {
        //        res = LogDetails[LogIndices.log].SearchResults[0].ElementAt(rtbLogLines.CurrentLineNumber);
        //    }
        //    catch
        //    {
        //        return;
        //    }

        //    rtbLogLinesPrev.Text = res.Buffer.GetHead();
        //    for (int i = 0; i != 4; i++)
        //    {
        //        rtbLogLinesPrev.Text += res.Buffer.GetNext();
        //    }
        //}

        //private void rtbLogLines_MouseDoubleClick(object sender, MouseEventArgs e)
        //{


        //    if (!ModifierKeys.HasFlag(System.Windows.Forms.Keys.Shift)) return;

        //    using (frmRTF rtf = new frmRTF())
        //    {
        //        rtf.workPath = WorkingPath;
        //        rtf.logDetails = logDetails[logIndices.log];
        //        rtf.logIndices = logIndices;
        //        rtf.logLineNumber = rtbLogLines.CurrentLogLineNumber;

        //        rtf.ShowDialog();
        //    }

        //}

        //WiFi TAB

        private void SetupCrashLogs()
        {
            DirectoryInfo dir = new DirectoryInfo(WorkingPath + @"\var\robot\logs\");
            IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);
            IEnumerable<FileInfo> res =
                from test in names
                where test.Name.StartsWith("crash")
                select test;
            IList<FileInfo> lst = res.ToList<FileInfo>();

            foreach (FileInfo file in lst)
            {

                Hyperlink hl = new Hyperlink()
                {
                    Tag = file.FullName,
                };
                hl.Inlines.Add(System.IO.Path.GetFileName(file.FullName));
                hl.Click += CrashLogHyperLink_Click;

                Label lb = new Label
                {
                    Tag = file.FullName,
                    Content = hl
                };
                flpCrashLogs.Children.Add(lb);

                Button but = new Button
                {
                    Tag = file.FullName,
                    Content = System.IO.Path.GetFileNameWithoutExtension(file.FullName),
                    Margin = new Thickness(3),
                    MinWidth = 74
                };
                but.Click += CrashLogButton_Click;

                flpCrashLogs.Children.Add(but);

                tabCrashLogs.Visibility = Visibility.Visible;
            }
        }
        private void CrashLogHyperLink_Click(object sender, RoutedEventArgs e)
        {
            if (((Hyperlink)sender).Tag is string path)
                System.Diagnostics.Process.Start(path);
        }
        private void CrashLogButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Control c in flpCrashLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            Button btn = (Button)sender;
            string filePath = (string)btn.Tag;
            btn.Background = Brushes.LightGreen;

            TxtCrashLogLines.Text = File.ReadAllText(filePath);
        }

        private void LoadWiFiLogs()
        {
            IList<FileInfo> lst = new List<FileInfo>();

            DirectoryInfo dir = new DirectoryInfo(WorkingPath + "var\\log\\");

            IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

            IEnumerable<FileInfo> res =
                from test in names
                where test.Name.StartsWith("wifiLog")
                select test;

            lst = res.ToList<FileInfo>();

            foreach (FileInfo log in lst)
            {
                Hyperlink hl = new Hyperlink()
                {
                    Tag = log.FullName,
                };
                hl.Inlines.Add((string)log.Name);
                hl.Click += WiFiLogHyperLink_Click;

                Label lb = new Label
                {
                    Content = hl,
                    Tag = log.FullName
                };
                flpWiFiLogs.Children.Add(lb);

                Button btn = new Button()
                {
                    Content = "View",
                    Tag = log.FullName,
                    Margin = new Thickness(3)
                };
                btn.Click += WiFiLogButton_Click;
                flpWiFiLogs.Children.Add(btn);
            }
        }
        private void WiFiLogButton_Click(object sender, RoutedEventArgs e)
        {
            BtnWiFiLogsViewAll.Background = ButtonFace;
            foreach (Control c in flpWiFiLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            Button btn = (Button)sender;
            btn.Background = Brushes.LightGreen;

            WifiLogs wifiLog = new WifiLogs((string)btn.Tag);

            WiFiLogsDisplayPlot(new List<WifiLogs>() { wifiLog });

            int cnt = 0;
            foreach (var data in wifiLog.Results)
                cnt += data.Value.Count();

            btn.Content = $"View ({cnt})";
        }
        private void BtnWiFiLogsViewAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (Control c in flpWiFiLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            Button btn = (Button)sender;
            btn.Background = Brushes.LightGreen;

            DirectoryInfo dir = new DirectoryInfo(WorkingPath + "var\\log\\");

            IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

            IEnumerable<FileInfo> res =
                from test in names
                where test.Name.StartsWith("wifiLog")
                select test;

            List<WifiLogs> wifiLogs = new List<WifiLogs>();
            foreach (FileInfo file in res)
                wifiLogs.Add(new WifiLogs(file.FullName));

            WiFiLogsDisplayPlot(wifiLogs);

            int cnt = 0;
            foreach (WifiLogs data in wifiLogs)
                foreach (var dataList in data.Results)
                    cnt += dataList.Value.Count();

            btn.Content = $"View All ({cnt})";
        }
        private void WiFiLogsDisplayPlot(List<WifiLogs> wifiLogs)
        {
            //BtnSaveWiFiDecibelsGraphImage.Visibility = Visibility.Visible;
            LblWiFiPlotDecibelsTitle.Content = "WiFi Decibels (Signal Strength)";

            PlotModel wiFiDecibelsPlotModel = new PlotModel
            {
                Title = string.Empty,
            };
            wiFiDecibelsPlotModel.Legends.Add(new OxyPlot.Legends.Legend()
            {
                LegendTitle = "SSID Names",
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.LeftTop
            });
            DateTimeAxis wiFiDecibelsXAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time & Date",
                StringFormat = "HH:mm:ss MM/dd/yy",
                IntervalType = DateTimeIntervalType.Auto,
                IntervalLength = 90
            };
            LinearAxis wiFiDecibelsYAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Decibels"
            };

            wiFiDecibelsPlotModel.Axes.Add(wiFiDecibelsXAxis);
            wiFiDecibelsPlotModel.Axes.Add(wiFiDecibelsYAxis);

            Chart_WiFiDecibels.Model = wiFiDecibelsPlotModel;


            //BtnSaveWiFiBaudGraphImage.Visibility = Visibility.Visible;
            LblWiFiPlotBaudTitle.Content = "WiFi Baud Rate (Speed)";

            PlotModel wiFiBaudPlotModel = new PlotModel
            {
                Title = string.Empty,
            };
            wiFiBaudPlotModel.Legends.Add(new OxyPlot.Legends.Legend()
            {
                LegendTitle = "SSID Names",
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.LeftTop
            });
            DateTimeAxis wiFiBaudXAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time & Date",
                StringFormat = "HH:mm:ss MM/dd/yy",
                IntervalType = DateTimeIntervalType.Auto,
                IntervalLength = 90
            };
            LinearAxis wiFiBaudYAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Baud Rate"
            };
            wiFiBaudPlotModel.Axes.Add(wiFiBaudXAxis);
            wiFiBaudPlotModel.Axes.Add(wiFiBaudYAxis);

            Chart_WiFiBaud.Model = wiFiBaudPlotModel;

            Dictionary<string, List<WifiLogs.WifiLogData>> ssidDict = new Dictionary<string, List<WifiLogs.WifiLogData>>();
            //foreach (WifiLogs log in wifiLogs)
            //{

            //    foreach (KeyValuePair<string, int> ssid in log.SSID)
            //    {
            //        if (ssidDict.ContainsKey(ssid.Key))
            //            ssidDict[ssid.Key].AddRange(log.Results[ssid.Value - 1]);
            //        else
            //            ssidDict.Add(ssid.Key, log.Results[ssid.Value - 1]);
            //    }
            //}
            foreach (KeyValuePair<string, List<WifiLogs.WifiLogData>> dict in wifiLogs[0].Results)
            {
                LineSeries wiFiBaudLineSeries = new LineSeries
                {
                    StrokeThickness = 1,
                    Title = dict.Key,
                };
                wiFiBaudPlotModel.Series.Add(wiFiBaudLineSeries);

                LineSeries wiFiDecibelsLineSeries = new LineSeries
                {
                    StrokeThickness = 1,
                    Title = dict.Key,
                };
                wiFiDecibelsPlotModel.Series.Add(wiFiDecibelsLineSeries);

                IEnumerable<WifiLogs.WifiLogData> ssidSorted = from s in dict.Value
                                                               orderby s.Time
                                                               select s;
                foreach (WifiLogs.WifiLogData dat in ssidSorted)
                {
                    wiFiDecibelsLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Decibels));
                    wiFiBaudLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Baud));
                }

            }
            Chart_WiFiDecibels.InvalidatePlot(true);
            Chart_WiFiBaud.InvalidatePlot(true);
        }
        private void WiFiLogHyperLink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink ll = (Hyperlink)sender;
            string log = (string)ll.Tag;

            System.Diagnostics.Process.Start(log);
        }

        //Battery TAB
        private void LoadBatteryLogs()
        {
            DirectoryInfo dir = new DirectoryInfo(WorkingPath + "var\\robot\\logs\\");

            IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

            IEnumerable<FileInfo> res =
                from test in names
                where test.Name.StartsWith("log_")
                select test;

            foreach (FileInfo file in res)
            {
                //UpdateStatus("Processing log for ([0-9] Voltage is): " + file.Name);

                IEnumerable<FileSearchResults> searchRes = FileSearch.FileSearch.Find(file.FullName, "[0-9] Voltage is", true);

                if (searchRes.Count() > 0)
                {
                    Hyperlink hl = new Hyperlink()
                    {
                        Tag = file.FullName,
                    };
                    hl.Inlines.Add((string)file.Name);
                    hl.Click += BatteryLogsHyperlink_Click; ;

                    Label lb = new Label
                    {
                        Content = hl,
                        Tag = file.FullName
                    };
                    flpBatteryLogs.Children.Add(lb);

                    Button btn = new Button()
                    {
                        Content = "View",
                        Tag = file.FullName,
                        Margin = new Thickness(3)
                    };
                    btn.Click += BatteryLogsButton_Click;
                    flpBatteryLogs.Children.Add(btn);
                }
            }
        }
        private void BtnBatteryLogsViewAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (Control c in flpBatteryLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            Button btn = (Button)sender;
            btn.Background = Brushes.LightGreen;

            DirectoryInfo dir = new DirectoryInfo(WorkingPath + "var\\robot\\logs\\");

            IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

            IEnumerable<FileInfo> res =
                from test in names
                where test.Name.StartsWith("log_")
                select test;

            IList<FileInfo> lst = res.ToList<FileInfo>();

            List<BatteryLogs> batLogs = new List<BatteryLogs>();
            foreach (FileInfo file in res)
                batLogs.Add(new BatteryLogs(file.FullName));

            BatteryLogsDisplayPlot(batLogs);

            int cnt = 0;
            foreach (BatteryLogs data in batLogs)
                cnt += data.Results.Count();

            btn.Content = $"View All ({cnt})";
        }
        private void BatteryLogsButton_Click(object sender, RoutedEventArgs e)
        {
            BtnBatteryLogsViewAll.Background = ButtonFace;
            foreach (Control c in flpBatteryLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            Button btn = (Button)sender;
            btn.Background = Brushes.LightGreen;

            List<BatteryLogs> batLogs = new List<BatteryLogs>() { new BatteryLogs((string)btn.Tag) };
            BatteryLogsDisplayPlot(batLogs);

            int cnt = 0;
            foreach (BatteryLogs data in batLogs)
                cnt += data.Results.Count();

            btn.Content = $"View ({cnt})";
        }
        private void BatteryLogsDisplayPlot(List<BatteryLogs> batLogs)
        {
            BtnSaveBatteryChargeStateGraphImage.Visibility = Visibility.Visible;
            LblPlotBatteryChargeStateTitle.Content = "Battery State of Charge (%)";

            PlotModel batterySOCPlotModel = new PlotModel
            {
                Title = string.Empty,
                IsLegendVisible = false
            };
            DateTimeAxis batterySOCXAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time & Date",
                StringFormat = "HH:mm:ss MM/dd/yy",
                IntervalType = DateTimeIntervalType.Auto,
                IntervalLength = 90
            };
            LinearAxis batterySOCYAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "State of Charge (%)"
            };

            batterySOCPlotModel.Axes.Add(batterySOCXAxis);
            batterySOCPlotModel.Axes.Add(batterySOCYAxis);

            Chart_BatteryChargeState.Model = batterySOCPlotModel;

            BtnSaveBatteryVoltageGraphImage.Visibility = Visibility.Visible;
            LblPlotBatteryVoltageTitle.Content = "Battery Voltage (vDC)";

            PlotModel batteryVoltagePlotModel = new PlotModel
            {
                Title = string.Empty,
                IsLegendVisible = false
            };
            DateTimeAxis batteryVoltageXAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time & Date",
                StringFormat = "HH:mm:ss MM/dd/yy",
                IntervalType = DateTimeIntervalType.Auto,
                IntervalLength = 90
            };
            LinearAxis batteryVoltageYAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Battery Voltage"
            };
            batteryVoltagePlotModel.Axes.Add(batteryVoltageXAxis);
            batteryVoltagePlotModel.Axes.Add(batteryVoltageYAxis);

            Chart_BatteryVoltage.Model = batteryVoltagePlotModel;


            LineSeries batteryVoltageLineSeries = new LineSeries
            {
                StrokeThickness = 1,
                Title = "Voltage",
            };
            batteryVoltagePlotModel.Series.Add(batteryVoltageLineSeries);

            LineSeries batterySOCLineSeries = new LineSeries
            {
                StrokeThickness = 1,
                Title = "State of Charge",
            };
            batterySOCPlotModel.Series.Add(batterySOCLineSeries);

            foreach (BatteryLogs log in batLogs)
            {
                IEnumerable<BatteryLogs.BatteryLogData> batterySorted = from s in log.Results
                                                                        orderby s.Time
                                                                        select s;

                foreach (BatteryLogs.BatteryLogData dat in batterySorted)
                {
                    batterySOCLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Level));
                    batteryVoltageLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Voltage));
                }
            }
            Chart_BatteryVoltage.InvalidatePlot(true);
            Chart_BatteryChargeState.InvalidatePlot(true);
        }
        private void BatteryLogsHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink ll = (Hyperlink)sender;
            string log = (string)ll.Tag;

            System.Diagnostics.Process.Start(log);
        }

        //Map TABs
        private void LoadMapContents()
        {
            ParseMapHeader();
            ParseMapInfo();
            tabMapContents.Visibility = Visibility.Visible;
        }
        private void ParseMapHeader()
        {
            TreeViewItem tviHeaderMain = new TreeViewItem()
            {
                Header = new Label() { Content = new Bold(new System.Windows.Documents.Run("Map Header") { FontSize = 20 }) }
            };

            StackPanel stkHeaderDataMain = new StackPanel()
            {

            };
            TreeViewItem tviHeaderData = new TreeViewItem()
            {
                Header = stkHeaderDataMain
            };

            foreach (PropertyInfo prop in MapFile.Map.Header.GetType().GetProperties())
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (type == typeof(float) | type == typeof(double) | type == typeof(int))
                {
                    StackPanel stkHeaderData = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                    };
                    stkHeaderData.Children.Add(new Label() { Content = prop.Name });
                    stkHeaderData.Children.Add(new TextBox() { Text = $"{prop.GetValue(MapFile.Map.Header, null).ToString()}", IsReadOnly = true, VerticalContentAlignment = System.Windows.VerticalAlignment.Center, BorderBrush = this.Background });

                    stkHeaderDataMain.Children.Add(stkHeaderData);
                }
            }

            tviHeaderMain.Items.Add(tviHeaderData);
            TvMapContents.Items.Add(tviHeaderMain);
        }
        private void ParseMapInfo()
        {
            TreeViewItem tviMapInfoMain = new TreeViewItem()
            {
                Header = new Label() { Content = new Bold(new System.Windows.Documents.Run("Map Info") { FontSize = 20 }) }
            };

            TvMapContents.Items.Add(tviMapInfoMain);

            foreach (MapInfo.InfoType mInfo in MapFile.Map.Info.List)
            {
                TreeViewItem tviMapInfoTypeMain = new TreeViewItem()
                {
                    Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(mInfo.Name)) }
                };
                tviMapInfoMain.Items.Add(tviMapInfoTypeMain);

                StackPanel stkMapInfoTypeData = new StackPanel()
                {

                };
                TreeViewItem tviMapInfoTypeData = new TreeViewItem()
                {
                    Header = stkMapInfoTypeData
                };
                tviMapInfoTypeMain.Items.Add(tviMapInfoTypeData);

                foreach (PropertyInfo prop in typeof(MapInfo.InfoType).GetProperties())
                {
                    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (type == typeof(string))
                    {
                        object data = prop.GetValue(mInfo, null);
                        string val = string.Empty;
                        if (data != null)
                            val = data.ToString();

                        StackPanel stkMapInfoTypeDataItem = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal,
                        };
                        stkMapInfoTypeDataItem.Children.Add(new Label() { Content = prop.Name });
                        stkMapInfoTypeDataItem.Children.Add(new TextBox() { Text = $"{val}", IsReadOnly = true, VerticalContentAlignment = System.Windows.VerticalAlignment.Center, BorderBrush = this.Background });

                        stkMapInfoTypeData.Children.Add(stkMapInfoTypeDataItem);
                    }

                    if (type == typeof(Dictionary<string, string>))
                    {
                        TreeViewItem tviMapInfoTypeParamDataMain = new TreeViewItem()
                        {
                            Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(prop.Name)) }
                        };
                        tviMapInfoTypeMain.Items.Add(tviMapInfoTypeParamDataMain);

                        StackPanel stkMapInfoTypeParamData = new StackPanel()
                        {

                        };
                        TreeViewItem tviMapInfoTypeParamData = new TreeViewItem()
                        {
                            Header = stkMapInfoTypeParamData
                        };
                        tviMapInfoTypeParamDataMain.Items.Add(tviMapInfoTypeParamData);

                        object data = prop.GetValue(mInfo, null);
                        Dictionary<string, string> dict;
                        if (data != null)
                            dict = (Dictionary<string, string>)data;
                        else
                            dict = new Dictionary<string, string>();

                        foreach (KeyValuePair<string, string> val in dict)
                        {
                            StackPanel stkMapInfoTypeDataParamItem = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal,
                            };
                            stkMapInfoTypeDataParamItem.Children.Add(new Label() { Content = val.Key });
                            stkMapInfoTypeDataParamItem.Children.Add(new TextBox() { Text = $"{val.Value}", IsReadOnly = true, VerticalContentAlignment = System.Windows.VerticalAlignment.Center, BorderBrush = this.Background });

                            stkMapInfoTypeParamData.Children.Add(stkMapInfoTypeDataParamItem);
                        }
                    }

                    if (type == typeof(Dictionary<string, MapArgDesc>))
                    {
                        TreeViewItem tviMapInfoTypeArgsMain = new TreeViewItem()
                        {
                            Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(prop.Name)) }
                        };
                        tviMapInfoTypeMain.Items.Add(tviMapInfoTypeArgsMain);

                        object data = prop.GetValue(mInfo, null);
                        Dictionary<string, MapArgDesc> dict;
                        if (data != null)
                            dict = (Dictionary<string, MapArgDesc>)data;
                        else
                            dict = new Dictionary<string, MapArgDesc>();

                        foreach (KeyValuePair<string, MapArgDesc> val in dict)
                        {
                            TreeViewItem tviMapInfoTypeArg = new TreeViewItem()
                            {
                                Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(val.Key)) }
                            };
                            tviMapInfoTypeArgsMain.Items.Add(tviMapInfoTypeArg);

                            StackPanel stkMapInfoTypeArgData = new StackPanel()
                            {

                            };
                            TreeViewItem tviMapInfoTypeArgData = new TreeViewItem()
                            {
                                Header = stkMapInfoTypeArgData
                            };
                            tviMapInfoTypeArg.Items.Add(tviMapInfoTypeArgData);

                            foreach (PropertyInfo prop1 in typeof(MapArgDesc).GetProperties())
                            {
                                var type1 = Nullable.GetUnderlyingType(prop1.PropertyType) ?? prop1.PropertyType;

                                if (type1 == typeof(string))
                                {
                                    object data1 = prop1.GetValue(val.Value, null);

                                    string val1 = string.Empty;
                                    if (data1 != null)
                                        val1 = data1.ToString();

                                    StackPanel stkMapInfoTypeArgDataItem = new StackPanel()
                                    {
                                        Orientation = Orientation.Horizontal,
                                    };
                                    stkMapInfoTypeArgDataItem.Children.Add(new Label() { Content = prop1.Name });
                                    stkMapInfoTypeArgDataItem.Children.Add(new TextBox() { Text = $"{val1}", VerticalContentAlignment = System.Windows.VerticalAlignment.Center, IsReadOnly = true, BorderBrush = this.Background });

                                    stkMapInfoTypeArgData.Children.Add(stkMapInfoTypeArgDataItem);
                                }

                                if (type1 == typeof(Dictionary<string, string>))
                                {
                                    TreeViewItem tviMapInfoTypeArgParams = new TreeViewItem()
                                    {
                                        Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(prop1.Name)) }
                                    };
                                    tviMapInfoTypeArgData.Items.Add(tviMapInfoTypeArgParams);

                                    StackPanel stkMapInfoTypeArgParamsData = new StackPanel()
                                    {

                                    };
                                    TreeViewItem tviMapInfoTypeArgParamsData = new TreeViewItem()
                                    {
                                        Header = stkMapInfoTypeArgParamsData
                                    };
                                    tviMapInfoTypeArgParams.Items.Add(tviMapInfoTypeArgParamsData);

                                    object data2 = prop1.GetValue(val.Value, null);

                                    Dictionary<string, string> dict2;
                                    if (data2 != null)
                                        dict2 = (Dictionary<string, string>)data2;
                                    else
                                        dict2 = new Dictionary<string, string>();

                                    foreach (KeyValuePair<string, string> val2 in dict2)
                                    {
                                        StackPanel stkMapInfoTypeArgParamsDataItem = new StackPanel()
                                        {
                                            Orientation = Orientation.Horizontal,
                                        };
                                        stkMapInfoTypeArgParamsDataItem.Children.Add(new Label() { Content = val2.Key });
                                        stkMapInfoTypeArgParamsDataItem.Children.Add(new TextBox() { Text = $"{val2.Value}", VerticalContentAlignment = System.Windows.VerticalAlignment.Center, IsReadOnly = true, BorderBrush = this.Background });

                                        stkMapInfoTypeArgParamsData.Children.Add(stkMapInfoTypeArgParamsDataItem);
                                    }
                                }
                            }
                            //    StkMain.Children.Add(new Label() { Content = new Bold(new Run(val.Key)), Margin = new Thickness(20, 0, 0, 0) });
                            //StkMain.Children.Add(new TextBox() { IsReadOnly = true, Text = $"{val.Value}", BorderBrush = this.Background, Margin = new Thickness(40, 0, 80, 0) });
                        }
                    }

                    if (type == typeof(List<MapCairn>))
                    {
                        TreeViewItem tviMapInfoTypeChildMain = new TreeViewItem()
                        {
                            Header = new Label() { Content = new Bold(new System.Windows.Documents.Run(prop.Name)) }
                        };
                        tviMapInfoTypeMain.Items.Add(tviMapInfoTypeChildMain);

                        object data = prop.GetValue(mInfo, null);

                        List<MapCairn> dict;
                        if (data != null)
                            dict = (List<MapCairn>)data;
                        else
                            dict = new List<MapCairn>();

                        if (dict.Count() > 0) tviMapInfoTypeMain.Tag = true;
                        else tviMapInfoTypeMain.Tag = false;

                        foreach (MapCairn mc in dict)
                        {
                            TreeViewItem tviMapInfoTypeChild = new TreeViewItem()
                            {
                                Header = new Label() { Content = new Bold(new System.Windows.Documents.Run($"{prop.Name}: {mc.Type}")) }
                            };
                            tviMapInfoTypeChildMain.Items.Add(tviMapInfoTypeChild);

                            StackPanel stkMapInfoTypeChildData = new StackPanel()
                            {

                            };
                            TreeViewItem tviMapInfoTypeChildData = new TreeViewItem()
                            {
                                Header = stkMapInfoTypeChildData
                            };
                            tviMapInfoTypeChild.Items.Add(tviMapInfoTypeChildData);

                            foreach (PropertyInfo prop3 in typeof(MapCairn).GetProperties())
                            {
                                var type3 = Nullable.GetUnderlyingType(prop3.PropertyType) ?? prop3.PropertyType;

                                object data3 = prop3.GetValue(mc, null);

                                if (data3 == null)
                                    data3 = string.Empty;

                                StackPanel stkMapInfoTypeChildDataItem = new StackPanel()
                                {
                                    Orientation = Orientation.Horizontal,
                                };
                                stkMapInfoTypeChildDataItem.Children.Add(new Label() { Content = prop3.Name });
                                stkMapInfoTypeChildDataItem.Children.Add(new TextBox() { Text = $"{data3.ToString()}", VerticalContentAlignment = System.Windows.VerticalAlignment.Center, IsReadOnly = true, BorderBrush = this.Background });

                                stkMapInfoTypeChildData.Children.Add(stkMapInfoTypeChildDataItem);
                            }
                        }
                    }
                }

            }
        }
        private void BtnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (object obj in this.TvMapContents.Items)
                if (obj is TreeViewItem tvi)
                    ExpandStateAll(tvi, true);
        }
        private void BtnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (object obj in this.TvMapContents.Items)
                if (obj is TreeViewItem tvi)
                    ExpandStateAll(tvi, false);
            foreach (object obj in this.TvMapContents.Items)
                if (obj is TreeViewItem tvi)
                    tvi.IsExpanded = true;
        }
        private void ExpandStateAll(TreeViewItem items, bool expand)
        {
            if (items.Visibility == Visibility.Collapsed) return;

            items.IsExpanded = expand;
            foreach (object obj in items.Items)
                if (obj is TreeViewItem tvi)
                    ExpandStateAll(tvi, expand);
        }
        private void ExpandStateAllMakeVisible(TreeViewItem items, bool expand)
        {
            items.Visibility = Visibility.Visible;
            items.IsExpanded = expand;
            foreach (object obj in items.Items)
                if (obj is TreeViewItem tvi)
                    ExpandStateAllMakeVisible(tvi, expand);
        }
        private void ChkShowOnlyChildren_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked.Value)
            {
                foreach (object obj in this.TvMapContents.Items)
                    if (obj is TreeViewItem tviMain)
                        foreach (object obj1 in tviMain.Items)
                            if (obj1 is TreeViewItem tvi)
                                if (tvi.Tag is bool val)
                                    if (!val)
                                    {
                                        tvi.IsExpanded = false;
                                        tvi.Visibility = Visibility.Collapsed;
                                    }
            }
            else
            {
                foreach (object obj in this.TvMapContents.Items)
                    if (obj is TreeViewItem tvi)
                        ExpandStateAllMakeVisible(tvi, false);
                foreach (object obj in this.TvMapContents.Items)
                    if (obj is TreeViewItem tvi)
                        tvi.IsExpanded = true;
            }
        }

        private void LoadMapContentsRaw(string mapPath)
        {
            SystemMap_Serializer.SystemMap serial = SystemMap_Serializer.Load($"{App.UserDataDirectory}SystemMap.xml");

            StringBuilder sb = new StringBuilder();
            sb.Append(rtfHead);

            using (StreamReader sr = new StreamReader(mapPath))
            {
                Hyperlink hl1 = new Hyperlink()
                {
                    Tag = "Header",
                    Foreground = Brushes.Black
                };
                hl1.Inlines.Add("Header");
                hl1.Click += MapSectorHyperLink_Click;

                SolidColorBrush b1 = new SolidColorBrush(System.Windows.Media.Colors.White) { Opacity = 0.0 };
                Label lb1 = new Label
                {
                    Content = hl1,
                    Tag = "Header",
                    Background = b1
                };

                flpMapSections.Children.Add(lb1);

                string line;
                int lineNum = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("LINES"))
                        break;

                    lineNum++;

                    foreach (SystemMap_Serializer.SystemMapSection sec in serial.Section)
                    {
                        foreach (Match m in Regex.Matches(line, sec.SearchText))
                        {
                            if (sec.FirstLine == -1 && !sec.SubSection)
                            {
                                Hyperlink hl = new Hyperlink()
                                {
                                    Tag = sec.SearchText,
                                    Foreground = Brushes.Black
                                };
                                hl.Inlines.Add((string)sec.Label);
                                hl.Click += MapSectorHyperLink_Click;

                                SolidColorBrush b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(sec.HighlightColor)) { Opacity = 0.50 };
                                Label lb = new Label
                                {
                                    Content = hl,
                                    Tag = sec.SearchText,
                                    Background = b
                                };

                                sec.FirstLine = lineNum;

                                flpMapSections.Children.Add(lb);
                            }

                            int offset = 0;
                            int i = (int)Enum.Parse(typeof(Colors), sec.HighlightColor);

                            line = line.Insert(m.Index + offset, rtfSearchFormatsColors[i]);
                            offset += rtfSearchFormatsColors[i].Length;
                            line = line.Insert(m.Index + m.Length + offset, rtfSearchFormatsColorsTail);
                            offset += rtfSearchFormatsColorsTail.Length;
                        }
                    }

                    sb.Append(line + "\\par" + Environment.NewLine);
                }
            }
            sb.Append(rtfTail);

            LoadRTF(sb.ToString(), rtbMap);

            tabMapContentsRaw.Visibility = Visibility.Visible;
        }
        private void MapSectorHyperLink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyp = (Hyperlink)sender;

            if (hyp.Tag is string line)
            {
                if (line.Equals("Header"))
                {
                    Rect r = rtbMap.Document.ContentStart.GetCharacterRect(LogicalDirection.Backward);
                    rtbMap.ScrollToVerticalOffset(rtbMap.VerticalOffset + r.Y);
                    return;
                }

                rtbMap.CaretPosition = rtbMap.Document.ContentStart;
                TextPointer txtP = rtbMap.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
                while (true)
                {
                    char[] text = new char[line.Length];

                    if (txtP.GetTextInRun(LogicalDirection.Forward, text, 0, line.Length) > 0)
                    {
                        string s = new string(text);
                        if (s.Equals(line))
                        {
                            Rect r = txtP.GetCharacterRect(LogicalDirection.Backward);
                            rtbMap.ScrollToVerticalOffset(rtbMap.VerticalOffset + r.Y);
                            return;
                        }
                    }

                    TextPointer txtP1 = txtP.GetLineStartPosition(1, out int skipped).GetInsertionPosition(LogicalDirection.Forward);
                    if (skipped == 1)
                        txtP = txtP1;
                    else
                        return;
                }
            }

        }

        //private void MapLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{


        //    LinkLabel ll = (LinkLabel)sender;

        //    rtbMap.SelectionStart = rtbMap.GetFirstCharIndexFromLine((int)ll.Tag - 1);
        //    rtbMap.ScrollToCaret();

        //}

        private List<StatusLogs.Status> MapLostLogPositions { get; set; } = new List<StatusLogs.Status>();

        private List<StatusLogs.Status> DrawPositionsFromLog(System.Drawing.Graphics g, Map map)
        {
            List<StatusLogs.Status> lost = new List<StatusLogs.Status>();

            if (map.LoggedStatus == null) return lost;

            System.Drawing.Rectangle r = new System.Drawing.Rectangle((int)map.Header.WidthOffset, (int)map.Header.HeightOffset, (int)map.Header.Width, (int)map.Header.Height);
            int sz = (int)(40.0 * (1 + Math.Abs((map.Header.ScaleFactor - 1.0))));
            System.Drawing.Brush blueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);
            foreach (StatusLogs.Status s in map.LoggedStatus)
            {
                if (r.Contains(s.Position))
                    g.FillPie(blueBrush, new System.Drawing.Rectangle(s.Position.X, s.Position.Y, sz, sz), 0, 360);
                else
                    lost.Add(s);
            }
            return lost;
        }
        private void DrawWiFiPositionsFromLog(System.Drawing.Graphics g, Map map)
        {
            if (map.LoggedWifi == null) return;

            System.Drawing.Rectangle r = new System.Drawing.Rectangle((int)map.Header.WidthOffset, (int)map.Header.HeightOffset, (int)map.Header.Width, (int)map.Header.Height);
            int sz = (int)(40.0 * (1 + Math.Abs((map.Header.ScaleFactor - 1.0))));
            foreach (var ssid in map.LoggedWifi)
            {
                foreach(var s in ssid.Value)
                {
                    System.Drawing.Brush bBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 255, 0, 0));

                    if (r.Contains(s.Position))
                        g.FillPie(bBrush, new System.Drawing.Rectangle(s.Position.X, s.Position.Y, sz, sz), 0, 360);
                }
            }
        }
        private void DrawMapApplyToImage(string bmp)
        {
            ImgMapBorder.Child = new Image() { Source = Base64StringToBitmap(bmp) };
            //MapUtils.DrawOnGrid(Map, GridMain, 1024, 1024);

            StringBuilder sb = new StringBuilder();
            foreach (StatusLogs.Status dat in MapLostLogPositions)
            {
                Point p = MapFile.Map.Header.OffsetTo(new Point(dat.Position.X, dat.Position.Y));
                sb.AppendLine(dat.Time.ToString() + "\t" + "Offset: " + p.X.ToString() + " " + p.Y.ToString());// + "\\v log?" + ind.log.ToString("00") + "\\v0" + "\\par");
            }
            TxtMapPointsOffMap.Text = sb.ToString();

            //Draw2DMap map = new Draw2DMap();
            //map.Map = Map;
            //map.Draw(ImgMapBorder);

            tabMapImage.Visibility = Visibility.Visible;
        }
        //private void rtbMapGLostPositions_MouseDoubleClick(object sender, MouseEventArgs e)
        //{


        //    if (!ModifierKeys.HasFlag(Keys.Shift)) return;

        //    RichEdit50W rtb = (RichEdit50W)sender;

        //    rtb.Select(rtb.GetFirstCharIndexFromLine(rtb.CurrentLineNumber), rtb.Lines[rtb.CurrentLineNumber].Length + 9);
        //    string s = rtb.SelectedRtf;

        //    int i = Convert.ToInt16(s.Substring(s.LastIndexOf("log?") + 4, 2));

        //    using (frmRTF rtf = new frmRTF())
        //    {
        //        rtf.workPath = workPath;
        //        rtf.logDetails = positionLogDetails[i];
        //        rtf.logLineNumber = rtb.CurrentLogLineNumber;

        //        rtf.ShowDialog();
        //    }

        //}

        public struct HeatPoint
        {
            public System.Drawing.Point Point;
            public int X => Point.X;
            public int Y => Point.Y;
            public byte Intensity;
            public HeatPoint(int iX, int iY, byte bIntensity)
            {
                Point = new System.Drawing.Point(iX, iY);
                Intensity = bIntensity;
            }
            public HeatPoint(System.Drawing.Point point, byte bIntensity)
            {
                Point = point;
                Intensity = bIntensity;
            }

        }
        private List<HeatPoint> HeatPoints { get; set; } = new List<HeatPoint>();
        private void DrawWiFiHeatMap(int width, int height)
        {

            UpdateStatus("Drawing WiFi Heat Map");

            MapFile.Map.Header.SetScaleFactor(width, height);

            //HeatMap.HeatMapImage img = new HeatMap.HeatMapImage((int)MapFile.Map.Header.WidthScaled, (int)MapFile.Map.Header.HeightScaled, 100, 10);

            //List<HeatMap.DataType> lst = new List<HeatMap.DataType>();

            //foreach(HeatPoint hp1 in HeatPoints)
            //{
            //    img.SetAData(new HeatMap.DataType() { X = (int)((hp1.X - MapFile.Map.Header.WidthOffset) * MapFile.Map.Header.ScaleFactor), Y = (int)((hp1.Y - MapFile.Map.Header.HeightOffset - MapFile.Map.Header.Height) * -MapFile.Map.Header.ScaleFactor), Weight = hp1.Intensity + 256 });
            //}
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)MapFile.Map.Header.WidthScaled, (int)MapFile.Map.Header.HeightScaled, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);//img.GetHeatMap();//

            bmp = DrawWiFiHeatMapFromLog(bmp);

            using (System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Black, 20))
            using (System.Drawing.Brush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                                        whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.TranslateTransform(-(int)MapFile.Map.Header.WidthOffset, -(int)MapFile.Map.Header.HeightOffset - (int)MapFile.Map.Header.Height);
                //g.ScaleTransform((int)MapFile.Map.Header.ScaleFactor, -(int)MapFile.Map.Header.ScaleFactor, System.Drawing.Drawing2D.MatrixOrder.Append);

                foreach (MapGeometry.Line ln in MapFile.Map.Geometry.Lines)
                    g.DrawLine(blackPen, ln.Start, ln.End);

                foreach (System.Drawing.Point ln in MapFile.Map.Geometry.Points)
                    g.FillRectangle(blackBrush, ln.X, ln.Y, 20, 20);

                foreach (HeatPoint hp in HeatPoints)
                    g.FillRectangle(blackBrush, hp.X, hp.Y, 20, 20);

            }
            string SigBase64;
            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                SigBase64 = Convert.ToBase64String(memory.GetBuffer());
            }

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) => { ImgMapWiFiHeatBorder.Child = new Image() { Source = Base64StringToBitmap(s) }; tabMapWiFiHeat.Visibility = Visibility.Visible; }), SigBase64);

            UpdateStatus("Completed!");
        }
        private System.Drawing.Bitmap DrawWiFiHeatMapFromLog(System.Drawing.Bitmap bSurface)
        {
            bSurface = CreateIntensityMask(bSurface);

            UpdateStatus("Colorizing WiFi Heat Map");

            bSurface = Colorize(bSurface, 1);

            return bSurface;
        }
        private System.Drawing.Bitmap CreateIntensityMask(System.Drawing.Bitmap bSurface)
        {
            // Create new graphics surface from memory bitmap
            using (System.Drawing.Graphics DrawSurface = System.Drawing.Graphics.FromImage(bSurface))
            {
                DrawSurface.TranslateTransform(-MapFile.Map.Header.WidthOffset, -MapFile.Map.Header.HeightOffset - MapFile.Map.Header.Height);
                DrawSurface.ScaleTransform(MapFile.Map.Header.ScaleFactor, -MapFile.Map.Header.ScaleFactor, System.Drawing.Drawing2D.MatrixOrder.Append);
                // Set background color to white so that pixels can be correctly colorized
                DrawSurface.Clear(System.Drawing.Color.White);
                double scale = MapFile.Map.Header.Width / MapFile.Map.Header.WidthScaled;
                // Traverse heat point data and draw masks for each heat point
                int cnt = HeatPoints.Count();
                int i = 1;
                int ii = 1;
                Stopwatch sw = new Stopwatch();
                sw.Start();

                foreach (HeatPoint DataPoint in HeatPoints)
                {
                    // Render current heat point on draw surface
                    if (ii == 100)
                    {
                        UpdateStatus($"Drawing WiFi Heat Map: {i} of {cnt}: {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).ToString()}");
                        ii = 0;
                    }
                    i++;
                    ii++;
                    DrawHeatPoint(DrawSurface, DataPoint, (int)(500 * scale));
                }
            }
            return bSurface;
        }
        private void DrawHeatPoint(System.Drawing.Graphics Canvas, HeatPoint HeatPoint, int Radius)
        {
            // Create points generic list of points to hold circumference points
            List<System.Drawing.Point> CircumferencePointsList = new List<System.Drawing.Point>();

            // Create an empty point to predefine the point struct used in the circumference loop
            System.Drawing.Point CircumferencePoint;

            // Create an empty array that will be populated with points from the generic list
            System.Drawing.Point[] CircumferencePointsArray;

            // Calculate ratio to scale byte intensity range from 0-255 to 0-1
            float fRatio = 1F / Byte.MaxValue;
            // Precalulate half of byte max value
            byte bHalf = Byte.MaxValue / 2;
            // Flip intensity on it's center value from low-high to high-low
            int iIntensity = (byte)(HeatPoint.Intensity - ((HeatPoint.Intensity - bHalf) * 2));
            // Store scaled and flipped intensity value for use with gradient center location
            float fIntensity = iIntensity * fRatio;

            // Loop through all angles of a circle
            // Define loop variable as a double to prevent casting in each iteration
            // Iterate through loop on 10 degree deltas, this can change to improve performance
            for (double i = 0; i <= 360; i += 10)
            {
                // Replace last iteration point with new empty point struct
                CircumferencePoint = new System.Drawing.Point
                {

                    // Plot new point on the circumference of a circle of the defined radius
                    // Using the point coordinates, radius, and angle
                    // Calculate the position of this iterations point on the circle
                    X = Convert.ToInt32(HeatPoint.X + Radius * Math.Cos(ConvertDegreesToRadians(i))),
                    Y = Convert.ToInt32(HeatPoint.Y + Radius * Math.Sin(ConvertDegreesToRadians(i)))
                };
                // Add newly plotted circumference point to generic point list
                CircumferencePointsList.Add(CircumferencePoint);
            }

            // Populate empty points system array from generic points array list
            // Do this to satisfy the datatype of the PathGradientBrush and FillPolygon methods
            CircumferencePointsArray = CircumferencePointsList.ToArray();

            // Create new PathGradientBrush to create a radial gradient using the circumference points
            System.Drawing.Drawing2D.PathGradientBrush GradientShaper = new System.Drawing.Drawing2D.PathGradientBrush(CircumferencePointsArray);
            // Create new color blend to tell the PathGradientBrush what colors to use and where to put them
            System.Drawing.Drawing2D.ColorBlend GradientSpecifications = new System.Drawing.Drawing2D.ColorBlend(3)
            {
                // Define positions of gradient colors, use intesity to adjust the middle color to
                // show more mask or less mask
                Positions = new float[3] { 0, fIntensity, 1 },
                // Define gradient colors and their alpha values, adjust alpha of gradient colors to match intensity
                Colors = new System.Drawing.Color[3]
                {
                    System.Drawing.Color.FromArgb(0, System.Drawing.Color.White),
                    System.Drawing.Color.FromArgb(HeatPoint.Intensity, System.Drawing.Color.Black),
                    System.Drawing.Color.FromArgb(HeatPoint.Intensity, System.Drawing.Color.Black)
                }
            };

            // Pass off color blend to PathGradientBrush to instruct it how to generate the gradient
            GradientShaper.InterpolationColors = GradientSpecifications;
            // Draw polygon (circle) using our point array and gradient brush
            Canvas.FillPolygon(GradientShaper, CircumferencePointsArray);
        }
        private double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }
        public System.Drawing.Bitmap Colorize(System.Drawing.Bitmap Mask, byte Alpha)
        {
            // Create new bitmap to act as a work surface for the colorization process
            System.Drawing.Bitmap Output = new System.Drawing.Bitmap(Mask.Width, Mask.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // Create a graphics object from our memory bitmap so we can draw on it and clear it's drawing surface
            System.Drawing.Graphics Surface = System.Drawing.Graphics.FromImage(Output);
            //Surface.TranslateTransform(-map.Header.WidthOffset, -map.Header.HeightOffset - map.Header.Height);
            //Surface.ScaleTransform(map.Header.ScaleFactor.Width, -map.Header.ScaleFactor.Height, System.Drawing.Drawing2D.MatrixOrder.Append);

            Surface.Clear(System.Drawing.Color.Transparent);
            // Build an array of color mappings to remap our greyscale mask to full color
            // Accept an alpha byte to specify the transparancy of the output image
            System.Drawing.Imaging.ColorMap[] Colors = CreatePaletteIndex(Alpha);
            // Create new image attributes class to handle the color remappings
            // Inject our color map array to instruct the image attributes class how to do the colorization
            System.Drawing.Imaging.ImageAttributes Remapper = new System.Drawing.Imaging.ImageAttributes();
            Remapper.SetRemapTable(Colors);
            // Draw our mask onto our memory bitmap work surface using the new color mapping scheme
            Surface.DrawImage(Mask, new System.Drawing.Rectangle(0, 0, Mask.Width, Mask.Height), 0, 0, Mask.Width, Mask.Height, System.Drawing.GraphicsUnit.Pixel, Remapper);
            // Send back newly colorized memory bitmap
            return Output;
        }
        private System.Drawing.Imaging.ColorMap[] CreatePaletteIndex(byte Alpha)
        {
            System.Drawing.Imaging.ColorMap[] OutputMap = new System.Drawing.Imaging.ColorMap[256];
            // Change this path to wherever you saved the palette image.
            System.Drawing.Bitmap Palette = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile($"{AppPath}Resources\\Palette.bmp");
            // Loop through each pixel and create a new color mapping
            for (int X = 0; X <= 255; X++)
            {
                OutputMap[X] = new System.Drawing.Imaging.ColorMap
                {
                    OldColor = System.Drawing.Color.FromArgb(X, X, X),
                    NewColor = System.Drawing.Color.FromArgb(Alpha, Palette.GetPixel(X, 0))
                };
            }
            return OutputMap;
        }


        public BitmapImage Base64StringToBitmap(string base64String)
        {
            BitmapImage bitmapImage = new BitmapImage();
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            using (MemoryStream memoryStream = new MemoryStream(byteBuffer))
            {
                memoryStream.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        //private void SetupMapMenu(string mapPath)
        //{
        //    if (HasMap)
        //    {
        //        MenuMap.Header = $"Map ({System.IO.Path.GetFileNameWithoutExtension(mapPath)})";
        //        MenuMap.Visibility = Visibility.Visible;
        //        MenuMap.Tag = mapPath;

        //        MenuItem save = new MenuItem()
        //        {
        //            Header = "Save As",
        //            Tag = mapPath
        //        };
        //        save.Click += SaveMapFile_Click;
        //        MenuMap.Items.Add(save);

        //        //if (HeatPoints.Count() > 0)
        //        //{
        //        //    MenuItem heat = new MenuItem()
        //        //    {
        //        //        Header = $"Create WiFi Heat Map: {HeatPoints.Count()} Points \u2248{(HeatPoints.Count() / 15) / 60} min.",
        //        //        Tag = mapPath
        //        //    };
        //        //    heat.Click += CreateHeatMap_Click;
        //        //    MenuMap.Items.Add(heat);
        //        //}

        //        //MenuItem dxMM;
        //        //if (CheckDxMobileMapPath())
        //        //{
        //        //    dxMM = new MenuItem()
        //        //    {
        //        //        Header = "Create DXMobileMap Database",
        //        //    };
        //        //    dxMM.Click += MapDBCreateFile_Click;
        //        //}
        //        //else
        //        //{
        //        //    dxMM = new MenuItem()
        //        //    {
        //        //        Header = "Get DXMobileMap",
        //        //    };
        //        //    dxMM.Click += MapDBGetDxMobileMap_Click;
        //        //}

        //        //MenuMap.Items.Add(dxMM);
        //    }
        //    else
        //    {
        //        MenuMap.Header = "Map";
        //        MenuMap.Visibility = Visibility.Collapsed;
        //        MenuMap.Tag = string.Empty;

        //        MenuMap.Items.Clear();
        //    }
        //}

        private void CreateHeatMap_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(() => DrawWiFiHeatMap(4096, 4096));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void SaveMapFile_Click(object sender, RoutedEventArgs e)
        {
            SaveMapFile();
        }
        private bool SaveMapFile()
        {
            Microsoft.Win32.SaveFileDialog file = new Microsoft.Win32.SaveFileDialog
            {
                CheckFileExists = false,
                AddExtension = true,
                CheckPathExists = true,
                Filter = "Map file (*.map)|*.map",
                FilterIndex = 1
            };

            string filePath;
            if (file.ShowDialog() == true)
            {
                filePath = file.FileName;
            }
            else return false;

            MapFile.LoadMapFromFile();
            MapFile.FilePath = filePath;
            MapFile.WriteFileContents();

            return true;
        }
        public void MapDBGetDxMobileMap_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://github.com/ZeroxCorbin/DxMobileMap_WPF");
        }
        //public void MapDBCreateFile_Click(object sender, RoutedEventArgs e)
        //{
        //    Thread thread = new Thread(() => MapDBCreateFile_Thread());
        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.Start();
        //}
        //public void MapDBCreateFile_Thread()
        //{
        //    string dxPath = App.Settings.GetValue("DxMobileMapPath");
        //    if (dxPath == null)
        //    {
        //        UpdateStatus("Please select the DxMobileMap.exe directory in the Application Settings.");
        //        return;
        //    }


        //    string dxDBPath = GetDXMobileMapDatabasePath(dxPath);
        //    if (!dxDBPath.EndsWith("\\")) dxDBPath += "\\";

        //    if (dxDBPath == null)
        //    {
        //        UpdateStatus("Please run DxMobileMap.exe once to allow it's application settings to be created. Then run this command again.");
        //        return;
        //    }

        //    if (!SaveMapFile())
        //        return;

        //    string dbPath = $"{dxDBPath}{MapFile.Map.MapFile.FileName}{MapDatabaseExtension}";

        //    int i = 1;
        //    while (File.Exists(dbPath))
        //    {
        //        dbPath = $"{dxDBPath}{MapFile.Map.MapFile.FileName}_{i++}{MapDatabaseExtension}";
        //        if (i > 200)
        //            return;
        //    }

        //    UpdateStatus($"Creating map database: {dbPath}");
        //    using (SimpleDataBase mapDb = new SimpleDataBase().Init(dbPath, MapDatabaseTableName, true))
        //    {
        //        if (mapDb == null) return;

        //        mapDb.SetValue(MapDatabaseFileKey, MapFile.Map.MapFile);
        //        mapDb.SetValue($"{MapFile.Map.MapFile.LoadedUID}{MapDatabaseContentsDateKey}", MapFile.Map.MapFile.FileLastUpdated);
        //        mapDb.SetValue($"{MapFile.Map.MapFile.LoadedUID}{MapDatabaseContentsHeaderKey}", MapFile.Map.Header);

        //        MapFile.Map.MapFile.FileToContents();
        //        mapDb.SetValue($"{MapFile.Map.MapFile.LoadedUID}{MapDatabaseContentsKey}", MapFile.Map.MapFile.Contents);
        //        MapFile.Map.MapFile.Contents = "";

        //        string bmp = MapUtils.GetBitmapString(Map, 220, 220);
        //        mapDb.SetValue($"{MapFile.Map.MapFile.LoadedUID}_thumbnail", bmp);

        //        if (MapFile.Map.LoggedStatus != null)
        //            if (MapFile.Map.LoggedStatus.Count > 0)
        //            {
        //                mapDb.SetValue($"{MapDatabasePositionsKey}", MapFile.Map.LoggedStatus);
        //                mapDb.SetValue("UseLoggedPositions", true);
        //            }

        //        if (MapFile.Map.LoggedWifi != null)
        //            if (MapFile.Map.LoggedWifi.Count > 0)
        //            {
        //                mapDb.SetValue($"{MapDatabaseWifiKey}", MapFile.Map.LoggedWifi);
        //                mapDb.SetValue("UseLoggedWiFi", true);
        //            }

        //        // MapFile.Map.MapFile.DatabasePath = dbPath;

        //        if (File.Exists($"{dxPath}DxMobileMap_WPF.exe"))
        //        {
        //            UpdateStatus($"Launching DxMobileMap");

        //            ProcessStartInfo proc = new ProcessStartInfo($"{dxPath}DxMobileMap_WPF.exe")
        //            {
        //                WorkingDirectory = dxPath
        //            };
        //            Process.Start(proc);
        //        }

        //        return;
        //    }
        //}

        //private string GetDXMobileMapDatabasePath(string dxPath)
        //{
        //    if (File.Exists(dxPath + "UserData\\ApplicationSettings.sqlite"))
        //        using (SimpleDataBase dxDb = new SimpleDataBase().Init(dxPath + "UserData\\ApplicationSettings.sqlite", false))
        //        {
        //            if (dxDb == null)
        //                return null;

        //            return dxDb.GetValue<string>("MapDatabaseDirectory", null);
        //        }
        //    else
        //        return null;
        //}

        private void Mdd_FileDownloaded(string filePath)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) =>
            {
                WorkingPath = s + ".temp\\";
                ZipFilePath = s;
                //AddToHistory(ZipFilePath);

                Thread thread = new Thread(() => RunThread());
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

            }), filePath);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rtbLogLines.SelectionChanged += RtbLogLines_SelectionChanged;

            //MenuItem men = new MenuItem()
            //{
            //    Header = "Open Zip File"
            //};
            //men.Click += MenuOpenZipFile_Click;
            //MenuFile.Items.Add(men);

            //MenuItem menF = new MenuItem()
            //{
            //    Header = "Open Folder"
            //};
            //menF.Click += MenuOpenFolder_Click;
            //MenuFile.Items.Add(menF);

            //MenuItem menR = new MenuItem()
            //{
            //    Header = "Open From LD/EM"
            //};
            //menR.Click += MenuOpenFromRobot_Click;
            //MenuFile.Items.Add(menR);

            //Separator sep = new Separator()
            //{
            //    Height = 3,
            //    Width = double.NaN,
            //    BorderThickness = new Thickness(1)
            //};
            //MenuFile.Items.Add(sep);

            //UpdateHistoryMenuItems(App.Settings.GetValue("FileHistory", new List<FileHistory>()));
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //if (FormLoading) return;
            //WindowSettings.Update(this.Top, this.Left, this.Width, this.Height, this.WindowState);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (FormLoading) return;
            //WindowSettings.Update(this.Top, this.Left, this.Width, this.Height, this.WindowState);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //App.Settings.SetValue("WindowSettings", WindowSettings);
        }

        private void Mf_FileOpened(string filePath)
        {
            //string msg = $"Parsing file: {filePath}";
            //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) =>
            //{
            //    //lblStatus.Content = s; 
            //    LstStatusList.Items.Insert(0, s);
            //}), msg);
            //Thread.Sleep(1);
        }



        //private void MenuOpenZipFile_Click(object sender, RoutedEventArgs e)
        //{
        //    Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog
        //    {
        //        CheckFileExists = true,
        //        CheckPathExists = true,
        //        Filter = "Zip file (*.zip)|*.zip",
        //        FilterIndex = 1
        //    };

        //    if ((bool)file.ShowDialog())
        //    {
        //        WorkingPath = file.FileName + ".temp\\";
        //        ZipFilePath = file.FileName;
        //        AddToHistory(ZipFilePath);

        //        Thread thread = new Thread(() => RunThread());
        //        thread.SetApartmentState(ApartmentState.STA);
        //        thread.Start();
        //    }
        //}
        //private void MenuOpenFolder_Click(object sender, EventArgs e)
        //{
        //    //var fol = new System.Windows.Forms.FolderBrowserDialog
        //    //{
        //    //    ShowNewFolderButton = false
        //    //};

        //    //if (fol.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    //{
        //    //    WorkingPath = fol.SelectedPath + "\\";
        //    //    ZipFilePath = null;
        //    //    AddToHistory(WorkingPath);

        //    //    Thread thread = new Thread(() => RunThread());
        //    //    thread.SetApartmentState(ApartmentState.STA);
        //    //    thread.Start();
        //    //}
        //}
        //private void MenuOpenFromRobot_Click(object sender, RoutedEventArgs e)
        //{
        //    MobileDebugDownload mdd = new MobileDebugDownload
        //    {
        //        Owner = this,
        //        WindowStartupLocation = WindowStartupLocation.CenterOwner
        //    };
        //    mdd.FileDownloaded += Mdd_FileDownloaded;
        //    mdd.Show();
        //}
        //private void MenuSettings_Search_Click(object sender, RoutedEventArgs e)
        //{
        //    LogDetailsEditor win = new LogDetailsEditor
        //    {
        //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        //        Owner = this
        //    };
        //    win.Show();
        //}
        //private void MenuSettings_Application_Click(object sender, RoutedEventArgs e)
        //{
        //    ApplicationSettingsEditor appEd = new ApplicationSettingsEditor
        //    {
        //        Owner = this,
        //        WindowStartupLocation = WindowStartupLocation.CenterOwner
        //    };
        //    appEd.Show();
        //}

        //private class FileHistory
        //{
        //    public string Path { get; set; }
        //    public bool IsDirectory { get; set; } = false;
        //}
        //private void AddToHistory(string filePath)
        //{
        //    List<FileHistory> history = App.Settings.GetValue("FileHistory", new List<FileHistory>());

        //    FileHistory fhFile;
        //    if (!File.Exists(filePath) && !Directory.Exists(filePath))
        //    {
        //        IEnumerable<FileHistory> his = history.Where(s => s.Path.Equals(filePath));

        //        foreach (FileHistory fh in history.ToList())
        //            history.Remove(fh);
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

        //        if (!history.Contains(fhFile))
        //            history.Add(fhFile);
        //    }

        //    App.Settings.SetValue("FileHistory", history);

        //    UpdateHistoryMenuItems(history);
        //}
        //private void UpdateHistoryMenuItems(List<FileHistory> history)
        //{
        //    int i = MenuFile.Items.Count - 1;

        //    for (; i > 0; i--)
        //    {
        //        if (MenuFile.Items[i] is MenuItem mi)
        //        {
        //            mi.Click -= HistoryMenuItem_Click;
        //            MenuFile.Items.Remove(mi);
        //        }
        //        else if (MenuFile.Items[i] is Separator)
        //            break;
        //    }

        //    foreach (FileHistory filePath in history)
        //    {
        //        MenuItem tsm = new MenuItem
        //        {
        //            Header = filePath.IsDirectory ? System.IO.Path.GetDirectoryName(filePath.Path) : System.IO.Path.GetFileName(filePath.Path),
        //            Tag = filePath
        //        };
        //        tsm.Click += HistoryMenuItem_Click;
        //        MenuFile.Items.Add(tsm);
        //    }
        //}
        //private void HistoryMenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is MenuItem mi)
        //    {
        //        if (mi.Tag is FileHistory filePath)
        //        {
        //            if (!filePath.IsDirectory)
        //            {
        //                if (!File.Exists(filePath.Path))
        //                {
        //                    AddToHistory(filePath.Path);
        //                    return;
        //                }

        //                WorkingPath = filePath.Path + ".temp\\";
        //                ZipFilePath = filePath.Path;

        //                Thread thread = new Thread(() => RunThread());
        //                thread.SetApartmentState(ApartmentState.STA);
        //                thread.Start();
        //            }
        //            else
        //            {
        //                if (!Directory.Exists(filePath.Path))
        //                {
        //                    AddToHistory(filePath.Path);
        //                    return;
        //                }

        //                WorkingPath = filePath.Path;
        //                ZipFilePath = null;

        //                Thread thread = new Thread(() => RunThread());
        //                thread.SetApartmentState(ApartmentState.STA);
        //                thread.Start();
        //            }
        //        }
        //    }
        //}

        private void ExpTOC_Collapsed(object sender, RoutedEventArgs e) => App.Settings.SetValue("ExpanderTOC", ((Expander)sender).IsExpanded);
        private void ExpTOC_Expanded(object sender, RoutedEventArgs e) => App.Settings.SetValue("ExpanderTOC", ((Expander)sender).IsExpanded);

        private void ExpSystemInfo_Collapsed(object sender, RoutedEventArgs e) => App.Settings.SetValue("ExpanderSystemInfo", ((Expander)sender).IsExpanded);
        private void ExpSystemInfo_Expanded(object sender, RoutedEventArgs e) => App.Settings.SetValue("ExpanderSystemInfo", ((Expander)sender).IsExpanded);

        private void ExpMapPointsOffMap_Collapsed(object sender, RoutedEventArgs e)
        {

        }

        private void ExpMapPointsOffMap_Expanded(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSaveMapImage_Click(object sender, RoutedEventArgs e)
        {
            //SaveBMPtoJPGFile((BitmapSource)((Image)ImgMapBorder.Child).Source);
        }

        private void BtnSaveWiFiHeatMapImage_Click(object sender, RoutedEventArgs e)
        {
            SaveBMPtoJPGFile((BitmapSource)((Image)ImgMapWiFiHeatBorder.Child).Source);
        }

        private void SaveBMPtoJPGFile(BitmapSource source)
        {
            Microsoft.Win32.SaveFileDialog file = new Microsoft.Win32.SaveFileDialog
            {
                CheckFileExists = false,
                AddExtension = true,
                CheckPathExists = true,
                Filter = "Image File (*.jpg)|*.jpg",
                FilterIndex = 1
            };

            string filePath;
            if (file.ShowDialog() == true)
                filePath = file.FileName;
            else return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
                encoder.Save(stream);
        }

        private void BtnSaveWiFiDecibelsGraphImage_Click(object sender, RoutedEventArgs e)
        {
            SaveBMPtoJPGFile(Chart_WiFiDecibels.ToBitmap());
        }

        private void BtnSaveWiFiBaudGraphImage_Click(object sender, RoutedEventArgs e)
        {
            SaveBMPtoJPGFile(Chart_WiFiBaud.ToBitmap());
        }

        private void BtnSaveBatteryVoltageGraphImage_Click(object sender, RoutedEventArgs e)
        {
            SaveBMPtoJPGFile(Chart_BatteryVoltage.ToBitmap());
        }

        private void BtnSaveBatteryChargeStateGraphImage_Click(object sender, RoutedEventArgs e)
        {
            SaveBMPtoJPGFile(Chart_BatteryChargeState.ToBitmap());
        }




    }
}
