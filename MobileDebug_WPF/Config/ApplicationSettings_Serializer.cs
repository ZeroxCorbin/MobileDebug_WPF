using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;


namespace MobileDebug_WPF
{
    public class ApplicationSettings_Serializer
    {
        public static App Load(string file)
        {
            StreamReader sr;
            App app;
            XmlSerializer serializer = new XmlSerializer(typeof(App));
            try
            {
                sr = new StreamReader(file);
            }catch(FileNotFoundException)
            {
                ApplicationSettings_Serializer.Save(file, new App());
                sr = new StreamReader(file);
            }

            app = (App)serializer.Deserialize(sr);
            sr.Close();
            return app;
        }
        public static void Save(string file, App app)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(App));
            using (StreamWriter sw = new StreamWriter(file))
            {
                serializer.Serialize(sw, app);
            }
        }

        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class App
        {

            //private AppFrmMain frmMainField = new AppFrmMain();

            //private AppFrmRTF frmRTFField = new AppFrmRTF();

            private AppFrmDownload frmFrmDownloadField = new AppFrmDownload();

            private AppFrmUpload frmFrmUploadField = new AppFrmUpload();

            private List<string> historyField;

            /// <remarks/>
            //public AppFrmMain frmMain
            //{
            //    get
            //    {
            //        return this.frmMainField;
            //    }
            //    set
            //    {
            //        this.frmMainField = value;
            //    }
            //}

            //public AppFrmRTF frmRTF
            //{
            //    get
            //    {
            //        return this.frmRTFField;
            //    }
            //    set
            //    {
            //        this.frmRTFField = value;
            //    }
            //}

            public AppFrmDownload frmDownload
            {
                get
                {
                    return this.frmFrmDownloadField;
                }
                set
                {
                    this.frmFrmDownloadField = value;
                }
            }

            public AppFrmUpload frmUpload
            {
                get
                {
                    return this.frmFrmUploadField;
                }
                set
                {
                    this.frmFrmUploadField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("History")]
            public List<string> History
            {
                get
                {
                    return this.historyField;
                }
                set
                {
                    this.historyField = value;
                }
            }

        }

        /// <remarks/>
        //[System.SerializableAttribute()]
        //[System.ComponentModel.DesignerCategoryAttribute("code")]
        //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        //public partial class AppFrmMain
        //{

        //    private FormWindowState WindowStateField = FormWindowState.Normal;

        //    private Size SizeField = new Size(1024, 768);

        //    private Point LocationField = new Point(10, 10);

        //    /// <remarks/>
        //    public FormWindowState WindowState
        //    {
        //        get
        //        {
        //            return this.WindowStateField;
        //        }
        //        set
        //        {
        //            this.WindowStateField = value;
        //        }
        //    }

        //    /// <remarks/>
        //    public Size Size
        //    {
        //        get
        //        {
        //            return this.SizeField;
        //        }
        //        set
        //        {
        //            this.SizeField = value;
        //        }
        //    }

        //    /// <remarks/>
        //    public Point Location
        //    {
        //        get
        //        {
        //            return this.LocationField;
        //        }
        //        set
        //        {
        //            this.LocationField = value;
        //        }
        //    }
        //}

        //[System.SerializableAttribute()]
        //[System.ComponentModel.DesignerCategoryAttribute("code")]
        //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        //public partial class AppFrmRTF
        //{

        //    private FormWindowState WindowStateField = FormWindowState.Normal;

        //    private Size SizeField = new Size(1024, 768);

        //    private Point LocationField = new Point(10, 10);

        //    /// <remarks/>
        //    public FormWindowState WindowState
        //    {
        //        get
        //        {
        //            return this.WindowStateField;
        //        }
        //        set
        //        {
        //            this.WindowStateField = value;
        //        }
        //    }

        //    /// <remarks/>
        //    public Size Size
        //    {
        //        get
        //        {
        //            return this.SizeField;
        //        }
        //        set
        //        {
        //            this.SizeField = value;
        //        }
        //    }

        //    /// <remarks/>
        //    public Point Location
        //    {
        //        get
        //        {
        //            return this.LocationField;
        //        }
        //        set
        //        {
        //            this.LocationField = value;
        //        }
        //    }
        //}

        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class AppFrmDownload
        {

            private string IPAddyField = "1.2.3.4";

            private string UserNameField = "admin";

            private string FileNamePrefixField = "";

            /// <remarks/>
            public string IPAddy
            {
                get
                {
                    return this.IPAddyField;
                }
                set
                {
                    this.IPAddyField = value;
                }
            }

            /// <remarks/>
            public string UserName
            {
                get
                {
                    return this.UserNameField;
                }
                set
                {
                    this.UserNameField = value;
                }
            }
            /// <remarks/>
            public string FileNamePrefix
            {
                get
                {
                    return this.FileNamePrefixField;
                }
                set
                {
                    this.FileNamePrefixField = value;
                }
            }

        }

        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class AppFrmUpload
        {

            private string SP_ServerField = "https://omronam.sharepoint.com/sites/OEIMobileRobotTeam";

            private string SP_UserNameField = "";

            private string SP_PasswordField = "";

            private string SP_ServerRelativeURIField = "/sites/OEIMobileRobotTeam/Shared Documents/Temporary";

            //private FormWindowState WindowStateField = FormWindowState.Normal;

            //private Size SizeField = new Size(1024, 768);

            //private Point LocationField = new Point(10, 10);

            ///// <remarks/>
            //public FormWindowState WindowState
            //{
            //    get
            //    {
            //        return this.WindowStateField;
            //    }
            //    set
            //    {
            //        this.WindowStateField = value;
            //    }
            //}

            ///// <remarks/>
            //public Size Size
            //{
            //    get
            //    {
            //        return this.SizeField;
            //    }
            //    set
            //    {
            //        this.SizeField = value;
            //    }
            //}

            ///// <remarks/>
            //public Point Location
            //{
            //    get
            //    {
            //        return this.LocationField;
            //    }
            //    set
            //    {
            //        this.LocationField = value;
            //    }
            //}

            /// <remarks/>
            public string SP_Server
            {
                get
                {
                    return this.SP_ServerField;
                }
                set
                {
                    this.SP_ServerField = value;
                }
            }

            /// <remarks/>
            public string SP_UserName
            {
                get
                {
                    return this.SP_UserNameField;
                }
                set
                {
                    this.SP_UserNameField = value;
                }
            }
            /// <remarks/>
            public string SP_Password
            {
                get
                {
                    return this.SP_PasswordField;
                }
                set
                {
                    this.SP_PasswordField = value;
                }
            }

            /// <remarks/>
            public string SP_ServerRelativeURI
            {
                get
                {
                    return this.SP_ServerRelativeURIField;
                }
                set
                {
                    this.SP_ServerRelativeURIField = value;
                }
            }
        }

    }
}
