using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class RoomBooking
    {
        //public int ID { get; set; }
        //public Nullable<int> RoomID { get; set; }
        //public string Check_In { get; set; }
        //public string Check_Out { get; set; }
        //public Nullable<int> UserID { get; set; }
        public int Booking_ID { get; set; }
        public double paymentModeCash { get; set; }
        public double paymentModePaytm { get; set; }
        public double paymentModeCard { get; set; }
        public double paymentModeCheque { get; set; }
        public Nullable<double> AdvancedPayment { get; set; }
        public string Check_In { get; set; }
        public string Check_Out { get; set; }
        public Nullable<double> Amount { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> MemberID { get; set; }
        public Nullable<int> NoOfNightStays { get; set; }
        public Nullable<double> Discount { get; set; }
        public Nullable<double> GST { get; set; }
        public Nullable<double> AmtToBePaid { get; set; }
        public Nullable<int> Bill_Number { get; set; }

        public string TempBillNo { get; set; }

        public string Customer { get; set; }
        public Nullable<double> Total { get; set; }
        public Nullable<System.DateTime> Billing_DateTime { get; set; }

        public List<RoomBooking_Details> roomDetails { get; set; }

        public string Mode_Of_Payment { get; set; }
        public string Cheque_No { get; set; }
        public string BankName { get; set; }
    }

    public class RoomBooking_Details
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();


        public int ID { get; set; }
        public Nullable<int> Booking_ID { get; set; }
        public Nullable<int> RoomID { get; set; }
        public Nullable<int> NoOfPerson { get; set; }
        public Nullable<bool> ExtraBed { get; set; }
        public Nullable<int> ComplementaryStays { get; set; }
        public Nullable<double> TAmtPerRoom { get; set; }

        public string Check_In { get; set; }
        public string Check_Out { get; set; }
        public string RoomNo { get; set; }
        public Nullable<int> UserID { get; set; }
        public string Customer { get; set; }
        public Nullable<int> PreviousStays { get; set; }
        public Nullable<int> LapseStays { get; set; }
        public Nullable<int> TotalStays { get; set; }
        public string MemberOrNot { get; set; }
        public Nullable<int> NoOfNightStays { get; set; }

        //public Nullable<int> NoOfNightStays { get; set; }


        public RoomBooking_Details getMembershipDetailsbyUserID(int? userId, int stayID)
        {
            RoomBooking_Details us = new RoomBooking_Details();
            int? TotalStays = 0;
            int? pStay = 0;
            int? lStay = 0;
            var usermember = context.tbl_UserMembership.SingleOrDefault(s => s.UserID == userId);
            if (usermember != null)
            {
                //var date = DateTime.Today;
                //string currentDate = date.ToString(@"MM\/dd\/yyyy");

                string[] arrDate = usermember.MembershipExpiryDate.Split('/');

                DateTime MemShipExpiryDate = new DateTime(Convert.ToInt32(arrDate[2]), Convert.ToInt32(arrDate[0]), Convert.ToInt32(arrDate[1]));

                if (DateTime.Today > MemShipExpiryDate)
                {
                    lStay = -2;
                }
                else
                {

                    TotalStays = context.tbl_MembershipPlans.SingleOrDefault(a => a.ID == usermember.MembershipID).NoOfStays;
                    var data1 = context.Database.SqlQuery<RoomBooking_Details>("exec staysAccToYearlyMembership @userID={0},@membershipID={1}", userId, usermember.MembershipID).ToList<RoomBooking_Details>();

                    var filteredData = (data1.Where(p => p.ID != stayID)).ToList();
                    foreach (var i in filteredData)
                    {
                        pStay = pStay + i.ComplementaryStays;
                    }

                    if ((TotalStays - pStay) > 0)
                    {
                        lStay = TotalStays - pStay;
                    }
                }
            }
            else
            {
                lStay = -1;
            }
            us.LapseStays = lStay;
            us.PreviousStays = pStay;
            us.TotalStays = TotalStays;
            us.MemberOrNot = (context.tbl_Profile.SingleOrDefault(w => w.ID == userId).Walk_In_Member) == false ? "Yes" : "No";

            return us;
        }
    }
}