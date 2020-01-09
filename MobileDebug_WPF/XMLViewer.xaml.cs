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
using System.Windows.Shapes;
using System.Xml;

namespace MobileDebug_WPF
{
    /// <summary>
    /// Interaction logic for XMLViewer.xaml
    /// </summary>
    public partial class XMLViewer : Window
    {
        public XMLViewer()
        {
            InitializeComponent();
        }
        public XMLViewer(string filePath)
        {
            InitializeComponent();

            ViewXmlFile(filePath);
        }
        public void ViewXmlFile(string filePath)
        {
            XmlDocument XMLdoc = new XmlDocument();
            try
            {
                XMLdoc.Load(filePath);
            }
            catch (XmlException)
            {
                //MessageBox.Show("The XML file is invalid");
                return;
            }

            txtFilePath.Text = System.IO.Path.GetFileName(filePath);
            vXMLViwer.xmlDocument = XMLdoc;
        }

        private void BrowseXmlFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "XML Files (*.xml)|*.xml|All Files(*.*)|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) { return; }

            XmlDocument XMLdoc = new XmlDocument();
            try
            {
                XMLdoc.Load(dlg.FileName);
            }
            catch (XmlException)
            {
                MessageBox.Show("The XML file is invalid");
                return;
            }

            txtFilePath.Text = dlg.FileName;
            vXMLViwer.xmlDocument = XMLdoc;
        }

        private void ClearXmlFile(object sender, RoutedEventArgs e)
        {
            txtFilePath.Text = string.Empty;
            vXMLViwer.xmlDocument = null;
        }
    }
}
