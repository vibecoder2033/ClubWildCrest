using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class Reports
    {
        public decimal Menus_Sale { get; set; }
        public decimal Menus_GST { get; set; }
        public decimal Menus_Discount { get; set; }
        public decimal Menus_Total { get; set; }

        public decimal Food_CardPay { get; set; }
        public decimal Food_CashPay { get; set; }
        public decimal Food_PaytmPay { get; set; }
        public decimal Food_ChequePay { get; set; }

        public decimal NonMenusGst_Sale { get; set; }
        public decimal NonMenusGst_Discount { get; set; }
        public decimal NonMenusGst_GST { get; set; }
        public decimal NonMenusGst_Total { get; set; }

        public decimal NonGst_Food_CardPay { get; set; }
        public decimal NonGst_Food_CashPay { get; set; }
        public decimal NonGst_Food_PaytmPay { get; set; }
        public decimal NonGst_Food_ChequePay { get; set; }
       

        public decimal Room_Sale { get; set; }
        public decimal Room_GST { get; set; }
        public decimal Room_Discount { get; set; }
        public decimal Room_Total { get; set; }

        public decimal Room_CashPay { get; set; }
        public decimal Room_CardPay { get; set; }
        public decimal Room_PaytmPay { get; set; }
        public decimal Room_ChequePay { get; set; }

        public decimal Total_Sale { get; set; }
        public decimal Total_Discount { get; set; }
        public decimal Total_GST { get; set; }
        public decimal Total_Cash { get; set; }
        public decimal Total_Paytm { get; set; }
        public decimal Total_Card { get; set; }
        public decimal Total_ChequePay { get; set; }
        public decimal Total_Amount { get; set; }

        public decimal MemBill_Sale { get; set; }
        public decimal MemBill_Discount { get; set; }
        public decimal MemBill_GST { get; set; }
        public decimal MemBill_Total { get; set; }
        public decimal MemBill_CardPay { get; set; }
        public decimal MemBill_CashPay { get; set; }
        public decimal MemBill_PaytmPay { get; set; }
        public decimal MemBill_ChequePay { get; set; }

        public decimal Bar_Sale { get; set; }
        public decimal Bar_GST { get; set; }
        public decimal Bar_Discount { get; set; }
        public decimal Bar_Total { get; set; }

        public decimal Bar_CardPay { get; set; }
        public decimal Bar_CashPay { get; set; }
        public decimal Bar_PaytmPay { get; set; }
        public decimal Bar_ChequePay { get; set; }


        public decimal NonGst_BarSale { get; set; }
        public decimal NonGst_BarDiscount { get; set; }
        public decimal NonGst_BarGST { get; set; }
        public decimal NonGst_BarTotal { get; set; }
       


        public decimal Wine_Sale { get; set; }
        public decimal Wine_GST { get; set; }
        public decimal Wine_Discount { get; set; }
        public decimal Wine_Total { get; set; }

        public decimal Wine_CardPay { get; set; }
        public decimal Wine_CashPay { get; set; }
        public decimal Wine_PaytmPay { get; set; }
        public decimal Wine_ChequePay { get; set; }

        public decimal NonGst_WineSale { get; set; }
        public decimal NonGst_WineDiscount { get; set; }
        public decimal NonGst_WineGST { get; set; }
        public decimal NonGst_WineTotal { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class RoomReports
    {        
        public decimal Room_Sale { get; set; }
        public decimal Room_GST { get; set; }
        public decimal Room_Discount { get; set; }
        public decimal Room_Total { get; set; }
    }

    public class MenusReports
    {
        public decimal Menus_Sale { get; set; }
        public decimal Menus_GST { get; set; }
        public decimal Menus_Discount { get; set; }
        public decimal Menus_Total { get; set; }

        public decimal Food_CardPay { get; set; }
        public decimal Food_CashPay { get; set; }
        public decimal Food_PaytmPay { get; set; }
        public decimal Food_ChequePay { get; set; }
    }

    public class NonMenusGstReports
    {
        public decimal NonMenusGst_Sale { get; set; }
        public decimal NonMenusGst_Discount { get; set; }
        public decimal NonMenusGst_GST { get; set; }
        public decimal NonMenusGst_Total { get; set; }
    }

    public class Members_MemShipReport
    {
        public decimal MemBill_Sale { get; set; }
        public decimal MemBill_Discount { get; set; }
        public decimal MemBill_GST { get; set; }
        public decimal MemBill_Total { get; set; }
    }

    public class BarReports
    {
        public decimal Bar_Sale { get; set; }
        public decimal Bar_GST { get; set; }
        public decimal Bar_Discount { get; set; }
        public decimal Bar_Total { get; set; }

        public decimal Bar_CardPay { get; set; }
        public decimal Bar_CashPay { get; set; }
        public decimal Bar_PaytmPay { get; set; }
        public decimal Bar_ChequePay { get; set; }
    }

    public class NonMenusBarReports
    {
        public decimal NonGst_BarSale { get; set; }
        public decimal NonGst_BarDiscount { get; set; }
        public decimal NonGst_BarGST { get; set; }
        public decimal NonGst_BarTotal { get; set; }
    }
    public class WineReports
    {
        public decimal Wine_Sale { get; set; }
        public decimal Wine_GST { get; set; }
        public decimal Wine_Discount { get; set; }
        public decimal Wine_Total { get; set; }

        public decimal Wine_CardPay { get; set; }
        public decimal Wine_CashPay { get; set; }
        public decimal Wine_PaytmPay { get; set; }
        public decimal Wine_ChequePay { get; set; }

    }

    public class NonMenusWineReports
    {
        public decimal NonGst_WineSale { get; set; }
        public decimal NonGst_WineDiscount { get; set; }
        public decimal NonGst_WineGST { get; set; }
        public decimal NonGst_WineTotal { get; set; }
    }
    public class WinePayment
    {
          public decimal Wine_CardPay { get; set; }
        public decimal Wine_CashPay { get; set; }
        public decimal Wine_PaytmPay { get; set; }
        public decimal Wine_ChequePay { get; set; }
    }
    public class BarPayment
    {
        public decimal Bar_CardPay { get; set; }
        public decimal Bar_CashPay { get; set; }
        public decimal Bar_PaytmPay { get; set; }
        public decimal Bar_ChequePay { get; set; }
    }
    public class FoodPayment
    {
        public decimal Food_CardPay { get; set; }
        public decimal Food_CashPay { get; set; }
        public decimal Food_PaytmPay { get; set; }
        public decimal Food_ChequePay { get; set; }
    }
    public class NonGstFoodPayment
    {
        public decimal NonGst_Food_CardPay { get; set; }
        public decimal NonGst_Food_CashPay { get; set; }
        public decimal NonGst_Food_PaytmPay { get; set; }
        public decimal NonGst_Food_ChequePay { get; set; }
    }
    public class RoomPayment
    {
        public decimal Room_CashPay { get; set; }
        public decimal Room_CardPay { get; set; }
        public decimal Room_PaytmPay { get; set; }
        public decimal Room_ChequePay { get; set; }
    }
    public class MemberPayment
    {
        public decimal MemBill_CardPay { get; set; }
        public decimal MemBill_CashPay { get; set; }
        public decimal MemBill_PaytmPay { get; set; }
        public decimal MemBill_ChequePay { get; set; }
    }
}