using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Mobile_FJ.Objects_and_Classes;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;


namespace Mobile_FJ
{
    public partial class ShoppingList : ContentPage
    {
        private bool isAddShopItemViewActive = false;
        private int _editIndex = -1;

        public ObservableCollection<ShopItem> ListView { get; set; }

        public ShoppingList()
        {
            ListView = Global.ShoppingList;

            InitializeComponent();

            ShopItemManagerListView.BindingContext = this;

            ShopItemNameBox.Keyboard = Keyboard.Chat; // Field starts with capital

            // Initialize category picker
            CategoryPicker.Items.Clear();

            // Add all distinct Categorys
            foreach (var Category in Global.CategoryList.Distinct().OrderBy(x => x))
                CategoryPicker.Items.Add(Category);
        }

        private void DeleteItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                DeleteShopItem(Global.ShoppingList.First(x => x.Name == menuItem.CommandParameter.ToString()));
                gridView1.IsVisible = false;
                isAddShopItemViewActive = false;
            }
        }

        private void DeleteShopItem(ShopItem SI)
        {
            //var answer = await DisplayAlert("Delete Item",
            //    String.Format("Are you sure you wish to remove {0} from your shopping list?", SI.Name),
            //    "Yes", "No");
            //if (answer)
            //{
                Global.ShoppingList.Remove(SI);
                Global.SaveLocalInformation();
            //}
        }

        private void NewShopItemButton_OnClicked(object sender, EventArgs e)
        {
            _editIndex = -1;
            gridView1.IsVisible = !isAddShopItemViewActive;
            isAddShopItemViewActive = !isAddShopItemViewActive;
            NewShopItemButton.Text = !isAddShopItemViewActive ? "ADD" : "HIDE";
        }

        private void AddShopItem_OnClicked(object sender, EventArgs e)
        {
            if (CategoryPicker.SelectedIndex > 0 && ShopItemNameBox.Text.Length > 0)
            {
                gridView1.IsVisible = false;
                isAddShopItemViewActive = false;
                NewShopItemButton.Text = "ADD";
                Global.ShoppingList.Add(new ShopItem(
                    ShopItemNameBox.Text, 
                    CategoryPicker.Items[CategoryPicker.SelectedIndex])
                );
                ShopItemNameBox.Text = "";
                Global.SaveLocalInformation();
            }
        }

        private void ClearButton_OnClick(object sender, EventArgs e)
        {
            ClearItemList();
        }

        private async void ClearItemList()
        {
            var answer = await DisplayAlert("Clear Shopping List",
                "Are you sure you wish to clear your shopping list?",
                "Yes", "No");
            if (answer)
            {
                for (int i = Global.ShoppingList.Count - 1; i >= 0; i--)
                {
                    Global.ShoppingList.RemoveAt(i);
                }
            }
        }

        private void Update_OnClick(object sender, EventArgs e)
        {
            if (Global.isOnWifi())
            {
                Global.ReadShoppingSyncFile();

                DisplayAlert("Refreshed!",
                    "You have successfully uploaded shopping list items from your desktop application", "OK");
            }
            else
            {
                DisplayAlert("Connection Error",
                    "You have be on a WiFi connection to download your shopping list", "OK");
            }
        }
    }
}
