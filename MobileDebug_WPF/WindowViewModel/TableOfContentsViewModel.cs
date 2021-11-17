using MobileDebug_WPF.Core;
using MobileDebug_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace MobileDebug_WPF.WindowViewModel
{
    public class TableOfContentsViewModel : Core.ViewModelBase
    {

        public bool IsExpanded
        {
            get => App.Settings.GetValue("ExpanderTableOfContents", false);
            set
            {
                App.Settings.SetValue("ExpanderTableOfContents", value);
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return _IsLoading; }
            set { Set(ref _IsLoading, value); }
        }
        private bool _IsLoading;

        private object ContentsLock = new object();
        public ObservableCollection<TableOfContentsEntry> Contents { get; } = new ObservableCollection<TableOfContentsEntry>();

        public TableOfContentsViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(Contents, ContentsLock);
        }

        public void Load()
        {
            IsLoading = true;

            ReadTOC();

            IsLoading = false;
        }

        public void Reset()
        {
            IsLoading = false;
            Contents.Clear();
        }

        private void ReadTOC()
        {
            using StreamReader file = new StreamReader(App.WorkingDirectory + "toc.txt");

            file.ReadLine();
            file.ReadLine();
            file.ReadLine();


            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] row = new string[3];

                int i = line.LastIndexOf(' ');
                row[0] = line.Substring(i + 1);

                if (row[0].ToString().EndsWith("/")) continue;
                if (row[0].ToString().StartsWith("-")) break;

                row[1] = line.Substring(i - 18, 10);
                row[2] = line.Substring(i - 7, 5);

                TableOfContentsEntry toc = new TableOfContentsEntry()
                {
                    ClickCommand = new RelayCommand(ClickCallback, c => true),
                    Name = row[0],
                    Path = Path.Join(Path.Join(App.WorkingDirectory, row[0])),
                };

                Contents.Add(toc);
            }
        }

        private void ClickCallback(object parameter)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(((TableOfContentsEntry)parameter).Path)
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }//Updated


}


