using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MobileDebug_WPF
{
    public class SystemMap_Serializer
    {
        public static SystemMap Load(string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SystemMap));
            using (StreamReader sr = new StreamReader(file))
            {
                return (SystemMap)serializer.Deserialize(sr);
            }
        }

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class SystemMap
        {

            private SystemMapSection[] sectionField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Section")]
            public SystemMapSection[] Section
            {
                get
                {
                    return this.sectionField;
                }
                set
                {
                    this.sectionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SystemMapSection
        {

            private string labelField;

            private string searchTextField;

            private string highlightColorField;

            private bool subSectionField;

            /// <remarks/>
            public string Label
            {
                get
                {
                    return this.labelField;
                }
                set
                {
                    this.labelField = value;
                }
            }

            /// <remarks/>
            public string SearchText
            {
                get
                {
                    return this.searchTextField;
                }
                set
                {
                    this.searchTextField = value;
                }
            }

            /// <remarks/>
            public string HighlightColor
            {
                get
                {
                    return this.highlightColorField;
                }
                set
                {
                    this.highlightColorField = value;
                }
            }

            /// <remarks/>
            public bool SubSection
            {
                get
                {
                    return this.subSectionField;
                }
                set
                {
                    this.subSectionField = value;
                }
            }

            private int FirstLineField = -1;
            public int FirstLine
            {
                get
                {
                    return this.FirstLineField;
                }
                set
                {
                    this.FirstLineField = value;
                }
            }
        }


    }
}
