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

        private static LogDetails_Serializer.LogDetails Serial;
        public LogDetailsEditor()
        {
            InitializeComponent();

            Serial = LogDetails_Serializer.Load($"{SearchConfigurationPath}LogDetails.xml");

            Load();

        }

        private void Load()
        {
            foreach (LogDetails_Serializer.LogDetailsLog log in Serial.Log)
            {

                StackPanel stkLog = new StackPanel()
                {
                    Orientation= Orientation.Horizontal,
                    
                };
                TextBox txtLogDisplayName = new TextBox()
                {
                    Text=log.DisplayName,
                    //IsReadOnly = true,
                    BorderBrush = Background,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };
                txtLogDisplayName.TextChanged += (sender, e) =>
                {
                    log.DisplayName = ((TextBox)sender).Text;
                };
                stkLog.Children.Add(txtLogDisplayName);
                CheckBox chkLogIsEm = new CheckBox()
                {
                    IsChecked = log.isEM,
                    Content = "Is for EM?",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 28
                };
                chkLogIsEm.Click += (sender, e) =>
                {
                    log.isEM = (bool)((CheckBox)sender).IsChecked;
                };
                stkLog.Children.Add(chkLogIsEm);
                CheckBox chkLogIsLD = new CheckBox()
                {
                    IsChecked = log.isLD,
                    Content = "Is for LD?",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 28
                };
                chkLogIsLD.Click += (sender, e) =>
                {
                    log.isLD = (bool)((CheckBox)sender).IsChecked;
                };
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

                StackPanel stkLogFileName = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                Label lblLogFileName = new Label()
                {
                    Content = "File Name:",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0),
                    Height = 28
                };
                TextBox txtLogFileName = new TextBox()
                {
                    Text = log.FileName,
                    //IsReadOnly = true,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderBrush = Background
                };
                txtLogFileName.TextChanged += (sender, e) =>
                {
                    log.FileName = ((TextBox)sender).Text;
                };
                CheckBox chkLogIsMulti = new CheckBox()
                {
                    IsChecked = log.MultiLog,
                    Content = "Has multiple logs?",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 28
                };
                chkLogIsMulti.Click += (sender, e) =>
                {
                    log.MultiLog = (bool)((CheckBox)sender).IsChecked;
                };
                stkLogFileName.Children.Add(lblLogFileName);
                stkLogFileName.Children.Add(txtLogFileName);
                stkLogFileName.Children.Add(chkLogIsMulti);
                stkLogDetails.Children.Add(stkLogFileName);

                StackPanel stkLogFilePath = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                Label lblLogFilePath = new Label()
                {
                    Content = "File Path:",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0),
                    Height = 28
                };
                TextBox txtLogFilePath = new TextBox()
                {
                    Text = log.FilePath,
                    IsReadOnly = true,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderBrush = Background
                };
                txtLogFileName.TextChanged += (sender, e) =>
                {
                    log.FilePath = ((TextBox)sender).Text;
                };
                stkLogFilePath.Children.Add(lblLogFilePath);
                stkLogFilePath.Children.Add(txtLogFilePath);
                stkLogDetails.Children.Add(stkLogFilePath);

                StackPanel stkLogFileType = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                Label lblLogFileType = new Label()
                {
                    Content = "File Type:",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0),
                    Height = 28
                };
                ComboBox cmbLogFileType = new ComboBox()
                {
                    IsReadOnly = true,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Height = 28
                };
                cmbLogFileType.Items.Add("TEXT");
                cmbLogFileType.Items.Add("CSV");
                cmbLogFileType.SelectedValue = log.FileType;
                cmbLogFileType.SelectionChanged += (sender, e) =>
                {
                    log.FileType = (string)((ComboBox)sender).SelectedValue;
                };

                stkLogFileType.Children.Add(lblLogFileType);
                stkLogFileType.Children.Add(cmbLogFileType);
                stkLogDetails.Children.Add(stkLogFileType);

                tviLog.Items.Add(stkLogDetails);

                foreach (LogDetails_Serializer.LogDetailsLogSearch ser in log.Search)
                {
                    StackPanel stkSearch = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                    };
                    TextBox txtSearchDisplayName = new TextBox()
                    {
                        Text = ser.DisplayName,
                        //IsReadOnly = true,
                        BorderBrush = Background,
                        VerticalContentAlignment = VerticalAlignment.Center,
                    };
                    txtSearchDisplayName.TextChanged += (sender, e) =>
                    {
                        ser.DisplayName = ((TextBox)sender).Text;
                    };
                    stkSearch.Children.Add(txtSearchDisplayName);
                    CheckBox chkSearchIsEm = new CheckBox()
                    {
                        IsChecked = ser.isEM,
                        Content = "Is for EM?",
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 0),
                        Height = 28
                    };
                    chkSearchIsEm.Click += (sender, e) =>
                    {
                        ser.isEM = (bool)((CheckBox)sender).IsChecked;
                    };
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
                        //IsReadOnly = true,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        BorderBrush = Background
                    };
                    txtSearchRegex.TextChanged += (sender, e) =>
                    {
                        ser.RegEx2Match = ((TextBox)sender).Text;
                    };
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //LogDetails_Serializer.LogDetails sLog = LogDetails_Serializer.Load($"{SearchConfigurationPath}LogDetails.xml");
            //if (sLog.GetHashCode().Equals(Serial.GetHashCode())) return;

            if (UserMessage.Show(this, "Would you like to save the changes?", "Save Changes?", new List<string>(){ "No","Yes"}) == "Yes")
            {
                File.Copy($"{SearchConfigurationPath}LogDetails.xml", $"{SearchConfigurationPath}LogDetails_{DateTime.Now.ToOADate().ToString()}.xml");
                LogDetails_Serializer.Save($"{SearchConfigurationPath}LogDetails.xml", Serial);
            }
        }
    }
}
