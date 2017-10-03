using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Mobile_FJ.Objects_and_Classes;
using Xamarin.Forms;

namespace Mobile_FJ
{
    public partial class NewOrder : ContentPage
    {
        public ObservableCollection<Item> Items { get; set; }
        private bool _editing = false;
        private Order _refOrder;

        private double _currentOrderTotal = 5.5;

        public NewOrder()
        {
            Items = Global.ItemList;

            Debug.WriteLine("Current OrderID = " + Global.CurrentOrderId);

            InitializeComponent();

            listView1.ItemSelected += listClick;

            if (Global.CurrentOrderId.Length > 0) //if editing existing
            {
                _editing = true;
                _refOrder = Global.OrderList.First(x => x.OrderID == Global.CurrentOrderId); //Order should be there
            }
            else
            {
                Global.CurrentOrderId = Global.GenerateNewOrderId();
            }

            UpdateOrderTotal();

            Global.FilterItemListById(Global.CurrentOrderId);

            
            listView1.BindingContext = this;

        }

        void listClick(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                Global.itemEditingIndex =
                    Global.ItemList.IndexOf(Global.ItemList.First(x => x.Name == e.SelectedItem.ToString()));

                Navigation.PushModalAsync(new NewItem());

                listView1.SelectedItem = null;
            }
        }

        private void UpdateOrderTotal()
        {
            orderTotal.Text = "ORDER TOTAL: $" + String.Format("{0:0.00}", Global.MasterItemList.Where(x => x.OrderID == Global.CurrentOrderId || x.OrderID == "999999999").Sum(x => x.GetTotalPrice()));
        }

        private void UpdateUI()
        {
            noItemFoundLabel.IsVisible = Items.Count == 0;
            listView1.IsVisible = Items.Count > 0;
            UpdateOrderTotal();
        }

        protected override void OnAppearing()
        {
            Global.itemEditingIndex = -1;

            UpdateUI();

            // remove listeners
            LocationPicker.SelectedIndexChanged -= LocationPicker_OnSelectedIndexChanged;
            PaymentPicker.SelectedIndexChanged -= PaymentPicker_OnSelectedIndexChanged;

            LocationPicker.Items.Clear();
            PaymentPicker.Items.Clear();

            if (_editing)
            {
                Global.CurrentLocation = _refOrder.Location;
                Global.CurrentPayment = _refOrder.Payment;
            }

            // Add all distinct locations
            foreach (var location in Global.LocationList.Distinct().OrderBy(x => x))
                LocationPicker.Items.Add(location);

            // Add all distinct payments
            foreach (var payment in Global.PaymentList.Distinct().OrderBy(x => x))
                PaymentPicker.Items.Add(payment);

            // Set default item
            if (Global.CurrentLocation != "")
            {
                LocationPicker.SelectedIndex =
                    Global.LocationList.IndexOf(Global.CurrentLocation); // set edit location
            }
            
            if (Global.CurrentPayment != "")
            {
                PaymentPicker.SelectedIndex =
                    Global.PaymentList.IndexOf(Global.CurrentPayment); // set edit payment
            }

            // add listeners
            LocationPicker.SelectedIndexChanged += LocationPicker_OnSelectedIndexChanged;
            PaymentPicker.SelectedIndexChanged += PaymentPicker_OnSelectedIndexChanged;

            UpdateOrderTotal();

            base.OnAppearing();
        }

        private void DeleteItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                Item refItem =
                    Global.MasterItemList.First(x => x.OrderID == Global.CurrentOrderId &&
                                                     x.Name == menuItem.CommandParameter);
                DeleteItem(refItem);
            }
        }

        private async void DeleteItem(Item item)
        {
            var answer = await DisplayAlert("Deleting item from order",
                "Are you sure you wish to delete " + item.Name + "?",
                "Yes", "No");
            if (answer)
            {
                Global.DeleteItem(item); // delete order
                Global.ItemList.Remove(item);
                UpdateUI();
            }

        }

        private void EditItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem) sender;

            if (menuItem != null)
            {
                Global.itemEditingIndex =
                    Global.ItemList.IndexOf(Global.ItemList.First(x => x.Name == menuItem.CommandParameter));
                Navigation.PushModalAsync(new NewItem());
            }
        }

        private void CreateNewItem(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new NewItem());
        }

        private void SaveOrder(object sender, EventArgs e)
        {
            if (LocationPicker.SelectedIndex >= 0 && PaymentPicker.SelectedIndex >= 0 && Items.Count > 0)
            {
                Global.SaveOrderItems(); // transfer ownership to current order

                string locationText = LocationPicker.Items[LocationPicker.SelectedIndex];
                string paymentText = PaymentPicker.Items[PaymentPicker.SelectedIndex];
                int editIndex = -1;
                if (_editing)
                {
                    editIndex = Global.OrderList.IndexOf(_refOrder);
                    Global.DeleteOrderById(Global.CurrentOrderId);
                }
                double orderTotal = 0;

                Global.AddOrder(new Order(locationText, paymentText, (_editing ? _refOrder.Date : DateTime.Now), (_editing && _refOrder.IsSyncing), Global.CurrentOrderId), editIndex);

                Global.SaveLocalInformation();
                Navigation.PopModalAsync();
            }
            else if (Items.Count == 0)
            {
                DisplayAlert("Error creating order", "This order has no items", "OK");
            }
            else if (LocationPicker.SelectedIndex < 0)
            {
                DisplayAlert("Error creating order", "This order is missing the location", "OK");
            }
            else if (PaymentPicker.SelectedIndex < 0)
            {
                DisplayAlert("Error creating order", "This order is missing the payment", "OK");
            }
        }

        private void AddLocation_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new locationManager());
        }

        private void AddPayment_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new paymentManager());
        }

        private void LocationPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Global.CurrentLocation = LocationPicker.Items[LocationPicker.SelectedIndex];
        }

        private void PaymentPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Global.CurrentPayment = PaymentPicker.Items[PaymentPicker.SelectedIndex];
        }
    }
}
