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

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty HeaderTextProp =
        DependencyProperty.Register("HeaderText", typeof(string), typeof(Header));

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProp); }
            set { SetValue(HeaderTextProp, value); }
        }
    }
}
