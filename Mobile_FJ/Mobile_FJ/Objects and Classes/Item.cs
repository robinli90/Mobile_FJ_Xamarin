using System;

namespace Mobile_FJ
{
    public class Item
    {
        //[PrimaryKey, AutoIncrement]
        //public int ID { get; set; }

        // Unsynced item variables
        public string Name { get; set; }
        public string Category { get; set; }
        public string OrderID { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        // Below contains only global item
        public string Status { get; set; }
        public string Location { get; set; }
        public string Payment_Type { get; set; }
        public string Memo { get; set; }
        public double Discount_Amt { get; set; }
        public bool RefundAlert { get; set; }
        public int consumedStatus { get; set; } // 2=regular, 1=pendingPurchase, 0=complete (for shopping list)
        public DateTime Date { get; set; }
        public DateTime Refund_Date { get; set; }


        public string labelText { get; set; }

        public Item()
        {
            
        }

        public Item(string name, double itemPrice, int qty, string category, string orderID)
        {
            Name = name;
            Price = itemPrice;
            Quantity = qty;
            OrderID = orderID;
            Category = category;
            labelText = Name + " (Qty: " + Quantity + ", Ctg: " + Category + ")"
                    + "                                                                                            "
                    + " $" + String.Format("{0:0.00}", Price);
        }

        public override string ToString()
        {
            return Name; 
        }

        /// <summary>
        /// Get the total price * quantity
        /// </summary>
        /// <returns></returns>
        public double GetTotalPrice()
        {
            return Price * Quantity;
        }
    }

}
