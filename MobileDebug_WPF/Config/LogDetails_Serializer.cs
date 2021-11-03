using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MobileDebug_WPF
{
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
            public LogDetailsLog[] Log
            {
                get
                {
                    return this.logField;
                }
                set
                {
                    this.logField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class LogDetailsLog
        {

            private bool isEMField;

            private bool isLDField;

            private string displayNameField;

            private string fileNameField;

            private string filePathField;

            private string fileTypeField;

            private bool multiLogField;

            private LogDetailsLogSearch[] searchField;

            /// <remarks/>
            public bool isEM
            {
                get
                {
                    return this.isEMField;
                }
                set
                {
                    this.isEMField = value;
                }
            }

            /// <remarks/>
            public bool isLD
            {
                get
                {
                    return this.isLDField;
                }
                set
                {
                    this.isLDField = value;
                }
            }

            /// <remarks/>
            public string DisplayName
            {
                get
                {
                    return this.displayNameField;
                }
                set
                {
                    this.displayNameField = value;
                }
            }

            /// <remarks/>
            public string FileName
            {
                get
                {
                    return this.fileNameField;
                }
                set
                {
                    this.fileNameField = value;
                }
            }

            /// <remarks/>
            public string FilePath
            {
                get
                {
                    return this.filePathField;
                }
                set
                {
                    this.filePathField = value;
                }
            }

            /// <remarks/>
            public string FileType
            {
                get
                {
                    return this.fileTypeField;
                }
                set
                {
                    this.fileTypeField = value;
                }
            }

            /// <remarks/>
            public bool MultiLog
            {
                get
                {
                    return this.multiLogField;
                }
                set
                {
                    this.multiLogField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Search")]
            public LogDetailsLogSearch[] Search
            {
                get
                {
                    return this.searchField;
                }
                set
                {
                    this.searchField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class LogDetailsLogSearch
        {

            private bool isEMField;

            private bool isLDField;

            private string displayNameField;

            private string regEx2MatchField;

            private string levelField;

            /// <remarks/>
            public bool isEM
            {
                get
                {
                    return this.isEMField;
                }
                set
                {
                    this.isEMField = value;
                }
            }

            /// <remarks/>
            public bool isLD
            {
                get
                {
                    return this.isLDField;
                }
                set
                {
                    this.isLDField = value;
                }
            }

            /// <remarks/>
            public string DisplayName
            {
                get
                {
                    return this.displayNameField;
                }
                set
                {
                    this.displayNameField = value;
                }
            }

            /// <remarks/>
            public string RegEx2Match
            {
                get
                {
                    return this.regEx2MatchField;
                }
                set
                {
                    this.regEx2MatchField = value;
                }
            }

            /// <remarks/>
            public string Level
            {
                get
                {
                    return this.levelField;
                }
                set
                {
                    this.levelField = value;
                }
            }
        }
    }
}
