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
