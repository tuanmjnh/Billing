using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class ConnectionController : BaseController
    {
        public static Models.Config _Config = new Models.Config();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Insert()
        {
            return PartialView("PartialCreate");
        }
        public ActionResult Update()
        {
            return PartialView("PartialEdit");
        }
        [HttpGet]
        public JsonResult Select(objBST obj)//string sort, string order, string search, int offset = 0, int limit = 10, int flag = 1
        {
            var index = 0;
            var qry = "";
            var cdt = "";
            try
            {
                var json = new TM.Helper.TMHelperJson(Server.MapPath("~/Systems/config.json"));
                _Config = json.LoadJson<Models.Config>();
                if (_Config.connection.Count < 1)
                    return Json(new { total = 0, rows = new List<string>() }, JsonRequestBehavior.AllowGet);
                //Get total item
                var total = _Config.connection.Count;
                //Sort And Orders
                //Page Site
                var rs = _Config.connection.Skip(obj.offset).Take(obj.limit).ToList();
                var ReturnJson = Json(new { total = total, rows = rs }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception ex) { return Json(new { danger = "Không tìm thấy dữ liệu, vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { }
            //return Json(new { success = "Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Get(long id)
        {
            var index = 0;
            var qry = "";
            try
            {
                var data = _Config.connection.Find(d => d.id == id);
                var ReturnJson = Json(new { data = data, success = "Lấy dữ liệu thành công!" }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception) { return Json(new { danger = "Không tìm thấy dữ liệu, vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { }
        }
        public class objBST : Common.ObjBSTable
        {
            public string maDvi { get; set; }
            public string timeBill { get; set; }
            public int export { get; set; }
            public string startDate { get; set; }
            public string endDate { get; set; }
        }
    }
}