using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.SuperAdmin
{
    public class RoomBillingController : Controller
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();

        [Authorize(Roles = "1,2")]
        public ActionResult RoomBillingSection()
        {
            var data = context.tbl_RoomBooking.ToList();
            List<RoomBooking> lst = new List<RoomBooking>();
            //if (data.Count > 0)
            //{
            //    int? maxVal = data.Max(t => t.Bill_Number);
            //    if (maxVal != null)
            //    {
            //        ViewBag.BillNumber = maxVal + 1;
            //    }
            //    else
            //    {
            //        ViewBag.BillNumber = 1;
            //    }
            //-----------------------------------
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var data1 = data.Where(s => s.Check_In == DateFormat).ToList();

            //var hashset = new HashSet<string>();
            foreach (var i in data1)
            {
                var prf = context.tbl_Profile.SingleOrDefault(b => b.ID == i.UserID);
                //if (hashset.Add(i.UserID.ToString()))
                //{
                lst.Add(new RoomBooking()
                    {
                        Booking_ID = i.Booking_ID,
                        Customer = prf != null ? (prf.F_Name + " " + ((string.IsNullOrEmpty(prf.L_Name)) ? "" : prf.L_Name)) : "",
                    });
                //}

            }
            // }

            return View(lst);
        }

        [HttpPost]
        public JsonResult getCustomerRoomDetails(int Booking_ID, string checkIn)
        {
            //int? discount = -1;
            var bill_number = 0;
            //var str = "";
            //double? TotalAmt = 0;
            var Rooms = "";
            double? advancedPayment = 0;
            var data = context.tbl_RoomBooking.SingleOrDefault(u => u.Booking_ID == Booking_ID && u.Check_In == checkIn);

            if (data != null)
            {
                var bookingDetails = context.tbl_RoomBooking_Details.Where(f => f.Booking_ID == data.Booking_ID);
                //TotalAmt = data.Amount;
                advancedPayment = data.AdvancedPayment;
                foreach (var i in bookingDetails)
                {
                    var roomNo = context.tbl_Rooms.SingleOrDefault(s => s.ID == i.RoomID).RoomNo;
                    Rooms += roomNo + ",";
                }

                if (Rooms.Length > 1)
                    Rooms = Rooms.Substring(0, Rooms.Length - 1);

                var jsonResult = new
                {
                    TotalAmount = data.Amount,
                    Rooms = Rooms,
                    PhoneNo = context.tbl_Profile.SingleOrDefault(p => p.ID == data.UserID).PhoneNo,
                    //str = str,
                    advancedPayment = advancedPayment,
                    Discount = data.Discount == null ? 0 : data.Discount,
                    Bill_Number = data.Bill_Number == null ? 0 : data.Bill_Number,
                    GST = data.GST == null ? 0 : data.GST,
                    AmtToBePaid = data.AmtToBePaid == null ? 0 : data.AmtToBePaid,
                    Amt_Paid_Or_Not = data.Bill_Number != null ? "Yes" : "No",
                    Payment_Mode=data.Mode_Of_Payment,
                    ChequeNo = data.Cheque_No != null ? data.Cheque_No : "",
                    BankName = data.BankName != null ? data.BankName : "",
                };
                return Json(jsonResult);
            }
            return Json("NotFound");
        }

        [HttpPost]
        public JsonResult getCustomerslistAccToDate(string checkinDate)
        {
            var data = context.tbl_RoomBooking.Where(s => s.Check_In == checkinDate).ToList();
            List<RoomBooking> lst = new List<RoomBooking>();
            foreach (var i in data)
            {
                var prf = context.tbl_Profile.SingleOrDefault(b => b.ID == i.UserID);
                lst.Add(new RoomBooking()
                {
                    Booking_ID = i.Booking_ID,
                    Customer = prf != null ? (prf.F_Name + " " + ((string.IsNullOrEmpty(prf.L_Name)) ? "" : prf.L_Name)) : ""
                });
            }
            return Json(lst);
        }

        public JsonResult addRoomBillingDetails(RoomBooking model)
        {
            var bookingDet = context.tbl_RoomBooking.SingleOrDefault(f => f.Booking_ID == model.Booking_ID);
            if (bookingDet != null)
            {
                if (bookingDet.Bill_Number == null)
                {
                    var data = context.tbl_RoomBooking.ToList();
                    int? maxVal = data.Max(t => t.Bill_Number);
                    if (maxVal != null)
                    {
                        model.Bill_Number = maxVal + 1;
                    }
                    else
                    {
                        model.Bill_Number = 1;
                    }
                    bookingDet.Bill_Number = model.Bill_Number;
                    bookingDet.GST = model.GST;
                    bookingDet.Discount = model.Discount;
                    bookingDet.Mode_Of_Payment = model.Mode_Of_Payment;
                    bookingDet.Cheque_No = model.Cheque_No;
                    bookingDet.BankName = model.BankName;
                    //bookingDet.AdvancedPayment = model.AdvancedPayment;
                    bookingDet.AmtToBePaid = model.AmtToBePaid;
                    bookingDet.Billing_DateTime = DateTime.Now;
                    context.Entry(bookingDet).State = EntityState.Modified;
                    context.SaveChanges();

                    if ( model.paymentModeCash > 0)
                    {
                        tbl_RoomBillingMode Model = new tbl_RoomBillingMode();
                        Model.Room_Bill_Number = model.Booking_ID;
                        Model.Amout = model.paymentModeCash;
                        Model.Mode_Of_Pay = "Cash";
                        context.tbl_RoomBillingMode.Add(Model);
                        context.SaveChanges();

                    }
                    if (model.paymentModePaytm > 0)
                    {
                        tbl_RoomBillingMode Model = new tbl_RoomBillingMode();
                        Model.Room_Bill_Number = model.Booking_ID;
                        Model.Amout = model.paymentModePaytm;
                        Model.Mode_Of_Pay = "Paytm";
                        context.tbl_RoomBillingMode.Add(Model);
                        context.SaveChanges();

                    }
                    if (model.paymentModeCard > 0)
                    {
                        tbl_RoomBillingMode Model = new tbl_RoomBillingMode();
                        Model.Room_Bill_Number = model.Booking_ID;
                        Model.Amout = model.paymentModeCard;
                        Model.Mode_Of_Pay = "Card";
                        context.tbl_RoomBillingMode.Add(Model);
                        context.SaveChanges();

                    }
                    if (model.paymentModeCheque > 0)
                    {
                        tbl_RoomBillingMode Model = new tbl_RoomBillingMode();
                        Model.Room_Bill_Number = model.Booking_ID;
                        Model.Amout = model.paymentModeCheque;
                        Model.Mode_Of_Pay = "Cheque";
                        context.tbl_RoomBillingMode.Add(Model);
                        context.SaveChanges();

                    }
                }
            }
            List<RoomBooking_Details> stayData = new List<RoomBooking_Details>();
            var roomData = context.tbl_RoomBooking_Details.Where(s => s.Booking_ID == model.Booking_ID);
            foreach (var i in roomData)
            {
                stayData.Add(new RoomBooking_Details()
                {
                    Check_In = bookingDet.Check_In,
                    Check_Out = bookingDet.Check_Out,
                    RoomNo = context.tbl_Rooms.SingleOrDefault(s => s.ID == i.RoomID).RoomNo,
                    TAmtPerRoom = i.TAmtPerRoom,
                    NoOfPerson = i.NoOfPerson
                });
            }
            var jsonResult = new
            {
                stayData = stayData,
                BillNo = bookingDet.Bill_Number,
                csgst = bookingDet.GST,
                discount = bookingDet.Discount,
                AmtToBePaid = bookingDet.AmtToBePaid
            };
            return Json(jsonResult);
        }

        //[HttpPost]
        //public JsonResult getCustomerRoomDetails(int userID, string checkIn)
        //{
        //    int? discount = -1;
        //    var bill_number = 0;
        //    var data = context.tbl_UsersStay.Where(u => u.UserID == userID && u.CheckInDate == checkIn).ToList();
        //    var t = context.tbl_RoomBilling.FirstOrDefault(f => f.UserID == userID && f.CheckInDate == checkIn);
        //    if (t != null)
        //    {
        //        discount = t.Discount;
        //        bill_number = t.ID;
        //    }

        //    var str = "";

        //    if (data.Count() == 0)
        //    {
        //        str = "No Data Available.";
        //    }

        //    double? TotalAmt = 0;
        //    var Rooms = "";
        //    double? advancedPayment = 0;
        //    foreach (var i in data)
        //    {
        //        TotalAmt += i.TotalAmountWithoutTax;
        //        var roomNo = context.tbl_Rooms.SingleOrDefault(s => s.ID == i.RoomID).RoomNo;
        //        Rooms += roomNo + ",";
        //        advancedPayment = i.AdvancedPayment;
        //    }

        //    if (Rooms.Length > 1)
        //        Rooms = Rooms.Substring(0, Rooms.Length - 1);

        //    var jsonResult = new
        //    {
        //        TotalAmount = TotalAmt,
        //        Rooms = Rooms,
        //        PhoneNo = context.tbl_Profile.SingleOrDefault(p => p.ID == userID).PhoneNo,
        //        str = str,
        //        advancedPayment = advancedPayment,
        //        Discount = discount,
        //        Bill_Number = bill_number
        //    };
        //    return Json(jsonResult);
        //}

        //[Authorize(Roles = "1,2")]
        //public ActionResult RoomBillingSection()
        //{
        //    var data1 = context.tbl_RoomBilling.ToList().LastOrDefault();
        //    if (data1 != null)
        //    {
        //        ViewBag.BillNumber = data1.ID + 1;
        //    }
        //    else
        //    {
        //        ViewBag.BillNumber = 1;
        //    }
        //    var date = DateTime.Today;
        //    string DateFormat = date.ToString(@"MM\/dd\/yyyy");
        //    var data = context.tbl_UsersStay.Where(s => s.CheckInDate == DateFormat).ToList();

        //    //var t = (from user in data
        //    //         where !context.tbl_RoomBilling.Any(f => f.UserID == user.UserID && f.CheckInDate == user.CheckInDate)
        //    //         select new
        //    //         {
        //    //             UserID = user.UserID,
        //    //             RoomID = user.RoomID,
        //    //             F_Name=context.tbl_Profile.SingleOrDefault(a=>a.ID==user.UserID).F_Name,
        //    //             L_Name = context.tbl_Profile.SingleOrDefault(a => a.ID == user.UserID).L_Name
        //    //         }).ToList();

        //    List<Profile> lst = new List<Profile>();

        //    //var data = context.tbl_Profile.OrderBy(s => s.F_Name).ToList();
        //    var hashset = new HashSet<string>();
        //    foreach (var i in data)
        //    {
        //        if (hashset.Add(i.UserID.ToString()))
        //        {
        //            lst.Add(new Profile()
        //            {
        //                ID = i.UserID.GetValueOrDefault(),
        //                F_Name = context.tbl_Profile.SingleOrDefault(b => b.ID == i.UserID).F_Name,
        //                L_Name = context.tbl_Profile.SingleOrDefault(b => b.ID == i.UserID).L_Name
        //            });
        //        }

        //    }

        //    return View(lst);
        //}

        //[HttpPost]
        //public JsonResult getCustomerRoomDetails(int userID, string checkIn)
        //{
        //    int? discount = -1;
        //    var bill_number = 0;
        //    var data = context.tbl_UsersStay.Where(u => u.UserID == userID && u.CheckInDate == checkIn).ToList();
        //    var t = context.tbl_RoomBilling.FirstOrDefault(f => f.UserID == userID && f.CheckInDate == checkIn);
        //    if (t != null)
        //    {
        //        discount = t.Discount;
        //        bill_number = t.ID;
        //    }
        //    //var t = (from user in data
        //    //         where !context.tbl_RoomBilling.Any(f => f.UserID == user.UserID && f.CheckInDate == user.CheckInDate)
        //    //         select new
        //    //         {
        //    //             TotalAmountWithoutTax = user.TotalAmountWithoutTax,
        //    //             RoomID = user.RoomID,
        //    //             AdvancedPayment=user.AdvancedPayment
        //    //         }).ToList();

        //    var str ="";
        //    //if (data.Count() > 0 && t.Count() == 0)
        //    //{
        //    //    str = "Payment Paid.";
        //    //}
        //    //else 
        //    if (data.Count() == 0) //&& t.Count() == 0)
        //    {
        //        str = "No Data Available.";
        //    }

        //    double? TotalAmt = 0;
        //    var Rooms = "";
        //    double? advancedPayment = 0;
        //    foreach (var i in data)
        //    {
        //        TotalAmt += i.TotalAmountWithoutTax;
        //        var roomNo = context.tbl_Rooms.SingleOrDefault(s => s.ID == i.RoomID).RoomNo;
        //        Rooms += roomNo + ",";
        //        advancedPayment = i.AdvancedPayment;
        //    }

        //    if (Rooms.Length > 1)
        //        Rooms = Rooms.Substring(0, Rooms.Length - 1);

        //    var jsonResult = new
        //    {
        //        TotalAmount = TotalAmt,
        //        Rooms = Rooms,
        //        PhoneNo=context.tbl_Profile.SingleOrDefault(p=>p.ID==userID).PhoneNo,
        //        str = str,
        //        advancedPayment = advancedPayment,
        //        Discount = discount,
        //        Bill_Number = bill_number
        //    };
        //    return Json(jsonResult);
        //}

        //[HttpPost]
        //public JsonResult getCustomerslistAccToDate(string checkinDate)
        //{

        //    var data = context.tbl_UsersStay.Where(s => s.CheckInDate == checkinDate).ToList();

        //    //var t = (from user in data
        //    //         where !context.tbl_RoomBilling.Any(f => f.UserID == user.UserID && f.CheckInDate == user.CheckInDate)
        //    //         select new
        //    //         {
        //    //             UserID = user.UserID,
        //    //             RoomID = user.RoomID,
        //    //             F_Name = context.tbl_Profile.SingleOrDefault(a => a.ID == user.UserID).F_Name,
        //    //             L_Name = context.tbl_Profile.SingleOrDefault(a => a.ID == user.UserID).L_Name
        //    //         }).ToList();

        //    List<Profile> lst = new List<Profile>();

        //    //var data = context.tbl_Profile.OrderBy(s => s.F_Name).ToList();
        //    var hashset = new HashSet<string>();
        //    foreach (var i in data)
        //    {
        //        if (hashset.Add(i.UserID.ToString()))
        //        {
        //            lst.Add(new Profile()
        //            {
        //                ID = i.UserID.GetValueOrDefault(),
        //                F_Name = context.tbl_Profile.SingleOrDefault(a => a.ID == i.UserID).F_Name,
        //                L_Name = context.tbl_Profile.SingleOrDefault(a => a.ID == i.UserID).L_Name
        //            });
        //        }

        //    }
        //    return Json(lst);
        //}

        ////public JsonResult getRoomslist(string checkinDate)
        ////{
        ////    var data = context.tbl_UsersStay.Where(s => s.CheckInDate == checkinDate).ToList();

        ////    var t = (from user in data
        ////             where !context.tbl_RoomBilling.Any(f => f.RoomID == user.RoomID && f.CheckInDate == user.CheckInDate)
        ////             select new
        ////             {
        ////                 ID = user.RoomID
        ////             }).ToList();


        ////    List<Rooms> lst = new List<Rooms>();
        ////    foreach (var i in t)
        ////    {
        ////        lst.Add(new Rooms()
        ////        {
        ////            RoomNo = context.tbl_Rooms.SingleOrDefault(a => a.ID == i.ID).RoomNo,
        ////            ID = i.ID
        ////        });
        ////    }
        ////    return Json(lst);
        ////}

        ////public JsonResult getCustomerDetailsbyRoomID(int RooomId, string checkIn)
        ////{
        ////    var data = context.tbl_UsersStay.SingleOrDefault(a => a.RoomID == RooomId && a.CheckInDate == checkIn);
        ////    var CustomerName = "";
        ////    var CustomerPhoneNo = "";
        ////    var MemberOrNot = "";
        ////    double? Amount = 0;
        ////    if (data != null)
        ////    {
        ////        var customer = context.tbl_Profile.SingleOrDefault(s => s.ID == data.UserID);
        ////        Amount = data.TotalAmountWithoutTax;

        ////        if (customer != null)
        ////        {
        ////            if (customer.Walk_In_Member == false)
        ////            {
        ////                MemberOrNot = "Yes";
        ////            }
        ////            else
        ////            {
        ////                MemberOrNot = "No";
        ////            }
        ////            CustomerName = customer.F_Name + " " + customer.L_Name;
        ////            CustomerPhoneNo = CustomerPhoneNo = customer.PhoneNo;

        ////        }
        ////        else
        ////        {
        ////            CustomerName = data.NonMemberName;
        ////            CustomerPhoneNo = data.NonMemberPhoneNo;
        ////            MemberOrNot = "No";
        ////        }
        ////    }

        ////    var jsonResult = new
        ////    {
        ////        CustomerName = CustomerName,
        ////        CustomerPhoneNo = CustomerPhoneNo,
        ////        MemberOrNot = MemberOrNot,
        ////        Amount = Amount
        ////    };
        ////    return Json(jsonResult);
        ////}

        ////public JsonResult getCustomerDetailsbyRoomID(int RooomId, string checkOut)
        ////{
        ////    var data = context.tbl_UsersStay.SingleOrDefault(a => a.RoomID == RooomId && a.CheckoutDate == checkOut);
        ////    var CustomerName = "";
        ////    var CustomerPhoneNo = "";
        ////    var MemberOrNot = "";
        ////    int? LapseStays = 0;
        ////    if (data != null)
        ////    {
        ////        var customer = context.tbl_Profile.SingleOrDefault(s => s.ID == data.UserID);


        ////        if (customer != null)
        ////        {
        ////            if (customer.Walk_In_Member == false)
        ////            {
        ////                MemberOrNot = "Yes";
        ////            }
        ////            else
        ////            {
        ////                MemberOrNot = "No";
        ////            }
        ////            CustomerName = customer.F_Name + " " + customer.L_Name;
        ////            CustomerPhoneNo = CustomerPhoneNo = customer.PhoneNo;


        ////            var usermember = context.tbl_UserMembership.SingleOrDefault(s => s.UserID == customer.ID);

        ////            if (usermember != null)
        ////            {
        ////                //var userstay = context.tbl_UsersStay.Where(a => a.UserID == customer.ID && a.MemberID == usermember.MembershipID).ToList();

        ////                //var query = "select us.* from tbl_UsersStay as us join tbl_UserMembership as um on us.UserID=" + customer.ID + " and um.MembershipID=" + usermember.MembershipID + " where Cast(us.CheckInDate as datetime)>=Cast(um.MembershipJoiningDate as datetime) and Cast(us.CheckoutDate as datetime)<=Cast(um.MembershipExpiryDate as datetime)";

        ////                //var query = "select (select count(*) from tbl_UsersStay US where Cast(US.CheckInDate as datetime)>=dateadd(year,datediff(YEAR,UM.MembershipJoiningDate,getdate()),UM.MembershipJoiningDate) and Cast(US.CheckInDate as datetime)<dateadd(year,(datediff(YEAR,UM.MembershipJoiningDate,getdate())+1),UM.MembershipJoiningDate) and US.userid=UM.UserID and US.memberid=UM.MembershipID) from tbl_UserMembership UM where UserID=" + customer.ID + " and MembershipID=" + usermember.MembershipID;
        ////                //var query = "select US.*,Case when UM.MembershipExpiryDate <getdate() then 'Expire' else 'NotExpire' end as Expire from tbl_UsersStay US join tbl_UserMembership UM on UM.UserID=" + customer.ID + " and UM.MembershipID=" + usermember.MembershipID + " where Cast(US.CheckInDate as datetime)>=dateadd(year,datediff(YEAR,UM.MembershipJoiningDate,getdate()),UM.MembershipJoiningDate) and Cast(US.CheckInDate as datetime)<dateadd(year,(datediff(YEAR,UM.MembershipJoiningDate,getdate())+1),UM.MembershipJoiningDate) and US.userid=UM.UserID and US.memberid=UM.MembershipID";
        ////                //var data1 = context.Database.SqlQuery<UserStays>(query);

        ////                //Get student name of string type

        ////                //if (usermember.MembershipPlanForYear > 1)
        ////                //{
        ////                //    TotalStays = TotalStays * usermember.MembershipPlanForYear;
        ////                //}

        ////                var date = DateTime.Today;
        ////                string currentDate = date.ToString(@"MM\/dd\/yyyy");
        ////                if (Convert.ToDateTime(currentDate) > Convert.ToDateTime(usermember.MembershipExpiryDate))
        ////                {
        ////                    LapseStays = -2;
        ////                }
        ////                else
        ////                {

        ////                    int? TotalStays = context.tbl_MembershipPlans.SingleOrDefault(a => a.ID == usermember.MembershipID).NoOfStays;
        ////                    var data1 = context.Database.SqlQuery<UserStays>("exec staysAccToYearlyMembership @userID={0},@membershipID={1}", customer.ID, usermember.MembershipID).ToList<UserStays>();

        ////                    int? pStay = 0;
        ////                    foreach (var i in data1)
        ////                    {
        ////                        pStay = pStay + i.NoOfNightStays;
        ////                    }

        ////                    if ((TotalStays - pStay) > 0)
        ////                    {
        ////                        LapseStays = TotalStays - pStay;
        ////                    }
        ////                }
        ////            }
        ////            else
        ////            {
        ////                LapseStays = -1;
        ////            }
        ////        }
        ////        else
        ////        {
        ////            CustomerName = data.NonMemberName;
        ////            CustomerPhoneNo = data.NonMemberPhoneNo;
        ////            MemberOrNot = "No";
        ////            LapseStays = -1;
        ////        }
        ////    }

        ////    var jsonResult = new
        ////    {
        ////        CustomerName = CustomerName,
        ////        CustomerPhoneNo = CustomerPhoneNo,
        ////        MemberOrNot = MemberOrNot,
        ////        LapseStays = LapseStays
        ////    };
        ////    return Json(jsonResult);
        ////}

        //public JsonResult addRoomBillingDetails(RoomBilling model)
        //{
        //    var t = context.tbl_RoomBilling.Any(f => f.UserID == model.UserID && f.CheckInDate == model.CheckinDate);
        //    if (!t)
        //    {
        //        tbl_RoomBilling details = new tbl_RoomBilling();
        //        details.ID = model.ID;
        //        details.CustomerName = model.CustomerName;
        //        details.PhoneNo = model.PhoneNo;
        //        details.RoomID = model.RoomID;
        //        details.Amount = model.Amount;
        //        details.Discount = model.Discount;
        //        //details.ExtraBedCharges = model.ExtraBedCharges;
        //        details.SGST = model.SGST;
        //        details.CGST = model.CGST;
        //        details.NetAmount = model.NetAmount;
        //        details.UserID = model.UserID;
        //        details.CheckInDate = model.CheckinDate;
        //        details.AdvancedPayment = model.AdvancedPayment;
        //        details.AmtToBePaid = model.AmtToBePaid;

        //        context.tbl_RoomBilling.Add(details);
        //        context.SaveChanges();
        //    }
        //    List<UserStays> stayData = new List<UserStays>();
        //    var data = context.tbl_UsersStay.Where(s => s.UserID == model.UserID && s.CheckInDate == model.CheckinDate);
        //    foreach (var i in data)
        //    {
        //        stayData.Add(new UserStays()
        //        {
        //            CheckInDate = i.CheckInDate,
        //            CheckoutDate = i.CheckoutDate,
        //            RoomNo = context.tbl_Rooms.SingleOrDefault(s => s.ID == i.RoomID).RoomNo,
        //            TotalAmountWithoutTax = i.TotalAmountWithoutTax,
        //            NoOfMembers=i.NoOfMembers
        //        });
        //    }

        //    return Json(stayData);
        //}
    }
}
