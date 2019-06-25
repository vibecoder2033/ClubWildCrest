using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.EntryMembers
{
    public class EntryController : Controller
    {
        //
        // GET: /Entry/
        ClubWildCrestEntities context = new ClubWildCrestEntities();
        [Authorize(Roles = "1,2,4,5")]
        public ActionResult AddNewEntry()
        {
            return View();
        }
        public JsonResult AddEntry(tbl_EntryMembers model)
        {
            if (model != null&&ModelState.IsValid)
            {
                context.tbl_EntryMembers.Add(model);
                context.SaveChanges();
                return Json(model.ID);
            }
            return Json("not saved");
        }
        [Authorize(Roles = "1,2,4,5")]
        public ActionResult EntryBillsIndex()
        {
            List<EntryMemberModel> li = new List<EntryMemberModel>();
            var data = context.tbl_EntryMember_Billing.ToList();
            if (data != null)
            {
                foreach(var d in data)
                {
                    li.Add(new EntryMemberModel()
                    {
                        BillNo = d.Bill_ID,
                        Name=context.tbl_EntryMembers.SingleOrDefault(x=>x.ID==d.Member_ID).Name,
                        Phone_No= context.tbl_EntryMembers.SingleOrDefault(x => x.ID == d.Member_ID).MobileNo,
                        Total_Member= d.Members,
                        AmountPaid=d.Amount_Paid,
                        ModeOfPayment=d.Mode_Of_Payment,
                        DateofBilling=d.Date_Of_Billing
                    });
                }
            }
            return View(li);
        }
        public JsonResult SaveBill(string PayMode,int ID)
        {
            if (ID > 0)
            {
                
                if (context.tbl_EntryMember_Billing.Where(x => x.Member_ID == ID).Any())
                {
                    var data = context.tbl_EntryMember_Billing.Where(x => x.Member_ID == ID).FirstOrDefault();
                    EntryMemberModel li = new EntryMemberModel();
                    li.BillNo = data.Bill_ID;
                    li.Name = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == ID).Name;
                    li.Phone_No = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == ID).MobileNo;
                    li.Total_Member = data.Members;
                    li.AmountPaid = data.Amount_Paid;
                    li.ModeOfPayment = data.Mode_Of_Payment;
                    li.DateofBilling = data.Date_Of_Billing;
                    li.GstAmount = data.Gst_Amount;
                    li.Price_Without_Gst = data.Price_Without_Gst;
                    return Json(li);
                }
                else
                {
                    tbl_EntryMember_Billing model = new tbl_EntryMember_Billing();
                    model.Price_With_Gst = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == ID).EntryFee;
                    model.Gst_Amount = (Convert.ToDouble(model.Price_With_Gst) * (Convert.ToDouble(18) / (double)100)).ToString();
                    model.Price_Without_Gst = (Convert.ToDouble(model.Price_With_Gst) - Convert.ToDouble(model.Gst_Amount)).ToString();

                    model.Members = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == ID).qty;
                    model.Gst_Amount = (Convert.ToDouble(model.Gst_Amount) * Convert.ToDouble(model.Members)).ToString();
                    model.Total_Amount = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == ID).TotalAmount;
                    model.Amount_Paid = model.Total_Amount;
                    model.Mode_Of_Payment = PayMode;
                    model.Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);
                    model.Member_ID = ID;
                    model.Date_Of_Billing = DateTime.Now.ToString("dd/MM/yyyy");
                    context.tbl_EntryMember_Billing.Add(model);
                    context.SaveChanges();
                    EntryMemberModel li = new EntryMemberModel();
                    li.BillNo = model.Bill_ID;
                    li.Name = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == ID).Name;
                        li.Phone_No = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == ID).MobileNo;
                        li.Total_Member = model.Members;
                    li.AmountPaid = model.Amount_Paid;
                    li.ModeOfPayment = model.Mode_Of_Payment;
                    li.DateofBilling = model.Date_Of_Billing;
                    li.GstAmount = model.Gst_Amount;
                    li.Price_Without_Gst = model.Price_Without_Gst;
                 
                    return Json(li);
                }
            }
            return Json("not Found");
        }
        public ActionResult BillDetailsByBillNo(int id=0)
        {
            EntryMemberModel li = new EntryMemberModel();
            if (id > 0)
            {
                var data = context.tbl_EntryMember_Billing.SingleOrDefault(x => x.Bill_ID == id);
                li.BillNo = data.Bill_ID;
                li.Name = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == data.Member_ID).Name;
                li.Phone_No = context.tbl_EntryMembers.SingleOrDefault(x => x.ID == data.Member_ID).MobileNo;
                li.Total_Member = data.Members;
                li.AmountPaid = data.Amount_Paid;
                li.ModeOfPayment = data.Mode_Of_Payment;
                li.DateofBilling = data.Date_Of_Billing;
            }
            return View(li);
        }
    }
}
