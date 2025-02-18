using AcademyApp.AppData;
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

namespace AcademyApp.Pages
{
    /// <summary>
    /// Interaction logic for StudentsPg.xaml
    /// </summary>
    public partial class StudentsPg : Page
    {
        public StudentsPg()
        {
            InitializeComponent();

            List<Student> studens = AppConnect.modelOdb.Students.ToList();

            if (studens.Count > 0)
            {
                tbCounter.Text = "Найдено " + studens.Count + " студентов";
            }
            else
            {
                tbCounter.Text = "Не найдено";
            }
            studentsList.ItemsSource = studens;
        }
    }
}
