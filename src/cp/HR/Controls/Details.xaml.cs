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
    /// Interaction logic for Details.xaml
    /// </summary>
    public partial class Details : UserControl
    {
        public static readonly DependencyProperty TypeProp =
        DependencyProperty.Register("Type", typeof(string), typeof(Details));

        public string Type
        {
            get { return (string)GetValue(TypeProp); }
            set { SetValue(TypeProp, value); }
        }

        public bool IsAsterisk => Type == "Asterisk";
        public bool IsQuestion => Type == "Question";

        public Details()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
