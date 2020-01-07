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

            private SystemHealthHeading[] headingField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Heading")]
            public SystemHealthHeading[] Heading
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
        public partial class SystemHealthHeading
        {

            private string nameField;

            private bool isEMField;

            private bool isLDField;

            private SystemHealthHeadingLabel[] labelField;

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
            public SystemHealthHeadingLabel[] Label
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
        public partial class SystemHealthHeadingLabel
        {

            private string nameField;

            private string filePathField;

            private string descriptionField;

            private string tailField;

            private decimal threshold_ldField;

            private decimal threshold_emField;

            private bool greaterField;

            private decimal multiplierField;

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
            public string Tail
            {
                get
                {
                    return this.tailField;
                }
                set
                {
                    this.tailField = value;
                }
            }

            /// <remarks/>
            public decimal Threshold_ld
            {
                get
                {
                    return this.threshold_ldField;
                }
                set
                {
                    this.threshold_ldField = value;
                }
            }

            /// <remarks/>
            public decimal Threshold_em
            {
                get
                {
                    return this.threshold_emField;
                }
                set
                {
                    this.threshold_emField = value;
                }
            }

            /// <remarks/>
            public bool Greater
            {
                get
                {
                    return this.greaterField;
                }
                set
                {
                    this.greaterField = value;
                }
            }

            /// <remarks/>
            public decimal Multiplier
            {
                get
                {
                    return this.multiplierField;
                }
                set
                {
                    this.multiplierField = value;
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
