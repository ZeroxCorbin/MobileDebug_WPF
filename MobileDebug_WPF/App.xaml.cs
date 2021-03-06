using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Serialization;

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
             


            if(!Directory.Exists(UserDataDirectory))
            {
                Console.WriteLine($"Creating directory: {UserDataDirectory}");
                Directory.CreateDirectory(UserDataDirectory);
            }
            FileStream filestream = new FileStream(UserDataDirectory + "log.txt", FileMode.Append);
            var streamwriter = new StreamWriter(filestream)
            {
                AutoFlush = true
            };
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);
            Settings = new SimpleDataBase().Init($"{UserDataDirectory}ApplicationSettings.sqlite", false);
            if(Settings == null)
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
