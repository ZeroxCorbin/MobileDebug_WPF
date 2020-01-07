using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
//using System.Windows.Forms;

namespace MobileDebug_WPF
{
    public class SimpleDataBase : IDisposable 

    {
        public class SimpleSetting
        {
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }
        public class FormSettings
        {
            public System.Drawing.Size Size { get; set; } = new System.Drawing.Size(1280, 720);
            public System.Drawing.Point Location { get; set; } = new System.Drawing.Point(0, 0);
            public System.Windows.Forms.FormWindowState State { get; set; } = System.Windows.Forms.FormWindowState.Normal;

            [Newtonsoft.Json.JsonIgnore]
            public bool IsLoading { get; set; } = true;

            public FormSettings() { }

            public void Update(System.Drawing.Size size, System.Windows.Forms.FormWindowState state)
            {
                if (state != System.Windows.Forms.FormWindowState.Minimized) State = state;
                if (state != System.Windows.Forms.FormWindowState.Normal) return;

                Size = size;
                return;
            }
            public void Update(System.Drawing.Point location, System.Windows.Forms.FormWindowState state)
            {
                if (state != System.Windows.Forms.FormWindowState.Minimized) State = state;
                if (state != System.Windows.Forms.FormWindowState.Normal) return;

                Location = location;
                return;
            }
            public bool IsOnScreen()
            {

                System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
                foreach (System.Windows.Forms.Screen screen in screens)
                {
                    System.Drawing.Rectangle formRectangle = new System.Drawing.Rectangle(Location.X, Location.Y,
                                                             Size.Width, Size.Height);

                    if (screen.WorkingArea.Contains(formRectangle))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public class WindowSettings
        {
            public double Top { get; set; } = 0;
            public double Left { get; set; } = 0;
            public double Width { get; set; }
            public double Height { get; set; }

            public System.Windows.Rect Bounds => new System.Windows.Rect(Left, Top, Width, Height);

            public System.Windows.WindowState State { get; set; } = System.Windows.WindowState.Normal;

            [Newtonsoft.Json.JsonIgnore]
            public bool IsLoading { get; set; } = true;

            public WindowSettings() { }

            public void Update(double top, double left, double width, double height, System.Windows.WindowState state)
            {
                if (state != System.Windows.WindowState.Minimized) State = state;
                if (state != System.Windows.WindowState.Normal) return;

                Top = top;
                Left = left;
                Width = width;
                Height = height;
            }

            public bool IsOnScreen()
            {

                if (FindAppropriateScreen(Top, Left, Width, Height) != null)
                    return true;
                else
                    return false;
            }

            public static WpfScreenHelper.Screen FindAppropriateScreen(double top, double left, double width, double height)
            {
                var windowRight = left + width;
                var windowBottom = top + height;

                var allScreens = WpfScreenHelper.Screen.AllScreens.ToList();

                // If the window is inside all of a single screen boundaries, maximize to that
                var screenInsideAllBounds = allScreens.Find(x => top >= x.Bounds.Top
                                                            && left >= x.Bounds.Left
                                                            && windowRight <= x.Bounds.Right
                                                            && windowBottom <= x.Bounds.Bottom);
                if (screenInsideAllBounds != null)
                {
                    return screenInsideAllBounds;
                }

                // Failing the above (between two screens in side-by-side configuration)
                // Measure if the window is between the top and bottom of any screens.
                // Then measure the percentage it is within each screen and pick a winner
                var screensInBounds = allScreens.FindAll(x => top >= x.Bounds.Top
                                      && windowBottom <= x.Bounds.Bottom);
                if (screensInBounds.Count > 0)
                {
                    var values = new List<Tuple<double, WpfScreenHelper.Screen>>();
                    // Determine the amount of width inside each screen
                    foreach (var screen in screensInBounds.OrderBy(x => x.Bounds.Left))
                    {
                        // This has only been tested in a two screen, side-by-side setup.
                        double amountInScreen;
                        if (screen.Bounds.Left == 0)
                        {
                            var rightOfWindow = left + width;
                            var outsideRightBoundary = rightOfWindow - screen.Bounds.Right;
                            amountInScreen = width - outsideRightBoundary;
                            values.Add(new Tuple<double, WpfScreenHelper.Screen>(amountInScreen, screen));
                        }
                        else
                        {
                            var outsideLeftBoundary = screen.Bounds.Left - left;
                            amountInScreen = width - outsideLeftBoundary;
                            values.Add(new Tuple<double, WpfScreenHelper.Screen>(amountInScreen, screen));
                        }
                    }

                    values = values.OrderByDescending(x => x.Item1).ToList();
                    if (values.Count > 0)
                    {
                        return values[0].Item2;
                    }
                }

                // Failing all else
                return null;
            }
        }


        private SQLiteConnection Connection = null;

        public string DbFilePath { get; private set; } = null;
        public string DbTableName { get; private set; } = null;

        private string CREATE_TABLE(string name) => $"CREATE TABLE IF NOT EXISTS '{name}' (id integer PRIMARY KEY AUTOINCREMENT," +
                "Key TEXT NOT NULL UNIQUE," +
                "Value DATA NULL);";

        public SimpleDataBase Init(string dbFilePath, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(dbFilePath)) return null;
            string dbTableName = Path.GetFileNameWithoutExtension(dbFilePath);

            return Init(dbFilePath, dbTableName, overwrite);
        }
        public SimpleDataBase Init(string dbFilePath, string dbTableName, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(dbFilePath)) return null;
            if (string.IsNullOrEmpty(dbTableName)) return null;

            try
            {
                CreateDatabaseFile(dbFilePath, overwrite);
                DbFilePath = dbFilePath;
                CreateTable(dbTableName);
                DbTableName = dbTableName;

                return this;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public T GetValue<T>(string key)
        {
            SimpleSetting settings = SelectSetting(key);
            if (settings.Value == string.Empty) return default;
            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(settings.Value, typeof(T));
        }
        public T GetValue<T>(string key, T defaultValue)
        {
            SimpleSetting settings = SelectSetting(key);
            if (settings.Value == string.Empty)
                return defaultValue;
            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(settings.Value, typeof(T));
        }
        public string GetValue(string key)
        {
            SimpleSetting settings = SelectSetting(key);
            if (settings.Value == string.Empty) return string.Empty;
            return settings.Value;
        }

        public void SetValue<T>(string key, T value)
        {
            SimpleSetting set = new SimpleSetting()
            {
                Key = key,
                Value = Newtonsoft.Json.JsonConvert.SerializeObject(value)
            };
            UpdateSetting(set);
        }
        public void SetValue(string key, string value)
        {
            SimpleSetting set = new SimpleSetting()
            {
                Key = key,
                Value = value
            };
            UpdateSetting(set);
        }

        public SimpleSetting GetSetting(string key)
        {
            return SelectSetting(key);
        }
        public void SetSetting(SimpleSetting setting)
        {
            UpdateSetting(setting);
        }


        private void CreateDatabaseFile(string dbFilePath, bool overwrite)
        {
            if (overwrite && File.Exists(dbFilePath))
            {
                Console.WriteLine($"Deleting map database file: {dbFilePath}");
                File.Delete(dbFilePath);
            }
                

            if (!File.Exists(dbFilePath))
            {
                Console.WriteLine($"Creating map database file: {dbFilePath}");
                SQLiteConnection.CreateFile(dbFilePath);
            }
                
        }
        private bool OpenConnection()
        {
            if (Connection == null) Connection = new SQLiteConnection($"Data Source='{DbFilePath}'; Version=3;");
            if (Connection.State == System.Data.ConnectionState.Closed) Connection.Open();
            if (Connection.State != System.Data.ConnectionState.Open) return false;
            else return true;
        }

        public bool ExistsTable(string tableName = null)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = DbTableName;
            if (!OpenConnection()) return false;
            using (SQLiteCommand command = new SQLiteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';", Connection))
            using (SQLiteDataReader rdr = command.ExecuteReader())
                if (rdr.Read())
                {
                    if (rdr.FieldCount == 0)
                        return false;
                    else
                        return true;
                }
                else
                {
                    return false;
                }
        }
        public void CreateTable(string tableName)
        {
            if (!OpenConnection()) return;
            using (SQLiteCommand command = new SQLiteCommand(CREATE_TABLE(tableName), Connection))
                command.ExecuteNonQuery();
        }

        public bool ExistsSetting(string key, string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = DbTableName;
            if (!OpenConnection()) return false;
            using (SQLiteCommand command = new SQLiteCommand($"SELECT count(*) FROM '{tableName}' WHERE Key = '{key}';", Connection))
            {
                    int count = Convert.ToInt32(command.ExecuteScalar());
                if (count == 0)
                    return false;
                else
                    return true;
            }

        }
        private int InsertSetting(SimpleSetting setting, string tableName = null)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = DbTableName;
            if (!OpenConnection()) return -1;
            StringBuilder sb = new StringBuilder();
            sb.Append($"INSERT OR IGNORE INTO '{tableName}' (Key, Value) VALUES (");
            sb.Append($"@Key,");
            sb.Append($"@Value);");

            using (SQLiteCommand command = new SQLiteCommand(sb.ToString(), Connection))
            {
                command.Parameters.AddWithValue("Key", setting.Key);
                command.Parameters.AddWithValue("Value", setting.Value);
                return command.ExecuteNonQuery();
            }
        }
        private int UpdateSetting(SimpleSetting setting, string tableName = null)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = DbTableName;
            if (!OpenConnection()) return -1;
            StringBuilder sb = new StringBuilder();
            sb.Append($"UPDATE '{tableName}' SET ");
            sb.Append($"Key = @Key,");
            sb.Append($"Value = @Value ");
            sb.Append($"WHERE Key = @Key;");

            int res = -1;
            using (SQLiteCommand command = new SQLiteCommand(sb.ToString(), Connection))
            {
                command.Parameters.AddWithValue("Key", setting.Key);
                command.Parameters.AddWithValue("Value", setting.Value);
                res = command.ExecuteNonQuery();
            }

            if(res == 0)
                res = InsertSetting(setting);

            return res;
        }
        public SimpleSetting SelectSetting(string key, string tableName = null)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = DbTableName;
            SimpleSetting settings = new SimpleSetting();
            if (!OpenConnection()) return settings;

            using (SQLiteCommand command = new SQLiteCommand($"SELECT * FROM '{tableName}' WHERE Key = '{key}';", Connection))
            using (SQLiteDataReader rdr = command.ExecuteReader())
                if (rdr.Read())
                {
                    settings.Key = rdr.GetString(1);
                    settings.Value = rdr.GetValue(2).ToString();
                }
            return settings;
        }
        public int DeleteSetting(string key, string tableName = null)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = DbTableName;
            if (!OpenConnection()) return -1;

            using (SQLiteCommand command = new SQLiteCommand($"DELETE FROM '{tableName}' WHERE Key = '{key}';", Connection))
                return command.ExecuteNonQuery();
        }

        public Dictionary<string, string> SelectAllSettingsRows()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (!OpenConnection()) return dict;

            using (SQLiteCommand command = new SQLiteCommand($"SELECT * FROM '{DbTableName}'", Connection))
            using (SQLiteDataReader rdr = command.ExecuteReader())
                while (rdr.Read())
                    dict.Add(rdr.GetString(1), rdr.GetValue(2).ToString());

            return dict;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                Connection?.Close();
                Connection?.Dispose();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Models() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
