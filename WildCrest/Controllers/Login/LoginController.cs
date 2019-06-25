using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WildCrest;

namespace WildCrest.Controllers.Login
{
    public class LoginController : Controller
    {
        ClubWildCrestEntities context = new ClubWildCrestEntities();
        // GET: Login
        public ActionResult Index()
        {
            if (Request.Cookies["RememberUserName"] != null && Request.Cookies["RememberPassword"] != null)
            {
                var UserName = Convert.ToString(Request.Cookies["RememberUserName"].Value);
                FormsAuthentication.SetAuthCookie(UserName, false);
                if (Request.Cookies["UserType"].Value == "1")
                {
                    //return RedirectToAction("Index", "Admin");
                    return RedirectToAction("Index", "Restaurant");
                }
                else if (Request.Cookies["UserType"].Value == "2")
                {
                    int usrid = Convert.ToInt32(Request.Cookies["UserID"].Value);
                    var setting = context.tbl_PermissionsToStaff.SingleOrDefault(ss => ss.UserID == usrid);
                    if (setting != null)
                    {
                        HttpCookie httpCookie = new HttpCookie("PageSetting");
                        httpCookie["MemberPermission"] = setting.MemberPermission.ToString();
                        httpCookie["MembershipPlanPermission"] = setting.MembershipPlanPermission.ToString();
                        httpCookie["EventsPermission"] = setting.EventsPermission.ToString();
                        httpCookie["VendorsPermission"] = setting.VendorsPermission.ToString();
                        httpCookie["MenuItemsPermission"] = setting.MenuItemsPermission.ToString();
                        httpCookie["FoodBillingEditPermission"] = setting.FoodBillingEditPermission.ToString();
                        Response.Cookies.Add(httpCookie);
                    }
                    return RedirectToAction("Index", "Restaurant");
                    //return RedirectToAction("EditProfilebyID", "AdminProfile", new { id = Request.Cookies["UserID"].Value });
                }
                else
                {
                    return RedirectToAction("EditProfilebyID", "UserProfile", new { id = Request.Cookies["UserID"].Value });
                }
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public JsonResult LoginCheck(string UserName, string Password, string RememberMe)
        {
            int? userType = 0;
            int userID = 0;
            var data = context.tbl_Profile.SingleOrDefault(a => a.Email == UserName && a.Password == Password);
            if (data != null)
            {
                if (data.NewUsrBySuperApprv == false)
                {
                    userType = -1;
                }
                else
                {
                    userType = data.UserType;
                    userID = data.ID;
                    FormsAuthentication.SetAuthCookie(UserName, false);

                    if (userType == 2)
                    {
                        var setting = context.tbl_PermissionsToStaff.SingleOrDefault(ss => ss.UserID == data.ID);
                        if (setting != null)
                        {
                            HttpCookie httpCookie = new HttpCookie("PageSetting");
                            httpCookie["MemberPermission"] = setting.MemberPermission.ToString();
                            httpCookie["MembershipPlanPermission"] = setting.MembershipPlanPermission.ToString();
                            httpCookie["EventsPermission"] = setting.EventsPermission.ToString();
                            httpCookie["VendorsPermission"] = setting.VendorsPermission.ToString();
                            httpCookie["MenuItemsPermission"] = setting.MenuItemsPermission.ToString();
                            httpCookie["FoodBillingEditPermission"] = setting.FoodBillingEditPermission.ToString();
                            Response.Cookies.Add(httpCookie);
                        }
                    }

                    Response.Cookies["UserID"].Value = data.ID.ToString();

                    Response.Cookies["UserName"].Value = UserName.Trim();
                    //Response.Cookies["Password"].Value = Password.Trim();
                    Response.Cookies["UserType"].Value = data.UserType.ToString();
                    Response.Cookies["AddedBy"].Value = data.F_Name;
                    Response.Cookies["RememberUserName"].Value = UserName.Trim();
                    Response.Cookies["RememberPassword"].Value = Password.Trim();
                    if (RememberMe == "true")
                    {
                        Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(30);
                        //Response.Cookies["Password"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["UserID"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["UserType"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["AddedBy"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["RememberUserName"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["RememberPassword"].Expires = DateTime.Now.AddDays(30);

                        if (Request.Cookies["PageSetting"] != null)
                        {
                            HttpCookie PageSetting = Request.Cookies["PageSetting"];
                            PageSetting.Expires = DateTime.Now.AddDays(30);
                            Response.Cookies.Add(PageSetting);
                        }
                    }
                    else
                    {
                        Response.Cookies["RememberUserName"].Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies["RememberPassword"].Expires = DateTime.Now.AddDays(-1);
                    }
                }
            }
            var arr = new
            {
                userType = userType,
                userid = userID
            };
            return Json(arr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogOut()
        {
            Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["Password"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["UserID"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["UserType"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["AddedBy"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["RememberUserName"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["RememberPassword"].Expires = DateTime.Now.AddDays(-1);

            if (Request.Cookies["PageSetting"] != null)
            {
                HttpCookie PageSetting = Request.Cookies["PageSetting"];
                PageSetting.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(PageSetting);
            }

            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
    }
}