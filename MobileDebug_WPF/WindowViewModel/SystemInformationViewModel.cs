using MobileDebug_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MobileDebug_WPF.WindowViewModel
{
    public class SystemInformationViewModel : Core.ViewModelBase
    {
        public bool IsExpanded
        {
            get => App.Settings.GetValue("ExpanderSystemInfo", false);
            set
            {
                App.Settings.SetValue("ExpanderSystemInfo", value);
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return _IsLoading; }
            set { Set(ref _IsLoading, value); }
        }
        private bool _IsLoading;

        private object SystemEntriesLock = new object();
        public ObservableCollection<SystemInformationHeader> SystemEntries { get; set; } = new ObservableCollection<SystemInformationHeader>();
        public bool IsEM { get; private set; }

        public SystemInformationViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(SystemEntries, SystemEntriesLock);
        }
        public void Load()
        {
            IsLoading = true;

            CheckProductType();
            LoadSystemHealth();
            LoadSystemDetails();
            LoadSystemApps();

            IsLoading = false;
        }

        public void Reset()
        {
            IsLoading = false;

            SystemEntries.Clear();
        }

        private void CheckProductType()
        {
            if (GetLineFromFile(App.WorkingDirectory + "mnt\\status\\platform\\productType").Contains("EM")) IsEM = true;
            else IsEM = false;
        }
        private void LoadSystemHealth()
        {
            SystemHealth_Serializer.SystemHealth serial = SystemHealth_Serializer.Load(Path.Join(App.UserDataDirectory, "Config\\", "SystemHealth.xml"));

            foreach (SystemHealth_Serializer.SystemHealthHeading head in serial.Heading)
            {
                if (IsEM && !head.isEM) continue;//If the data is from an EM and the config file indicates the data is not relevant to an EM, the section is ignored.
                if (!IsEM && !head.isLD) continue;

                SystemInformationHeader header = new SystemInformationHeader()
                {
                    Name = head.Name,
                    IsExpanded = true,
                };

                foreach (SystemHealth_Serializer.SystemHealthHeadingLabel label in head.Label)
                {
                    if (IsEM && !label.isEM) continue;
                    if (!IsEM && !label.isLD) continue;

                    string line = GetLineFromFile(App.WorkingDirectory + label.FilePath);
                    double res = 0;
                    if (!string.IsNullOrEmpty(line))
                        res = Convert.ToDouble(line) * Convert.ToDouble(label.Multiplier);

                    header.SystemInformationEntries.Add(new SystemInformationHeader()
                    {
                        Name = label.Name + res.ToString() + label.Tail,
                        IsExpanded = false,
                    });
                }

                SystemEntries.Add(header);
            }
        }//Updated
        private void LoadSystemDetails()
        {
            SystemDetails_Serializer.SystemDetails serial = SystemDetails_Serializer.Load(Path.Join(App.UserDataDirectory, "Config\\", "SystemDetails.xml"));

            foreach (SystemDetails_Serializer.SystemDetailsHeading head in serial.Heading)
            {
                if (IsEM && !head.isEM) continue;//If the data is from an EM and the config file indicates the data is not relevant to an EM, the section is ignored.
                if (!IsEM && !head.isLD) continue;

                SystemInformationHeader header = new SystemInformationHeader()
                {
                    Name = head.Name,
                    IsExpanded = true,
                };

                foreach (SystemDetails_Serializer.SystemDetailsHeadingLabel label in head.Label)
                {
                    if (IsEM && !label.isEM) continue;
                    if (!IsEM && !label.isLD) continue;

                    string line = GetLineFromFile(App.WorkingDirectory + label.FilePath);

                    if (!string.IsNullOrEmpty(line))
                        header.SystemInformationEntries.Add(new SystemInformationHeader()
                        {
                            Name = label.Name + line.Replace("\t", " , "),
                            IsExpanded = false,
                        });
                    else
                        header.SystemInformationEntries.Add(new SystemInformationHeader()
                        {
                            Name = "File Not Found!",
                            IsExpanded = false,
                        });
                }

                SystemEntries.Add(header);
            }
        }//Updated
        private void LoadSystemApps()
        {
            SystemApps_Serializer.SystemApps serial = SystemApps_Serializer.Load(Path.Join(App.UserDataDirectory, "Config\\", "SystemApps.xml"));

            DirectoryInfo di = new DirectoryInfo(App.WorkingDirectory + serial.Path);

            SystemInformationHeader header = new SystemInformationHeader()
            {
                Name = serial.Title,
                IsExpanded = true,
            };

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                foreach (FileInfo fi in dir.GetFiles())
                {
                    if (fi.Name == serial.FileName)
                    {
                        using (StreamReader file = new System.IO.StreamReader(fi.FullName))
                        {
                            string line = file.ReadLine();

                            SystemInformationHeader header1 = new SystemInformationHeader()
                            {
                                Name = line,
                                IsExpanded = false,
                            };
                            header.SystemInformationEntries.Add(header1);

                            //TextBox txtLabel = new TextBox() { Text = line, IsReadOnly = true, BorderThickness = new Thickness(0), Margin = new Thickness(3) };
                            //TreeViewItem tviLabel = new TreeViewItem()
                            //{
                            //    Header = txtLabel,
                            //};
                            //tviHeading.Items.Add(tviLabel);

                            while ((line = file.ReadLine()) != null)
                            {
                                SystemInformationHeader header2 = new SystemInformationHeader()
                                {
                                    Name = line,
                                    IsExpanded = false,
                                };
                                header1.SystemInformationEntries.Add(header2);
                                //TextBox txtLabel1 = new TextBox() { Text = line, IsReadOnly = true, BorderThickness = new Thickness(0), Margin = new Thickness(3) };
                                //TreeViewItem tviLabel1 = new TreeViewItem()
                                //{
                                //    Header = txtLabel1,
                                //};
                                //tviLabel.Items.Add(tviLabel1);
                            }
                        }
                    }
                }
            }

            SystemEntries.Add(header);
        }//Updated
        private string GetLineFromFile(string filePath)
        {
            try
            {
                using StreamReader file = new StreamReader(filePath);
                return file.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }

        }
    }
}
