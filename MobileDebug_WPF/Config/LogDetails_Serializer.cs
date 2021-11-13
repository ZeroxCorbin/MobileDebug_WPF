using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MobileDebug_WPF.Config
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LogDetails
    {

        private LogDetailsLog[] logField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Log")]
        public LogDetailsLog[] Log { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class LogDetailsLog
    {
        /// <remarks/>
        public bool isEM { get; set; }

        /// <remarks/>
        public bool isLD { get; set; }

        /// <remarks/>
        public string DisplayName { get; set; }

        /// <remarks/>
        public string FileName { get; set; }

        /// <remarks/>
        public string FilePath { get; set; }
        /// <remarks/>
        public string FileType { get; set; }

        /// <remarks/>
        public bool MultiLog { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Search")]
        public LogDetailsLogSearch[] Search { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class LogDetailsLogSearch
    {
        /// <remarks/>
        public bool isEM { get; set; }

        /// <remarks/>
        public bool isLD { get; set; }

        /// <remarks/>
        public string DisplayName { get; set; }

        /// <remarks/>
        public string RegEx2Match { get; set; }

        /// <remarks/>
        public string Level { get; set; }
    }
    public class LogDetails_Serializer
    {
        public static LogDetails Load(string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LogDetails));
            using (StreamReader sr = new StreamReader(file))
            {
                return (LogDetails)serializer.Deserialize(sr);
            }
        }

        public static void Save(string file, LogDetails logDetails)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LogDetails));
            using (StreamWriter sw = new StreamWriter(file))
            {
                serializer.Serialize(sw, logDetails);
            }
        }


    }
}
