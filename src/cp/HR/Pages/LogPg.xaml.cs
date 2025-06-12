using HR.Services;
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
using static HR.Services.AppEventHelper;

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for LogPg.xaml
    /// </summary>
    public partial class LogPg : Page
    {
        public LogPg()
        {
            InitializeComponent();
            AppEventHelper.AppEvent +=  (sender, args) =>
            {
                MessageBox.Show($"App evt: {args.Message}: {args.Details}");
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppEventArgs evt = new AppEventArgs
            {
                Category = EventCategory.Service,
                Type = EventType.Warning,
                Message = "Тест генерации",
                Details = "Детали генерации"
            };
            AppEventHelper.RaiseAppEvent(evt);
        }
    }
}
