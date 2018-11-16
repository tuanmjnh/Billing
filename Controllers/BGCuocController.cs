using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TM.Helper;
using Dapper;
using Dapper.Contrib.Extensions;
using ExcelDataReader;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class BGCuocController : BaseController
    {
        TM.Connection.SQLServer SQLServer;
        //public ActionResult Index(int? flag, string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, string export)
        //{
        //    try
        //    {
        //        if (searchString != null)
        //        {
        //            page = 1;
        //            searchString = searchString.Trim();
        //        }
        //        else searchString = currentFilter;
        //        ViewBag.order = order;
        //        ViewBag.currentFilter = searchString;
        //        ViewBag.flag = flag;
        //        ViewBag.datetime = datetime;
        //        ViewBag.datetimeType = datetimeType;

        //        var rs = db.BANGGIACUOC.Where(m => m.FLAG > 0);

        //        if (!String.IsNullOrEmpty(searchString) && searchString.isNumber())
        //        {
        //            var id = int.Parse(searchString);
        //            rs = rs.Where(d => d.GOICUOCID == id || d.TOCDO == id || d.GIA == id);
        //        }
        //        else if (!String.IsNullOrEmpty(searchString))
        //            rs = rs.Where(d =>
        //            d.TENGOI.Contains(searchString) ||
        //            d.PROFILE.Contains(searchString) ||
        //            d.KIEU.Contains(searchString));

        //        if (!String.IsNullOrEmpty(datetime))
        //        {
        //            var date = datetime.Split('-');
        //            if (date.Length > 1)
        //            {
        //                var dateStart = TM.Format.Formating.StartOfDate(TM.Format.Formating.DateParseExactVNToEN(date[0]));
        //                var dateEnd = TM.Format.Formating.EndOfDate(TM.Format.Formating.DateParseExactVNToEN(date[1]));
        //                rs = datetimeType == 0 ? rs.Where(d => d.CREATEDAT >= dateStart && d.CREATEDAT <= dateEnd) : rs.Where(d => d.UPDATEDAT >= dateStart && d.UPDATEDAT <= dateEnd);
        //            }
        //        }

        //        if (flag == 0) rs = rs.Where(d => d.FLAG == 0);
        //        else rs = rs.Where(d => d.FLAG > 0);

        //        switch (order)
        //        {
        //            case "profile_asc":
        //                rs = rs.OrderBy(d => d.PROFILE);
        //                break;
        //            case "profile_desc":
        //                rs = rs.OrderByDescending(d => d.PROFILE);
        //                break;
        //            case "name_asc":
        //                rs = rs.OrderBy(d => d.TENGOI);
        //                break;
        //            case "name_desc":
        //                rs = rs.OrderByDescending(d => d.TENGOI);
        //                break;
        //            case "speed_asc":
        //                rs = rs.OrderBy(d => d.TOCDO);
        //                break;
        //            case "speed_desc":
        //                rs = rs.OrderByDescending(d => d.TOCDO);
        //                break;
        //            case "price_asc":
        //                rs = rs.OrderBy(d => d.GIA);
        //                break;
        //            case "price_desc":
        //                rs = rs.OrderByDescending(d => d.GIA);
        //                break;
        //            case "type_asc":
        //                rs = rs.OrderBy(d => d.KIEU);
        //                break;
        //            case "type_desc":
        //                rs = rs.OrderByDescending(d => d.KIEU);
        //                break;
        //            case "goicuocId_asc":
        //                rs = rs.OrderBy(d => d.GOICUOCID);
        //                break;
        //            case "goicuocId_desc":
        //                rs = rs.OrderByDescending(d => d.GOICUOCID);
        //                break;
        //            default:
        //                rs = rs.OrderBy(d => d.TENGOI).ThenBy(d => d.TOCDO);
        //                break;
        //        }
        //        //Export to any
        //        if (!String.IsNullOrEmpty(export))
        //        {
        //            TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Bảng giá cói cước");
        //            return RedirectToAction("Index");
        //        }

        //        ViewBag.TotalRecords = rs.Count();
        //        int pageSize = 15;
        //        int pageNumber = (page ?? 1);

        //        return View(rs.AsEnumerable().Select(d => d.ToExpando()).ToPagedList(pageNumber, pageSize));
        //    }
        //    catch (Exception ex)
        //    {
        //        this.danger(ex.Message);
        //    }
        //    return View();
        //}
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
        public ActionResult ImportTextData()
        {
            return PartialView("PartialImportTextData");
        }
        [HttpGet]
        public JsonResult Select(objBST obj)//string sort, string order, string search, int offset = 0, int limit = 10, int flag = 1
        {
            var index = 0;
            var qry = "";
            var cdt = "";
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                //
                qry = $"SELECT * FROM BGCUOC WHERE FLAG={obj.flag}";

                //Get data for Search
                if (!String.IsNullOrEmpty(obj.search) && obj.search.isNumber())
                    cdt += $"(GOICUOCID={obj.search}) AND ";
                else if (!string.IsNullOrEmpty(obj.search))
                    cdt += $"(TENGOI LIKE '%{obj.search}%' OR PROFILE LIKE '%{obj.search}%' OR PROFILEIP LIKE '%,{obj.search},%') AND ";

                if (!string.IsNullOrEmpty(cdt))
                    qry += $" AND {cdt.Substring(0, cdt.Length - 5)}";

                //export
                if (obj.export == 1)
                {
                    //var startDate = DateTime.ParseExact($"{obj.startDate}", "dd/MM/yyyy HH:mm", provider);
                    //var endDate = DateTime.ParseExact($"{obj.endDate}", "dd/MM/yyyy HH:mm", provider);
                    //qry += $" AND tb.FLAG=2 AND tb.UPDATEDAT>=CAST('{startDate.ToString("yyyy-MM-dd")}' as datetime) AND tb.UPDATEDAT<=CAST('{endDate.ToString("yyyy-MM-dd")}' as datetime) ORDER BY tb.MA_DVI,tb.UPDATEDAT";
                    //var export = SQLServer.Connection.Query<Portal.Areas.ND49.Models.ND49Export>(qry);
                    //qry = "SELECT * FROM users";
                    //var user = SQLServer.Connection.Query<Authentication.user>(qry);
                    //foreach (var i in export)
                    //{
                    //    var tmp = user.FirstOrDefault(d => d.username == i.NVQL);
                    //    i.TEN_NVQL = tmp != null ? tmp.full_name : null;
                    //}
                    //var rsJson = Json(new { data = export, SHA = Guid.NewGuid() }, JsonRequestBehavior.AllowGet);
                    //rsJson.MaxJsonLength = int.MaxValue;
                    //return rsJson;
                }
                //
                var data = SQLServer.Connection.Query<Models.BGCUOC>(qry);

                ////Get data for Search
                //if (!string.IsNullOrEmpty(obj.search))
                //    data = data.Where(d =>
                //    d.MA_KH.Contains(obj.search) ||
                //    d.MA_TT_HNI.Contains(obj.search) ||
                //    d.ACCOUNT.Contains(obj.search) ||
                //    d.MA_TB.Contains(obj.search) ||
                //    d.TEN_TT.Contains(obj.search) ||
                //    d.DIACHI_TT.Contains(obj.search));
                //
                if (data.ToList().Count < 1)
                    return Json(new { total = 0, rows = data }, JsonRequestBehavior.AllowGet);
                //Get total item
                var total = data.Count();
                //Sort And Orders
                if (!string.IsNullOrEmpty(obj.sort))
                {
                    if (obj.sort.ToUpper() == "TENGOI" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.TENGOI);
                    else if (obj.sort.ToUpper() == "TENGOI" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.TENGOI);
                    else if (obj.sort.ToUpper() == "PROFILE" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.PROFILE);
                    else if (obj.sort.ToUpper() == "PROFILE" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.PROFILE);
                    else
                        data = data.OrderBy(m => m.NGAY_BD).ThenBy(m => m.TENGOI);
                }
                else
                    data = data.OrderBy(m => m.NGAY_BD).ThenBy(m => m.TENGOI);
                //Page Site
                var rs = data.Skip(obj.offset).Take(obj.limit).ToList();
                var ReturnJson = Json(new { total = total, rows = rs }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception) { return Json(new { danger = "Không tìm thấy dữ liệu, vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
            //return Json(new { success = "Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Get(long id)
        {
            var index = 0;
            var qry = "";
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                qry = $"SELECT * FROM BGCUOC WHERE BGCUOCID={id}";
                var data = SQLServer.Connection.QueryFirstOrDefault<Models.BGCUOC>(qry);
                var ReturnJson = Json(new { data = data, success = "Lấy dữ liệu thành công!" }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception) { return Json(new { danger = "Không tìm thấy dữ liệu, vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult InsertUpdate(Models.BGCUOC obj, long? id)
        {
            //var provider = System.Globalization.CultureInfo.InvariantCulture;
            var index = 0;
            var qry = "";
            var msg = "Cập nhật thông tin thành công!";
            var profile_ip = "profile_ip";
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                //
                qry = $"SELECT i.*,g.TITLE AS GROUPTITLE FROM ITEMS i,GROUPS g WHERE i.GROUPID=g.GROUPID AND i.APPKEY='{profile_ip}' AND g.APPKEY='{profile_ip}' AND i.FLAG=1 AND g.FLAG=1 ORDER BY i.TITLE";
                var ProfileIPList = SQLServer.Connection.QueryFirstOrDefault<Models.GROUPS>(qry);
                //
                if (id == null)
                {
                    obj.CREATEDBY = Authentication.Auth.AuthUser.username;
                    obj.CREATEDAT = DateTime.Now;
                    SQLServer.Connection.Insert(obj);
                    msg = "Tạo mới thông tin thành công!";
                }
                else
                {
                    qry = $"SELECT * FROM BGCUOC WHERE BGCUOCID={id}";
                    var data = SQLServer.Connection.QueryFirstOrDefault<Models.BGCUOC>(qry);
                    data.TENGOI = obj.TENGOI;
                    data.PROFILE = obj.PROFILE;
                    data.PROFILEIP = getProfileIP(ProfileIPList.TITLE, data.PROFILE);
                    data.TOCDO = obj.TOCDO;
                    data.GIA = obj.GIA;
                    data.DICHVUVT_ID = obj.DICHVUVT_ID;
                    data.GOICUOCID = obj.GOICUOCID;
                    data.TICHHOPID = obj.TICHHOPID;
                    data.IS_DATA = obj.IS_DATA;
                    data.IS_TH = obj.IS_TH;
                    data.GHICHU = obj.GHICHU;
                    data.NGAY_BD = obj.NGAY_BD;
                    data.NGAY_KT = obj.NGAY_KT;
                    data.EXTRA_TYPE = obj.EXTRA_TYPE;
                    data.UPDATEDBY = Authentication.Auth.AuthUser.username;
                    data.UPDATEDAT = DateTime.Now;
                    data.FLAG = obj.FLAG;
                    SQLServer.Connection.Update(data);
                }
                var ReturnJson = Json(new { success = msg }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception ex) { return Json(new { danger = "Lỗi hệ thống vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Delete(string id)
        {
            var index = 0;
            var qry = "";
            var msg = "Xóa bản ghi thành công!";
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                var _id = id.Trim(',');

                qry = $"SELECT * FROM BGCUOC WHERE BGCUOCID IN({_id})";
                var data = SQLServer.Connection.QueryFirstOrDefault<Models.BGCUOC>(qry);

                if (data == null) return Json(new { danger = "Không tìm thấy dữ liệu!" });
                if (data.FLAG == 0)
                {
                    qry = $"UPDATE BGCUOC SET FLAG=1 WHERE BGCUOCID IN({_id})";
                    msg = "Khôi phục bản ghi thành công!";
                }
                else
                    qry = $"UPDATE BGCUOC SET FLAG=0,DELETEDBY='{Authentication.Auth.AuthUser.username}',DELETEDAT=GETDATE() WHERE BGCUOCID IN({_id})";
                SQLServer.Connection.Query(qry);

                var ReturnJson = Json(new { success = msg }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception) { return Json(new { danger = "Lỗi hệ thống vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Xử lý nhập Text Data
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ImportTextData(string txtDataVal, int actionType)
        {
            var SQLServer = new TM.Connection.SQLServer();
            long index = 0;
            var provider = System.Globalization.CultureInfo.InvariantCulture;
            var msg = "Cập nhật thành công";
            var profile_ip = "profile_ip";
            try
            {
                //
                if (string.IsNullOrEmpty(txtDataVal))
                    return Json(new { danger = "Vui lòng nhập giá trị!" }, JsonRequestBehavior.AllowGet);
                //
                //var qry = $"SELECT i.*,g.TITLE AS GROUPTITLE FROM ITEMS i,GROUPS g WHERE i.GROUPID=g.GROUPID AND i.APPKEY='{profile_ip}' AND g.APPKEY='{profile_ip}' AND i.FLAG=1 AND g.FLAG=1 ORDER BY i.TITLE";
                var qry = $"SELECT * FROM GROUPS WHERE APPKEY='{profile_ip}' AND FLAG=1 ORDER BY TITLE";
                var ProfileIPList = SQLServer.Connection.QueryFirstOrDefault<Models.GROUPS>(qry);
                var dataRow = txtDataVal.Split('\n');
                //Remove old
                if (actionType == 2)
                {
                    qry = $"DELETE BGCUOC";
                    SQLServer.Connection.Query(qry);
                }
                index = 0;
                //
                var dataList = new List<Models.BGCUOC>();
                foreach (var i in dataRow)
                {
                    index++;
                    var tmp = i.Trim('\r').Split('\t');
                    if (index == 1) continue;
                    if (tmp.Length > 13)
                    {
                        var _data = new Models.BGCUOC();
                        _data.TENGOI = string.IsNullOrEmpty(tmp[0]) ? null : tmp[0].Trim();
                        _data.PROFILE = string.IsNullOrEmpty(tmp[1]) ? null : tmp[1].Trim();
                        _data.PROFILEIP = getProfileIP(ProfileIPList.TITLE, _data.PROFILE);
                        _data.TOCDO = string.IsNullOrEmpty(tmp[2]) ? 0 : int.Parse(tmp[2].Trim());
                        _data.GIA = string.IsNullOrEmpty(tmp[3]) ? 0 : decimal.Parse(tmp[3].Trim());
                        _data.DICHVUVT_ID = string.IsNullOrEmpty(tmp[4]) ? 0 : int.Parse(tmp[4].Trim());
                        _data.GOICUOCID = string.IsNullOrEmpty(tmp[5]) ? 0 : int.Parse(tmp[5].Trim());
                        _data.TICHHOPID = string.IsNullOrEmpty(tmp[6]) ? 0 : int.Parse(tmp[6].Trim());
                        _data.IS_DATA = string.IsNullOrEmpty(tmp[7]) ? 0 : int.Parse(tmp[7].Trim());
                        _data.IS_TH = string.IsNullOrEmpty(tmp[8]) ? 0 : int.Parse(tmp[8].Trim());
                        if (!string.IsNullOrEmpty(tmp[9])) _data.NGAY_BD = DateTime.ParseExact(tmp[9], "dd/MM/yyyy", provider);
                        if (!string.IsNullOrEmpty(tmp[10])) _data.NGAY_KT = DateTime.ParseExact(tmp[10], "dd/MM/yyyy", provider);
                        _data.EXTRA_TYPE = string.IsNullOrEmpty(tmp[11]) ? 0 : int.Parse(tmp[11].Trim());
                        _data.FLAG = string.IsNullOrEmpty(tmp[12]) ? 0 : int.Parse(tmp[12].Trim());
                        _data.GHICHU = string.IsNullOrEmpty(tmp[13]) ? null : tmp[13].Trim();
                        _data.CREATEDBY = Authentication.Auth.AuthUser.username;
                        _data.CREATEDAT = DateTime.Now;
                        //PROFILEIP
                        //var PROFILEIP = ProfileIPList.TITLE.Trim(',').Split(',');
                        //_data.PROFILEIP = ",";
                        //if (!string.IsNullOrEmpty(tmp[1]))
                        //    foreach (var item in PROFILEIP)
                        //    {
                        //        var PROFILEList = tmp[1].Split('_');
                        //        _data.PROFILEIP += PROFILEList.Length > 1 ? $"{PROFILEList[0]}{item}_{PROFILEList[1]}," : "";
                        //    }
                        ////_data.PROFILEIP = string.IsNullOrEmpty(tmp[0]) ? null : tmp[0].Trim();
                        dataList.Add(_data);
                    }
                }
                //
                SQLServer.Connection.Insert(dataList);
                return Json(new { success = $"{msg} - Count: {dataList.Count}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        public string getProfileIP(string ProfileIPList, string PROFILE)
        {
            var _array = ProfileIPList.Trim(',').Split(',');
            var PROFILEIP = ",";
            if (!string.IsNullOrEmpty(PROFILE))
            {
                var PROFILEList = PROFILE.Split('_');
                if (PROFILEList.Length > 1)
                    foreach (var i in _array)
                        PROFILEIP += $"{PROFILEList[0]}{i}_{PROFILEList[1]},";
                else
                    PROFILEIP = $",{PROFILE},";
            }
            //_data.PROFILEIP = string.IsNullOrEmpty(tmp[0]) ? null : tmp[0].Trim();
            return PROFILEIP;
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