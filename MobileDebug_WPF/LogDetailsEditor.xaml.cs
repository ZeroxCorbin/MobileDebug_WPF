using MobileDebug_WPF.Config;
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
    /// Interaction logic for LogDetailsEditor.xaml
    /// </summary>
    public partial class LogDetailsEditor : Window
    {
        private string SearchConfigurationPath => System.AppDomain.CurrentDomain.BaseDirectory + "Config\\";

        private static LogDetails Serial;
        public LogDetailsEditor()
        {
            InitializeComponent();

            Serial = LogDetails_Serializer.Load($"{SearchConfigurationPath}LogDetails.xml");

            Load();

        }

        private class TextTag
        {
            public string Data { get; set; }
            public bool IsLog { get; set; } = false;
            public LogDetailsLog Log { get; set; }
            public bool IsSearch { get; set; } = false;
            public LogDetailsLogSearch Search { get; set; }
        }

        private void Load()
        {
            TvMain.Items.Clear();

            foreach (LogDetailsLog log in Serial.Log)
            {
                StackPanel stkLog = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,

                };

                TextBox txtLogDisplayName = new TextBox()
                {
                    Text = log.DisplayName,
                    Tag = new TextTag() { Data = log.DisplayName, IsLog = true, Log = log },
                    IsReadOnly = true,
                    BorderBrush = Background,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };
                txtLogDisplayName.GotFocus += (sender, e) => SelectedTextBox = (TextBox)sender;
                txtLogDisplayName.TextChanged += (sender, e) => log.DisplayName = ((TextBox)sender).Text;
                stkLog.Children.Add(txtLogDisplayName);

                CheckBox chkLogIsEm = new CheckBox()
                {
                    IsChecked = log.isEM,
                    Content = "Is for EM?",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 28
                };
                chkLogIsEm.Click += (sender, e) => log.isEM = (bool)((CheckBox)sender).IsChecked;
                stkLog.Children.Add(chkLogIsEm);

                CheckBox chkLogIsLD = new CheckBox()
                {
                    IsChecked = log.isLD,
                    Content = "Is for LD?",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 28
                };
                chkLogIsLD.Click += (sender, e) => log.isLD = (bool)((CheckBox)sender).IsChecked;
                stkLog.Children.Add(chkLogIsLD);

                TreeViewItem tviLog = new TreeViewItem()
                {
                    Header = stkLog
                };
                tviLog.Selected += (sender, e) =>
                {
                    if (((TreeViewItem)sender).IsSelected)
                        ((TreeViewItem)sender).IsSelected = false;
                };
                TvMain.Items.Add(tviLog);

                StackPanel stkLogDetails = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                };
                tviLog.Items.Add(stkLogDetails);

                StackPanel stkLogFileName = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                stkLogDetails.Children.Add(stkLogFileName);

                Label lblLogFileName = new Label()
                {
                    Content = "File Name:",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0),
                    Height = 28
                };
                stkLogFileName.Children.Add(lblLogFileName);

                TextBox txtLogFileName = new TextBox()
                {
                    Text = log.FileName,
                    Tag = new TextTag() { Data = log.FileName },
                    IsReadOnly = true,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderBrush = Background
                };
                txtLogFileName.GotFocus += (sender, e) => SelectedTextBox = (TextBox)sender;
                txtLogFileName.TextChanged += (sender, e) => log.FileName = ((TextBox)sender).Text;
                stkLogFileName.Children.Add(txtLogFileName);

                CheckBox chkLogIsMulti = new CheckBox()
                {
                    IsChecked = log.MultiLog,
                    Content = "Has multiple logs?",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 28
                };
                chkLogIsMulti.Click += (sender, e) => log.MultiLog = (bool)((CheckBox)sender).IsChecked;
                stkLogFileName.Children.Add(chkLogIsMulti);

                StackPanel stkLogFilePath = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                stkLogDetails.Children.Add(stkLogFilePath);

                Label lblLogFilePath = new Label()
                {
                    Content = "File Path:",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0),
                    Height = 28
                };
                stkLogFilePath.Children.Add(lblLogFilePath);

                TextBox txtLogFilePath = new TextBox()
                {
                    Text = log.FilePath,
                    Tag = new TextTag() { Data = log.FilePath },
                    IsReadOnly = true,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderBrush = Background
                };
                txtLogFilePath.GotFocus += (sender, e) => SelectedTextBox = (TextBox)sender;
                txtLogFileName.TextChanged += (sender, e) => log.FilePath = ((TextBox)sender).Text;
                stkLogFilePath.Children.Add(txtLogFilePath);

                StackPanel stkLogFileType = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                stkLogDetails.Children.Add(stkLogFileType);

                Label lblLogFileType = new Label()
                {
                    Content = "File Type:",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0),
                    Height = 28
                };
                stkLogFileType.Children.Add(lblLogFileType);

                ComboBox cmbLogFileType = new ComboBox()
                {
                    IsReadOnly = true,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Height = 28
                };
                cmbLogFileType.Items.Add("TEXT");
                cmbLogFileType.Items.Add("CSV");
                cmbLogFileType.SelectedValue = log.FileType;
                cmbLogFileType.SelectionChanged += (sender, e) => log.FileType = (string)((ComboBox)sender).SelectedValue;
                stkLogFileType.Children.Add(cmbLogFileType);

                foreach (LogDetailsLogSearch ser in log.Search)
                {
                    StackPanel stkSearch = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                    };

                    TextBox txtSearchDisplayName = new TextBox()
                    {
                        Text = ser.DisplayName,
                        Tag = new TextTag() { Data = ser.DisplayName, IsSearch = true, Search = ser },
                        IsReadOnly = true,
                        BorderBrush = Background,
                        VerticalContentAlignment = VerticalAlignment.Center,
                    };
                    txtSearchDisplayName.GotFocus += (sender, e) => SelectedTextBox = (TextBox)sender;
                    txtSearchDisplayName.TextChanged += (sender, e) => ser.DisplayName = ((TextBox)sender).Text;

                    stkSearch.Children.Add(txtSearchDisplayName);
                    CheckBox chkSearchIsEm = new CheckBox()
                    {
                        IsChecked = ser.isEM,
                        Content = "Is for EM?",
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 0),
                        Height = 28
                    };
                    chkSearchIsEm.Click += (sender, e) => ser.isEM = (bool)((CheckBox)sender).IsChecked;

                    stkSearch.Children.Add(chkSearchIsEm);
                    CheckBox chkSearchIsLD = new CheckBox()
                    {
                        IsChecked = ser.isLD,
                        Content = "Is for LD?",
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 0),
                        Height = 28
                    };
                    chkSearchIsLD.Click += (sender, e) =>
                    {
                        ser.isLD = (bool)((CheckBox)sender).IsChecked;
                    };
                    stkSearch.Children.Add(chkSearchIsLD);

                    TreeViewItem tviSearch = new TreeViewItem()
                    {
                        Header = stkSearch
                    };
                    tviSearch.Selected += (sender, e) =>
                    {
                        if (((TreeViewItem)sender).IsSelected)
                            ((TreeViewItem)sender).IsSelected = false;
                    };

                    tviLog.Items.Add(tviSearch);

                    StackPanel stkSearchDetails = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                    };

                    StackPanel stkSearchRegex = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                    };
                    Label lblSearchRegex = new Label()
                    {
                        Content = "RegEx: ",
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 10, 0),
                        Height = 28
                    };
                    TextBox txtSearchRegex = new TextBox()
                    {
                        Text = ser.RegEx2Match,
                        Tag = new TextTag() { Data = ser.RegEx2Match },
                        IsReadOnly = true,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        BorderBrush = Background
                    };
                    txtSearchRegex.GotFocus += (sender, e) => SelectedTextBox = (TextBox)sender;
                    txtSearchRegex.TextChanged += (sender, e) => ser.RegEx2Match = ((TextBox)sender).Text;

                    stkSearchRegex.Children.Add(lblSearchRegex);
                    stkSearchRegex.Children.Add(txtSearchRegex);
                    stkSearchDetails.Children.Add(stkSearchRegex);

                    StackPanel stkSearchLevel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                    };
                    Label lblSearchLevel = new Label()
                    {
                        Content = "Level:",
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 10, 0),
                        Height = 28
                    };
                    ComboBox cmbSearchLevel = new ComboBox()
                    {
                        IsReadOnly = true,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Height = 28
                    };
                    cmbSearchLevel.Items.Add("DEBUG");
                    cmbSearchLevel.Items.Add("INFO");
                    cmbSearchLevel.Items.Add("WARN");
                    cmbSearchLevel.Items.Add("ERROR");
                    cmbSearchLevel.Items.Add("FATAL");
                    cmbSearchLevel.SelectedValue = ser.Level;
                    cmbSearchLevel.SelectionChanged += (sender, e) =>
                    {
                        ser.Level = (string)((ComboBox)sender).SelectedValue;
                    };

                    stkSearchLevel.Children.Add(lblSearchLevel);
                    stkSearchLevel.Children.Add(cmbSearchLevel);
                    stkSearchDetails.Children.Add(stkSearchLevel);

                    tviSearch.Items.Add(stkSearchDetails);
                }
            }
        }

        private void StkSearch_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private TextBox SelectedTextBox { get; set; } = null;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //LogDetails_Serializer.LogDetails sLog = LogDetails_Serializer.Load($"{SearchConfigurationPath}LogDetails.xml");
            //if (sLog.GetHashCode().Equals(Serial.GetHashCode())) return;

            //if (UserMessage.Show(this, "Would you like to save the changes?", "Save Changes?", new List<string>() { "Yes", "No" }) == "Yes")
            //{
            //    File.Copy($"{SearchConfigurationPath}LogDetails.xml", $"{SearchConfigurationPath}LogDetails_{DateTime.Now.ToOADate().ToString()}.xml");
            //    LogDetails_Serializer.Save($"{SearchConfigurationPath}LogDetails.xml", Serial);
            //}
        }

        private void TvMain_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (SelectedTextBox == null)
            {
                e.Handled = true;
                return;
            }

            TextTag tag = (TextTag)SelectedTextBox.Tag;
            if (tag.IsLog)
            {
                MenuNewLog.Visibility = Visibility.Visible;
                MenuDeleteLog.Visibility = Visibility.Visible;

                MenuNewSearch.Visibility = Visibility.Collapsed;
                MenuDeleteSearch.Visibility = Visibility.Collapsed;

                MenuEdit.Visibility = Visibility.Visible;
            }
            else if (tag.IsSearch)
            {
                MenuNewLog.Visibility = Visibility.Collapsed;
                MenuDeleteLog.Visibility = Visibility.Collapsed;

                MenuNewSearch.Visibility = Visibility.Visible;
                MenuDeleteSearch.Visibility = Visibility.Visible;

                MenuEdit.Visibility = Visibility.Visible;
            }
            else
            {
                MenuNewLog.Visibility = Visibility.Collapsed;
                MenuDeleteLog.Visibility = Visibility.Collapsed;

                MenuNewSearch.Visibility = Visibility.Collapsed;
                MenuDeleteSearch.Visibility = Visibility.Collapsed;

                MenuEdit.Visibility = Visibility.Visible;
            }
        }

        private void MenuNewLog_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuDeleteLog_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuNewSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuDeleteSearch_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
