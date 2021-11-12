using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MobileDebug_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SimpleDataBase Settings;

#if DEBUG
        public static string RootDirectory { get; set; } = Path.Join(System.IO.Directory.GetCurrentDirectory(), "\\42Nexus\\MobileDebug_WPF\\");
#else        
        public static string RootDirectory { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "\\42Nexus\\MobileDebug_WPF\\");
#endif
        public static string WorkingDirectory => Path.Join(RootDirectory, "Working\\");
        public static string UserDataDirectory => Path.Join(RootDirectory , "UserData\\");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!Directory.Exists(UserDataDirectory))
            {
                Console.WriteLine($"Creating directory: {UserDataDirectory}");
                Directory.CreateDirectory(UserDataDirectory);
            }

            FileStream filestream = new FileStream(Path.Join(UserDataDirectory, "\\log.txt"), FileMode.Append);
            var streamwriter = new StreamWriter(filestream)
            {
                AutoFlush = true
            };
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            Settings = new SimpleDataBase().Init(Path.Join(UserDataDirectory, "\\ApplicationSettings.sqlite"), false);
            if (Settings == null)
            {
                Console.WriteLine($"Could not initialize the application settings database: {Path.Join(UserDataDirectory, "\\ApplicationSettings.sqlite")}");
                throw new Exception();
            }
            else
            {
                Console.WriteLine("Application settings loaded.");
            }
            
            ThemeManager.Current.ChangeTheme(this, Settings.GetValue("App.Theme", "Light.Steel"));
        }
        public App()
        {
            //GetCommandData cmd = new GetCommandData();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("Exiting.");

            base.OnExit(e);

            Settings.Dispose();
        }
    }
}
