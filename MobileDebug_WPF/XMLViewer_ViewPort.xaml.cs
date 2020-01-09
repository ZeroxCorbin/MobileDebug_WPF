using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace MobileDebug_WPF
{
    /// <summary>
    /// Interaction logic for XMLViewer_ViewPort.xaml
    /// </summary>
    public partial class XMLViewer_ViewPort : UserControl
    {
        private XmlDocument _xmldocument;

        public XMLViewer_ViewPort()
        {
            InitializeComponent();
        }

        public XmlDocument xmlDocument
        {
            get { return _xmldocument; }
            set
            {
                _xmldocument = value;
                BindXMLDocument();
            }
        }

        private void BindXMLDocument()
        {
            if (_xmldocument == null)
            {
                xmlTree.ItemsSource = null;
                return;
            }

            XmlDataProvider provider = new XmlDataProvider
            {
                Document = _xmldocument
            };
            Binding binding = new Binding
            {
                Source = provider,
                XPath = "child::node()"
            };
            xmlTree.SetBinding(TreeView.ItemsSourceProperty, binding);
        }
    }
}
