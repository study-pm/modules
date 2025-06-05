using HR.Data.Models;
using HR.Services;
using HR.Utilities;
using PdfSharp.Xps.XpsModel;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
using System.Windows.Xps.Packaging;
using PdfSharp.Xps;
using System.Diagnostics;

namespace HR.Pages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        internal User dm;
        public bool IsChanged => dm.Is2faOn != Is2FA;
        public bool IsEnabled => IsChanged && !IsInProgress;
        private bool _isInProgress;
        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                if (_isInProgress == value) return;
                _isInProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private bool _is2fa;
        public bool Is2FA
        {
            get => _is2fa;
            set
            {
                if (_is2fa == value) return;
                _is2fa = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsChanged));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public SettingsViewModel()
        {
            Is2FA = false;
        }
        public SettingsViewModel(User dataModel)
        {
            dm = dataModel;
            Is2FA = dm.Is2faOn;
        }
        public void Reset()
        {
            Is2FA = dm.Is2faOn;
        }
        public void Set()
        {
            dm.Is2faOn = Is2FA;
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
    /// <summary>
    /// Interaction logic for SettingsPg.xaml
    /// </summary>
    public partial class SettingsPg : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        private int uid = ((App)(Application.Current)).CurrentUser.Id;
        private const double MinColumnWidth = 350;
        private byte[] secret;
        private SettingsViewModel _vm;
        public SettingsViewModel vm
        {
            get => _vm;
            set
            {
                _vm = value;
                OnPropertyChanged();
            }
        }
        public SettingsPg()
        {
            InitializeComponent();
            this.SizeChanged += SettingsPg_SizeChanged;
            Loaded += SettingsPg_Loaded;
        }
        private void AdjustGridLayout(double availableWidth)
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            if (availableWidth >= MinColumnWidth * 2)
            {
                // Две колонки, одна строка
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetRow(FirstSection, 0);
                Grid.SetColumn(FirstSection, 0);

                Grid.SetRow(SecondSection, 0);
                Grid.SetColumn(SecondSection, 1);
            }
            else
            {
                // Одна колонка, две строки
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetRow(FirstSection, 0);
                Grid.SetColumn(FirstSection, 0);

                Grid.SetRow(SecondSection, 1);
                Grid.SetColumn(SecondSection, 0);
            }
        }
        private void SettingsPg_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustGridLayout(this.ActualWidth);
        }

        private void SettingsPg_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustGridLayout(e.NewSize.Width);
        }
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            vm.Reset();
        }
        private FlowDocument CloneFlowDocument(FlowDocument original)
        {
            if (original == null) return null;

            // Сериализуем в XAML строку
            string xaml = System.Windows.Markup.XamlWriter.Save(original);

            // Загружаем из XAML обратно — создаём копию
            using (var stringReader = new System.IO.StringReader(xaml))
            using (var xmlReader = System.Xml.XmlReader.Create(stringReader))
            {
                return (FlowDocument)System.Windows.Markup.XamlReader.Load(xmlReader);
            }
        }
        private void ExportToPdf(FlowDocument doc, string fileName)
        {
            try
            {
                // Открываем диалог сохранения PDF
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileName,
                    DefaultExt = ".pdf",
                    Filter = "Документы PDF (.pdf)|*.pdf"
                };

                if (dlg.ShowDialog() == true)
                {
                    // Путь к файлу
                    string pdfFilePath = dlg.FileName;
                    // Создаем временный файл для XPS
                    string tempXpsFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + ".xps");

                    // Сохраняем FlowDocument из RichTextBox в XPS
                    // Создаем XPS-документ через WPF
                    using (Package package = Package.Open(tempXpsFile, FileMode.Create))
                    {
                        System.Windows.Xps.Packaging.XpsDocument xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(package);
                        // Создаем XpsDocumentWriter через статический метод XpsDocument.CreateXpsDocumentWriter
                        var xpsWriter = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                        // Пагинатор документа
                        var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                        xpsWriter.Write(paginator);
                        xpsDoc.Close();
                    }

                    // Конвертируем XPS в PDF с помощью PDFSharp.Xps
                    PdfSharp.Xps.XpsConverter.Convert(tempXpsFile, pdfFilePath, 0);

                    // Удаляем временный XPS файл
                    File.Delete(tempXpsFile);

                    MessageBox.Show("Экспорт в PDF выполнен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте в PDF:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SetQrCode(byte[] qrCodeBytes)
        {
            imgQrCode.Source = Fs.LoadImage(qrCodeBytes);
            imgQrCode.Visibility = Visibility.Visible;
        }
        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            // vm.Set();
        }
        private void GetQrBtn_Click(object sender, RoutedEventArgs e)
        {
            // Generate secret
            secret = Crypto.GenerateSecret();
            // Get and display QR code in Image
            SetQrCode(Utils.GetQrCode(secret, "root"));
        }
        FlowDocument CreateReceiptDocument()
        {
            var doc = new FlowDocument();

            doc.PagePadding = new Thickness(20);
            doc.ColumnWidth = double.PositiveInfinity; // чтобы не разбивался на колонки

            // Заголовок
            var header = new Paragraph(new Bold(new Run("КАССОВЫЙ ЧЕК")));
            header.FontSize = 24;
            header.TextAlignment = TextAlignment.Center;
            doc.Blocks.Add(header);

            // Дата и время
            var dateParagraph = new Paragraph(new Run($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}"));
            dateParagraph.FontSize = 14;
            dateParagraph.TextAlignment = TextAlignment.Right;
            doc.Blocks.Add(dateParagraph);

            // Таблица с товарами
            var table = new Table();
            table.CellSpacing = 0;
            table.Columns.Add(new TableColumn() { Width = new GridLength(200) });
            table.Columns.Add(new TableColumn() { Width = new GridLength(60) });
            table.Columns.Add(new TableColumn() { Width = new GridLength(80) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            // Заголовок таблицы
            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Товар")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Кол-во")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Цена")))));
            rowGroup.Rows.Add(headerRow);

            // Пример строки товара
            var itemRow = new TableRow();
            itemRow.Cells.Add(new TableCell(new Paragraph(new Run("Молоко 1л"))));
            itemRow.Cells.Add(new TableCell(new Paragraph(new Run("2"))));
            itemRow.Cells.Add(new TableCell(new Paragraph(new Run("120 ₽"))));
            rowGroup.Rows.Add(itemRow);

            // Итого
            var totalParagraph = new Paragraph(new Bold(new Run("Итого: 240 ₽")));
            totalParagraph.FontSize = 16;
            totalParagraph.TextAlignment = TextAlignment.Right;
            doc.Blocks.Add(table);
            doc.Blocks.Add(totalParagraph);

            return doc;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            //ExportToPdf(CloneFlowDocument(ManualRtb.Document), "2FA_manual");
            var receiptDoc = CreateReceiptDocument();
            ExportToPdf(receiptDoc, "чек");
        }
    }
}
