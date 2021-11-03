using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MobileDebug_WPF
{
    public class SystemDetails_Serializer
    {
        public static SystemDetails Load(string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SystemDetails));
            using (StreamReader sr = new StreamReader(file))
            {
                return (SystemDetails)serializer.Deserialize(sr);
            }
        }

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class SystemDetails
        {

            private SystemDetailsHeading[] headingField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Heading")]
            public SystemDetailsHeading[] Heading
            {
                get
                {
                    return this.headingField;
                }
                set
                {
                    this.headingField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SystemDetailsHeading
        {

            private string nameField;

            private bool isEMField;

            private bool isLDField;

            private SystemDetailsHeadingLabel[] labelField;

            /// <remarks/>
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

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
            [System.Xml.Serialization.XmlElementAttribute("Label")]
            public SystemDetailsHeadingLabel[] Label
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
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SystemDetailsHeadingLabel
        {

            private string nameField;

            private string filePathField;

            private string descriptionField;

            private bool isEMField;

            private bool isLDField;

            /// <remarks/>
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
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
            public string Description
            {
                get
                {
                    return this.descriptionField;
                }
                set
                {
                    this.descriptionField = value;
                }
            }

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
        }


    }
}
