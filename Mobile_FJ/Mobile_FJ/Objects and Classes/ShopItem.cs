using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile_FJ.Objects_and_Classes
{
    public class ShopItem
    {
        public string Name { get; set; }
        public string Category { get; set; }

        public string LabelString { get; set; }

        public ShopItem(string name, string category)
        {
            Name = name;
            Category = category;
            LabelString = Name + " (" + Category.Trim() + ")"; 
        }
    }
}
