using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Mobile_FJ.Objects_and_Classes;
using Xamarin.Forms;


namespace Mobile_FJ
{
    public partial class locationManager : ContentPage
    {
        private bool isAddLocationViewActive = false;
        private int _editIndex = -1;

        public ObservableCollection<string> Locations { get; set; }

        public locationManager()
        {
            if (Global.LocationList.Count == 0)
            {
                PromptNoEntries();
                Global.SaveLocalInformation();
            }

            Locations = Global.LocationList;

            InitializeComponent();

            locationManagerListView.BindingContext = this;

            LocationNameBox.Keyboard = Keyboard.Chat; // Field starts with capital
        }

        private async void PromptNoEntries()
        {
            var answer = await DisplayAlert("No locations found!",
                "We noticed you do not have any locations. Would you like to import a template?", 
                "Yes", "No");
            if (answer)
            {
                Global.LocationList = _defaultList;
                Locations = Global.LocationList;
                locationManagerListView.BindingContext = this;
            }
        }

        private void DeleteItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                DeleteLocation(menuItem.CommandParameter.ToString());
                _editIndex = -1;
                gridView1.IsVisible = false;
                isAddLocationViewActive = false;
            }
        }

        private void EditItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                string refLocation = menuItem.CommandParameter.ToString();
                _editIndex = Global.LocationList.IndexOf(refLocation);
                gridView1.IsVisible = true;
                isAddLocationViewActive = true;
                LocationNameBox.Text = refLocation;
            }
        }

        private async void DeleteLocation(string locationName)
        {
            var answer = await DisplayAlert("Delete location",
                String.Format("Are you sure you wish to remove {0} from your list of locations?", locationName),
                "Yes", "No");
            if (answer)
            {
                Global.LocationList.Remove(locationName);
                Global.SaveLocalInformation();
            }
        }

        private void NewLocationButton_OnClicked(object sender, EventArgs e)
        {
            _editIndex = -1;
            gridView1.IsVisible = !isAddLocationViewActive;
            isAddLocationViewActive = !isAddLocationViewActive;
            NewLocationButton.Text = !isAddLocationViewActive ? "ADD" : "HIDE";
        }

        private void AddLocation_OnClicked(object sender, EventArgs e)
        {
            if (LocationNameBox.Text == null) return;

            string locationName = LocationNameBox.Text;

            if (locationName.Length > 0 && (!Global.LocationList.Contains(locationName) || _editIndex >= 0))
            {
                gridView1.IsVisible = false;
                isAddLocationViewActive = false;
                LocationNameBox.Text = "";

                if (_editIndex >= 0)
                {
                    Global.LocationList.RemoveAt(_editIndex);
                    Global.LocationList.Insert(_editIndex, locationName);
                }
                else
                {
                    Global.LocationList.Add(locationName);
                }
                NewLocationButton.Text = "ADD";
                Global.CurrentLocation = locationName;
                Global.SaveLocalInformation();
                _editIndex = -1;
            }
            else if (Global.LocationList.Select(x => x.ToLower()).Contains(locationName.ToLower()) && _editIndex < 0)
            {
                DisplayAlert("Location Error", "You already have this location added in your list", "OK");
            }
            else
            {
                DisplayAlert("Location Error", "Location name is invalid", "OK");
            }
        }

        private void DebugTest_OnClicked(object sender, EventArgs e)
        {
            string[] lines = Global.GetFileContent("Mobile_FJ.config")
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            Debug.WriteLine("Location File: " + String.Join(Environment.NewLine, lines));
        }

        private static ObservableCollection<string> _defaultList = new ObservableCollection<string>()
        {
            "Amazon.ca",
            "Aritzia",
            "Banana Republic",
            "BBT Shop",
            "Bed Bath & Beyond",
            "Beer Store",
            "Bestbuy",
            "Canada Computers",
            "Canadian Tire",
            "Chinese Restaurant",
            "Costco",
            "Dollarama",
            "ESSO",
            "Fido",
            "Foody Supermarket",
            "Home Depot",
            "IKEA",
            "Japanese Restaurant",
            "LCBO",
            "Loblaw",
            "Lowes",
            "McDonald's",
            "NoFrills",
            "Personal",
            "Petro-Canada",
            "Petsmart",
            "RainCo",
            "Shell",
            "Shoppers",
            "T&T Supermarket",
            "Tim Hortons",
            "Wal-mart",
            "Western Restaurant",
            "WinCo Supermarket",
            "Winners",
        };
    }
}
