using System;
using System.IO;
using System.Text;
using System.Windows;

namespace MobileDebug_WPF
{
    public partial class App : Application
    {
        public class ConsoleWriterEventArgs : EventArgs
        {
            public string Value { get; private set; }
            public ConsoleWriterEventArgs(string value)
            {
                Value = value;
            }
        }

        public class ConsoleWriter : TextWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }

            public override void Write(string value)
            {
                WriteEvent?.Invoke(this, new ConsoleWriterEventArgs(value));
                base.Write(value);
            }

            public override void WriteLine(string value)
            {
                WriteLineEvent?.Invoke(this, new ConsoleWriterEventArgs(value));
                base.WriteLine(value);
            }

            public event EventHandler<ConsoleWriterEventArgs> WriteEvent;
            public event EventHandler<ConsoleWriterEventArgs> WriteLineEvent;
        }

        public static SimpleDataBase Settings;
        public const string UserDataDirectory = "UserData";
#if !DEBUG
        public static TextWriter ConsoleOut;
#endif
        //MainWindow MainWindow;
        protected override void OnStartup(StartupEventArgs e)
        {
            //DxMobileMap.Resources.AddResourceItem.Extract();
            base.OnStartup(e);
        }
        public App()
        {
            //var consoleWriter = new ConsoleWriter();
            //consoleWriter.WriteEvent += ConsoleWriter_WriteEvent; ;
            //consoleWriter.WriteLineEvent += ConsoleWriter_WriteLineEvent; ;

            //Console.SetOut(consoleWriter);

#if DEBUG
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
#else
            ConsoleOut = new StreamWriter("log.txt");
            Console.SetOut(ConsoleOut);
#endif
            if (!Directory.Exists(UserDataDirectory))
            {
                Console.WriteLine($"Creating directory: {UserDataDirectory}");
                Directory.CreateDirectory(UserDataDirectory);
            }
            Settings = new SimpleDataBase().Init($"{UserDataDirectory}\\ApplicationSettings.sqlite", false);
            if (Settings == null)
            {
                Console.WriteLine($"Could not initialize the application settings database: {UserDataDirectory}\\ApplicationSettings.sqlite");
                throw new Exception();
            }
            else
            {
                Console.WriteLine("Application settings loaded.");
            }
        }

        private void ConsoleWriter_WriteLineEvent(object sender, ConsoleWriterEventArgs e)
        {
            //this.Dispatcher.Invoke(new Action(delegate
            //{
            //    if (MainWindow != null)
            //    {
            //        if (MainWindow.IsActive)
            //        {
            //            ((MainWindow)MainWindow).UpdateMapLoad(e.Value);
            //        }
            //    }
            //}
            //));

        }

        private void ConsoleWriter_WriteEvent(object sender, ConsoleWriterEventArgs e)
        {
            if (MainWindow != null)
            {
                if (MainWindow.IsActive)
                {
                    //((MainWindow)MainWindow).UpdateMapLoad(e.Value);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("Exiting.");

            base.OnExit(e);

            Settings.Dispose();
#if !DEBUG
            ConsoleOut.Close();
#endif
        }
    }
}
