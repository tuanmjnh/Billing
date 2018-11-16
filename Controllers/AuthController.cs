using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Billing.Controllers
{
    public class AuthController : Controller
    {
        TM.Connection.SQLServer SQLServer;
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult CheckConnection(AuthObj obj)
        {
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                SQLServer = new TM.Connection.SQLServer("Portal");
            }
            catch (Exception)
            {
                return Json(new { danger = "Không thể kết nối đến CSDL!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = "Đăng nhập thành công!" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Login(AuthObj obj)
        {
            try
            {
                SQLServer = new TM.Connection.SQLServer("Portal");
                var qry = "";
                //var collection = HttpContext.Request.ReadFormAsync();
                //string username = collection["username"].ToString();
                //string password = collection["password"].ToString();

                //AuthStatic
                var AuthStatic = Authentication.Auth.isAuthStatic(obj.username, obj.password);
                if (AuthStatic != null)
                {
                    Authentication.Auth.SetAuth(AuthStatic);
                    return Json(new { success = "Đăng nhập thành công!", url = TM.Url.RedirectContinue() }, JsonRequestBehavior.AllowGet);
                }
                //AuthDB
                qry = $"SELECT * FROM users WHERE username='{obj.username}'";
                var user = SQLServer.Connection.QueryFirstOrDefault<Authentication.Users>(qry);// db.users.SingleOrDefault(u => u.username == username);

                //Account not Exist
                if (user == null)
                {
                    return Json(new { danger = "Sai tên tài khoản hoặc mật khẩu!" }, JsonRequestBehavior.AllowGet);
                }

                //Password wrong
                obj.password = TM.Encrypt.CryptoMD5TM(obj.password + user.salt);
                if (user.password != obj.password)
                {
                    return Json(new { danger = "Sai tên tài khoản hoặc mật khẩu!" }, JsonRequestBehavior.AllowGet);
                }

                //Account is locked
                if (user.flag < 1)
                {
                    return Json(new { danger = "Tài khoản đã bị khóa. Vui lòng liên hệ admin!" }, JsonRequestBehavior.AllowGet);
                }

                //Update last login
                user.last_login = DateTime.Now;
                SQLServer.Connection.Update(user);
                //Set Auth Account
                Authentication.Auth.SetAuth(user);
                //return Redirect(TM.Url.RedirectContinue());
            }
            catch (Exception ex)
            {
                return Json(new { danger = "Đăng nhập không thành công, vui lòng liên hệ admin!" }, JsonRequestBehavior.AllowGet);
            }
            finally { SQLServer.Close(); }
            return Json(new { success = "Đăng nhập thành công!", url = TM.Url.RedirectContinue(obj.currentUrl, true) }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Filters.Auth]
        public ActionResult logout()
        {
            try
            {
                Authentication.Auth.Logout();
            }
            catch (Exception)
            {
                return Json(new { danger = "Lỗi vui lòng thử lại sau!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = "Đăng xuất hệ thống thành công!", url = System.Web.HttpContext.Current.Request.UrlReferrer.ToString() }, JsonRequestBehavior.AllowGet);
        }
        [Filters.Auth]
        public ActionResult ChangePassword(Guid id)
        {
            //return View(db.users.SingleOrDefault(u => u.id == id));
            return View();
        }
        [HttpPost, Filters.Auth]
        public JsonResult ChangePassword(Guid id, string password)
        {
            try
            {
                SQLServer = new TM.Connection.SQLServer("Portal");
                var qry = $"SELECT * FROM users WHERE id='{id.ToString()}'";
                var user = SQLServer.Connection.QueryFirstOrDefault<Authentication.Users>(qry);
                user.password = TM.Encrypt.CryptoMD5TM(password + user.salt);
                SQLServer.Connection.Update(user);
            }
            catch (Exception)
            {
                return Json(new { danger = "Cập nhật mật khẩu không thành công, vui lòng thử lại sau!" }, JsonRequestBehavior.AllowGet);
            }
            finally { SQLServer.Close(); }
            return Json(new { success = "Cập nhật mật khẩu thành công!" }, JsonRequestBehavior.AllowGet);
        }
    }
    public class AuthObj
    {
        public string username { get; set; }
        public string password { get; set; }
        public bool remeberMe { get; set; }
        public string currentUrl { get; set; }
    }
}