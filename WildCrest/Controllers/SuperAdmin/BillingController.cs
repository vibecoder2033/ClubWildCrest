using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;


namespace WildCrest.Controllers.SuperAdmin
{
    public class BillingController : Controller
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();
        [Authorize(Roles = "1,2")]
        public ActionResult FoodBillingSection()
        {
            var data = context.tbl_MenusBillingSection.ToList().LastOrDefault();
            if (data != null)
            {
                ViewBag.BillNumber = data.Bill_Number + 1;
            }
            else
            {
                ViewBag.BillNumber = 1;
            }
            return View();
        }


        public JsonResult NonGst_BillNo()
        {
            var data = context.tbl_NonGST_MenusBillingSection.ToList().LastOrDefault();
            int NonGstBillNo = 1;
            if (data != null)
            {
                NonGstBillNo = data.NonGstBillNo + 1;
            }
            return Json(NonGstBillNo);
        }

        [HttpPost]
        public JsonResult GetItemNames(string prefix)
        {
            if (prefix != null && prefix != string.Empty)
            {
                var result = ((from u in context.tbl_Menu.Where(x => x.Food_Item_Name.Contains(prefix))
                               select new { ID = u.ID, Value = u.Food_Item_Name })).Distinct().ToList();
                return Json(result);
            }
            else
            {
                var result = ((from u in context.tbl_Menu
                               select new { ID = u.ID, Value = u.Food_Item_Name })).Distinct().ToList();
                return Json(result);

            }
        }

        [HttpPost]
        public JsonResult GetCustomerNames(string prefix)
        {         
            //if (prefix != null && prefix != string.Empty)
        //    {
        //        var result = ((from u in context.tbl_Profile.Where(x => x.F_Name.Contains(prefix))
        //                       select new { ID = u.ID, Value = u.F_Name + " " + ((u.L_Name == null || u.L_Name == "") ? "" : u.L_Name) })).Distinct().ToList();
        //        return Json(result);
        //    }
        //    else
        //    {
                var customers = context.Database.SqlQuery<CustomerList>("sp_CustomerNameList").ToList();
            var foodItems = ((from u in context.tbl_Menu
                               select new { ID = u.ID, Value = u.Food_Item_Name })).Distinct().ToList();

                var result = new
                {
                    customer = customers,
                  foodItem=foodItems
                };
                return Json(result);
            //}
        }
        [HttpPost]
        public JsonResult GetCustomerNamesForWine(string prefix) {

            //var customers = ((from u in context.tbl_Profile
            //                  select new { ID = u.ID, Value = u.F_Name + " " + ((u.L_Name == null || u.L_Name == "") ? "" : u.L_Name), Phone = u.PhoneNo })).Distinct();
            var customers = context.Database.SqlQuery<CustomerList>("sp_CustomerNameList").ToList();
            var foodItems = ((from u in context.tbl_WineMenu
                              select new { ID = u.ID, Value = u.Item_Name })).Distinct().ToList();

            var result = new
            {
                customer = customers.ToList(),
                foodItem = foodItems
            };
            return Json(result);
        }
        public JsonResult GetCustomerNamesForBar(string prefix)
        {

            var customers = context.Database.SqlQuery<CustomerList>("sp_CustomerNameList").ToList();
            var foodItems = ((from u in context.tbl_BarMenu
                              select new { ID = u.ID, Value = u.Item_Name })).Distinct().ToList();

            var result = new
            {
                customer = customers,
                foodItem = foodItems
            };
            return Json(result);
        }
        public JsonResult getCustomerPhone(int id)
        {
            if (id > 0)
            {
                var data = context.tbl_Profile.Find(id);
                if (data != null)
                {
                    return Json(data.PhoneNo);
                }
            }
            return Json("");
        }

        public JsonResult getFoodPrice(int itemID)
        {
            double? price = 0;
            double? leftQty = -1;
            var data = context.tbl_Menu.SingleOrDefault(s => s.ID == itemID);
            if (data != null)
            {
                price = data.Price;

                if (data.InventoryID != 0)
                {
                    var data1 = context.tbl_Inventory.SingleOrDefault(a => a.ID == data.InventoryID);
                    if (data1 != null)
                    {
                        double? usedQty = 0;
                        var tbl = context.tbl_InventoryUsage.Where(s => s.InventoryID == data1.ID).ToList();
                        foreach (var q in tbl)
                        {
                            usedQty = usedQty + q.Used_Qty;
                        }

                        leftQty = data1.Quantity - usedQty;
                    }
                }
            }
            var jsonResult = new
            {
                Price = price,
                LeftQuantity = leftQty
            };
            return Json(jsonResult);
        }

        [HttpPost]
        public JsonResult SaveBillingInfo(MenusBillingSection model)
        {
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["FoodGstPercent"]);
            double? amtWithoutTax = 0;
            double? gst = 0;
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            TimeSpan time = DateTime.Now.TimeOfDay;

