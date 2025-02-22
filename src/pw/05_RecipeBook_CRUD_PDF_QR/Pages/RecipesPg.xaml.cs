using RecipeBook.AppData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;   // For Include() method
using System.Linq;          // For ToList() method
using System.Runtime.Remoting.Messaging;
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
    /// Interaction logic for RecipesPg.xaml
    /// </summary>
    public partial class RecipesPg : Page
    {
        private Author author;
        public RecipesPg(Author currentAuthor)
        {
            InitializeComponent();

            // Fill in current author
            author = currentAuthor;

            // Fill in recipes
            recipesList.ItemsSource = AppConnect.modelOdb.Recipes
                .Include(r => r.Category)   // Eager load the Category data
                .Include(r => r.Author)   // Eager load the Author data
                .ToList();                  // Convert to list for binding
            cmbCat.Items.Add("");
            // cmbCat.SelectedIndex = 0;
            var category = AppConnect.modelOdb.Categories;
            foreach (var item in category) cmbCat.Items.Add(item.CategoryName);
        }

        Recipe[] RecipesList()
        {
            try
            {
                List<Recipe> recipes = AppConnect.modelOdb.Recipes.ToList();
                if (txbNameSearch != null)
                {
                    recipes = recipes.Where(x => x.RecipeName.ToLower().Contains(txbNameSearch.Text.ToLower())).ToList();
                }
                if (cmbCat.SelectedIndex > 0)
                {
                    switch (cmbCat.SelectedIndex)
                    {
                        case 1:
                            recipes = recipes.Where(x => x.CategoryID == 1).ToList();
                            break;
                        case 2:
                            recipes = recipes.Where(x => x.CategoryID == 2).ToList();
                            break;
                        case 3:
                            recipes = recipes.Where(x => x.CategoryID == 3).ToList();
                            break;
                        case 4:
                            recipes = recipes.Where(x => x.CategoryID == 4).ToList();
                            break;
                    }
                }
                if (chkOnlyMy.IsChecked == true) recipes = recipes.Where(x => x.AuthorID == author.AuthorID).ToList();

                txbCounter.Text = "Найдено: " + recipes.Count + " рецепт";

                int lastDigit = recipes.Count % 10;

                if (lastDigit == 2 || lastDigit == 3 || lastDigit == 4) txbCounter.Text += "а";
                else if (lastDigit == 1) txbCounter.Text += "";
                else txbCounter.Text += "ов";

                return recipes.ToArray();
            }
            catch
            {
                MessageBox.Show("Повторите попытку позже");
                return null;
            }

        }

        private void updateItems()
        {
            recipesList.ItemsSource = RecipesList();
        }
        private void chkOnlyMy_Checked(object sender, RoutedEventArgs e)
        {
            updateItems();
        }
        private void TextSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateItems();
        }
        private void btnSearchReset_Click(object sender, RoutedEventArgs e)
        {
            txbNameSearch.Text = "";
            updateItems();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            updateItems();
        }
        private ListSortDirection handleSortState(Button ctl)
        {
            string sortBy = ctl.Content.ToString();
            ListSortDirection direction = ListSortDirection.Ascending;

            // Determine sort direction
            if (ctl.Tag != null && ctl.Tag.ToString() == "▲")
            {
                direction = ListSortDirection.Descending;
                ctl.Tag = "▼";
            }
            else
            {
                ctl.Tag = "▲";
            }
            return direction;
        }

        private void btnSortRecipe_Click(object sender, RoutedEventArgs e)
        {
            SortByName(handleSortState(sender as Button));
        }

        private void btnSortTime_Click(object sender, RoutedEventArgs e)
        {
            SortByTime(handleSortState(sender as Button));
        }
        private void SortByName(ListSortDirection direction)
        {
            if (direction == ListSortDirection.Ascending)
            {
                recipesList.ItemsSource = RecipesList().OrderBy(c => c.RecipeName).ToArray();
            }
            else
            {
                recipesList.ItemsSource = RecipesList().OrderByDescending(c => c.RecipeName).ToArray();
            }
        }
        private void SortByTime(ListSortDirection direction)
        {
            if (direction == ListSortDirection.Ascending)
            {
                recipesList.ItemsSource = RecipesList().OrderBy(c => c.CookingTime).ToArray();
            }
            else
            {
                recipesList.ItemsSource = RecipesList().OrderByDescending(c => c.CookingTime).ToArray();
            }
        }

        private void cmbCat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ctl = sender as ComboBox;
            if (ctl.SelectedValue.ToString() == "") {
                txblCat.Visibility = Visibility.Visible;
            }
            else txblCat.Visibility = Visibility.Hidden;
            recipesList.ItemsSource = RecipesList();
        }

        private void ListViewItem_MouseClick (object sender, MouseEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;

            if (item != null)
            {
                var data = item.DataContext;
                if (data is Recipe recipe) AppFrame.frameMain.Navigate(new RecipePg(recipe, author));
            }
        }

        private void btnToEnd_Click(object sender, RoutedEventArgs e)
        {
            recipesList.ScrollIntoView(recipesList.Items[recipesList.Items.Count - 1]);
        }

        private void btnToHome_Click(object sender, RoutedEventArgs e)
        {
            recipesList.ScrollIntoView(recipesList.Items[0]);
        }

        private void btnLineUp_Click(object sender, RoutedEventArgs e)
        {
            var scrollViewer = ListViewExtensions.GetScrollViewer(recipesList) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.LineUp();
            }
        }
        private void btnLineDown_Click(object sender, RoutedEventArgs e)
        {
            var scrollViewer = ListViewExtensions.GetScrollViewer(recipesList) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.LineDown();
            }
        }

        public static class ListViewExtensions
        {
            public static ScrollViewer GetScrollViewer(DependencyObject o)
            {
                if (o is ScrollViewer)
                {
                    return (ScrollViewer)o;
                }

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
                {
                    var child = VisualTreeHelper.GetChild(o, i);
                    var result = GetScrollViewer(child);
                    if (result == null)
                    {
                        continue;
                    }
                    else
                    {
                        return result;
                    }
                }

                return null;
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new AddEditRecipe(null));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AppConnect.modelOdb.ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
            recipesList.ItemsSource = AppConnect.modelOdb.Recipes.ToList();
        }
    }
}
