using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class MenuItems
    {
        public int ID { get; set; }
        public string Food_Item_Name { get; set; }
        public string Details { get; set; }
        public Nullable<double> Price { get; set; }
        public Nullable<bool> NewItemApprvFrmSuperAdm { get; set; }
        public Nullable<bool> DelItemApprvFromSuperAdm { get; set; }
        public string FoodType { get; set; }

        public string AddedBy { get; set; }
        public string DeletedBy { get; set; }

        public Nullable<int> InventoryID { get; set; }

        public int ConsumableID { get; set; }
    }
}