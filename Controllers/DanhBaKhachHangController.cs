using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Dapper.Contrib.Extensions;
using TM.Helper;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class DanhBaKhachHangController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DanhBaKhachHangNull()
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE ISNULL=1 OR ISNULLMT=1 OR MA_DVI IS NULL OR MA_CBT IS NULL OR MA_TUYEN IS NULL";
                var data = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
                return Json(new { data = data, success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateDanhBaKhachHangNull(Guid id, string col, string val)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = $"UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET {col}=N'{val}' WHERE ID='{id}'";
                SQLServer.Connection.Query(qry);
                return Json(new { success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
    }
}