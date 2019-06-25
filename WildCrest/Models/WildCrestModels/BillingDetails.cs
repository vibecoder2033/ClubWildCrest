using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class BillingDetails
    {
        public int ID { get; set; }
        public Nullable<double> Amount_Paid { get; set; }
        public string Mode_Of_Payment { get; set; }
        public string Cheque_No { get; set; }
        //public string Transaction_No { get; set; }
        public string BankName { get; set; }

        public Nullable<int> UserStay_ID { get; set; }
        public Nullable<int> UserID { get; set; }

        public string Payment_Date { get; set; }

        public string F_Name { get; set; }
        public string L_Name { get; set; }

        public double? paymentModeCash { get; set; }
        public double? paymentModePaytm { get; set; }
        public double? paymentModeCard { get; set; }
        public double? paymentModeCheque { get; set; }
    }
}