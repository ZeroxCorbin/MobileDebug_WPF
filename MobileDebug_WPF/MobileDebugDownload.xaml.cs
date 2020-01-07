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

        public MobileDebugDownload()
        {
            InitializeComponent();

            TxtIPAddress.Text = App.Settings.GetValue("MobileDebugDownload.IPAddress");
            TxtUserName.Text = App.Settings.GetValue("MobileDebugDownload.UserName");
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip;
            try
            {
                ip = IPAddress.Parse(TxtIPAddress.Text);
            }
            catch
            {
                //UserMessage.Show(this, "Invalid IP : " + TxtIPAddress.Text, "Ooops!", new List<string>() { "OK" });
                return;
            }

            App.Settings.SetValue("MobileDebugDownload.IPAddress", ip.ToString());

            Microsoft.Win32.SaveFileDialog saveDiag = new Microsoft.Win32.SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "Zip file (*.zip)|*.zip",
                FilterIndex = 1
            };

            if (saveDiag.ShowDialog() == false)
                return;

            string fileName = System.IO.Path.GetFileNameWithoutExtension(saveDiag.FileName);
            string filePath = System.IO.Path.GetDirectoryName(saveDiag.FileName);

            fileName = $"{fileName}_{DateTime.Now.ToString("MMddyy_HHmmss")}.zip";
            string file = System.IO.Path.Combine(filePath, fileName);

            ServicePointManager.ServerCertificateValidationCallback += (sender1, certificate, chain, sslPolicyErrors) => true;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Credentials = new NetworkCredential(TxtUserName.Text, TxtPassword.Text);
                    wc.DownloadFile("https://" + ip.ToString() + "/cgi-bin/debugInfo.cgi", file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //UserMessage.Show(this, ex.Message , "Ooops!", new List<string>() { "OK" });
                    return;
                }
            }

            App.Settings.SetValue("MobileDebugDownload.UserName", TxtUserName.Text);

            FileDownloaded?.Invoke(file);

            this.Close();
        }
    }
}
