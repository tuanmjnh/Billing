using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Helper;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class DonViBKNController : Controller
    {
        TM.Connection.SQLServer SQLServer;
        TM.Connection.Oracle Oracle;
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
        //public ActionResult ImportTextData()
        //{
        //    return PartialView("PartialImportTextData");
        //}
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
                qry = $"SELECT * FROM DONVI_BKN WHERE FLAG={obj.flag}";

                //Get data for Search
                if (!String.IsNullOrEmpty(obj.search) && obj.search.isNumber())
                    cdt += $"(DONVI_ID={obj.search} OR MA_QUANHUYEN={obj.search}) AND ";
                else if (!string.IsNullOrEmpty(obj.search))
                    cdt += $"(TEN_DV LIKE '%{obj.search}%' OR DIACHI_DV LIKE '%{obj.search}%') AND ";

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
                var data = SQLServer.Connection.Query<Models.DONVI_BKN>(qry);

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
                    if (obj.sort.ToUpper() == "TEN_DV" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.TEN_DV);
                    else if (obj.sort.ToUpper() == "TEN_DV" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.TEN_DV);
                    else if (obj.sort.ToUpper() == "MA_QUANHUYEN" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.MA_QUANHUYEN);
                    else if (obj.sort.ToUpper() == "MA_QUANHUYEN" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.MA_QUANHUYEN);
                    else
                        data = data.OrderBy(m => m.MA_QUANHUYEN).ThenBy(m => m.TEN_DV);
                }
                else
                    data = data.OrderBy(m => m.MA_QUANHUYEN).ThenBy(m => m.TEN_DV);
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
        [HttpGet]
        public JsonResult UpdateDataHNI(Models.DONVI_BKN obj)
        {
            //var provider = System.Globalization.CultureInfo.InvariantCulture;
            var index = 0;
            var qry = "";
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
                //
                qry = $"SELECT * FROM DONVI_BKN WHERE CAPDONVI_ID IN(29,30)";
                var donvi_hni = Oracle.Connection.Query<Models.DONVI_BKN>(qry);
                qry = $"SELECT * FROM DONVI_BKN";
                var data = SQLServer.Connection.Query<Models.DONVI_BKN>(qry);
                //
                var DataInsert = new List<Models.DONVI_BKN>();
                var DataUpdate = new List<Models.DONVI_BKN>();
                foreach (var i in donvi_hni)
                {
                    var _tmp = data.FirstOrDefault(d => d.DONVI_ID == i.DONVI_ID);
                    if (_tmp != null)
                    {
                        _tmp.DONVI_CHA_ID = i.DONVI_CHA_ID;
                        _tmp.CAPDONVI_ID = i.CAPDONVI_ID;
                        _tmp.MA_DV = i.MA_DV;
                        _tmp.TEN_DV = i.TEN_DV;
                        _tmp.DIACHI_DV = i.DIACHI_DV;
                        _tmp.SO_FAX = i.SO_FAX;
                        _tmp.MST = i.MST;
                        _tmp.SO_TK = i.SO_TK;
                        _tmp.SO_DT = i.SO_DT;
                        _tmp.GHICHU = i.GHICHU;
                        _tmp.DV_NOITHANH = i.DV_NOITHANH;
                        _tmp.DV_DAUVAO = i.DV_DAUVAO;
                        _tmp.DV_TT = i.DV_TT;
                        _tmp.DV_CAP1 = i.DV_CAP1;
                        _tmp.TRANGTHAI = i.TRANGTHAI;
                        _tmp.LOAIHOST_ID = i.LOAIHOST_ID;
                        _tmp.TINHTP_ID = i.TINHTP_ID;
                        _tmp.DONVI_ID_MAP = i.DONVI_ID_MAP;
                        _tmp.FLAG = 1;
                        DataUpdate.Add(_tmp);
                    }
                    else
                    {
                        var tmp = new Models.DONVI_BKN();
                        tmp.DONVI_ID = i.DONVI_ID;
                        tmp.DONVI_CHA_ID = i.DONVI_CHA_ID;
                        tmp.CAPDONVI_ID = i.CAPDONVI_ID;
                        tmp.MA_DV = i.MA_DV;
                        tmp.TEN_DV = i.TEN_DV;
                        tmp.DIACHI_DV = i.DIACHI_DV;
                        tmp.SO_FAX = i.SO_FAX;
                        tmp.MST = i.MST;
                        tmp.SO_TK = i.SO_TK;
                        tmp.SO_DT = i.SO_DT;
                        tmp.GHICHU = i.GHICHU;
                        tmp.DV_NOITHANH = i.DV_NOITHANH;
                        tmp.DV_DAUVAO = i.DV_DAUVAO;
                        tmp.DV_TT = i.DV_TT;
                        tmp.DV_CAP1 = i.DV_CAP1;
                        tmp.TRANGTHAI = i.TRANGTHAI;
                        tmp.LOAIHOST_ID = i.LOAIHOST_ID;
                        tmp.TINHTP_ID = i.TINHTP_ID;
                        tmp.DONVI_ID_MAP = i.DONVI_ID_MAP;
                        tmp.FLAG = 1;
                        DataInsert.Add(tmp);
                    }
                }
                //
                if (DataInsert.Count > 0) SQLServer.Connection.Insert(DataInsert);
                if (DataUpdate.Count > 0) SQLServer.Connection.Update(DataUpdate);
                //
                var ReturnJson = Json(new { success = $"DONVI_BKN - Cập nhật: {DataUpdate.Count} - Thêm mới: {DataInsert.Count}" }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception ex) { return Json(new { danger = "Lỗi hệ thống vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
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