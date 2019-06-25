using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.SuperAdmin
{
    public class AdvancedBookingFromMemberController : Controller
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();
        // GET: AdvancedBookingFromMember
        [Authorize(Roles = "1,2")]
        public ActionResult Index()
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var time = DateTime.Now.ToString("H.mm");
            // Last Query --- >> var query = "select r.ID as UserStayID,r.UserID as UserID,r.RoomID as RoomID, r.CheckInDate as Checkin,r.CheckoutDate as Checkout,tr.RoomNo as RoomNo,tr.RoomCost as RoomCost,tr.Room_Type as RoomType,r.NonMemberName as NonMember,r.NonMemberPhoneNo as NonMemberPhone,case when (p.L_Name!=null) then p.F_Name+ ' ' + p.L_Name else p.F_Name end as MemberName,p.PhoneNo as MemberPhoneNo from tbl_UsersStay as r join tbl_Rooms as tr on tr.ID = r.RoomID left join tbl_Profile as p on r.UserID = p.ID where Cast(r.CheckoutDate as datetime) >=Cast('" + DateFormat + "' as datetime)";
            var query = "select rd.RoomID,rd.ID,rb.UserID,rb.Booking_ID,rb.Check_In,rb.Check_Out,rb.Bill_Number from tbl_RoomBooking as rb join tbl_RoomBooking_Details as rd on rb.Booking_ID = rd.Booking_ID where Cast(rb.Check_Out as datetime) >=Cast('" + DateFormat + "' as datetime)";
            List<AdvancedInfoOfRoomBooking> bookedRooms = new List<AdvancedInfoOfRoomBooking>();
            var data = context.Database.SqlQuery<AdvancedInfoOfRoomBooking>(query);
            if (data != null)
            {
                foreach (var i in data)
                {
                    var memProfile = context.tbl_Profile.SingleOrDefault(p => p.ID == i.UserID);
                    bookedRooms.Add(new AdvancedInfoOfRoomBooking()
                    {
                        ID = i.ID,
                        RoomID = i.RoomID,
                        Check_In = i.Check_In,
                        Check_Out = i.Check_Out,
                        RoomNo = (context.tbl_Rooms.SingleOrDefault(r => r.ID == i.RoomID)) != null ? context.tbl_Rooms.SingleOrDefault(r => r.ID == i.RoomID).RoomNo : "",
                        MemberName = memProfile != null ? memProfile.F_Name + " " + memProfile.L_Name : "",
                        MemberPhoneNo = memProfile != null ? memProfile.PhoneNo : "",
                        UserID = i.UserID,
                        Booking_ID = i.Booking_ID,
                        Bill_Number=i.Bill_Number
                    });
                }
            }

            ViewBag.RoomBookingInfo = bookedRooms;


            //var tblQuery = "select t.TableID as TableID,t.BookingDate as BookingDate,t.BookingStartTime as BookingStartTime,t.BookingEndTime as BookingEndTime,tb.TableNo as TableNo,pr.F_Name+ ' ' + pr.L_Name as MemberName,pr.PhoneNo as MemberPhoneNo from tbl_BookingTable as t join tbl_TablesForBooking as tb on tb.ID = t.TableID join tbl_Profile as pr on pr.ID = t.UserID where t.BookingDate >= '" + DateFormat + "' and t.BookingEndTime > '" + time + "'";
            //List<AdvancedInfoOfTableBooking> bookedTables = new List<AdvancedInfoOfTableBooking>();
            //var data1 = context.Database.SqlQuery<AdvancedInfoOfTableBooking>(tblQuery);
            //if (data1 != null)
            //{
            //    foreach (var i in data1)
            //    {
            //        bookedTables.Add(new AdvancedInfoOfTableBooking()
            //        {
            //            TableID=i.TableID,
            //            TableNo=i.TableNo,
            //            BookingDate=i.BookingDate,
            //            BookingStartTime=i.BookingStartTime,
            //            BookingEndTime=i.BookingEndTime,
            //            MemberName = i.MemberName,
            //            MemberPhoneNo = i.MemberPhoneNo
            //        });
            //    }
            //}

            //ViewBag.TableBookingInfo = bookedTables;           
            return View();
        }

        public JsonResult getOtherDetailsOfUser(int stayID)
        {

            var data = context.tbl_RoomBooking_Details.SingleOrDefault(s => s.ID == stayID);
            if (data != null)
            {
                var jsonResult = new
                {
                    NightStays = (context.tbl_RoomBooking.SingleOrDefault(s => s.Booking_ID == data.Booking_ID)) != null ? context.tbl_RoomBooking.SingleOrDefault(s => s.Booking_ID == data.Booking_ID).NoOfNightStays : 0,
                    ComplementaryStays = (data.ComplementaryStays != null) ? data.ComplementaryStays : 0,
                    Amount = data.TAmtPerRoom
                };
                return Json(jsonResult);
            }
            return Json("");
            
        }

        [Authorize(Roles = "1")]
        public ActionResult EditRoomBookingOfUser(int userStayID)
        {
            string name = "";
            var data = context.tbl_RoomBooking_Details.SingleOrDefault(s => s.ID == userStayID);
            RoomBooking_Details stayData = new RoomBooking_Details();
            if (data != null)
            {
                var d = context.tbl_RoomBooking.SingleOrDefault(s => s.Booking_ID == data.Booking_ID);
                stayData.ID = data.ID;
                stayData.Check_In = d != null ? d.Check_In : "";
                stayData.Check_Out = d != null ? d.Check_Out : "";
                stayData.RoomID = data.RoomID;
                stayData.RoomNo = context.tbl_Rooms.SingleOrDefault(r => r.ID == data.RoomID).RoomNo;
                stayData.ComplementaryStays = data.ComplementaryStays;
                stayData.UserID = d != null ? d.UserID : 0;
                stayData.NoOfPerson = data.NoOfPerson;
                stayData.TAmtPerRoom = data.TAmtPerRoom;
                stayData.ExtraBed = data.ExtraBed;

                stayData.Customer = context.tbl_Profile.SingleOrDefault(p => p.ID == d.UserID).F_Name + " " + context.tbl_Profile.SingleOrDefault(p => p.ID == d.UserID).L_Name;
            }                       
            return View(stayData);
        }

        [HttpPost]
        public JsonResult UpdateRoomBookingOfUserByStayID(RoomBooking_Details stayData)
        {
            var data = context.tbl_RoomBooking_Details.SingleOrDefault(s => s.ID == stayData.ID);
            if (data != null)
            {                              
                data.NoOfPerson = stayData.NoOfPerson;
                data.RoomID = stayData.RoomID;
                data.ComplementaryStays = stayData.ComplementaryStays;
                data.TAmtPerRoom = stayData.TAmtPerRoom;
                data.ExtraBed = stayData.ExtraBed;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();

                var roomBooking = context.tbl_RoomBooking.SingleOrDefault(s => s.Booking_ID == data.Booking_ID);
                if (roomBooking != null)
                {
                    roomBooking.Check_In = stayData.Check_In;
                    roomBooking.Check_Out = stayData.Check_Out;
                    roomBooking.NoOfNightStays = stayData.NoOfNightStays;
                    roomBooking.Amount = calculateAmount(roomBooking.Booking_ID);
                    context.Entry(roomBooking).State = EntityState.Modified;
                    context.SaveChanges();
                }

            }
            return Json("UpdatedRoomBooking");
        }

        public double? calculateAmount(int? bookingID)
        {
            var data = context.tbl_RoomBooking_Details.Where(s => s.Booking_ID == bookingID).ToList();
            double? amt = 0;
            foreach (var d in data)
            {
                amt += d.TAmtPerRoom;
            }
            return amt;
        }

        [HttpPost]
        public JsonResult DeleteRoomBookingOfUserByStayID(int stayID, int bookingID)
        {
            var stayData = context.tbl_RoomBooking_Details.SingleOrDefault(s => s.ID == stayID);
            if (stayData != null)
            {
                context.Entry(stayData).State = EntityState.Deleted;
                context.SaveChanges();
            }
            var lst = context.tbl_RoomBooking_Details.Where(s => s.Booking_ID == bookingID).ToList();
            var roomBooking = context.tbl_RoomBooking.SingleOrDefault(s => s.Booking_ID == bookingID);
            if (lst.Count > 0)
            {
                if (roomBooking != null)
                {
                    roomBooking.Amount = calculateAmount(roomBooking.Booking_ID);
                    context.Entry(roomBooking).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            else
            {
                if (roomBooking != null)
                {
                    context.Entry(roomBooking).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
            return Json("Deleted");
        }
        
        //      -----------------------------------  COMMENTED ON 29-May-2018  -------------------------------

        //public JsonResult getMembershipDetailsbyUserID(int id,int stayID)
        //{
        //    int? TotalStays = 0;
        //    int? pStay = 0;
        //    int? lStay = 0;
        //    var usermember = context.tbl_UserMembership.SingleOrDefault(s => s.UserID == id);
        //    if (usermember != null)
        //    {
        //        var date = DateTime.Today;
        //        string currentDate = date.ToString(@"MM\/dd\/yyyy");

        //        string[] arrDate = usermember.MembershipExpiryDate.Split('/');

        //        DateTime MemShipExpiryDate = new DateTime(Convert.ToInt32(arrDate[2]), Convert.ToInt32(arrDate[0]), Convert.ToInt32(arrDate[1]));

        //        if (DateTime.Today > MemShipExpiryDate)
        //        {
        //            lStay = -2;
        //        }
        //        else
        //        {

        //            TotalStays = context.tbl_MembershipPlans.SingleOrDefault(a => a.ID == usermember.MembershipID).NoOfStays;
        //            var data1 = context.Database.SqlQuery<UserStays>("exec staysAccToYearlyMembership @userID={0},@membershipID={1}", id, usermember.MembershipID).ToList<UserStays>();

        //            var filteredData = (data1.Where(p => p.ID != stayID)).ToList();
        //            foreach (var i in filteredData)
        //            {
        //                pStay = pStay + i.ComplementaryStays;
        //            }

        //            if ((TotalStays - pStay) > 0)
        //            {
        //                lStay = TotalStays - pStay;
        //            }
        //        }

        //    }
        //    else
        //    {
        //        lStay = -1;
        //    }

        //    var jsonResult = new
        //    {
        //        LapseStays = lStay,
        //        TakenStays = pStay,
        //        TotalStays = TotalStays,
        //        Member = (context.tbl_Profile.SingleOrDefault(w => w.ID == id).Walk_In_Member) == false ? "Yes" : "No"
        //    };
        //    return Json(jsonResult);
        //}


        [HttpPost]
        public JsonResult checkRoomsAvailablity(string checkIn, string checkOut,int UserStayID)
        {
            var data = getAvailableRoomsList(checkIn, checkOut, UserStayID);
            return Json(data);
        }

        public List<Rooms> getAvailableRoomsList(string checkIn, string checkOut, int UserStayID)
        {
            // LAST QUERY --->>>  var query = "select * from tbl_Rooms r where r.ID not in (select b.RoomID from tbl_UsersStay b where (cast(b.CheckInDate as date) <= (cast('" + checkIn + "' as date)) and cast(b.CheckoutDate as date)>=(cast('" + checkIn + "' as date))) or (cast(b.CheckInDate as date)<=cast('" + checkOut + "' as date) and cast(b.CheckoutDate as date)>=cast('" + checkOut + "' as date)) or (cast(b.CheckInDate as date) >= (cast('" + checkIn + "' as date)) and cast(b.CheckoutDate as date) <=cast('" + checkOut + "' as date)))";
            //var bookedRoom = "select * from tbl_UsersStay b where b.RoomID in (select r.ID from tbl_Rooms r where (cast(b.CheckInDate as date) <= (cast('" + checkIn + "' as date)) and cast(b.CheckoutDate as date)>=(cast('" + checkIn + "' as date))) or (cast(b.CheckInDate as date)<=cast('" + checkOut + "' as date) and cast(b.CheckoutDate as date)>=cast('" + checkOut + "' as date)) or (cast(b.CheckInDate as date) >= (cast('" + checkIn + "' as date)) and cast(b.CheckoutDate as date) <=cast('" + checkOut + "' as date))) and b.ID='" + UserStayID + "'";
            
            var query = "select r.* from tbl_Rooms r where r.ID not in (select rd.RoomID from tbl_RoomBooking_Details rd join tbl_RoomBooking rb on rd.Booking_ID=rb.Booking_ID where (cast(rb.Check_In as date) <= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date)>=(cast('" + checkIn + "' as date))) or (cast(rb.Check_In as date)<=cast('" + checkOut + "' as date) and cast(rb.Check_Out as date)>=cast('" + checkOut + "' as date)) or (cast(rb.Check_In as date) >= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date) <=cast('" + checkOut + "' as date)))";
            var bookedRoom = "select b.* from tbl_RoomBooking_Details b join tbl_RoomBooking rb on rb.Booking_ID=b.Booking_ID where b.RoomID in (select r.ID from tbl_Rooms r where (cast(rb.Check_In as date) <= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date)>=(cast('" + checkIn + "' as date))) or (cast(rb.Check_In as date)<=cast('" + checkOut + "' as date) and cast(rb.Check_Out as date)>=cast('" + checkOut + "' as date)) or (cast(rb.Check_In as date) >= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date) <=cast('" + checkOut + "' as date))) and b.ID=" + UserStayID;

            //var checkedOutRooms = "select r.* from tbl_Rooms r where (r.ID  in (select rd.RoomID from tbl_RoomBooking_Details rd join tbl_RoomBooking rb on rd.Booking_ID=rb.Booking_ID and rb.Bill_Number IS NOT NULL where (cast(rb.Check_In as date) <= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date)>=(cast('" + checkIn + "' as date))) or (cast(rb.Check_In as date)<=cast('" + checkOut + "' as date) and cast(rb.Check_Out as date)>=cast('" + checkOut + "' as date)) or (cast(rb.Check_In as date) >= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date) <=cast('" + checkOut + "' as date))))";
            var checkedOutRooms = context.Database.SqlQuery<tbl_Rooms>("exec Members_Checked_Out_Rooms @checkin={0},@checkout={1}", checkIn, checkOut).ToList<tbl_Rooms>();
            List<Rooms> lst = new List<Rooms>();
            var data = context.Database.SqlQuery<tbl_Rooms>(query);
            var getBookedRoom = context.Database.SqlQuery<tbl_RoomBooking_Details>(bookedRoom);
            //var data1 = context.Database.SqlQuery<tbl_Rooms>(checkedOutRooms);
            if (data != null)
            {
                foreach (var i in data)
                {
                    lst.Add(new Rooms()
                    {
                        ID = i.ID,
                        RoomCost = i.RoomCost,
                        Room_Type = i.Room_Type,
                        RoomNo = i.RoomNo
                    });
                }
            }
            if (getBookedRoom != null)
            {
                foreach (var rr in getBookedRoom)
                {
                    lst.Add(new Rooms()
                    {
                        ID = rr.RoomID,
                        RoomNo = context.tbl_Rooms.SingleOrDefault(r => r.ID == rr.RoomID).RoomNo
                    });
                }
            }
            if (checkedOutRooms != null)
            {
                foreach (var i in checkedOutRooms)
                {
                    lst.Add(new Rooms()
                    {
                        ID = i.ID,
                        RoomCost = i.RoomCost,
                        Room_Type = i.Room_Type,
                        RoomNo = i.RoomNo
                    });
                }
            }
            return lst.OrderBy(a=>a.ID).ToList();
        }

        [Authorize(Roles = "1,2")]
        public ActionResult PreviousBookings(string sDate = "", string eDate = "")
        {
            if (sDate != "" && eDate != "")
            {
                ViewBag.StartDate = sDate;
                ViewBag.EndDate = eDate;
                ViewBag.ReportPage = "Yes";
            }
            return View();
        }

        public DateTime StartOfWeek(DateTime d)
        {
            if (d == DateTime.MinValue)
            {
                return d;
            }
            var result = d.DayOfWeek - DayOfWeek.Sunday;

            if (result < 0)
            {
                result += 7;
            }

            return d.AddDays(result * -1);
        }

        public ActionResult BookingsBillsDataAccToDay(int day = 1, string sDate = "", string eDate = "")
        {
            DateTime startdate = DateTime.Now;
            DateTime LastDate = DateTime.Now;

            switch (day)
            {
                //Today
                case (2):
                    startdate = DateTime.Today;
                    LastDate = DateTime.Now;
                    break;
                //Yesterday
                case (3):
                    startdate = DateTime.Today.AddDays(-1);
                    LastDate = DateTime.Today.AddDays(-1);
                    break;
                //ThisWeek
                case (4):
                    startdate = StartOfWeek(DateTime.Now);
                    LastDate = DateTime.Now;
                    break;
                //ThisMonth
                case (5):
                    startdate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    LastDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                    break;
                //This Year
                case (6):
                    var year = DateTime.Now.Year;
                    startdate = new DateTime(year, 1, 1);
                    LastDate = new DateTime(year, 12, DateTime.DaysInMonth(year, 12));
                    break;
                //Custom
                case (7):
                    string[] arrDate = sDate.Split('/');
                    string[] arrDate1 = eDate.Split('/');
                    startdate = new DateTime(Convert.ToInt32(arrDate[2]), Convert.ToInt32(arrDate[0]), Convert.ToInt32(arrDate[1]));
                    LastDate = new DateTime(Convert.ToInt32(arrDate1[2]), Convert.ToInt32(arrDate1[0]), Convert.ToInt32(arrDate1[1]));
                    break;
                default:
                    startdate = new DateTime(1900, 1, 1);
                    LastDate = DateTime.Now;
                    break;
            }
            string startDateFormat = startdate.ToString(@"MM\/dd\/yyyy");
            string LastDateFormat = LastDate.ToString(@"MM\/dd\/yyyy");

            //var roomBills = "select m.* from tbl_RoomBilling m where cast(m.CheckInDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.CheckInDate as date)<=cast('" + LastDateFormat + "' as date)";

            var roomBills = "select rb.* from tbl_RoomBooking rb  where cast(rb.Check_In as date)>=cast('" + startDateFormat + "' as date) and cast(rb.Check_In as date)<=cast('" + LastDateFormat + "' as date)  and rb.Bill_Number IS NOT NULL";
                //var roomBills = context.tbl_RoomBilling.ToList();
            var data = context.Database.SqlQuery<RoomBooking>(roomBills);
            List<RoomBooking> bookedRoomsBills = new List<RoomBooking>();
                double? finaltotalVal = 0;
                double? finalcsgst = 0;

                if (data.Count() > 0)
                {
                    foreach (var i in data)
                    {
                        var prf = context.tbl_Profile.SingleOrDefault(b => b.ID == i.UserID);
                    bookedRoomsBills.Add(new RoomBooking()
                    {
                        Booking_ID = i.Booking_ID,
                        Check_In = i.Check_In,
                        Check_Out = i.Check_Out,
                        Customer = prf != null ? (prf.F_Name + " " + ((string.IsNullOrEmpty(prf.L_Name)) ? "" : prf.L_Name)) : "",
                        UserID = i.UserID,
                        //Total = i.Amount + i.GST,
                        Total = i.Amount,
                        AmtToBePaid = i.AmtToBePaid,
                        Bill_Number = i.Bill_Number,
                        Discount = i.Discount,
                        AdvancedPayment = i.AdvancedPayment,
                        GST = i.GST,
                        Amount = i.Amount,
                        Mode_Of_Payment = i.Mode_Of_Payment,
                        Billing_DateTime = i.Billing_DateTime
                        });
                        //finaltotalVal += i.Amount - i.Discount;
                        finaltotalVal += i.Amount - i.Discount - i.GST;
                        finalcsgst += i.GST;
                    }

                    ViewBag.TotalAmount = Math.Round((Double)finaltotalVal, 2);
                    ViewBag.CSGST = Math.Round((Double)finalcsgst, 2);
                }
                return PartialView("~/Views/AdvancedBookingFromMember/_BookingsBillsDataAccToDay.cshtml", bookedRoomsBills);
            }

        public JsonResult RoomBookingDetailsByBookingID(int bookingID)
        {
            var bookingData = context.tbl_RoomBooking.SingleOrDefault(s => s.Booking_ID == bookingID);
            RoomBooking booking = new RoomBooking();
            List<RoomBooking_Details> details = new List<RoomBooking_Details>();
            if (bookingData != null) {
                var prf = context.tbl_Profile.SingleOrDefault(b => b.ID == bookingData.UserID);

                booking.Booking_ID = bookingID;
                booking.AdvancedPayment = bookingData.AdvancedPayment;
                booking.Amount = bookingData.Amount;
                booking.AmtToBePaid = bookingData.AmtToBePaid;
                booking.Bill_Number = bookingData.Bill_Number;
                booking.Check_In = bookingData.Check_In;
                booking.Check_Out = bookingData.Check_Out;
                booking.Customer = prf != null ? (prf.F_Name + " " + ((string.IsNullOrEmpty(prf.L_Name)) ? "" : prf.L_Name)) : "";
                booking.Discount = bookingData.Discount;
                booking.GST = bookingData.GST;
                booking.NoOfNightStays = bookingData.NoOfNightStays;

                var roomDet = context.tbl_RoomBooking_Details.Where(b => b.Booking_ID == bookingID).ToList();
                foreach (var i in roomDet)
                {
                    details.Add(new RoomBooking_Details()
                    {
                        ID = i.ID,
                        RoomID = i.RoomID,
                        RoomNo = context.tbl_Rooms.SingleOrDefault(a => a.ID == i.RoomID).RoomNo,
                        NoOfPerson = i.NoOfPerson,
                        ExtraBed = i.ExtraBed,
                        ComplementaryStays = i.ComplementaryStays,
                        TAmtPerRoom = i.TAmtPerRoom
                    });
                }
                booking.roomDetails = details;
            }
            return Json(booking);
        }
    }
}
