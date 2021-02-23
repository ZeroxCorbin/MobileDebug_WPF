using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MobileDebug_WPF.Classes
{
    public class MobileDownload : IDisposable
    {
        public class ConnectionString
        {
            public ConnectionString() { }
            public ConnectionString(string ip, int port, string userName, string password) => String = $"{ip}:{port}:{userName}:{password}";

            public string String { get; set; } = "192.168.0.20:7171:admin:admin";
            public string IP => Regex.Match(String, @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)").Value;
            public string Port => Regex.Matches(String, @":[a-zA-Z0-9]*")[0].Value;
            public string UserName => Regex.Matches(String, @":[a-zA-Z0-9]*")[1].Value;
            public string Password => Regex.Matches(String, @":[a-zA-Z0-9]*")[2].Value;

            public bool IsValid => Regex.IsMatch(String, @" ^ ((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):[1-9][0-9]{1,4}:.+?:.+?$");
        }

        public delegate void DownloadFileCompletedDel(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
        public event DownloadFileCompletedDel DownloadFileCompleted;

        public delegate void DownloadProgressChangedDel(object sender, DownloadProgressChangedEventArgs e);
        public event DownloadProgressChangedDel DownloadProgressChanged;

        public delegate void DownloadDataCompletedDel(object sender, DownloadDataCompletedEventArgs e);
        public event DownloadDataCompletedDel DownloadDataCompleted;

        private WebClient WebClient { get; set; }

        public MobileDownload()
        {
            WebClient = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback += (sender1, certificate, chain, sslPolicyErrors) => true;
        }

        public static bool GetDebugFile(string userName, string password, string ip, string destinationFile)
        {
            IPAddress ipAdd;
            try
            {
                ipAdd = IPAddress.Parse(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(destinationFile);
            string filePath = System.IO.Path.GetDirectoryName(destinationFile);

            fileName = $"{fileName}_{DateTime.Now.ToString("MMddyy_HHmmss")}.zip";
            string file = System.IO.Path.Combine(filePath, fileName);

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Credentials = new NetworkCredential(userName, password);
                    wc.DownloadFile("https://" + ip.ToString() + "/cgi-bin/debugInfo.cgi", file);

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

        }
        public bool StartGetDebugFile(string userName, string password, string ip, string destinationFile)
        {
            IPAddress ipAdd;
            try
            {
                ipAdd = IPAddress.Parse(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            try
            {
                WebClient.Credentials = new NetworkCredential(userName, password);

                WebClient.DownloadDataCompleted += WebClient_DownloadDataCompleted;
                WebClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                WebClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;

                WebClient.DownloadFileAsync(new Uri("https://" + ip.ToString() + "/cgi-bin/debugInfo.cgi"), destinationFile);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {


            DownloadFileCompleted?.Invoke(sender, e);
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {


            DownloadProgressChanged?.Invoke(sender, e);
        }

        private void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {


            DownloadDataCompleted?.Invoke(sender, e);
        }

        public void Dispose()
        {
            WebClient?.Dispose();
        }
    }
}
