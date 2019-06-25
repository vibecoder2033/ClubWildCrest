using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.SuperAdmin
{
    public class RoomBookingBySaController : Controller
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();

        [Authorize(Roles = "1,2")]
        public ActionResult RoomBooking()
        {
            //var members = context.tbl_Profile.Where(s => s.NewUsrBySuperApprv == true && s.DelUsrBySuperApprv == false && s.Walk_In_Member == false && s.UserType == 3).ToList();
            var members = context.tbl_Profile.Where(s => s.NewUsrBySuperApprv == true && s.DelUsrBySuperApprv == false && s.UserType == 3).OrderBy(a => a.F_Name).ToList();

            List<Profile> memList = new List<Profile>();
            foreach (var i in members)
            {
                memList.Add(new Profile()
                {
                    ID = i.ID,
                    F_Name = i.F_Name,
                    L_Name = i.L_Name,
                    Walk_In_Member = i.Walk_In_Member
                });
            }
            return View(memList);
        }

        [HttpPost]
        public JsonResult checkRoomsAvailablity(string checkIn, string checkOut)
        {
            var data = getAvailableRoomsList(checkIn, checkOut);
            return Json(data);
        }

        public List<Rooms> getAvailableRoomsList(string checkIn, string checkOut)
        {
            //Last Executed Query ----> var query = "select * from tbl_Rooms r where r.ID not in (select b.RoomID from tbl_UsersStay b where (cast(b.CheckInDate as date) <= (cast('" + checkIn + "' as date)) and cast(b.CheckoutDate as date)>=(cast('" + checkIn + "' as date))) or (cast(b.CheckInDate as date)<=cast('" + checkOut + "' as date) and cast(b.CheckoutDate as date)>=cast('" + checkOut + "' as date)) or (cast(b.CheckInDate as date) >= (cast('" + checkIn + "' as date)) and cast(b.CheckoutDate as date) <=cast('" + checkOut + "' as date)))";

            var query = "select r.* from tbl_Rooms r where r.ID not in (select rd.RoomID from tbl_RoomBooking_Details rd join tbl_RoomBooking rb on rd.Booking_ID=rb.Booking_ID where (cast(rb.Check_In as date) <= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date)>=(cast('" + checkIn + "' as date))) or (cast(rb.Check_In as date)<=cast('" + checkOut + "' as date) and cast(rb.Check_Out as date)>=cast('" + checkOut + "' as date)) or (cast(rb.Check_In as date) >= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date) <=cast('" + checkOut + "' as date)))";

            // Last Executed Query on 30_May_2018 ----> var checkedOutRooms = "select r.* from tbl_Rooms r where (r.ID  in (select rd.RoomID from tbl_RoomBooking_Details rd join tbl_RoomBooking rb on rd.Booking_ID=rb.Booking_ID and rb.Bill_Number IS NOT NULL where (cast(rb.Check_In as date) <= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date)>=(cast('" + checkIn + "' as date))) or (cast(rb.Check_In as date)<=cast('" + checkOut + "' as date) and cast(rb.Check_Out as date)>=cast('" + checkOut + "' as date)) or (cast(rb.Check_In as date) >= (cast('" + checkIn + "' as date)) and cast(rb.Check_Out as date) <=cast('" + checkOut + "' as date))))";

//            var checkedOutRooms = "select  r.* from tbl_Rooms r where (r.ID  in (select rd.RoomID from tbl_RoomBooking_Details rd  join tbl_RoomBooking rb on " +
//"rd.Booking_ID=rb.Booking_ID and rb.Bill_Number IS not NULL where (cast(rb.Check_In as date) <= (cast('" + checkIn + "' as date)) and " +
//"cast(rb.Check_Out as date)>=(cast('" + checkIn + "' as date))) or (cast(rb.Check_In as date)<=cast('" + checkOut + "' as date) and " +
//"cast(rb.Check_Out as date)>=cast('" + checkOut + "' as date)) or (cast(rb.Check_In as date) >= (cast('" + checkIn + "' as date)) and " +
//"cast(rb.Check_Out as date) <=cast('" + checkOut + "' as date)))) " +
//"EXCEPT " +
//"select  r.* from tbl_Rooms r where (r.ID  in (select rd.RoomID from tbl_RoomBooking_Details rd join tbl_RoomBooking rb on " +
//"rd.Booking_ID=rb.Booking_ID and rb.Bill_Number IS  NULL where (cast(rb.Check_In as date) <= (cast('" + checkIn + "' as date)) and " +
//"cast(rb.Check_Out as date)>=(cast('" + checkIn + "' as date))) or (cast(rb.Check_In as date)<=cast('" + checkOut + "' as date) and " +
//"cast(rb.Check_Out as date)>=cast('" + checkOut + "' as date)) or (cast(rb.Check_In as date) >= (cast('" + checkIn + "' as date)) and " +
//"cast(rb.Check_Out as date) <=cast('" + checkOut + "' as date))))";

            var checkedOutRooms = context.Database.SqlQuery<tbl_Rooms>("exec Members_Checked_Out_Rooms @checkin={0},@checkout={1}", checkIn, checkOut).ToList<tbl_Rooms>();

            List<Rooms> lst = new List<Rooms>();
            var data = context.Database.SqlQuery<tbl_Rooms>(query);
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
            return lst.OrderBy(a => a.ID).ToList();
        }

        public JsonResult getMembershipDetailsbyUserID(int id)
        {
            int? TotalStays = 0;
            int? pStay = 0;
            int? lStay = 0;
            int? MembershipID = 0;
            var usermember = context.tbl_UserMembership.SingleOrDefault(s => s.UserID == id);
            if (usermember != null)
            {
                //==========================================================================================

                //var date = DateTime.Today;
                //string currentDate = date.ToString(@"MM\/dd\/yyyy");

                string[] arrDate = usermember.MembershipExpiryDate.Split('/');
                //var tempDate = arrDate[1] + "/" + arrDate[0] + "/" + arrDate[2];

                DateTime MemShipExpiryDate = new DateTime(Convert.ToInt32(arrDate[2]), Convert.ToInt32(arrDate[0]), Convert.ToInt32(arrDate[1]));

                if (DateTime.Today > MemShipExpiryDate)
                {
                    lStay = -2;
                }
                else
                {

                    TotalStays = context.tbl_MembershipPlans.SingleOrDefault(a => a.ID == usermember.MembershipID).NoOfStays;
                    var data1 = context.Database.SqlQuery<RoomBooking_Details>("exec staysAccToYearlyMembership @userID={0},@membershipID={1}", id, usermember.MembershipID).ToList<RoomBooking_Details>();

                    foreach (var i in data1)
                    {
                        pStay = pStay + i.ComplementaryStays;
                    }

                    if ((TotalStays - pStay) > 0)
                    {
                        lStay = TotalStays - pStay;
                    }
                }

                //==========================================================================================




                //var userstay = context.tbl_UsersStay.Where(a => a.UserID == id && a.MemberID == usermember.MembershipID).ToList();

                //TotalStays = context.tbl_MembershipPlans.SingleOrDefault(a => a.ID == usermember.MembershipID).NoOfStays;
                //pStay = userstay.Count();
                //if ((TotalStays - pStay) > 0)
                //{
                //    lStay = TotalStays - pStay;
                //}
                MembershipID = usermember.MembershipID;
            }
            else
            {
                MembershipID = 0;
                lStay = -1;
            }

            var jsonResult = new
            {
                LapseStays = lStay,
                MembershipId = MembershipID,
                UserID = id,
                TakenStays = pStay,
                TotalStays = TotalStays,
                Member = (context.tbl_Profile.SingleOrDefault(w => w.ID == id).Walk_In_Member) == false ? "Yes" : "No"
            };
            return Json(jsonResult);
        }

        [HttpPost]
        public JsonResult addStayOfCustomer(string checkin, string checkout, string UserID, string membershipID, string memberName, string phno, int noOfNights, List<RoomBooking_Details> roomInfo, double? advancedPayment)
        {
            double? amt = 0;
            if (UserID == "0")
            {
                tbl_Profile prf = new tbl_Profile();
                prf.F_Name = memberName;
                prf.PhoneNo = phno;
                prf.Walk_In_Member = true;

                prf.Status = true;
                prf.UserType = 3;
                prf.AddedBy = Request.Cookies["AddedBy"].Value;
                prf.NewUsrBySuperApprv = true;
                prf.DelUsrBySuperApprv = false;
                prf.Reference_Of_Walk_In = "0";
                prf.EmailNotifications = true;
                context.tbl_Profile.Add(prf);
                context.SaveChanges();
                UserID = prf.ID.ToString();
            }
            foreach (var i in roomInfo)
            {
                amt += i.TAmtPerRoom;
            }
            tbl_RoomBooking usrStay = new tbl_RoomBooking();
            if (UserID != "0")
                usrStay.UserID = Convert.ToInt32(UserID);
            else
                usrStay.UserID = null;

            if (membershipID != "0" && membershipID != null)
                usrStay.MemberID = Convert.ToInt32(membershipID);
            else
                usrStay.MemberID = null;

            usrStay.Check_In = checkin;
            usrStay.Check_Out = checkout;

            usrStay.NoOfNightStays = noOfNights;

            usrStay.Amount = Math.Round((double)amt, 2);

            if (advancedPayment != null)
                usrStay.AdvancedPayment = advancedPayment;
            else
                usrStay.AdvancedPayment = 0;

            context.tbl_RoomBooking.Add(usrStay);
            context.SaveChanges();

            foreach (var i in roomInfo)
            {
                tbl_RoomBooking_Details roomDetails = new tbl_RoomBooking_Details();
                roomDetails.Booking_ID = usrStay.Booking_ID;
                roomDetails.RoomID = i.RoomID;
                roomDetails.NoOfPerson = i.NoOfPerson;
                roomDetails.ExtraBed = i.ExtraBed;
                roomDetails.ComplementaryStays = i.ComplementaryStays;
                roomDetails.TAmtPerRoom = i.TAmtPerRoom;
                context.tbl_RoomBooking_Details.Add(roomDetails);
                context.SaveChanges();
            }

            return Json("RoomBooked");
        }

        //[HttpPost]
        //public JsonResult addStayOfCustomer(string checkin, string checkout, string UserID, string membershipID, string memberName, string phno, int noOfNights, List<MultipleRoomBookings> roomInfo, double? advancedPayment)
        //{
        //    if (UserID == "0")
        //    {
        //        tbl_Profile prf = new tbl_Profile();
        //        prf.F_Name = memberName;
        //        prf.PhoneNo = phno;
        //        prf.Walk_In_Member = true;

        //        prf.Status = true;
        //        prf.UserType = 3;
        //        prf.AddedBy = Request.Cookies["AddedBy"].Value;
        //        prf.NewUsrBySuperApprv = true;
        //        prf.DelUsrBySuperApprv = false;
        //        prf.Reference_Of_Walk_In = "0";
        //        prf.EmailNotifications = true;
        //        context.tbl_Profile.Add(prf);
        //        context.SaveChanges();
        //        UserID = prf.ID.ToString();
        //    }
        //    foreach (var r in roomInfo)
        //    {
        //        tbl_UsersStay usrStay = new tbl_UsersStay();
        //        if (UserID != "0")
        //            usrStay.UserID = Convert.ToInt32(UserID);
        //        else
        //            usrStay.UserID = null;

        //        if (membershipID != "0" && membershipID != null)
        //            usrStay.MemberID = Convert.ToInt32(membershipID);
        //        else
        //            usrStay.MemberID = null;

        //        usrStay.CheckInDate = checkin;
        //        usrStay.CheckoutDate = checkout;
        //        usrStay.RoomID = r.roomId;
        //        if (memberName != "")
        //            usrStay.NonMemberName = memberName;
        //        else
        //            usrStay.NonMemberName = null;

        //        if (phno != "")
        //            usrStay.NonMemberPhoneNo = phno;
        //        else
        //            usrStay.NonMemberPhoneNo = null;

        //        if (r.noOfPerson != 0 && r.noOfPerson!= null)
        //            usrStay.NoOfMembers = r.noOfPerson;
        //        else
        //            usrStay.NoOfMembers = null;

        //        usrStay.NoOfNightStays = noOfNights;
        //        if (r.complementaryStays != null)
        //            usrStay.ComplementaryStays = Convert.ToInt32(r.complementaryStays);
        //        usrStay.TotalAmountWithoutTax = r.amtWithoutTax;

        //        if (advancedPayment != null)
        //            usrStay.AdvancedPayment = advancedPayment;
        //        else
        //            usrStay.AdvancedPayment = 0;

        //        context.tbl_UsersStay.Add(usrStay);
        //        context.SaveChanges();

        //    }

        //    return Json("RoomBooked");
        //}                

    }
}
