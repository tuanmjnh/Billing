using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Data;
using Billing.vn.mytv.portal;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class VASCController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyNhapTextData(Common.DefaultObj obj, int rdoTextAddType, string txtDataVal)
        {
            try
            {
                SubscriberManagement clswebservice = new SubscriberManagement();
                AuthHeader MyAuthHeader = new AuthHeader();
                MyAuthHeader.strUserName = "dongbk";
                MyAuthHeader.strPassword = "123456";
                clswebservice.AuthHeaderValue = MyAuthHeader;
                ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy(); // Qua SSL
                //GetSubscriberInfoVO clsGetSubriberVO = new GetSubscriberInfoVO(); // Thực hiện hàm truy vấn thông tin khách hàng
                //clsGetSubriberVO = clswebservice.GetSubscriberInfo("bcnmytv315869");
                //
                var index = 0;
                var msg = "";
                if (string.IsNullOrEmpty(txtDataVal))
                    return Json(new { danger = "Vui lòng nhập giá trị!" }, JsonRequestBehavior.AllowGet);
                List<dynamic> data = new List<dynamic>();
                var rss = "";
                var qry = "";
                var dataRow = txtDataVal.Split('\n');
                // Tích hợp
                if (rdoTextAddType == 1)
                {
                    //Remove old
                    if (obj.data_id == 2) { }
                    //
                    var dataList = new List<InputData>();
                    index = 0;
                    foreach (var i in dataRow)
                    {
                        index++;
                        if (index == 1) continue;
                        var tmp = i.Trim('\r').Split('\t');
                        if (tmp.Length > 1)
                        {
                            var vl = new ChangeMegaVNNRequest();
                            vl.IPTVAccount = tmp[0].Trim();
                            vl.MegaMyTV = tmp[1].Trim();
                            vl.Combo_FiberCD = null;
                            clswebservice.ChangeMegaVNNV3(vl);
                        }
                    }
                    //
                    msg += $"{index - 1} thuê bao";
                }
                // Lấy thông tin chính phụ
                if (rdoTextAddType == 2)
                {
                    var dataList = new List<InputData>();
                    index = 0;
                    foreach (var i in dataRow)
                    {
                        index++;
                        if (index == 1) continue;
                        var tmp = i.Trim('\r').Split('\t');
                        if (tmp.Length > 0 && tmp[0].Length > 0)
                        {
                            DataSet dataSet = clswebservice.GetUserParent(tmp[0].ToString().Trim()).Data;
                            if (dataSet != null && dataSet.Tables.Count > 0)
                            {
                                foreach (DataRow row in dataSet.Tables[0].Rows)
                                {
                                    foreach (DataColumn col in dataSet.Tables[0].Columns)
                                    {
                                        if (col.ColumnName == "CUST_USER_PARENT")
                                        {
                                            //dynamic x = new System.Dynamic.ExpandoObject();
                                            //x.account = tmp[0].ToString().Trim();
                                            //x.parent = row[col.ColumnName];
                                            //data.Add(x);
                                            rss += tmp[0].ToString().Trim() + "\t" + row[col.ColumnName] + "\n";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //
                return Json(new { data = data, rs = rss, success = $"Cập nhật thành công - {msg}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { }
        }
        public class TrustAllCertificatePolicy : ICertificatePolicy
        {
            public TrustAllCertificatePolicy() { }
            public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem) { return true; }
        }
        //Common
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

            obj.month_before = DateTime.Now.AddMonths(-2).ToString("yyyyMM");
            obj.time = obj.time;
            obj.ckhMerginMonth = obj.ckhMerginMonth;
            //obj.file = $"BKN_th";
            obj.DataSource = Server.MapPath("~/" + obj.DataSource) + obj.time + "\\";
            return obj;
        }
        public partial class InputData
        {
            public string account { get; set; }
            public int pack { get; set; }
        }
    }
}