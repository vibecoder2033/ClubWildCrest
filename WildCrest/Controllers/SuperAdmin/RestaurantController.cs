using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.SuperAdmin
{
    public class RestaurantController : Controller
    {
        //
        // GET: /Restaurant/
        ClubWildCrestEntities context = new ClubWildCrestEntities();

        [Authorize(Roles = "1,2")]
        public ActionResult Index()
        {
            var date = DateTime.Today;
            string currentDate = date.ToString(@"MM\/dd\/yyyy");
            var getdata = context.Database.SqlQuery<TablesForBooking>("sp_FoodOrderTable").ToList();

            // Last Query Executed --> var query = "SELECT t1.ID,t1.TableNo,t2.Table_Status as status FROM tbl_MenusBillingSection t2 right OUTER JOIN tbl_TablesForBooking t1 ON t2.TableID = t1.ID and Cast(t2.PaymentDate as date)=Cast('" + currentDate + "' as date)";

            //var a = (from t2 in context.tbl_MenusBillingSection
            //         join t1 in context.tbl_TablesForBooking on t2.TableID equals t1.ID
            //         where t2.PaymentDate == currentDate
            //         select new
            //         {
            //             TableID = t2.TableID,
            //             Table_Status = t2.Table_Status
            //         }).ToList();
           /* var data = context.tbl_TablesForBooking.ToList();
            List<TablesForBooking> tblList = new List<TablesForBooking>();
            foreach (var i in data)
            {
                var OrderReceived = context.tbl_MenusBillingSection.SingleOrDefault(s => s.TableID == i.ID && s.Table_Status == i.Table_Status);
                tblList.Add(new TablesForBooking()
                      {
                          ID = i.ID,
                          TableNo = i.TableNo,
                          Table_Status = i.Table_Status,
                          OrderReceivedBy = (OrderReceived != null) ? OrderReceived.OrderTakenBy : ""
                      });
            }*/

            return View(getdata);
        }

        [HttpPost]
        public JsonResult CreateOrder(MenusBillingSection model)
        {
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["FoodGstPercent"]);
            double? amtWithoutTax = 0;
            double? gst = 0;
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");

            TimeSpan time = DateTime.Now.TimeOfDay;
            var tableDetails = context.tbl_TablesForBooking.SingleOrDefault(v => v.ID == model.TableID && v.Table_Status == "closed");
            if (tableDetails != null)
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
                var billDet = context.tbl_MenusBillingSection.ToList().LastOrDefault();

                foreach (var f in model.MenusBillingDetailsWithBillNo)
                {
                    tbl_MenusBillingDetailsWithBillNo menusdetails = new tbl_MenusBillingDetailsWithBillNo();
                    menusdetails.BillNo = (billDet != null) ? billDet.Bill_Number + 1 : 1;
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
                            usage.Description = "(Bill No. : " + ((billDet != null) ? (billDet.Bill_Number + 1) : 1) + ") Sold to customer on " + DateFormat;
                            usage.Used_Date = DateFormat;
                            usage.BillNo = (billDet != null) ? billDet.Bill_Number + 1 : 1;
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
                            usage.Used_Qty = Convert.ToDouble(tempUsedQty);  //Math.Round((double)(f.Quantity * m.Quantity), 2);
                            usage.Description = "Bill No. : " + ((billDet != null) ? (billDet.Bill_Number + 1) : 1) + " | " + f.FoodName;
                            usage.Used_Date = DateFormat;
                            usage.BillNo = (billDet != null) ? billDet.Bill_Number + 1 : 1;
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
                menus.Bill_Number = (billDet != null) ? billDet.Bill_Number + 1 : 1;
                menus.Customer_Name = model.Customer_Name;
                menus.Phone = model.Phone;
                menus.Price = Math.Round((double)(gst + amtWithoutTax), 2);
                menus.UserID = model.UserID;
                menus.GST = gst;
                menus.PriceWithoutTax = amtWithoutTax;
                menus.TableID = model.TableID;
                menus.OrderTakenBy = model.OrderTakenBy;
                menus.Table_Status = "opened";

                menus.PaymentDate = DateFormat;
                menus.Order_Time = time;
                context.tbl_MenusBillingSection.Add(menus);
                context.SaveChanges();

                var tblOrder = context.tbl_TablesForBooking.SingleOrDefault(d => d.ID == model.TableID);
                if (tblOrder != null)
                {
                    tblOrder.Table_Status = "opened";
                    context.Entry(tblOrder).State = EntityState.Modified;
                    context.SaveChanges();
                }
                return Json("Saved");
            }
            return Json("Already opened");
        }


        public MenusBillingSection ViewDetails(int tableID)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var data = context.tbl_MenusBillingSection.SingleOrDefault(p => p.TableID == tableID && p.Table_Status != "closed" && p.Table_Status != null);
            MenusBillingSection menus = new MenusBillingSection();
            List<MenusBillingDetailsWithBillNo> menusList = new List<MenusBillingDetailsWithBillNo>();
            if (data != null)
            {
                var lst = context.tbl_MenusBillingDetailsWithBillNo.Where(s => s.BillNo == data.Bill_Number).ToList();
                foreach (var i in lst)
                {
                    menusList.Add(new MenusBillingDetailsWithBillNo()
                    {
                        ID = i.ID,
                        FoodName = i.FoodName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    });
                }
                menus.MenusBillingDetailsWithBillNo = menusList;
                menus.Bill_Number = data.Bill_Number;
                menus.Customer_Name = data.Customer_Name;
                menus.Phone = data.Phone;
                menus.Price = data.Price;
                menus.OrderTakenBy = data.OrderTakenBy;
                menus.PaymentDate = data.PaymentDate;
                menus.TableID = data.TableID;
                menus.Order_Time = data.Order_Time;
                menus.Table_Status = data.Table_Status;
                menus.Table_No = context.tbl_TablesForBooking.SingleOrDefault(w => w.ID == data.TableID).TableNo;
                
            }
            return menus;
        }

        [HttpPost]
        public JsonResult ViewDetailsInModal(int tableID)
        {
            var data = ViewDetails(tableID);
            return Json(data);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult CreateTableBill(int tableID)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var menusData = context.tbl_MenusBillingSection.SingleOrDefault(p => p.TableID == tableID && p.Table_Status == "opened");
            MenusBillingSection menus = new MenusBillingSection();
            List<MenusBillingDetailsWithBillNo> menusList = new List<MenusBillingDetailsWithBillNo>();
            if (menusData != null)
            {
                var lst = context.tbl_MenusBillingDetailsWithBillNo.Where(s => s.BillNo == menusData.Bill_Number).ToList();
                foreach (var i in lst)
                {
                    bool alreadyExists = menusList.Any(x => x.FoodName == i.FoodName && x.Price == i.Price);
                    if (!alreadyExists)
                    {
                        menusList.Add(new MenusBillingDetailsWithBillNo()
                        {
                            ID = i.ID,
                            FoodName = i.FoodName,
                            Price = i.Price,
                            Quantity = i.Quantity
                        });
                    }
                    else
                    {
                        menusList.Find(p => p.FoodName == i.FoodName && p.Price == i.Price).Quantity += i.Quantity;
                    }
                }
                menus.MenusBillingDetailsWithBillNo = menusList;
                menus.Bill_Number = menusData.Bill_Number;
                menus.Customer_Name = menusData.Customer_Name;
                menus.Phone = menusData.Phone;
                menus.Price = menusData.Price;
                menus.PaymentDate = menusData.PaymentDate;
                menus.TableID = menusData.TableID;
                menus.OrderTakenBy = menusData.OrderTakenBy;
                menus.PriceWithoutTax = menusData.PriceWithoutTax;
                menus.GST = menusData.GST;
                menus.Table_No = context.tbl_TablesForBooking.SingleOrDefault(w => w.ID == menusData.TableID).TableNo;
            }
            return View(menus);
        }

        [HttpPost]
        public JsonResult CreateBill(int billNo)
        {
            var menusData = context.tbl_MenusBillingSection.SingleOrDefault(p => p.Bill_Number == billNo);
            var tableData = context.tbl_TablesForBooking.SingleOrDefault(w => w.ID == menusData.TableID);
            menusData.Table_Status = "billed";
            context.Entry(menusData).State = EntityState.Modified;
            tableData.Table_Status = "billed";
            context.Entry(tableData).State = EntityState.Modified;
            context.SaveChanges();

            List<MenusBillingDetailsWithBillNo> Lst = new List<MenusBillingDetailsWithBillNo>();
            var data = context.tbl_MenusBillingDetailsWithBillNo.Where(s => s.BillNo == billNo).ToList();
            foreach (var i in data)
            {
                bool alreadyExists = Lst.Any(x => x.FoodName == i.FoodName && x.Price == i.Price);
                if (!alreadyExists)
                {
                    Lst.Add(new MenusBillingDetailsWithBillNo()
                    {
                        ID = i.ID,
                        FoodName = i.FoodName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    });
                }
                else
                {
                    Lst.Find(p => p.FoodName == i.FoodName && p.Price == i.Price).Quantity += i.Quantity;
                }
            }
            return Json(Lst);
        }

        [HttpPost]
        public JsonResult CloseOrder(int tableID, string discount, double paymentModeCash = 0.0, double paymentModePaytm = 0.0, double paymentModeCard = 0.0, double Balance = 0.0)
        {
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["FoodGstPercent"]);
            double? gst = 0;
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var menusData = context.tbl_MenusBillingSection.SingleOrDefault(p => p.TableID == tableID && p.Table_Status == "billed");
            if (menusData != null)
            {
                var tableData = context.tbl_TablesForBooking.SingleOrDefault(w => w.ID == tableID);
                menusData.Table_Status = null;
                menusData.Mode_Of_Payment = null;
                menusData.Discount = Convert.ToDouble(discount);
                menusData.Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);
               
                context.Entry(menusData).State = EntityState.Modified;
                tableData.Table_Status = "closed";
                context.Entry(tableData).State = EntityState.Modified;
                context.SaveChanges();
                if (menusData.Discount > 0)
                {
                    var FinalAmt = menusData.Price - menusData.Discount;

                    gst = FinalAmt * ((double)gstPercentFromConfig / (double)100);
                    gst = Math.Round((double)gst, 2);
                    menusData.GST = gst;
                    context.Entry(menusData).State = EntityState.Modified;
                    context.SaveChanges();
                }
                if (paymentModeCash > 0)
                {
                    tbl_BillingMode model = new tbl_BillingMode();
                    model.tbl_Bill_ID = menusData.Bill_Number;
                    model.Amount = paymentModeCash - Balance;
                    model.Mode_Of_Pay = "Cash";
                    context.tbl_BillingMode.Add(model);
                    context.SaveChanges();
                }

                if (paymentModePaytm > 0)
                {
                    tbl_BillingMode model = new tbl_BillingMode();
                    model.tbl_Bill_ID = menusData.Bill_Number;
                    model.Amount = paymentModePaytm;
                    model.Mode_Of_Pay = "Paytm";
                    context.tbl_BillingMode.Add(model);
                    context.SaveChanges();
                }
                if (paymentModeCard > 0)
                {
                    tbl_BillingMode model = new tbl_BillingMode();
                    model.tbl_Bill_ID = menusData.Bill_Number;
                    model.Amount = paymentModeCard;
                    model.Mode_Of_Pay = "Card";
                    context.tbl_BillingMode.Add(model);
                    context.SaveChanges();
                }
            }
            return Json("closed");
        }

        [HttpPost]
        public JsonResult addMoreNewItemsInDB(MenusBillingSection model)
        {
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["FoodGstPercent"]);
            double? amtWithoutTax = 0;
            double? gst = 0;
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");

            var billDet = context.tbl_MenusBillingSection.SingleOrDefault(s => s.Bill_Number == model.Bill_Number);
            if (billDet != null)
            {
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
                            //usage.Used_Qty = f.Quantity * m.Quantity;   // Math.Round((double)(f.Quantity * m.Quantity), 2);
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

                amtWithoutTax = amtWithoutTax + billDet.PriceWithoutTax;

                //gst = amtWithoutTax * (2.5 / 100);
                //gst = (gst * 2);
                gst = amtWithoutTax * ((double)gstPercentFromConfig / (double)100);
                gst = Math.Round((double)gst, 2);
                amtWithoutTax = Math.Round((double)amtWithoutTax, 2);

                tbl_MenusBillingSection menus = new tbl_MenusBillingSection();
                billDet.Price = Math.Round((double)(gst + amtWithoutTax), 2);
                billDet.GST = gst;
                billDet.PriceWithoutTax = amtWithoutTax;
                context.Entry(billDet).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Saved");
        }

        public JsonResult DelFoodItemFromMenusBillingSection(int menusBillingDetailsID)
        {
            var data = context.tbl_MenusBillingDetailsWithBillNo.SingleOrDefault(s => s.ID == menusBillingDetailsID);
            List<MenusBillingDetailsWithBillNo> menusDetailList = new List<MenusBillingDetailsWithBillNo>();
            if (data != null)
            {
                int? billNo = data.BillNo;
                context.Entry(data).State = EntityState.Deleted;
                context.SaveChanges();

                var invData = context.tbl_InventoryUsage.Where(a => a.MenusBillingDetailsID == menusBillingDetailsID && a.GST_NonGST_Bill == "GST").ToList();
                if (invData.Count() > 0)
                {
                    foreach (var i in invData)
                    {
                        context.Entry(i).State = EntityState.Deleted;
                        context.SaveChanges();
                    }
                }
                BillingController billContr = new BillingController();
                billContr.calculateAmount(billNo);

                var lst = context.tbl_MenusBillingDetailsWithBillNo.Where(s => s.BillNo == billNo).ToList();
                foreach (var i in lst)
                {
                    menusDetailList.Add(new MenusBillingDetailsWithBillNo()
                    {
                        ID = i.ID,
                        FoodName = i.FoodName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    });
                }
            }
            return Json(menusDetailList);
        }

    }
}