            if (model.UserID == 0 && model.Customer_Name != "" && model.Customer_Name != null)
            {
                tbl_Profile prf = new tbl_Profile();
                prf.F_Name = model.Customer_Name;
                prf.PhoneNo = model.Phone;
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
                model.UserID = prf.ID;
            }

            foreach (var f in model.MenusBillingDetailsWithBillNo)
            {
                tbl_MenusBillingDetailsWithBillNo menusdetails = new tbl_MenusBillingDetailsWithBillNo();
                menusdetails.BillNo = model.Bill_Number;
                menusdetails.FoodName = f.FoodName;
                menusdetails.Price = f.Price;
                menusdetails.Quantity = f.Quantity;

                menusdetails.OldQuantity = f.Quantity;

                context.tbl_MenusBillingDetailsWithBillNo.Add(menusdetails);
                context.SaveChanges();

                amtWithoutTax += (f.Price * f.Quantity);

                var data = context.tbl_Menu.SingleOrDefault(a => a.ID == f.ItemNameID);    // && a.Price==model.Price);
                if (data != null)
                {
                    if (data.InventoryID != 0)
                    {

                        tbl_InventoryUsage usage = new tbl_InventoryUsage();
                        usage.InventoryID = data.InventoryID;
                        usage.Used_Qty = f.Quantity;
                        usage.Description = "(Bill No. : " + model.Bill_Number + ") Sold to customer on " + DateFormat;
                        usage.Used_Date = DateFormat;
                        usage.BillNo = model.Bill_Number;
                        usage.MenusBillingDetailsID = menusdetails.ID;
                        usage.GST_NonGST_Bill = "GST";
                        context.tbl_InventoryUsage.Add(usage);
                        context.SaveChanges();
                    }
                }

                var consumeItem = context.tbl_ConsumableItems.Where(s => s.MenuItem_ID == f.ItemNameID).ToList();
                if (consumeItem.Count > 0)
                {
                    foreach (var m in consumeItem)
                    {
                        string tempUsedQty = Convert.ToString(f.Quantity * m.Quantity);
                        tbl_InventoryUsage usage = new tbl_InventoryUsage();
                        usage.InventoryID = m.Inventory_ID;
                        usage.Used_Qty = Convert.ToDouble(tempUsedQty);
                        //usage.Used_Qty = (f.Quantity * m.Quantity);
                        usage.Description = "Bill No. : " + model.Bill_Number + " | " + f.FoodName;
                        usage.Used_Date = DateFormat;
                        usage.BillNo = model.Bill_Number;
                        usage.MenusBillingDetailsID = menusdetails.ID;
                        usage.GST_NonGST_Bill = "GST";
                        context.tbl_InventoryUsage.Add(usage);
                        context.SaveChanges();
                    }
                }
            }
            //gst = amtWithoutTax * (2.5 / 100);
            //gst = (gst * 2);
            gst = amtWithoutTax * ((double)gstPercentFromConfig / (double)100);
            gst = Math.Round((double)gst, 2);
            amtWithoutTax = Math.Round((double)amtWithoutTax, 2);
            tbl_MenusBillingSection menus = new tbl_MenusBillingSection();
            menus.Bill_Number = model.Bill_Number;
            menus.Customer_Name = model.Customer_Name;
            menus.Phone = model.Phone;
            menus.Price = model.Price;
            menus.UserID = model.UserID;
            menus.GST = gst;
            menus.PriceWithoutTax = amtWithoutTax;
            menus.Order_Time = time;
            menus.Mode_Of_Payment = model.Mode_Of_Payment;
            menus.Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);
            menus.PaymentDate = DateFormat;
            context.tbl_MenusBillingSection.Add(menus);
            context.SaveChanges();
            return Json("Saved");
        }

        [HttpPost]
        public JsonResult SaveBillingInfo_Of_NonGSTBill(NonGST_MenusBillingSection model)
        {
            if (model.UserID == 0 && model.Customer_Name != "" && model.Customer_Name != null)
            {
                tbl_Profile prf = new tbl_Profile();
                prf.F_Name = model.Customer_Name;
                prf.PhoneNo = model.Phone;
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
                model.UserID = prf.ID;
            }

            tbl_NonGST_MenusBillingSection menus = new tbl_NonGST_MenusBillingSection();
            menus.NonGstBillNo = model.NonGstBillNo;
            menus.Customer_Name = model.Customer_Name;
            menus.Phone = model.Phone;
            menus.PriceWithoutTax = model.PriceWithoutTax;
            menus.UserID = model.UserID;
            menus.Mode_Of_Payment = model.Mode_Of_Payment;
            menus.Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);

            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");

            menus.PaymentDate = DateFormat;
            context.tbl_NonGST_MenusBillingSection.Add(menus);
            context.SaveChanges();

            foreach (var f in model.NonGST_MenusBillDetWithBillNo)
            {
                tbl_NonGST_MenusBillDetWithBillNo menusdetails = new tbl_NonGST_MenusBillDetWithBillNo();
                menusdetails.NonGst_BillNo = model.NonGstBillNo;
                menusdetails.FoodName = f.FoodName;
                menusdetails.Price = f.Price;
                menusdetails.Quantity = f.Quantity;

                menusdetails.OldQuantity = f.Quantity;

                context.tbl_NonGST_MenusBillDetWithBillNo.Add(menusdetails);
                context.SaveChanges();

                var data = context.tbl_Menu.SingleOrDefault(a => a.ID == f.ItemNameID);    // && a.Price==model.Price);
                if (data != null)
                {
                    if (data.InventoryID != 0)
                    {

                        tbl_InventoryUsage usage = new tbl_InventoryUsage();
                        usage.InventoryID = data.InventoryID;
                        usage.Used_Qty = f.Quantity;
                        usage.Description = "(Non-Gst Bill No. : " + model.NonGstBillNo + ") Sold to customer on " + DateFormat;
                        usage.Used_Date = DateFormat;
                        usage.BillNo = model.NonGstBillNo;
                        usage.MenusBillingDetailsID = menusdetails.ID;
                        usage.GST_NonGST_Bill = "NonGST";
                        context.tbl_InventoryUsage.Add(usage);
                        context.SaveChanges();
                    }
                }
                var consumeItem = context.tbl_ConsumableItems.Where(s => s.MenuItem_ID == f.ItemNameID).ToList();
                if (consumeItem.Count > 0)
                {
                    foreach (var m in consumeItem)
                    {
                        string tempUsedQty = Convert.ToString(f.Quantity * m.Quantity);
                        tbl_InventoryUsage usage = new tbl_InventoryUsage();
                        usage.InventoryID = m.Inventory_ID;
                        usage.Used_Qty = Convert.ToDouble(tempUsedQty);
                        //usage.Used_Qty = f.Quantity * m.Quantity;   // Math.Round((double)(f.Quantity * m.Quantity), 2);
                        usage.Description = "Non-Gst Bill No. : " + model.NonGstBillNo + " | " + f.FoodName;
                        usage.Used_Date = DateFormat;
                        usage.BillNo = model.NonGstBillNo;
                        usage.MenusBillingDetailsID = menusdetails.ID;
                        usage.GST_NonGST_Bill = "NonGST";
                        context.tbl_InventoryUsage.Add(usage);
                        context.SaveChanges();
                    }
                }
            }
            return Json("Saved");
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

        [Authorize(Roles = "1,2")]
        public ActionResult BillsIndex(string filter = "", string filterFromReport = "")
        {
            var AdminList = context.tbl_Profile.OrderBy(a => a.F_Name).Where(s => s.UserType == 2).ToList();
            Profile prf = new Profile();
            List<Profile> admins = new List<Profile>();
            foreach (var i in AdminList)
            {
                admins.Add(new Profile()
                {
                    ID = i.ID,
                    F_Name = i.F_Name,
                    L_Name = i.L_Name
                });
            }
            ViewBag.Admins = admins;
            if (filter != "")
            {
                var Data = JsonConvert.DeserializeObject<dynamic>(filter);
                ViewBag.AdminID = Data[5];
                ViewBag.Tax = Data[1];
                ViewBag.Day = Data[3];
                ViewBag.SDate = Data[7];
                ViewBag.EDate = Data[9];

            }
            ViewBag.FilterFromReport = filterFromReport;
            //var data = context.tbl_MenusBillingSection.ToList();
            //foreach (var i in data)
            //{
            //    double? gst = 0;
            //    double? withoutTaxPrice = 0;
            //    var d = context.tbl_MenusBillingDetailsWithBillNo.Where(a => a.BillNo == i.Bill_Number).ToList();
            //    foreach (var e in d)
            //    {
            //        withoutTaxPrice += (e.Price * e.Quantity);
            //    }
            //    gst = withoutTaxPrice * (2.5 / 100);
            //    gst = Math.Round((double)(gst * 2), 2);

            //    i.PriceWithoutTax = Math.Round((double)withoutTaxPrice, 2);
            //    i.GST = gst;
            //    context.Entry(i).State = EntityState.Modified;
            //    context.SaveChanges();

            //}
            return View();
        }

        [Authorize(Roles = "1,2")]
        public ActionResult BillsDataAccToDay(int day = 1, string tax = "gst", int adminID = 0, string sDate = "", string eDate = "")
        {
            int Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);
            int UserType = Convert.ToInt32(Request.Cookies["UserType"].Value);
            DateTime startdate = DateTime.Now;
            DateTime LastDate = DateTime.Now;

            Session["Day"] = day;
            Session["Tax"] = tax;
            Session["AdminID"] = adminID;
            Session["StartDate"] = sDate;
            Session["EndDate"] = eDate;

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
            //var data = context.tbl_MenusBillingSection.ToList();

            if (tax == "gst")
            {
                string queryForBills = "";
                if (UserType == 1 || (Request.Cookies["UserType"].Value == "2" && Request.Cookies["PageSetting"] != null && Request.Cookies["PageSetting"]["FoodBillingEditPermission"] == "All"))
                {
                    if (adminID == 0)
                        queryForBills = "select m.Bill_Number as Bill_Number,IsNull(m.Discount,0) as Discount,m.OrderTakenBy,m.Price as Price,m.PriceWithoutTax as PriceWithoutTax,m.GST as GST,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_MenusBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Table_Status is null";
                    else
                        queryForBills = "select m.Bill_Number as Bill_Number,IsNull(m.Discount,0) as Discount,m.OrderTakenBy,m.Price as Price,m.PriceWithoutTax as PriceWithoutTax,m.GST as GST,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_MenusBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Table_Status is null and m.Billed_By=" + adminID;
                }
                else
                {
                    queryForBills = "select m.Bill_Number as Bill_Number,IsNull(m.Discount,0) as Discount,m.OrderTakenBy,m.Price as Price,m.PriceWithoutTax as PriceWithoutTax,m.GST as GST,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_MenusBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Table_Status is null and m.Billed_By=" + Billed_By;

                }
                var data = context.Database.SqlQuery<MenusBillingSection>(queryForBills);
                double? finaltotalVal = 0;
                double? finalcsgst = 0, finalDiscount = 0;
                List<MenusBillingSection> menusBill = new List<MenusBillingSection>();
                foreach (var i in data)
                {
                    //double? totalVal = 0;
                    //double? csgst = 0;
                    menusBill.Add(new MenusBillingSection()
                    {
                        Bill_Number = i.Bill_Number,
                        Customer_Name = i.Customer_Name,
                        Phone = i.Phone,
                        PaymentDate = i.PaymentDate,
                        Price = Math.Round((Double)(i.Price - i.Discount), 2),
                        Mode_Of_Payment = i.Mode_Of_Payment,
                        OrderTakenBy = i.OrderTakenBy,
                        Discount = i.Discount
                    });
                    finaltotalVal += i.Price-i.Discount;
                    finalcsgst += i.GST;
                    finalDiscount += i.Discount;
                    //var d = context.tbl_MenusBillingDetailsWithBillNo.Where(a => a.BillNo == i.Bill_Number).ToList();
                    //foreach (var ii in d)
                    //{
                    //    totalVal = totalVal + (ii.Price * ii.Quantity);
                    //}
                    //csgst = totalVal * (2.5 / 100);
                    //csgst = (csgst * 2);
                    //finaltotalVal += totalVal;
                    //finalcsgst += csgst;
                }

                finaltotalVal = Math.Round((Double)finaltotalVal, 2);

                ViewBag.TotalAmount = finaltotalVal+ finalDiscount- finalcsgst;
                ViewBag.CSGST = Math.Round((Double)finalcsgst, 2);
                ViewBag.Discount = Math.Round((Double)finalDiscount, 2);
                return PartialView("~/Views/Billing/_BillsDataAccToDay.cshtml", menusBill);
            }
            else
            {
                string queryForBills = "";
                if (UserType == 1 || (Request.Cookies["UserType"].Value == "2" && Request.Cookies["PageSetting"] != null && Request.Cookies["PageSetting"]["FoodBillingEditPermission"] == "All"))
                {
                    if (adminID == 0)
                        queryForBills = "select m.NonGstBillNo as NonGstBillNo,m.PriceWithoutTax as PriceWithoutTax,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_NonGST_MenusBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date)";
                    else
                        queryForBills = "select m.NonGstBillNo as NonGstBillNo,m.PriceWithoutTax as PriceWithoutTax,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_NonGST_MenusBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Billed_By=" + adminID;
                }
                else
                {
                    queryForBills = "select m.NonGstBillNo as NonGstBillNo,m.PriceWithoutTax as PriceWithoutTax,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_NonGST_MenusBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Billed_By=" + Billed_By;
                }

                var data = context.Database.SqlQuery<NonGST_MenusBillingSection>(queryForBills);
                double? totalVal = 0;
                List<NonGST_MenusBillingSection> menusBill = new List<NonGST_MenusBillingSection>();
                foreach (var i in data)
                {
                    menusBill.Add(new NonGST_MenusBillingSection()
                    {
                        NonGstBillNo = i.NonGstBillNo,
                        Customer_Name = i.Customer_Name,
                        Phone = i.Phone,
                        PaymentDate = i.PaymentDate,
                        PriceWithoutTax = i.PriceWithoutTax,
                        Mode_Of_Payment = i.Mode_Of_Payment
                    });
                    totalVal += i.PriceWithoutTax;
                    //var d = context.tbl_NonGST_MenusBillDetWithBillNo.Where(a => a.NonGst_BillNo == i.Bill_Number).ToList();
                    //foreach (var ii in d)
                    //{
                    //    totalVal = totalVal + (ii.Price * ii.Quantity);
                    //}
                }
                totalVal = Math.Round((Double)totalVal, 2);
                ViewBag.TotalAmount = totalVal;
                return PartialView("~/Views/Billing/_NonGstBillsAccToDay.cshtml", menusBill);
            }
        }

      /* public ActionResult LoadIndex()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoadData()
        {
          
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
           
                // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
           
            var v = (from a in context.tbl_MenusBillingSection select a);

                //SORT
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v =v.OrderBy(sortColumn + " " + sortColumnDir);
                    
                }

                recordsTotal = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
            
        }
        */
 
        [Authorize(Roles = "1,2")]
        public ActionResult BillDetailsByBillNo(int id)
        {          
            double? totalPrice = 0;
            double? csgst = 0;
            var data = context.tbl_MenusBillingSection.SingleOrDefault(s => s.Bill_Number == id);
            MenusBillingSection menusDetails = new MenusBillingSection();
            List<MenusBillingDetailsWithBillNo> details = new List<MenusBillingDetailsWithBillNo>();
            if (data != null)
            {
                if (data.FoodItemName != "" && data.FoodItemName != null)
                {
                    details.Add(new MenusBillingDetailsWithBillNo()
                    {
                        BillNo = data.Bill_Number,
                        FoodName = data.FoodItemName,
                        Price = data.Price,
                        Quantity = data.Quantity,
                        //ID=data.Bill_Number
                    });
                    menusDetails.PriceWithoutTax = data.Price;
                }
                else
                {
                    var d = context.tbl_MenusBillingDetailsWithBillNo.Where(a => a.BillNo == id).ToList();
                    foreach (var i in d)
                    {
                        details.Add(new MenusBillingDetailsWithBillNo()
                        {
                            BillNo = i.BillNo,
                            FoodName = i.FoodName,
                            Price = i.Price,
                            Quantity = i.Quantity,
                            ID = i.ID
                        });
                        //totalPrice += (i.Price * i.Quantity);
                    }
                    //csgst = totalPrice * (2.5 / 100);
                    //csgst = (csgst * 2);
                    //ViewBag.CSGST = Math.Round((Double)csgst, 2);
                    //totalPrice = Math.Round((Double)totalPrice, 2);
                    //menusDetails.Price = totalPrice;
                    menusDetails.GST = data.GST;
                    menusDetails.PriceWithoutTax = data.PriceWithoutTax;
                }

                var billmode = context.tbl_BillingMode.Where(x => x.tbl_Bill_ID == id).ToList();

                if (billmode != null && billmode.Count > 0)
                {
                    foreach (var Data in billmode)
                    {
                        switch (Data.Mode_Of_Pay)
                        {
                            case "Cash":
                                menusDetails.Cash_Payment = Data.Amount.ToString();
                                break;

                            case "Paytm":
                                menusDetails.Paytm_Payment = Data.Amount.ToString(); ;
                                break;

                            case "Card":
                                menusDetails.Card_Payment = Data.Amount.ToString(); ;
                                break;

                        }

                    }
                }
                menusDetails.Mode_Of_Payment = data.Mode_Of_Payment;
                menusDetails.Customer_Name = data.Customer_Name;
                menusDetails.Phone = data.Phone;
                menusDetails.PaymentDate = data.PaymentDate;
                menusDetails.Bill_Number = data.Bill_Number;
                menusDetails.MenusBillingDetailsWithBillNo = details;
                menusDetails.Discount = data.Discount != null ? data.Discount : 0;
                menusDetails.Temp_Day_Data = (Session["Day"] != null) ? Convert.ToInt32(Session["Day"].ToString()) : 1;
                menusDetails.Temp_Tax_Data = (Session["Tax"] != null) ? Session["Tax"].ToString() : "gst";
                menusDetails.Temp_AdminID_Data = (Session["AdminID"] != null) ? Convert.ToInt32(Session["AdminID"].ToString()) : 0;
                menusDetails.Temp_SDate_Data = (Session["StartDate"] != null) ? Session["StartDate"].ToString() : "";
                menusDetails.Temp_EndDate_Data = (Session["EndDate"] != null) ? Session["EndDate"].ToString() : "";
            }
            return View(menusDetails);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult EditBillByBillNo(int id)
        {
            double? totalPrice = 0;
            double? csgst = 0;
            var data = context.tbl_MenusBillingSection.SingleOrDefault(s => s.Bill_Number == id);
            MenusBillingSection menusDetails = new MenusBillingSection();
            List<MenusBillingDetailsWithBillNo> details = new List<MenusBillingDetailsWithBillNo>();
            if (data != null)
            {
                if (data.FoodItemName != "" && data.FoodItemName != null)
                {
                    details.Add(new MenusBillingDetailsWithBillNo()
                    {
                        BillNo = data.Bill_Number,
                        FoodName = data.FoodItemName,
                        Price = data.Price,
                        Quantity = data.Quantity,
                       
                        //ID = data.Bill_Number
                    });
                    menusDetails.PriceWithoutTax = data.Price;
                }
                else
                {
                    var d = context.tbl_MenusBillingDetailsWithBillNo.Where(a => a.BillNo == id).ToList();
                    foreach (var i in d)
                    {
                        details.Add(new MenusBillingDetailsWithBillNo()
                        {
                            BillNo = i.BillNo,
                            FoodName = i.FoodName,
                            Price = i.Price,
                            Quantity = i.Quantity,
                            ID = i.ID
                        });
                        //totalPrice += (i.Price * i.Quantity);
                    }
                    //csgst = totalPrice * (2.5 / 100);
                    //csgst = (csgst * 2);
                    //ViewBag.CSGST = Math.Round((Double)csgst, 2);
                    //totalPrice = Math.Round((Double)totalPrice, 2);
                    //menusDetails.Price = totalPrice;
                    menusDetails.GST = data.GST;
                    menusDetails.PriceWithoutTax = data.PriceWithoutTax;
                }

                menusDetails.Mode_Of_Payment = data.Mode_Of_Payment;
                menusDetails.Customer_Name = data.Customer_Name;
                menusDetails.Phone = data.Phone;
                menusDetails.PaymentDate = data.PaymentDate;
                menusDetails.Bill_Number = data.Bill_Number;
                menusDetails.MenusBillingDetailsWithBillNo = details;
                menusDetails.Discount = data.Discount != null ? data.Discount : 0;
                menusDetails.Temp_Day_Data = (Session["Day"] != null) ? Convert.ToInt32(Session["Day"].ToString()) : 1;
                menusDetails.Temp_Tax_Data = (Session["Tax"] != null) ? Session["Tax"].ToString() : "gst";
                menusDetails.Temp_AdminID_Data = (Session["AdminID"] != null) ? Convert.ToInt32(Session["AdminID"].ToString()) : 0;
                menusDetails.Temp_SDate_Data = (Session["StartDate"] != null) ? Session["StartDate"].ToString() : "";
                menusDetails.Temp_EndDate_Data = (Session["EndDate"] != null) ? Session["EndDate"].ToString() : "";
            }
            return View(menusDetails);
        }

        [HttpPost]
        public JsonResult UpdateQuantityOfFood(int idOfBillingDetOrBillNo, double? quantity, int invoiceNo)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var data = context.tbl_MenusBillingDetailsWithBillNo.SingleOrDefault(s => s.ID == idOfBillingDetOrBillNo);
            if (data != null)
            {
                data.Quantity = quantity;

                data.QtyUpdatedDate = DateFormat;
                data.QtyUpdatedBy = Request.Cookies["AddedBy"].Value;

                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
                calculateAmount(invoiceNo);
                return Json("Quantity Updated.");
            }
            return Json("Data not Found.");
        }

        public void calculateAmount(int? billNo)
        {
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["FoodGstPercent"]);
            double? total = 0;
            double? csgst = 0;
            double? pricewithoutTax = 0;

            var data = context.tbl_MenusBillingDetailsWithBillNo.Where(s => s.BillNo == billNo).ToList();
            foreach (var i in data)
            {
                pricewithoutTax = pricewithoutTax + (i.Price * i.Quantity);
            }
            //csgst = pricewithoutTax * (2.5 / 100);
            //csgst = (csgst * 2);
            csgst = pricewithoutTax * ((double)gstPercentFromConfig / (double)100);
            total = csgst + pricewithoutTax;
            var d = context.tbl_MenusBillingSection.SingleOrDefault(a => a.Bill_Number == billNo);
            if (d != null)
            {
                d.Price = Math.Round((Double)total, 2);
                d.GST = Math.Round((Double)csgst, 2);
                d.PriceWithoutTax = Math.Round((Double)pricewithoutTax, 2);
                context.Entry(d).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        [HttpPost]
        public JsonResult DeleteQuantityOfFood(List<int> selectedItems, int invoiceNo)
        {
            foreach (var id in selectedItems)
            {
                var data = context.tbl_MenusBillingDetailsWithBillNo.SingleOrDefault(s => s.ID == id);
                if (data != null)
                {
                    context.Entry(data).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
            calculateAmount(invoiceNo);
            return Json("Deleted.");
        }

        //public JsonResult SaveBillingInfo(MenusBillingSection model)
        //{
        //    if (model.UserID == 0)
        //    {
        //        tbl_Profile prf = new tbl_Profile();
        //        prf.F_Name = model.Customer_Name;
        //        prf.PhoneNo = model.Phone;
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
        //        model.UserID = prf.ID;
        //    }

        //    tbl_MenusBillingSection menus = new tbl_MenusBillingSection();
        //    menus.Bill_Number = model.Bill_Number;
        //    menus.Customer_Name = model.Customer_Name;
        //    menus.Phone = model.Phone;
        //    menus.FoodItemName = model.FoodItemName;
        //    menus.Price = model.Price;
        //    menus.Quantity = model.Quantity;

        //    menus.UserID = model.UserID;

        //    var date = DateTime.Today;
        //    string DateFormat = date.ToString(@"MM\/dd\/yyyy");

        //    menus.PaymentDate = DateFormat;
        //    context.tbl_MenusBillingSection.Add(menus);
        //    context.SaveChanges();
        //    var data = context.tbl_Menu.SingleOrDefault(a => a.ID == model.ItemNameID);    // && a.Price==model.Price);
        //    if (data != null)
        //    {
        //        if (data.InventoryID != 0)
        //        {

        //            tbl_InventoryUsage usage = new tbl_InventoryUsage();
        //            usage.InventoryID = data.InventoryID;
        //            usage.Used_Qty = model.Quantity;
        //            usage.Description = "(Bill No. : " + model.Bill_Number + ") Sold to customer on " + DateFormat;
        //            usage.Used_Date = DateFormat;
        //            context.tbl_InventoryUsage.Add(usage);
        //            context.SaveChanges();
        //        }
        //    }
        //    var lastRecord = context.tbl_MenusBillingSection.ToList().LastOrDefault();
        //    MenusBillingSection menuData = new MenusBillingSection();
        //    menuData.Bill_Number = lastRecord.Bill_Number;
        //    menuData.Customer_Name = lastRecord.Customer_Name;
        //    menuData.Phone = lastRecord.Phone;
        //    menuData.FoodItemName = lastRecord.FoodItemName;
        //    menuData.Price = lastRecord.Price;
        //    menuData.Quantity = lastRecord.Quantity;
        //    return Json(menuData);
        //}

        [Authorize(Roles = "1,2")]
        public ActionResult Index()
        {
            var data = context.tbl_MenusBillingSection.ToList();
            List<MenusBillingSection> lst = new List<MenusBillingSection>();
            foreach (var i in data)
            {
                lst.Add(new MenusBillingSection()
                {
                    Bill_Number = i.Bill_Number,
                    Customer_Name = i.Customer_Name,
                    Phone = i.Phone,
                    FoodItemName = i.FoodItemName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    FakeQtyAddedBy = i.FakeQtyAddedBy,
                    FakeQtyAddedDate = i.FakeQtyAddedDate,
                    FakeQuantity = i.FakeQuantity
                });
            }
            return View(lst);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult EditBill(int id)
        {
            var data = context.tbl_MenusBillingSection.SingleOrDefault(s => s.Bill_Number == id);
            MenusBillingSection menus = new MenusBillingSection();
            menus.Bill_Number = data.Bill_Number;
            menus.Customer_Name = data.Customer_Name;
            menus.Phone = data.Phone;
            menus.FoodItemName = data.FoodItemName;
            menus.Price = data.Price;
            menus.Quantity = data.Quantity;
            //menus.FakeQtyAddedBy = data.FakeQtyAddedBy;
            //menus.FakeQtyAddedDate = data.FakeQtyAddedDate;
            menus.FakeQuantity = data.FakeQuantity;
            return View(menus);
        }

        [HttpPost]
        public JsonResult EditBillByID(MenusBillingSection model)
        {
            var data = context.tbl_MenusBillingSection.SingleOrDefault(s => s.Bill_Number == model.Bill_Number);
            if (data != null)
            {
                data.FakeQuantity = model.FakeQuantity;
                data.FakeQtyAddedDate = DateTime.Now;
                data.FakeQtyAddedBy = Request.Cookies["AddedBy"].Value;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Updated");
        }

        [Authorize(Roles = "1,2")]
        public ActionResult NonGstBillDetailsByBillNo(int id)
        {
            var data = context.tbl_NonGST_MenusBillingSection.SingleOrDefault(s => s.NonGstBillNo == id);
            NonGST_MenusBillingSection menusDetails = new NonGST_MenusBillingSection();
            List<NonGST_MenusBillDetWithBillNo> details = new List<NonGST_MenusBillDetWithBillNo>();
            if (data != null)
            {

                var d = context.tbl_NonGST_MenusBillDetWithBillNo.Where(a => a.NonGst_BillNo == id).ToList();
                foreach (var i in d)
                {
                    details.Add(new NonGST_MenusBillDetWithBillNo()
                    {
                        NonGst_BillNo = i.NonGst_BillNo,
                        FoodName = i.FoodName,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        ID = i.ID
                    });
                }
                menusDetails.Mode_Of_Payment = data.Mode_Of_Payment;
                menusDetails.PriceWithoutTax = data.PriceWithoutTax;
                menusDetails.Customer_Name = data.Customer_Name;
                menusDetails.Phone = data.Phone;
                menusDetails.PaymentDate = data.PaymentDate;
                menusDetails.NonGstBillNo = data.NonGstBillNo;
                menusDetails.NonGST_MenusBillDetWithBillNo = details;

                menusDetails.Temp_Day_Data = (Session["Day"] != null) ? Convert.ToInt32(Session["Day"].ToString()) : 1;
                menusDetails.Temp_Tax_Data = (Session["Tax"] != null) ? Session["Tax"].ToString() : "gst";
                menusDetails.Temp_AdminID_Data = (Session["AdminID"] != null) ? Convert.ToInt32(Session["AdminID"].ToString()) : 0;
                menusDetails.Temp_SDate_Data = (Session["StartDate"] != null) ? Session["StartDate"].ToString() : "";
                menusDetails.Temp_EndDate_Data = (Session["EndDate"] != null) ? Session["EndDate"].ToString() : "";
            }
            return View(menusDetails);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult NonGstEditBillByBillNo(int id)
        {
            //double? totalVal = 0;
            var data = context.tbl_NonGST_MenusBillingSection.SingleOrDefault(s => s.NonGstBillNo == id);
            NonGST_MenusBillingSection menusDetails = new NonGST_MenusBillingSection();
            List<NonGST_MenusBillDetWithBillNo> details = new List<NonGST_MenusBillDetWithBillNo>();
            if (data != null)
            {

                var d = context.tbl_NonGST_MenusBillDetWithBillNo.Where(a => a.NonGst_BillNo == id).ToList();
                foreach (var i in d)
                {
                    details.Add(new NonGST_MenusBillDetWithBillNo()
                    {
                        NonGst_BillNo = i.NonGst_BillNo,
                        FoodName = i.FoodName,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        ID = i.ID
                    });
                    //totalVal += i.Price;
                }
                //totalVal = Math.Round((Double)totalVal, 2);
                menusDetails.Mode_Of_Payment = data.Mode_Of_Payment;
                menusDetails.PriceWithoutTax = data.PriceWithoutTax;
                menusDetails.Customer_Name = data.Customer_Name;
                menusDetails.Phone = data.Phone;
                menusDetails.PaymentDate = data.PaymentDate;
                menusDetails.NonGstBillNo = data.NonGstBillNo;
                menusDetails.NonGST_MenusBillDetWithBillNo = details;

                menusDetails.Temp_Day_Data = (Session["Day"] != null) ? Convert.ToInt32(Session["Day"].ToString()) : 1;
                menusDetails.Temp_Tax_Data = (Session["Tax"] != null) ? Session["Tax"].ToString() : "gst";
                menusDetails.Temp_AdminID_Data = (Session["AdminID"] != null) ? Convert.ToInt32(Session["AdminID"].ToString()) : 0;
                menusDetails.Temp_SDate_Data = (Session["StartDate"] != null) ? Session["StartDate"].ToString() : "";
                menusDetails.Temp_EndDate_Data = (Session["EndDate"] != null) ? Session["EndDate"].ToString() : "";
            }
            return View(menusDetails);
        }

        [HttpPost]
        public JsonResult NonGst_UpdateQuantityOfFood(int idOfBillingDetOrBillNo, double? quantity, int invoiceNo)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var data = context.tbl_NonGST_MenusBillDetWithBillNo.SingleOrDefault(s => s.ID == idOfBillingDetOrBillNo);
            if (data != null)
            {
                data.Quantity = quantity;

                data.QtyUpdatedDate = DateFormat;
                data.QtyUpdatedBy = Request.Cookies["AddedBy"].Value;

                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
                calculateAmountOfNonGst(invoiceNo);
                return Json("Quantity Updated.");
            }
            return Json("Data not Found.");
        }

        [HttpPost]
        public JsonResult NonGst_DeleteQuantityOfFood(List<int> selectedItems, int invoiceNo)
        {
            foreach (var id in selectedItems)
            {
                var data = context.tbl_NonGST_MenusBillDetWithBillNo.SingleOrDefault(s => s.ID == id);
                if (data != null)
                {
                    context.Entry(data).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
            calculateAmountOfNonGst(invoiceNo);
            return Json("Deleted.");
        }

        public void calculateAmountOfNonGst(int billNo)
        {
            double? total = 0;

            var data = context.tbl_NonGST_MenusBillDetWithBillNo.Where(s => s.NonGst_BillNo == billNo).ToList();
            foreach (var i in data)
            {
                total = total + (i.Price * i.Quantity);
            }

            total = Math.Round((Double)total, 2);
            var d = context.tbl_NonGST_MenusBillingSection.SingleOrDefault(a => a.NonGstBillNo == billNo);
            if (d != null)
            {
                d.PriceWithoutTax = total;
                context.Entry(d).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public JsonResult SaveNewItemsInBills(MenusBillingSection model)
        {
            foreach (var i in model.MenusBillingDetailsWithBillNo)
            {
                tbl_MenusBillingDetailsWithBillNo menuDet = new tbl_MenusBillingDetailsWithBillNo();
                menuDet.FoodName = i.FoodName;
                menuDet.OldQuantity = i.Quantity;
                menuDet.Price = i.Price;
                menuDet.BillNo = model.Bill_Number;
                menuDet.Quantity = i.Quantity;
                context.tbl_MenusBillingDetailsWithBillNo.Add(menuDet);
                context.SaveChanges();

                
            }
            calculateAmount(model.Bill_Number);
            return Json("");
        }

        public JsonResult SaveNewItemsInNonGstBills(MenusBillingSection model)
        {
            foreach (var i in model.MenusBillingDetailsWithBillNo)
            {
                tbl_NonGST_MenusBillDetWithBillNo menuDet = new tbl_NonGST_MenusBillDetWithBillNo();
                menuDet.FoodName = i.FoodName;
                menuDet.OldQuantity = i.Quantity;
                menuDet.Price = i.Price;
                menuDet.NonGst_BillNo = model.Bill_Number;
                menuDet.Quantity = i.Quantity;
                context.tbl_NonGST_MenusBillDetWithBillNo.Add(menuDet);
                context.SaveChanges();
            }
            calculateAmountOfNonGst(model.Bill_Number);
            return Json("");
        }
    }
}
