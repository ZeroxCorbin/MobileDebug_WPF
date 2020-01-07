using MobileLogs;
using MobileMap;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
using static FileSearch.FileSearch;

namespace MobileDebug_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class LogDetails_class
        {
            public string LogFileFullName { get; set; }
            public IList<IEnumerable<FileSearchResults>> SearchResults { get; set; } = new List<IEnumerable<FileSearchResults>>();

            public LogDetails_Serializer.LogDetailsLog Log { get; set; }
        }

        private bool FormLoading { get; set; } = true;
        private SimpleDataBase.WindowSettings WindowSettings { get; set; }
        private Brush ButtonFace { get; set; }

        //Set by CheckProductType() if the debug data is for an EM.
        public bool IsEM { get; private set; }
        //If a zip file is opened this is the path and name.
        public string ZipFilePath { get; private set; }
        //This is either the user selected folder or the extracted folder of the ZIP file.
        private string WorkingPath { get; set; }


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

        public MainWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;

            WindowSettings = App.Settings.GetValue("WindowSettings", new SimpleDataBase.WindowSettings());

            Title = "Mobile Debug (Beta)";

            if (WindowSettings.State == WindowState.Maximized) this.WindowState = WindowSettings.State;
            this.Top = WindowSettings.Top;
            this.Left = WindowSettings.Left;

            this.Width = WindowSettings.Width;
            this.Height = WindowSettings.Height;

            if (!WindowSettings.IsOnScreen())
            {
                this.Top = 0;
                this.Left = 0;
            }

            FormLoading = false;
        }

        private void GoGo()
        {
            ClearForm();



            if (!string.IsNullOrEmpty(ZipFilePath))
            {
                lblStatus.Content = "Extracting Files...";
                new Action(() => { ExtractFile(); }).Invoke();
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

            try
            {
                LoadSystemHealth();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                LoadSystemDetails();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                LoadSystemApps();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                ReadTOC();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            SetupLogs();

            if (IsEM)
                tabBatteryLogs.Visibility = Visibility.Collapsed;
            else
            {
                tabBatteryLogs.Visibility = Visibility.Visible;

                try
                {
                    LoadBatteryLogs();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (IsEM)
                tabWiFiLogs.Visibility = Visibility.Collapsed;
            else
            {
                tabWiFiLogs.Visibility = Visibility.Visible;

                try
                {
                    if (!IsEM) LoadWiFiLogs();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            DirectoryInfo di = new DirectoryInfo(WorkingPath + "\\home\\admin");
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name.EndsWith(".map"))
                {
                    LoadMapDelegate del = new LoadMapDelegate(LoadMapThread);

                    lblStatus.Content = "Loading Map";
                    del.BeginInvoke(fi.FullName, new AsyncCallback(DrawMapCallBack), null);

                    break;
                }
            }
        }

        public delegate Map LoadMapDelegate(string path);
        public delegate Map LoadLoggedDataDelegate(Map map);

        public Map LoadMapThread(string path)
        {
            return new Map(path);
        }
        private void DrawMapCallBack(IAsyncResult result)
        {
            AsyncResult ar = (AsyncResult)result;
            LoadMapDelegate bp = (LoadMapDelegate)ar.AsyncDelegate;
            Map map = bp.EndInvoke(result);

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((path) => { LoadMap(path); }), map.MapFile.FilePath);

            if(!IsEM)
                LoadLoggedDataThread(map);

            //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<Map>((map1) =>
            //{
            //    LoadLoggedDataDelegate del = new LoadLoggedDataDelegate(LoadLoggedDataThread);

            //    lblStatus.Content = "Loading Logged Data";
            //    del.BeginInvoke(map1, new AsyncCallback(LoadLoggedDataCallBack), null); ;
            //}), map);

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => { lblStatus.Content = "Drawing Map"; }));
            Thread.Sleep(1);

            string bmp = DrawMap(map);

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) => { border.Child = new Image() { Source = Base64StringToBitmap(s) }; }), bmp);
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => { lblStatus.Content = "Completed"; }));
        }
        private Map LoadLoggedDataThread(Map map)
        {
            map.MapFile.LoggedWifi = new List<MobileLogs.WifiLogs.WifiLogData>();
            MobileLogs.MultiFile mf = new MobileLogs.MultiFile("wifiLog", WorkingPath + "\\var\\log");
            mf.FileOpened += Mf_FileOpened;

            var wifiFiles = mf.GetResults<MobileLogs.WifiLogs>();
            foreach (MobileLogs.WifiLogs ssid in wifiFiles)
                foreach (List<MobileLogs.WifiLogs.WifiLogData> dats in ssid.Results)
                    foreach (MobileLogs.WifiLogs.WifiLogData dat in dats.Reverse<MobileLogs.WifiLogs.WifiLogData>())
                    {
                        if (dat.Position.X == 0 & dat.Position.Y == 0 & dat.Heading == 0)
                            continue;
                        map.MapFile.LoggedWifi.Add(dat);
                    }


            map.MapFile.LoggedStatus = new List<MobileLogs.StatusLogs.Status>();
            mf = new MobileLogs.MultiFile("log_", WorkingPath + "\\var\\robot\\logs");
            mf.FileOpened += Mf_FileOpened;

            var statusFiles = mf.GetResults<MobileLogs.StatusLogs>();
            foreach (MobileLogs.StatusLogs status in statusFiles)
                foreach (MobileLogs.StatusLogs.Status dat in status.Results)
                {
                    if (dat.Position.X == 0 & dat.Position.Y == 0 & dat.Heading == 0)
                        continue;
                    map.MapFile.LoggedStatus.Add(dat);
                }

            return map;
        }

        //private void LoadLoggedDataCallBack(IAsyncResult result)
        //{
        //    AsyncResult ar = (AsyncResult)result;
        //    LoadLoggedDataDelegate bp = (LoadLoggedDataDelegate)ar.AsyncDelegate;
        //    Map map = bp.EndInvoke(result);


        //}

        private void ClearForm()
        {
            flpLogs.Children.Clear();
            flpWiFiLogs.Children.Clear();
            flpBatteryLogs.Children.Clear();

            stkTOC.Children.Clear();

            TvSystemData.Items.Clear();

            rtbLogLines.Document.Blocks.Clear();

            TxtLogLinesPrev.Text = string.Empty;

            LogDetails = new List<LogDetails_class>();
            LogIndices = new LogIndices();

            IsEM = false;

            if (ZipFilePath != null)
            {
                if (WorkingPath != "")
                {
                    if (!RemoveWorkDir())
                    {
                        MessageBox.Show("The extraction directory already exists and could not be removed.", "Ooops!", MessageBoxButton.OK);
                    }
                }
            }

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
            using (StreamReader file = new StreamReader(filePath))
            {
                return file.ReadLine();
            }
        }

        //private void CreateZipFile()
        //{
        //    if (!string.IsNullOrEmpty(ZipFilePath))
        //    {
        //        SaveFileDialog sfd = new SaveFileDialog();
        //        sfd.FileName = Path.GetFileName(ZipFilePath);// zipFileFullName.Substring(zipFileFullName.LastIndexOf('\\') + 1);
        //        sfd.Filter = "Zip file (*.zip)|*.zip";
        //        sfd.InitialDirectory = ZipFilePath.Substring(0, ZipFilePath.LastIndexOf('\\'));

        //        if (sfd.ShowDialog() == DialogResult.OK)
        //        {
        //            File.Copy(ZipFilePath, sfd.FileName, true);
        //        }
        //    }
        //    else if (!string.IsNullOrEmpty(WorkingPath))
        //    {
        //        SaveFileDialog sfd = new SaveFileDialog();
        //        sfd.FileName = WorkingPath.Substring(WorkingPath.LastIndexOf('\\') + 1);
        //        sfd.Filter = "Zip file (*.zip)|*.zip";
        //        sfd.InitialDirectory = WorkingPath.Substring(0, WorkingPath.LastIndexOf('\\'));

        //        if (sfd.ShowDialog() == DialogResult.OK)
        //        {
        //            if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);

        //            ZipFile.CreateFromDirectory(WorkingPath, sfd.FileName);
        //        }
        //    }
        //}


        //private void LoadSubbmittedDetails()
        //{
        //    SubmittedDetails_Serializer.SubmittedDetails serial = SubmittedDetails_Serializer.Load("Config\\SubmittedDetails.xml");

        //    TreeNode tn = new TreeNode();
        //    tn.NodeFont = new Font(this.Font, FontStyle.Bold);
        //    trvSystemData.Items.Add(tn);
        //    tn.Text = serial.Title;

        //    DirectoryInfo di = new DirectoryInfo(WorkingPath);

        //    foreach (FileInfo fi in di.GetFiles())
        //    {
        //        if (fi.Name.Contains(serial.Keyword))
        //        {
        //            submittedDetailsFullName = fi.FullName;
        //            using (StreamReader sr = new StreamReader(fi.FullName))
        //            {
        //                string line;
        //                while ((line = sr.ReadLine()) != null)
        //                {
        //                    if (string.IsNullOrEmpty(line)) continue;
        //                    TreeNode tn1 = new TreeNode();
        //                    tn.Nodes.Add(tn1);
        //                    tn1.Text = line;
        //                }
        //            }
        //        }
        //    }
        //    tn.Expand();
        //}

        private void CheckProductType()
        {
            string str = GetLineFromFile(WorkingPath + "\\mnt\\status\\platform\\productType");

            if (str.Contains("EM")) IsEM = true;
            else IsEM = false;
        }
        private void LoadSystemHealth()
        {
            SystemHealth_Serializer.SystemHealth serial = SystemHealth_Serializer.Load("Config\\SystemHealth.xml");

            foreach (SystemHealth_Serializer.SystemHealthHeading head in serial.Heading)
            {
                if (IsEM && !head.isEM) continue;//If the data is from an EM and the config file indicates the data is not relevant to an EM, the section is ignored.
                if (!IsEM && !head.isLD) continue;

                TreeViewItem tviHeading = new TreeViewItem()
                {
                    Header = head.Name,
                };
                TvSystemData.Items.Add(tviHeading);

                foreach (SystemHealth_Serializer.SystemHealthHeadingLabel label in head.Label)
                {
                    if (IsEM && !label.isEM) continue;
                    if (!IsEM && !label.isLD) continue;

                    double res = Convert.ToDouble(GetLineFromFile(WorkingPath + label.FilePath)) * Convert.ToDouble(label.Multiplier);

                    TextBox txtLabel = new TextBox() { Text = label.Name + res.ToString() + label.Tail, IsReadOnly = true, BorderThickness = new Thickness(0), Margin = new Thickness(3) };
                    TreeViewItem tviLabel = new TreeViewItem()
                    {
                        Header = txtLabel,
                        ToolTip = label.Description
                    };
                    tviHeading.Items.Add(tviLabel);

                    double thres;

                    if (IsEM) thres = Convert.ToDouble(label.Threshold_em);
                    else thres = Convert.ToDouble(label.Threshold_ld);

                    if (label.Greater)
                    {
                        if (res > thres)
                            txtLabel.Background = Brushes.LightYellow;
                        else
                            txtLabel.Background = Brushes.LightGreen;
                    }
                    else
                    {
                        if (res < thres)
                            txtLabel.Background = Brushes.LightYellow;
                        else
                            txtLabel.Background = Brushes.LightGreen;
                    }

                }

                tviHeading.IsExpanded = true;
            }
        }//Updated
        private void LoadSystemDetails()
        {
            SystemDetails_Serializer.SystemDetails serial = SystemDetails_Serializer.Load("Config\\SystemDetails.xml");

            foreach (SystemDetails_Serializer.SystemDetailsHeading head in serial.Heading)
            {
                if (IsEM && !head.isEM) continue;//If the data is from an EM and the config file indicates the data is not relevant to an EM, the section is ignored.
                if (!IsEM && !head.isLD) continue;

                TreeViewItem tviHeading = new TreeViewItem()
                {
                    Header = head.Name,
                };
                TvSystemData.Items.Add(tviHeading);

                foreach (SystemDetails_Serializer.SystemDetailsHeadingLabel label in head.Label)
                {
                    if (IsEM && !label.isEM) continue;
                    if (!IsEM && !label.isLD) continue;

                    TextBox txtLabel = new TextBox() { Text = label.Name + GetLineFromFile(WorkingPath + label.FilePath).Replace("\t", " , "), IsReadOnly = true, BorderThickness = new Thickness(0), Margin = new Thickness(3) };
                    TreeViewItem tviLabel = new TreeViewItem()
                    {
                        Header = txtLabel,
                        ToolTip = label.Description
                    };
                    tviHeading.Items.Add(tviLabel);
                }

                tviHeading.IsExpanded = true;
            }
        }//Updated
        private void LoadSystemApps()
        {
            SystemApps_Serializer.SystemApps serial = SystemApps_Serializer.Load("Config\\SystemApps.xml");

            DirectoryInfo di = new DirectoryInfo(WorkingPath + serial.Path);

            TreeViewItem tviHeading = new TreeViewItem()
            {
                Header = serial.Title,
            };
            TvSystemData.Items.Add(tviHeading);

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                foreach (FileInfo fi in dir.GetFiles())
                {
                    if (fi.Name == serial.FileName)
                    {
                        using (StreamReader file = new System.IO.StreamReader(fi.FullName))
                        {
                            string line = file.ReadLine();

                            TextBox txtLabel = new TextBox() { Text = line, IsReadOnly = true, BorderThickness = new Thickness(0), Margin = new Thickness(3) };
                            TreeViewItem tviLabel = new TreeViewItem()
                            {
                                Header = txtLabel,
                            };
                            tviHeading.Items.Add(tviLabel);

                            while ((line = file.ReadLine()) != null)
                            {
                                TextBox txtLabel1 = new TextBox() { Text = line, IsReadOnly = true, BorderThickness = new Thickness(0), Margin = new Thickness(3) };
                                TreeViewItem tviLabel1 = new TreeViewItem()
                                {
                                    Header = txtLabel1,
                                };
                                tviLabel.Items.Add(tviLabel1);
                            }
                        }
                    }
                }
            }

            tviHeading.IsExpanded = true;
        }//Updated

        //Table of Contents TAB
        private void ReadTOC()
        {
            using (StreamReader file = new StreamReader(WorkingPath + "\\toc.txt"))
            {
                file.ReadLine();
                file.ReadLine();
                file.ReadLine();


                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] row = new string[3];

                    int i = line.LastIndexOf(' ');
                    row[0] = line.Substring(i + 1);

                    if (row[0].ToString().EndsWith("/")) continue;
                    if (row[0].ToString().StartsWith("-")) break;

                    row[1] = line.Substring(i - 18, 10);
                    row[2] = line.Substring(i - 7, 5);

                    Hyperlink hyp = new Hyperlink()
                    {
                        Tag = new Uri(WorkingPath + row[0]).ToString(),
                    };
                    hyp.Inlines.Add(row[0]);
                    hyp.Click += TOCHyperlink_Click;

                    Label lbl = new Label()
                    {
                        Content = hyp,
                    };
                    stkTOC.Children.Add(lbl);

                }
            }
        }//Updated
        private void TOCHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            if (hl.Tag is string s)
                System.Diagnostics.Process.Start(s);
        }

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
            LogDetails_Serializer.LogDetails serial = LogDetails_Serializer.Load("Config\\LogDetails.xml");

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

        private void LoadWiFiLogs()
        {
            IList<FileInfo> lst = new List<FileInfo>();

            DirectoryInfo dir = new DirectoryInfo(WorkingPath + "\\var\\log\\");

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

                ButtonFace = btn.Background;
            }
        }
        private void WiFiLogButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Control c in flpWiFiLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            Button btn = (Button)sender;
            btn.Background = Brushes.LightGreen;

            string oldFileName = (string)btn.Tag;
            string newFileName = oldFileName;
            bool removeFile = false;

            if (oldFileName.EndsWith(".gz"))
            {
                using (FileStream decompressFileStream = new FileInfo(oldFileName).OpenRead())
                {
                    newFileName = oldFileName.TrimEnd(".gz".ToCharArray());

                    using (FileStream decompressedFileStream = File.Create(newFileName))
                    {
                        using (GZipStream gz = new GZipStream(decompressFileStream, CompressionMode.Decompress))
                        {
                            gz.CopyTo(decompressedFileStream);
                        }
                    }
                }
                removeFile = true;
            }

            WifiLogs log = new WifiLogs(newFileName);

            if (removeFile) File.Delete(newFileName);

            PlotModel wiFiDecibelsPlotModel = new PlotModel
            {
                Title = "WiFi Decibels (Signal Strength)",
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea,
                LegendTitle = "SSID Names",
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.LeftTop
            };
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

            PlotModel wiFiBaudPlotModel = new PlotModel
            {
                Title = "WiFi Baud Rate (Speed)",
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea,
                LegendTitle = "SSID Names",
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.LeftTop
            };
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

            DateTime dt = DateTime.Now;
            foreach (IList<WifiLogs.WifiLogData> ssid in log.Results)
            {
                IEnumerable<WifiLogs.WifiLogData> ssidSorted = from s in ssid
                                                               orderby s.Time
                                                               select s;
                LineSeries wiFiBaudLineSeries = new LineSeries
                {
                    StrokeThickness = 1,
                    Title = ssid[0].SSID,
                };
                wiFiBaudPlotModel.Series.Add(wiFiBaudLineSeries);

                LineSeries wiFiDecibelsLineSeries = new LineSeries
                {
                    StrokeThickness = 1,
                    Title = ssid[0].SSID,
                };
                wiFiDecibelsPlotModel.Series.Add(wiFiDecibelsLineSeries);

                foreach (WifiLogs.WifiLogData dat in ssid)
                {
                    dt = dat.Time;
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
            DirectoryInfo dir = new DirectoryInfo(WorkingPath + "\\var\\robot\\logs\\");

            IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

            IEnumerable<FileInfo> res =
                from test in names
                where test.Name.StartsWith("log_")
                select test;

            IList<FileInfo> lst = res.ToList<FileInfo>();

            foreach (FileInfo file in lst)
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
        private void BatteryLogsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Control c in flpBatteryLogs.Children)
                if (typeof(Button) == c.GetType())
                    c.Background = ButtonFace;

            Button btn = (Button)sender;
            btn.Background = Brushes.LightGreen;
            string newFileName = (string)btn.Tag;

            BatteryLogs log = new BatteryLogs(newFileName);

            PlotModel batterySOCPlotModel = new PlotModel
            {
                Title = "Battery State of Charge (%)",
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea,
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

            PlotModel batteryVoltagePlotModel = new PlotModel
            {
                Title = "Battery Voltage",
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea,
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


            IEnumerable<BatteryLogs.BatteryLogData> batterySorted = from s in log.Results
                                                                    orderby s.Time
                                                                    select s;

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

            foreach (BatteryLogs.BatteryLogData dat in batterySorted)
            {
                batterySOCLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Level));
                batteryVoltageLineSeries.Points.Add(new DataPoint(dat.Time.ToOADate(), dat.Voltage));
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


        private string DrawMap(Map map)
        {

            //DrawingVisual dv = new DrawingVisual();
            //Pen black = new Pen(Brushes.Black, 20);

            //map.Header.SetScaleF(1024, 1024);
            //using (DrawingContext dc = dv.RenderOpen())
            //{
            //    //dc..FillRectangle(, 0, 0, map.Header.WidthScaled, map.Header.HeightScaled);
            //    dc.PushTransform(new TranslateTransform(-map.Header.WidthOffset, -map.Header.HeightOffset - map.Header.Height));
            //    dc.PushTransform(new ScaleTransform(map.Header.ScaleFactor.Width, -map.Header.ScaleFactor.Height));

            //    foreach (MapGeometry.Line ln in map.Geometry.Lines)
            //        dc.DrawLine(black, new Point(ln.Start.X,ln.Start.Y), new Point(ln.End.X,ln.End.Y));

            //    foreach (System.Drawing.Point ln in map.Geometry.Points)
            //        dc.DrawRectangle(Brushes.Black, black, new Rect(ln.X, ln.Y, 20, 20));

            //    //dc.DrawRectangle(Brushes.Green, null, new Rect(20, 20, 150, 100));
            //}

            //RenderTargetBitmap rtb = new RenderTargetBitmap(map.Header.WidthScaled, map.Header.HeightScaled, 96, 96, PixelFormats.Pbgra32);
            //rtb.Render(dv);

            //return rtb;

            map.Header.SetScaleF(4096, 4096);
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(map.Header.WidthScaled, map.Header.HeightScaled, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            using (System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Black, 20))
            {
                using (System.Drawing.Brush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                                            whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                {
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        //picMapGraphics.Image = bm;
                        g.FillRectangle(whiteBrush, 0, 0, map.Header.WidthScaled, map.Header.HeightScaled);
                        g.TranslateTransform(-map.Header.WidthOffset, -map.Header.HeightOffset - map.Header.Height);
                        g.ScaleTransform(map.Header.ScaleFactor.Width, -map.Header.ScaleFactor.Height, System.Drawing.Drawing2D.MatrixOrder.Append);

                        foreach (MapGeometry.Line ln in map.Geometry.Lines)
                            g.DrawLine(blackPen, ln.Start, ln.End);

                        foreach (System.Drawing.Point ln in map.Geometry.Points)
                            g.FillRectangle(blackBrush, ln.X, ln.Y, 20, 20);

                        List<StatusLogs.Status> lost = DrawPositionsFromLog(g, map);

                        DrawWiFiPositionsFromLog(g, map);
                    }
                }
            }

            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                var SigBase64 = Convert.ToBase64String(memory.GetBuffer());
                return SigBase64;
            }
        }
        private List<StatusLogs.Status> DrawPositionsFromLog(System.Drawing.Graphics g, Map map)
        {
            List<StatusLogs.Status> lost = new List<StatusLogs.Status>();

            if (map.MapFile.LoggedStatus == null) return lost;

            System.Drawing.Rectangle r = new System.Drawing.Rectangle(map.Header.WidthOffset, map.Header.HeightOffset, map.Header.Width, map.Header.Height);
            int sz = (int)(40.0 * (1 + Math.Abs((map.Header.ScaleFactor.Width - 1.0))));
            System.Drawing.Brush blueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);
            foreach (StatusLogs.Status s in map.MapFile.LoggedStatus)
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
            if (map.MapFile.LoggedWifi == null) return;

            System.Drawing.Rectangle r = new System.Drawing.Rectangle(map.Header.WidthOffset, map.Header.HeightOffset, map.Header.Width, map.Header.Height);
            int sz = (int)(40.0 * (1 + Math.Abs((map.Header.ScaleFactor.Width - 1.0))));
            foreach (WifiLogs.WifiLogData s in map.MapFile.LoggedWifi)
            {
                System.Drawing.Brush bBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 255, 0, 0));

                if (r.Contains(s.Position))
                    g.FillPie(bBrush, new System.Drawing.Rectangle(s.Position.X, s.Position.Y, sz, sz), 0, 360);
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rtbLogLines.SelectionChanged += RtbLogLines_SelectionChanged;
            rtbLogLines.Selection.Changed += Selection_Changed;
            MenuItem men = new MenuItem()
            {
                Header = "Open Zip File"
            };
            men.Click += MenuOpenZipFile_Click;

            MenuFile.Items.Add(men);

            UpdateHistoryMenuItems(App.Settings.GetValue("FileHistory", new List<string>()));
        }

        private void Selection_Changed(object sender, EventArgs e)
        {

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

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (FormLoading) return;
            WindowSettings.Update(this.Top, this.Left, this.Width, this.Height, this.WindowState);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (FormLoading) return;
            WindowSettings.Update(this.Top, this.Left, this.Width, this.Height, this.WindowState);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Settings.SetValue("WindowSettings", WindowSettings);
        }

        //Map TAB
        private void LoadMap(string mapPath)
        {
            SystemMap_Serializer.SystemMap serial = SystemMap_Serializer.Load("Config\\SystemMap.xml");

            StringBuilder sb = new StringBuilder();
            sb.Append(rtfHead);

            using (StreamReader sr = new StreamReader(mapPath))
            {
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
                                    Tag = lineNum,
                                };
                                hl.Inlines.Add((string)sec.Label);
                                hl.Click += MapSectorHyperLink_Click;

                                Label lb = new Label
                                {
                                    Content = hl,
                                    Tag = lineNum
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

        }

        private void Mf_FileOpened(string filePath)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action<string>((s) => { lblStatus.Content = s; }), filePath);
            Thread.Sleep(1);
        }
        private void MapSectorHyperLink_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void MapLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{


        //    LinkLabel ll = (LinkLabel)sender;

        //    rtbMap.SelectionStart = rtbMap.GetFirstCharIndexFromLine((int)ll.Tag - 1);
        //    rtbMap.ScrollToCaret();

        //}


        //File Menu
        //private void tsmOpenFolder_Click(object sender, EventArgs e)
        //{


        //    FolderBrowserDialog fol = new FolderBrowserDialog();

        //    fol.ShowNewFolderButton = false;


        //    if ((bool)fol.ShowDialog())
        //    {
        //        FilesLoading = true;

        //        WorkingPath = fol.SelectedPath;
        //        ZipFilePath = null;
        //        AddToHistory(WorkingPath);

        //        GoGo();
        //    }

        //}

        //private void tsmGetFromRobot_Click(object sender, EventArgs e)
        //{


        //    frmDownload frm = new frmDownload();
        //    frm.ShowDialog();

        //    if (frm.ok)
        //    {
        //        FilesLoading = true;

        //        zipFileFullName = frm.filePath + frm.fileName;
        //        workPath = frm.filePath + frm.fileName + ".temp";
        //        AddToHistory(zipFileFullName);

        //        GoGo();
        //    }
        //    frm.Dispose();
        //}

        //private void tsmSaveMap_Click(object sender, EventArgs e)
        //{


        //    if (mapFullName != "")
        //    {
        //        SaveFileDialog sfd = new SaveFileDialog();

        //        sfd.FileName = Path.GetFileName(mapFullName);// mapFullName.Substring(mapFullName.LastIndexOf('\\')+1);
        //        sfd.Filter = "Map file (*.map)|*.map";
        //        sfd.InitialDirectory = workPath.Substring(0, workPath.LastIndexOf('\\'));

        //        if (sfd.ShowDialog() == DialogResult.OK)
        //        {
        //            File.Copy(mapFullName, sfd.FileName, true);
        //        }
        //    }
        //}

        //private void tsmSaveZip_Click(object sender, EventArgs e)
        //{


        //    CreateZipFile();
        //}

        //private void tsmUpload_Click(object sender, EventArgs e)
        //{


        //    if (string.IsNullOrEmpty(zipFileFullName)) return;

        //    //frmUpload frm = new frmUpload(zipFileFullName, submittedDetailsFullName);
        //    //frm.ShowDialog();
        //}

        private void MenuOpenZipFile_Click(object sender, RoutedEventArgs e)
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
                WorkingPath = file.FileName + ".temp";
                ZipFilePath = file.FileName;
                AddToHistory(ZipFilePath);

                GoGo();
            }
        }
        private void AddToHistory(string filePath)
        {
            List<string> history = App.Settings.GetValue("FileHistory", new List<string>());

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                if (history.Contains(filePath))
                    history.Remove(filePath);
            }
            else
            {
                if (!history.Contains(filePath))
                    history.Add(filePath);
            }

            App.Settings.SetValue("FileHistory", history);

            UpdateHistoryMenuItems(history);
        }
        private void UpdateHistoryMenuItems(List<string> history)
        {
            int i = MenuFile.Items.Count - 1;

            for (; i > 0; i--)
            {
                if (MenuFile.Items[i] is MenuItem mi)
                {
                    mi.Click -= HistoryMenuItem_Click;
                    MenuFile.Items.Remove(mi);
                }
            }


            foreach (string filePath in history)
            {
                MenuItem tsm = new MenuItem
                {
                    Header = System.IO.Path.GetFileName(filePath),
                    Tag = filePath
                };
                tsm.Click += HistoryMenuItem_Click;
                MenuFile.Items.Add(tsm);
            }
        }
        private void HistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                if (mi.Tag is string filePath)
                {
                    bool isFile = false;
                    bool isDir = false;

                    if (File.Exists(filePath)) isFile = true;
                    if (Directory.Exists(filePath)) isDir = true;

                    if (!isFile && !isDir)
                    {
                        AddToHistory(filePath);
                        return;
                    }

                    if (isFile)
                    {
                        WorkingPath = filePath + ".temp";
                        ZipFilePath = filePath;

                        GoGo();
                    }
                    else
                    {
                        WorkingPath = filePath;
                        ZipFilePath = null;


                        GoGo();
                    }
                }
            }
        }

        //private void DrawMap()
        //{
        //    System.Windows.Media.Pen blackPen = new Pen(Color.Black, 20);
        //    Brush blackBrush = new SolidBrush(Color.Black);
        //    Brush whiteBrush = new SolidBrush(Color.White);

        //    if (picMapGraphics.Image != null) picMapGraphics.Image.Dispose();

        //    MapGeometry.SetScaleF(panel1.Size.Width, panel1.Size.Height);
        //    Bitmap bm = new Bitmap(MapGeometry.WidthScaled - 40, MapGeometry.HeightScaled - 40, PixelFormat.Format16bppRgb555);

        //    using (Graphics g = Graphics.FromImage(bm))
        //    {
        //        picMapGraphics.Image = bm;
        //        g.FillRectangle(whiteBrush, 0, 0, MapGeometry.WidthScaled, MapGeometry.HeightScaled);
        //        g.TranslateTransform(-MapGeometry.WidthOffset, -MapGeometry.HeightOffset - MapGeometry.Height);
        //        g.ScaleTransform(MapGeometry.scaleFactor.Width, -MapGeometry.scaleFactor.Height, System.Drawing.Drawing2D.MatrixOrder.Append);

        //        foreach (MapGeometry.Line ln in MapGeometry.Lines)
        //        {
        //            g.DrawLine(blackPen, ln.Start, ln.End);
        //        }
        //        foreach (Point ln in MapGeometry.Points)
        //        {
        //            g.FillRectangle(blackBrush, ln.X, ln.Y, 20, 20);
        //        }

        //        DrawPositionsFromLog(g);
        //    }
        //    picMapGraphics.Refresh();
        //}

        //public class LogSearchResult
        //{
        //    public LogSearchResult(FileSearchResults r, Point p)
        //    {
        //        this.point = p;
        //        this.searchResult = r;
        //    }
        //    public Point point;
        //    public FileSearchResults searchResult;
        //}

        //public void AddLogSearchResult(FileSearchResults r, Point p)
        //{
        //    LogSearchResult rp = new LogSearchResult(r, p);
        //    LogSearchResults.Add(rp);
        //}

        //public List<LogSearchResult> LogSearchResults { get; set; } = new List<LogSearchResult>();

        //private void LoadPositionsFromLogs()
        //{
        //    LogDetails_Serializer.LogDetailsLog log = new LogDetails_Serializer.LogDetailsLog();
        //    log.MultiLog = true;
        //    log.isLD = true;
        //    log.FileName = "log";
        //    log.FilePath = "\\var\\robot\\logs\\";

        //    LogDetails_Serializer.LogDetailsLogSearch ser = new LogDetails_Serializer.LogDetailsLogSearch();
        //    ser.DisplayName = "Log";
        //    ser.isLD = true;
        //    ser.Level = "WARN";
        //    ser.RegEx2Match = "Robot at";

        //    log.Search = new LogDetails_Serializer.LogDetailsLogSearch[1];
        //    log.Search[0] = ser;
        //    LogIndices ind;

        //    IList<FileInfo> lst = new List<FileInfo>();

        //    if (log.MultiLog)
        //    {
        //        DirectoryInfo dir = new DirectoryInfo(WorkingPath + log.FilePath);

        //        IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

        //        IEnumerable<FileInfo> res =
        //            from test in names
        //            where test.Name.StartsWith(log.FileName)
        //            select test;

        //        lst = res.ToList<FileInfo>();
        //    }

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(rtfHead);

        //    int i = 0;
        //    foreach (FileInfo file in lst)
        //    {
        //        UpdateStatus("Processing log for (Robot at): " + file.Name);

        //        IEnumerable<FileSearchResults> searchRes = Enumerable.Empty<FileSearchResults>();

        //        LogDetails ld = new LogDetails();
        //        ld.Log = log;

        //        searchRes = FileSearch.FileSearch.Find(file.FullName, ser.RegEx2Match);

        //        ld.LogFileFullName = file.FullName;
        //        positionLogDetails.Add(ld);
        //        ind.log = i;
        //        ind.search = 0;

        //        int ii = -1;
        //        foreach (FileSearchResults dat in searchRes)
        //        {
        //            //Tue Oct  3 08:11:30.049143 Robot at 1779 45640 164.0
        //            ii++;

        //            if (dat.Line.Contains("mm/sec")) continue;

        //            //DateTime time;
        //            string[] spl = dat.Line.Split(' ');

        //            int xVal;
        //            int yVal;
        //            if (spl[3].Length == 1)
        //            {
        //                //time = DateTime.ParseExact(spl[1] + ' ' + spl[3] + " 2018 " + spl[4].Remove(spl[4].LastIndexOf('.')), "MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        //                xVal = Convert.ToInt32(spl[7].TrimEnd(','));
        //                yVal = Convert.ToInt32(spl[8].TrimEnd(','));
        //            }

        //            else
        //            {
        //                //time = DateTime.ParseExact(spl[1] + ' ' + spl[2] + " 2018 " + spl[3].Remove(spl[3].LastIndexOf('.')), "MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        //                xVal = Convert.ToInt32(spl[6].TrimEnd(','));
        //                yVal = Convert.ToInt32(spl[7].TrimEnd(','));
        //            }


        //            if (MapGeometry.Contains(xVal, yVal))
        //            {
        //                AddLogSearchResult(dat, new Point(xVal, yVal));
        //            }
        //            else
        //            {
        //                Point p = MapGeometry.OffsetTo(new Point(xVal, yVal));
        //                sb.AppendLine(dat.LineNumber.ToString() + "\\tab " + dat.Line + " - " + rtfSearchFormatsColors[3] + "Offset: " + p.X.ToString() + " " + p.Y.ToString() + rtfSearchFormatsColorsTail + "\\v log?" + ind.log.ToString("00") + "\\v0" + "\\par");
        //            }
        //        }

        //        i++;
        //    }
        //    sb.Append(rtfTail);

        //    rtbMapGLostPositions.Rtf = sb.ToString();

        //    tslMapGPosOnMap.Text = LogSearchResults.Count.ToString();
        //    tslMapGPosOffMap.Text = rtbMapGLostPositions.Lines.Count<string>().ToString();
        //}

        //private void DrawPositionsFromLog(Graphics g)
        //{
        //    if (LogSearchResults.Count <= 0) return;

        //    Brush redBrush = new SolidBrush(Color.Red);

        //    int sz = (int)(40.0 * (1 + Math.Abs((MapGeometry.scaleFactor.Width - 1.0))));

        //    IList<LinkLabel> links = new List<LinkLabel>();
        //    foreach (LogSearchResult p in LogSearchResults)
        //    {
        //        Rectangle r = new Rectangle(MapGeometry.WidthOffset, MapGeometry.HeightOffset, MapGeometry.Width, MapGeometry.Height);
        //        if (r.Contains(p.point))
        //        {

        //            g.FillRectangle(redBrush, p.point.X, p.point.Y, sz, sz);
        //        }
        //    }
        //}

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

        //bool MapLoaded = false;
        //private void panel1_SizeChanged(object sender, EventArgs e)
        //{


        //    if (MapGeometry == null) return;
        //    if (!MapLoaded) return;

        //    DrawMap();
        //}

    }
}
