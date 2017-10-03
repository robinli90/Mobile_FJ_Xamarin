using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Mobile_FJ.Objects_and_Classes;

namespace Mobile_FJ
{
    public class Order : INotifyPropertyChanged
    {
        //[PrimaryKey, AutoIncrement]
        //public int ID { get; set; }

        // Unsynced order variables
        public string Location { get; set; }
        public string OrderID { get; set; }
        public bool isSynced { get; set; }
        public string Payment { get; set; }

        // Global order variables
        public string OrderMemo { get; set; }
        public string OrderSerial { get; set; }
        public double GC_Amount { get; set; }
        public double Order_Total_Pre_Tax { get; set; }
        public double Order_Taxes { get; set; }
        public double Order_Discount_Amt { get; set; }
        public bool Tax_Overridden { get; set; }
        public int Order_Quantity { get; set; }
        public bool IUO_IsSynced { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string labelText { get; set; }

        private bool _IsSyncing;
        public bool IsSyncing
        {
            get { return _IsSyncing; }
            set { _IsSyncing = value; OnPropertyChanged(); }
        }

        // Set sync boolean
        public void SetSync(bool boolean)
        {
            IsSyncing = boolean;
        }

        //public int Order_Quantity { get; set; }
        public DateTime Date { get; set; }

        public Order(string location, string payment, DateTime date, bool isSyncing=false, string orderID="")
        {
            IsSyncing = isSyncing;
            Payment = payment;
            Location = location;
            //OrderTotalPreTax = orderTotalPreTax;
            OrderID = orderID .Length > 0 ? orderID : Global.CurrentOrderId;
            Date = date;
            labelText = Date.ToString("MM/dd/yyyy") + " at " + Date.ToString("hh:mm tt")
                        + "                                                                                            "
                        + " $" + String.Format("{0:0.00}", Global.MasterItemList.Where(x => x.OrderID == OrderID).Sum(x => x.GetTotalPrice())) + 
                        " - " + Location + " (" + Global.MasterItemList.Where(x => x.OrderID == OrderID).Sum(x => x.Quantity) + " item(s) in order)";

        }

        public Order()
        {
        }

        public override string ToString()
        {
            //return Location + " ($" + String.Format("{0:0.00}", OrderTotalPreTax) + ")";
            return OrderID;
        }

        public void OnPropertyChanged(String propertyName=null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
