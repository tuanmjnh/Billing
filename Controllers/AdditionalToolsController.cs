using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using TM.Helper;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class AdditionalToolsController : BaseController
    {
        // GET: AdditionalTools
        public ActionResult Index()
        {
            try
            {
                FileManagerController.InsertDirectory(Common.Directories.HDDataSource);
                ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(Common.Directories.HDDataSource).OrderByDescending(d => d).ToList();
            }
            catch (Exception ex) { this.danger(ex.Message); }
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult RemoveDuplicate(ObjAdditionalTools obj)
        {
            try
            {
                TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.orther);
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false, new[] { ".dbf" });
                if (file.UploadError().Count > 0)
                    return Json(new { danger = "Vui lòng chọn file DBF trước khi thực hiện!" });
                var fileName = file.UploadFileString();
                var fileNameFull = TM.Common.Directories.orther + fileName;
                TM.OleDBF.DataSource = fileNameFull;
                //
                var ExtraValueList = new List<string>();
                var ExtraValueStr = "";
                var ExtraValueStr2 = "";
                var ExtraValueStr3 = "";
                if (obj.ExtraValue != null)
                {
                    ExtraValueStr = obj.ExtraValue.Trim().Trim(',');
                    ExtraValueList.AddRange(ExtraValueStr.Split(',').Trim());
                    foreach (var i in ExtraValueList)
                    {
                        ExtraValueStr2 += $"o.{i}=oc.{i} AND ";
                        ExtraValueStr3 += $"o.{i},";
                    }
                    ExtraValueStr2 = ExtraValueStr2.Substring(0, ExtraValueStr2.Length - 5);
                    ExtraValueStr3 = ExtraValueStr3.Trim(',');
                }
                else
                    obj.IsExtraValue = false;
                //
                string sql = $"ALTER table {fileName} ADD COLUMN app_id n(10)";
                try
                {
                    TM.OleDBF.Execute(sql);
                }
                catch (Exception) { }
                sql = $"ALTER table {fileName} ADD COLUMN dupe_flag n(2)";
                try
                {
                    TM.OleDBF.Execute(sql);
                }
                catch (Exception) { }
                sql = $"UPDATE {fileName} SET app_id=RECNO()";
                TM.OleDBF.Execute(sql);
                sql = $"UPDATE {fileName} SET dupe_flag=0";
                TM.OleDBF.Execute(sql);

                if (obj.IsExtraValue)
                    sql = $"UPDATE {fileName} SET dupe_flag=1 WHERE app_id in(SELECT app_id FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE {ExtraValueStr2})";
                else
                    sql = $"UPDATE {fileName} SET dupe_flag=1 WHERE app_id in(SELECT app_id FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey})";
                TM.OleDBF.Execute(sql);

                if (obj.IsExtraValue)
                    sql = $"UPDATE {fileName} SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},{obj.ExtraValue},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey},{obj.ExtraValue} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE {ExtraValueStr2} GROUP BY {ExtraValueStr3})";
                //sql = $"UPDATE {fileName} SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE {ExtraValueStr2})";
                else
                    sql = $"UPDATE {fileName} SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} GROUP BY o.{obj.PrimeryKey})";
                TM.OleDBF.Execute(sql);

                sql = $"DELETE FROM {fileName} WHERE dupe_flag=1";
                TM.OleDBF.Execute(sql);
                sql = $"PACK {fileName}";
                TM.OleDBF.Execute(sql);
                FileManagerController.InsertDirectory(TM.Common.Directories.orther);
                FileManagerController.InsertFile(TM.OleDBF.DataSource);
                return Json(new { success = "Xử lý Thành công!", url = UrlDownloadFiles(TM.OleDBF.DataSource, $"{fileName.ToLower().Replace(".dbf", "")}_RemoveDuplicate.dbf") }, JsonRequestBehavior.AllowGet);
                // return DownloadFiles(TM.OleDBF.DataSource, fileName.ToLower().Replace(".dbf", "") + "_RemoveDuplicate.dbf");
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GetDuplicate(ObjAdditionalTools obj)
        {
            try
            {
                TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.orther);
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false, new[] { ".dbf" });
                if (file.UploadError().Count > 0)
                    return Json(new { danger = "Vui lòng chọn file DBF trước khi thực hiện!" });
                var fileName = file.UploadFileString();
                var fileNameFull = TM.Common.Directories.orther + fileName;
                TM.OleDBF.DataSource = fileNameFull;
                //
                var ExtraValueList = new List<string>();
                var ExtraValueStr = "";
                var ExtraValueStr2 = "";
                var ExtraValueStr3 = "";
                if (obj.ExtraValue != null)
                {
                    ExtraValueStr = obj.ExtraValue.Trim().Trim(',');
                    ExtraValueList.AddRange(ExtraValueStr.Split(',').Trim());
                    foreach (var i in ExtraValueList)
                    {
                        ExtraValueStr2 += $"o.{i}=oc.{i} AND ";
                        ExtraValueStr3 += $"o.{i},";
                    }
                    ExtraValueStr2 = ExtraValueStr2.Substring(0, ExtraValueStr2.Length - 5);
                    ExtraValueStr3 = ExtraValueStr3.Trim(',');
                }
                else
                    obj.IsExtraValue = false;
                //
                string sql = $"ALTER table {fileName} ADD COLUMN app_id n(10)";
                try
                {
                    TM.OleDBF.Execute(sql);
                }
                catch (Exception) { }
                sql = $"ALTER table {fileName} ADD COLUMN dupe_flag n(2)";
                try
                {
                    TM.OleDBF.Execute(sql);
                }
                catch (Exception) { }
                sql = $"UPDATE {fileName} SET app_id=RECNO()";
                TM.OleDBF.Execute(sql);
                sql = $"UPDATE {fileName} SET dupe_flag=0";
                TM.OleDBF.Execute(sql);

                if (obj.IsExtraValue)
                    sql = $"UPDATE {fileName} SET dupe_flag=1 WHERE app_id in(SELECT app_id FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE {ExtraValueStr2})";
                else
                    sql = $"UPDATE {fileName} SET dupe_flag=1 WHERE app_id in(SELECT app_id FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey})";
                TM.OleDBF.Execute(sql);

                if (obj.IsExtraValue)
                    sql = $"UPDATE {fileName} SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},{obj.ExtraValue},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey},{obj.ExtraValue} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE o.{obj.PrimeryKey}!='' AND {ExtraValueStr2} GROUP BY o.{obj.PrimeryKey},{ExtraValueStr3})";
                //sql = $"UPDATE {fileName} SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE {ExtraValueStr2})";
                else
                    sql = $"UPDATE {fileName} SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM {fileName} o INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} GROUP BY o.{obj.PrimeryKey})";
                TM.OleDBF.Execute(sql);

                sql = $"DELETE FROM {fileName} WHERE dupe_flag=0";
                TM.OleDBF.Execute(sql);
                sql = $"PACK {fileName}";
                TM.OleDBF.Execute(sql);
                FileManagerController.InsertDirectory(TM.Common.Directories.orther);
                FileManagerController.InsertFile(TM.OleDBF.DataSource);
                return Json(new { success = "Xử lý Thành công!", url = UrlDownloadFiles(TM.OleDBF.DataSource, $"{fileName.ToLower().Replace(".dbf", "")}_RemoveDuplicate.dbf") }, JsonRequestBehavior.AllowGet);
                // return DownloadFiles(TM.OleDBF.DataSource, fileName.ToLower().Replace(".dbf", "") + "_RemoveDuplicate.dbf");
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GeneralUpload(Common.DefaultObj obj)
        {
            var index = 0;

            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            string strUpload = "Tải tệp thành công!";
            try
            {
                TM.IO.FileDirectory.CreateDirectory(obj.DataSource, false);
                var uploadData = UploadBase(obj.DataSource, strUpload);
                if (uploadData == (int)Common.Objects.ResultCode._extension)
                    return Json(new { danger = "Tệp phải định dạng .dbf!" }, JsonRequestBehavior.AllowGet);
                else if (uploadData == (int)Common.Objects.ResultCode._length)
                    return Json(new { danger = "Chưa đủ tệp!" }, JsonRequestBehavior.AllowGet);
                //else if (uploadData == (int)Common.Objects.ResultCode._success)
                //    return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ImportGDSExcel(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var HNIVNPTBACKAN2 = new TM.Connection.Oracle("HNIVNPTBACKAN2");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var UploadFiles = new List<string>();
            string strUpload = "Nhập file GDS thành công!";
            try
            {
                TM.IO.FileDirectory.CreateDirectory(obj.DataSource, false);
                var uploadData = UploadBase(obj.DataSource, strUpload, UploadFiles, ".xls");
                if (UploadFiles.Count < 1)
                    return Json(new { danger = "Chưa tải được tệp, Vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
                //
                var xxxx = TM.OleExcel.ToDataSet(obj.DataSource + UploadFiles[0]);
                var excel = xxxx.Tables[0];
                var list = new List<Models.CUOC_FIBERVNN>();
                foreach (System.Data.DataRow r in excel.Rows)
                {
                    index++;
                    var tmp = new Models.CUOC_FIBERVNN();
                    tmp.MA_MEN = r["MA_MEN"].ToString();
                    tmp.CUOC = r["CUOC"] != null ? decimal.Parse(r["CUOC"].ToString()) : 0;
                    tmp.THANGNAM = obj.month_year_time;
                    list.Add(tmp);
                }
                //list.Add(new Models.GDSIMPORT { MA_MEN = r["MA_MEN"].ToString(), CUOC = (int)r["CUOC"], THANGNAM = ((DateTime)r["THANGNAM"]).ToString("MM/yyyy") });
                //HNIVNPTBACKAN2.Connection.Query("DELETE CUOC_FIBERVNN WHERE THANGNAM='11/2017'");
                TM.Dapper.CRUDOracle.InsertList(HNIVNPTBACKAN2.Connection, list);
                //
                if (uploadData == (int)Common.Objects.ResultCode._extension)
                    return Json(new { danger = "Tệp phải định dạng .dbf!" }, JsonRequestBehavior.AllowGet);
                else if (uploadData == (int)Common.Objects.ResultCode._length)
                    return Json(new { danger = "Chưa đủ tệp!" }, JsonRequestBehavior.AllowGet);
                //else if (uploadData == (int)Common.Objects.ResultCode._success)
                //    return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
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
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult GetDuplicate(ObjAdditionalTools obj)
        //{
        //    try
        //    {
        //        TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.orther);
        //        var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false, new[] { ".dbf" });
        //        if (file.UploadError().Count > 0)
        //            return Json(new { danger = "Vui lòng chọn file DBF trước khi thực hiện!" });
        //        var fileName = file.UploadFileString();
        //        var fileNameFull = TM.Common.Directories.orther + fileName;
        //        TM.OleDBF.DataSource = fileNameFull;
        //        //
        //        string sql = $"ALTER table {fileName} ADD COLUMN flag_madt n(1)";
        //        try { TM.OleDBF.Execute(sql); } catch { }
        //        //
        //        sql = $"UPDATE {fileName} SET flag_madt=0";
        //        TM.OleDBF.Execute(sql);
        //        //
        //        if (obj.IsExtraValue)
        //            sql = $"UPDATE {fileName} SET flag_madt=1 " +
        //                $"WHERE {obj.PrimeryKey} IN(SELECT o.{obj.PrimeryKey} FROM {fileName} o INNER JOIN (SELECT o.* FROM {fileName} o " +
        //                $"INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*) AS dupeCount FROM {fileName} GROUP BY {obj.PrimeryKey} HAVING COUNT(*) > 1) oc ON " +
        //                $"o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} ORDER BY o.{obj.PrimeryKey}) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE LEFT(o.{2},3)!=LEFT(oc.{2},3))",
        //                fileName, obj.PrimeryKey, obj.ExtraValue);
        //        else
        //            sql = string.Format("UPDATE {0} SET flag_madt=1 " +
        //                "WHERE {1} IN(SELECT o.{1} FROM {0} o INNER JOIN (SELECT o.* FROM {0} o " +
        //                "INNER JOIN (SELECT {1},COUNT(*) AS dupeCount FROM {0} GROUP BY {1} HAVING COUNT(*) > 1) oc ON " +
        //                "o.{1}=oc.{1} ORDER BY o.{1}) oc ON o.{1}=oc.{1} WHERE LEFT(o.{2},3)!=LEFT(oc.{2},3))",
        //                fileName, obj.PrimeryKey, obj.ExtraValue);
        //        TM.OleDBF.Execute(sql);
        //        //
        //        sql = $"DELETE FROM {fileName} WHERE flag_madt=0";
        //        TM.OleDBF.Execute(sql);
        //        sql = $"PACK {fileName}";
        //        TM.OleDBF.Execute(sql);
        //        FileManagerController.InsertDirectory(TM.Common.Directories.orther);
        //        FileManagerController.InsertFile(TM.OleDBF.DataSource);
        //        return Json(new { success = "Xử lý Thành công!", url = UrlDownloadFiles(TM.OleDBF.DataSource, $"{fileName.ToLower().Replace(".dbf", "")}_GetDuplicate.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        //}
    }
    public class ObjAdditionalTools
    {
        public string PrimeryKey { get; set; }
        public string ExtraValue { get; set; }
        public bool IsExtraValue { get; set; }
    }
}