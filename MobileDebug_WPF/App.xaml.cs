using System;
using System.IO;
using System.Text;
using System.Windows;

namespace MobileDebug_WPF
{
    public partial class App : Application
    {
        public static SimpleDataBase Settings;
        public static string AppRootDirectory => System.AppDomain.CurrentDomain.BaseDirectory;
        public static string UserDataDirectory => System.AppDomain.CurrentDomain.BaseDirectory + "UserData\\";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            FileStream filestream = new FileStream(UserDataDirectory + "log.txt", FileMode.Append);
            var streamwriter = new StreamWriter(filestream)
            {
                AutoFlush = true
            };
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            if (!Directory.Exists(UserDataDirectory))
            {
                Console.WriteLine($"Creating directory: {UserDataDirectory}");
                Directory.CreateDirectory(UserDataDirectory);
            }
            Settings = new SimpleDataBase().Init($"{UserDataDirectory}ApplicationSettings.sqlite", false);
            if (Settings == null)
            {
                Console.WriteLine($"Could not initialize the application settings database: {UserDataDirectory}ApplicationSettings.sqlite");
                throw new Exception();
            }
            else
            {
                Console.WriteLine("Application settings loaded.");
            }
        }
        public App()
        {

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("Exiting.");

            base.OnExit(e);

            Settings.Dispose();
        }
    }
}
