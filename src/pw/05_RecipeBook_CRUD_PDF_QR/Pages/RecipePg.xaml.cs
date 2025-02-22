using Aspose.BarCode.Generation;
using RecipeBook.AppData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
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
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using ZXing;
using ZXing.QrCode;

namespace RecipeBook.Pages
{
    /// <summary>
    /// Interaction logic for RecipePg.xaml
    /// </summary>
    public partial class RecipePg : Page
    {
        Recipe recipe;

        public RecipePg(Recipe currentRecipe, Author currentAuthor)
        {
            InitializeComponent();
            recipe = currentRecipe;

            // Set DataContext to the recipe object
            this.DataContext = recipe;

            if (Validity.checkFilled(recipe.URL)) GenerateQrCode(recipe.URL);

            // Fill in tags
            var tags = AppConnect.modelOdb.RecipeTags
                .Where(x => x.RecipeID == recipe.RecipeID)
                .Include(r => r.Tag)
                .ToList();

            foreach (var tag in tags) txblTags.Text += tag.Tag.TagName;

            // Fill in ingredients
            ingredientsList.ItemsSource = AppConnect.modelOdb.RecipeIngredients
                .Where(x => x.RecipeID == recipe.RecipeID)
                .Include(r => r.Ingredient)
                .ToList();

            // Fill in steps
            stepsList.ItemsSource = AppConnect.modelOdb.CookingSteps.Where(x => x.RecipeID == recipe.RecipeID).ToList();

            // Fill in reviews
            var reviews = AppConnect.modelOdb.Reviews.Where(x => x.RecipeID == recipe.RecipeID);
            reviewsList.ItemsSource = reviews.ToList();

            int rateSum = 0;
            foreach (var r in reviews) rateSum += r.Rating;

            if (reviews.Count() != 0)
            {
                double rate = (double)rateSum / reviews.Count();
                txblRate.Text = rate.ToString();
            }

            if (recipe.AuthorID == currentAuthor.AuthorID)
            {
                btnDelete.Visibility = Visibility.Visible;
                btnEdit.Visibility = Visibility.Visible;
            }
        }

        private void GenerateQrCode(string url)
        {
            // Configure the QR code writer
            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = 200,
                    Height = 200,
                    Margin = 0
                }
            };

            // Generate the barcode
            var pixelData = barcodeWriter.Write(url);

