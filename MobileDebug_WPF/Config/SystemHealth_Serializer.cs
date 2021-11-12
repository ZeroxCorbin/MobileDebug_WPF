using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MobileDebug_WPF
{
    public class SystemHealth_Serializer
    {

        public static SystemHealth Load(string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SystemHealth));
            using (StreamReader sr = new StreamReader(file))
            {
                return (SystemHealth)serializer.Deserialize(sr);
            }
        }

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class SystemHealth
        {
            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Heading")]
            public SystemHealthHeading[] Heading { get; set; }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SystemHealthHeading
        {
            /// <remarks/>
            public string Name { get; set; }

            /// <remarks/>
            public bool isEM { get; set; }

            /// <remarks/>
            public bool isLD { get; set; }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Label")]
            public SystemHealthHeadingLabel[] Label { get; set; }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SystemHealthHeadingLabel
        {
            /// <remarks/>
            public string Name { get; set; }

            /// <remarks/>
            public string FilePath { get; set; }

            /// <remarks/>
            public string Description { get; set; }

            /// <remarks/>
            public string Tail { get; set; }

            /// <remarks/>
            public decimal Threshold_ld { get; set; }

            /// <remarks/>
            public decimal Threshold_em { get; set; }

            /// <remarks/>
            public bool Greater { get; set; }

            /// <remarks/>
            public decimal Multiplier { get; set; }

            /// <remarks/>
            public bool isEM { get; set; }

            /// <remarks/>
            public bool isLD { get; set; }
        }


    }
}
