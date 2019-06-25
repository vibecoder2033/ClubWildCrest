using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WildCrest.Models.WildCrestModels;

namespace WildCrest.Controllers.SuperAdmin
{
    public class BuffetMenuController : Controller
    {
        //
        // GET: /BuffetMenu/
        public class PartyfoodMenu
        {
            public string Buffet_Item_Name { get; set; }
            public double? Consumption_Cost { get; set; }

        }
        ClubWildCrestEntities context = new ClubWildCrestEntities();
        // GET: MenuItems
        #region Buffet MenuItem
        [Authorize(Roles = "1,2")]
        public ActionResult Index()
        {
            List<MenuItems> items = new List<MenuItems>();
            if (Request.Cookies["PageSetting"] != null)
            {
                if (Request.Cookies["PageSetting"]["MenuItemsPermission"] != "None")
                {
                    var data = context.tbl_Buffet_Menu.OrderByDescending(a => a.ID).Where(s => s.NewItemApprvFrmSuperAdm != false && s.DelItemApprvFromSuperAdm != true).ToList();

                    foreach (var i in data)
                    {
                        var menuItemConsumable = context.tbl_Buffet_ConsumableItems.Where(a => a.BuffetItem_ID == i.ID).ToList();
                        var price = 0.0;
                        foreach (var j in menuItemConsumable)
                        {
                            price += Convert.ToDouble(context.tbl_Inventory.FirstOrDefault(a => a.ID == j.Inventory_ID).Price * j.Quantity);
                        }
                        var buffedata = context.tbl_Buffet_Menu.SingleOrDefault(a => a.ID == i.ID);
                        if (buffedata != null)
                        {
                            if (buffedata.InventoryID > 0)
                            {
                                buffedata.Consumption_Cost = context.tbl_Inventory.SingleOrDefault(x => x.ID == buffedata.InventoryID).Price;
                                context.Entry(buffedata).State = EntityState.Modified;
                                context.SaveChanges();
                            }
                            else
                            {
                                buffedata.Consumption_Cost = price;
                                context.Entry(buffedata).State = EntityState.Modified;
                                context.SaveChanges();
                            }
                        }
                      
                            items.Add(new MenuItems()
                            {
                                ID = i.ID,
                                Food_Item_Name = i.Buffet_Item_Name,
                                Details = i.Details,
                                Price = i.Consumption_Cost ?? 0.0,
                                FoodType = i.FoodType,
                                AddedBy = i.AddedBy,
                                InventoryID = i.InventoryID
                            });
                        
                    }
                    return View(items);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            else
            {
                var data = context.tbl_Buffet_Menu.OrderByDescending(a => a.ID).Where(s => s.NewItemApprvFrmSuperAdm != false && s.DelItemApprvFromSuperAdm != true).ToList();

                foreach (var i in data)
                {
                    var menuItemConsumable = context.tbl_Buffet_ConsumableItems.Where(a => a.BuffetItem_ID == i.ID).ToList();
                    var price = 0.0;
                    foreach (var j in menuItemConsumable)
                    {
                        price += Convert.ToDouble(context.tbl_Inventory.FirstOrDefault(a => a.ID == j.Inventory_ID).Price * j.Quantity);
                    }
                    var buffedata = context.tbl_Buffet_Menu.SingleOrDefault(a => a.ID == i.ID);
                    if (buffedata != null)
                    {
                        if (buffedata.InventoryID > 0)
                        {
                            buffedata.Consumption_Cost = context.tbl_Inventory.SingleOrDefault(x => x.ID == buffedata.InventoryID).Price;
                            context.Entry(buffedata).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                        else
                        {
                            buffedata.Consumption_Cost = price;
                            context.Entry(buffedata).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                   

                        items.Add(new MenuItems()
                        {
                            ID = i.ID,
                            Food_Item_Name = i.Buffet_Item_Name,
                            Details = i.Details,
                            Price = i.Consumption_Cost ?? 0.0,
                            FoodType = i.FoodType,
                            AddedBy = i.AddedBy,
                            InventoryID = i.InventoryID
                        });
                    
                }
                return View(items);
            }

        }

        [Authorize(Roles = "1")]
        public ActionResult NewFoodItemApprovalBySuperAdmin()
        {
            var data = context.tbl_Buffet_Menu.OrderByDescending(a => a.ID).Where(s => s.NewItemApprvFrmSuperAdm == false).ToList();
            List<MenuItems> items = new List<MenuItems>();
            foreach (var i in data)
            {
                items.Add(new MenuItems()
                {
                    ID = i.ID,
                    Food_Item_Name = i.Buffet_Item_Name,
                    Details = i.Details,
                    Price = i.Consumption_Cost ?? 0.0,
                    FoodType = i.FoodType,
                    AddedBy = i.AddedBy
                });
            }
            return View(items);
        }

        [Authorize(Roles = "1")]
        public ActionResult DeleteFoodItemBySuperAdminApproval()
        {
            var data = context.tbl_Buffet_Menu.OrderByDescending(a => a.ID).Where(s => s.DelItemApprvFromSuperAdm == true).ToList();
            List<MenuItems> items = new List<MenuItems>();
            foreach (var i in data)
            {
                items.Add(new MenuItems()
                {
                    ID = i.ID,
                    Food_Item_Name = i.Buffet_Item_Name,
                    Details = i.Details,
                    Price = i.Consumption_Cost ?? 0.0,
                    FoodType = i.FoodType,
                    DeletedBy = i.DeletedBy
                });
            }
            return View(items);
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public ActionResult AddNewFoodItem()
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
            //var invData = context.tbl_Inventory.ToList();
            var invData = context.tbl_Inventory.Where(c => !context.tbl_Menu.Select(b => b.InventoryID).Contains(c.ID)).OrderBy(s => s.Item_Name);
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

            if (Request.Cookies["PageSetting"] != null)
            {
                if (Request.Cookies["PageSetting"]["MenuItemsPermission"] == "All")
                {
                    return View(typeList);
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            else
            {
                return View(typeList);
            }

        }

        [HttpPost]
        public JsonResult AddNewFoodItem(MenuItems menu)
        {
            int currentUserTypeLoggedin = Convert.ToInt32(Request.Cookies["UserType"].Value);
            tbl_Buffet_Menu item = new tbl_Buffet_Menu();
            item.Buffet_Item_Name = menu.Food_Item_Name;
            item.Details = menu.Details;
            //item.Price = menu.Price;
            
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
            context.tbl_Buffet_Menu.Add(item);
            context.SaveChanges();
            return Json(currentUserTypeLoggedin);
        }

        [Authorize(Roles = "1,2")]
        public ActionResult FoodItemEditByID(int id, string editDetail)
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
            if (Request.Cookies["PageSetting"] != null)
            {

                var data = context.tbl_Buffet_Menu.SingleOrDefault(i => i.ID == id);
                MenuItems item = new MenuItems();
                item.ID = data.ID;
                item.Food_Item_Name = data.Buffet_Item_Name;
                item.Details = data.Details;
                //item.Price = data.Price;
                item.FoodType = data.FoodType;
                if (Request.Cookies["PageSetting"]["MenuItemsPermission"] == "All")
                {
                    if (editDetail == "Details")
                    {
                        return View("~/Views/BuffetMenu/MenuItemDetailsByID.cshtml", item);
                    }
                    else
                    {
                        return View("~/Views/BuffetMenu/FoodItemEditByID.cshtml", item);
                    }
                }
                else if (Request.Cookies["PageSetting"]["MenuItemsPermission"] == "View")
                {
                    if (editDetail == "Details")
                    {
                        return View("~/Views/BuffetMenu/MenuItemDetailsByID.cshtml", item);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }
                }

                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            else
            {
                var data = context.tbl_Buffet_Menu.SingleOrDefault(i => i.ID == id);
                MenuItems item = new MenuItems();
                item.ID = data.ID;
                item.Food_Item_Name = data.Buffet_Item_Name;
                item.Details = data.Details;
                //item.Price = data.Price;
                item.FoodType = data.FoodType;
                if (editDetail == "Details")
                {
                    return View("~/Views/BuffetMenu/MenuItemDetailsByID.cshtml", item);
                }
                else
                {
                    return View("~/Views/BuffetMenu/FoodItemEditByID.cshtml", item);
                }
            }

        }

        public ActionResult NewDelFoodItemEditByID(int id, string editDetail)
        {
            var data = context.tbl_Buffet_Menu.SingleOrDefault(i => i.ID == id);
            MenuItems item = new MenuItems();
            item.ID = data.ID;
            item.Food_Item_Name = data.Buffet_Item_Name;
            item.Details = data.Details;
            //item.Price = data.Price;
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
            var data = context.tbl_Buffet_Menu.Find(menu.ID);
            if (data != null)
            {
                data.Buffet_Item_Name = menu.Food_Item_Name;
                data.Details = menu.Details;
                //data.Price = menu.Price;
                data.FoodType = menu.FoodType;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Updated.");
        }

        [HttpPost]
        public JsonResult DeleteFoodItemById(int id)
        {
            int currentUserTypeLoggedin = Convert.ToInt32(Request.Cookies["UserType"].Value);
            if (currentUserTypeLoggedin == 1)
            {
                var findUser = context.tbl_Buffet_Menu.SingleOrDefault(u => u.ID == id);

                if (findUser != null)
                {
                    context.Entry(findUser).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
            else
            {
                var findUser = context.tbl_Buffet_Menu.SingleOrDefault(u => u.ID == id);
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
        public JsonResult ApproveFoodItem(int id)
        {
            var data = context.tbl_Buffet_Menu.SingleOrDefault(s => s.ID == id);
            if (data != null)
            {
                data.NewItemApprvFrmSuperAdm = true;
                context.Entry(data).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Approved");
        }

        [HttpPost]
        public JsonResult CancelDeleteFoodItemByID(int id)
        {
            var findUser = context.tbl_Buffet_Menu.SingleOrDefault(u => u.ID == id);
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

        public JsonResult InventoryDetailsByID(int id)
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
            return Json(inv);
        }

        public ActionResult ConsumablesItem()
        {
            //var consumableItemsList = (from menu in context.tbl_Menu
            //                           join consumable in context.tbl_ConsumableItems on menu.ID equals consumable.MenuItem_ID
            //                           select new MenuItems
            //                           {
            //                               ID = menu.ID,
            //                               Food_Item_Name = menu.Food_Item_Name
            //                           }).Distinct().ToList();

            var consumableItemsList = (from menu in context.tbl_Buffet_Menu
                                       join consumable in context.tbl_Buffet_ConsumableItems on menu.ID equals consumable.BuffetItem_ID
                                       select new MenuItems
                                       {
                                           ID = menu.ID,
                                           Food_Item_Name = menu.Buffet_Item_Name,
                                           ConsumableID = consumable.ID
                                       }).ToList();
            var lst = consumableItemsList.GroupBy(s => s.ID).OrderByDescending(d => d.Max(a => a.ConsumableID)).Select(f => f.FirstOrDefault()).ToList();


            ViewBag.MenuItemsConsumableList = lst;
            return View();
        }

        public ActionResult AddNewConsumableItem()
        {
            var menuItems_Data = context.tbl_Buffet_Menu.Where(m => m.InventoryID == 0).ToList();
            var inventoryItems = context.tbl_Inventory.ToList();
            var menuItems = (from m in menuItems_Data
                             where !context.tbl_Buffet_ConsumableItems.Any(i => i.BuffetItem_ID == m.ID)
                             select m).ToList();
            List<MenuItems> menusList = new List<MenuItems>();
            foreach (var v in menuItems)
            {
                menusList.Add(new MenuItems()
                {
                    ID = v.ID,
                    Food_Item_Name = v.Buffet_Item_Name
                });
            }
            ViewBag.MenuItems = menusList.OrderBy(s => s.Food_Item_Name).ToList();
            ViewBag.InventoryItems = inventoryItems.OrderBy(s => s.Item_Name).ToList();
            return View();
        }

        [HttpPost]
        public JsonResult SaveConsumableQuantity(List<Buffet_ConsumableItems> foodList)
        {
         int  ? BuffetItem_ID = 0;
            foreach (var i in foodList)
            {
                //var d = context.tbl_ConsumableItems.SingleOrDefault(s => s.MenuItem_ID == i.MenuItem_ID && s.Inventory_ID == i.Inventory_ID);
                //if (d != null)
                //{
                //    d.Quantity += i.Quantity;
                //    context.Entry(d).State = EntityState.Modified;
                //    context.SaveChanges();
                //}
                //else
                //{
                tbl_Buffet_ConsumableItems consumable = new tbl_Buffet_ConsumableItems();
                BuffetItem_ID =i.BuffetItem_ID;
                consumable.Inventory_ID = i.Inventory_ID;
                consumable.BuffetItem_ID = i.BuffetItem_ID;
                consumable.MeasurementUnit = i.Measurement;
                consumable.Quantity = i.Quantity;
                context.tbl_Buffet_ConsumableItems.Add(consumable);
                
                context.SaveChanges();

               
                //}
            }
            var menuItemConsumable = context.tbl_Buffet_ConsumableItems.Where(a => a.BuffetItem_ID == BuffetItem_ID).ToList();
            var price = 0.0;
            foreach (var i in menuItemConsumable)
            {
                price += Convert.ToDouble(context.tbl_Inventory.FirstOrDefault(a => a.ID == i.Inventory_ID).Price * i.Quantity);
            }
            var buffedata = context.tbl_Buffet_Menu.SingleOrDefault(a => a.ID == BuffetItem_ID);
            if (buffedata != null)
            {
                buffedata.Consumption_Cost = price;
                context.Entry(buffedata).State = EntityState.Modified;
                context.SaveChanges();
            }
            return Json("Saved");
        }

        [HttpPost]
        public JsonResult DeleteConsumableItemByMenuId(int menuId)
        {
            var menuItemConsumable = context.tbl_Buffet_ConsumableItems.Where(a => a.BuffetItem_ID == menuId).ToList();
            foreach (var i in menuItemConsumable)
            {
                context.Entry(i).State = EntityState.Deleted;
                context.SaveChanges();
            }
            
                var buffedata = context.tbl_Buffet_Menu.SingleOrDefault(a => a.ID == menuId);
                if (buffedata != null)
                {
                    buffedata.Consumption_Cost = 0.0;
                    context.Entry(buffedata).State = EntityState.Modified;
                    context.SaveChanges();
                }
            
            return Json("Deleted");
        }

        [HttpPost]
        public JsonResult DetailsOfConsumableItemByMenuId(int menuId)
        {
            try
            {
                var price = 0.0;
                var menuItemConsumable = context.tbl_Buffet_ConsumableItems.Where(a => a.BuffetItem_ID == menuId).ToList();
                List<Buffet_ConsumableItems> items = new List<Buffet_ConsumableItems>();
                foreach (var i in menuItemConsumable)
                {
                     price += Convert.ToDouble(context.tbl_Inventory.FirstOrDefault(a => a.ID == i.Inventory_ID).Price * i.Quantity);
                    

                    var invName = context.tbl_Inventory.SingleOrDefault(s => s.ID == i.Inventory_ID);
                    //items.Add(new ConsumableItems()
                    //{
                    //    Inventory_ItemName = (invName != null) ? invName.Item_Name : "",
                    //    Quantity = i.Quantity,
                    //    Measurement = i.MeasurementUnit
                    //});
                    bool alreadyExists = items.Any(x => x.BuffetItem_ID == i.BuffetItem_ID && x.Inventory_ID == i.Inventory_ID);
                    if (!alreadyExists)
                    {
                        items.Add(new Buffet_ConsumableItems()
                        {
                            Inventory_ItemName = (invName != null) ? invName.Item_Name : "",
                            Quantity = i.Quantity,
                            Measurement = i.MeasurementUnit,
                            Inventory_ID = i.Inventory_ID,
                            BuffetItem_ID = i.BuffetItem_ID,
                            Price = (context.tbl_Inventory.FirstOrDefault(a => a.ID == i.Inventory_ID).Price * i.Quantity).ToString()
                        });

                    }
                    else
                    {
                        items.Find(p => p.BuffetItem_ID == i.BuffetItem_ID && p.Inventory_ID == i.Inventory_ID).Quantity += i.Quantity;
                    }
                }
                var buffedata = context.tbl_Buffet_Menu.SingleOrDefault(a => a.ID == menuId);
                if (buffedata != null)
                {
                    buffedata.Consumption_Cost = price;
                    context.Entry(buffedata).State = EntityState.Modified;
                    context.SaveChanges();
                }
                return Json(items);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return Json("");
        }

        public ActionResult ConsumableFoodItemEditByID(int menuId)
        {
            var menuItemConsumable = context.tbl_Buffet_ConsumableItems.Where(a => a.BuffetItem_ID == menuId).ToList();
            List<Buffet_ConsumableItems> items = new List<Buffet_ConsumableItems>();
            string mName = "";
            foreach (var i in menuItemConsumable)
            {
                var invName = context.tbl_Inventory.SingleOrDefault(s => s.ID == i.Inventory_ID);
                var menuName = context.tbl_Buffet_Menu.SingleOrDefault(s => s.ID == i.BuffetItem_ID);
                items.Add(new Buffet_ConsumableItems()
                {
                    Inventory_ItemName = (invName != null) ? invName.Item_Name : "",
                    Quantity = i.Quantity,
                    Measurement = i.MeasurementUnit,
                    ID = i.ID,
                    BuffetItem_ID = i.BuffetItem_ID,
                });
                mName = (menuName != null) ? menuName.Buffet_Item_Name : "";
            }
            ViewBag.Menu_Item_Name = mName;
            ViewBag.Menu_Item_ID = menuId;
            //------------------------------------------------------------------------------
            var inventoryItems = context.tbl_Inventory.ToList();
            var invItems = (from m in inventoryItems
                            where !context.tbl_Buffet_ConsumableItems.Any(i => i.BuffetItem_ID == menuId && i.Inventory_ID == m.ID)
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
        public JsonResult UpdateQuantityOfConsumableItem(int idOfConsumableItem, double? quantity,int Buffet_Menu_Id)
        {
            var item = context.tbl_Buffet_ConsumableItems.Find(idOfConsumableItem);
            if (item != null)
            {
                item.Quantity = (quantity != null) ? quantity : 0;
                context.Entry(item).State = EntityState.Modified;
                context.SaveChanges();
                var menuItemConsumable = context.tbl_Buffet_ConsumableItems.Where(a => a.BuffetItem_ID == Buffet_Menu_Id).ToList();
                var price = 0.0;
                foreach (var i in menuItemConsumable)
                {
                    price += Convert.ToDouble(context.tbl_Inventory.FirstOrDefault(a => a.ID == i.Inventory_ID).Price * i.Quantity);
                }
                var buffedata = context.tbl_Buffet_Menu.SingleOrDefault(a => a.ID == Buffet_Menu_Id);
                if (buffedata != null)
                {
                    buffedata.Consumption_Cost = price;
                    context.Entry(buffedata).State = EntityState.Modified;
                    context.SaveChanges();
                }
                return Json("Quantity Updated.");
            }
            
            return Json("Data not Found.");
        }

        [HttpPost]
        public JsonResult DeleteQuantityOfConsumableItem(List<int> selectedItems,int Buffet_Menu_Id)
        {
            foreach (var id in selectedItems)
            {
                var data = context.tbl_Buffet_ConsumableItems.SingleOrDefault(s => s.ID == id);
                if (data != null)
                {
                    context.Entry(data).State = EntityState.Deleted;
                    context.SaveChanges();
                    
                }
                var menuItemConsumable = context.tbl_Buffet_ConsumableItems.Where(a => a.BuffetItem_ID == Buffet_Menu_Id).ToList();
                var price = 0.0;
                foreach (var i in menuItemConsumable)
                {
                    price += Convert.ToDouble(context.tbl_Inventory.FirstOrDefault(a => a.ID == i.Inventory_ID).Price * i.Quantity);
                }
                var buffedata = context.tbl_Buffet_Menu.SingleOrDefault(a => a.ID == Buffet_Menu_Id);
                if (buffedata != null)
                {
                    buffedata.Consumption_Cost = price;
                    context.Entry(buffedata).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            return Json("Deleted.");
        }

        [HttpPost]
        public JsonResult getMeasurementUnit(int inventoryID)
        {
            var mUnit = context.tbl_Inventory.SingleOrDefault(s => s.ID == inventoryID);
            string unit = "";
            if (mUnit != null)
            {
                unit = mUnit.Measurement;
            }
            return Json(unit);
        }
        #endregion
        #region Buffet Party Clander
       
        [Authorize(Roles = "1,2")]
        public ActionResult PartyClander()
        {
            List<string> dates = new List<string>();
            ViewBag.FoodMenu = context.tbl_Buffet_Menu.ToList(); //new SelectList(context.tbl_Buffet_Menu.ToList(), "Consumption_Cost", "Buffet_Item_Name");
            var data = context.tbl_Party.ToList();
            if (data != null)
            {
                foreach (var d in data)
                {
                    dates.Add(d.Party_Date);
                }
            }
            var array = dates.ToArray();
            ViewBag.Dates = dates;
                return View();
            

        }
      [Authorize(Roles = "1,2")]
        public ActionResult Booked_Party(string PartyDate)
        {

            ViewBag.partydetails = context.tbl_Party.Where(x => x.Party_Date == PartyDate).ToList();
            ViewBag.partyBookDate = PartyDate;
            return View();


        }
         [Authorize(Roles = "1,2")]
        public ActionResult PartyDetailsByID(int id=0)
        {
            List<tbl_Buffet_Menu> menus = new List<tbl_Buffet_Menu>();
            ViewBag.partydetailsbyid = context.tbl_Party.Where(x => x.ID == id).FirstOrDefault();
            var data = context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == id && x.IsAdvance_Payment == true);
            if (data != null)
            {
                string advancepay = context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == id && x.IsAdvance_Payment == true).Amount_Paid;
                ViewBag.advancePay = advancepay;
                var Tdata = context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == id && x.IsAdvance_Payment == false);
                if (Tdata != null)
                {
                    var balanceamount = Math.Round((double.Parse(context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == id && x.IsAdvance_Payment == false).Total_Amount) - double.Parse(advancepay)), 2);
                    ViewBag.balanceAmt = balanceamount;
                }
                else
                {
                    ViewBag.balanceAmt = 0.0;

                }
            }
            else
            {ViewBag.advancePay=0.0;
            ViewBag.balanceAmt = 0.0;
            }
            var menulist = context.tbl_Party_FoodMenu.Where(x => x.Party_ID == id).ToList();
            if (menulist != null)
            {
                foreach (var d in menulist)
                {
                    if (context.tbl_Buffet_Menu.Where(x => x.ID == d.Buffet_Menu_Id).Any())
                    {
                        menus.Add(new tbl_Buffet_Menu()
                        {
                            Buffet_Item_Name = context.tbl_Buffet_Menu.SingleOrDefault(x => x.ID == d.Buffet_Menu_Id).Buffet_Item_Name,
                            Consumption_Cost = context.tbl_Buffet_Menu.SingleOrDefault(x => x.ID == d.Buffet_Menu_Id).Consumption_Cost
                        });
                    }
                }
            }
           
           
            ViewBag.foodmenus = menus;
            return View();


        }
        [Authorize(Roles = "1,2")]
        public ActionResult AddNewParty(string pDate)
        {
            ViewBag.partyFoodMenu = context.tbl_Buffet_Menu.ToList();
            ViewBag.partyDate = pDate;
            return View();


        }
        
        [HttpPost]
        public JsonResult IsPartyAvilable(string sDate = "")
        {
            var data = context.tbl_Party.Where(x => x.Party_Date == sDate).ToList();
            if (data != null&&data.Count!=0)
            {
                ViewBag.partydetails = context.tbl_Party.Where(x => x.Party_Date == sDate).ToList();
                return Json("Already Booked");
            }
            else
            {
                return Json("Avilable");
            }
            
        }
        public JsonResult AddParty(partyModel PartyModel, IEnumerable<PartyfoodMenu> PartyFoods)
        {

                tbl_Party model = new tbl_Party();
                model.Party_Name = PartyModel.Party_Name;
                model.Party_Owner = PartyModel.Party_Owner;
                model.Phone_No = PartyModel.Phone_No??"";
                model.Description = PartyModel.Description??"";
                model.Total_Member = PartyModel.Total_Member;
                model.Gst = (Convert.ToDouble(PartyModel.Price) * (Convert.ToDouble(PartyModel.Gst) / (double)100)).ToString();
                model.Party_Date = PartyModel.Party_Date;
                //var price = (double.Parse(PartyModel.Price) + double.Parse(model.Gst)).ToString();
                model.Price = PartyModel.Price;
                model.Total_Amount = ((Convert.ToDouble(PartyModel.Total_Member)* Convert.ToDouble(model.Price))).ToString();
                context.tbl_Party.Add(model);
                context.SaveChanges();
               
             foreach (var data in PartyFoods)
                {
                    int id = context.tbl_Buffet_Menu.SingleOrDefault(x => x.Buffet_Item_Name == data.Buffet_Item_Name && x.Consumption_Cost == data.Consumption_Cost).ID;
                    tbl_Party_FoodMenu party = new tbl_Party_FoodMenu();
                    party.Buffet_Menu_Id = id;
                    party.Party_ID = context.tbl_Party.ToList().LastOrDefault().ID;
                    context.tbl_Party_FoodMenu.Add(party);
                    context.SaveChanges();
                }
              
            
            return Json("Saved");
        }
        #endregion
        #region Buffet Party Billing
        [Authorize(Roles = "1,2")]
        public ActionResult Billing()
        {
            ViewBag.PartyDtl = context.tbl_Party.ToList().OrderByDescending(x=>x.ID);
            
            return View();


        }
        
            [HttpPost]
        public JsonResult foodMenuByPartyid(int partyID)
        {
            List<tbl_Buffet_Menu> menus = new List<tbl_Buffet_Menu>();
            var menulist = context.tbl_Party_FoodMenu.Where(x => x.Party_ID == partyID).ToList();
            if (menulist != null)
            {

                foreach (var d in menulist)
                {
                    if (context.tbl_Buffet_Menu.Where(x => x.ID == d.Buffet_Menu_Id).Any())
                    {
                        menus.Add(new tbl_Buffet_Menu()
                        {


                            Buffet_Item_Name = context.tbl_Buffet_Menu.SingleOrDefault(x => x.ID == d.Buffet_Menu_Id).Buffet_Item_Name,
                            Consumption_Cost = context.tbl_Buffet_Menu.SingleOrDefault(x => x.ID == d.Buffet_Menu_Id).Consumption_Cost
                        });
                    }
                }
                return Json(menus);
            }
            return Json("");
               
            }
       [Authorize(Roles = "1,2")]
        public ActionResult ShowBillingByID(int id)
        {
            partyModel li = new partyModel();

            var data = context.tbl_Party.SingleOrDefault(x => x.ID == id);
            var count = context.tbl_PartyBilling.Where(x => x.Party_ID == data.ID).ToList().Count;
            switch (count)
            {
                case 0:
                    li.ID = data.ID;

                li.Price = data.Price;
                li.Gst = data.Gst;

                li.Party_Date = data.Party_Date;
                li.Party_Name = data.Party_Name;
                li.Party_Owner = data.Party_Owner;
                li.Phone_No = data.Phone_No;
                li.Total_Amount = data.Total_Amount;
                li.Total_Member = data.Total_Member;
                li.amount_tobepay = data.Total_Amount;
                li.ModeOfPayment = "Cash";
                    break;
                case 1:
                 var billdata = data != null ? context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == data.ID && x.IsAdvance_Payment == true) : null;
                if (billdata != null)
                {
                    li.BillNo = billdata.Bill_ID;
                    li.amount_tobepay = (double.Parse(billdata.Total_Amount) - double.Parse(billdata.Amount_Paid)).ToString();
                    li.IsAdvance_Payment = billdata.IsAdvance_Payment;
                    li.ID = Convert.ToInt32(billdata.Party_ID);

                    li.Price = billdata.Price_With_Gst;
                    li.Gst = billdata.Gst_Amount;
                    li.AdvancePay = billdata.Amount_Paid;
                    li.Party_Date = data.Party_Date;
                    li.Party_Name = data.Party_Name;
                    li.Party_Owner = data.Party_Owner;
                    li.Phone_No = data.Phone_No;
                    li.Total_Amount = billdata.Total_Amount;
                    li.Total_Member = billdata.Qty;
                    li.ModeOfPayment = billdata.Mode_Of_Payment;
                    if (billdata.Total_Amount == billdata.Amount_Paid)
                    {
                        ViewBag.Paid = "amtpaid";
                    }
                }
                else
                {
                    var billdatatotal = data != null ? context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == data.ID && x.IsAdvance_Payment == false) : null;
                    if (billdatatotal != null)
                    {
                        li.BillNo = billdatatotal.Bill_ID;
                        li.amount_tobepay = billdatatotal.Amount_Paid;
                        li.IsAdvance_Payment = billdatatotal.IsAdvance_Payment;
                        li.ID = Convert.ToInt32(billdatatotal.Party_ID);

                        li.Price = billdatatotal.Price_With_Gst;
                        li.Gst = billdatatotal.Gst_Amount;
                        li.AdvancePay = (double.Parse(billdatatotal.Total_Amount) - double.Parse(billdatatotal.Amount_Paid)).ToString(); 
                        li.Party_Date = data.Party_Date;
                        li.Party_Name = data.Party_Name;
                        li.Party_Owner = data.Party_Owner;
                        li.Phone_No = data.Phone_No;
                        li.Total_Amount = billdatatotal.Total_Amount;
                        li.Total_Member = billdatatotal.Qty;
                        li.ModeOfPayment = billdatatotal.Mode_Of_Payment;
                        if (billdatatotal.Total_Amount == billdatatotal.Amount_Paid)
                        {
                            ViewBag.Paid = "amtpaid";
                        }
                    }

                }
                    break;
                case 2:
                    var finalbilldata = data != null ? context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == data.ID && x.IsAdvance_Payment == false) : null;
                if (finalbilldata != null)
                {
                    li.BillNo = finalbilldata.Bill_ID;
                    li.amount_tobepay = finalbilldata.Amount_Paid; 
                  
                    li.ID = Convert.ToInt32(finalbilldata.Party_ID);
                     li.IsAdvance_Payment = finalbilldata.IsAdvance_Payment;
                    li.Price = finalbilldata.Price_With_Gst;
                    li.Gst = finalbilldata.Gst_Amount;
                    li.AdvancePay = (double.Parse(finalbilldata.Total_Amount) - double.Parse(finalbilldata.Amount_Paid)).ToString(); 
                    li.Party_Date = data.Party_Date;
                    li.Party_Name = data.Party_Name;
                    li.Party_Owner = data.Party_Owner;
                    li.Phone_No = data.Phone_No;
                    li.Total_Amount = finalbilldata.Total_Amount;
                    li.Total_Member = finalbilldata.Qty;
                    li.ModeOfPayment = finalbilldata.Mode_Of_Payment;
                    ViewBag.Paid = "amtpaid";
                }
                    break;
            }
            
            
            return PartialView("~/Views/BuffetMenu/_Partybillsdata.cshtml", li);


        }
        [Authorize(Roles = "1,2")]
        public ActionResult ShowBillingByDate(string partydate)
        {

            partyModel li = new partyModel();

            //var data = context.tbl_Party.SingleOrDefault(x => x.Party_Date == partydate);

            var party_data = context.tbl_Party.Where(x => x.Party_Date == partydate).ToList();
            var data = party_data != null ? party_data.LastOrDefault() : null;
            var count =data!=null? context.tbl_PartyBilling.Where(x => x.Party_ID == data.ID).ToList().Count:-1;
            switch (count)
            {
                case 0:
                    li.ID = data.ID;

                    li.Price = data.Price;
                    li.Gst = data.Gst;

                    li.Party_Date = data.Party_Date;
                    li.Party_Name = data.Party_Name;
                    li.Party_Owner = data.Party_Owner;
                    li.Phone_No = data.Phone_No;
                    li.Total_Amount = data.Total_Amount;
                    li.Total_Member = data.Total_Member;
                    li.amount_tobepay = data.Total_Amount;
                    li.ModeOfPayment = "Cash";
                    break;
                case 1:
                    var billdata = data != null ? context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == data.ID && x.IsAdvance_Payment == true) : null;
                    if (billdata != null)
                    {
                        li.BillNo = billdata.Bill_ID;
                        li.amount_tobepay = (double.Parse(billdata.Total_Amount) - double.Parse(billdata.Amount_Paid)).ToString();
                        li.IsAdvance_Payment = billdata.IsAdvance_Payment;
                        li.ID = Convert.ToInt32(billdata.Party_ID);

                        li.Price = billdata.Price_With_Gst;
                        li.Gst = billdata.Gst_Amount;
                        li.AdvancePay = billdata.Amount_Paid;
                        li.Party_Date = data.Party_Date;
                        li.Party_Name = data.Party_Name;
                        li.Party_Owner = data.Party_Owner;
                        li.Phone_No = data.Phone_No;
                        li.Total_Amount = billdata.Total_Amount;
                        li.Total_Member = billdata.Qty;
                        li.ModeOfPayment = billdata.Mode_Of_Payment;
                        if (billdata.Total_Amount == billdata.Amount_Paid)
                        {
                            ViewBag.Paid = "amtpaid";
                        }
                    }
                    else
                    {
                        var billdatatotal = data != null ? context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == data.ID && x.IsAdvance_Payment == false) : null;
                        if (billdatatotal != null)
                        {
                            li.BillNo = billdatatotal.Bill_ID;
                            li.amount_tobepay = billdatatotal.Amount_Paid;
                            li.IsAdvance_Payment = billdatatotal.IsAdvance_Payment;
                            li.ID = Convert.ToInt32(billdatatotal.Party_ID);

                            li.Price = billdatatotal.Price_With_Gst;
                            li.Gst = billdatatotal.Gst_Amount;
                            li.AdvancePay = (double.Parse(billdatatotal.Total_Amount) - double.Parse(billdatatotal.Amount_Paid)).ToString();
                            li.Party_Date = data.Party_Date;
                            li.Party_Name = data.Party_Name;
                            li.Party_Owner = data.Party_Owner;
                            li.Phone_No = data.Phone_No;
                            li.Total_Amount = billdatatotal.Total_Amount;
                            li.Total_Member = billdatatotal.Qty;
                            li.ModeOfPayment = billdatatotal.Mode_Of_Payment;
                            if (billdatatotal.Total_Amount == billdatatotal.Amount_Paid)
                            {
                                ViewBag.Paid = "amtpaid";
                            }
                        }

                    }
                    break;
                case 2:
                    var finalbilldata = data != null ? context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == data.ID && x.IsAdvance_Payment == false) : null;
                    if (finalbilldata != null)
                    {
                        li.BillNo = finalbilldata.Bill_ID;
                        li.amount_tobepay = finalbilldata.Amount_Paid;

                        li.ID = Convert.ToInt32(finalbilldata.Party_ID);
                        li.IsAdvance_Payment = finalbilldata.IsAdvance_Payment;
                        li.Price = finalbilldata.Price_With_Gst;
                        li.Gst = finalbilldata.Gst_Amount;
                        li.AdvancePay = (double.Parse(finalbilldata.Total_Amount) - double.Parse(finalbilldata.Amount_Paid)).ToString();
                        li.Party_Date = data.Party_Date;
                        li.Party_Name = data.Party_Name;
                        li.Party_Owner = data.Party_Owner;
                        li.Phone_No = data.Phone_No;
                        li.Total_Amount = finalbilldata.Total_Amount;
                        li.Total_Member = finalbilldata.Qty;
                        li.ModeOfPayment = finalbilldata.Mode_Of_Payment;
                        ViewBag.Paid = "amtpaid";
                    }
                    break;
                default:
                    break;
            }


            return PartialView("~/Views/BuffetMenu/_Partybillsdata.cshtml", li);




        }
        [HttpPost]
        public JsonResult SavePartyBill(PartyBillModel billModel)
        {
            if (double.Parse(billModel.Amount_Paid) > double.Parse(billModel.Total_Amount))
                return Json("Enter Correct Amount");
            
           
                tbl_PartyBilling model = new tbl_PartyBilling();
                model.Amount_Paid = billModel.Amount_Paid;
                model.Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);
                model.Date_Of_Billing = DateTime.Now.ToShortDateString();
                model.Gst_Amount = billModel.Gst_Amount;
                model.IsAdvance_Payment = billModel.IsAdvance_Payment;
                model.Mode_Of_Payment = billModel.Mode_Of_Payment;
                model.Party_ID = billModel.Party_ID;
                model.Price_With_Gst = billModel.Price_With_Gst;
                model.Price_Without_Gst = billModel.Price_Without_Gst;
                model.Qty = billModel.Qty;
                model.Total_Amount = billModel.Total_Amount;
                context.tbl_PartyBilling.Add(model);
                context.SaveChanges();
                if (model.IsAdvance_Payment == false)
                {
                    Save_InventoryUsage(model);
                }
               
            
                var rersponse = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == model.Bill_ID);

                var jsonResult = new
                {
                    partyBill = rersponse,
                    Party_Owner = context.tbl_Party.SingleOrDefault(x => x.ID == model.Party_ID).Party_Owner,
                    Phone_No = context.tbl_Party.SingleOrDefault(x => x.ID == model.Party_ID).Phone_No,
                    Party_Name = context.tbl_Party.SingleOrDefault(x => x.ID == model.Party_ID).Party_Name,
                    Party_Date = context.tbl_Party.SingleOrDefault(x => x.ID == model.Party_ID).Party_Date

                };
                return Json(jsonResult);
            
        }
         [Authorize(Roles = "1,2")]
        public ActionResult AdvanceBillingByID(int id)
        {
            
            partyModel li = new partyModel();
       
            
            var data = context.tbl_Party.SingleOrDefault(x => x.ID == id);
            var billdata = context.tbl_PartyBilling.SingleOrDefault(x => x.Party_ID == id && x.IsAdvance_Payment == true);
            if (billdata != null)
            {
                li.BillNo = billdata.Bill_ID;
                li.amount_tobepay = (double.Parse(billdata.Total_Amount) - double.Parse(billdata.Amount_Paid)).ToString();
                li.IsAdvance_Payment = billdata.IsAdvance_Payment;
                li.ID =Convert.ToInt32(billdata.Party_ID);

                li.Price = billdata.Price_With_Gst;
                li.Gst = billdata.Gst_Amount;
                li.AdvancePay = billdata.Amount_Paid;
                li.Party_Date = data.Party_Date;
                li.Party_Name = data.Party_Name;
                li.Party_Owner = data.Party_Owner;
                li.Phone_No = data.Phone_No;
                li.Total_Amount = billdata.Total_Amount;
                li.Total_Member = billdata.Qty;
            }
            else if (data != null)
            {
               
                li.ID = data.ID;
            
                li.Price = data.Price;
                li.Gst = data.Gst;
                
                li.Party_Date = data.Party_Date;
                li.Party_Name = data.Party_Name;
                li.Party_Owner = data.Party_Owner;
                li.Phone_No = data.Phone_No;
                li.Total_Amount = data.Total_Amount;
                li.Total_Member = data.Total_Member;
                li.amount_tobepay = data.Total_Amount;
            }

         
        
            return View(li);


        }
         [Authorize(Roles = "1,2")]
        public ActionResult PartyBillsIndex()
        {
            List<partyModel> li = new List<partyModel>();
            var data = context.tbl_PartyBilling.ToList();
            if (data != null)
            {
                foreach (var d in data)
                {
                    li.Add(new partyModel() {
              BillNo=d.Bill_ID,
              Total_Amount=d.Total_Amount,
              IsAdvance_Payment=d.IsAdvance_Payment,
                 ModeOfPayment=d.Mode_Of_Payment,
                 Party_Name=context.tbl_Party.SingleOrDefault(x=>x.ID==d.Party_ID).Party_Name,
              Party_Owner = context.tbl_Party.SingleOrDefault(x => x.ID == d.Party_ID).Party_Owner,
              Phone_No = context.tbl_Party.SingleOrDefault(x => x.ID == d.Party_ID).Phone_No,
              DateofBilling= d.Date_Of_Billing != null && d.Date_Of_Billing != string.Empty ? Convert.ToDateTime(d.Date_Of_Billing).ToString("dd/MM/yyyy") : d.Date_Of_Billing,
             Total_Member=d.Qty,
             AmountPaid=d.Amount_Paid

                    });
                }
            }

            return View(li);
        }
         [Authorize(Roles = "1,2")]
        public ActionResult BillDetailsByBillNo(int id)
        {
            partyModel li = new partyModel();
            var partyid=context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == id).Party_ID;


            var data = context.tbl_Party.SingleOrDefault(x => x.ID == partyid);
            var billdata = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == id);
            if (billdata != null)
            {

                li.BillNo = billdata.Bill_ID;
                li.IsAdvance_Payment = billdata.IsAdvance_Payment;
                li.ID = Convert.ToInt32(billdata.Party_ID);

                li.Price = billdata.Price_With_Gst;
                li.Gst = billdata.Gst_Amount;
               
                if (billdata.IsAdvance_Payment == true)
                {
                    li.AdvancePay = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == id && x.IsAdvance_Payment == true).Amount_Paid;

                }
                else
                {
                    li.AdvancePay = (double.Parse(billdata.Total_Amount) - double.Parse(billdata.Amount_Paid)).ToString();
                    li.amount_tobepay = billdata.Amount_Paid; ;
                }
                li.Party_Date = data.Party_Date;
                li.Party_Name = data.Party_Name;
                li.Party_Owner = data.Party_Owner;
                li.Phone_No = data.Phone_No;
                li.Total_Amount = billdata.Total_Amount;
                li.Total_Member = billdata.Qty;
                li.ModeOfPayment = billdata.Mode_Of_Payment;
                li.DateofBilling = billdata.Date_Of_Billing;
               
            }
            return View(li);

        }
        [HttpPost]
        public JsonResult PrintPartyBill(int billl_id=0)
        {
            if (billl_id > 0)
            {
                var model = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == billl_id);
               
                int? partyid = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == billl_id).Party_ID;
                var jsonResult = new
                {
                    partyBill = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == billl_id),
                    Party_Owner = context.tbl_Party.SingleOrDefault(x => x.ID == partyid).Party_Owner,
                    Phone_No = context.tbl_Party.SingleOrDefault(x => x.ID == partyid).Phone_No,
                    Party_Name = context.tbl_Party.SingleOrDefault(x => x.ID == partyid).Party_Name,
                    Party_Date = context.tbl_Party.SingleOrDefault(x => x.ID == partyid).Party_Date

                };
                return Json(jsonResult);
            }
           return Json("");
        }

