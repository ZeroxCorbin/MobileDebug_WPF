using FileSearch;
using MobileDebug_WPF.Config;
using MobileLogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MobileDebug_WPF.Models
{
    public class BatteryViewerEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand ViewCommand { get; set; }
        public DateTime DateTime { get; set; }
        public List<string> ResultStrings { get => new List<string>() { $"Entries {Log.Results.Count}" }; }
        public List<BatteryLogData> SearchResults { get => Log.Results; }
        public BatteryLogs Log { get; set; }
    }
}
