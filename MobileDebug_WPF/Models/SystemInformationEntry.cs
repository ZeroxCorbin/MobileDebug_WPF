using System;
using System.Collections.Generic;
using System.Text;

namespace MobileDebug_WPF.Models
{
    public class SystemInformationHeader
    {
        public bool IsExpanded { get; set; }
        public string Name { get; set; }
        public List<SystemInformationHeader> SystemInformationEntries { get; set; }  = new List<SystemInformationHeader>();
    }
}
