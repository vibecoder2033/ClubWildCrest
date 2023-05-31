using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.SuperAdmin
{
    public class InventoryController : Controller
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();
        // GET: Inventory
        [Authorize(Roles = "1,2")]
        public ActionResult Index()
        {
            var data = (from i in context.tbl_Inventory
                        join v in context.tbl_Vendors on i.VendorID equals v.ID
                        join iu in context.tbl_InventoryUsage on i.ID equals iu.InventoryID
                        group new { iu } by i into g
                        select new Inventory()
                        {
                            ID = g.Key.ID,
                            Item_Name = g.Key.Item_Name,
                            Type = g.Key.Type,
                            Price = g.Key.Price,
                            Quantity = g.Key.Quantity,
                            VendorID = g.Key.VendorID,
                            VendorName = g.Key.tbl_Vendors.Vendor_First_Name + " " + g.Key.tbl_Vendors.Vendor_Last_Name,
                            Added_Date = g.Key.Added_Date,
                            InStock = g.Key.Quantity - g.Sum(o=>o.iu.Used_Qty) ,
                            Measurement = g.Key.Measurement
                        }).ToList();



            var vendors = (from v in context.tbl_Vendors
                           select new Vendors
                           {
                               ID = v.ID,
                               Vendor_First_Name = v.Vendor_First_Name,
                               Vendor_Last_Name = v.Vendor_Last_Name
                           }).ToList();


            ViewBag.VendorsList = vendors;
            data = data.OrderByDescending(s => DateTime.ParseExact(s.Added_Date, "dd/mm/yyyy", CultureInfo.InvariantCulture)).ToList();
            return View(data);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult AddNewItem()
        {
            var vendorsList = context.tbl_Vendors.Where(vl => vl.IsNewApproved != false && vl.isDeleted != true).OrderBy(s=>s.Vendor_First_Name).ToList();
            List<Vendors> lst = new List<Vendors>();
            foreach (var i in vendorsList)
            {
                lst.Add(new Vendors()
                {
                    ID = i.ID,
                    Vendor_First_Name = i.Vendor_First_Name,
                    Vendor_Last_Name = i.Vendor_Last_Name
                });
            }
            return View(lst);
        }

        [HttpPost]
        public JsonResult addNewItem(Inventory newItem)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            //double? usedQty = 0;
            var data = context.tbl_Inventory.SingleOrDefault(s => s.Item_Name == newItem.Item_Name && s.Type == newItem.Type);
            if (data != null)
            {
                //var usage = context.tbl_InventoryUsage.Where(a => a.InventoryID == data.ID).ToList();
                //foreach (var q in usage)
                //{
                //    usedQty = usedQty + q.Used_Qty;
                //}
                data.Price = newItem.Price;
                //if (usage.Count() != 0)
                //{
                //    double? qty = data.Quantity - usedQty;
                //    if (qty >= 0)
                //    {
                //        data.Quantity = qty + newItem.Quantity;
                //    }
                //    else
                //    {
                //        data.Quantity = newItem.Quantity;
                //    }
                //    foreach (var ug in usage)
                //    {
                //        context.Entry(ug).State = EntityState.Deleted;
                //        context.SaveChanges();
                //    }

                //}
                //else
                //{
                    data.Quantity = newItem.Quantity + data.Quantity;
                //}
                data.Added_Date = DateFormat;
                data.VendorID = newItem.VendorID;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();

                tbl_InventoryUsage usg = new tbl_InventoryUsage();
                usg.InventoryID = data.ID;
                usg.Used_Qty = 0;
                usg.Description = newItem.Quantity + " " + data.Measurement + " added.";
                usg.Used_Date = DateFormat;
                context.tbl_InventoryUsage.Add(usg);
                context.SaveChanges();

                return Json("Modified");
            }
            else
            {
                tbl_Inventory inventory = new tbl_Inventory();
                inventory.Item_Name = newItem.Item_Name;
                inventory.Type = newItem.Type;
                inventory.Price = newItem.Price;
                inventory.Quantity = newItem.Quantity;
                inventory.VendorID = newItem.VendorID;
                inventory.Added_Date = DateFormat;
                inventory.Measurement = newItem.Measurement;
                context.tbl_Inventory.Add(inventory);
                context.SaveChanges();
                return Json("Added");
            }

        }

        [HttpPost]
        public JsonResult addQuantity(Inventory newItem)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var data = context.tbl_Inventory.SingleOrDefault(s => s.ID == newItem.ID);
            if (data != null)
            {
                data.Quantity=data.Quantity == null ? 0 : data.Quantity;
                data.Price = newItem.Price;
                data.Quantity = newItem.Quantity + data.Quantity;
                data.Added_Date = DateFormat;
                data.VendorID = newItem.VendorID;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();

                tbl_InventoryUsage usg = new tbl_InventoryUsage();
                usg.InventoryID = data.ID;
                usg.Used_Qty = 0;
                usg.Description = newItem.Quantity + " " + data.Measurement + " added.";
                usg.Used_Date = DateFormat;
                context.tbl_InventoryUsage.Add(usg);
                context.SaveChanges();
            }
                var lst = (from x in context.tbl_ConsumableItems.Where(x => x.Inventory_ID == newItem.ID) select new { ID = x.MenuItem_ID }).ToList();
                return Json(lst);
            
        }
        
        [HttpPost]
        public JsonResult UpdateQuantity(int InventoryID,double Instock)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var data = context.tbl_Inventory.SingleOrDefault(s => s.ID == InventoryID);
            if (data != null)
            {
                var r = context.tbl_InventoryUsage.Where(a => a.InventoryID == data.ID).ToList();
                double? usedStock = 0;
                if (r.Count() > 0)
                {
                    foreach (var a in r)
                    {
                        usedStock = usedStock + a.Used_Qty;
                    }
                }
                usedStock=Math.Round(Convert.ToDouble(usedStock), 2);
                double OldStock =Convert.ToDouble(data.Quantity) - Convert.ToDouble(usedStock);
                OldStock= Math.Round(OldStock, 2);
                Instock= Math.Round(Instock, 2);
                if (OldStock < Instock)
                {
                    double Quantity = Instock - OldStock;
                    Quantity= Math.Round(Quantity, 2);
                    data.Quantity =data.Quantity+ Quantity;
                    data.Added_Date = DateFormat;
                    context.Entry(data).State = EntityState.Modified;
                    context.SaveChanges();

                    tbl_InventoryUsage usg = new tbl_InventoryUsage();
                    usg.InventoryID = data.ID;
                    usg.Used_Qty = 0;
                    usg.Description =Quantity + " " + data.Measurement + " added.";
                    usg.Used_Date = DateFormat;
                    context.tbl_InventoryUsage.Add(usg);
                    context.SaveChanges();
                    return Json("updated");
                }
                else if (OldStock > Instock)
                {
                    double Quantity =OldStock- Instock;
                    Quantity = Math.Round(Quantity, 2);
                    data.Quantity = data.Quantity - Quantity;
                    data.Added_Date = DateFormat;
                    context.Entry(data).State = EntityState.Modified;
                    context.SaveChanges();

                    tbl_InventoryUsage usg = new tbl_InventoryUsage();
                    usg.InventoryID = data.ID;
                    usg.Used_Qty = 0;
                    usg.Description = Quantity + " " + data.Measurement + " removed.";
                    usg.Used_Date = DateFormat;
                    context.tbl_InventoryUsage.Add(usg);
                    context.SaveChanges();
                    return Json("updated");
                }
                return Json("both equal");
            }
                return Json("");
        }
            public JsonResult getMenuItemID(int id)
        {
            
            var lst = (from x in context.tbl_ConsumableItems.Where(x => x.Inventory_ID == id) select new { ID=x.MenuItem_ID }).ToList();
            return Json(lst);

        }
        //[HttpPost]
        //public JsonResult addNewItem(Inventory newItem)
        //{
        //    var date = DateTime.Today;
        //    //string DateFormat = date.ToString("MM/dd/yyyy");
        //    string DateFormat = date.ToString(@"MM\/dd\/yyyy");
        //    int? usedQty = 0;
        //    var data = context.tbl_Inventory.SingleOrDefault(s => s.Item_Name == newItem.Item_Name && s.Type == newItem.Type);
        //    if (data != null)
        //    {
        //        var usage = context.tbl_InventoryUsage.Where(a => a.InventoryID == data.ID).ToList();
        //        foreach (var q in usage)
        //        {
        //            usedQty = usedQty + q.Used_Qty;
        //        }
        //        data.Price = newItem.Price;
        //        if (usage.Count() != 0)
        //        {
        //            var qty = data.Quantity - usedQty;
        //            if (qty >= 0)
        //            {
        //                data.Quantity = qty + newItem.Quantity;
        //            }
        //            else
        //            {
        //                data.Quantity = newItem.Quantity;
        //            }
        //            foreach (var ug in usage)
        //            {
        //                context.Entry(ug).State = EntityState.Deleted;
        //                context.SaveChanges();
        //            }

        //        }
        //        else
        //        {
        //            data.Quantity = newItem.Quantity + data.Quantity;
        //        }
        //        data.Added_Date = DateFormat;
        //        data.VendorID = newItem.VendorID;
        //        context.Entry(data).State = EntityState.Modified;
        //        context.SaveChanges();
        //        return Json("Modified");
        //    }
        //    else
        //    {
        //        tbl_Inventory inventory = new tbl_Inventory();
        //        inventory.Item_Name = newItem.Item_Name;
        //        inventory.Type = newItem.Type;
        //        inventory.Price = newItem.Price;
        //        inventory.Quantity = newItem.Quantity;
        //        inventory.VendorID = newItem.VendorID;
        //        inventory.Added_Date = DateFormat;
        //        context.tbl_Inventory.Add(inventory);
        //        context.SaveChanges();
        //        return Json("Added");
        //    }

        //}

        [HttpPost]
        public JsonResult GetItemNames(string prefix)
        {
            var result = ((from u in context.tbl_Inventory.Where(x => x.Item_Name.Contains(prefix))
                           select u.Item_Name)).Distinct().ToList();
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetItemType(string prefix)
        {
            var result = ((from u in context.tbl_Inventory.Where(x => x.Type.Contains(prefix))
                           select u.Type)).Distinct().ToList();
            return Json(result);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult AddNewVendor()
        {
            return View();
        }

        [Authorize(Roles = "1,2")]
        public ActionResult InventoryDetailsByID(int id)
        {
            var data = context.tbl_Inventory.Find(id);
            Inventory inv = new Inventory();
            if (data != null)
            {                
                inv.ID = data.ID;
                inv.Item_Name = data.Item_Name;
                inv.Type = data.Type;
                inv.Price = data.Price;
                inv.Quantity = data.Quantity;
                inv.VendorID = data.VendorID;
                inv.VendorName = context.tbl_Vendors.SingleOrDefault(s => s.ID == data.VendorID).Vendor_First_Name + " " + context.tbl_Vendors.SingleOrDefault(s => s.ID == data.VendorID).Vendor_Last_Name;
            }
            return View(inv);
        }

        [HttpPost]
        public JsonResult DeleteInventoryById(int id)
        {
            var data = context.tbl_Inventory.Find(id);
            if (data != null)
            {
                context.Entry(data).State = EntityState.Deleted;
                context.SaveChanges();
            }
            return Json("Deleted");
        }

        [Authorize(Roles = "1,2")]
        public ActionResult InventoryUsage()
        {
            var data = context.tbl_Inventory.OrderBy(s => s.Item_Name).ToList();
            List<Inventory> lst = new List<Inventory>();
            if (data != null)
            {
                foreach(var i in data)
                {
                    lst.Add(new Inventory()
                    {
                        ID = i.ID,
                        Item_Name = i.Item_Name
                    });
                }
            }
            return View(lst);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult InventoryUsedRecord(int day = 1, string sDate = "", string eDate = "")
        {
            
            return View();
        }

        public ActionResult InventoryUsedTable(int day = 1, string sDate = "", string eDate = "")
        {
            DateTime startdate = DateTime.Now;
            DateTime LastDate = DateTime.Now;
            //Session["Day"] = day;
            //Session["StartDate"] = sDate;
            //Session["EndDate"] = eDate;

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
            var data = (from i in context.tbl_Inventory
                        join iu in context.tbl_InventoryUsage on i.ID equals iu.InventoryID
                        join v in context.tbl_Vendors on i.VendorID equals v.ID
                        group new { iu } by new { i } into g
                        select new Inventory()
                        {
                            ID = g.Key.i.ID,
                            Item_Name = g.Key.i.Item_Name,
                            Type = g.Key.i.Type,
                            Price = g.Key.i.Price,
                            VendorName = g.Key.i.tbl_Vendors.Vendor_First_Name + " " + g.Key.i.tbl_Vendors.Vendor_Last_Name,
                            UsedQuantity = g.Sum(o => o.iu.Used_Qty),
                            Measurement = g.Key.i.Measurement,
                            UsageDate = g.OrderByDescending(x => x.iu.ID).FirstOrDefault().iu.Used_Date
                        }).ToList(); ;

            var filteredList = data.Where(x => DateTime.ParseExact(x.UsageDate, "MM/dd/yyyy", CultureInfo.InvariantCulture) >= startdate && DateTime.ParseExact(x.UsageDate, "MM/dd/yyyy", CultureInfo.InvariantCulture) <= LastDate);
            return Json(filteredList, JsonRequestBehavior.AllowGet);
        }




        public DateTime GetUsageDate(string sd)
        {
            DateTime d = DateTime.Parse(sd);
            return d;
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

        [HttpPost]
        public JsonResult getQtyFromInventory(int invID)
       {
           double? usedQty = 0;
           var data = context.tbl_Inventory.SingleOrDefault(s => s.ID == invID);
           var tbl = context.tbl_InventoryUsage.Where(s => s.InventoryID == invID).ToList();
           foreach (var q in tbl)
           {
               usedQty = usedQty + q.Used_Qty;
           }
           
           var left = data.Quantity - usedQty;
           var jsonResult = new
           {
               left = left,
               Measurement = data.Measurement
           };
           return Json(jsonResult);
            
        }

        [HttpPost]
        public JsonResult SaveUsageofItem(InventoryUsage invUsage)
        {           
            var date = DateTime.Today;
            //string DateFormat = date.ToString("MM/dd/yyyy");
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            tbl_InventoryUsage usg = new tbl_InventoryUsage();
            usg.InventoryID = invUsage.InventoryID;
            usg.Used_Qty = invUsage.Used_Qty;
            usg.Description = invUsage.Description;
            usg.Used_Date = DateFormat;
            context.tbl_InventoryUsage.Add(usg);
            context.SaveChanges();
           
            return Json("Saved");
        }

        public JsonResult stockInfo(int inventoryID)
        {
            List<InventoryUsage> usageList = new List<InventoryUsage>();
            
            var data = context.tbl_Inventory.Find(inventoryID);
            var usage = context.tbl_InventoryUsage.Where(w => w.InventoryID == inventoryID).ToList();
            //foreach (var q in usage)
            //{

            //    usageList.Add(new InventoryUsage()
            //    {
            //        Used_Qty = q.Used_Qty,
            //        Description = q.Description,
            //        TotalQuantity = data.Quantity,
            //        Used_Date = q.Used_Date,
            //        BillNo = q.BillNo,
            //        GST_NonGST_Bill = q.GST_NonGST_Bill,
            //        IsBuffetFood = q.IsBuffetFood ?? false
            //    });
            //}
 

            JsonResult jsonResult = Json(usage.Select(x => { x.TotalQuantity = String.Format("{0:0.##}", data.Quantity); return x; }));
            jsonResult.MaxJsonLength = Int32.MaxValue;
            return jsonResult;
        }

        


    }
}