            if (pixelData != null)
            {
                // Create a WriteableBitmap from the pixel data
                var bitmap = new WriteableBitmap(pixelData.Width, pixelData.Height, 96, 96, PixelFormats.Bgra32, null);

                // Write the pixel data to the WriteableBitmap
                bitmap.WritePixels(
                    new Int32Rect(0, 0, pixelData.Width, pixelData.Height),
                    pixelData.Pixels,
                    pixelData.Width * 4, // Use Width * 4 as RowBytes (4 bytes per pixel)
                    0);

                // Set the Image control's source
                BarcodeImage.Source = bitmap;
            }
            else
            {
                MessageBox.Show("Failed to generate QR code.");
            }
        }
        public static void CreatePdf(string filePath)
        {
            /*
            // Create a new PDF document
            PdfDocument pdfDocument = new PdfDocument(new PdfWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write)));

            // Create a document object
            Document doc = new Document(pdfDocument);

            // Add a paragraph to the document
            iText.Layout.Element.Paragraph line = new iText.Layout.Element.Paragraph("Hello! Welcome to iTextSharp.");
            doc.Add(line);

            // Close the document
            doc.Close();

            Console.WriteLine("PDF created successfully!");
            */
        }
        private void CreateInMemoryPDF ()
        {
            string executingDirectory = AppContext.BaseDirectory;

            // Set the page size of our PDF
            PageSize pageSize = new PageSize(612, 792);

            // masterStream holds the running-pdf document as
            // more and more HTML is added to it
            using (MemoryStream masterStream = new MemoryStream())
            {
                string page1 = $"<h1>{recipe.RecipeName}</h1>" +
                    $"<p>{recipe.Description}</p><h2>Продукты</h2>";
                string page2 = $"<h2>Отзывы</h2>";

                // Add HTML to our PDF
                AppendHTML(masterStream, page1, pageSize);
                AppendHTML(masterStream, page2, pageSize);

                // Resolve path
                string projectRoot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
                string fullPath = System.IO.Path.GetFullPath(projectRoot);
                // string targetPath = System.IO.Path.Combine(fullPath, Validity.checkFilled(_initialImage) ? "Images" : "Resources");
                string docsDir = System.IO.Path.Combine(fullPath, "Resources", "Docs");
                // Create a new directory for our output PDF
                if (!Directory.Exists(docsDir)) Directory.CreateDirectory(docsDir);

                // Save our in-memory PDF as a PDF on our file system
                var filePath = $"{docsDir}\\recipe_{recipe.RecipeID}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.pdf";
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    masterStream.WriteTo(fileStream);

                MessageBox.Show($"Рецепт успешно сформирован и находится в файле {filePath}", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Converts <paramref name="html"/> into a PDF and appends it onto the existing in-memory PDF which is being stored in <paramref name="masterStream"/>.
        /// </summary>
        /// <param name="masterStream"></param>
        /// <param name="html"></param>
        /// <param name="pageSize"></param>
        static void AppendHTML(MemoryStream masterStream, string html, PageSize pageSize)
        {
            // Create the in-memory PDF of html
            using (MemoryStream componentStream = new MemoryStream())
            {
                // This memory stream is used by the pdfWriter below
                using (MemoryStream tempStream = new MemoryStream())
                {
                    using (PdfWriter pdfWriter = new PdfWriter(tempStream))
                    {
                        // Since creating a reader/writer in iText causes the underlying
                        // stream to close, we need to prevent that with this call
                        pdfWriter.SetCloseStream(false);

                        using (PdfDocument document = new PdfDocument(pdfWriter))
                        {
                            document.SetDefaultPageSize(pageSize);

                            HtmlConverter.ConvertToPdf(html, document, new ConverterProperties());

                            tempStream.WriteTo(componentStream);
                        }
                    }
                }

                // Append new component to PDF
                if (masterStream.Length == 0)
                    componentStream.WriteTo(masterStream);
                else
                {
                    // Append to existing PDF in-memory
                    using (MemoryStream tempStream = new MemoryStream())
                    {
                        // Since previous updates may move the position of the stream,
                        // set the position to 0 so the PDF can be read in full
                        masterStream.Position = 0;
                        componentStream.Position = 0;
                        using (PdfDocument combinedDocument = new PdfDocument(new PdfReader(masterStream), new PdfWriter(tempStream)))
                        {
                            // Read the new HTML content into a PDF document
                            using (PdfDocument componentDocument = new PdfDocument(new PdfReader(componentStream)))
                            {
                                combinedDocument.SetCloseWriter(false);

                                // Copies the new PDF onto the existing PDF (masterStream)
                                componentDocument.CopyPagesTo(1, componentDocument.GetNumberOfPages(), combinedDocument);
                            }

                            combinedDocument.Close();
                        }

                        // Reads all bytes from tempStream which at this point holds all
                        // existing PDF data, and any new PDF data we just appended to it.
                        // All of this data is then saved back into masterStream
                        byte[] temporaryBytes = tempStream.ToArray();
                        masterStream.Position = 0;
                        masterStream.SetLength(temporaryBytes.Length);
                        masterStream.Capacity = temporaryBytes.Length;
                        masterStream.Write(temporaryBytes, 0, temporaryBytes.Length);
                    }
                }
            }
        }


        private void backBtn_Click(object sender, EventArgs e)
        {
            AppFrame.frameMain.GoBack();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // CreatePdf("sample.pdf");
                CreateInMemoryPDF();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new AddEditRecipe(recipe));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить данный рецепт?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    AppConnect.modelOdb.Recipes.Remove(recipe);
                    AppConnect.modelOdb.SaveChanges();
                    MessageBox.Show("Данные успешно удалены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppFrame.frameMain.GoBack();
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"Ошибка при удалении: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