        void Save_InventoryUsage(tbl_PartyBilling model)
        {
             
            
            var date = DateTime.Today;
            string DateFormat = date.ToString(@"MM\/dd\/yyyy");
            var partyfoodmenus=context.tbl_Party_FoodMenu.Where(x=>x.Party_ID==model.Party_ID).ToList();
            if (partyfoodmenus != null)
            {
                foreach (var d in partyfoodmenus)
                {
                    var data = context.tbl_Buffet_Menu.SingleOrDefault(a => a.ID == d.Buffet_Menu_Id);    // && a.Price==model.Price);
                    if (data != null)
                    {
                        if (data.InventoryID > 0)
                        {

                            tbl_InventoryUsage usage = new tbl_InventoryUsage();
                            usage.InventoryID = data.InventoryID;
                            usage.Used_Qty = context.tbl_Inventory.SingleOrDefault(x => x.ID == data.InventoryID).Quantity;
                            usage.Description = "(Bill No. : " + model.Bill_ID + ") Sold to customer on " + DateFormat;
                            usage.Used_Date = DateFormat;
                            usage.BillNo = model.Bill_ID;
                            //usage.MenusBillingDetailsID = menusdetails.ID;
                            usage.GST_NonGST_Bill = "GST";
                            usage.IsBuffetFood = true;
                            context.tbl_InventoryUsage.Add(usage);
                            context.SaveChanges();
                        }
                    }
                }
            }

            var menus = context.tbl_Party_FoodMenu.Where(x => x.Party_ID == model.Party_ID).ToList();
            if (menus != null)
            {

                foreach (var f in menus)
                {
                    var consumeItem = context.tbl_Buffet_ConsumableItems.Where(s => s.BuffetItem_ID == f.Buffet_Menu_Id).ToList();
                    if (consumeItem.Count > 0)
                    {
                        foreach (var m in consumeItem)
                        {

                            tbl_InventoryUsage usage = new tbl_InventoryUsage();
                            usage.InventoryID = m.Inventory_ID;
                            usage.Used_Qty = Math.Round((double)(m.Quantity *double.Parse(context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == model.Bill_ID).Qty)), 2); //Math.Round((double)(f.Quantity * m.Quantity), 2);
                            usage.Description = "Bill No. : " + model.Bill_ID + " | " + context.tbl_Buffet_Menu.SingleOrDefault(x => x.ID == m.BuffetItem_ID).Buffet_Item_Name;
                            usage.Used_Date = DateFormat;
                            usage.BillNo = model.Bill_ID;
                            //usage.MenusBillingDetailsID = menusdetails.ID;
                            usage.GST_NonGST_Bill = "GST";
                            usage.IsBuffetFood = true;
                            context.tbl_InventoryUsage.Add(usage);
                            context.SaveChanges();
                        }
                    }
                }
            }
        }
        #endregion

    }
}


