using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class Inventory
    {
        public int ID { get; set; }
        public string Item_Name { get; set; }
        public string Type { get; set; }
        public Nullable<double> Quantity { get; set; }
        public Nullable<int> Price { get; set; }
        public Nullable<int> VendorID { get; set; }
        public string Added_Date { get; set; }
        public double? InStock { get; set; }

        public string VendorName { get; set; }

        public string Measurement { get; set; }

        public DateTime AddedDate1 { get; set; }
        
    }
}