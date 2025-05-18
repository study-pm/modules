using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using HR.Data.Models;

namespace HR.Services
{
    public static class DataGridCommandsHelper
    {
        /// <summary>
        /// Копирует выделенные строки DataGrid в буфер обмена в табличном формате с заголовками.
        /// </summary>
        public static void CopySelectedRowsToClipboard(DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.SelectedItems == null || dataGrid.SelectedItems.Count == 0)
                return;

            var selectedItems = dataGrid.SelectedItems.Cast<object>().ToList();

            var sb = new StringBuilder();

            // Заголовки видимых колонок
            var headers = dataGrid.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .Select(c => c.Header?.ToString())
                .ToArray();

            sb.AppendLine(string.Join("\t", headers));

            // Данные по каждой выделенной строке
            foreach (var item in selectedItems)
            {
                var row = dataGrid.Columns
                    .Where(c => c.Visibility == Visibility.Visible)
                    .Select(c =>
                    {
                        if (c is DataGridBoundColumn boundColumn)
                        {
                            var binding = boundColumn.Binding as System.Windows.Data.Binding;
                            if (binding == null) return string.Empty;

                            var propName = binding.Path.Path;
                            var prop = item.GetType().GetProperty(propName);
                            var val = prop?.GetValue(item);
                            return val?.ToString() ?? string.Empty;
                        }
                        return string.Empty;
                    })
                    .ToArray();

                sb.AppendLine(string.Join("\t", row));
            }

            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// Печатает DataGrid с масштабированием под размер страницы.
        /// </summary>
        public static void PrintDataGrid(DataGrid dataGrid, string description = "Печать таблицы")
        {
            if (dataGrid == null)
                return;

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var originalTransform = dataGrid.LayoutTransform;

            Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

            dataGrid.Measure(pageSize);
            dataGrid.Arrange(new Rect(0, 0, pageSize.Width, pageSize.Height));

            double scale = Math.Min(pageSize.Width / dataGrid.ActualWidth, 1.0);
            dataGrid.LayoutTransform = new ScaleTransform(scale, scale);

            printDialog.PrintVisual(dataGrid, description);

            dataGrid.LayoutTransform = originalTransform;
        }
    }

    public static class ItemsControlHelper
    {
        /// <summary>
        /// Копирует список сотрудников в буфер обмена в табличном виде.
        /// </summary>
        public static void CopyStaffToClipboard(IEnumerable<Employee> staff)
        {
            if (staff == null) return;

            var sb = new StringBuilder();

            // Заголовки
            sb.AppendLine("Фамилия\tИмя\tОтчество\tДолжность\tСтаж работы\tПодразделение");

            foreach (var emp in staff)
            {
                sb.AppendLine($"{emp.Surname}\t{emp.GivenName}\t{emp.Patronymic}\t{emp.Experience}");
            }

            Clipboard.SetText(sb.ToString());
        }
        public static void PrintStaff(IEnumerable<Employee> staff, string description = "Печать сотрудников")
        {
            if (staff == null) return;

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var panel = new StackPanel { Margin = new Thickness(20) };

            var header = new TextBlock
            {
                Text = "Список сотрудников",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(header);

            var headerGrid = new Grid();
            for (int i = 0; i < 6; i++)
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition());

            headerGrid.Children.Add(CreateTextBlock("Фамилия", 0, true));
            headerGrid.Children.Add(CreateTextBlock("Имя", 1, true));
            headerGrid.Children.Add(CreateTextBlock("Отчество", 2, true));
            headerGrid.Children.Add(CreateTextBlock("Должность", 3, true));
            headerGrid.Children.Add(CreateTextBlock("Стаж работы", 4, true));
            headerGrid.Children.Add(CreateTextBlock("Подразделение", 5, true));

            panel.Children.Add(headerGrid);

            foreach (var emp in staff)
            {
                var rowGrid = new Grid();
                for (int i = 0; i < 6; i++)
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition());

                rowGrid.Children.Add(CreateTextBlock(emp.Surname, 0));
                rowGrid.Children.Add(CreateTextBlock(emp.GivenName, 1));
                rowGrid.Children.Add(CreateTextBlock(emp.Patronymic, 2));
                //rowGrid.Children.Add(CreateTextBlock(emp.Position, 3));
                rowGrid.Children.Add(CreateTextBlock(emp.Experience.ToString(), 4));
                //rowGrid.Children.Add(CreateTextBlock(emp.Department, 5));

                panel.Children.Add(rowGrid);
            }

            Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            panel.Measure(pageSize);
            panel.Arrange(new Rect(0, 0, pageSize.Width, pageSize.Height));

            printDialog.PrintVisual(panel, description);
        }

        private static TextBlock CreateTextBlock(string text, int column, bool isHeader = false)
        {
            var tb = new TextBlock
            {
                Text = text,
                Margin = new Thickness(5, 2, 5, 2),
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(tb, column);
            return tb;
        }
    }
}
