using FileSearch;
using MobileDebug_WPF.Config;
using MobileLogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MobileDebug_WPF.Models
{
    public class WiFiViewerEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand ViewCommand { get; set; }
        public Dictionary<string, List<WifiLogData>> SearchResults { get => Log.Results; }
        public WifiLogs Log { get; set; }
    }
}
