using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MobileDebug_WPF.Models
{
    public class TableOfContentsEntry
    {
        public string Name { get; set; }
        public ICommand ClickCommand { get; set; }
        public string Path { get; set; }
    }
}
