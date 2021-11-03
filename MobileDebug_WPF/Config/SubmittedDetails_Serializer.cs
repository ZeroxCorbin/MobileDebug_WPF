using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MobileDebug_WPF
{
    public class SubmittedDetails_Serializer
    {
        public static SubmittedDetails Load(string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SubmittedDetails));
            using (StreamReader sr = new StreamReader(file))
            {
                return (SubmittedDetails)serializer.Deserialize(sr);
            }
        }

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class SubmittedDetails
        {

            private string titleField;

            private string keywordField;

            /// <remarks/>
            public string Title
            {
                get
                {
                    return this.titleField;
                }
                set
                {
                    this.titleField = value;
                }
            }

            /// <remarks/>
            public string Keyword
            {
                get
                {
                    return this.keywordField;
                }
                set
                {
                    this.keywordField = value;
                }
            }
        }


    }
}
