using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.SuperAdmin
{
    public class WineController : Controller
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();

        //  ---------------------------------------- BAR INVENTORY ---------------------------
        [Authorize(Roles = "1,2")]
        public ActionResult Inventory()
        {
            var data = context.tbl_WineInventory.ToList();
            List<Inventory> invList = new List<Inventory>();
            foreach (var i in data)
            {
                string[] arrDate = i.Added_Date.Split('/');
                DateTime Added_Date = new DateTime(Convert.ToInt32(arrDate[2]), Convert.ToInt32(arrDate[0]), Convert.ToInt32(arrDate[1]));

                var r = context.tbl_WineInventoryUsage.Where(a => a.WineInventoryID == i.ID).ToList();
                double? usedStock = 0;
                if (r.Count() > 0)
                {
                    foreach (var a in r)
                    {
                        usedStock = usedStock + a.Used_Qty;
                    }
                }
                invList.Add(new Inventory()
                {
                    ID = i.ID,
                    Item_Name = i.Item_Name,
                    Type = i.Type,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    VendorID = i.VendorID,
                    VendorName = context.tbl_Vendors.SingleOrDefault(s => s.ID == i.VendorID).Vendor_First_Name + " " + context.tbl_Vendors.SingleOrDefault(s => s.ID == i.VendorID).Vendor_Last_Name,
                    //Added_Date = i.Added_Date,
                    AddedDate1 = Added_Date,
                    InStock = i.Quantity - usedStock,
                    Measurement = i.Measurement
                });
            }

            var vendorList = context.tbl_Vendors.Where(vl => vl.IsNewApproved != false && vl.isDeleted != true).OrderBy(s => s.Vendor_First_Name).ToList();
            List<Vendors> vendors = new List<Vendors>();
            foreach (var v in vendorList)
            {
                vendors.Add(new Vendors()
                {
                    ID = v.ID,
                    Vendor_First_Name = v.Vendor_First_Name,
                    Vendor_Last_Name = v.Vendor_Last_Name
                });
            }
            ViewBag.VendorsList = vendors;
            invList = invList.OrderByDescending(s => s.AddedDate1).ToList();
            return View(invList);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult AddNewItem()
        {
            var vendorsList = context.tbl_Vendors.Where(vl => vl.IsNewApproved != false && vl.isDeleted != true).OrderBy(s => s.Vendor_First_Name).ToList();
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
            var data = context.tbl_WineInventory.SingleOrDefault(s => s.Item_Name == newItem.Item_Name && s.Type == newItem.Type);
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

                tbl_WineInventoryUsage usg = new tbl_WineInventoryUsage();
                usg.WineInventoryID = data.ID;
                usg.Used_Qty = 0;
                usg.Description = newItem.Quantity + " " + data.Measurement + " added.";
                usg.Used_Date = DateFormat;
                context.tbl_WineInventoryUsage.Add(usg);
                context.SaveChanges();

                return Json("Modified");
            }
            else
            {
                tbl_WineInventory inventory = new tbl_WineInventory();
                inventory.Item_Name = newItem.Item_Name;
                inventory.Type = newItem.Type;
                inventory.Price = newItem.Price;
                inventory.Quantity = newItem.Quantity;
                inventory.VendorID = newItem.VendorID;
                inventory.Added_Date = DateFormat;
                inventory.Measurement = newItem.Measurement;
                context.tbl_WineInventory.Add(inventory);
                context.SaveChanges();
                return Json("Added");
            }

        }

        [Authorize(Roles = "1,2")]
        public ActionResult InventoryUsage()
        {
            var data = context.tbl_WineInventory.OrderBy(s => s.Item_Name).ToList();
            List<Inventory> lst = new List<Inventory>();
            if (data != null)
            {
                foreach (var i in data)
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
        public ActionResult InventoryDetailsByID(int id)
        {
            var data = context.tbl_WineInventory.Find(id);
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
        public JsonResult GetItemNames(string prefix)
        {
            var result = ((from u in context.tbl_WineInventory.Where(x => x.Item_Name.Contains(prefix))
                           select u.Item_Name)).Distinct().ToList();
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetItemType(string prefix)
        {
            var result = ((from u in context.tbl_WineInventory.Where(x => x.Type.Contains(prefix))
                           select u.Type)).Distinct().ToList();
            return Json(result);
        }

        [HttpPost]
        public JsonResult getQtyFromInventory(int invID)
        {
            double? usedQty = 0;
            var data = context.tbl_WineInventory.SingleOrDefault(s => s.ID == invID);
            var tbl = context.tbl_WineInventoryUsage.Where(s => s.WineInventoryID == invID).ToList();
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
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            tbl_WineInventoryUsage usg = new tbl_WineInventoryUsage();
            usg.WineInventoryID = invUsage.InventoryID;
            usg.Used_Qty = invUsage.Used_Qty;
            usg.Description = invUsage.Description;
            usg.Used_Date = DateFormat;
            context.tbl_WineInventoryUsage.Add(usg);
            context.SaveChanges();
            return Json("Saved");
        }

        public JsonResult stockInfo(int inventoryID)
        {
            List<InventoryUsage> usageList = new List<InventoryUsage>();

            var data = context.tbl_WineInventory.Find(inventoryID);
            var usage = context.tbl_WineInventoryUsage.OrderByDescending(a => a.ID).Where(w => w.WineInventoryID == inventoryID).ToList();
            foreach (var q in usage)
            {

                usageList.Add(new InventoryUsage()
                {
                    Used_Qty = q.Used_Qty,
                    Description = q.Description,
                    TotalQuantity = data.Quantity,
                    Used_Date = q.Used_Date,
                    BillNo = q.Wine_BillNo,
                    GST_NonGST_Bill = q.GST_NonGST_Bill
                });
            }

            return Json(usageList);
        }

        [HttpPost]
        public JsonResult addQuantity(Inventory newItem)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var data = context.tbl_WineInventory.SingleOrDefault(s => s.ID == newItem.ID);
            if (data != null)
            {
                data.Price = newItem.Price;
                data.Quantity = newItem.Quantity + data.Quantity;
                data.Added_Date = DateFormat;
                data.VendorID = newItem.VendorID;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();

                tbl_WineInventoryUsage usg = new tbl_WineInventoryUsage();
                usg.WineInventoryID = data.ID;
                usg.Used_Qty = 0;
                usg.Description = newItem.Quantity + " " + data.Measurement + " added.";
                usg.Used_Date = DateFormat;
                context.tbl_WineInventoryUsage.Add(usg);
                context.SaveChanges();
            }
            return Json("Modified");
        }

        [HttpPost]
        public JsonResult DeleteInventoryById(int id)
        {
            var data = context.tbl_WineInventory.Find(id);
            if (data != null)
            {
                context.Entry(data).State = EntityState.Deleted;
                context.SaveChanges();
            }
            return Json("Deleted");
        }

        //  ---------------------------------------- BAR ITEMS ---------------------------
        [Authorize(Roles = "1,2")]
        public ActionResult MenuItems()
        {
            var data = context.tbl_WineMenu.OrderByDescending(a => a.ID).Where(s => s.NewItemApprvFrmSuperAdm != false && s.DelItemApprvFromSuperAdm != true).ToList();
            List<MenuItems> items = new List<MenuItems>();
            foreach (var i in data)
            {
                items.Add(new MenuItems()
                {
                    ID = i.ID,
                    Food_Item_Name = i.Item_Name,
                    Details = i.Details,
                    Price = i.Price,
                    FoodType = i.FoodType,
                    AddedBy = i.AddedBy
                });
            }
            return View(items);
        }

        [Authorize(Roles = "1")]
        public ActionResult NewItemApprovalBySuperAdmin()
        {
            var data = context.tbl_WineMenu.OrderByDescending(a => a.ID).Where(s => s.NewItemApprvFrmSuperAdm == false).ToList();
            List<MenuItems> items = new List<MenuItems>();
            foreach (var i in data)
            {
                items.Add(new MenuItems()
                {
                    ID = i.ID,
                    Food_Item_Name = i.Item_Name,
                    Details = i.Details,
                    Price = i.Price,
                    FoodType = i.FoodType,
                    AddedBy = i.AddedBy
                });
            }
            return View(items);
        }

        [Authorize(Roles = "1")]
        public ActionResult DeleteItemBySuperAdminApproval()
        {
            var data = context.tbl_WineMenu.OrderByDescending(a => a.ID).Where(s => s.DelItemApprvFromSuperAdm == true).ToList();
            List<MenuItems> items = new List<MenuItems>();
            foreach (var i in data)
            {
                items.Add(new MenuItems()
                {
                    ID = i.ID,
                    Food_Item_Name = i.Item_Name,
                    Details = i.Details,
                    Price = i.Price,
                    FoodType = i.FoodType,
                    DeletedBy = i.DeletedBy
                });
            }
            return View(items);
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public ActionResult AddNewWineItem()
        {
            List<FoodItemType> typeList = new List<FoodItemType>();
            var data = context.tbl_FoodType.OrderBy(d => d.FoodItemTypeName).ToList();
            foreach (var i in data)
            {
                typeList.Add(new FoodItemType()
                {
                    ID = i.ID,
                    FoodItemTypeName = i.FoodItemTypeName
                });
            }

            List<Inventory> invList = new List<Inventory>();
            var invData = context.tbl_WineInventory.Where(c => !context.tbl_WineMenu.Select(b => b.InventoryID).Contains(c.ID)).OrderBy(s => s.Item_Name);
            foreach (var inv in invData)
            {
                invList.Add(new Inventory()
                {
                    ID = inv.ID,
                    Item_Name = inv.Item_Name,
                    Type = inv.Type
                });
            }
            ViewBag.InventoryList = invList;
            return View(typeList);
        }

        [HttpPost]
        public JsonResult AddNewWineItem(MenuItems menu)
        {
            int currentUserTypeLoggedin = Convert.ToInt32(Request.Cookies["UserType"].Value);
            tbl_WineMenu item = new tbl_WineMenu();
            item.Item_Name = menu.Food_Item_Name;
            item.Details = menu.Details;
            item.Price = menu.Price;
            item.FoodType = menu.FoodType;
            item.AddedBy = Request.Cookies["AddedBy"].Value;
            item.InventoryID = menu.InventoryID;
            if (currentUserTypeLoggedin == 2)
            {
                item.NewItemApprvFrmSuperAdm = false;
            }
            else
            {
                item.NewItemApprvFrmSuperAdm = true;
            }
            item.DelItemApprvFromSuperAdm = false;
            context.tbl_WineMenu.Add(item);
            context.SaveChanges();
            return Json(currentUserTypeLoggedin);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult ItemEditByID(int id, string editDetail)
        {
            List<FoodItemType> typeList = new List<FoodItemType>();
            var data1 = context.tbl_FoodType.ToList();
            foreach (var i in data1)
            {
                typeList.Add(new FoodItemType()
                {
                    ID = i.ID,
                    FoodItemTypeName = i.FoodItemTypeName
                });
            }
            ViewBag.FoodType = typeList;
            var data = context.tbl_WineMenu.SingleOrDefault(i => i.ID == id);
            MenuItems item = new MenuItems();
            item.ID = data.ID;
            item.Food_Item_Name = data.Item_Name;
            item.Details = data.Details;
            item.Price = data.Price;
            item.FoodType = data.FoodType;
            if (editDetail == "Details")
            {
                return View("~/Views/Wine/MenuItemDetailsByID.cshtml", item);
            }
            else
            {
                return View("~/Views/Wine/MenuItemEditByID.cshtml", item);
            }
        }

        public ActionResult NewDelItemEditByID(int id, string editDetail)
        {
            var data = context.tbl_WineMenu.SingleOrDefault(i => i.ID == id);
            MenuItems item = new MenuItems();
            item.ID = data.ID;
            item.Food_Item_Name = data.Item_Name;
            item.Details = data.Details;
            item.Price = data.Price;
            item.FoodType = data.FoodType;
            if (editDetail == "New")
            {
                ViewBag.NewDelMenu = true;
            }
            else
            {
                ViewBag.NewDelMenu = false;
            }
            return View(item);
        }

        [HttpPost]
        public JsonResult UpdateDetailsByID(MenuItems menu)
        {
            var data = context.tbl_WineMenu.Find(menu.ID);
            if (data != null)
            {
                data.Item_Name = menu.Food_Item_Name;
                data.Details = menu.Details;
                data.Price = menu.Price;
                data.FoodType = menu.FoodType;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Updated.");
        }

        [HttpPost]
        public JsonResult DeleteItemById(int id)
        {
            int currentUserTypeLoggedin = Convert.ToInt32(Request.Cookies["UserType"].Value);
            if (currentUserTypeLoggedin == 1)
            {
                var findUser = context.tbl_WineMenu.SingleOrDefault(u => u.ID == id);

                if (findUser != null)
                {
                    context.Entry(findUser).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
            else
            {
                var findUser = context.tbl_WineMenu.SingleOrDefault(u => u.ID == id);
                if (findUser != null)
                {
                    findUser.DelItemApprvFromSuperAdm = true;
                    findUser.DeletedBy = Request.Cookies["AddedBy"].Value;
                    context.Entry(findUser).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            return Json(currentUserTypeLoggedin);
        }

        [HttpPost]
        public JsonResult ApproveItem(int id)
        {
            var data = context.tbl_WineMenu.SingleOrDefault(s => s.ID == id);
            if (data != null)
            {
                data.NewItemApprvFrmSuperAdm = true;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Approved");
        }

        [HttpPost]
        public JsonResult CancelDeleteItemById(int id)
        {
            var findUser = context.tbl_WineMenu.SingleOrDefault(u => u.ID == id);
            if (findUser != null)
            {
                findUser.DelItemApprvFromSuperAdm = false;
                context.Entry(findUser).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("NotDeleted.");
        }

        public JsonResult AddNewFoodItemType(string typeName)
        {
            tbl_FoodType typ = new tbl_FoodType();
            typ.FoodItemTypeName = typeName;
            context.tbl_FoodType.Add(typ);
            context.SaveChanges();
            var data = context.tbl_FoodType.OrderBy(s => s.FoodItemTypeName).ToList();
            List<FoodItemType> list = new List<FoodItemType>();
            foreach (var i in data)
            {
                list.Add(new FoodItemType()
                {
                    ID = i.ID,
                    FoodItemTypeName = i.FoodItemTypeName
                });
            }
            return Json(list);
        }

        public JsonResult WineInventoryDetailsByID(int id)
        {
            var data = context.tbl_WineInventory.Find(id);
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
            return Json(inv);
        }

        public ActionResult ConsumablesItem()
        {
            var consumableItemsList = (from menu in context.tbl_WineMenu
                                       join consumable in context.tbl_Consumable_WineItems on menu.ID equals consumable.WineMenuItem_ID
                                       select new MenuItems
                                       {
                                           ID = menu.ID,
                                           Food_Item_Name = menu.Item_Name,
                                           ConsumableID = consumable.ID
                                       }).ToList();
            var lst = consumableItemsList.GroupBy(s => s.ID).OrderByDescending(d => d.Max(a => a.ConsumableID)).Select(f => f.FirstOrDefault()).ToList();


            ViewBag.MenuItemsConsumableList = lst;
            return View();
        }

        public ActionResult AddNewConsumableItem()
        {
            var menuItems_Data = context.tbl_WineMenu.Where(m => m.InventoryID == 0).ToList();
            var inventoryItems = context.tbl_WineInventory.ToList();
            var menuItems = (from m in menuItems_Data
                             where !context.tbl_Consumable_WineItems.Any(i => i.WineMenuItem_ID == m.ID)
                             select m).ToList();
            List<MenuItems> menusList = new List<MenuItems>();
            foreach (var v in menuItems)
            {
                menusList.Add(new MenuItems()
                {
                    ID = v.ID,
                    Food_Item_Name = v.Item_Name
                });
            }
            ViewBag.MenuItems = menusList.OrderBy(s => s.Food_Item_Name).ToList();
            ViewBag.InventoryItems = inventoryItems.OrderBy(s => s.Item_Name).ToList();
            return View();
        }

        [HttpPost]
        public JsonResult SaveConsumableQuantity(List<ConsumableItems> foodList)
        {
            foreach (var i in foodList)
            {
                tbl_Consumable_WineItems consumable = new tbl_Consumable_WineItems();
                consumable.Inventory_ID = i.Inventory_ID;
                consumable.WineMenuItem_ID = i.MenuItem_ID;
                consumable.MeasurementUnit = i.Measurement;
                consumable.Quantity = i.Quantity;
                context.tbl_Consumable_WineItems.Add(consumable);
                context.SaveChanges();
            }
            return Json("Saved");
        }

        [HttpPost]
        public JsonResult DeleteConsumableItemByMenuId(int menuId)
        {
            var menuItemConsumable = context.tbl_Consumable_WineItems.Where(a => a.WineMenuItem_ID == menuId).ToList();
            foreach (var i in menuItemConsumable)
            {
                context.Entry(i).State = EntityState.Deleted;
                context.SaveChanges();
            }
            return Json("Deleted");
        }

        [HttpPost]
        public JsonResult DetailsOfConsumableItemByMenuId(int menuId)
        {
            var menuItemConsumable = context.tbl_Consumable_WineItems.Where(a => a.WineMenuItem_ID == menuId).ToList();
            List<ConsumableItems> items = new List<ConsumableItems>();
            foreach (var i in menuItemConsumable)
            {
                var invName = context.tbl_WineInventory.SingleOrDefault(s => s.ID == i.Inventory_ID);
                bool alreadyExists = items.Any(x => x.MenuItem_ID == i.WineMenuItem_ID && x.Inventory_ID == i.Inventory_ID);
                if (!alreadyExists)
                {
                    items.Add(new ConsumableItems()
                    {
                        Inventory_ItemName = (invName != null) ? invName.Item_Name : "",
                        Quantity = i.Quantity,
                        Measurement = i.MeasurementUnit,
                        Inventory_ID = i.Inventory_ID,
                        MenuItem_ID = i.WineMenuItem_ID
                    });
                }
                else
                {
                    items.Find(p => p.MenuItem_ID == i.WineMenuItem_ID && p.Inventory_ID == i.Inventory_ID).Quantity += i.Quantity;
                }
            }
            return Json(items);
        }

        public ActionResult ConsumableItemEditByID(int menuId)
        {
            var menuItemConsumable = context.tbl_Consumable_WineItems.Where(a => a.WineMenuItem_ID == menuId).ToList();
            List<ConsumableItems> items = new List<ConsumableItems>();
            string mName = "";
            foreach (var i in menuItemConsumable)
            {
                var invName = context.tbl_WineInventory.SingleOrDefault(s => s.ID == i.Inventory_ID);
                var menuName = context.tbl_WineMenu.SingleOrDefault(s => s.ID == i.WineMenuItem_ID);
                items.Add(new ConsumableItems()
                {
                    Inventory_ItemName = (invName != null) ? invName.Item_Name : "",
                    Quantity = i.Quantity,
                    Measurement = i.MeasurementUnit,
                    ID = i.ID,
                    MenuItem_ID = i.WineMenuItem_ID,
                });
                mName = (menuName != null) ? menuName.Item_Name : "";
            }
            ViewBag.Menu_Item_Name = mName;
            ViewBag.Menu_Item_ID = menuId;
            //------------------------------------------------------------------------------
            var inventoryItems = context.tbl_WineInventory.ToList();
            var invItems = (from m in inventoryItems
                            where !context.tbl_Consumable_WineItems.Any(i => i.WineMenuItem_ID == menuId && i.Inventory_ID == m.ID)
                            select m).ToList();
            List<MenuItems> invList = new List<MenuItems>();
            foreach (var v in invItems)
            {
                invList.Add(new MenuItems()
                {
                    ID = v.ID,
                    Food_Item_Name = v.Item_Name
                });
            }
            ViewBag.InventoryItems = invList.OrderBy(s => s.Food_Item_Name).ToList();
            return View(items);
        }

        [HttpPost]
        public JsonResult UpdateQuantityOfConsumableItem(int idOfConsumableItem, double? quantity)
        {
            var item = context.tbl_Consumable_WineItems.Find(idOfConsumableItem);
            if (item != null)
            {
                item.Quantity = (quantity != null) ? quantity : 0;
                context.Entry(item).State = EntityState.Modified;
                context.SaveChanges();
                return Json("Quantity Updated.");
            }
            return Json("Data not Found.");
        }

        [HttpPost]
        public JsonResult DeleteQuantityOfConsumableItem(List<int> selectedItems)
        {
            foreach (var id in selectedItems)
            {
                var data = context.tbl_Consumable_WineItems.SingleOrDefault(s => s.ID == id);
                if (data != null)
                {
                    context.Entry(data).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
            return Json("Deleted.");
        }

        [HttpPost]
        public JsonResult getMeasurementUnit(int inventoryID)
        {
            var mUnit = context.tbl_WineInventory.SingleOrDefault(s => s.ID == inventoryID);
            string unit = "";
            if (mUnit != null)
            {
                unit = mUnit.Measurement;
            }
            return Json(unit);
        }

        //  -------------------------------------------------  BAR BILLING ----------------------

        [Authorize(Roles = "1,2")]
        public ActionResult WineBillingSection()
        {
            var data = context.tbl_WineBillingSection.ToList().LastOrDefault();
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

        [HttpPost]
        public JsonResult GetWineItemNames(string prefix)
        {
            if (prefix != null && prefix != string.Empty)
            {
                var result = ((from u in context.tbl_WineMenu.Where(x => x.Item_Name.Contains(prefix))
                               select new { ID = u.ID, Value = u.Item_Name })).Distinct().ToList();
                return Json(result);
            }
            else
            {
                var result = ((from u in context.tbl_WineMenu
                               select new { ID = u.ID, Value = u.Item_Name })).Distinct().ToList();
                return Json(result);

            }
        }

        public JsonResult getFoodPrice(int itemID)
        {
            double? price = 0;
            double? leftQty = -1;
            var data = context.tbl_WineMenu.SingleOrDefault(s => s.ID == itemID);
            if (data != null)
            {
                price = data.Price;

                if (data.InventoryID != 0)
                {
                    var data1 = context.tbl_WineInventory.SingleOrDefault(a => a.ID == data.InventoryID);
                    if (data1 != null)
                    {
                        double? usedQty = 0;
                        var tbl = context.tbl_WineInventoryUsage.Where(s => s.WineInventoryID == data1.ID).ToList();
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
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["BarGstPercent"]);
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
                tbl_WineBillingDetailsWithBillNo menusdetails = new tbl_WineBillingDetailsWithBillNo();
                menusdetails.BillNo = model.Bill_Number;
                menusdetails.ItemName = f.FoodName;
                menusdetails.Price = f.Price;
                menusdetails.Quantity = f.Quantity;

                menusdetails.OldQuantity = f.Quantity;

                context.tbl_WineBillingDetailsWithBillNo.Add(menusdetails);
                context.SaveChanges();

                amtWithoutTax += (f.Price * f.Quantity);

                var data = context.tbl_WineMenu.SingleOrDefault(a => a.ID == f.ItemNameID);    // && a.Price==model.Price);
                if (data != null)
                {
                    if (data.InventoryID != 0)
                    {

                        tbl_WineInventoryUsage usage = new tbl_WineInventoryUsage();
                        usage.WineInventoryID = data.InventoryID;
                        usage.Used_Qty = f.Quantity;
                        usage.Description = "(Bill No. : " + model.Bill_Number + ") Sold to customer on " + DateFormat;
                        usage.Used_Date = DateFormat;
                        usage.Wine_BillNo = model.Bill_Number;
                        usage.Wine_MenusBillingDetailsID = menusdetails.ID;
                        usage.GST_NonGST_Bill = "GST";
                        context.tbl_WineInventoryUsage.Add(usage);
                        context.SaveChanges();
                    }
                }

                var consumeItem = context.tbl_Consumable_WineItems.Where(s => s.WineMenuItem_ID == f.ItemNameID).ToList();
                if (consumeItem.Count > 0)
                {
                    foreach (var m in consumeItem)
                    {
                        string tempUsedQty = Convert.ToString(f.Quantity * m.Quantity);
                        tbl_WineInventoryUsage usage = new tbl_WineInventoryUsage();
                        usage.WineInventoryID = m.Inventory_ID;
                        usage.Used_Qty = Convert.ToDouble(tempUsedQty);
                        //usage.Used_Qty = (f.Quantity * m.Quantity);
                        usage.Description = "Bill No. : " + model.Bill_Number + " | " + f.FoodName;
                        usage.Used_Date = DateFormat;
                        usage.Wine_BillNo = model.Bill_Number;
                        usage.Wine_MenusBillingDetailsID = menusdetails.ID;
                        usage.GST_NonGST_Bill = "GST";
                        context.tbl_WineInventoryUsage.Add(usage);
                        context.SaveChanges();
                    }
                }
            }
            //gst = amtWithoutTax * (9f / 100f);
            //gst = (gst * 2);
            gst = amtWithoutTax * ((double)gstPercentFromConfig / (double)100);
            gst = Math.Round((double)gst, 2);
            amtWithoutTax = Math.Round((double)amtWithoutTax, 2);
            tbl_WineBillingSection menus = new tbl_WineBillingSection();
            menus.Bill_Number = model.Bill_Number;
            menus.Customer_Name = model.Customer_Name;
            menus.Phone = model.Phone;
            menus.Price = model.Price;
            menus.UserID = model.UserID;
            menus.GST = 0.0;//gst;
            menus.PriceWithoutTax = amtWithoutTax;
            menus.Order_Time = time;
            menus.Mode_Of_Payment = model.Mode_Of_Payment;
            menus.Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);
            menus.PaymentDate = DateFormat;
            context.tbl_WineBillingSection.Add(menus);
            context.SaveChanges();
            return Json("Saved");
        }

        [Authorize(Roles = "1,2")]
        public ActionResult WineBillsIndex(string filter = "", string filterFromReport = "")
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

            if (tax == "gst")
            {
                string queryForBills = "";
                if (UserType == 1 || (Request.Cookies["UserType"].Value == "2" && Request.Cookies["PageSetting"] != null && Request.Cookies["PageSetting"]["FoodBillingEditPermission"] == "All"))
                {
                    if (adminID == 0)
                        queryForBills = "select m.Bill_Number as Bill_Number,IsNull(m.Discount,0) as Discount,m.OrderTakenBy,m.Price as Price,m.PriceWithoutTax as PriceWithoutTax,m.GST as GST,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_WineBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Table_Status is null";
                    else
                        queryForBills = "select m.Bill_Number as Bill_Number,IsNull(m.Discount,0) as Discount,m.OrderTakenBy,m.Price as Price,m.PriceWithoutTax as PriceWithoutTax,m.GST as GST,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_WineBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Table_Status is null and m.Billed_By=" + adminID;
                }
                else
                {
                    queryForBills = "select m.Bill_Number as Bill_Number,IsNull(m.Discount,0) as Discount,m.OrderTakenBy,m.Price as Price,m.PriceWithoutTax as PriceWithoutTax,m.GST as GST,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_WineBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Table_Status is null and m.Billed_By=" + Billed_By;

                }
                var data = context.Database.SqlQuery<MenusBillingSection>(queryForBills);
                double? finaltotalVal = 0;
                double? finalcsgst = 0, finalDiscount = 0;
                List<MenusBillingSection> menusBill = new List<MenusBillingSection>();
                foreach (var i in data)
                {
                    menusBill.Add(new MenusBillingSection()
                    {
                        Bill_Number = i.Bill_Number,
                        Customer_Name = i.Customer_Name,
                        Phone = i.Phone,
                        PaymentDate = i.PaymentDate!=null&&i.PaymentDate!=string.Empty?Convert.ToDateTime(i.PaymentDate).ToString("dd/MM/yyyy") :i.PaymentDate,
                        Price = Math.Round((Double)(i.Price - i.Discount), 2),
                        Mode_Of_Payment = i.Mode_Of_Payment,
                        OrderTakenBy = i.OrderTakenBy,
                        Discount = i.Discount
                    });
                    finaltotalVal += i.PriceWithoutTax;
                    finalcsgst += i.GST;
                    finalDiscount += i.Discount;
                }

                finaltotalVal = Math.Round((Double)finaltotalVal, 2);

                ViewBag.TotalAmount = finaltotalVal;
                ViewBag.CSGST = Math.Round((Double)finalcsgst, 2);
                ViewBag.Discount = Math.Round((Double)finalDiscount, 2);
                return PartialView("~/Views/Wine/_WineBillsDataAccToDay.cshtml", menusBill);
            }
            else
            {
                string queryForBills = "";
                if (UserType == 1 || (Request.Cookies["UserType"].Value == "2" && Request.Cookies["PageSetting"] != null && Request.Cookies["PageSetting"]["FoodBillingEditPermission"] == "All"))
                {
                    if (adminID == 0)
                        queryForBills = "select m.NonGstBillNo as NonGstBillNo,m.PriceWithoutTax as PriceWithoutTax,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_NonGST_WineBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date)";
                    else
                        queryForBills = "select m.NonGstBillNo as NonGstBillNo,m.PriceWithoutTax as PriceWithoutTax,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_NonGST_WineBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Billed_By=" + adminID;
                }
                else
                {
                    queryForBills = "select m.NonGstBillNo as NonGstBillNo,m.PriceWithoutTax as PriceWithoutTax,m.Customer_Name as Customer_Name,m.Phone as Phone,m.PaymentDate as PaymentDate,m.Mode_Of_Payment,m.Billed_By from tbl_NonGST_WineBillingSection m where cast(m.PaymentDate as date)>=cast('" + startDateFormat + "' as date) and cast(m.PaymentDate as date)<=cast('" + LastDateFormat + "' as date) and m.Billed_By=" + Billed_By;
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
                        PaymentDate = i.PaymentDate != null && i.PaymentDate != string.Empty ? Convert.ToDateTime(i.PaymentDate).ToString("dd/MM/yyyy") : i.PaymentDate,

                        PriceWithoutTax = i.PriceWithoutTax,
                        Mode_Of_Payment = i.Mode_Of_Payment
                    });
                    totalVal += i.PriceWithoutTax;
                }
                totalVal = Math.Round((Double)totalVal, 2);
                ViewBag.TotalAmount = totalVal;
                return PartialView("~/Views/Wine/_NonGstWineBillsAccToDay.cshtml", menusBill);
            }
        }

        [Authorize(Roles = "1,2")]
        public ActionResult Wine_BillDetailsByBillNo(int id)
        {
            var data = context.tbl_WineBillingSection.SingleOrDefault(s => s.Bill_Number == id);
            MenusBillingSection menusDetails = new MenusBillingSection();
            List<MenusBillingDetailsWithBillNo> details = new List<MenusBillingDetailsWithBillNo>();
            if (data != null)
            {
                var d = context.tbl_WineBillingDetailsWithBillNo.Where(a => a.BillNo == id).ToList();
                foreach (var i in d)
                {
                    details.Add(new MenusBillingDetailsWithBillNo()
                    {
                        BillNo = i.BillNo,
                        FoodName = i.ItemName,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        ID = i.ID
                    });
                }

                menusDetails.GST = data.GST;
                menusDetails.PriceWithoutTax = data.PriceWithoutTax;
                menusDetails.Discount = data.Discount;
                menusDetails.Mode_Of_Payment = data.Mode_Of_Payment;
                menusDetails.Customer_Name = data.Customer_Name;
                menusDetails.Phone = data.Phone;
                menusDetails.PaymentDate = data.PaymentDate;
                menusDetails.Bill_Number = data.Bill_Number;
                menusDetails.MenusBillingDetailsWithBillNo = details;
                menusDetails.Price = data.Price;
                menusDetails.Winebillno = data.Winebillno;
                menusDetails.Temp_Day_Data = (Session["Day"] != null) ? Convert.ToInt32(Session["Day"].ToString()) : 1;
                menusDetails.Temp_Tax_Data = (Session["Tax"] != null) ? Session["Tax"].ToString() : "gst";
                menusDetails.Temp_AdminID_Data = (Session["AdminID"] != null) ? Convert.ToInt32(Session["AdminID"].ToString()) : 0;
                menusDetails.Temp_SDate_Data = (Session["StartDate"] != null) ? Session["StartDate"].ToString() : "";
                menusDetails.Temp_EndDate_Data = (Session["EndDate"] != null) ? Session["EndDate"].ToString() : "";
            }
            return View(menusDetails);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult Wine_EditBillByBillNo(int id)
        {
            var data = context.tbl_WineBillingSection.SingleOrDefault(s => s.Bill_Number == id);
            MenusBillingSection menusDetails = new MenusBillingSection();
            List<MenusBillingDetailsWithBillNo> details = new List<MenusBillingDetailsWithBillNo>();
            if (data != null)
            {
                var d = context.tbl_WineBillingDetailsWithBillNo.Where(a => a.BillNo == id).ToList();
                foreach (var i in d)
                {
                    details.Add(new MenusBillingDetailsWithBillNo()
                    {
                        BillNo = i.BillNo,
                        FoodName = i.ItemName,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        ID = i.ID
                    });
                }

                menusDetails.GST = data.GST;
                menusDetails.PriceWithoutTax = data.PriceWithoutTax;
                menusDetails.Mode_Of_Payment = data.Mode_Of_Payment;
                menusDetails.Customer_Name = data.Customer_Name;
                menusDetails.Phone = data.Phone;
                menusDetails.PaymentDate = data.PaymentDate;
                menusDetails.Bill_Number = data.Bill_Number;
                menusDetails.MenusBillingDetailsWithBillNo = details;
                menusDetails.Discount = data.Discount;
                menusDetails.Winebillno = data.Winebillno;
                menusDetails.Temp_Day_Data = (Session["Day"] != null) ? Convert.ToInt32(Session["Day"].ToString()) : 1;
                menusDetails.Temp_Tax_Data = (Session["Tax"] != null) ? Session["Tax"].ToString() : "gst";
                menusDetails.Temp_AdminID_Data = (Session["AdminID"] != null) ? Convert.ToInt32(Session["AdminID"].ToString()) : 0;
                menusDetails.Temp_SDate_Data = (Session["StartDate"] != null) ? Session["StartDate"].ToString() : "";
                menusDetails.Temp_EndDate_Data = (Session["EndDate"] != null) ? Session["EndDate"].ToString() : "";
            }
            return View(menusDetails);
        }

        [HttpPost]
        public JsonResult UpdateQuantityOfItem(int idOfBillingDetOrBillNo, double? quantity, int invoiceNo)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var data = context.tbl_WineBillingDetailsWithBillNo.SingleOrDefault(s => s.ID == idOfBillingDetOrBillNo);
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
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["BarGstPercent"]);
            
            double? pricewithoutTax = 0;

            var data = context.tbl_WineBillingDetailsWithBillNo.Where(s => s.BillNo == billNo).ToList();
            foreach (var i in data)
            {
                pricewithoutTax = pricewithoutTax + (i.Price * i.Quantity);
            }
            //csgst = pricewithoutTax * (9f / 100f);
            //csgst = (csgst * 2);
           // csgst = pricewithoutTax * ((double)gstPercentFromConfig / (double)100);
            //total = csgst + pricewithoutTax;
            var d = context.tbl_WineBillingSection.SingleOrDefault(a => a.Bill_Number == billNo);
            if (d != null)
            {
                d.Price = pricewithoutTax;// Math.Round((Double)total, 2);
                //d.GST = Math.Round((Double)csgst, 2);
                d.PriceWithoutTax = pricewithoutTax;// Math.Round((Double)pricewithoutTax, 2);
                context.Entry(d).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        [HttpPost]
        public JsonResult DeleteQuantityOfItem(List<int> selectedItems, int invoiceNo)
        {
            foreach (var id in selectedItems)
            {
                var data = context.tbl_WineBillingDetailsWithBillNo.SingleOrDefault(s => s.ID == id);
                if (data != null)
                {
                    context.Entry(data).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
            calculateAmount(invoiceNo);
            return Json("Deleted.");
        }

        public JsonResult SaveNewItemsInBills(MenusBillingSection model)
        {
            foreach (var i in model.MenusBillingDetailsWithBillNo)
            {
                tbl_WineBillingDetailsWithBillNo menuDet = new tbl_WineBillingDetailsWithBillNo();
                menuDet.ItemName = i.FoodName;
                menuDet.OldQuantity = i.Quantity;
                menuDet.Price = i.Price;
                menuDet.BillNo = model.Bill_Number;
                menuDet.Quantity = i.Quantity;
                context.tbl_WineBillingDetailsWithBillNo.Add(menuDet);
                context.SaveChanges();
            }
            calculateAmount(model.Bill_Number);
            return Json("");
        }

        public JsonResult NonGst_BillNo()
        {
            var data = context.tbl_NonGST_WineBillingSection.ToList().LastOrDefault();
            int NonGstBillNo = 1;
            if (data != null)
            {
                NonGstBillNo = data.NonGstBillNo + 1;
            }
            return Json(NonGstBillNo);
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

            tbl_NonGST_WineBillingSection menus = new tbl_NonGST_WineBillingSection();
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
            context.tbl_NonGST_WineBillingSection.Add(menus);
            context.SaveChanges();

            foreach (var f in model.NonGST_MenusBillDetWithBillNo)
            {
                tbl_NonGST_WineBillDetWithBillNo menusdetails = new tbl_NonGST_WineBillDetWithBillNo();
                menusdetails.NonGst_BillNo = model.NonGstBillNo;
                menusdetails.FoodName = f.FoodName;
                menusdetails.Price = f.Price;
                menusdetails.Quantity = f.Quantity;

                menusdetails.OldQuantity = f.Quantity;

                context.tbl_NonGST_WineBillDetWithBillNo.Add(menusdetails);
                context.SaveChanges();

                var data = context.tbl_WineMenu.SingleOrDefault(a => a.ID == f.ItemNameID);    // && a.Price==model.Price);
                if (data != null)
                {
                    if (data.InventoryID != 0)
                    {

                        tbl_WineInventoryUsage usage = new tbl_WineInventoryUsage();
                        usage.WineInventoryID = data.InventoryID;
                        usage.Used_Qty = f.Quantity;
                        usage.Description = "(Non-Gst Bill No. : " + model.NonGstBillNo + ") Sold to customer on " + DateFormat;
                        usage.Used_Date = DateFormat;
                        usage.Wine_BillNo = model.NonGstBillNo;
                        usage.Wine_MenusBillingDetailsID = menusdetails.ID;
                        usage.GST_NonGST_Bill = "NonGST";
                        context.tbl_WineInventoryUsage.Add(usage);
                        context.SaveChanges();
                    }
                }
                var consumeItem = context.tbl_Consumable_WineItems.Where(s => s.WineMenuItem_ID == f.ItemNameID).ToList();
                if (consumeItem.Count > 0)
                {
                    foreach (var m in consumeItem)
                    {
                        string tempUsedQty = Convert.ToString(f.Quantity * m.Quantity);
                        tbl_WineInventoryUsage usage = new tbl_WineInventoryUsage();
                        usage.WineInventoryID = m.Inventory_ID;
                        usage.Used_Qty = Convert.ToDouble(tempUsedQty);
                        //usage.Used_Qty = f.Quantity * m.Quantity;   // Math.Round((double)(f.Quantity * m.Quantity), 2);
                        usage.Description = "Non-Gst Bill No. : " + model.NonGstBillNo + " | " + f.FoodName;
                        usage.Used_Date = DateFormat;
                        usage.Wine_BillNo = model.NonGstBillNo;
                        usage.Wine_MenusBillingDetailsID = menusdetails.ID;
                        usage.GST_NonGST_Bill = "NonGST";
                        context.tbl_WineInventoryUsage.Add(usage);
                        context.SaveChanges();
                    }
                }
            }
            return Json("Saved");
        }

        [Authorize(Roles = "1,2")]
        public ActionResult NonGstBillDetailsByBillNo(int id)
        {
            var data = context.tbl_NonGST_WineBillingSection.SingleOrDefault(s => s.NonGstBillNo == id);
            NonGST_MenusBillingSection menusDetails = new NonGST_MenusBillingSection();
            List<NonGST_MenusBillDetWithBillNo> details = new List<NonGST_MenusBillDetWithBillNo>();
            if (data != null)
            {

                var d = context.tbl_NonGST_WineBillDetWithBillNo.Where(a => a.NonGst_BillNo == id).ToList();
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
            var data = context.tbl_NonGST_WineBillingSection.SingleOrDefault(s => s.NonGstBillNo == id);
            NonGST_MenusBillingSection menusDetails = new NonGST_MenusBillingSection();
            List<NonGST_MenusBillDetWithBillNo> details = new List<NonGST_MenusBillDetWithBillNo>();
            if (data != null)
            {

                var d = context.tbl_NonGST_WineBillDetWithBillNo.Where(a => a.NonGst_BillNo == id).ToList();
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
            var data = context.tbl_NonGST_WineBillDetWithBillNo.SingleOrDefault(s => s.ID == idOfBillingDetOrBillNo);
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
                var data = context.tbl_NonGST_WineBillDetWithBillNo.SingleOrDefault(s => s.ID == id);
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

            var data = context.tbl_NonGST_WineBillDetWithBillNo.Where(s => s.NonGst_BillNo == billNo).ToList();
            foreach (var i in data)
            {
                total = total + (i.Price * i.Quantity);
            }

            total = Math.Round((Double)total, 2);
            var d = context.tbl_NonGST_WineBillingSection.SingleOrDefault(a => a.NonGstBillNo == billNo);
            if (d != null)
            {
                d.PriceWithoutTax = total;
                context.Entry(d).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public JsonResult SaveNewItemsInNonGstBills(MenusBillingSection model)
        {
            foreach (var i in model.MenusBillingDetailsWithBillNo)
            {
                tbl_NonGST_WineBillDetWithBillNo menuDet = new tbl_NonGST_WineBillDetWithBillNo();
                menuDet.FoodName = i.FoodName;
                menuDet.OldQuantity = i.Quantity;
                menuDet.Price = i.Price;
                menuDet.NonGst_BillNo = model.Bill_Number;
                menuDet.Quantity = i.Quantity;
                context.tbl_NonGST_WineBillDetWithBillNo.Add(menuDet);
                context.SaveChanges();
            }
            calculateAmountOfNonGst(model.Bill_Number);
            return Json("");
        }


        //  ---------------------------------------- ORDERS -----------------------------------

        [Authorize(Roles = "1,2")]
        public ActionResult WineOrders()
        {
            var date = DateTime.Today;
            string currentDate = date.ToString(@"MM\/dd\/yyyy");
            
            var getdata = context.Database.SqlQuery<TablesForBooking>("sp_WineOrderTable").ToList();

           /* var data = context.tbl_TablesForBooking.ToList();
            List<TablesForBooking> tblList = new List<TablesForBooking>();
            foreach (var i in data)
            {
                var OrderReceived = context.tbl_WineBillingSection.SingleOrDefault(s => s.TableID == i.ID && s.Table_Status == i.Wine_Status);
                tblList.Add(new TablesForBooking()
                {
                    ID = i.ID,
                    TableNo = i.TableNo,
                    Table_Status = i.Table_Status,
                    Bar_Status = i.Bar_Status,
                    Wine_Status=i.Wine_Status,
                    OrderReceivedBy = (OrderReceived != null) ? OrderReceived.OrderTakenBy : ""
                });
            }*/
            return View(getdata);
        }
        string GetBillNO()
        {

            string BillNo = context.tbl_WineBillingSection.OrderByDescending(obj => obj.Bill_Number).FirstOrDefault().Winebillno;

            if (!string.IsNullOrEmpty(BillNo))
            {
                int lastno = int.Parse(BillNo.Substring(BillNo.LastIndexOf("/") + 1)) + 1;
                DateTime AprilDay = new DateTime(DateTime.Today.Year, 4, 01);
                int compareValue = AprilDay.CompareTo(DateTime.Today);
                string dateformat = "";
                string nextyear = "";
                switch (compareValue)
                {
                    case 0:
                        string paymentDate = context.tbl_WineBillingSection.OrderByDescending(obj => obj.Bill_Number).FirstOrDefault().PaymentDate;
                        if (paymentDate == AprilDay.ToString("MM/dd/yyyy"))
                        {
                            lastno = int.Parse(BillNo.Substring(BillNo.LastIndexOf("/") + 1)) + 1;
                        }
                        else
                        {
                            lastno = 1;
                        }
                        nextyear = DateTime.Now.AddYears(1).Year.ToString().Substring(2);
                        dateformat = "Wine/" + DateTime.Now.Year.ToString() + "-" + nextyear + "/00" + lastno;

                        break;
                    case -1:
                        string LastpaymentDate = context.tbl_WineBillingSection.OrderByDescending(obj => obj.Bill_Number).FirstOrDefault().PaymentDate;
                        int compare = AprilDay.CompareTo(Convert.ToDateTime(LastpaymentDate));
                        if (compare == 1)
                        {
                            lastno = 1;
                        }
                        else
                        {
                            lastno = int.Parse(BillNo.Substring(BillNo.LastIndexOf("/") + 1)) + 1;
                        }
                        nextyear = DateTime.Now.AddYears(1).Year.ToString().Substring(2);
                        dateformat = "Wine/" + DateTime.Now.Year.ToString() + "-" + nextyear + "/00" + lastno;
                        break;

                    case 1:
                        lastno = int.Parse(BillNo.Substring(BillNo.LastIndexOf("/") + 1)) + 1;

                        nextyear = DateTime.Now.Year.ToString().Substring(2);
                        dateformat = "Wine/" + DateTime.Now.AddYears(-compareValue).Year.ToString() + "-" + nextyear + "/00" + lastno;
                        break;
                }

                return dateformat;
            }
            return string.Empty;
        }
        [HttpPost]
        public JsonResult CreateOrder(MenusBillingSection model)
        {
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["BarGstPercent"]);
            double? amtWithoutTax = 0;
            double? gst = 0;
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");

            TimeSpan time = DateTime.Now.TimeOfDay;
            var tableDetails = context.tbl_TablesForBooking.SingleOrDefault(v => v.ID == model.TableID && v.Wine_Status == "closed");
            var IsTableAlreadyOpened = context.tbl_WineBillingSection.Where(o => o.TableID == model.TableID && o.Table_Status == "opened").SingleOrDefault();

            if (tableDetails != null && IsTableAlreadyOpened == null)
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
                var billDet = context.tbl_WineBillingSection.ToList().LastOrDefault();

                foreach (var f in model.MenusBillingDetailsWithBillNo)
                {
                    tbl_WineBillingDetailsWithBillNo menusdetails = new tbl_WineBillingDetailsWithBillNo();
                    menusdetails.BillNo = (billDet != null) ? billDet.Bill_Number + 1 : 1;
                    menusdetails.ItemName = f.FoodName;
                    menusdetails.Price = f.Price;
                    menusdetails.Quantity = f.Quantity;

                    menusdetails.OldQuantity = f.Quantity;

                    context.tbl_WineBillingDetailsWithBillNo.Add(menusdetails);
                    context.SaveChanges();

                    amtWithoutTax += (f.Price * f.Quantity);

                    var data = context.tbl_WineMenu.SingleOrDefault(a => a.ID == f.ItemNameID);    // && a.Price==model.Price);
                    if (data != null)
                    {
                        if (data.InventoryID != 0)
                        {

                            tbl_WineInventoryUsage usage = new tbl_WineInventoryUsage();
                            usage.WineInventoryID = data.InventoryID;
                            usage.Used_Qty = f.Quantity;
                            usage.Description = "(Bill No. : " + ((billDet != null) ? (billDet.Bill_Number + 1) : 1) + ") Sold to customer on " + DateFormat;
                            usage.Used_Date = DateFormat;
                            usage.Wine_BillNo = (billDet != null) ? billDet.Bill_Number + 1 : 1;
                            usage.Wine_MenusBillingDetailsID = menusdetails.ID;
                            usage.GST_NonGST_Bill = "GST";
                            context.tbl_WineInventoryUsage.Add(usage);
                            context.SaveChanges();
                        }
                    }

                    var consumeItem = context.tbl_Consumable_WineItems.Where(s => s.WineMenuItem_ID == f.ItemNameID).ToList();
                    if (consumeItem.Count > 0)
                    {
                        foreach (var m in consumeItem)
                        {
                            string tempUsedQty = Convert.ToString(f.Quantity * m.Quantity);
                            tbl_WineInventoryUsage usage = new tbl_WineInventoryUsage();
                            usage.WineInventoryID = m.Inventory_ID;
                            usage.Used_Qty = Convert.ToDouble(tempUsedQty);  //Math.Round((double)(f.Quantity * m.Quantity), 2);
                            usage.Description = "Bill No. : " + ((billDet != null) ? (billDet.Bill_Number + 1) : 1) + " | " + f.FoodName;
                            usage.Used_Date = DateFormat;
                            usage.Wine_BillNo = (billDet != null) ? billDet.Bill_Number + 1 : 1;
                            usage.Wine_MenusBillingDetailsID = menusdetails.ID;
                            usage.GST_NonGST_Bill = "GST";
                            context.tbl_WineInventoryUsage.Add(usage);
                            context.SaveChanges();
                        }
                    }
                }
                //gst = amtWithoutTax * (9f / 100f);
                //gst = (gst * 2);
                gst = amtWithoutTax * ((double)gstPercentFromConfig / (double)100);
                gst = Math.Round((double)gst, 2);
                amtWithoutTax = Math.Round((double)amtWithoutTax, 2);

                tbl_WineBillingSection menus = new tbl_WineBillingSection();
                menus.Bill_Number = (billDet != null) ? billDet.Bill_Number + 1 : 1;
                menus.Customer_Name = model.Customer_Name;
                menus.Phone = model.Phone;
                menus.Price = amtWithoutTax; //Math.Round((double)(gst + amtWithoutTax), 2);
                menus.UserID = model.UserID;
                menus.GST = 0.0;// gst;
                menus.PriceWithoutTax = amtWithoutTax;
                menus.TableID = model.TableID;
                menus.OrderTakenBy = model.OrderTakenBy;
                menus.Table_Status = "opened";
                menus.Winebillno = GetBillNO();
                menus.PaymentDate = DateFormat;
                menus.Order_Time = time;
                context.tbl_WineBillingSection.Add(menus);
                context.SaveChanges();

                var tblOrder = context.tbl_TablesForBooking.SingleOrDefault(d => d.ID == model.TableID);
                if (tblOrder != null)
                {
                    tblOrder.Wine_Status = "opened";
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
            var data = context.tbl_WineBillingSection.SingleOrDefault(p => p.TableID == tableID && p.Table_Status != "closed" && p.Table_Status != null);
            MenusBillingSection menus = new MenusBillingSection();
            List<MenusBillingDetailsWithBillNo> menusList = new List<MenusBillingDetailsWithBillNo>();
            if (data != null)
            {
                var lst = context.tbl_WineBillingDetailsWithBillNo.Where(s => s.BillNo == data.Bill_Number).ToList();
                foreach (var i in lst)
                {
                    menusList.Add(new MenusBillingDetailsWithBillNo()
                    {
                        ID = i.ID,
                        FoodName = i.ItemName,
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
            var menusData = context.tbl_WineBillingSection.SingleOrDefault(p => p.TableID == tableID && p.Table_Status == "opened");
            MenusBillingSection menus = new MenusBillingSection();
            List<MenusBillingDetailsWithBillNo> menusList = new List<MenusBillingDetailsWithBillNo>();
            if (menusData != null)
            {
                var lst = context.tbl_WineBillingDetailsWithBillNo.Where(s => s.BillNo == menusData.Bill_Number).ToList();
                foreach (var i in lst)
                {
                    bool alreadyExists = menusList.Any(x => x.FoodName == i.ItemName && x.Price == i.Price);
                    if (!alreadyExists)
                    {
                        menusList.Add(new MenusBillingDetailsWithBillNo()
                        {
                            ID = i.ID,
                            FoodName = i.ItemName,
                            Price = i.Price,
                            Quantity = i.Quantity
                        });
                    }
                    else
                    {
                        menusList.Find(p => p.FoodName == i.ItemName && p.Price == i.Price).Quantity += i.Quantity;
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
            var menusData = context.tbl_WineBillingSection.SingleOrDefault(p => p.Bill_Number == billNo);
            var tableData = context.tbl_TablesForBooking.SingleOrDefault(w => w.ID == menusData.TableID);
            menusData.Table_Status = "billed";
            context.Entry(menusData).State = EntityState.Modified;
            tableData.Wine_Status = "billed";
            context.Entry(tableData).State = EntityState.Modified;
            context.SaveChanges();

            List<MenusBillingDetailsWithBillNo> Lst = new List<MenusBillingDetailsWithBillNo>();
            var data = context.tbl_WineBillingDetailsWithBillNo.Where(s => s.BillNo == billNo).ToList();
            foreach (var i in data)
            {
                bool alreadyExists = Lst.Any(x => x.FoodName == i.ItemName && x.Price == i.Price);
                if (!alreadyExists)
                {
                    Lst.Add(new MenusBillingDetailsWithBillNo()
                    {
                        ID = i.ID,
                        FoodName = i.ItemName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    });
                }
                else
                {
                    Lst.Find(p => p.FoodName == i.ItemName && p.Price == i.Price).Quantity += i.Quantity;
                }
            }
            return Json(Lst);
        }

        [HttpPost]
        public JsonResult CloseOrder(int tableID, string paymentMode, string discount)
        {
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var menusData = context.tbl_WineBillingSection.SingleOrDefault(p => p.TableID == tableID && p.Table_Status == "billed");
            if (menusData != null)
            {
                var tableData = context.tbl_TablesForBooking.SingleOrDefault(w => w.ID == tableID);
                menusData.Table_Status = null;
                menusData.Mode_Of_Payment = paymentMode;
                menusData.Discount = Convert.ToDouble(discount);
                menusData.Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);
                context.Entry(menusData).State = EntityState.Modified;
                tableData.Wine_Status = "closed";
                context.Entry(tableData).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("closed");
        }

        [HttpPost]
        public JsonResult addMoreNewItemsInDB(MenusBillingSection model)
        {
            var gstPercentFromConfig = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["BarGstPercent"]);
            double? amtWithoutTax = 0;
            double? gst = 0;
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");

            var billDet = context.tbl_WineBillingSection.SingleOrDefault(s => s.Bill_Number == model.Bill_Number);
            if (billDet != null)
            {
                foreach (var f in model.MenusBillingDetailsWithBillNo)
                {
                    tbl_WineBillingDetailsWithBillNo menusdetails = new tbl_WineBillingDetailsWithBillNo();
                    menusdetails.BillNo = model.Bill_Number;
                    menusdetails.ItemName = f.FoodName;
                    menusdetails.Price = f.Price;
                    menusdetails.Quantity = f.Quantity;

                    menusdetails.OldQuantity = f.Quantity;

                    context.tbl_WineBillingDetailsWithBillNo.Add(menusdetails);
                    context.SaveChanges();

                    amtWithoutTax += (f.Price * f.Quantity);

                    var data = context.tbl_WineMenu.SingleOrDefault(a => a.ID == f.ItemNameID);    // && a.Price==model.Price);
                    if (data != null)
                    {
                        if (data.InventoryID != 0)
                        {

                            tbl_WineInventoryUsage usage = new tbl_WineInventoryUsage();
                            usage.WineInventoryID = data.InventoryID;
                            usage.Used_Qty = f.Quantity;
                            usage.Description = "(Bill No. : " + model.Bill_Number + ") Sold to customer on " + DateFormat;
                            usage.Used_Date = DateFormat;
                            usage.Wine_BillNo = model.Bill_Number;
                            usage.Wine_MenusBillingDetailsID = menusdetails.ID;
                            usage.GST_NonGST_Bill = "GST";
                            context.tbl_WineInventoryUsage.Add(usage);
                            context.SaveChanges();
                        }
                    }

                    var consumeItem = context.tbl_Consumable_WineItems.Where(s => s.WineMenuItem_ID == f.ItemNameID).ToList();
                    if (consumeItem.Count > 0)
                    {
                        foreach (var m in consumeItem)
                        {
                            string tempUsedQty = Convert.ToString(f.Quantity * m.Quantity);
                            tbl_WineInventoryUsage usage = new tbl_WineInventoryUsage();
                            usage.WineInventoryID = m.Inventory_ID;
                            usage.Used_Qty = Convert.ToDouble(tempUsedQty);
                            //usage.Used_Qty = f.Quantity * m.Quantity;   // Math.Round((double)(f.Quantity * m.Quantity), 2);
                            usage.Description = "Bill No. : " + model.Bill_Number + " | " + f.FoodName;
                            usage.Used_Date = DateFormat;
                            usage.Wine_BillNo = model.Bill_Number;
                            usage.Wine_MenusBillingDetailsID = menusdetails.ID;
                            usage.GST_NonGST_Bill = "GST";
                            context.tbl_WineInventoryUsage.Add(usage);
                            context.SaveChanges();
                        }
                    }
                }

                amtWithoutTax = amtWithoutTax + billDet.PriceWithoutTax;

                //gst = amtWithoutTax * (9f / 100f);
                //gst = (gst * 2);
                gst = amtWithoutTax * ((double)gstPercentFromConfig / (double)100);
                gst = Math.Round((double)gst, 2);
                amtWithoutTax = Math.Round((double)amtWithoutTax, 2);

                tbl_WineBillingSection menus = new tbl_WineBillingSection();
                billDet.Price = amtWithoutTax;// Math.Round((double)(gst + amtWithoutTax), 2);
                billDet.GST = 0.0;//gst;
                billDet.PriceWithoutTax = amtWithoutTax;
                context.Entry(billDet).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Saved");
        }

        public JsonResult DelItemFromMenusBillingSection(int menusBillingDetailsID)
        {
            var data = context.tbl_WineBillingDetailsWithBillNo.SingleOrDefault(s => s.ID == menusBillingDetailsID);
            List<MenusBillingDetailsWithBillNo> menusDetailList = new List<MenusBillingDetailsWithBillNo>();
            if (data != null)
            {
                int? billNo = data.BillNo;
                context.Entry(data).State = EntityState.Deleted;
                context.SaveChanges();

                var invData = context.tbl_WineInventoryUsage.Where(a => a.Wine_MenusBillingDetailsID == menusBillingDetailsID && a.GST_NonGST_Bill == "GST").ToList();
                if (invData.Count() > 0)
                {
                    foreach (var i in invData)
                    {
                        context.Entry(i).State = EntityState.Deleted;
                        context.SaveChanges();
                    }
                }
                calculateAmount(billNo);

                var lst = context.tbl_WineBillingDetailsWithBillNo.Where(s => s.BillNo == billNo).ToList();
                foreach (var i in lst)
                {
                    menusDetailList.Add(new MenusBillingDetailsWithBillNo()
                    {
                        ID = i.ID,
                        FoodName = i.ItemName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    });
                }
            }
            return Json(menusDetailList);
        }

        public JsonResult UpdateBillByBillNo(int Bill_Number, string CustomerName, string Phone, string payMode)
        {
            var data = context.tbl_WineBillingSection.SingleOrDefault(x => x.Bill_Number == Bill_Number);
            if (data != null)
            {
                data.Customer_Name = CustomerName;
                data.Phone = Phone;
                data.Mode_Of_Payment = payMode;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
                return Json("updated");
            }
            return Json("");
        }
    }
}
