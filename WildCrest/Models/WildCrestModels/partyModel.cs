using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class partyModel
    {
        public int ID { get; set; }
        public string Party_Name { get; set; }
        public string Party_Owner { get; set; }
        public string Phone_No { get; set; }
        public string Description { get; set; }
        public string Total_Member { get; set; }
        public string Price { get; set; }
        public string Total_Amount { get; set; }
        public Nullable<int> party_Food_menu_Id { get; set; }
        public string Party_Date { get; set; }
        public string Gst { get; set; }
        public List<tbl_Buffet_Menu> FoodMenus { get; set; }
        public string AdvancePay { get; set; }
        public string amount_tobepay { get; set; }
        public Nullable<bool> IsAdvance_Payment { get; set; }

        public int BillNo { get; set; }
        public string ModeOfPayment { get; set; }
        public string DateofBilling { get; set; }
         public string AmountPaid { get; set; }
    }
   
}