using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class EntryMemberModel
    {
        public int ID { get; set; }
      
        public string Name { get; set; }
        public string Phone_No { get; set; }
       
        public string Total_Member { get; set; }
        public string Price { get; set; }
        public string Total_Amount { get; set; }

        public string GstAmount { get; set; }
        public string Price_Without_Gst { get; set; }





        public int BillNo { get; set; }
        public string ModeOfPayment { get; set; }
        public string DateofBilling { get; set; }
        public string AmountPaid { get; set; }
    }
}