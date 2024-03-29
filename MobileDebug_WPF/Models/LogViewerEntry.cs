﻿using FileSearch;
using MobileDebug_WPF.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MobileDebug_WPF.Models
{
    public class LogViewerEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand ViewCommand { get; set; }
        public DateTime DateTime { get; set; }
        public Dictionary<string, List<FileSearchResults>> SearchResults { get; set; } = new Dictionary<string, List<FileSearchResults>>();
        public LogDetailsLog Log { get; set; }
    }
}
