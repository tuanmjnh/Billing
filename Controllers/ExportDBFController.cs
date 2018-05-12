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
    public class ExportDBFController : BaseController
    {
        public ActionResult Index()
        {
            FileManagerController.InsertDirectory(Common.Directories.HDData);
            ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(Common.Directories.HDData).OrderByDescending(d => d).ToList();
            //var b = "LỚP TRUNG CẤP LÝ LUẬN CHÍNH TRỊ".UnicodeToTCVN3();
            //var c = "Lớp Trung Cấp Lý Luận Chính Trị".UnicodeToTCVN3();
            return View();
        }
        [HttpGet]
        public JsonResult GetTableList(string database)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME";
                var data = SQLServer.Connection.Query<Models.TABLES>(qry).ToList();
                return Json(new { data = data, success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GetDetailsTableExport(string tableName)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'{tableName}';";
                var data = SQLServer.Connection.Query<Models.COLUMNS>(qry).ToList();
                //
                qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{tableName}'";
                var listETOld = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                //
                var listET = new List<Models.EXPORT_TABLE>();
                foreach (var i in data)
                {
                    var check = listETOld.FirstOrDefault(m => m.COLUMN_NAME == i.COLUMN_NAME);
                    if (check == null)
                    {
                        var tmp = new Models.EXPORT_TABLE();
                        var MapDBF = MappingDBF(i.DATA_TYPE);
                        tmp.TABLE_TYPE = (int)Common.Objects.TABLE_TYPE.DBF;
                        tmp.TABLE_NAME = i.TABLE_NAME;
                        tmp.COLUMN_NAME = i.COLUMN_NAME;
                        tmp.COLUMN_TYPE = i.DATA_TYPE;
                        tmp.COLUMN_LENGTH = i.CHARACTER_MAXIMUM_LENGTH;
                        tmp.COLUMN_NAME_EXPORT = i.COLUMN_NAME.Length > 10 ? i.COLUMN_NAME.Substring(0, 10).Trim('-').Trim('_') : i.COLUMN_NAME;
                        tmp.COLUMN_TYPE_EXPORT = MapDBF[0];
                        tmp.COLUMN_LENGTH_EXPORT = MapDBF[1];
                        tmp.CONDITION = "";
                        tmp.ORDERS = i.ORDINAL_POSITION;
                        tmp.FLAG = 1;
                        listET.Add(tmp);
                    }
                }
                SQLServer.Connection.Insert(listET);
                listET = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                return Json(new { data = listET, success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateTableExport(long id, string col, string val)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = $"UPDATE EXPORT_TABLE SET {col}='{val}' WHERE ID={id}";
                SQLServer.Connection.Query(qry);
                return Json(new { success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ExportToDBF(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            FileManagerController.InsertDirectory(obj.DataSource);
            obj = getDefaultObj(obj);
            FileManagerController.InsertDirectory(obj.DataSource, false);
            //
            var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
            try
            {
                var tmp_qry_fox = "";
                var tmp_qry_sql = "";
                var condition = "";
                var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
                var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                //
                if (EXPORT_TABLE.Count <= 0)
                    return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
                qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
                FoxPro.Connection.Query(qry);
                //
                var ColumnExport = new List<string>();
                foreach (var i in EXPORT_TABLE)
                {
                    ColumnExport.Add(i.COLUMN_NAME_EXPORT);
                    tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
                    tmp_qry_sql += $"{i.COLUMN_NAME},";
                    if (!string.IsNullOrEmpty(i.CONDITION))
                        condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
                }
                //
                qry = $"SELECT {tmp_qry_sql.Trim(',')} FROM {obj.file} {(!string.IsNullOrEmpty(condition) ? "WHERE " + condition.Substring(0, condition.Length - 5) : "")}";
                var data = SQLServer.Connection.Query<dynamic>(qry).ToList();
                //FoxPro.Insert(data);
                //
                qry = $"INSERT INTO {obj.file}({tmp_qry_fox.Trim(',')}) VALUES(";
                foreach (var i in data)
                {
                    qry += "";
                }
                CRUDFoxPro.InsertList(FoxPro.Connection, obj.file, ColumnExport, data);
                FileManagerController.InsertFile(obj.DataSource + obj.file + ".DBF", false);
                return Json(new { success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); FoxPro.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateExport(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            //
            try
            {
                var qry = $"SELECT a.*,a.DATA_TYPE AS COLUMN_TYPE,a.CHARACTER_MAXIMUM_LENGTH AS COLUMN_LENGTH,a.ORDINAL_POSITION AS ORDERS FROM INFORMATION_SCHEMA.COLUMNS a ORDER BY a.TABLE_NAME";
                var data = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                foreach (var i in data)
                {
                    var MapDBF = MappingDBF(i.COLUMN_TYPE);
                    i.TABLE_TYPE = (int)Common.Objects.TABLE_TYPE.EXPORT_CUSTOM;
                    i.COLUMN_NAME_EXPORT = i.COLUMN_NAME.Length > 10 ? i.COLUMN_NAME.Substring(0, 10).Trim('-').Trim('_') : i.COLUMN_NAME;
                    i.COLUMN_TYPE_EXPORT = MapDBF[0];
                    i.COLUMN_LENGTH_EXPORT = MapDBF[1];
                    i.CONDITION = "";
                    i.FLAG = 1;
                }
                return Json(new { data = data, success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpPost, AllowAnonymous, ValidateJsonAntiForgeryToken]
        public JsonResult CreateExportSave(ExportCustom obj, List<Models.EXPORT_TABLE> DataList)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                if (string.IsNullOrEmpty(obj.ExportName))
                    return Json(new { danger = "Vui lòng nhập tên kết xuất!" }, JsonRequestBehavior.AllowGet);

                var checkUpdate = true;
                var tables = new List<string>();
                var cols = new List<string>();
                var qry = "";
                var qry_table = "FROM ";
                var qry_create = $"CREATE TABLE {obj.ExportTableName}(";//CreateTable(Common.Directories.HDData, ExportTableName, data);
                var qry_select = "SELECT ";
                var qry_insert = "INSERT INTO";
                var qry_Condition = !string.IsNullOrEmpty(obj.Condition) ? $"WHERE {obj.Condition}" : "";

                qry = $"SELECT * FROM EXPORT_CUSTOM WHERE LOWER(NAME)='{obj.ExportName.ToLower()}'";
                var EXPORT_CUSTOM = SQLServer.Connection.QueryFirstOrDefault<Models.EXPORT_CUSTOM>(qry);
                if (EXPORT_CUSTOM == null)
                {
                    EXPORT_CUSTOM = new Models.EXPORT_CUSTOM();
                    EXPORT_CUSTOM.ID = Guid.NewGuid();
                    EXPORT_CUSTOM.NAME = obj.ExportName;
                    EXPORT_CUSTOM.TABLE_NAME = obj.ExportTableName.ToLower();
                    EXPORT_CUSTOM.TABLE_LIST = ",";
                    checkUpdate = false;
                }
                //
                foreach (var i in DataList)
                {

                    if (!tables.Contains(i.TABLE_NAME))
                        tables.Add(i.TABLE_NAME);

                    //Create
                    if (!cols.Contains(i.COLUMN_NAME_EXPORT))
                    {
                        cols.Add(i.COLUMN_NAME_EXPORT);
                        qry_create += $"[{i.COLUMN_NAME_EXPORT}] {i.COLUMN_TYPE_EXPORT}({i.COLUMN_LENGTH_EXPORT}),";
                    }

                    //Select
                    qry_select += $"{i.TABLE_NAME}.{i.COLUMN_NAME},";
                    //
                    i.APP_KEY = EXPORT_CUSTOM.ID;
                    i.TABLE_TYPE = (int)Common.Objects.TABLE_TYPE.EXPORT_CUSTOM;
                    i.COLUMN_TYPE = i.COLUMN_TYPE.ToLower();
                    i.COLUMN_TYPE_EXPORT = i.COLUMN_TYPE_EXPORT.ToLower();
                    i.ORDERS = index;
                    i.FLAG = 1;
                    index++;
                }
                foreach (var i in tables)
                {
                    qry_table += $"{i},";
                    EXPORT_CUSTOM.TABLE_LIST += $"{i},";
                }
                //
                qry_create = $"{qry_create.Trim(',')})";
                qry_select = $"{qry_select.Trim(',')} {qry_table.Trim(',')} {qry_Condition}";
                //
                if (obj.chkUpdateQuery)
                {
                    EXPORT_CUSTOM.QUERY_CREATE = obj.ExportQueryCreate;
                    EXPORT_CUSTOM.QUERY_SELECT = obj.ExportQuerySelect;
                    //EXPORT_CUSTOM.QUERY_INSERT = ExportQueryInsert;
                    EXPORT_CUSTOM.QUERY_END = obj.ExportQueryEnd;
                }
                else
                {
                    EXPORT_CUSTOM.QUERY_CREATE = qry_create;
                    EXPORT_CUSTOM.QUERY_SELECT = qry_select;
                    EXPORT_CUSTOM.QUERY_INSERT = qry_insert;
                }
                EXPORT_CUSTOM.CONDITION = obj.Condition;
                EXPORT_CUSTOM.FLAG = 1;

                if (checkUpdate)
                {
                    SQLServer.Connection.Update(EXPORT_CUSTOM);
                    //Remove EXPORT_TABLE Old
                    qry = $"DELETE FROM EXPORT_TABLE WHERE APP_KEY='{EXPORT_CUSTOM.ID}'";
                    SQLServer.Connection.Query(qry);
                }
                else
                    SQLServer.Connection.Insert(EXPORT_CUSTOM);
                //Insert EXPORT_TABLE New
                SQLServer.Connection.Insert(DataList);
                return Json(new { data = DataList, success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult EditQueryExportCustom(Guid id, string ExportName, string ExportTableName, string ExportQueryCreate, string ExportQuerySelect, string Condition)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = $"UPDATE EXPORT_CUSTOM SET NAME='{ExportName}',TABLE_NAME='{ExportTableName}',QUERY_CREATE='{ExportQueryCreate}',QUERY_SELECT='{ExportQuerySelect}',CONDITION='{Condition}' WHERE ID='{id}'";
                SQLServer.Connection.Query(qry);
                return Json(new { success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpGet]
        public JsonResult GetExportCustom()
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = "SELECT * FROM EXPORT_CUSTOM WHERE FLAG=1 ORDER BY NAME";
                var data = SQLServer.Connection.Query<Models.EXPORT_CUSTOM>(qry);
                return Json(new { data = data, roles = Authentication.Auth.AuthUser.roles, success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult EditExportCustom(Guid id)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = $"SELECT * FROM EXPORT_CUSTOM WHERE ID='{id}'";
                var EXPORT_CUSTOM = SQLServer.Connection.QueryFirstOrDefault<Models.EXPORT_CUSTOM>(qry);
                if (EXPORT_CUSTOM == null)
                    return Json(new { success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);

                qry = $"SELECT * FROM EXPORT_TABLE WHERE APP_KEY='{EXPORT_CUSTOM.ID}' AND TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.EXPORT_CUSTOM}";
                var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();

                return Json(new { EXPORT_CUSTOM = EXPORT_CUSTOM, EXPORT_TABLE = EXPORT_TABLE, success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ExportExportCustom(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            FileManagerController.InsertDirectory(obj.DataSource);
            obj = getDefaultObj(obj);
            FileManagerController.InsertDirectory(obj.DataSource, false);
            var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
            try
            {
                var qry = $"SELECT * FROM EXPORT_CUSTOM WHERE ID='{obj.file}'";
                var EXPORT_CUSTOM = SQLServer.Connection.QueryFirstOrDefault<Models.EXPORT_CUSTOM>(qry);
                if (EXPORT_CUSTOM == null)
                    return Json(new { success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);

                //Remove Old File
                TM.IO.FileDirectory.Delete($"{obj.DataSource}{EXPORT_CUSTOM.TABLE_NAME}.dbf", false);
                //Create New File
                FoxPro.Connection.Query(EXPORT_CUSTOM.QUERY_CREATE);

                var data = SQLServer.Connection.Query(EXPORT_CUSTOM.QUERY_SELECT.Replace("$timebill", obj.month_year_time)).ToList();
                qry = $"SELECT * FROM EXPORT_TABLE WHERE APP_KEY='{EXPORT_CUSTOM.ID}' AND TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.EXPORT_CUSTOM}";
                var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                var ColumnExport = new Dictionary<string, string>();
                FoxPro.Connection.InsertList3(EXPORT_CUSTOM.TABLE_NAME, EXPORT_TABLE, data);


                //
                FoxPro.Connection.Query($"USE {EXPORT_CUSTOM.TABLE_NAME}");
                if (EXPORT_CUSTOM.QUERY_END != null)
                {
                    var QUERY_END = EXPORT_CUSTOM.QUERY_END.Trim(';').Split(';');
                    foreach (var i in QUERY_END)
                        FoxPro.Connection.Query(i.Trim().Replace("$table", EXPORT_CUSTOM.TABLE_NAME));
                }
                return Json(new { success = TM.Common.Language.msgUpdateSucsess, url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{EXPORT_CUSTOM.TABLE_NAME}.dbf", $"{EXPORT_CUSTOM.TABLE_NAME}{obj.datetime.ToString("yyyyMM")}.dbf") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); FoxPro.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult RemoveExportCustom(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            try
            {
                var qry = $@"DELETE EXPORT_TABLE WHERE APP_KEY='{obj.file}';
                             DELETE EXPORT_CUSTOM WHERE ID='{obj.file}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = "Xóa kết xuất thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Connection.Close(); }
        }
        private string[] MappingDBF(string type)
        {
            switch (type)
            {
                case "bigint":
                    return new[] { "n", "15" };
                case "nvarchar":
                    return new[] { "c", "50" };
                case "varchar":
                    return new[] { "c", "50" };
                case "char":
                    return new[] { "c", "15" };
                case "text":
                    return new[] { "c", "200" };
                case "ntext":
                    return new[] { "c", "200" };
                case "int":
                    return new[] { "n", "12" };
                case "decimal":
                    return new[] { "n", "12,2" };
                case "smalldatetime":
                    return new[] { "d", "8" };
                case "datetime":
                    return new[] { "d", "8" };
                case "uniqueidentifier":
                    return new[] { "c", "36" };
                default:
                    return new[] { "c", "50" };
            }
        }
        private string MappingCondition(string type, string column, string value)
        {
            switch (type)
            {
                case "bigint":
                    return $"{column}={value}";
                case "nvarchar":
                    return $"{column}='{value}'";
                case "varchar":
                    return $"{column}='{value}'";
                case "char":
                    return $"{column}='{value}'";
                case "text":
                    return $"{column}='{value}'";
                case "ntext":
                    return $"{column}='{value}'";
                case "int":
                    return $"{column}={value}";
                case "decimal":
                    return $"{column}={value}";
                case "smalldatetime":
                    return $"{column}='{value}'"; //return $"FORMAT({column},'MM/yyyy')='{value}'";
                case "datetime":
                    return $"{column}='{value}'";
                case "uniqueidentifier":
                    return $"{column}='{value}'";
                default:
                    return $"{column}='{value}'";
            }
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
            obj.month_year_time = obj.datetime.ToString("MM/yyyy");
            obj.year_month_time = obj.datetime.ToString("yyyy/MM");
            obj.month_before = DateTime.Now.AddMonths(-2).ToString("yyyyMM");
            obj.time = obj.time;
            obj.ckhMerginMonth = obj.ckhMerginMonth;
            //obj.file = $"BKN_th";
            obj.DataSource = Server.MapPath("~/" + obj.DataSource) + obj.time + "\\";
            return obj;
        }
        private string CreateTable(string DataSource, string tableName, List<Models.EXPORT_TABLE> tableCol)
        {
            try
            {
                TM.IO.FileDirectory.Delete(DataSource + tableName, false);
                string sql = $"CREATE TABLE {tableName}(";
                foreach (var col in tableCol)
                    sql += $"[{col.COLUMN_NAME_EXPORT}] {col.COLUMN_TYPE_EXPORT}({col.COLUMN_LENGTH_EXPORT}),";
                sql = $"{sql.Trim(',')})";
                return sql;
                //TM.OleDBF.Execute(DataSource, sql, tableName);
            }
            catch (Exception) { throw; }
        }
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDNETFULL(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = obj.file = Common.Objects.TYPE_HD.HD_NET.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdnet_full{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        var rp = new GetData();
        //        var data = rp.GetNetFULL(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDNETIN(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = obj.file = Common.Objects.TYPE_HD.HD_NET.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdnet{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        var rp = new GetData();
        //        var data = rp.GetNetIN(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDNETTTT(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = obj.file = Common.Objects.TYPE_HD.HD_NET.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdnet_ttt{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        //qry = $"SELECT b.*,a.CUOC_DATA,a.TONG,a.VAT,a.TONGCONG from HD_NET_MERGIN a JOIN HD_NET b ON a.HD_NET_ID=b.ID WHERE a.TYPE_BILL=9005 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.FLAG=1 ORDER BY a.TONG DESC";
        //        var rp = new GetData();
        //        var data = rp.GetNetTTT(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDNETDC(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = obj.file = Common.Objects.TYPE_HD.HD_NET.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdnet_dc{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        //qry = $"SELECT b.*,a.CUOC_DATA,a.TONG,a.VAT,a.TONGCONG from HD_NET_MERGIN a JOIN HD_NET b ON a.HD_NET_ID=b.ID WHERE a.TYPE_BILL=9005 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.FLAG=1 ORDER BY a.TONG DESC";
        //        var rp = new GetData();
        //        var data = rp.GetNetDC(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDNETGD(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = obj.file = Common.Objects.TYPE_HD.HD_NET.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdnet_gd{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        //qry = $"SELECT b.*,a.CUOC_DATA,a.TONG,a.VAT,a.TONGCONG from HD_NET_MERGIN a JOIN HD_NET b ON a.HD_NET_ID=b.ID WHERE a.TYPE_BILL=9005 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.FLAG=1 ORDER BY a.TONG DESC";
        //        var rp = new GetData();
        //        var data = rp.GetNetGD(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDMYTVFULL(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = Common.Objects.TYPE_HD.HD_MYTV.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdtv_full{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        //qry = $"SELECT b.*,a.CUOC_DATA,a.TONG,a.VAT,a.TONGCONG from HD_NET_MERGIN a JOIN HD_NET b ON a.HD_NET_ID=b.ID WHERE a.TYPE_BILL=9005 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.FLAG=1 ORDER BY a.TONG DESC";
        //        var rp = new GetData();
        //        var data = rp.GetMyTVFULL(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDMYTVIN(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = Common.Objects.TYPE_HD.HD_MYTV.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdtv{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        //qry = $"SELECT b.*,a.CUOC_DATA,a.TONG,a.VAT,a.TONGCONG from HD_NET_MERGIN a JOIN HD_NET b ON a.HD_NET_ID=b.ID WHERE a.TYPE_BILL=9005 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.FLAG=1 ORDER BY a.TONG DESC";
        //        var rp = new GetData();
        //        var data = rp.GetMyTVIN(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDMYTVTTT(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = Common.Objects.TYPE_HD.HD_MYTV.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdtv_ttt{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        //qry = $"SELECT b.*,a.CUOC_DATA,a.TONG,a.VAT,a.TONGCONG from HD_NET_MERGIN a JOIN HD_NET b ON a.HD_NET_ID=b.ID WHERE a.TYPE_BILL=9005 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.FLAG=1 ORDER BY a.TONG DESC";
        //        var rp = new GetData();
        //        var data = rp.GetMyTVTTT(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult ExportDBFHDMYTVDC(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDData;
        //    FileManagerController.InsertDirectory(obj.DataSource);
        //    obj = getDefaultObj(obj);
        //    FileManagerController.InsertDirectory(obj.DataSource, false);
        //    //
        //    var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
        //    try
        //    {
        //        var tmp_qry_fox = "";
        //        var tmp_qry_sql = "";
        //        var condition = "";
        //        obj.file = Common.Objects.TYPE_HD.HD_MYTV.ToString();
        //        var qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{obj.file}' AND FLAG=1 ORDER BY ORDERS";
        //        obj.file = $"hdtv_dc{obj.datetime.ToString("yyyyMM")}";
        //        var EXPORT_TABLE = SQLServer.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
        //        //
        //        if (EXPORT_TABLE.Count <= 0)
        //            return Json(new { warning = "Vui lòng cấu hình dữ liệu Export trước!" }, JsonRequestBehavior.AllowGet);
        //        qry = CreateTable(obj.DataSource, obj.file, EXPORT_TABLE);
        //        FoxPro.Connection.Query(qry);
        //        //
        //        var ColumnExport = new Dictionary<string, string>();
        //        foreach (var i in EXPORT_TABLE)
        //        {
        //            ColumnExport.Add(i.COLUMN_NAME, i.COLUMN_NAME_EXPORT);
        //            tmp_qry_fox += $"{i.COLUMN_NAME_EXPORT},";
        //            tmp_qry_sql += $"{i.COLUMN_NAME},";
        //            if (!string.IsNullOrEmpty(i.CONDITION))
        //                condition += MappingCondition(i.COLUMN_TYPE, i.COLUMN_NAME, i.CONDITION) + " AND ";
        //        }

        //        //qry = $"SELECT b.*,a.CUOC_DATA,a.TONG,a.VAT,a.TONGCONG from HD_NET_MERGIN a JOIN HD_NET b ON a.HD_NET_ID=b.ID WHERE a.TYPE_BILL=9005 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.FLAG=1 ORDER BY a.TONG DESC";
        //        var rp = new GetData();
        //        var data = rp.GetMyTVDC(obj.month_year_time);
        //        FoxPro.Connection.InsertList2(obj.file, ColumnExport, data);

        //        FileManagerController.InsertFile($"{obj.DataSource}{obj.file}.dbf", false);
        //        return Json(new { success = "Xử lý thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{obj.file}.dbf", $"{obj.file}.dbf") }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally { SQLServer.Connection.Close(); FoxPro.Close(); }
        //}
    }
    public class ExportCustom
    {
        public string ExportName { get; set; }
        public string ExportTableName { get; set; }
        public string ExportQueryCreate { get; set; }
        public string ExportQuerySelect { get; set; }
        public string ExportQueryEnd { get; set; }
        public string Condition { get; set; }
        public bool chkUpdateQuery { get; set; }
    }
    public static class CRUDFoxPro
    {
        public static string InsertQry<T>(string TableExport, List<string> ColumnExport, T entity)
        {
            var strKey = $"INSERT INTO {TableExport}(";
            var strValue = "";
            int index = 0;
            foreach (var i in (dynamic)entity)
            {
                //var type = i.Value.GetType();//Int32//DateTime//String
                if (!Object.ReferenceEquals(null, i.Value))
                {
                    strKey += $"{ColumnExport[index]},";//$"{i.Key},";
                    if (i.Value.GetType().Name == "DateTime")
                    {
                        var val = ((DateTime)i.Value);
                        strValue += $"DATE({val.Year},{val.Month},{val.Day}),";
                    }
                    else if (i.Value.GetType().Name == "Guid")
                        strValue += $"'{((Guid)i.Value).ToString()}',";
                    else if (i.Value.GetType().Name != "String")
                        strValue += $"{i.Value},";
                    else
                        strValue += $"'{((string)i.Value).UnicodeToTCVN3()}',";
                }
                index++;
            }
            strKey = $"{ strKey.Trim(',')}) VALUES({strValue.Trim(',')})";
            return strKey;
        }
        public static dynamic Insert<T>(this System.Data.IDbConnection connection, string TableExport, List<string> ColumnExport, T entity, System.Data.IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            try
            {
                var qry = InsertQry(TableExport, ColumnExport, entity);
                connection.Query(qry);
                return 1;
            }
            catch (Exception) { throw; }
        }
        public static dynamic InsertList<T>(this System.Data.IDbConnection connection, string TableExport, List<string> ColumnExport, List<T> entity, int? jump = 500, System.Data.IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            try
            {
                var index = 0;
                foreach (var i in entity)
                {
                    index++;
                    if (index == 713)
                    {
                        var a = 1;
                    }
                    connection.Insert(TableExport, ColumnExport, i);
                }

            }
            catch (Exception) { throw; }
            return 1;
        }
        public static string InsertQry2<T>(string TableExport, Dictionary<string, string> ColumnExport, T entity)
        {
            var strKey = $"INSERT INTO {TableExport}(";
            var strValue = "";
            int index = 0;

            foreach (var i in ColumnExport)
            {
                var x = i.Key;
                var y = i.Value;
                System.Reflection.PropertyInfo pi = entity.GetType().GetProperty(i.Key);

                //String name = (String)(pi.GetValue(entity, null));
                var val = pi.GetValue(entity, null);//((DateTime)i.Value);
                if (val != null)
                {
                    strKey += $"{i.Value},";//$"{i.Key},";
                    var typeofname = TypeNameLower(val);
                    if (typeofname == "datetime")
                    {
                        var vals = (DateTime)val;//((DateTime)i.Value);
                        strValue += $"DATE({vals.Year},{vals.Month},{vals.Day}),";
                    }
                    else if (typeofname == "guid")
                        strValue += $"'{((Guid)val).ToString()}',";
                    else if (typeofname != "string")
                        strValue += $"{val},";
                    else
                        strValue += $"'{(val == null ? "''" : val.ToString().Trim().UnicodeToTCVN3())}',";
                    index++;
                }
            }
            //foreach (var i in entity.GetType().GetProperties())
            //{
            //    if (i.GetValue(entity, null) != null)
            //    {
            //        strKey += $"{i.Name},";//$"{i.Key},";
            //        var val = i.GetValue(entity, null);//((DateTime)i.Value);
            //        var typeofname = TypeNameLower(val);
            //        if (typeofname == "datetime")
            //        {
            //            var vals = (DateTime)val;//((DateTime)i.Value);
            //            strValue += $"DATE({vals.Year},{vals.Month},{vals.Day}),";
            //        }
            //        else if (typeofname == "guid")
            //            strValue += $"'{((Guid)val).ToString()}',";
            //        else if (typeofname != "string")
            //            strValue += $"{val},";
            //        else
            //            strValue += $"'{(val == null ? "''" : val.ToString().Trim().UnicodeToTCVN3())}',";

            //        //if (i.PropertyType.Name.ToLower() == "datetime")
            //        //{
            //        //    var val = (DateTime)i.GetValue(entity, null);//((DateTime)i.Value);
            //        //    strValue += $"DATE({val.Year},{val.Month},{val.Day}),";
            //        //}
            //        //else if (i.PropertyType.Name.ToLower() == "guid")
            //        //    strValue += $"'{((Guid)i.GetValue(entity, null)).ToString()}',";
            //        //else if (i.PropertyType.Name.ToLower() != "string")
            //        //    strValue += $"{i.GetValue(entity, null)},";
            //        //else
            //        //    strValue += $"'{(i.GetValue(entity, null) == null ? "''" : i.GetValue(entity, null).ToString().Trim().UnicodeToTCVN3())}',";
            //        index++;
            //    }
            //}
            strKey = $"{ strKey.Trim(',')}) VALUES({strValue.Trim(',')})";
            return strKey;
        }
        public static dynamic Insert2<T>(this System.Data.IDbConnection connection, string TableExport, Dictionary<string, string> ColumnExport, T entity, System.Data.IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            try
            {
                var qry = InsertQry2(TableExport, ColumnExport, entity);
                connection.Query(qry);
                return 1;
            }
            catch (Exception) { throw; }
        }
        public static dynamic InsertList2<T>(this System.Data.IDbConnection connection, string TableExport, Dictionary<string, string> ColumnExport, List<T> entity, int? jump = 500, System.Data.IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            try
            {
                var index = 0;
                foreach (var i in entity)
                {
                    index++;
                    if (index == 713)
                    {
                        var a = 1;
                    }
                    connection.Insert2(TableExport, ColumnExport, i);
                }

            }
            catch (Exception) { throw; }
            return 1;
        }
        public static string InsertQry3<T>(string TableExport, List<Models.EXPORT_TABLE> ColumnExport, T entity)
        {
            var strKey = $"INSERT INTO {TableExport}(";
            var strValue = "";
            int index = 0;
            var data = new Dictionary<string, object>();
            foreach (var item in (dynamic)entity)
                data.Add(item.Key, item.Value);

            foreach (var i in ColumnExport)
            {
                var val = data[i.COLUMN_NAME];
                if (val != null)
                {
                    strKey += $"{i.COLUMN_NAME_EXPORT},";//$"{i.Key},";
                    if (i.COLUMN_TYPE == "datetime")
                    {
                        var vals = (DateTime)val;//((DateTime)i.Value);
                        strValue += $"DATE({vals.Year},{vals.Month},{vals.Day}),";
                    }
                    else if (i.COLUMN_TYPE == "uniqueidentifier")
                        strValue += $"'{((Guid)val).ToString()}',";
                    else if (i.COLUMN_TYPE != "nvarchar" && i.COLUMN_TYPE != "ntext" && i.COLUMN_TYPE != "nchar" && i.COLUMN_TYPE != "varchar")
                        strValue += $"{val},";
                    else
                        strValue += $"'{(val == null ? "''" : val.ToString().Trim().UnicodeToTCVN3())}',";
                    index++;
                }
            }
            strKey = $"{ strKey.Trim(',')}) VALUES({strValue.Trim(',')})";
            return strKey;
        }
        public static dynamic Insert3<T>(this System.Data.IDbConnection connection, string TableExport, List<Models.EXPORT_TABLE> ColumnExport, T entity, System.Data.IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            try
            {
                var qry = InsertQry3(TableExport, ColumnExport, entity);
                connection.Query(qry);
                return 1;
            }
            catch (Exception) { throw; }
        }
        public static dynamic InsertList3<T>(this System.Data.IDbConnection connection, string TableExport, List<Models.EXPORT_TABLE> ColumnExport, List<T> entity, int? jump = 500, System.Data.IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            try
            {
                var index = 0;
                foreach (var i in entity)
                {
                    index++;
                    if (index == 713)
                    {
                        var a = 1;
                    }
                    connection.Insert3(TableExport, ColumnExport, i);
                }

            }
            catch (Exception) { throw; }
            return 1;
        }
        public static string TypeNameLower<T>(T obj)
        {
            return typeof(T).Name.ToLower();
        }
        public static string TypeNameLower(object obj)
        {
            if (obj != null) { return obj.GetType().Name.ToLower(); }
            else { return null; }
        }

    }
}