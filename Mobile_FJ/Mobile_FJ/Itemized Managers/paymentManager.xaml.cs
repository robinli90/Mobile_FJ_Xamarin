using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Mobile_FJ.Objects_and_Classes;
using Xamarin.Forms;


namespace Mobile_FJ
{
    public partial class paymentManager : ContentPage
    {
        private bool isAddPaymentViewActive = false;
        private int _editIndex = -1;

        public ObservableCollection<string> Payments { get; set; }

        public paymentManager()
        {
            Payments = Global.PaymentList;

            InitializeComponent();

            PaymentManagerListView.BindingContext = this;

            PaymentNameBox.Keyboard = Keyboard.Chat; // Field starts with capital
        }

        private void DeleteItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                DeletePayment(menuItem.CommandParameter.ToString());
                _editIndex = -1;
                gridView1.IsVisible = false;
                isAddPaymentViewActive = false;
            }
        }

        private void EditItem_OnClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem != null)
            {
                string refPayment = menuItem.CommandParameter.ToString();
                _editIndex = Global.PaymentList.IndexOf(refPayment);
                gridView1.IsVisible = true;
                isAddPaymentViewActive = true;
                PaymentNameBox.Text = refPayment;
            }
        }

        private async void DeletePayment(string PaymentName)
        {
            var answer = await DisplayAlert("Delete Payment",
                String.Format("Are you sure you wish to remove {0} from your list of payments?", PaymentName),
                "Yes", "No");
            if (answer)
            {
                Global.PaymentList.Remove(PaymentName);
                Global.SaveLocalInformation();
            }
        }

        private void NewPaymentButton_OnClicked(object sender, EventArgs e)
        {
            _editIndex = -1;
            gridView1.IsVisible = !isAddPaymentViewActive;
            isAddPaymentViewActive = !isAddPaymentViewActive;
            NewPaymentButton.Text = !isAddPaymentViewActive ? "ADD" : "HIDE";
        }

        private void AddPayment_OnClicked(object sender, EventArgs e)
        {
            if (PaymentNameBox.Text == null) return;

            string PaymentName = PaymentNameBox.Text;

            if (PaymentName.Length > 0 && (!Global.PaymentList.Contains(PaymentName) || _editIndex >= 0))
            {
                gridView1.IsVisible = false;
                isAddPaymentViewActive = false;
                PaymentNameBox.Text = "";

                if (_editIndex >= 0)
                {
                    Global.PaymentList.RemoveAt(_editIndex);
                    Global.PaymentList.Insert(_editIndex, PaymentName);
                }
                else
                {
                    Global.PaymentList.Add(PaymentName);
                }

                NewPaymentButton.Text = "ADD";
                Global.CurrentPayment = PaymentName;
                Global.SaveLocalInformation();
                _editIndex = -1;
            }
            else if (Global.PaymentList.Select(x => x.ToLower()).Contains(PaymentName.ToLower()) && _editIndex < 0)
            {
                DisplayAlert("Payment Error", "You already have this payment added in your list", "OK");
            }
            else
            {
                DisplayAlert("Payment Error", "Payment name is invalid", "OK");
            }
        }

        private void DebugTest_OnClicked(object sender, EventArgs e)
        {
            string[] lines = Global.GetFileContent("Mobile_FJ.config")
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            Debug.WriteLine("Payment File: " + String.Join(Environment.NewLine, lines));
        }
    }
}
