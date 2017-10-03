using System;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Mobile_FJ.Objects_and_Classes;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;


namespace Mobile_FJ
{
    public partial class MainPage : ContentPage
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                Debug.WriteLine("Set to {0}", value);
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private string _textStr;
        public string textStr
        {
            get { return _textStr; }
            set
            {
                _textStr = value;
                OnPropertyChanged();
            }
        }

        private static bool EnableWifiCheck = false;

        public ObservableCollection<Order> Orders { get; set; }

        public MainPage()
        {
            Debug.WriteLine("Starting Application");

            // Remove verification page from the stack)
            try
            {
                Navigation.RemovePage(Global.VerificationPage);
            }
            catch
            {
                // No verification page loaded
            }


            // Only load once
            if (!Global.isLoggedIn)
            {
                Global.LoadLocalInformation();
                //Task.Run(() => Global.DownloadAllInformation());
            }
            Orders = Global.OrderList;

            InitializeComponent();

            if (!Global.isLoggedIn)
                Navigation.PushModalAsync(new Login());

            listView1.ItemSelected += listClick;

            //This is a setting to make my page look autonomous
            NavigationPage.SetHasNavigationBar(this, false);

            listView1.BindingContext = this;

            AIndicator.BindingContext = this;
            //label123.BindingContext = this;
        }


        void SettingsButton_Clicked(object sender, System.EventArgs e)
        {
            //Global.hasSyncedItems = true;
            //Global.SaveLocalInformation();
            //DisplayAlert("Internal Config File", Global.GetFileContent(Global.ConfigFileName), "OK");
            //DisplayAlert("Local Sync File (copy to FTP)", Global.GetFileContent("localSyncFTP"), "OK");
            //AIndicator.IsRunning = true;
            textStr = "TEST";
            Debug.WriteLine("Start: {0}", DateTime.Now);

            RefreshTaskAsync();
            textStr = "TEST COMPLETE";
            Debug.WriteLine("End: {0}", DateTime.Now);
           // AIndicator.IsRunning = false;
        }

        private async Task RefreshTaskAsync()
        {
            RefreshClick();
        }

        private async Task<bool> RefreshClick()
        {
            Debug.WriteLine("FTP Start: {0}", DateTime.Now);
            if (!EnableWifiCheck || Global.isOnWifi())
            {
                bool syncComplete = await Global.LoadSyncFile();
                //DisplayAlert("Removing...", "Removed: " + Global.RemoveSyncedOrders(), "OK");
                Global.RemoveSyncedOrders();
                UpdateUI();

                Debug.WriteLine("FTP End: {0}", DateTime.Now);
                return syncComplete;
            }

            await DisplayAlert("Connection Issue",
                "Oops! It seems like you are not connected to a WiFi connection. You cannot refresh while you are not connected to WiFi",
                "OK");

            Debug.WriteLine("FTP End: {0}", DateTime.Now);
            return false;

        }

        void listClick(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                EditClick(e.SelectedItem.ToString());
                
            }
        }

        private async void EditClick(string orderID)
        {
            if (Global.GetOrderById(orderID).IsSyncing)
            {
                var answer = await DisplayAlert("Order is syncing",
                    "Do you wish view this order? Continuing will remove this item from sync",
                    "Yes", "No");
                if (answer)
                {
                    if (!EnableWifiCheck || Global.isOnWifi())
                    {
                        // Set edit ID
                        Global.CurrentOrderId = orderID;

                        Global.SyncList = Global.SyncList.Where(x => !x.Contains(orderID)).ToList();

                        // Set sync off 
                        if (Global.OrderList.Count(x => x.OrderID == orderID) > 0)
                            Global.OrderList.First(x => x.OrderID == orderID).IsSyncing = false;

                        await Navigation.PushModalAsync(new NewOrder());

                        Global.hasSyncedItems = true;
                        Global.SaveLocalInformation();
                    }
                    else
                    {
                        await DisplayAlert("Connection Issue",
                            "Oops! It seems like you are not connected to a WiFi connection. Order cannot be unsynced while you're not connected to WiFi",
                            "OK");
                    }
                }
            }
            else
            {
                Global.CurrentOrderId = orderID;
                await Navigation.PushModalAsync(new NewOrder());
            }

            listView1.SelectedItem = null;
        }

        private void CreateNewOrder(object sender, EventArgs e)
        {
            Global.CurrentOrderId = ""; // set to new order
            Navigation.PushModalAsync(new NewOrder());
        }

        private void DeleteOrder_OnClick(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                DeleteOrder(menuItem.CommandParameter.ToString());
                UpdateUI();
            }
        }

        private async void DeleteOrder(string orderID)
        {
            if (!Global.GetOrderById(orderID).IsSyncing)
            {
                var answer = await DisplayAlert("Deleting order from " + Global.GetOrderById(orderID).Location,
                    "Are you sure you wish to delete this order?",
                    "Yes", "No");
                if (answer)
                {
                    Global.DeleteOrderById(orderID); // delete order
                    Global.DeleteItemsById(orderID); // delete associated items
                }
            }
            else
            {
                await DisplayAlert("Delete Error", "You cannot delete order. Order is currently syncing", "OK");
            }

        }

        private void SyncToServer_OnClick(object sender, EventArgs e)
        {
            SyncToServer();
        }

        private async void SyncToServer()
        {
            if (Global.OrderList.Count(x => !x.IsSyncing) > 0
            ) // Check if there's any orders that have not been synced
            {
                var answer = await DisplayAlert("Sync Orders to Server",
                    "Do you want to sync all unsynced orders to server?",
                    "Yes", "No");
                if (answer)
                {
                    if (!EnableWifiCheck || Global.isOnWifi())
                    {
                        Global.hasSyncedItems = true;
                        foreach (Order order in Global.OrderList)
                        {
                            Global.SyncList.Add(order.OrderID);
                            order.SetSync(true);
                        }

                        Global.SaveLocalInformation();
                    }
                    else
                    {
                        await DisplayAlert("Connection Issue",
                            "Oops! It seems like you are not connected to a WiFi connection. Syncing works best with a stable network",
                            "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Sync Error", "No orders available for syncing", "OK");
            }
        }

        private void UpdateUI()
        {
            noOrdersFoundLabel.IsVisible = Global.OrderList.Count == 0;
            listView1.IsVisible = Global.OrderList.Count > 0;
        }

        protected override void OnAppearing()
        {
            Global.ClearTempItems();
            Task.Run(() => Global.SaveLocalInformation());

            // Sync update
            //Global.LoadSyncFile();
            //Global.RemoveSyncedOrders();


            UpdateUI();
            Global.ResetVolatileParameters();

        }

        private void ShoppingButton_OnClick(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new ShoppingList());
        }
    }
}
