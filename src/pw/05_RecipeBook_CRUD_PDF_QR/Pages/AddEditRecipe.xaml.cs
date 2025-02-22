using Microsoft.Win32;
using RecipeBook.AppData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace RecipeBook.Pages
{
    /// <summary>
    /// Interaction logic for AddEditRecipe.xaml
    /// </summary>
    public partial class AddEditRecipe : Page, INotifyPropertyChanged
    {
        private Recipe _currentRecipe;
        private string _initialDescription;
        private string _initialRecipeName;
        private int _initialCategoryIndex;
        private string _initialCookingTime;
        private string _initialUrl;
        private string _initialImage;

        public AddEditRecipe(Recipe currentRec)
        {
            InitializeComponent();
            if (currentRec == null)
            {
                this.Title = "Добавление рецепта";
                _currentRecipe = new Recipe();
            }
            else
            {
                this.Title = "Редактирование рецепта";
                _currentRecipe = AppConnect.modelOdb.Recipes.Where(x => x.RecipeID == 1).ToList().First();
            }

            DataContext = _currentRecipe;
            var category = AppConnect.modelOdb.Categories;
            foreach (var item in category) cmbCat.Items.Add(item.CategoryName);
            if (currentRec != null && _currentRecipe.CategoryID.HasValue)
            {
                _initialCategoryIndex = (int)_currentRecipe.CategoryID;
                // Adjust index (subtract 1) because we're not adding an empty string
                cmbCat.SelectedIndex = _initialCategoryIndex - 1; // Subtract 1
            }
            else
            {
                // If CategoryID is null or Recipe is null, default to no selection
                _initialCategoryIndex = -1;
                cmbCat.SelectedIndex = -1; // No selection
            }

            // Store initial values
            _initialDescription = _currentRecipe.Description;
            _initialRecipeName = _currentRecipe.RecipeName;
            _initialImage = _currentRecipe.Image;
            _initialCookingTime = _currentRecipe?.CookingTime.ToString();
            _initialUrl = _currentRecipe.URL;

            if (Validity.checkFilled(_initialUrl)) imgBtn.Content = "Изменить изображение"; // Add image text local binding instead
        }
        private bool checkFormValidity()
        {
            if (!Validity.checkFilled(nameTxb.Text))
            {
                MessageBox.Show("Поле 'Название' должно быть заполнено", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Validity.checkFilled(timeTbx.Text) && !Validity.checkWhole(timeTbx.Text))
            {
                MessageBox.Show("Время приготовления должно составлять целое число минут", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            // Add image text local binding instead
            imgBtn.Content = Validity.checkFilled(_currentRecipe.Image) ? "Изменить" : "Добавить" + " изображение";
            return true;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isChanged = false; // Backing field for isChanged

        public bool isChanged
        {
            get { return _isChanged; }
            set
            {
                if (_isChanged != value)
                {
                    _isChanged = value;
                    OnPropertyChanged(nameof(isChanged)); // Notify the UI
                }
            }
        }
        private void handleChangedState()
        {
            isChanged = false;
            if (nameTxb.Text != _currentRecipe.RecipeName
                && !(!Validity.checkFilled(nameTxb.Text) && !Validity.checkFilled(_currentRecipe.RecipeName))) isChanged = true;
            if (descriptionTbx.Text != _currentRecipe.Description
                && !(!Validity.checkFilled(descriptionTbx.Text) && !Validity.checkFilled(_currentRecipe.Description))) isChanged = true;
            if (urlTbx.Text != _currentRecipe.URL
                && !(!Validity.checkFilled(urlTbx.Text) && !Validity.checkFilled(_currentRecipe.URL))) isChanged = true;
            if (cmbCat.SelectedIndex != -1 && cmbCat.SelectedIndex != _initialCategoryIndex -1) isChanged = true;
            if (_initialImage != _currentRecipe.Image
                && !(!Validity.checkFilled(_initialImage) && !Validity.checkFilled(_currentRecipe.Image))) isChanged = true;

            // if (timeTbx.Text != _initialCookingTime) isChanged = true;
        }
        private void cmbCat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ctl = sender as ComboBox;
            handleChangedState();
        }
        public void txbName_TextChanged (object sender, EventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            bool isValid = Validity.checkFilled(ctl.Text);
            Validity.setValidityState(ctl, isValid);
            handleChangedState();
        }
        public void descriptionTbx_TextChanged(object sender, EventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            handleChangedState();
        }
        public void timeTbx_TextChanged(object sender, EventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            bool isValid = false;
            if (Validity.checkFilled(ctl.Text)) isValid = Validity.checkWhole(ctl.Text);
            else isValid = true;
            if (isValid) handleChangedState();
            Validity.setValidityState(ctl, isValid);

        }
        private void urlTbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ctl = (TextBox)sender;
            bool isValid = false;
            if (Validity.checkFilled(ctl.Text)) isValid = Validity.checkUrl(ctl.Text);
            else isValid = true;
            if (isValid) handleChangedState();
            Validity.setValidityState(ctl, isValid);
        }
        private void timeTbx_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((TextBox)sender);
        }
        private void urlTbx_LostFocus(object sender, RoutedEventArgs e)
        {
            Validity.setValidityState((TextBox)sender);
        }

        private BitmapImage ChooseImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png";
            BitmapImage loaded = null;
            if (openFileDialog.ShowDialog() == true)
            {
                loaded = new BitmapImage(new System.Uri(openFileDialog.FileName));
            }
            return loaded;
        }
        private string SaveImage(BitmapImage loadedImage)
        {
            // Specify the path to save the image
            string projectRoot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            string fullPath = System.IO.Path.GetFullPath(projectRoot);
            string imagesPath = System.IO.Path.Combine(fullPath, "Images");
            Directory.CreateDirectory(imagesPath); // Create Images directory if it doesn't exist
            string fileName = $"recipe_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png";
            string filePath = System.IO.Path.Combine(imagesPath, fileName);

            // Save the image
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder(); // Use desired encoder (e.g., PngBitmapEncoder)
                encoder.Frames.Add(BitmapFrame.Create(loadedImage));
                encoder.Save(stream);
            }

            return fileName;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!checkFormValidity()) return;
            try
            {
                if (_currentRecipe.RecipeID == 0)
                {
                    _currentRecipe.AuthorID = User.AuthorId;
                    AppConnect.modelOdb.Recipes.Add(_currentRecipe);
                }
                AppConnect.modelOdb.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                handleChangedState();
            }
            catch (Exception exc) {
                MessageBox.Show($"Ошибка при сохранения: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void imgBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapImage loaded = ChooseImage();
                if (loaded == null) return;
                recipeImg.Source = loaded;
                _currentRecipe.Image = SaveImage(loaded);
                handleChangedState();
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка при загрузке: {exc.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentRecipe.Description = _initialDescription;
                _currentRecipe.RecipeName = _initialRecipeName;
                _currentRecipe.URL = _initialUrl;
                cmbCat.SelectedIndex = _initialCategoryIndex != -1 ? _initialCategoryIndex - 1 : -1;                
                if (Validity.checkFilled(_initialCookingTime)) _currentRecipe.CookingTime = Convert.ToInt32(_initialCookingTime);
                else _currentRecipe.CookingTime = null;

                if (_initialImage != _currentRecipe.Image)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    string projectRoot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
                    string fullPath = System.IO.Path.GetFullPath(projectRoot);
                    string targetPath = System.IO.Path.Combine(fullPath, Validity.checkFilled(_initialImage) ? "Images" : "Resources");
                    string filePath = System.IO.Path.Combine(targetPath, Validity.checkFilled(_initialImage) ? _initialImage : "no_image.jpg");
                    bitmap.UriSource = new Uri(filePath, UriKind.Absolute); // Use UriKind.Relative if the path is relative
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load the image immediately
                    bitmap.EndInit();
                    bitmap.Freeze(); // Freeze to make it cross-thread accessible if necessary
                    recipeImg.Source = bitmap; // Assigning the BitmapImage to ImageSource

                    // Delete the file
                    string imagesPath = System.IO.Path.Combine(fullPath, "Images");
                    string curImgPath = System.IO.Path.Combine(imagesPath, _currentRecipe.Image);
                    if (File.Exists(curImgPath)) File.Delete(curImgPath);
                    _initialImage = _currentRecipe.Image;
                }

                // Refresh bindings
                recipeImg.GetBindingExpression(Image.SourceProperty)?.UpdateTarget();
                descriptionTbx.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                nameTxb.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                timeTbx.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                urlTbx.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                handleChangedState();
            }
            catch (Exception exc) {
                MessageBox.Show(exc.Message);
            }
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.GoBack();
        }


        // If there are changes, ask user for saving them
    }
}
