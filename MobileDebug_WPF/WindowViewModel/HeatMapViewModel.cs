using MobileDebug_WPF.Models;
using MobileLogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MobileDebug_WPF.WindowViewModel
{
    public class HeatMapViewModel : Core.ViewModelBase
    {
        public bool IsLoading
        {
            get { return _IsLoading; }
            set { Set(ref _IsLoading, value); }
        }
        private bool _IsLoading;

        public bool IsVisible
        {
            get { return _IsVisible; }
            set { Set(ref _IsVisible, value); }
        }
        private bool _IsVisible;

        public bool IsDarkTheme
        {
            get { return _IsDarkTheme; }
            set { Set(ref _IsDarkTheme, value); }
        }
        private bool _IsDarkTheme;

        public BitmapImage HeatMapImage
        {
            get { return _HeatMapImage; }
            set { Set(ref _HeatMapImage, value); }
        }
        private BitmapImage _HeatMapImage = new BitmapImage();


        private MobileMap.MapFile _MapFile ;

        public HeatMapViewModel()
        {
            SaveCommand = new Core.RelayCommand(SaveCallback);
            EditMapCommand = new Core.RelayCommand(EditMapCallback);
        }

        public ICommand SaveCommand { get; }
        private void SaveCallback(object parameter)
        {

        }
        public ICommand EditMapCommand { get; }
        private void EditMapCallback(object parameter)
        {

        }

        public void Load(Dictionary<string, List<WifiLogData>> wifi)
        {
            IsVisible = true;
            IsLoading = true;

            //HeatMapImage.Source = null;
            //_MapFile.Map.Destroy();

            string file= GetMapFile();
            if(file == null)
            {
                IsLoading = false;
                return;
            }

            _MapFile = new MobileMap.MapFile(file, true);
            if (!_MapFile.HasContents)
            {
               IsLoading = false;
                return;
            }

            if (!_MapFile.LoadMapFromContent(true))
            {
               IsLoading = false;
                return;
            }

            _MapFile.Map.LoggedWifi = wifi;
            _MapFile.Map.MakeMultiThreaded();

            string bmp = MobileMap.MapUtils.GetBitmapString(_MapFile.Map, 4096, 4096, IsDarkTheme);

            Application.Current.Dispatcher.Invoke(new Action(() => { HeatMapImage = HeatMap.Base64StringToBitmap(bmp); }));
            

            IsLoading = false;
        }
        public void Reset()
        {
            IsVisible = false;
            IsLoading = false;

            Application.Current.Dispatcher.Invoke(new Action(() => { HeatMapImage = null; }));
            //HeatMapImage.Source = null;
            if (_MapFile != null)
                if (_MapFile.Map != null)
                    _MapFile.Map.Destroy();
        }

        public void ThemeChanged(bool dark)
        {
            IsDarkTheme = dark;

            if(_MapFile != null)
            {
                if(_MapFile.Map != null)
                {
                    string bmp = MobileMap.MapUtils.GetBitmapString(_MapFile.Map, 4096, 4096, IsDarkTheme);

                    Application.Current.Dispatcher.Invoke(new Action(() => { HeatMapImage = HeatMap.Base64StringToBitmap(bmp); }));
                }
            }
        }

        private string GetMapFile()
        {
            try
            {
                string file = null;
                string name = null;
                string path = Path.Join(App.WorkingDirectory, @"\usr\local\aramConfig\aramConfig.txt");
                if(!File.Exists(path))
                    path = Path.Join(App.WorkingDirectory, @"\usr\local\aramConfig\centralConfig.txt");

                foreach (string line in System.IO.File.ReadLines(path))
                {
                    if (line.StartsWith("Map ", StringComparison.Ordinal))
                    {
                        name = line.Replace("Map ", "");
                        int loc = name.IndexOf(".map");
                        name = name.Substring(0, loc+4);
                        break;
                    }
                }

                DirectoryInfo dir = new DirectoryInfo(Path.Join(App.WorkingDirectory, @"\home\admin\"));

                IEnumerable<FileInfo> names = dir.GetFiles().OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime);

                foreach (FileInfo fi in names)
                    if (fi.Extension.Equals(".map", StringComparison.Ordinal))
                    {
                        file = fi.Name.Equals(name, StringComparison.Ordinal) ? fi.FullName : null;
                        break;
                    }

                return file;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
