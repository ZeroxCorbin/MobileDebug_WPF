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

namespace MobileDebug_WPF
{
    /// <summary>
    /// Interaction logic for UserMessage.xaml
    /// </summary>
    public partial class UserMessage : Window
    {
        public string Result { get; private set; } = string.Empty;
        public UserMessage(Window owner, string message, string title, List<string> buttons)
        {
            this.Owner = owner;
            this.SizeToContent = SizeToContent.WidthAndHeight;

            InitializeComponent();

            this.Title = title;

            TxtMessage.Text = message;

            foreach (string s in buttons)
            {
                Button but = new Button()
                {
                    Content = s,
                    Width = 75,
                    Height = 25,
                    Margin = new Thickness(10, 0, 10, 0)
                };
                but.Click += But_Click;
                stkButtons.Children.Add(but);
            }
                
        }

        private void But_Click(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            Result = (string)but.Content;
            this.Close();
        }

        public static string Show(Window owner, string message, string title, List<string> buttons)
        {
            UserMessage um = new UserMessage(owner, message, title, buttons);
            um.ShowDialog();
            return um.Result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Size size = new Size(this.ActualWidth + 10, this.ActualHeight + 20);

            this.SizeToContent = SizeToContent.Manual;

            this.Height = size.Height;
            this.Width = size.Width;

            this.Left = (this.Owner.Left + (this.Owner.ActualWidth / 2)) - (this.ActualWidth / 2);
            this.Top = (this.Owner.Top + (this.Owner.ActualHeight / 2)) - (this.ActualHeight / 2);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.Left = (this.Owner.Left + (this.Owner.ActualWidth / 2)) - (this.ActualWidth / 2);
            this.Top = (this.Owner.Top + (this.Owner.ActualHeight / 2)) - (this.ActualHeight / 2);
        }
    }
}
