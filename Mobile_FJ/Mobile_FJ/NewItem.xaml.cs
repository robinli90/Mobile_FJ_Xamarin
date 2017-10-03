using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Mobile_FJ.Objects_and_Classes;
using Xamarin.Forms;

namespace Mobile_FJ
{
    public partial class NewItem : ContentPage
    {
        private bool _editing = false;
        private Item _refItem;

        public NewItem()
        {
            InitializeComponent();

            itemNameBox.Keyboard = Keyboard.Chat; // Field starts with capital

            if (Global.itemEditingIndex > -1) // is editing
            {
                _editing = true;
                _refItem = Global.ItemList[Global.itemEditingIndex];

                Global.CurrentItemName = _refItem.Name;
                Global.CurrentItemQuantity = _refItem.Quantity;
                Global.CurrentItemPrice = _refItem.Price.ToString();
                AddItemButton.Text = "SAVE";
            }
        }

        private void AddItem(object sender, EventArgs e)
        {
            //Global.AddItem(new Item("itemName", 50, 2, Global.CurrentOrderId)
            //{
            //    Category = "category",
            //    Date = DateTime.Now,
            //    Location = "location",
            //    Quantity = 1
            //});

            Global.FilterItemListById(Global.CurrentOrderId);
            Navigation.PopModalAsync();
        }

        private void AddCategoryButton_OnClicked(object sender, EventArgs e)
        {
            Global.CurrentItemName = itemNameBox.Text;
            Global.CurrentItemQuantity = QuantityPicker.SelectedIndex + 1;
            Global.CurrentItemPrice = itemPriceBox.Text;
            Navigation.PushModalAsync(new CategoryManager());
        }

        protected override void OnAppearing()
        {
            // remove listeners
            CategoryPicker.SelectedIndexChanged -= CategoryPicker_OnSelectedIndexChanged;

            // Initialize quantity picker
            QuantityPicker.Items.Clear();
            for (int i = 1; i <= 20; i++)
                QuantityPicker.Items.Add(i.ToString());

            // Initialize category picker
            CategoryPicker.Items.Clear();
            if (_editing)
            {
                Global.CurrentCategory = _refItem.Category;
            }

            // Add all distinct Categorys
            foreach (var Category in Global.CategoryList.Distinct().OrderBy(x => x))
                CategoryPicker.Items.Add(Category);

            if (_editing)
            {

                // Set default item
                if (Global.CurrentCategory != "")
                {
                    CategoryPicker.SelectedIndex =
                        Global.CategoryList.IndexOf(Global.CurrentCategory); // set edit Category
                }

                itemNameBox.Text = Global.CurrentItemName;
                itemPriceBox.Text = Global.CurrentItemPrice;

            }
            else
            {
                if (Global.CurrentCategory.Length > 0)
                    CategoryPicker.SelectedIndex = Global.CategoryList.IndexOf(Global.CurrentCategory);
            }

            if (Global.CurrentItemQuantity > 0)
            {
                QuantityPicker.SelectedIndex = Global.CurrentItemQuantity - 1;
            }
            else
            {
                QuantityPicker.SelectedIndex = 0;
            }

            // add listeners
            CategoryPicker.SelectedIndexChanged += CategoryPicker_OnSelectedIndexChanged;

            base.OnAppearing();
        }


        private void CategoryPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            //Global.CurrentCategory = CategoryPicker.Items[CategoryPicker.SelectedIndex];
        }

        private void AddItemButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                if (CategoryPicker.SelectedIndex >= 0 && QuantityPicker.SelectedIndex >= 0 &&
                    itemNameBox.Text.Length > 0 && itemPriceBox.Text.Length > 0)
                {

                    var categoryText = CategoryPicker.Items[CategoryPicker.SelectedIndex];
                    var quantityText = QuantityPicker.Items[QuantityPicker.SelectedIndex];

                    Global.CurrentCategory = categoryText;

                    Global.AddItem(
                        new Item(itemNameBox.Text, Convert.ToDouble(itemPriceBox.Text), Convert.ToInt32(quantityText),
                            categoryText, "999999999"), _editing ? Global.ItemList.IndexOf(_refItem) : -1);

                    if (_editing)
                    {
                        Global.DeleteItem(_refItem);
                    }

                    Navigation.PopModalAsync();
                }
                else if (itemPriceBox.Text.Length == 0)
                {
                    DisplayAlert("Error creating item", "This item has no price", "OK");
                }
                else if (CategoryPicker.SelectedIndex < 0)
                {
                    DisplayAlert("Error creating item", "This item is missing the category", "OK");
                }
                else if (QuantityPicker.SelectedIndex < 0)
                {
                    DisplayAlert("Error creating item", "This order is missing the quantity", "OK");
                }
                else if (itemNameBox.Text.Length == 0)
                {
                    DisplayAlert("Error creating item", "This order is missing the name", "OK");
                }
            }
            catch
            {
                DisplayAlert("Error creating item", "Missing a required field", "OK");
            }
        }
    }
}
 