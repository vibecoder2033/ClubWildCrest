using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.SuperAdmin
{
    public class StaffController : Controller
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();
        [Authorize(Roles = "1,2")]
        public ActionResult Index()
        {
            List<Profile> lst = new List<Profile>();
            var staffList = context.tbl_Profile.OrderByDescending(a => a.ID).Where(s => s.UserType == 4 && s.NewUsrBySuperApprv != false && s.DelUsrBySuperApprv != true && s.Walk_In_Member == false).ToList();
            foreach (var i in staffList)
            {
                lst.Add(new Profile()
                {
                    ID = i.ID,
                    F_Name = i.F_Name,
                    L_Name = i.L_Name,
                    Address = i.Address,
                    Email = i.Email,
                    PhoneNo = i.PhoneNo,
                    DOB = i.DOB,
                    City = i.City,
                    State = i.State,
                    EmailNotifications = i.EmailNotifications,
                    UserType = i.UserType,
                    Status = i.Status,
                    AddedBy = i.AddedBy,
                    NewUsrBySuperApprv = i.NewUsrBySuperApprv,
                    DelUsrBySuperApprv = i.DelUsrBySuperApprv
                });
            }
            return View(lst);
        }

        [Authorize(Roles = "1")]
        public ActionResult NewStaffApprovalBySuperAdmin()
        {
            List<Profile> lst = new List<Profile>();
            var staffList = context.tbl_Profile.OrderByDescending(a => a.ID).Where(s => s.UserType == 4 && s.NewUsrBySuperApprv == false && s.Walk_In_Member == false).ToList();
            foreach (var i in staffList)
            {
                lst.Add(new Profile()
                {
                    ID = i.ID,
                    F_Name = i.F_Name,
                    L_Name = i.L_Name,
                    Address = i.Address,
                    Email = i.Email,
                    PhoneNo = i.PhoneNo,
                    DOB = i.DOB,
                    City = i.City,
                    State = i.State,
                    EmailNotifications = i.EmailNotifications,
                    UserType = i.UserType,
                    Status = i.Status,
                    AddedBy = i.AddedBy,
                    NewUsrBySuperApprv = i.NewUsrBySuperApprv,
                    DelUsrBySuperApprv = i.DelUsrBySuperApprv
                });
            }
            return View(lst);
        }

        [HttpPost]
        public JsonResult ApproveStaff(int id)
        {
            var data = context.tbl_Profile.SingleOrDefault(s => s.ID == id);
            if (data != null)
            {
                data.NewUsrBySuperApprv = true;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Approved");
        }

        [Authorize(Roles = "1")]
        public ActionResult DeleteStaffBySuperAdminApproval()
        {
            List<Profile> lst = new List<Profile>();
            var staffList = context.tbl_Profile.OrderByDescending(a => a.ID).Where(s => s.UserType == 4 && s.DelUsrBySuperApprv == true && s.Walk_In_Member == false).ToList();
            foreach (var i in staffList)
            {
                lst.Add(new Profile()
                {
                    ID = i.ID,
                    F_Name = i.F_Name,
                    L_Name = i.L_Name,
                    Address = i.Address,
                    Email = i.Email,
                    PhoneNo = i.PhoneNo,
                    DOB = i.DOB,
                    City = i.City,
                    State = i.State,
                    EmailNotifications = i.EmailNotifications,
                    UserType = i.UserType,
                    Status = i.Status,
                    AddedBy = i.AddedBy,
                    NewUsrBySuperApprv = i.NewUsrBySuperApprv,
                    DelUsrBySuperApprv = i.DelUsrBySuperApprv,
                    DeletedBy = i.DeletedBy
                });
            }
            return View(lst);
        }

        [HttpPost]
        public JsonResult CancelDeleteStaffById(int id)
        {
            var findUser = context.tbl_Profile.SingleOrDefault(u => u.ID == id);
            if (findUser != null)
            {
                findUser.DelUsrBySuperApprv = false;
                context.Entry(findUser).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("NotDeleted.");
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public ActionResult AddNewStaff()
        {
            return View();
        }

        [HttpPost]
        public JsonResult DeleteStaffByID(int id)
        {
            int currentUserTypeLoggedin = Convert.ToInt32(Request.Cookies["UserType"].Value);
            if (currentUserTypeLoggedin == 1)
            {
                var findUser = context.tbl_Profile.SingleOrDefault(u => u.ID == id);
                var usrInMemberTbl = context.tbl_UserMembership.SingleOrDefault(s => s.UserID == id);
                if (findUser != null)
                {
                    if (usrInMemberTbl != null)
                    {
                        context.Entry(usrInMemberTbl).State = EntityState.Deleted;
                        context.SaveChanges();
                    }
                    context.Entry(findUser).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
            else
            {
                var findUser = context.tbl_Profile.SingleOrDefault(u => u.ID == id);
                if (findUser != null)
                {
                    findUser.DelUsrBySuperApprv = true;
                    findUser.DeletedBy = Request.Cookies["AddedBy"].Value;
                    context.Entry(findUser).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            return Json(currentUserTypeLoggedin);
        }

        //[HttpPost]
        [Authorize(Roles = "1,2")]
        public ActionResult StaffDetailsByID(int id, string editDetail)
        {
            var data = context.tbl_Profile.SingleOrDefault(s => s.ID == id);
            Profile prf = new Profile();
            if (data != null)
            {
                var memPhoto = context.tbl_MembersPhoto.Where(p => p.UserID == id).ToList();
                var memDocs = context.tbl_MembersDocs.Where(p => p.UserID == id).ToList();

                var UserMembership = context.tbl_UserMembership.SingleOrDefault(a => a.UserID == id);

                prf.ID = data.ID;
                prf.AddedBy = data.AddedBy;
                prf.Address = data.Address;
                prf.City = data.City;
                prf.DOB = data.DOB;
                prf.Email = data.Email;
                prf.EmailNotifications = data.EmailNotifications;
                prf.F_Name = data.F_Name;
                prf.L_Name = data.L_Name;
                prf.FatherName = data.FatherName;
                //prf.Reference_Of_Walk_In = data.Reference_Of_Walk_In;
                prf.PhoneNo = data.PhoneNo;
                prf.State = data.State;
                prf.Status = data.Status;
                prf.AadharNo = data.AadharNo;
                prf.PAN_Card = data.PAN_Card;
                prf.Staff_JoiningDate = data.JoiningDate;
                //prf.CustomMemberID = data.CustomMemberID;

                //prf.Addon_Member = data.Addon_Member;
                //prf.Relationship = data.Relationship;
                prf.Additional_Notes = data.Additional_Notes;

                //var billingDetailsData = context.tbl_MembersBillingDetails.FirstOrDefault(f => f.UserID == id);
                //if (billingDetailsData != null)
                //{
                //    BillingDetails bill = new BillingDetails();
                //    bill.Mode_Of_Payment = billingDetailsData.Mode_Of_Payment;
                //    bill.Amount_Paid = billingDetailsData.TotalAmount;
                //    bill.Cheque_No = billingDetailsData.Cheque_No;
                //    bill.BankName = billingDetailsData.BankName;
                //    bill.UserID = id;
                //    bill.Payment_Date = billingDetailsData.Payment_Date;
                //    prf.billingDetailsInJson = JsonConvert.SerializeObject(bill);
                //}
                //else
                //{
                //    prf.billingDetailsInJson = "";
                //}

                //if (UserMembership != null)
                //{
                //    prf.MembershipPlanIDForUser = UserMembership.MembershipID;
                //    prf.MembershipPlanNameForUser = context.tbl_MembershipPlans.SingleOrDefault(a => a.ID == UserMembership.MembershipID).PlanName;
                //    prf.MembershipJoiningDate = UserMembership.MembershipJoiningDate;
                //    prf.MembershipExpiryDate = UserMembership.MembershipExpiryDate;
                //}
                //else
                //{
                //    prf.MembershipPlanIDForUser = 0;
                //}
                //var plans = context.tbl_MembershipPlans.ToList();
                //ViewBag.MembershipPlans = plans;
                //ViewBag.UserStays = getMembershipDetailsByUserID(data.ID);

                //------------------------------------------------------------------------------------------

                //List<KeyValuePair<int, int>> MembershipWithYear = new List<KeyValuePair<int, int>>();
                //foreach (var p in plans)
                //{
                //    MembershipWithYear.Add(new KeyValuePair<int, int>(p.ID, p.MembershipPlanForYear));
                //}
                //prf.MembershipWithYear = JsonConvert.SerializeObject(MembershipWithYear);
                //------------------------------------------------------------------------------------------

                ViewBag.MembersPhoto = memPhoto;
                ViewBag.MembersDOcs = memDocs;
            }
            if (editDetail == "Details")
            {
                return View("~/Views/Staff/StaffDetailsByID.cshtml", prf);
            }
            else
            {
                return View("~/Views/Staff/EditStaffByID.cshtml", prf);
            }

        }

        [Authorize(Roles = "1,2")]
        public ActionResult NewDelStaffDetailsByID(int id, string editDetail)
        {
            var data = context.tbl_Profile.SingleOrDefault(s => s.ID == id);
            Profile prf = new Profile();
            if (data != null)
            {
                //var UserMembership = context.tbl_UserMembership.SingleOrDefault(a => a.UserID == id);

                prf.ID = data.ID;
                prf.AddedBy = data.AddedBy;
                prf.Address = data.Address;
                prf.City = data.City;
                prf.DOB = data.DOB;
                prf.Email = data.Email;
                prf.EmailNotifications = data.EmailNotifications;
                prf.F_Name = data.F_Name;
                prf.L_Name = data.L_Name;
                //prf.Password = data.Password;
                prf.FatherName = data.FatherName;
                //prf.Reference_Of_Walk_In = data.Reference_Of_Walk_In;
                prf.PhoneNo = data.PhoneNo;
                prf.State = data.State;
                prf.Status = data.Status;
                prf.AadharNo = data.AadharNo;
                prf.PAN_Card = data.PAN_Card;
                //prf.CustomMemberID = data.CustomMemberID;

                //prf.Addon_Member = data.Addon_Member;
                //prf.Relationship = data.Relationship;
                prf.Additional_Notes = data.Additional_Notes;

                //var billingDetailsData = context.tbl_MembersBillingDetails.FirstOrDefault(f => f.UserID == id);
                //if (billingDetailsData != null)
                //{
                //    BillingDetails bill = new BillingDetails();
                //    bill.Mode_Of_Payment = billingDetailsData.Mode_Of_Payment;
                //    bill.Amount_Paid = billingDetailsData.TotalAmount;
                //    bill.Cheque_No = billingDetailsData.Cheque_No;
                //    bill.BankName = billingDetailsData.BankName;
                //    bill.UserID = id;
                //    bill.Payment_Date = billingDetailsData.Payment_Date;
                //    prf.billingDetailsInJson = JsonConvert.SerializeObject(bill);
                //}
                //else
                //{
                //    prf.billingDetailsInJson = "";
                //}

                //if (UserMembership != null)
                //{
                //    prf.MembershipPlanIDForUser = UserMembership.MembershipID;
                //    prf.MembershipPlanNameForUser = context.tbl_MembershipPlans.SingleOrDefault(a => a.ID == UserMembership.MembershipID).PlanName;
                //    prf.MembershipJoiningDate = UserMembership.MembershipJoiningDate;
                //    prf.MembershipExpiryDate = UserMembership.MembershipExpiryDate;
                //}
                //else
                //{
                //    prf.MembershipPlanIDForUser = 0;
                //}
                //var plans = context.tbl_MembershipPlans.ToList();
                //ViewBag.MembershipPlans = plans;
                //ViewBag.UserStays = getMembershipDetailsByUserID(data.ID);

                //------------------------------------------------------------------------------------------

                //List<KeyValuePair<int, int>> MembershipWithYear = new List<KeyValuePair<int, int>>();
                //foreach (var p in plans)
                //{
                //    MembershipWithYear.Add(new KeyValuePair<int, int>(p.ID, p.MembershipPlanForYear));
                //}
                //prf.MembershipWithYear = JsonConvert.SerializeObject(MembershipWithYear);
                //------------------------------------------------------------------------------------------
            }
            if (editDetail == "New")
            {
                ViewBag.NewDelMember = true;
                return View(prf);
            }
            else
            {
                ViewBag.NewDelMember = false;
                return View(prf);
            }
        }

        [HttpPost]
        public JsonResult UpdateDetailsByID(Profile prf)
        {
            var data = context.tbl_Profile.Find(prf.ID);
            if (data != null)
            {
                data.F_Name = prf.F_Name;
                data.L_Name = prf.L_Name;
                data.Address = prf.Address;
                data.Email = prf.Email;
                data.PhoneNo = prf.PhoneNo;
                data.City = prf.City;
                //data.Password = prf.Password;
                data.State = prf.State;
                data.AddedBy = Request.Cookies["AddedBy"].Value;
                data.UserType = prf.UserType;
                data.DOB = prf.DOB;
                // data.CustomMemberID = prf.CustomMemberID;
                data.FatherName = prf.FatherName;
                //data.Reference_Of_Walk_In = prf.Reference_Of_Walk_In;
                data.EmailNotifications = prf.EmailNotifications;
                data.JoiningDate = prf.Staff_JoiningDate;

                data.Additional_Notes = prf.Additional_Notes;
                //data.Addon_Member = prf.Addon_Member;
                //data.Relationship = prf.Relationship;

                if (prf.AadharNo != null && prf.AadharNo != "")
                    data.AadharNo = prf.AadharNo;

                if (prf.PAN_Card != null && prf.PAN_Card != "")
                    data.PAN_Card = prf.PAN_Card;


                //if (prf.MembershipPlanIDForUser != null && prf.MembershipPlanIDForUser != 0)
                //{
                //    var date = DateTime.Today;
                //    string DateFormat = date.ToString(@"MM\/dd\/yyyy");
                //    var modifyUserPlan = context.tbl_UserMembership.SingleOrDefault(m => m.UserID == data.ID);
                //    var getPlanYear = context.tbl_MembershipPlans.SingleOrDefault(py => py.ID == prf.MembershipPlanIDForUser);
                //    if (modifyUserPlan != null)
                //    {
                //        string[] expiredate = prf.MembershipJoiningDate.Split('/');
                //        var expireFullDate = new DateTime(Convert.ToInt32(expiredate[2]), Convert.ToInt32(expiredate[0]), Convert.ToInt32(expiredate[1]));

                //        modifyUserPlan.MembershipID = prf.MembershipPlanIDForUser;
                //        modifyUserPlan.MembershipJoiningDate = prf.MembershipJoiningDate;
                //        modifyUserPlan.MembershipExpiryDate = expireFullDate.AddYears(getPlanYear.MembershipPlanForYear).ToString(@"MM\/dd\/yyyy");
                //        //modifyUserPlan.MembershipPlanForYear = prf.PlanForYear;

                //        context.Entry(modifyUserPlan).State = EntityState.Modified;
                //        var delUserStay = context.tbl_UsersStay.Where(du => du.UserID == data.ID).ToList();
                //        if (delUserStay != null)
                //        {
                //            foreach (var d in delUserStay)
                //            {
                //                var delBillingDet = context.tbl_BillingDetails.Where(du => du.UserStay_ID == d.ID).ToList();
                //                if (delBillingDet != null)
                //                {
                //                    foreach (var b in delBillingDet)
                //                    {
                //                        context.Entry(b).State = EntityState.Deleted;
                //                    }
                //                }
                //                var delUsersOrders = context.tbl_UsersOrder.Where(du => du.UserStay_ID == d.ID).ToList();
                //                if (delUsersOrders != null)
                //                {
                //                    foreach (var u in delUsersOrders)
                //                    {
                //                        context.Entry(u).State = EntityState.Deleted;
                //                    }
                //                }
                //                var delMemWhileStaying = context.tbl_MembersWhileStayingWithUser.Where(du => du.UserStay_ID == d.ID).ToList();
                //                if (delMemWhileStaying != null)
                //                {
                //                    foreach (var m in delMemWhileStaying)
                //                    {
                //                        context.Entry(m).State = EntityState.Deleted;
                //                    }
                //                }

                //                context.Entry(d).State = EntityState.Deleted;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        tbl_UserMembership usrMember = new tbl_UserMembership();
                //        usrMember.UserID = data.ID;
                //        usrMember.MembershipID = prf.MembershipPlanIDForUser;
                //        usrMember.MembershipJoiningDate = DateFormat;
                //        usrMember.MembershipExpiryDate = date.AddYears(getPlanYear.MembershipPlanForYear).ToString(@"MM\/dd\/yyyy");
                //        //usrMember.MembershipPlanForYear = prf.PlanForYear;
                //        context.tbl_UserMembership.Add(usrMember);
                //    }
                //}
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
                //-----------------------------------------------------------------------------------------------------
                if (prf.MemberPhoto != null)
                {
                    string directoryPath = Server.MapPath("/images/MembersPhotoFolder/" + data.ID);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    var fileName = Path.GetFileName(prf.MemberPhoto.FileName);
                    string _name = Guid.NewGuid().ToString() + fileName;
                    var comPath = Server.MapPath("/images/MembersPhotoFolder/" + data.ID + "/") + _name;
                    var path = "/images/MembersPhotoFolder/" + data.ID + "/" + _name;
                    prf.MemberPhoto.SaveAs(comPath);
                    tbl_MembersPhoto photo = new tbl_MembersPhoto();
                    photo.UserID = data.ID;
                    photo.MemberImagePath = path;
                    context.tbl_MembersPhoto.Add(photo);
                    context.SaveChanges();
                }
                if (prf.MemberDocuments != null)
                {
                    for (int i = 0; i < prf.MemberDocuments.Count; i++)
                    {
                        HttpPostedFileBase file = prf.MemberDocuments[i];
                        string directoryPath = Server.MapPath("/images/MembersDocumentsFolder/" + data.ID);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        var fileName = Path.GetFileName(file.FileName);
                        string _name = Guid.NewGuid().ToString() + fileName;
                        var comPath = Server.MapPath("/images/MembersDocumentsFolder/" + data.ID + "/") + _name;
                        var path = "/images/MembersDocumentsFolder/" + data.ID + "/" + _name;
                        file.SaveAs(comPath);
                        tbl_MembersDocs photo = new tbl_MembersDocs();
                        photo.UserID = data.ID;
                        photo.MemberDocsPath = path;
                        context.tbl_MembersDocs.Add(photo);
                        context.SaveChanges();
                    }
                }
            }
            return Json("Updated.");
        }

        [HttpPost]
        public JsonResult AddNewStaff(Profile newMember)
        {
            //------------------------------for Custom MemberID-----------------------------
            var date = DateTime.Today;
            //var dateforcustomMemberID = date.ToString("yyyyMMdd");
            //var customMemberID = "";
            //var data = context.tbl_Profile.Where(o => o.Walk_In_Member != true && o.UserType == 3).ToList();
            //if (data.Count != 0)
            //{
            //    var lastRec = data.LastOrDefault();
            //    if (lastRec.CustomMemberID != null && lastRec.CustomMemberID != "")
            //    {
            //        if (lastRec.CustomMemberID.Count(x => x == '-') != 0)
            //        {
            //            customMemberID = "CWC" + dateforcustomMemberID + "001";
            //        }
            //        else
            //        {
            //            string dateSub = lastRec.CustomMemberID.Substring(3, 8);
            //            if (dateforcustomMemberID == dateSub)
            //            {
            //                string seriesSub = lastRec.CustomMemberID.Substring(11);
            //                int series = Convert.ToInt32(seriesSub);
            //                series += 1;
            //                customMemberID = "CWC" + dateSub + series.ToString("000");
            //            }
            //            else
            //            {
            //                customMemberID = "CWC" + dateforcustomMemberID + "001";
            //            }
            //        }
            //    }
            //    else
            //    {
            //        customMemberID = "CWC" + dateforcustomMemberID + "001";
            //    }
            //}
            //else
            //{
            //    customMemberID = "CWC" + dateforcustomMemberID + "001";
            //}

            //-----------------------------------------------------------
            int currentUserTypeLoggedin = Convert.ToInt32(Request.Cookies["UserType"].Value);
            tbl_Profile prf = new tbl_Profile();
            prf.F_Name = newMember.F_Name;
            prf.L_Name = newMember.L_Name;
            prf.Address = newMember.Address;
            prf.Email = newMember.Email;
            prf.PhoneNo = newMember.PhoneNo;
            prf.DOB = newMember.DOB;
            prf.City = newMember.City;
            prf.State = newMember.State;
            prf.Status = true;
            prf.EmailNotifications = newMember.EmailNotifications;
            prf.UserType = newMember.UserType;
            //prf.Password = newMember.Password;
            prf.FatherName = newMember.FatherName;
           // prf.Reference_Of_Walk_In = newMember.Reference_Of_Walk_In;
            //prf.CustomMemberID = customMemberID;
            prf.AddedBy = Request.Cookies["AddedBy"].Value;
            prf.Walk_In_Member = false;
            prf.Additional_Notes = newMember.Additional_Notes;
            //prf.Addon_Member = newMember.Addon_Member;
            //prf.Relationship = newMember.Relationship;
            prf.JoiningDate = newMember.Staff_JoiningDate;
            if (newMember.AadharNo != null && newMember.AadharNo != "")
                prf.AadharNo = newMember.AadharNo;

            if (newMember.PAN_Card != null && newMember.PAN_Card != "")
                prf.PAN_Card = newMember.PAN_Card;

            if (currentUserTypeLoggedin == 2)
            {
                prf.NewUsrBySuperApprv = false;
            }
            else
            {
                prf.NewUsrBySuperApprv = true;
            }
            prf.DelUsrBySuperApprv = false;
            context.tbl_Profile.Add(prf);
            context.SaveChanges();

            int id = prf.ID;
            //string expDate = "";
            //if (id != 0)
            //{
            //    //var date = DateTime.Today;
            //    string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            //    if (newMember.MembershipPlanIDForUser != 0 && newMember.MembershipPlanIDForUser != null)
            //    {
            //        string[] expiredate = newMember.MembershipJoiningDate.Split('/');
            //        var expireFullDate = new DateTime(Convert.ToInt32(expiredate[2]), Convert.ToInt32(expiredate[0]), Convert.ToInt32(expiredate[1]));
            //        var getMembershipPlanYear = context.tbl_MembershipPlans.SingleOrDefault(py => py.ID == newMember.MembershipPlanIDForUser);
            //        int planYear = getMembershipPlanYear.MembershipPlanForYear;
            //        tbl_UserMembership n = new tbl_UserMembership();
            //        n.UserID = id;
            //        n.MembershipID = newMember.MembershipPlanIDForUser;
            //        n.MembershipJoiningDate = newMember.MembershipJoiningDate;
            //        n.MembershipExpiryDate = expireFullDate.AddYears(planYear).ToString(@"MM\/dd\/yyyy");
            //        //n.MembershipPlanForYear = newMember.PlanForYear;
            //        context.tbl_UserMembership.Add(n);
            //        context.SaveChanges();

            //        expDate = expireFullDate.AddYears(planYear).ToString(@"MM\/dd\/yyyy");
            //    }

            //    tbl_MembersBillingDetails bill = new tbl_MembersBillingDetails();

            //    var billingDetails = JsonConvert.DeserializeObject<BillingDetails>(newMember.billingDetailsInJson);


            //    bill.Mode_Of_Payment = billingDetails.Mode_Of_Payment;
            //    bill.TotalAmount = billingDetails.Amount_Paid;

            //    if (billingDetails.Cheque_No != "" && billingDetails.Cheque_No != null)
            //        bill.Cheque_No = billingDetails.Cheque_No;
            //    else
            //        bill.Cheque_No = null;

            //    if (billingDetails.BankName != "" && billingDetails.BankName != null)
            //        bill.BankName = billingDetails.BankName;
            //    else
            //        bill.BankName = null;

            //    bill.UserID = id;

            //    bill.Payment_Date = DateFormat;

            //    context.tbl_MembersBillingDetails.Add(bill);
            //    context.SaveChanges();

                //===================================================================================

                if (newMember.MemberPhoto != null)
                {
                    string directoryPath = Server.MapPath("/images/MembersPhotoFolder/" + id);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    var fileName = Path.GetFileName(newMember.MemberPhoto.FileName);
                    string _name = Guid.NewGuid().ToString() + fileName;
                    var comPath = Server.MapPath("/images/MembersPhotoFolder/" + id + "/") + _name;
                    var path = "/images/MembersPhotoFolder/" + id + "/" + _name;
                    newMember.MemberPhoto.SaveAs(comPath);
                    tbl_MembersPhoto photo = new tbl_MembersPhoto();
                    photo.UserID = id;
                    photo.MemberImagePath = path;
                    context.tbl_MembersPhoto.Add(photo);
                    context.SaveChanges();
                }
                if (newMember.MemberDocuments != null)
                {
                    for (int i = 0; i < newMember.MemberDocuments.Count; i++)
                    {
                        HttpPostedFileBase file = newMember.MemberDocuments[i];
                        string directoryPath = Server.MapPath("/images/MembersDocumentsFolder/" + id);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        var fileName = Path.GetFileName(file.FileName);
                        string _name = Guid.NewGuid().ToString() + fileName;
                        var comPath = Server.MapPath("/images/MembersDocumentsFolder/" + id + "/") + _name;
                        var path = "/images/MembersDocumentsFolder/" + id + "/" + _name;
                        file.SaveAs(comPath);
                        tbl_MembersDocs photo = new tbl_MembersDocs();
                        photo.UserID = id;
                        photo.MemberDocsPath = path;
                        context.tbl_MembersDocs.Add(photo);
                        context.SaveChanges();
                    }
                //}
                //===================================================================================


                //    HttpPostedFileBase imageFile = Request.Files["MemberPhoto"];
                //    if (imageFile != null)
                //    {
                //        string directoryPath = Server.MapPath("/MembersPhotoFolder/" + id);
                //        if (!Directory.Exists(directoryPath))
                //        {
                //            Directory.CreateDirectory(directoryPath);
                //        }
                //        var fileName = Path.GetFileName(imageFile.FileName);
                //        string _name = Guid.NewGuid().ToString() + fileName;
                //        var comPath = Server.MapPath("/MembersPhotoFolder/" + id + "/") + _name;
                //        var path = "/MembersPhotoFolder/" + id + "/" + _name;
                //        imageFile.SaveAs(comPath);
                //        tbl_MembersPhoto photo = new tbl_MembersPhoto();
                //        photo.UserID = id;
                //        photo.MemberImagePath = path;
                //        context.tbl_MembersPhoto.Add(photo);
                //        context.SaveChanges();
                //    }
                //}
            }
            var result = new
            {
                currentUserTypeLoggedin = currentUserTypeLoggedin,
                //userID = id,
                //ExpiryDate = expDate
            };
            return Json(result);
        }

        //[HttpPost]
        //public JsonResult MembersDocumentsUpload(MembersDocs docs)
        //{
        //    HttpFileCollectionBase docFiles = Request.Files;
        //    for (int i = 0; i < docFiles.Count; i++)
        //    {
        //        HttpPostedFileBase file = docFiles[i];
        //        string directoryPath = Server.MapPath("/MembersDocumentsFolder/" + docs.UserID);
        //        if (!Directory.Exists(directoryPath))
        //        {
        //            Directory.CreateDirectory(directoryPath);
        //        }
        //        var fileName = Path.GetFileName(file.FileName);
        //        string _name = Guid.NewGuid().ToString() + fileName;
        //        var comPath = Server.MapPath("/MembersDocumentsFolder/" + docs.UserID + "/") + _name;
        //        var path = "/MembersDocumentsFolder/" + docs.UserID + "/" + _name;
        //        file.SaveAs(comPath);
        //        tbl_MembersDocs photo = new tbl_MembersDocs();
        //        photo.UserID = docs.UserID;
        //        photo.MemberDocsPath = path;
        //        context.tbl_MembersDocs.Add(photo);
        //        context.SaveChanges();

        //    }
        //    return Json("");
        //}

        public List<RoomBooking_Details> getMembershipDetailsByUserID(int userId)
        {
            List<RoomBooking_Details> usr = new List<RoomBooking_Details>();
            var a = context.tbl_UserMembership.SingleOrDefault(u => u.UserID == userId);
            if (a != null)
            {

                ViewBag.CurrentMembership = context.tbl_MembershipPlans.SingleOrDefault(m => m.ID == a.MembershipID).PlanName;

                //var query = "select US.* from tbl_UsersStay US join tbl_UserMembership UM on UM.UserID=" + userId + " and UM.MembershipID=" + a.MembershipID + " where Cast(US.CheckInDate as datetime)>=dateadd(year,datediff(YEAR,UM.MembershipJoiningDate,getdate()),UM.MembershipJoiningDate) and Cast(US.CheckInDate as datetime)<dateadd(year,(datediff(YEAR,UM.MembershipJoiningDate,getdate())+1),UM.MembershipJoiningDate) and US.userid=UM.UserID and US.memberid=UM.MembershipID";

                //var data = context.Database.SqlQuery<UserStays>(query);
                var data = context.Database.SqlQuery<RoomBooking_Details>("exec staysAccToYearlyMembership @userID={0},@membershipID={1}", userId, a.MembershipID).ToList<RoomBooking_Details>();

                int? TotalStays = context.tbl_MembershipPlans.SingleOrDefault(t => t.ID == a.MembershipID).NoOfStays;

                int? pStay = 0;
                int? lStay = 0;
                foreach (var i in data)
                {
                    var e = context.tbl_RoomBooking.SingleOrDefault(s => s.Booking_ID == i.Booking_ID);
                    if (e != null)
                    {
                        pStay = pStay + i.ComplementaryStays;

                        //usr.Add(new RoomBooking_Details()
                        //{
                        //    ID = i.ID,
                        //    Check_In = e.Check_In,
                        //    Check_Out = e.Check_Out,
                        //    NoOfPerson = i.NoOfPerson,
                        //    ComplementaryStays = i.ComplementaryStays
                        //});
                        bool alreadyExists = usr.Any(x => x.Booking_ID == i.Booking_ID);
                        if (!alreadyExists)
                        {
                            usr.Add(new RoomBooking_Details()
                            {
                                Booking_ID = i.Booking_ID,
                                Check_In = e.Check_In,
                                Check_Out = e.Check_Out,
                                NoOfPerson = i.NoOfPerson,
                                ComplementaryStays = i.ComplementaryStays,
                                RoomNo = context.tbl_Rooms.SingleOrDefault(r => r.ID == i.RoomID).RoomNo
                            });
                        }
                        else
                        {
                            usr.Find(x => x.Booking_ID == i.Booking_ID).ComplementaryStays += i.ComplementaryStays;
                            usr.Find(x => x.Booking_ID == i.Booking_ID).NoOfPerson += i.NoOfPerson;
                            usr.Find(x => x.Booking_ID == i.Booking_ID).RoomNo += ", " + context.tbl_Rooms.SingleOrDefault(r => r.ID == i.RoomID).RoomNo;
                        }
                    }
                }

                ViewBag.TotalStays = TotalStays;
                ViewBag.Complementary = pStay;
            }

            return usr;
        }

        [Authorize(Roles = "1,2")]
        public ActionResult AddMemberStay(int id)
        {
            //List<UserStays> UserstaysList = new List<UserStays>();
            int? TotalStays = 0;
            int? pStay = 0;
            int? lStay = 0;
            var usermember = context.tbl_UserMembership.SingleOrDefault(s => s.UserID == id);
            if (usermember != null)
            {
                var userstay = context.tbl_UsersStay.Where(a => a.UserID == id && a.MemberID == usermember.MembershipID).ToList();

                TotalStays = context.tbl_MembershipPlans.SingleOrDefault(a => a.ID == usermember.MembershipID).NoOfStays;
                pStay = userstay.Count();
                if ((TotalStays - pStay) > 0)
                {
                    lStay = TotalStays - pStay;
                }
                ViewBag.MembershipID = usermember.MembershipID;
            }
            else
            {
                ViewBag.MembershipID = 0;
            }
            ViewBag.PreviousStays = pStay;
            ViewBag.LapseStays = lStay;
            ViewBag.UserID = id;

            return View();
        }

        [HttpPost]
        public JsonResult addStay(int roomId, string checkin, string checkout, int UserID, int membershipID)
        {
            tbl_UsersStay usrStay = new tbl_UsersStay();
            usrStay.UserID = UserID;
            usrStay.MemberID = membershipID;
            usrStay.CheckInDate = checkin;
            usrStay.CheckoutDate = checkout;
            //usrStay.NoOfMembers = NoOfMembers;
            usrStay.RoomID = roomId;
            context.tbl_UsersStay.Add(usrStay);
            context.SaveChanges();
            int id = usrStay.ID;
            //ViewBag.UserStayID = id.ToString();
            return Json(id);
        }


        public ActionResult OtherInfo(int id, int UserID)
        {
            ViewBag.UserStayID = id.ToString();
            ViewBag.MemberID = UserID.ToString();
            return View();
        }

        [HttpPost]
        public JsonResult AddMemberWhileStaying(MembersWhileStayingWithUser member)
        {
            tbl_MembersWhileStayingWithUser tbl = new tbl_MembersWhileStayingWithUser();
            tbl.Name = member.Name;
            tbl.Age = member.Age;
            tbl.Gender = member.Gender;
            tbl.UserStay_ID = member.UserStay_ID;
            tbl.UserID = member.UserID;
            context.tbl_MembersWhileStayingWithUser.Add(tbl);
            context.SaveChanges();
            return Json("Saved");
        }

        [HttpPost]
        public JsonResult getMembersWhileStaying(int userStay_ID)
        {
            List<MembersWhileStayingWithUser> resList = new List<MembersWhileStayingWithUser>();
            var data = context.tbl_MembersWhileStayingWithUser.OrderByDescending(o => o.ID).Where(m => m.UserStay_ID == userStay_ID).ToList();
            if (data != null)
            {
                foreach (var i in data)
                {
                    resList.Add(new MembersWhileStayingWithUser()
                    {
                        ID = i.ID,
                        Name = i.Name,
                        Age = i.Age,
                        Gender = i.Gender
                    });
                }
            }
            return Json(resList);
        }

        [HttpPost]
        public JsonResult OrderWhileStaying(UsersOrder order)
        {
            tbl_UsersOrder tbl = new tbl_UsersOrder();
            tbl.FoodItemName = order.FoodItemName;
            tbl.UserStay_ID = order.UserStay_ID;
            tbl.Cost = order.Cost;
            tbl.Description = order.Description;
            tbl.UserID = order.UserID;
            context.tbl_UsersOrder.Add(tbl);
            context.SaveChanges();
            return Json("OrderSaved");
        }

        [HttpPost]
        public JsonResult getOrderWhileStaying(int userStay_ID)
        {
            List<UsersOrder> resList = new List<UsersOrder>();
            var data = context.tbl_UsersOrder.OrderByDescending(o => o.ID).Where(m => m.UserStay_ID == userStay_ID).ToList();
            if (data != null)
            {
                foreach (var i in data)
                {
                    resList.Add(new UsersOrder()
                    {
                        ID = i.ID,
                        FoodItemName = i.FoodItemName,
                        Cost = i.Cost,
                        Description = i.Description
                    });
                }
            }
            return Json(resList);
        }

        [HttpPost]
        public JsonResult AddBillingDetails(BillingDetails billingDetails)
        {
            tbl_BillingDetails bill = new tbl_BillingDetails();
            bill.Amount_Paid = billingDetails.Amount_Paid;
            bill.Cheque_No = billingDetails.Cheque_No;
            bill.Transaction_No = billingDetails.BankName;
            bill.Mode_Of_Payment = billingDetails.Mode_Of_Payment;
            bill.UserStay_ID = billingDetails.UserStay_ID;
            bill.UserID = billingDetails.UserID;
            context.tbl_BillingDetails.Add(bill);
            context.SaveChanges();
            return Json("Saved");
        }


        [HttpPost]
        public JsonResult checkRoomsAvailablity(string checkIn, string checkOut)
        {
            var data = getAvailableRoomsList(checkIn, checkOut);
            //TempData["AvailableRoomsList"] = lst;
            return Json(data);
        }

        public List<Rooms> getAvailableRoomsList(string checkIn, string checkOut)
        {
            //Last Executed Query ---->  var query = "SELECT r.* FROM tbl_Rooms r WHERE NOT EXISTS(SELECT 1 FROM tbl_UsersStay b WHERE b.RoomID=r.ID and (('" + checkIn + "' >= b.CheckInDate AND '" + checkIn + "' <= b.CheckoutDate) OR ('" + checkIn + "' <= b.CheckInDate AND '" + checkOut + "' >= b.CheckoutDate)))";
            var query = "select * from tbl_Rooms r where r.ID not in (select b.RoomID from tbl_UsersStay b where (cast(b.CheckInDate as date) <= (cast('" + checkIn + "' as date)) and cast(b.CheckoutDate as date)>=(cast('" + checkIn + "' as date))) or (cast(b.CheckInDate as date)<=cast('" + checkOut + "' as date) and cast(b.CheckoutDate as date)>=cast('" + checkOut + "' as date)) or (cast(b.CheckInDate as date) >= (cast('" + checkIn + "' as date)) and cast(b.CheckoutDate as date) <=cast('" + checkOut + "' as date)))";
            List<Rooms> lst = new List<Rooms>();
            var data = context.Database.SqlQuery<tbl_Rooms>(query);
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
            return lst;
        }

        public ActionResult RoomBooking(string checkIn, string checkOut, int userID, int membershipID)
        {
            //var data = TempData["AvailableRoomsList"] as List<Rooms>;
            var data = getAvailableRoomsList(checkIn, checkOut);
            ViewBag.CheckINDate = checkIn;
            ViewBag.CheckOutDate = checkOut;

            ViewBag.userID = userID;
            ViewBag.membershipID = membershipID;
            return View(data);
        }

    }
}
