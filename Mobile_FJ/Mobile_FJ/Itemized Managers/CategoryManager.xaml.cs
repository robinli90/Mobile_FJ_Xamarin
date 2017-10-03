using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Mobile_FJ.Objects_and_Classes;
using Xamarin.Forms;


namespace Mobile_FJ
{
    public partial class CategoryManager : ContentPage
    {
        private bool isAddCategoryViewActive = false;
        private int _editIndex = -1;

        public ObservableCollection<string> Categories { get; set; }

        public CategoryManager()
        {
            if (Global.CategoryList.Count == 0)
            {
                PromptNoEntries();
                Global.SaveLocalInformation();
            }

            Categories = Global.CategoryList;

            InitializeComponent();

            CategoryManagerListView.BindingContext = this;

            CategoryNameBox.Keyboard = Keyboard.Chat; // Field starts with capital
        }

        private void DeleteItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                DeleteCategory(menuItem.CommandParameter.ToString());
                _editIndex = -1;
                gridView1.IsVisible = false;
                isAddCategoryViewActive = false;
            }
        }

        private async void PromptNoEntries()
        {
            var answer = await DisplayAlert("No categories found!",
                "We noticed you do not have any categories. Would you like to import a template?",
                "Yes", "No");
            if (answer)
            {
                Global.CategoryList = _defaultList;
                Categories = Global.LocationList;
                CategoryManagerListView.BindingContext = this;
            }
        }

        private void EditItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                string refCategory = menuItem.CommandParameter.ToString();
                _editIndex = Global.CategoryList.IndexOf(refCategory);
                gridView1.IsVisible = true;
                isAddCategoryViewActive = true;
                CategoryNameBox.Text = refCategory;
            }
        }

        private async void DeleteCategory(string CategoryName)
        {
            var answer = await DisplayAlert("Delete Category",
                String.Format("Are you sure you wish to remove {0} from your list of categories?", CategoryName),
                "Yes", "No");
            if (answer)
            {
                Global.CategoryList.Remove(CategoryName);
                Global.SaveLocalInformation();
            }
        }

        private void NewCategoryButton_OnClicked(object sender, EventArgs e)
        {
            _editIndex = -1;
            gridView1.IsVisible = !isAddCategoryViewActive;
            isAddCategoryViewActive = !isAddCategoryViewActive;
            NewCategoryButton.Text = !isAddCategoryViewActive ? "ADD" : "HIDE";
        }

        private void AddCategory_OnClicked(object sender, EventArgs e)
        {
            if (CategoryNameBox.Text == null) return;

            string CategoryName = CategoryNameBox.Text;

            if (CategoryName.Length > 0 && (!Global.CategoryList.Contains(CategoryName) || _editIndex >= 0))
            {
                gridView1.IsVisible = false;
                isAddCategoryViewActive = false;
                CategoryNameBox.Text = "";

                if (_editIndex >= 0)
                {
                    Global.CategoryList.RemoveAt(_editIndex);
                    Global.CategoryList.Insert(_editIndex, CategoryName);
                }
                else
                {
                    Global.CategoryList.Add(CategoryName);
                }
                NewCategoryButton.Text = "ADD";
                Global.CurrentCategory = CategoryName;
                Global.SaveLocalInformation();
                _editIndex = -1;
            }
            else if (Global.CategoryList.Select(x => x.ToLower()).Contains(CategoryName.ToLower()) && _editIndex < 0)
            {
                DisplayAlert("Category Error", "You already have this Category added in your list", "OK");
            }
            else
            {
                DisplayAlert("Category Error", "Category name is invalid", "OK");
            }
        }

        private static ObservableCollection<string> _defaultList = new ObservableCollection<string>()
        {
            "Alcohol",
            "Arts & Crafts",
            "Beauty Product",
            "Car Maintenance",
            "Clothing",
            "Computer & Tech",
            "Dine-Out",
            "Entertainment",
            "Furniture",
            "Games",
            "Gasoline",
            "Grocery (Tax Exempt)",
            "Grocery (Taxable)",
            "Health & Wellness",
            "Home Decor",
            "Household Items",
            "Hygeine Items",
            "Make-up",
            "Medication",
            "Miscellaneous",
            "Non-Alcoholic Beverages",
            "Outdoor",
            "Parking",
            "Personal",
            "Pet Items"
        };
    }
}
