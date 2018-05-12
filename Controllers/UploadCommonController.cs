using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using Dapper;
using Dapper.Contrib.Extensions;
using TM.Helper;
using System.Dynamic;
using System.Reflection;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class UploadCommonController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                var SQLServer = new TM.Connection.SQLServer();
                FileManagerController.InsertDirectory(Common.Directories.HDData);
                ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(Common.Directories.HDData).OrderByDescending(d => d).ToList();
                ViewBag.dvvt = SQLServer.Connection.Query<Models.DICHVU_VT_BKN>("SELECT * FROM DICHVU_VT_BKN WHERE TT_UPLOAD=1 AND FLAG=1 ORDER BY ORDERS");
            }
            catch (Exception ex) { this.danger(ex.Message); }
            return View();
        }
        private int UploadData(string DataSource, string strUpload, List<string> fileUpload)
        {
            try
            {
                TM.IO.FileDirectory.CreateDirectory(DataSource, false);
                return UploadBase(DataSource, strUpload, fileUpload);
            }
            catch (Exception) { throw; }
        }
        private T Create<T>() where T : class, new()
        {
            return new T();
        }
        private string ImportData(Common.DefaultObj obj, Models.DICHVU_VT_BKN DVVT)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
            try
            {
                if (DVVT.TABLE_TARGET == "NET")
                {
                    var qry = $"SELECT * FROM {DVVT.MA_DVVT}";
                    var collection = FoxPro.Connection.Query<Models.NET>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                }
                if (DVVT.TABLE_TARGET == "MEGA")
                {
                    var qry = $"SELECT * FROM {DVVT.MA_DVVT}";
                    var collection = FoxPro.Connection.Query<Models.MEGA>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                }
                if (DVVT.TABLE_TARGET == "HD_NET")
                {
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    var qry = $"SELECT * FROM {DVVT.MA_DVVT}";

                    //
                    var collection = FoxPro.Connection.Query<Models.HD_NET>(qry).ToList(); ;
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    var collectionInsert = new List<Models.HD_NET>();
                    //
                    var new_dbkh = FoxPro.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
                    UpdateProperties(new_dbkh, TABLE_FIELD_SET, DVVT, obj.datetime);
                    var old_dbkh = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>($"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=1");
                    var DataInsert = new List<Models.DB_THANHTOAN_BKN>();
                    var DataUpdate = new List<Models.DB_THANHTOAN_BKN>();
                    foreach (var i in new_dbkh)
                    {
                        var _tmp = old_dbkh.FirstOrDefault(d => d.ACCOUNT == i.ACCOUNT);
                        var _tmp_collection = collection.FirstOrDefault(d => d.ACCOUNT == i.ACCOUNT);
                        if (_tmp_collection != null)
                            _tmp_collection.DBKH_ID = i.ID;
                        collectionInsert.Add(_tmp_collection);

                        if (_tmp != null)
                        {
                            if (!string.IsNullOrEmpty(i.MA_TB)) _tmp.MA_TB = i.MA_TB;
                            if (!string.IsNullOrEmpty(i.TEN_TT)) _tmp.TEN_TT = i.TEN_TT.Trim();
                            if (!string.IsNullOrEmpty(i.DIACHI_TT)) _tmp.DIACHI_TT = i.DIACHI_TT.Trim();
                            if (!string.IsNullOrEmpty(i.DIENTHOAI)) _tmp.DIENTHOAI = i.DIENTHOAI.Trim();
                            if (!string.IsNullOrEmpty(i.MA_DVI)) _tmp.MA_DVI = i.MA_DVI.Trim();
                            if (!string.IsNullOrEmpty(i.MA_CBT)) _tmp.MA_CBT = i.MA_CBT.Trim();
                            if (!string.IsNullOrEmpty(i.MA_TUYEN)) _tmp.MA_TUYEN = i.MA_TUYEN.Trim();
                            if (!string.IsNullOrEmpty(i.MA_KH)) _tmp.MA_KH = i.MA_KH.Trim();
                            if (!string.IsNullOrEmpty(i.MA_TT_HNI)) _tmp.MA_TT_HNI = i.MA_TT_HNI.Trim();
                            if (!string.IsNullOrEmpty(i.MS_THUE)) _tmp.MS_THUE = i.MS_THUE.Trim();
                            _tmp.MA_DT = i.MA_DT;
                            _tmp.KHLON_ID = i.KHLON_ID;
                            _tmp.LOAIKH_ID = i.LOAIKH_ID;
                            _tmp.TH_SD = 1;
                            _tmp.ISNULL = 0;
                            _tmp.ISNULLMT = 0;
                            _tmp.FIX = 1;
                            _tmp.FLAG = 1;
                            DataUpdate.Add(_tmp);
                        }
                        else
                        {
                            i.MA_DT = 1;
                            i.TH_SD = 1;
                            i.ISNULL = 1;
                            i.ISNULLMT = 1;
                            i.FIX = 1;
                            i.FLAG = 1;
                            DataInsert.Add(i);
                        }
                    }
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collectionInsert);
                    SQLServer.Connection.Insert(DataInsert);
                    SQLServer.Connection.Update(DataUpdate);
                }
                else if (DVVT.TABLE_TARGET == "MYTV")
                {
                    var qry = $"SELECT * FROM {DVVT.MA_DVVT}";
                    var collection = FoxPro.Connection.Query<Models.MYTV>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                }
                else if (DVVT.TABLE_TARGET == "HD_MYTV")
                {
                    var qry = $"SELECT * FROM {DVVT.MA_DVVT}";
                    var collection = FoxPro.Connection.Query<Models.HD_MYTV>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                }
                else if (DVVT.TABLE_TARGET == "DISCOUNT")
                {
                    var qry = $"SELECT * FROM {DVVT.MA_DVVT}";
                    var collection = FoxPro.Connection.Query<Models.DISCOUNT>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                }
                else if (DVVT.TABLE_TARGET == "THANHTOANTRUOC")
                {
                    var qry = $"SELECT a.*,a.MA_TB AS ACCOUNT FROM {DVVT.MA_DVVT} a";
                    var collection = FoxPro.Connection.Query<Models.THANHTOANTRUOC>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                    //Update NGAY_BD
                    qry = $"UPDATE THANHTOANTRUOC SET NGAY_BD=CAST(CAST(NAM AS varchar(4))+'-'+CAST(THANG AS varchar(2))+'-1' as datetime) WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    SQLServer.Connection.Query(qry);
                    //Update NGAY_KT
                    qry = $"UPDATE THANHTOANTRUOC SET NGAY_KT=DATEADD(MONTH,SOTHANG,CAST(CAST(NAM as varchar(4))+'-'+CAST(THANG as varchar(2))+'-01' as datetime)) WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    SQLServer.Connection.Query(qry);
                }
                else if (DVVT.TABLE_TARGET == "CD")
                {
                    var qry = $"ALTER TABLE {DVVT.MA_DVVT} ALTER COLUMN dia_chi c(100)";
                    FoxPro.Connection.Query(qry);
                    qry = $"SELECT a.*,a.MA_KH1 AS MA_TT,a.TEN_CQ AS TEN_TT,a.DIA_CHI AS DIACHI_TT,a.MA_ST AS MS_THUE,a.dvql_id AS MA_DVI,a.TONG_CUOC AS TONGCONG FROM {DVVT.MA_DVVT} a";
                    var collection = FoxPro.Connection.Query<Models.CD>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                }
                else if (DVVT.TABLE_TARGET == "DD")
                {
                    var qry = $"SELECT a.*,a.cuoc_cthue AS TONG,a.thue AS VAT,a.ma_cq AS MA_KH,a.ma_cq AS MA_TT,a.taikhoan AS BANKNUMBER,GIAMTRU AS GIAM_TRU FROM {DVVT.MA_DVVT} a";
                    var collection = FoxPro.Connection.Query<Models.DD>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                }
                else if (DVVT.TABLE_TARGET == "HD_TSL")
                {
                    var qry = $"SELECT * FROM {DVVT.MA_DVVT}";
                    var collection = FoxPro.Connection.Query<Models.HD_TSL>(qry).ToList();
                    //
                    var TABLE_FIELD_SET = DVVT.TABLE_FIELD_SET.Trim(',').Split(',');
                    UpdateProperties(collection, TABLE_FIELD_SET, DVVT, obj.datetime);
                    //Delete old
                    SQLServer.Connection.Query($"DELETE {DVVT.TABLE_TARGET} WHERE TYPE_BILL={DVVT.DICHVUVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'");
                    //
                    SQLServer.Connection.Insert(collection);
                }
            }
            catch (Exception) { throw; }
            finally { SQLServer.Close(); FoxPro.Close(); }
            return DVVT.TEN_DVVT;
        }
        private IEnumerable<T> UpdateProperties<T>(IEnumerable<T> collection, string[] TABLE_FIELD_SET, Models.DICHVU_VT_BKN DVVT, DateTime TIME_BILL)
        {
            foreach (var item in collection)
            {
                //1/1/0001 12:00:00 AM
                var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var p in properties)
                    if (TABLE_FIELD_SET.Contains(p.Name)) SetNullData(item, p, DVVT, TIME_BILL);
                item.Trim().TCVN3ToUnicode().FixDateFoxPro();
            }
            return collection;
        }
        private void SetNullData<T>(T item, System.Reflection.PropertyInfo p, Models.DICHVU_VT_BKN DVVT, DateTime TIME_BILL)
        {
            if (p.PropertyType == typeof(Guid) || !p.CanWrite || !p.CanRead)
            {
                p.SetValue(item, Guid.NewGuid());
            }
            else if (p.PropertyType == typeof(int) || !p.CanWrite || !p.CanRead)
            {
                p.SetValue(item, DVVT.DICHVUVT_ID);
            }
            else if (p.PropertyType == typeof(DateTime) || !p.CanWrite || !p.CanRead)
            {
                p.SetValue(item, TIME_BILL);
            }
        }
        //Upload
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GeneralUpload(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            string strUpload = "Cập nhật thành công ";
            var qry = "";
            try
            {
                //
                qry = "SELECT * FROM DICHVU_VT_BKN WHERE TT_SUDUNG=2";
                var DICHVU_VT_BKN = SQLServer.Connection.Query<Models.DICHVU_VT_BKN>(qry);
                var fileUpload = new List<string>();
                var dvvt = DICHVU_VT_BKN.FirstOrDefault(d => d.DICHVUVT_ID == obj.data_id);
                if (obj.data_type == 0 || obj.data_type == 2)
                {
                    if (dvvt != null)
                        fileUpload.Add($"{dvvt.MA_DVVT.ToLower()}.dbf");
                    var upload = UploadData(obj.DataSource, strUpload, fileUpload);
                    if (upload == (int)Common.Objects.ResultCode._extension)
                        return Json(new { danger = "Tệp phải định dạng .dbf!" }, JsonRequestBehavior.AllowGet);
                    else if (upload == (int)Common.Objects.ResultCode._length)
                        return Json(new { danger = "Chưa đủ tệp!" }, JsonRequestBehavior.AllowGet);
                }
                if (dvvt != null && obj.data_type > 0)
                    strUpload += ImportData(obj, dvvt);
                return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        //Upload
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GetPreData(Common.DefaultObj obj, string TimePreData)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            string dvvv = "???";
            var qry = "";
            try
            {
                qry = "SELECT * FROM DICHVU_VT_BKN WHERE TT_SUDUNG=2";
                var DICHVU_VT_BKN = SQLServer.Connection.Query<Models.DICHVU_VT_BKN>(qry);
                var dvvt = DICHVU_VT_BKN.FirstOrDefault(d => d.DICHVUVT_ID == obj.data_id);
                if (dvvt != null)
                    dvvv = dvvt.TEN_DVVT;
                var OldObj = new Common.DefaultObj();
                OldObj.time = TimePreData;
                OldObj = getDefaultObj(OldObj);
                if (obj.data_id == 9001) //Khuyến Mại
                {
                    qry = $"DELETE FROM DISCOUNT WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    SQLServer.Connection.Query(qry);
                    //
                    qry = $@"INSERT INTO DISCOUNT 
                             SELECT NEWID() AS ID,TYPE_HD,TYPE_BILL,CAST('{obj.datetime.ToString("yyyy/MM/dd")}' AS datetime) AS TIME_BILL,ACCOUNT,TYPE,TYPEID,VALUE,DETAILS,NGAY_DK,NGAY_BD,NGAY_KT,FLAG 
                             FROM DISCOUNT WHERE FORMAT(TIME_BILL,'MM/yyyy')='{OldObj.month_year_time}' AND FLAG=1 AND ((NGAY_KT IS NULL) OR (CAST('{obj.datetime.ToString("yyyy/MM/dd")}' AS datetime) BETWEEN NGAY_BD AND NGAY_KT))";
                    SQLServer.Connection.Query(qry);
                }
                else if (obj.data_id == 9003) //Đặt cọc
                {
                    qry = $"DELETE FROM THANHTOANTRUOC WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9003";
                    SQLServer.Connection.Query(qry);
                    //
                    qry = $@"INSERT INTO THANHTOANTRUOC 
                             SELECT NEWID() AS ID,TYPE_BILL,CAST('{obj.datetime.ToString("yyyy/MM/dd")}' AS datetime) AS TIME_BILL,ID_TTT,ACCOUNT,MA_TB,SO_MAY,DVVT_ID,TOCDO_ID,0 AS GOICUOC_ID,TIENHANMUC,TONGHANMUC,TT,KHOANTIEN,THANG,NAM,NGAY_BD,NGAY_KT,SODU_TONG AS SODU,ID_CV,NGAY_TT,NGUONDL,CUOC_FIX,TEN_DVVT,SOTHANG,0 AS THUC_TRU,NGAY_SD,GHI_CHU,0 AS TONG,0 AS EXTRA_TONG,0 AS SODU_TONG,1 AS FLAG 
                             FROM THANHTOANTRUOC WHERE FORMAT(TIME_BILL,'MM/yyyy')='{OldObj.month_year_time}' AND TYPE_BILL=9003 AND SODU_TONG>0";
                    SQLServer.Connection.Query(qry);
                }
                else if (obj.data_id == 9682) //MyTV Mergin
                {
                    qry = $"DELETE FROM HD_MYTV WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9682";
                    SQLServer.Connection.Query(qry);
                    //
                    qry = $@"INSERT INTO HD_MYTV 
                             SELECT NEWID() AS ID,MYTV_ID,DBKH_ID,TYPE_BILL,CAST('{obj.datetime.ToString("yyyy/MM/dd")}' AS datetime) AS TIME_BILL,ACCOUNT,TOC_DO,TT_THANG,NGAY_TB,NGAY_TB_PTTB,GOICUOCID,TH_THANG,TH_HUY,DUPECOUNT,ISDATMOI,ISHUY,ISTTT,ISDATCOC,PAYTV_FEE,SUB_FEE,GIAM_TRU,TONG_TTT,TONG_DC,TONG_IN,TONG,VAT,TONGCONG,SIGNDATE,REGISTDATE,NGAY_SD,NGAY_KHOA,NGAY_MO,NGAY_KT 
                             FROM HD_MYTV WHERE FORMAT(TIME_BILL,'MM/yyyy')='{OldObj.month_year_time}' AND TYPE_BILL=9682";
                    SQLServer.Connection.Query(qry);
                }
                else if (obj.data_id == 9662) //MegaVNN Mergin
                {
                    qry = $"DELETE FROM HD_NET WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9662";
                    SQLServer.Connection.Query(qry);
                    //
                    qry = $@"INSERT INTO HD_NET 
                             SELECT NEWID() AS ID,NET_ID,DBKH_ID,TYPE_BILL,CAST('{obj.datetime.ToString("yyyy/MM/dd")}' AS datetime) AS TIME_BILL,ACCOUNT,MA_TB,TOC_DO,TT_THANG,TBTHG,NGAY_TB,NGAY_TB_PTTB,GOICUOCID,TH_THANG,TH_HUY,DUPECOUNT,ISDATMOI,ISHUY,ISTTT,ISDATCOC,GIAM_TRU,CUOC_IP,CUOC_EMAIL,CUOC_DATA,CUOC_SD,CUOC_TB,TONG_TTT,TONG_DC,TONG_IN,TONG,VAT,TONGCONG,NGAY_DKY,NGAY_CAT,NGAY_HUY,NGAY_CHUYEN,NGAY_SD,NGAY_KHOA,NGAY_MO,NGAY_KT
                             FROM HD_NET WHERE FORMAT(TIME_BILL,'MM/yyyy')='{OldObj.month_year_time}' AND TYPE_BILL=9662";
                    SQLServer.Connection.Query(qry);
                }
                else if (obj.data_id == 9692) //Fiber Mergin
                {
                    qry = $"DELETE FROM HD_NET WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9692";
                    SQLServer.Connection.Query(qry);
                    //
                    qry = $@"INSERT INTO HD_NET 
                             SELECT NEWID() AS ID,NET_ID,DBKH_ID,TYPE_BILL,CAST('{obj.datetime.ToString("yyyy/MM/dd")}' AS datetime) AS TIME_BILL,ACCOUNT,MA_TB,TOC_DO,TT_THANG,TBTHG,NGAY_TB,NGAY_TB_PTTB,GOICUOCID,TH_THANG,TH_HUY,DUPECOUNT,ISDATMOI,ISHUY,ISTTT,ISDATCOC,GIAM_TRU,CUOC_IP,CUOC_EMAIL,CUOC_DATA,CUOC_SD,CUOC_TB,TONG_TTT,TONG_DC,TONG_IN,TONG,VAT,TONGCONG,NGAY_DKY,NGAY_CAT,NGAY_HUY,NGAY_CHUYEN,NGAY_SD,NGAY_KHOA,NGAY_MO,NGAY_KT
                             FROM HD_NET WHERE FORMAT(TIME_BILL,'MM/yyyy')='{OldObj.month_year_time}' AND TYPE_BILL=9692";
                    SQLServer.Connection.Query(qry);
                }
                else if (obj.data_id == 9999) //Tích hợp
                {
                    qry = $"DELETE FROM DANHBA_GOICUOC_TICHHOP WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FIX=1 AND FLAG=1";
                    SQLServer.Connection.Query(qry);
                    //
                    qry = $@"INSERT INTO DANHBA_GOICUOC_TICHHOP 
                             SELECT NEWID() AS ID,CAST('{obj.datetime.ToString("yyyy/MM/dd")}' AS datetime) AS TIME_BILL,MA_TB,ACCOUNT,TEN_GOICUOC,LOAIGOICUOC_ID,DICHVUVT_ID,LOAIMAY_ID,GOICUOC_ID,NGAY_BD,NGAY_KT,DUPE_COUNT,DUPE_FLAG,NO_DUPE,FIX_NGAY_KT,TH_THANG,TH_SO_NGAY,FIX,EXTRA_TYPE,DETAILS,FLAG
                             FROM HD_NET WHERE FORMAT(TIME_BILL,'MM/yyyy')='{OldObj.month_year_time}' AND FIX=1 AND FLAG=1";
                    SQLServer.Connection.Query(qry);
                }
                return Json(new { success = $"Nhập dữ liệu {dvvv} từ {OldObj.datetime.ToString("MM/yyyy")} sang {obj.datetime.ToString("MM/yyyy")} thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        //Xử lý nhập Text Data
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyNhapTextData(Common.DefaultObj obj, int rdoTextAddType, string txtDataVal)
        {
            var SQLServer = new TM.Connection.SQLServer();
            long index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var provider = System.Globalization.CultureInfo.InvariantCulture;
            var msg = "Cập nhật thành công";
            try
            {
                //
                if (string.IsNullOrEmpty(txtDataVal))
                    return Json(new { danger = "Vui lòng nhập giá trị!" }, JsonRequestBehavior.AllowGet);
                var qry = "";
                var dataRow = txtDataVal.Split('\n');
                //
                if (rdoTextAddType == 9999)//Tích hợp
                {
                    //Remove old
                    if (obj.data_id == 2)
                    {
                        qry = $"DELETE DANHBA_GOICUOC_TICHHOP WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FIX=1";
                        SQLServer.Connection.Query(qry);
                    }
                    qry = "SELECT MAX(ID) ID FROM DANHBA_GOICUOC_TICHHOP";
                    var rs_tmp = SQLServer.Connection.QueryFirstOrDefault(qry).ID;
                    if (rs_tmp == null)
                        return Json(new { danger = "Vui lòng lấy danh sách biến động tích hợp trước!" }, JsonRequestBehavior.AllowGet);
                    index = rs_tmp;
                    //
                    var dataList = new List<Models.DANHBA_GOICUOC_TICHHOP>();
                    foreach (var i in dataRow)
                    {
                        index++;
                        var tmp = i.Trim('\r').Split('\t');
                        if (index == rs_tmp + 1) continue;
                        if (tmp.Length > 4)
                        {
                            dataList.Add(new Models.DANHBA_GOICUOC_TICHHOP()
                            {
                                ID = index,
                                TIME_BILL = obj.datetime,
                                MA_TB = tmp[0],
                                ACCOUNT = tmp[0],
                                DICHVUVT_ID = int.Parse(tmp[1]),
                                GOICUOC_ID = int.Parse(tmp[2]),
                                LOAIGOICUOC_ID = int.Parse(tmp[3]),
                                DETAILS = tmp[4],
                                NGAY_BD = obj.datetime,
                                FIX = 1,
                                EXTRA_TYPE = 0,
                                FLAG = 1
                            });
                        }
                    }
                    //
                    SQLServer.Connection.Insert(dataList);
                    //
                    qry = $"UPDATE DANHBA_GOICUOC_TICHHOP SET EXTRA_TYPE=1 WHERE LOAIGOICUOC_ID IN (SELECT GOICUOCID FROM BGCUOC WHERE EXTRA_TYPE=1) AND FIX=1 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    SQLServer.Connection.Query(qry);
                    //
                    //qry = $@"INSERT DANHBA_GOICUOC_TICHHOP select ((select MAX(ID) from DANHBA_GOICUOC_TICHHOP)+ROW_NUMBER() OVER(ORDER BY ID)) as [ID],[TIME_BILL],[MA_TB],[ACCOUNT],[TEN_GOICUOC],[LOAIGOICUOC_ID],[DICHVUVT_ID],[LOAIMAY_ID],[GOICUOC_ID],[NGAY_BD],[NGAY_KT],[DUPE_COUNT],[DUPE_FLAG],[NO_DUPE],[FIX_NGAY_KT],[TH_THANG],[TH_SO_NGAY],[FIX],[EXTRA_TYPE],0 as [FLAG] from DANHBA_GOICUOC_TICHHOP a where FIX=1 and FLAG=1 and FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and ACCOUNT in (select ACCOUNT from DANHBA_GOICUOC_TICHHOP where FIX=0 and LOAIGOICUOC_ID!=a.LOAIGOICUOC_ID and FLAG=1 and FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}');
                    //         UPDATE DANHBA_GOICUOC_TICHHOP SET EXTRA_TYPE=1 WHERE LOAIGOICUOC_ID IN (SELECT GOICUOCID FROM BANGGIACUOC WHERE EXTRA_TYPE=1) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    //SQLServer.Connection.Query(qry, null, null, true, 0);

                    //qry = $"update a set a.FLAG=2 from DANHBA_GOICUOC_TICHHOP a where FIX=1 and FLAG=1 and FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and ACCOUNT in (select ACCOUNT from DANHBA_GOICUOC_TICHHOP where FIX=0 and LOAIGOICUOC_ID!=a.LOAIGOICUOC_ID and FLAG=1 and FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}')";
                    //SQLServer.Connection.Query(qry);
                    //qry = $"update a set a.FLAG=1,a.LOAIGOICUOC_ID=(select MAX(LOAIGOICUOC_ID) from DANHBA_GOICUOC_TICHHOP where FIX=0 and LOAIGOICUOC_ID!=a.LOAIGOICUOC_ID and FLAG=1 and FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}') from DANHBA_GOICUOC_TICHHOP a where a.FIX=1 and a.FLAG=2 and FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    //SQLServer.Connection.Query(qry);
                    msg += $" tích hợp - {dataList.Count} thuê bao";
                }
                else if (rdoTextAddType == 9001)//Khuyến mại
                {
                    //Remove old
                    if (obj.data_id == 2)
                    {
                        qry = $"DELETE DISCOUNT WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";//AND TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX_IN}
                        SQLServer.Connection.Query(qry);
                    }
                    var dataList = new List<Models.DISCOUNT>();
                    index = 0;
                    foreach (var i in dataRow)
                    {
                        index++;
                        var tmp = i.Trim('\r').Split('\t');
                        if (index == 1) continue;
                        if (tmp.Length > 6)
                        {
                            var _data = new Models.DISCOUNT();
                            _data.ID = Guid.NewGuid();
                            _data.TYPE_BILL = 9001;
                            _data.TIME_BILL = obj.datetime;
                            _data.ACCOUNT = string.IsNullOrEmpty(tmp[0]) ? null : tmp[0].Trim();
                            _data.TYPE_HD = int.Parse(tmp[1]);
                            _data.TYPEID = int.Parse(tmp[2]);
                            _data.TYPE = Enum.GetName(typeof(Common.Objects.TYPE_DISCOUNT), int.Parse(tmp[2]));//Common.Objects.TYPE_DISCOUNT.FIX_IN.ToString(),
                            _data.VALUE = decimal.Parse(tmp[3]);
                            _data.NGAY_DK = obj.datetime;
                            if (!string.IsNullOrEmpty(tmp[4])) _data.NGAY_BD = DateTime.ParseExact(tmp[4], "dd/MM/yyyy", provider);
                            if (!string.IsNullOrEmpty(tmp[5])) _data.NGAY_KT = DateTime.ParseExact(tmp[5], "dd/MM/yyyy", provider);
                            _data.DETAILS = string.IsNullOrEmpty(tmp[6]) ? null : tmp[6].Trim();
                            _data.FLAG = 1;
                            dataList.Add(_data);
                        }
                    }
                    SQLServer.Connection.Insert(dataList);
                    msg += $" khuyến mại - {dataList.Count} thuê bao";
                }
                else if (rdoTextAddType == 9006)//Thanh toán trước
                {
                    //Remove old
                    if (obj.data_id == 2)
                    {
                        qry = $"DELETE THANHTOANTRUOC WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9006";
                        SQLServer.Connection.Query(qry);
                    }
                    //Get data ttt
                    qry = $"SELECT * FROM THANHTOANTRUOC WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL IN(9002,9006)";
                    var datattt = SQLServer.Connection.Query<Models.THANHTOANTRUOC>(qry).ToList();
                    var dataList = new List<Models.THANHTOANTRUOC>();
                    index = 0;
                    //
                    foreach (var i in dataRow)
                    {
                        index++;
                        var tmp = i.Trim('\r').Split('\t');
                        if (index == 1) continue;
                        if (tmp.Length > 7)
                        {
                            var account = string.IsNullOrEmpty(tmp[0]) ? null : tmp[0].Trim();
                            //Check exist ttt
                            if (datattt.Any(d => d.ACCOUNT == account)) continue;
                            //Check exist List Insert
                            if (dataList.Any(d => d.ACCOUNT == account)) continue;

                            var _data = new Models.THANHTOANTRUOC();
                            _data.ID = Guid.NewGuid();
                            _data.TYPE_BILL = 9006;
                            _data.TIME_BILL = obj.datetime;
                            _data.ACCOUNT = _data.MA_TB = _data.SO_MAY = account;
                            _data.DVVT_ID = int.Parse(tmp[1]);
                            _data.TOCDO_ID = 0;
                            _data.GOICUOC_ID = 0;
                            _data.TONGHANMUC = _data.TIENHANMUC = int.Parse(tmp[2]);
                            _data.SODU = int.Parse(tmp[3]);
                            _data.KHOANTIEN = "TTT";
                            _data.THUC_TRU = 0;
                            _data.ID_CV = 0;
                            _data.NGUONDL = 0;
                            _data.SOTHANG = int.Parse(tmp[6]);
                            _data.THANG = int.Parse(tmp[4]);
                            _data.NAM = int.Parse(tmp[5]);
                            _data.NGAY_TT = DateTime.Now;
                            _data.GHI_CHU = string.IsNullOrEmpty(tmp[7]) ? null : tmp[7].Trim();
                            _data.FLAG = 1;
                            dataList.Add(_data);
                        }
                    }
                    SQLServer.Connection.Insert(dataList);
                    //Update NGAY_BD
                    qry = $"UPDATE THANHTOANTRUOC SET NGAY_BD=CAST(CAST(NAM AS varchar(4))+'-'+CAST(THANG AS varchar(2))+'-1' as datetime) WHERE TYPE_BILL=9006 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    SQLServer.Connection.Query(qry);
                    //Update NGAY_KT
                    qry = $"UPDATE THANHTOANTRUOC SET NGAY_KT=DATEADD(MONTH,SOTHANG,CAST(CAST(NAM as varchar(4))+'-'+CAST(THANG as varchar(2))+'-01' as datetime)) WHERE TYPE_BILL=9006 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    SQLServer.Connection.Query(qry);
                    msg += $" thanh toán trước fix - {dataList.Count} thuê bao";
                }
                else if (rdoTextAddType == 9003)//Đặt cọc
                {
                    //Remove old
                    if (obj.data_id == 2)
                    {
                        qry = $"DELETE THANHTOANTRUOC WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9003";
                        SQLServer.Connection.Query(qry);
                    }
                    var dataList = new List<Models.THANHTOANTRUOC>();
                    index = 0;
                    foreach (var i in dataRow)
                    {
                        index++;
                        var tmp = i.Trim('\r').Split('\t');
                        if (index == 1) continue;
                        if (tmp.Length > 4)
                        {
                            var account = string.IsNullOrEmpty(tmp[0]) ? null : tmp[0].Trim();
                            //Check exist
                            qry = $"SELECT * FROM THANHTOANTRUOC WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9003 AND ACCOUNT='{account}'";
                            if (SQLServer.Connection.Query<Models.THANHTOANTRUOC>(qry).Count() > 0) continue;
                            if (dataList.Any(d => d.ACCOUNT == account)) continue;

                            var _data = new Models.THANHTOANTRUOC();
                            _data.ID = Guid.NewGuid();
                            _data.TYPE_BILL = 9003;
                            _data.TIME_BILL = obj.datetime;
                            _data.ACCOUNT = _data.MA_TB = _data.SO_MAY = account;
                            _data.DVVT_ID = int.Parse(tmp[1]);
                            _data.TOCDO_ID = 0;
                            _data.GOICUOC_ID = 0;
                            _data.TONGHANMUC = int.Parse(tmp[2]);
                            _data.SODU = int.Parse(tmp[3]);
                            _data.KHOANTIEN = "DC";
                            _data.THUC_TRU = 0;
                            _data.THANG = obj.datetime.Month;
                            _data.NAM = obj.datetime.Year;
                            _data.NGAY_TT = DateTime.Now;
                            _data.GHI_CHU = string.IsNullOrEmpty(tmp[4]) ? null : tmp[4].Trim();
                            _data.FLAG = 1;
                            dataList.Add(_data);
                        }
                    }
                    SQLServer.Connection.Insert(dataList);
                    msg += $" đặt cọc - {dataList.Count} thuê bao";
                }
                return Json(new { success = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Cập nhật Danh bạ đường thu
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateDBDT(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                //Get Data HNI
                var qry = $"SELECT * FROM DB_DUONGTHU_BKN";
                var data_hni = Oracle.Connection.Query<Models.DB_DUONGTHU_BKN>(qry).ToList();
                //Update Data
                foreach (var i in data_hni)
                {
                    i.ID = Guid.NewGuid();
                    i.FIX = 0;
                    i.FLAG = 1;
                    i.MA_DT = i.MA_DT.Trim().ToUpper();
                }
                //Delete Old
                qry = "DELETE DB_DUONGTHU_BKN WHERE FIX=0";
                SQLServer.Connection.Query(qry);
                //Insert Data
                SQLServer.Connection.Insert(data_hni);
                return Json(new { success = $"DB_DUONGTHU_BKN - Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
        }
        //Cập nhật Quận huyện
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateQuanHuyen(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                //Get Data HNI
                var qry = $"SELECT * FROM QUAN_HUYEN_BKN";
                var data_hni = Oracle.Connection.Query<Models.QUAN_HUYEN_BKN>(qry).ToList();
                //Delete Old
                qry = "DELETE QUAN_HUYEN_BKN";
                SQLServer.Connection.Query(qry);
                //Insert Data
                SQLServer.Connection.Insert(data_hni);
                return Json(new { success = $"QUAN_HUYEN_BKN - Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
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
        //private string MappingFile(int type)
        //{
        //    switch (type)
        //    {
        //        case (int)Common.Objects.TYPE_BILL.MEGAVNN:
        //            return Common.Objects.TYPE_BILL.MEGAVNN.ToString().ToLower();
        //        case (int)Common.Objects.TYPE_BILL.FIBERVNN:
        //            return Common.Objects.TYPE_BILL.FIBERVNN.ToString().ToLower();
        //        case (int)Common.Objects.TYPE_BILL.MYTV:
        //            return Common.Objects.TYPE_BILL.MYTV.ToString().ToLower();
        //        case (int)Common.Objects.TYPE_BILL.FIBERVNNGD:
        //            return Common.Objects.TYPE_BILL.FIBERVNNGD.ToString().ToLower();
        //        default: return null;
        //    }
        //}
    }
}