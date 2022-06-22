using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class MenusBillingSection
    {
        public int Bill_Number { get; set; }
        public string MenuBillNo { get; set; }
        public string Barbillno { get; set; }
        public string Winebillno { get; set; }
        public string Customer_Name { get; set; }
        public string Phone { get; set; }
        public string FoodItemName { get; set; }
        public Nullable<double> Price { get; set; }
        public Nullable<double> Quantity { get; set; }
        public int UserID { get; set; }

        public int ItemNameID { get; set; }
        public Nullable<int> FakeQuantity { get; set; }
        public Nullable<System.DateTime> FakeQtyAddedDate { get; set; }
        public string FakeQtyAddedBy { get; set; }

        public string PaymentDate { get; set; }
        public Nullable<double> GST { get; set; }
        public Nullable<double> PriceWithoutTax { get; set; }
        public Nullable<int> TableID { get; set; }
        public string Table_No { get; set; }
        public Nullable<System.TimeSpan> Order_Time { get; set; }
        public string OrderTakenBy { get; set; }
        public string Table_Status { get; set; }
        public string Mode_Of_Payment { get; set; }
        public Nullable<int> Billed_By { get; set; }
        public Nullable<double> Discount { get; set; }

        public List<MenusBillingDetailsWithBillNo> MenusBillingDetailsWithBillNo { get; set; }

        public int Temp_Day_Data { get; set; }
        public string Temp_Tax_Data { get; set; }
        public int Temp_AdminID_Data { get; set; }
        public string Temp_SDate_Data { get; set; }
        public string Temp_EndDate_Data { get; set; }
        public string Customer_GstNO { get; set; }
    }

    public class MenusBillingDetailsWithBillNo
    {
        public int ID { get; set; }
        public Nullable<int> BillNo { get; set; }
        public string FoodName { get; set; }
        public Nullable<double> Price { get; set; }
        public Nullable<double> Quantity { get; set; }
        public int ItemNameID{ get; set; }

        public Nullable<double> OldQuantity { get; set; }
        public string QtyUpdatedDate { get; set; }
        public string QtyUpdatedBy { get; set; }
    }
}