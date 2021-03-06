﻿using System;
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
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            try
            {
                // var qry = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME";
                var qry = $"SELECT DISTINCT OWNER,OBJECT_NAME TABLE_NAME FROM DBA_OBJECTS WHERE OBJECT_TYPE='TABLE' AND OWNER='{OWNER}';";
                var data = Oracle.Connection.Query<Models.TABLES>(qry).ToList();
                return Json(new { data = data, success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GetDetailsTableExport(string tableName)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            try
            {
                // var qry = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'{tableName}';";
                var qry = $"SELECT * FROM all_tab_cols WHERE table_name='{tableName}' AND owner='{OWNER}'";
                var data = Oracle.Connection.Query<Models.COLUMNS>(qry).ToList();
                //
                qry = $"SELECT * FROM EXPORT_TABLE WHERE TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.DBF} AND TABLE_NAME='{tableName}'";
                var listETOld = Oracle.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                //
                var listET = new List<Models.EXPORT_TABLE>();
                foreach (var i in data)
                {
                    var check = listETOld.FirstOrDefault(m => m.COLUMN_NAME == i.COLUMN_NAME);
                    if (check == null)
                    {
                        var tmp = new Models.EXPORT_TABLE();
                        var MapDBF = MappingOracleDBF(i.DATA_TYPE);
                        tmp.TABLE_TYPE = (int)Common.Objects.TABLE_TYPE.DBF;
                        tmp.TABLE_NAME = i.TABLE_NAME;
                        tmp.COLUMN_NAME = i.COLUMN_NAME;
                        tmp.COLUMN_TYPE = i.DATA_TYPE;
                        tmp.COLUMN_LENGTH = i.DATA_LENGTH;
                        tmp.COLUMN_NAME_EXPORT = i.COLUMN_NAME.Length > 10 ? i.COLUMN_NAME.Substring(0, 10).Trim('-').Trim('_') : i.COLUMN_NAME;
                        tmp.COLUMN_TYPE_EXPORT = MapDBF[0];
                        tmp.COLUMN_LENGTH_EXPORT = MapDBF[1];
                        tmp.CONDITION = "";
                        tmp.ORDERS = i.ORDINAL_POSITION;
                        tmp.FLAG = 1;
                        listET.Add(tmp);
                    }
                }
                Oracle.Connection.Insert(listET);
                listET = Oracle.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                return Json(new { data = listET, success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateTableExport(long id, string col, string val)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            try
            {
                var qry = $"UPDATE EXPORT_TABLE SET {col}='{val}' WHERE ID={id}";
                Oracle.Connection.Query(qry);
                return Json(new { success = "Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ExportToDBF(Common.DefaultObj obj)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
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
                var EXPORT_TABLE = Oracle.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
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
                var data = Oracle.Connection.Query<dynamic>(qry).ToList();
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
            finally { Oracle.Connection.Close(); FoxPro.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateExport(Common.DefaultObj obj)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            //
            try
            {
                var qry = $"SELECT a.*,a.data_type COLUMN_TYPE,data_length COLUMN_LENGTH,a.column_id ORDERS FROM all_tab_cols a WHERE a.owner='{OWNER}' order by a.table_name";
                var data = Oracle.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                foreach (var i in data)
                {
                    var MapDBF = MappingOracleDBF(i.COLUMN_TYPE);
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
            finally { Oracle.Connection.Close(); }
        }
        [HttpPost, AllowAnonymous, ValidateJsonAntiForgeryToken]
        public JsonResult CreateExportSave(ExportCustom obj, List<Models.EXPORT_TABLE> DataList)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            try
            {
                if (string.IsNullOrEmpty(obj.ExportName) && string.IsNullOrEmpty(obj.ExportTableName))
                    return Json(new { danger = "Vui lòng nhập tên và tên bảng kết xuất!" }, JsonRequestBehavior.AllowGet);

                var checkUpdate = true;
                var tables = new List<string>();
                var cols = new List<string>();
                var qry = "";
                var qry_table = "FROM ";
                var qry_create = $"CREATE TABLE {obj.ExportTableName}(";//CreateTable(Common.Directories.HDData, ExportTableName, data);
                var qry_select = "SELECT ";
                var qry_insert = "INSERT INTO";
                var qry_Condition = !string.IsNullOrEmpty(obj.Condition) ? $"WHERE {obj.Condition}" : "";

                qry = $"SELECT * FROM EXPORT_CUSTOM WHERE LOWER(NAME)=N'{obj.ExportName.ToLower()}' AND TYPE_NAME='{obj.ExportType}'";
                var EXPORT_CUSTOM = Oracle.Connection.QueryFirstOrDefault<Models.EXPORT_CUSTOM>(qry);
                if (EXPORT_CUSTOM == null)
                {
                    EXPORT_CUSTOM = new Models.EXPORT_CUSTOM();
                    EXPORT_CUSTOM.ID = Guid.NewGuid().ToString("N");
                    EXPORT_CUSTOM.NAME = obj.ExportName;
                    EXPORT_CUSTOM.TABLE_NAME = obj.ExportTableName.ToLower();
                    checkUpdate = false;
                }
                EXPORT_CUSTOM.TABLE_LIST = ",";
                //
                if (obj.ExportType == Common.Objects.EXPORT_CUSTOM.DEFAULT.ToString())
                {
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
                }
                else if (obj.ExportType == Common.Objects.EXPORT_CUSTOM.MODULES.ToString())
                {
                    EXPORT_CUSTOM.TABLE_LIST = !string.IsNullOrEmpty(obj.ExportTableList) ? ("," + obj.ExportTableList.Trim().Trim(',') + ",") : "";
                    EXPORT_CUSTOM.QUERY_CREATE = obj.ExportQueryCreate;
                    EXPORT_CUSTOM.QUERY_SELECT = ""; // qry_select;
                    EXPORT_CUSTOM.QUERY_INSERT = ""; // qry_insert;
                    EXPORT_CUSTOM.QUERY_END = obj.ExportQueryEnd;
                }
                else
                    return Json(new { danger = "Sai định dạng kết suất, Vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet);

                EXPORT_CUSTOM.TYPE_NAME = obj.ExportType.ToString();
                EXPORT_CUSTOM.CONDITION = obj.Condition;
                EXPORT_CUSTOM.FLAG = 1;

                if (checkUpdate)
                {
                    Oracle.Connection.Update(EXPORT_CUSTOM);
                    //Remove EXPORT_TABLE Old
                    qry = $"DELETE FROM EXPORT_TABLE WHERE APP_KEY='{EXPORT_CUSTOM.ID}'";
                    Oracle.Connection.Query(qry);
                }
                else
                    Oracle.Connection.Insert(EXPORT_CUSTOM);
                //var x = new Models.TEST();
                //x.ID = Guid.NewGuid().ToString("N");
                //x.NAME = "tuanmjnh";
                //x.LEVELS = 2;
                //x.MONEY = 3.5M;
                //x.TIME = DateTime.Now;
                //Oracle.Connection.Insert(x);

                //Insert EXPORT_TABLE New
                if (DataList != null && DataList.Count > 0) Oracle.Connection.Insert(DataList);
                return Json(new { data = DataList, success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult EditQueryExportCustom(string id, string ExportName, string ExportTableName, string ExportQueryCreate, string ExportQuerySelect, string Condition)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            try
            {
                var qry = $"UPDATE EXPORT_CUSTOM SET NAME='{ExportName}',TABLE_NAME='{ExportTableName}',QUERY_CREATE='{ExportQueryCreate}',QUERY_SELECT='{ExportQuerySelect}',CONDITION='{Condition}' WHERE ID='{id}'";
                Oracle.Connection.Query(qry);
                return Json(new { success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        [HttpGet]
        public JsonResult GetExportCustom(int flag = 1)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            try
            {
                var qry = $"SELECT * FROM EXPORT_CUSTOM WHERE FLAG={flag} ORDER BY TYPE_NAME DESC,NAME";
                var data = Oracle.Connection.Query<Models.EXPORT_CUSTOM>(qry);
                return Json(new { data = data, roles = Authentication.Auth.AuthUser.roles, success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult EditExportCustom(string id)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            try
            {
                var qry = $"SELECT * FROM EXPORT_CUSTOM WHERE ID='{id}'";
                var EXPORT_CUSTOM = Oracle.Connection.QueryFirstOrDefault<Models.EXPORT_CUSTOM>(qry);
                if (EXPORT_CUSTOM == null)
                    return Json(new { success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);

                qry = $"SELECT * FROM EXPORT_TABLE WHERE APP_KEY='{EXPORT_CUSTOM.ID}' AND TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.EXPORT_CUSTOM}";
                var EXPORT_TABLE = Oracle.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();

                return Json(new { EXPORT_CUSTOM = EXPORT_CUSTOM, EXPORT_TABLE = EXPORT_TABLE, success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ExportExportCustom(Common.DefaultObj obj)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            FileManagerController.InsertDirectory(obj.DataSource, false);
            var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
            try
            {
                var qry = $"SELECT * FROM EXPORT_CUSTOM WHERE ID='{obj.file}'";
                var EXPORT_CUSTOM = Oracle.Connection.QueryFirstOrDefault<Models.EXPORT_CUSTOM>(qry);
                if (EXPORT_CUSTOM == null) return Json(new { success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
                if (obj.data_value == Common.Objects.EXPORT_CUSTOM.DEFAULT.ToString())
                {
                    var rs = _ExportExportCustom(Oracle, FoxPro, obj, EXPORT_CUSTOM);
                    if (!rs) return Json(new { danger = "Lỗi kết xuất, vui lòng thử lại" }, JsonRequestBehavior.AllowGet);
                }
                else if (obj.data_value == Common.Objects.EXPORT_CUSTOM.MODULES.ToString())
                {
                    var tableList = EXPORT_CUSTOM.TABLE_LIST.Trim(',').Split(',');
                    if (tableList.Length < 1) return Json(new { warning = "Chưa có kết xuất!" }, JsonRequestBehavior.AllowGet);
                    foreach (var item in tableList)
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        qry = $"SELECT * FROM EXPORT_CUSTOM WHERE NAME='{item}' AND TYPE_NAME='{Common.Objects.EXPORT_CUSTOM.DEFAULT}'";
                        var _export = Oracle.Connection.QueryFirstOrDefault<Models.EXPORT_CUSTOM>(qry);
                        var rs = _ExportExportCustom(Oracle, FoxPro, obj, _export);

                        //
                        if (!string.IsNullOrEmpty(EXPORT_CUSTOM.QUERY_CREATE))
                            _ExportQueryExportCustom(FoxPro, EXPORT_CUSTOM.TABLE_NAME, EXPORT_CUSTOM.QUERY_CREATE);
                        if (!string.IsNullOrEmpty(EXPORT_CUSTOM.QUERY_END))
                            _ExportQueryExportCustom(FoxPro, EXPORT_CUSTOM.TABLE_NAME, EXPORT_CUSTOM.QUERY_END);

                        if (!rs) return Json(new { danger = "Lỗi kết suất, vui lòng thử lại" }, JsonRequestBehavior.AllowGet);
                    }
                }
                // FileManagerController.ReExtensionToLower($"{Common.Directories.HDData}{obj.time}");
                var fileExport = $"{Common.Directories.HDData}{obj.time}\\{EXPORT_CUSTOM.TABLE_NAME}.dbf";
                var fileName = $"{EXPORT_CUSTOM.TABLE_NAME}{obj.datetime.ToString("yyyyMM")}.dbf";
                if (new System.IO.FileInfo(Server.MapPath($"~/{fileExport}")).Exists)
                    return Json(new { success = "Kết xuất thành công!", url = UrlDownloadFiles(fileExport, fileName) }, JsonRequestBehavior.AllowGet);
                return Json(new { success = "Kết xuất thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); FoxPro.Connection.Close(); }
        }
        public bool _ExportExportCustom(TM.Connection.Oracle Oracle, TM.Connection.OleDBF FoxPro, Common.DefaultObj obj, Models.EXPORT_CUSTOM EXPORT_CUSTOM)
        {
            try
            {
                //Remove Old File
                TM.IO.FileDirectory.Delete($"{obj.DataSource}{EXPORT_CUSTOM.TABLE_NAME}.dbf", false);
                //Create New File
                FoxPro.Connection.Query(EXPORT_CUSTOM.QUERY_CREATE);

                var data = Oracle.Connection.Query(EXPORT_CUSTOM.QUERY_SELECT.Replace("$kyhoadon", obj.KYHD.ToString())).ToList();
                var qry = $"SELECT * FROM EXPORT_TABLE WHERE APP_KEY='{EXPORT_CUSTOM.ID}' AND TABLE_TYPE={(int)Common.Objects.TABLE_TYPE.EXPORT_CUSTOM}";
                var EXPORT_TABLE = Oracle.Connection.Query<Models.EXPORT_TABLE>(qry).ToList();
                var ColumnExport = new Dictionary<string, string>();
                FoxPro.Connection.InsertList3(EXPORT_CUSTOM.TABLE_NAME, EXPORT_TABLE, data);

                //Execute Query
                FoxPro.Connection.Query($"USE {EXPORT_CUSTOM.TABLE_NAME}");
                _ExportQueryExportCustom(FoxPro, EXPORT_CUSTOM.TABLE_NAME, EXPORT_CUSTOM.QUERY_END);
                //FoxPro.Connection.Query($"USE {EXPORT_CUSTOM.TABLE_NAME}");
                //if (EXPORT_CUSTOM.QUERY_END != null)
                //{
                //    var QUERY_END = EXPORT_CUSTOM.QUERY_END.Trim(';').Split(';');
                //    foreach (var i in QUERY_END)
                //        FoxPro.Connection.Query(i.Trim().TrimStart('\n').Replace("$table", EXPORT_CUSTOM.TABLE_NAME));
                //}
                //Delete .BAK
                FileManagerController.RemoveFileSource(obj.DataSource, false);
                return true;
            }
            catch (Exception) { throw; }
        }
        public bool _ExportQueryExportCustom(TM.Connection.OleDBF FoxPro, string table_name, string query)
        {
            try
            {
                if (!string.IsNullOrEmpty(query))
                {
                    var QUERY_END = query.Trim(';').Split(';');
                    foreach (var i in QUERY_END)
                        FoxPro.Connection.Query(i.Trim().TrimStart('\n').Replace("$table", table_name));
                }
                return true;
            }
            catch (Exception) { throw; }
            finally { FoxPro.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult DeleteExportCustom(Common.DefaultObj obj)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            try
            {
                var data = Oracle.Connection.Get<Models.EXPORT_CUSTOM>(obj.file);
                if (data != null)
                {
                    var qry = $"UPDATE EXPORT_CUSTOM SET FLAG={(data.FLAG == 0 ? 1 : 0)} WHERE ID='{obj.file}'";
                    Oracle.Connection.Query(qry);
                }
                return Json(new { success = "Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult RemoveExportCustom(Common.DefaultObj obj)
        {
            var OWNER = "TTKD_BKN";
            var Oracle = new TM.Connection.Oracle(OWNER);
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            try
            {
                var qry = $@"DELETE EXPORT_TABLE WHERE APP_KEY='{obj.file}';
                             DELETE EXPORT_CUSTOM WHERE ID='{obj.file}';";
                Oracle.Connection.Query(qry);
                return Json(new { success = "Xóa kết xuất thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { Oracle.Connection.Close(); }
        }
        // SQLServer
        private string[] MappingDBF(string type)
        {
            type = type.ToLower();
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
        // Oracle
        private string[] MappingOracleDBF(string type)
        {
            type = type.ToUpper();
            switch (type)
            {
                case "NUMBER":
                    return new[] { "n", "15" };
                case "VARCHAR2":
                    return new[] { "c", "50" };
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
            obj.KYHD = int.Parse(obj.datetime.ToString("yyyyMM") + "01");
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
    }
    public class ExportCustom
    {
        public string ExportType { get; set; }
        public string ExportName { get; set; }
        public string ExportTableName { get; set; }
        public string ExportTableList { get; set; }
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
                    if (i.COLUMN_TYPE == "date")
                    {
                        var vals = (DateTime)val;//((DateTime)i.Value);
                        strValue += $"DATE({vals.Year},{vals.Month},{vals.Day}),";
                    }
                    else if (i.COLUMN_TYPE == "uniqueidentifier")
                        strValue += $"'{((Guid)val).ToString()}',";

                    //else if (i.COLUMN_TYPE != "nvarchar" && i.COLUMN_TYPE != "ntext" && i.COLUMN_TYPE != "nchar" && i.COLUMN_TYPE != "varchar")
                    //    strValue += $"{val},";
                    else if (i.COLUMN_TYPE != "varchar2" && i.COLUMN_TYPE != "ntext" && i.COLUMN_TYPE != "nchar" && i.COLUMN_TYPE != "varchar")
                        strValue += $"{val},";
                    else
                        strValue += $"'{(val == null ? "''" : val.ToString().Trim().Replace("\'", "").Trim('\'').UnicodeToTCVN3())}',";
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
                    if (index == 1468)
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