using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using Dapper;
using Dapper.Contrib.Extensions;
using TM.Helper;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class ReportController : BaseController
    {

        public ActionResult Index()
        {
            try
            {
                var SQLServer = new TM.Connection.SQLServer();
                FileManagerController.InsertDirectory(Common.Directories.HDData);
                ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(Common.Directories.HDData).OrderByDescending(d => d).ToList();
            }
            catch (Exception ex) { this.danger(ex.Message); }
            return View();
        }
        [HttpGet]
        public JsonResult GetReportCustom()
        {
            var SQLServer = new TM.Connection.SQLServer();
            var appkey = "report_doanh_thu";
            var index = 0;
            string msg = "Lấy báo cáo thành công!";
            try
            {
                string qry = $"SELECT * FROM GROUPS WHERE APPKEY='{appkey}' AND FLAG=1 ORDER BY ORDERS";
                var data = SQLServer.Connection.Query<Models.GROUPS>(qry);
                return Json(new { data = data, success = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GetReportDetailCustom(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            string msg = "Lấy báo cáo thành công!";
            try
            {
                string qry = $"SELECT * FROM ITEMS WHERE GROUPID={obj.data_id} AND FLAG=1 ORDER BY ORDERS";
                var item = SQLServer.Connection.Query<Models.ITEMS>(qry);
                var data = new ReportDetailCustom();
                foreach (var i in item)
                {
                    if (i.APPKEY == "header")
                    {
                        data.hd = i.DESCRIPTION;
                        data.hc = i.CONTENTS;
                    }
                    else if (i.APPKEY == "footer")
                    {
                        data.fd = i.DESCRIPTION;
                        data.fc = i.CONTENTS;
                    }
                    else if (i.APPKEY == "content")
                    {
                        if (i.QUANTITY == 0)
                        {
                            var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
                            data.cd = FoxPro.Connection.Query(i.DESCRIPTION);
                            data.cc = FixContent(i.CONTENTS);
                            FoxPro.Close();
                        }
                        else if (i.QUANTITY == 1)
                        {
                            data.cd = SQLServer.Connection.Query(i.DESCRIPTION);
                            data.cc = FixContent(i.CONTENTS);
                        }

                    }
                }
                return Json(new { data = data, datetime = obj.datetime, success = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpGet]
        public JsonResult GetQuanHuyen()
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            string msg = "Lấy dữ liệu thành công!";
            try
            {
                string qry = $"SELECT * FROM QUAN_HUYEN_BKN ORDER BY MA_QUANHUYEN";
                var data = SQLServer.Connection.Query<Models.QUAN_HUYEN_BKN>(qry);
                return Json(new { data = data, success = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        public string FixContent(string val)
        {
            if (string.IsNullOrEmpty(val))
                return null;
            return val
                .Replace("&lt;p&gt;", "")
                .Replace("&lt;/p&gt;", "")
                .Replace("<p>", "")
                .Replace("</p>", "");
        }
        Common.DefaultObj getDefaultObj(Common.DefaultObj obj)
        {
            //Kiểm tra tháng đầu vào
            if (obj.ckhMerginMonth)
            {
                obj.time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                obj.year_time = int.Parse(obj.time.Substring(0, 4));
                obj.month_time = int.Parse(obj.time.Substring(4, 2));
            }

            obj.month_time = int.Parse(obj.time.Substring(4, 2));
            obj.year_time = int.Parse(obj.time.Substring(0, 4));
            obj.day_in_month = DateTime.DaysInMonth(obj.year_time, obj.month_time);
            obj.datetime = new DateTime(obj.year_time, obj.month_time, 1);
            obj.month_year_time = (obj.month_time < 10 ? "0" + obj.month_time.ToString() : obj.month_time.ToString()) + "/" + obj.year_time;
            obj.block_time = obj.datetime.ToString("yyyy/MM") + "/16";
            obj.month_before = DateTime.Now.AddMonths(-2).ToString("yyyyMM");
            obj.time = obj.time;
            obj.ckhMerginMonth = obj.ckhMerginMonth;
            obj.file = "BKN_th";
            obj.DataSource = Server.MapPath("~/" + obj.DataSource) + obj.time + "\\";
            return obj;
        }
        public class ReportDetailCustom
        {
            public string hd { get; set; }
            public string hc { get; set; }
            public object cd { get; set; }
            public string cc { get; set; }
            public string fd { get; set; }
            public string fc { get; set; }
        }
    }
}