/*
public JsonResult SavePartyBill(PartyBillModel billModel)
        {
            if(double.Parse(billModel.Amount_Paid)>double.Parse(billModel.Total_Amount))
                return Json("Enter Correct Amount");


            var check = context.tbl_PartyBilling.Where(x => x.Party_ID == billModel.Party_ID).ToList();

            if (check != null && check.Count == 2 && billModel.Bill_ID > 0)
            {
                var jsonRes = new
                {
                    partyBill = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == billModel.Bill_ID),
                    Party_Owner = context.tbl_Party.SingleOrDefault(x => x.ID == billModel.Party_ID).Party_Owner,
                    Phone_No = context.tbl_Party.SingleOrDefault(x => x.ID == billModel.Party_ID).Phone_No,
                    Party_Name = context.tbl_Party.SingleOrDefault(x => x.ID == billModel.Party_ID).Party_Name,
                    Party_Date = context.tbl_Party.SingleOrDefault(x => x.ID == billModel.Party_ID).Party_Date

                };

                return Json(jsonRes);
            }
            else if (check != null && check.Count == 1 && billModel.Bill_ID > 0 && context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == billModel.Bill_ID).Amount_Paid == context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == billModel.Bill_ID).Total_Amount)
            {
                var jsonRes = new
                {
                    partyBill = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == billModel.Bill_ID),
                    Party_Owner = context.tbl_Party.SingleOrDefault(x => x.ID == billModel.Party_ID).Party_Owner,
                    Phone_No = context.tbl_Party.SingleOrDefault(x => x.ID == billModel.Party_ID).Phone_No,
                    Party_Name = context.tbl_Party.SingleOrDefault(x => x.ID == billModel.Party_ID).Party_Name,
                    Party_Date = context.tbl_Party.SingleOrDefault(x => x.ID == billModel.Party_ID).Party_Date

                };

                return Json(jsonRes);
            }
           
           
                tbl_PartyBilling model = new tbl_PartyBilling();
                model.Amount_Paid = billModel.Amount_Paid;
                model.Billed_By = Convert.ToInt32(Request.Cookies["UserID"].Value);
                model.Date_Of_Billing = DateTime.Now.ToShortDateString();
                model.Gst_Amount = billModel.Gst_Amount;
                model.IsAdvance_Payment = billModel.IsAdvance_Payment;
                model.Mode_Of_Payment = billModel.Mode_Of_Payment;
                model.Party_ID = billModel.Party_ID;
                model.Price_With_Gst = billModel.Price_With_Gst;
                model.Price_Without_Gst = billModel.Price_Without_Gst;
                model.Qty = billModel.Qty;
                model.Total_Amount = billModel.Total_Amount;
                context.tbl_PartyBilling.Add(model);
                context.SaveChanges();
                if (model.IsAdvance_Payment == false)
                {
                    Save_InventoryUsage(model);
                }
               
            
                var rersponse = context.tbl_PartyBilling.SingleOrDefault(x => x.Bill_ID == model.Bill_ID);

                var jsonResult = new
                {
                    partyBill = rersponse,
                    Party_Owner = context.tbl_Party.SingleOrDefault(x => x.ID == model.Party_ID).Party_Owner,
                    Phone_No = context.tbl_Party.SingleOrDefault(x => x.ID == model.Party_ID).Phone_No,
                    Party_Name = context.tbl_Party.SingleOrDefault(x => x.ID == model.Party_ID).Party_Name,
                    Party_Date = context.tbl_Party.SingleOrDefault(x => x.ID == model.Party_ID).Party_Date

                };
                return Json(jsonResult);
            
        }
*/