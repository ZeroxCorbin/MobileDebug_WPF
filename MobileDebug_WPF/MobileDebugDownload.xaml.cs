using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;


namespace MobileDebug_WPF
{

    public partial class MobileDebugDownload : Window
    {
        public delegate void FileDownloadedEventHandler(string filePath);
        public event FileDownloadedEventHandler FileDownloaded;

        private Classes.MobileDownload Download { get; set; } = new Classes.MobileDownload();

        public MobileDebugDownload()
        {
            InitializeComponent();

            TxtIPAddress.Text = App.Settings.GetValue("MobileDebugDownload.IPAddress");
            TxtUserName.Text = App.Settings.GetValue("MobileDebugDownload.UserName");
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.SetValue("MobileDebugDownload.IPAddress", TxtIPAddress.Text);
            App.Settings.SetValue("MobileDebugDownload.UserName", TxtUserName.Text);

            Microsoft.Win32.SaveFileDialog saveDiag = new Microsoft.Win32.SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "Zip file (*.zip)|*.zip",
                FilterIndex = 1
            };

            if (saveDiag.ShowDialog() == false)
                return;

            if (Download.StartGetDebugFile(TxtUserName.Text, TxtPassword.Password, TxtIPAddress.Text, saveDiag.FileName))
            {
                Download.DownloadProgressChanged += Download_DownloadProgressChanged;
                Download.DownloadFileCompleted += Download_DownloadFileCompleted;
            }
        }

        private void Download_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                FileDownloaded?.BeginInvoke("", null, null);
                Close();
            }
                
        }

        private void Download_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            PrgDownload.Maximum = 100;
            PrgDownload.Value = e.ProgressPercentage;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Download?.Dispose();
        }
    }
}
