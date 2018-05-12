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
    public class MyTVController : BaseController
    {
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
        public JsonResult GeneralUpload(Common.DefaultObj obj)
        {
            var index = 0;

            obj.DataSource = Common.Directories.HDDataSource;
            FileManagerController.InsertDirectory(obj.DataSource);
            obj = getDefaultObj(obj);
            string strUpload = "Tải tệp thành công!";
            try
            {
                //TM.IO.FileDirectory.CreateDirectory(obj.DataSource, false);
                FileManagerController.InsertDirectory(obj.DataSource, false);
                var uploadData = UploadBase(obj.DataSource, strUpload);

                if (uploadData == (int)Common.Objects.ResultCode._extension)
                    return Json(new { danger = "Tệp phải định dạng .dbf!" }, JsonRequestBehavior.AllowGet);
                else if (uploadData == (int)Common.Objects.ResultCode._length)
                    return Json(new { danger = "Chưa đủ tệp!" }, JsonRequestBehavior.AllowGet);
                //else if (uploadData == (int)Common.Objects.ResultCode._success)
                //    return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
                else
                {
                    FileManagerController.InsertFile(obj.DataSource + obj.file + ".dbf");
                    return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateBill(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
            try
            {
                var qry = $"SELECT * FROM {obj.file}";
                var data = FoxPro.Connection.Query<Models.MYTV>(qry);
                //Insert MYTV FROM TH_MYTV
                foreach (var i in data)
                {
                    index++;
                    i.ID = Guid.NewGuid();
                    i.TYPE_BILL = 8;
                    i.TIME_BILL = obj.datetime;
                    //12/30/1899
                    if (i.USE_DATE.Value.ToString("yyyy/MM/dd") == "1899/12/30" && i.USE_DATE.Value.Year < 9999)
                        i.USE_DATE = (DateTime?)null;
                    if (i.STOP_DATE.Value.ToString("yyyy/MM/dd") == "1899/12/30" && i.STOP_DATE.Value.Year < 9999)
                        i.STOP_DATE = (DateTime?)null;
                    if (i.SUSPENDATE.Value.ToString("yyyy/MM/dd") == "1899/12/30" && i.SUSPENDATE.Value.Year < 9999)
                        i.SUSPENDATE = (DateTime?)null;
                    if (i.RESUMEDATE.Value.ToString("yyyy/MM/dd") == "1899/12/30" && i.RESUMEDATE.Value.Year < 9999)
                        i.RESUMEDATE = (DateTime?)null;
                }
                //Delete MYTV
                qry = $"DELETE MYTV WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //
                SQLServer.Connection.Insert(data.Trim());
                return Json(new { success = $"Cập nhật file cước thành công - {data.Count()} Thuê bao" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                FoxPro.Close();
                SQLServer.Close();
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateContact(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "8";
            try
            {
                #region old
                //    //Get Data
                //    var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.MYTV} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TOTAL_FEE>0";
                //    var MyTV = SQLServer.Connection.Query<Models.MYTV>(qry);
                //    //Get DB PTTB
                //    qry = "SELECT * FROM DANH_BA_MYTV";
                //    var dbpttb = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                //    //Insert MYTV_PTTB with DB PTTB
                //    var data = new List<Models.HD_MYTV>();
                //    foreach (var i in MyTV)
                //    {
                //        index++;
                //        var _data = new Models.HD_MYTV();
                //        _data.ID = Guid.NewGuid();
                //        _data.MYTV_ID = i.ID;
                //        _data.TYPE_BILL = i.TYPE_BILL;
                //        _data.TIME_BILL = i.TIME_BILL;
                //        _data.ACCOUNT = i.USERNAME;
                //        _data.TOC_DO = i.PACKCD;
                //        _data.TH_SD = 1;
                //        _data.NGAY_SD = i.USE_DATE;
                //        _data.NGAY_KHOA = i.SUSPENDATE;
                //        _data.NGAY_MO = i.RESUMEDATE;
                //        _data.NGAY_KT = i.STOP_DATE;
                //        //
                //        var pttb = dbpttb.FirstOrDefault(d => d.LOGINNAME.Trim() == _data.ACCOUNT);
                //        if (pttb != null)
                //        {
                //            _data.TH_SD = pttb.STATUS;
                //            _data.NGAY_TB_PTTB = pttb.NGAY_SUDUNG;
                //            _data.ISDATMOI = pttb.DATMOI_TRONGTHANG;
                //            if (!string.IsNullOrEmpty(pttb.FULLNAME)) _data.TEN_TT = pttb.FULLNAME.Trim();
                //            if (!string.IsNullOrEmpty(pttb.CUSTCATE)) _data.CUSTCATE = pttb.CUSTCATE.Trim();
                //            if (!string.IsNullOrEmpty(pttb.ADDRESS1)) _data.DIACHI_TT = pttb.ADDRESS1.Trim();
                //            if (!string.IsNullOrEmpty(pttb.MOBILE)) _data.DIENTHOAI = pttb.MOBILE.Trim();
                //            if (!string.IsNullOrEmpty(pttb.MA_DVI)) _data.MA_DVI = pttb.MA_DVI.Trim();
                //            if (!string.IsNullOrEmpty(pttb.MA_CBT)) _data.MA_CBT = pttb.MA_CBT.Trim();
                //            if (!string.IsNullOrEmpty(pttb.MA_TUYENTHU)) _data.MA_TUYEN = pttb.MA_TUYENTHU.Trim();
                //            if (!string.IsNullOrEmpty(pttb.MA_KH)) _data.MA_KH = pttb.MA_KH.Trim();
                //            if (!string.IsNullOrEmpty(pttb.MA_TT_HNI)) _data.MA_TT_HNI = pttb.MA_TT_HNI.Trim();
                //            if (!string.IsNullOrEmpty(pttb.MA_ST)) _data.MS_THUE = pttb.MA_ST.Trim();
                //            _data.MA_DT = pttb.MA_DT;
                //            if (pttb.SIGNDATE.Year > 1752 && pttb.SIGNDATE.Year <= 9999) _data.SIGNDATE = pttb.SIGNDATE;
                //            if (pttb.REGISTDATE.Year > 1752 && pttb.REGISTDATE.Year <= 9999) _data.REGISTDATE = pttb.REGISTDATE;
                //        }
                //        else
                //        {
                //            _data.ISNULLDB = 1;
                //        }
                //        data.Add(_data);
                //    }
                //    //Delete HD
                //    qry = $"DELETE {Common.Objects.TYPE_HD.HD_MYTV} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //    SQLServer.Connection.Query(qry);
                //    //
                //    SQLServer.Connection.Insert(data);
                #endregion
                var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.MYTV} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                var data = SQLServer.Connection.Query<Models.MYTV>(qry);
                //Get DB PTTB
                qry = "SELECT * FROM DANH_BA_MYTV";
                var dbpttb = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                //
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var dbkh = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry);
                var DataInsert = new List<Models.DB_THANHTOAN_BKN>();
                var DataUpdate = new List<Models.DB_THANHTOAN_BKN>();
                foreach (var i in data)
                {
                    var _tmp = dbkh.FirstOrDefault(d => d.ACCOUNT == i.USERNAME);
                    var pttb = dbpttb.FirstOrDefault(d => d.LOGINNAME.Trim() == i.USERNAME);
                    if (_tmp != null)
                    {
                        if (pttb != null)
                        {
                            if (!string.IsNullOrEmpty(pttb.MA_KH)) _tmp.MA_KH = pttb.MA_KH.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_TT_HNI)) _tmp.MA_TT_HNI = pttb.MA_TT_HNI.Trim();
                            if (!string.IsNullOrEmpty(pttb.FULLNAME)) _tmp.TEN_TT = pttb.FULLNAME.Trim();
                            if (!string.IsNullOrEmpty(pttb.ADDRESS1)) _tmp.DIACHI_TT = pttb.ADDRESS1.Trim();
                            if (!string.IsNullOrEmpty(pttb.MOBILE)) _tmp.DIENTHOAI = pttb.MOBILE.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_ST)) _tmp.MS_THUE = pttb.MA_ST.Trim();
                            //_tmp.BANKNUMBER = null;
                            if (!string.IsNullOrEmpty(pttb.MA_DVI)) _tmp.MA_DVI = pttb.MA_DVI.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_CBT)) _tmp.MA_CBT = pttb.MA_CBT.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_TUYENTHU)) _tmp.MA_TUYEN = pttb.MA_TUYENTHU.Trim();
                            if (!string.IsNullOrEmpty(pttb.CUSTCATE)) _tmp.CUSTCATE = pttb.CUSTCATE.Trim();
                            //_tmp.STK = null;
                            _tmp.MA_DT = pttb.MA_DT;
                            _tmp.TH_SD = pttb.STATUS;
                            _tmp.ISNULL = 0;
                            _tmp.ISNULLMT = 0;
                        }
                        else
                        {
                            _tmp.MA_DT = 1;
                            _tmp.TH_SD = 1;
                            _tmp.ISNULL = 1;
                            _tmp.ISNULLMT = 1;
                        }
                        _tmp.FIX = 0;
                        _tmp.FLAG = 1;
                        DataUpdate.Add(_tmp);
                    }
                    else
                    {
                        var _d = new Models.DB_THANHTOAN_BKN();
                        _d.ID = Guid.NewGuid();
                        _d.TYPE_BILL = i.TYPE_BILL;
                        _d.ACCOUNT = _d.MA_TB = i.USERNAME;
                        if (pttb != null)
                        {
                            if (!string.IsNullOrEmpty(pttb.MA_KH)) _d.MA_KH = pttb.MA_KH.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_TT_HNI)) _d.MA_TT_HNI = pttb.MA_TT_HNI.Trim();
                            if (!string.IsNullOrEmpty(pttb.FULLNAME)) _d.TEN_TT = pttb.FULLNAME.Trim();
                            if (!string.IsNullOrEmpty(pttb.ADDRESS1)) _d.DIACHI_TT = pttb.ADDRESS1.Trim();
                            if (!string.IsNullOrEmpty(pttb.MOBILE)) _d.DIENTHOAI = pttb.MOBILE.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_ST)) _d.MS_THUE = pttb.MA_ST.Trim();
                            //_tmp.BANKNUMBER = null;
                            if (!string.IsNullOrEmpty(pttb.MA_DVI)) _d.MA_DVI = pttb.MA_DVI.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_CBT)) _d.MA_CBT = pttb.MA_CBT.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_TUYENTHU)) _d.MA_TUYEN = pttb.MA_TUYENTHU.Trim();
                            if (!string.IsNullOrEmpty(pttb.CUSTCATE)) _d.CUSTCATE = pttb.CUSTCATE.Trim();
                            //_tmp.STK = null;
                            _d.MA_DT = pttb.MA_DT;
                            _d.TH_SD = pttb.STATUS;
                            _d.ISNULL = 0;
                            _d.ISNULLMT = 0;
                        }
                        else
                        {
                            _d.MA_DT = 1;
                            _d.TH_SD = 1;
                            _d.ISNULL = 1;
                            _d.ISNULLMT = 1;
                        }
                        _d.FIX = 0;
                        _d.FLAG = 1;
                        DataInsert.Add(_d);
                    }
                }
                //
                if (DataInsert.Count > 0) SQLServer.Connection.Insert(DataInsert);
                if (DataUpdate.Count > 0) SQLServer.Connection.Update(DataUpdate);
                //
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật: {DataUpdate.Count} - Thêm mới: {DataInsert.Count}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateContactNULL(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "8";
            try
            {
                //Get Data
                var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE ISNULL>0 AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var data = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
                //Get DB PTTB
                qry = "select tt.MA_TT as MA_TT_HNI,tb.MA_TB as LOGINNAME,tt.DIACHI_TT as ADDRESS1,tt.TEN_TT as FULLNAME,tt.DIENTHOAI_TT as MOBILE,tt.KHACHHANG_ID as MA_KH,tt.MAPHO_ID as MA_DVI,tt.MA_TUYENTHU as MA_TUYENTHU from DB_THUEBAO_BKN tb,DB_THANHTOAN_BKN tt where tb.thanhtoan_id=tt.thanhtoan_id";
                var dbpttb = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                qry = "select MA_KH,DOITUONGKH_ID as MA_DT,KHACHHANG_ID,KHACHHANG_ID as LOGINNAME from VTT.DB_KHACHHANG_BKN";
                var dbpttb_kh = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                qry = "select a.MAPHO_ID as MA_CQ,c.MA_QUANHUYEN as MA_DVI,c.VIETTAT as MA_ST from MA_PHO_BKN a,PHUONG_XA_BKN b,QUAN_HUYEN_BKN c where a.PHUONGXA_ID=b.PHUONGXA_ID and b.QUANHUYEN_ID=c.QUANHUYEN_ID";
                var dbpttb_dvi = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=1 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var dbfix = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
                foreach (var i in data)
                {
                    var db = dbpttb.FirstOrDefault(d => d.LOGINNAME == i.ACCOUNT);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        if (!string.IsNullOrEmpty(db.FULLNAME)) i.TEN_TT = db.FULLNAME.Trim();
                        if (!string.IsNullOrEmpty(db.ADDRESS1)) i.DIACHI_TT = db.ADDRESS1.Trim();
                        if (!string.IsNullOrEmpty(db.MOBILE)) i.DIENTHOAI = db.MOBILE.Trim();
                        if (!string.IsNullOrEmpty(db.MA_TUYENTHU)) i.MA_TUYEN = db.MA_TUYENTHU.Trim();
                        if (!string.IsNullOrEmpty(db.MA_DVI)) i.MA_DVI = db.MA_DVI;
                        if (!string.IsNullOrEmpty(db.MA_CBT)) i.MA_CBT = db.MA_CBT;
                        //i.MA_KH = !string.IsNullOrEmpty(db.MA_KH) ? db.MA_KH.Trim() : null;
                        if (!string.IsNullOrEmpty(db.MA_TT_HNI)) i.MA_TT_HNI = db.MA_TT_HNI.Trim();
                        //i.MA_DT = db.MA_DT == 0 ? 1 : db.MA_DT;
                        if (!string.IsNullOrEmpty(db.MA_ST)) i.MS_THUE = db.MA_ST.Trim();
                    }
                    db = dbpttb_kh.FirstOrDefault(d => d.LOGINNAME == i.MA_KH);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        if (!string.IsNullOrEmpty(db.MA_KH)) i.MA_KH = db.MA_KH.Trim();
                        i.MA_DT = db.MA_DT == 0 ? 1 : db.MA_DT;
                    }
                    db = dbpttb_dvi.FirstOrDefault(d => d.MA_CQ == i.MA_DVI);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        i.MA_DVI = db.MA_DVI;
                        if (i.MA_TUYEN == null || i.MA_TUYEN.isNumber()) i.MA_TUYEN = $"T{db.MA_ST}000";
                    }
                    //Cập nhật danh bạ Fix
                    var dbkh = dbfix.FirstOrDefault(d => d.ACCOUNT == i.ACCOUNT);
                    if (dbkh != null)
                    {
                        if (dbkh.ACCOUNT == "bcntv00067946")
                        {
                            dbkh.ACCOUNT = "bcntv00067946";
                        }
                        i.ISNULL = 3;
                        if (!string.IsNullOrEmpty(dbkh.TEN_TT)) i.TEN_TT = dbkh.TEN_TT.Trim();
                        if (!string.IsNullOrEmpty(dbkh.DIACHI_TT)) i.DIACHI_TT = dbkh.DIACHI_TT.Trim();
                        if (!string.IsNullOrEmpty(dbkh.DIENTHOAI)) i.DIENTHOAI = dbkh.DIENTHOAI.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_DVI)) i.MA_DVI = dbkh.MA_DVI.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_TUYEN)) i.MA_TUYEN = dbkh.MA_TUYEN.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_CBT)) i.MA_CBT = dbkh.MA_CBT.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_KH)) i.MA_KH = dbkh.MA_KH.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_TT_HNI)) i.MA_TT_HNI = dbkh.MA_TT_HNI.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MS_THUE)) i.MS_THUE = dbkh.MS_THUE.Trim();
                        i.MA_DT = dbkh.MA_DT;

                    }
                    //i.KHLON_ID = pttb.KHLON_ID;
                    //i.LOAIKH_ID = pttb.LOAIKH_ID;
                    //if (pttb.NGAY_DKY.Year > 1752 && pttb.NGAY_DKY.Year <= 9999)
                    //    _data.NGAY_DKY = pttb.NGAY_DKY;
                    //if (pttb.NGAY_CAT.Year > 1752 && pttb.NGAY_CAT.Year <= 9999)
                    //    _data.NGAY_CAT = pttb.NGAY_CAT;
                    //if (pttb.NGAY_HUY.Year > 1752 && pttb.NGAY_HUY.Year <= 9999)
                    //    _data.NGAY_HUY = pttb.NGAY_HUY;
                    //if (pttb.NGAY_CHUYEN.Year > 1752 && pttb.NGAY_CHUYEN.Year <= 9999)
                    //    _data.NGAY_CHUYEN = pttb.NGAY_CHUYEN;
                }
                SQLServer.Connection.Update(data);
                //Tìm và cập nhật Mã tuyến null về mặc định
                qry = $@"UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET ISNULLMT=1,MA_TUYEN=REPLACE(MA_TUYEN,'000','001') WHERE MA_TUYEN LIKE '%000' AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});
                         UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET MA_CBT=CAST(CAST(ma_dvi as varchar)+'01' as int) WHERE ISNULLMT=1 AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});
                         UPDATE a SET a.MA_TUYEN='T'+b.VIETTAT+'001' FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} a,QUAN_HUYEN_BKN b WHERE a.MA_DVI=b.MA_QUANHUYEN AND a.MA_TUYEN IS NULL AND a.FIX=0 AND a.FLAG=1 AND a.TYPE_BILL IN({TYPE_BILL});
                         UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET MA_KH=MA_TT_HNI WHERE MA_KH IS NULL AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});
                         UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET ISNULL=1,MA_CBT=CAST(CAST(ma_dvi as varchar)+'01' as int) WHERE MA_CBT is null AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});";
                //UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET ISNULL=1,MA_TUYEN=REPLACE(MA_TUYEN,'000','001') WHERE MA_TUYEN is null AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật danh bạ thành công {data.Count()} Thuê bao" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateData(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "8";
            try
            {
                var qry = $"DELETE {Common.Objects.TYPE_HD.HD_MYTV} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_MYTV} 
                         SELECT NEWID() AS ID,ID AS MYTV_ID,NEWID() AS DBKH_ID,TYPE_BILL,TIME_BILL,USERNAME AS ACCOUNT,PACKCD AS TOC_DO,1 AS TT_THANG,
                         {obj.day_in_month} AS NGAY_TB,{obj.day_in_month} AS NGAY_TB_PTTB,0 AS GOICUOCID,0 AS TH_THANG,0 AS TH_HUY,0 AS DUPECOUNT,0 AS ISDATMOI,
                         0 AS ISHUY,0 AS ISTTT,0 AS ISDATCOC,PAYTV_FEE,SUB_FEE,DISCOUNT AS GIAM_TRU,0 AS TONG_TTT,0 AS TONG_DC,0 AS TONG_IN,TOTAL_FEE AS TONG,ROUND(TOTAL_FEE*0.1,0) AS VAT,ROUND(TOTAL_FEE*1.1,0) AS TONGCONG,
                         NULL AS SIGNDATE,NULL AS REGISTDATE,USE_DATE AS NGAY_SD,SUSPENDATE AS NGAY_KHOA,RESUMEDATE AS NGAY_MO,STOP_DATE AS NGAY_KT 
                         FROM {Common.Objects.TYPE_HD.MYTV} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TOTAL_FEE>0";
                SQLServer.Connection.Query(qry);
                qry = $"UPDATE a SET a.DBKH_ID=b.ID FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.ACCOUNT=b.ACCOUNT WHERE a.TYPE_BILL=b.TYPE_BILL AND b.FIX=0 AND b.FLAG=1";
                SQLServer.Connection.Query(qry);
                //
                //qry = $@"UPDATE a set a.GOICUOCID=0,a.TT_THANG=1,a.TH_THANG=0,a.TH_HUY=0,a.DUPECOUNT=0,a.ISNULLDB=0,
                //            a.ISNULLMT=0,a.ISHUY=0,a.ISTTT=0,a.ISDATCOC=0,a.GIAM_TRU=b.DISCOUNT,a.PAYTV_FEE=b.PAYTV_FEE,
                //            a.SUB_FEE=b.SUB_FEE,a.TONG=b.TOTAL_FEE,a.VAT=ROUND(b.TOTAL_FEE*0.1,0),a.TONGCONG=ROUND(b.TOTAL_FEE*1.1,0)
                //            FROM {Common.Objects.TYPE_HD.HD_MYTV} a join {Common.Objects.TYPE_HD.MYTV} b ON a.MYTV_ID=b.ID";
                //
                ////Cập nhật VAT và Tổng cộng
                //qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0),TONGCONG=TONG+VAT;
                //        UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT";
                //SQLServer.Connection.Query(qry);
                //Fix lại thanh toán trước
                qry = $"UPDATE THANHTOANTRUOC SET THUC_TRU=0 WHERE TYPE_BILL IN({TYPE_BILL}) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyTichHop(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                //var qry = $"update tv SET tv.GOICUOCID=thdvnet.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} tv inner join (select * from DANHBA_GOICUOC_TICHHOP where goicuoc_id in (select thdv.goicuoc_id from HD_NET net,DANHBA_GOICUOC_TICHHOP thdv where net.ACCOUNT=thdv.ACCOUNT and (net.TYPE_BILL=6 or net.TYPE_BILL=9) and FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0)) thdvnet on tv.ACCOUNT=thdvnet.ACCOUNT where tv.TYPE_BILL=8 and FORMAT(tv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and thdvnet.NGAY_KT>=CAST('{obj.block_time}' as datetime)";
                //SQLServer.Connection.Query(qry);

                //qry = $"update tv SET tv.GOICUOCID=thdvnet.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} tv inner join (select * from DANHBA_GOICUOC_TICHHOP where goicuoc_id in (select thdv.goicuoc_id from HD_NET net,DANHBA_GOICUOC_TICHHOP thdv where net.ACCOUNT=thdv.ACCOUNT and (net.TYPE_BILL=6 or net.TYPE_BILL=9) and FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0)) thdvnet on tv.ACCOUNT=thdvnet.ACCOUNT where tv.TYPE_BILL=8 and FORMAT(tv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and thdvnet.NGAY_BD<CAST('{obj.block_time}' as datetime) AND thdvnet.NGAY_KT IS NULL";
                //SQLServer.Connection.Query(qry);

                var qry = $@"UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.ACCOUNT=thdv.ACCOUNT AND thdv.NGAY_KT>=CAST('{obj.block_time}' as datetime) AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0 AND thdv.FLAG=1;
                             UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.ACCOUNT=thdv.ACCOUNT AND thdv.NGAY_BD<CAST('{obj.block_time}' as datetime) AND thdv.NGAY_KT IS NULL AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0 AND thdv.FLAG=1;";
                SQLServer.Connection.Query(qry);
                //Xử lý tích hợp thêm
                qry = $"UPDATE tv SET tv.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} tv INNER JOIN DANHBA_GOICUOC_TICHHOP thdv ON tv.ACCOUNT=thdv.ACCOUNT WHERE tv.TYPE_BILL=8 AND FORMAT(tv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.DICHVUVT_ID=8 AND thdv.FIX=1";
                SQLServer.Connection.Query(qry);
                //Cập nhật giá từ bảng giá đối với thuê bao tích hợp
                qry = $@"UPDATE hd SET hd.TONG=bg.GIA+hd.PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN BGCUOC bg ON hd.GOICUOCID=bg.GOICUOCID WHERE hd.GOICUOCID>0 AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND bg.DICHVUVT_ID=8 AND bg.FLAG=1";
                SQLServer.Connection.Query(qry);
                //Cập nhật thuê bao tích hợp không tròn tháng
                //qry = $@"UPDATE a SET a.TONG=b.TOTAL_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN {Common.Objects.TYPE_HD.MYTV} b ON a.MYTV_ID=b.ID WHERE a.GOICUOCID>0 AND (a.NGAY_KHOA is not null or a.NGAY_MO is not null or a.NGAY_KT is not null) AND a.TYPE_BILL=8 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONG=PAYTV_FEE+SUB_FEE,GOICUOCID=0 WHERE GOICUOCID>0 AND (PAYTV_FEE+SUB_FEE)<TONG AND (NGAY_KHOA is not null or NGAY_MO is not null or NGAY_KT is not null) AND TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";

                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật tích hợp thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyKhuyenMai(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                var qry = "";
                //PERCENT
                qry = $"UPDATE hd SET hd.TONG=hd.TONG*((100-dc.VALUE)/100) FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.PERCENT} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                SQLServer.Connection.Query(qry);
                //MONEY
                qry = $"UPDATE hd SET hd.TONG=hd.TONG-dc.VALUE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MONEY} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                SQLServer.Connection.Query(qry);
                //FIX
                qry = $"UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                SQLServer.Connection.Query(qry);
                //Gói GD
                qry = $"UPDATE dc SET dc.FLAG=0 FROM DISCOUNT dc WHERE FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.TYPEID=5 AND dc.ACCOUNT IN(SELECT DISTINCT th.ACCOUNT FROM DANHBA_GOICUOC_TICHHOP th,BGCUOC bg WHERE th.LOAIGOICUOC_ID=bg.GOICUOCID and th.DICHVUVT_ID=8 AND FORMAT(th.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND th.NGAY_KT IS NULL)";
                SQLServer.Connection.Query(qry);
                qry = $"UPDATE hd SET hd.TONG=((hd.TONG-PAYTV_FEE)*((100-dc.VALUE)/100))+PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MYTVGD};";
                //qry += $"UPDATE hd SET hd.TONG=TONG+GIAM_TRU-PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MYTVGD};";
                //qry += $"UPDATE hd SET hd.GIAM_TRU=(dc.VALUE/100)*TONG FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MYTVGD};";
                //qry += $"UPDATE hd SET hd.TONG=TONG-GIAM_TRU+PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MYTVGD};";
                SQLServer.Connection.Query(qry);
                //UPDATE VAT
                qry = $"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //UPDATE TONGCONG
                qry = $"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật khuyến mại thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyManu(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            obj.data_id = 782;
            var FixPrice = 122727;
            try
            {
                //Tích hợp
                var qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONG={FixPrice} WHERE MA_CBT={obj.data_id} AND GOICUOCID>0 AND TYPE_BILL=9 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG/10,0) WHERE MA_CBT={obj.data_id} AND GOICUOCID>0 AND TYPE_BILL=9 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE MA_CBT={obj.data_id} AND GOICUOCID>0 AND TYPE_BILL=9 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật thuê bao Manu thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyThanhToanTruoc(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            //
            var TYPE_BILL = 9002;
            var DVVT_ID = "8";
            try
            {
                //-- Thông tin --
                //TTT Tiền thanh toán trước
                //KT Khuyến mại tặng cước (tháng)
                //CK Tiền chiết khấu

                //-- Trạng thái Flag --
                //0: Trạng thái ban đầu chưa xử lý
                //1: TTT sau khi trừ có số dư tổng > 0
                //2: TTT sau khi trừ có số dư tổng < 0
                //3: TTT sau khi trừ có số dư tổng < 0 và không có KT hoặc CK (Trả lại tiền thiếu vào hóa đơn)
                //4: KT hoặc CK sau khi trừ có số dư tổng > 0
                //5: KT hoặc CK sau khi trừ có số dư tổng < 0 (Trả lại tiền thiếu vào hóa đơn)
                //6: KT hoặc CK không còn TTT có số dư tổng > 0
                //7: KT hoặc CK không còn TTT có số dư tổng < 0 (Trả lại tiền thiếu vào hóa đơn)
                //8: KT hoặc CK khi TTT có số dư tổng > 0

                //Đặt lại đầu vào
                var qry = $@"UPDATE ttt SET ttt.FLAG=0 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                             UPDATE ttt SET ttt.FLAG=-1 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.SODU<0;";
                SQLServer.Connection.Query(qry);
                //Check Thuê bao thanh toán trước không có trong hóa đơn
                qry = $"UPDATE ttt SET ttt.FLAG=-1 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.ACCOUNT NOT IN(SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_MYTV} WHERE TYPE_BILL={DVVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}');";
                SQLServer.Connection.Query(qry);

                //Bước 1: Xử lý các thuê bao còn TTT và (KT hoặc CK)
                //Cập nhật cước từ hóa đơn
                qry = $@"UPDATE ttt SET ttt.FLAG=1,ttt.TONG=hd.TONG,ttt.EXTRA_TONG=PAYTV_FEE,SODU_TONG=ttt.SODU-(hd.TONG-hd.PAYTV_FEE) FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=0 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL={DVVT_ID} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Pay TV VTVCab 38000
                qry = $@"UPDATE ttt SET ttt.FLAG=1,ttt.TONG=hd.TONG,ttt.EXTRA_TONG=PAYTV_FEE,SODU_TONG=ttt.SODU-(hd.TONG-hd.PAYTV_FEE+38000) FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=1 AND ttt.ID_CV=203 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL={DVVT_ID} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);

                //Cập nhật thực trừ cho TTT có số dư tổng > 0
                qry = $@"UPDATE ttt SET ttt.THUC_TRU=ttt.TONG-ttt.EXTRA_TONG FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=1;";
                SQLServer.Connection.Query(qry);
                //Pay TV VTVCab 38000
                qry = $@"UPDATE ttt SET ttt.THUC_TRU=ttt.TONG-ttt.EXTRA_TONG+38000 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.ID_CV=203 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=1;";
                SQLServer.Connection.Query(qry);

                //Cập nhật thực trừ bằng số dư đối với các TTT có số dư tổng < 0
                qry = $@"UPDATE ttt SET ttt.FLAG=2,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=1 AND ttt.SODU_TONG<0;";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái cho TTT sau khi trừ có số dư tổng < 0 và không có KT hoặc CK
                qry = $@"UPDATE ttt SET ttt.FLAG=3 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=2 AND ttt.MA_TB NOT IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN!='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID={DVVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}');";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái cho KT hoặc CK khi TTT có số dư tổng < 0
                qry = $@"UPDATE ttt SET ttt.FLAG=2 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=0 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.MA_TB IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID={DVVT_ID} AND FLAG=2 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}');";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái cho KT hoặc CK khi TTT có số dư tổng > 0
                qry = $@"UPDATE ttt SET ttt.FLAG=8 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=0 AND ttt.MA_TB IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID={DVVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FLAG=1);";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái, tổng, thực trừ bằng số tiền còn thiếu cho KT hoặc CK sau khi trừ TTT và số du tổng sau khi trừ
                qry = $@"UPDATE ttt SET ttt.FLAG=4,ttt.THUC_TRU=ttt2.SODU_TONG*-1,ttt.TONG=ttt2.TONG,ttt.SODU_TONG=ttt.SODU+ttt2.SODU_TONG FROM THANHTOANTRUOC ttt,THANHTOANTRUOC ttt2 WHERE ttt.MA_TB=ttt2.MA_TB AND ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=2 AND ttt2.KHOANTIEN='TTT' AND ttt2.TYPE_BILL={TYPE_BILL} AND ttt2.DVVT_ID={DVVT_ID} AND FORMAT(ttt2.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt2.FLAG=2;";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái và thực trừ bằng số dư đối với KT hoặc CK có số dư tổng < 0
                qry = $@"UPDATE ttt SET ttt.FLAG=5,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=4 AND ttt.SODU_TONG<0;";
                SQLServer.Connection.Query(qry);

                //Bước 2: Xử lý các thuê bao không còn TTT chỉ còn KT hoặc CK
                //Cập nhật trạng thái, thực trừ, cước từ hóa đơn
                qry = $@"UPDATE ttt SET ttt.FLAG=6,ttt.TONG=hd.TONG,ttt.EXTRA_TONG=PAYTV_FEE,SODU_TONG=ttt.SODU-(hd.TONG-hd.PAYTV_FEE),ttt.THUC_TRU=hd.TONG-hd.PAYTV_FEE FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=0 AND hd.TYPE_BILL={DVVT_ID} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái, thực trừ cho KT hoặc CK có số dư tổng < 0
                qry = $@"UPDATE ttt SET ttt.FLAG=7,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=6 AND ttt.SODU_TONG<0;";
                SQLServer.Connection.Query(qry);

                //Bước 3: Cập nhật thực trừ vào hóa đơn
                //Cập nhật các thuê bao có số dư tổng > 0
                qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=hd.TONG-hd.PAYTV_FEE,hd.TONG=PAYTV_FEE FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG IN(1,4,6) AND hd.TYPE_BILL={DVVT_ID} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Pay TV VTVCab 38000
                qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.THUC_TRU,hd.TONG=PAYTV_FEE-38000 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG IN(1,4,6) AND ttt.ID_CV=203 AND hd.TYPE_BILL={DVVT_ID} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);

                //Cập nhật các thuê bao có số dư tổng < 0
                qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.TONG+SODU_TONG,hd.TONG=ttt.SODU_TONG*-1 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG IN(3,5,7) AND hd.TYPE_BILL={DVVT_ID} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Pay TV VTVCab 38000
                //qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.TONG+SODU_TONG,hd.TONG=ttt.SODU_TONG*-1 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG IN(3,5,7) AND ttt.ID_CV=203 AND hd.TYPE_BILL={DVVT_ID} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                //SQLServer.Connection.Query(qry);

                //PayTv VTV Cap
                //qry = $"UPDATE a SET a.TONG=a.TONG+b.TIENHANMUC FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN THANHTOANTRUOC b ON a.ACCOUNT=b.ACCOUNT WHERE b.KHOANTIEN='KT' AND b.TIENHANMUC=-38000 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                //SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE ISTTT=1 AND TYPE_BILL={DVVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE ISTTT=1 AND TYPE_BILL={DVVT_ID} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật thanh toán trước thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyThanhToanTruocFix(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            //
            int TYPE_BILL = 9006;
            try
            {
                //Check Thuê bao thanh toán trước không có trong hóa đơn
                var qry = $"UPDATE ttt SET ttt.FLAG=1 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.TIME_BILL>=ttt.NGAY_BD AND ttt.TIME_BILL<ttt.NGAY_KT AND ttt.ACCOUNT IN(SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_MYTV}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);

                qry = $"UPDATE ttt SET ttt.THUC_TRU=hd.TONG-hd.PAYTV_FEE FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND hd.TIME_BILL>=ttt.NGAY_BD AND hd.TIME_BILL<ttt.NGAY_KT AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry, null, null, true, 3000);
                qry = $"UPDATE hd SET hd.TONG_TTT=hd.TONG FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.TIME_BILL>=ttt.NGAY_BD AND ttt.TIME_BILL<ttt.NGAY_KT AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //qry = $"UPDATE DATCOC SET SODU=TONGHANMUC-THUC_TRU WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                // SQLServer.Connection.Query(qry);
                qry = $"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET ISTTT=1,TONG=PAYTV_FEE WHERE ACCOUNT IN(SELECT ACCOUNT FROM THANHTOANTRUOC WHERE TYPE_BILL={TYPE_BILL} AND TIME_BILL>=NGAY_BD AND TIME_BILL<NGAY_KT AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}') AND TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);

                ////PayTv VTV Cap
                //qry = $"UPDATE a SET a.TONG=a.TONG+b.TIENHANMUC FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN THANHTOANTRUOC b ON a.ACCOUNT=b.ACCOUNT WHERE b.KHOANTIEN='KT' AND b.TIENHANMUC=-38000 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                //SQLServer.Connection.Query(qry);

                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE ISTTT=1 AND TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE ISTTT=1 AND TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật thanh toán trước fix thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyDatCoc(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            //
            int TYPE_BILL = 9003;
            try
            {
                var qry = "";
                //Check Thuê bao đặt cọc không có trong hóa đơn
                //var qry = $"UPDATE THANHTOANTRUOC SET FLAG=1 WHERE TYPE_BILL={TYPE_BILL} AND ACCOUNT IN(SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_MYTV}) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //SQLServer.Connection.Query(qry);

                //Check số dư nhỏ hơn 0
                //qry = $"UPDATE THANHTOANTRUOC SET FLAG=0 WHERE TYPE_BILL={TYPE_BILL} AND SODU<1 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //SQLServer.Connection.Query(qry);
                //Đặt lại đầu vào
                qry = $"UPDATE dc SET dc.FLAG=0 FROM THANHTOANTRUOC dc WHERE dc.TYPE_BILL={TYPE_BILL} AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật cước từ hóa đơn
                qry = $"UPDATE dc SET dc.FLAG=3,dc.SODU_TONG=dc.SODU FROM THANHTOANTRUOC dc INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON dc.ACCOUNT=hd.ACCOUNT WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=0 AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.ISTTT=1 AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật cước từ hóa đơn
                qry = $"UPDATE dc SET dc.FLAG=1,dc.TONG=hd.TONGCONG,dc.EXTRA_TONG=hd.PAYTV_FEE,dc.SODU_TONG=dc.SODU-hd.TONGCONG FROM THANHTOANTRUOC dc INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON dc.ACCOUNT=hd.ACCOUNT WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=0 AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.ISTTT=0 AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật thực trừ bằng tổng
                qry = $"UPDATE dc SET dc.THUC_TRU=dc.TONG FROM THANHTOANTRUOC dc WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=1 AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật thực trưc bằng số dư khi SODU_TONG<0
                qry = $"UPDATE dc SET dc.FLAG=2,dc.THUC_TRU=dc.SODU FROM THANHTOANTRUOC dc WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=1 AND dc.SODU_TONG<0 AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại hóa đơn khi SODU_TONG>=0
                qry = $"UPDATE hd SET hd.ISDATCOC=1,hd.TONG_DC=dc.THUC_TRU,hd.TONG=0,hd.VAT=0,hd.TONGCONG=0 FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN THANHTOANTRUOC dc ON hd.ACCOUNT=dc.ACCOUNT WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=1 AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.ISTTT=0 AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại hóa đơn khi SODU_TONG<0
                qry = $"UPDATE hd SET hd.ISDATCOC=1,hd.TONG_DC=dc.THUC_TRU,hd.TONGCONG=dc.SODU_TONG*-1 FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN THANHTOANTRUOC dc ON hd.ACCOUNT=dc.ACCOUNT WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=2 AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.ISTTT=0 AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại hóa đơn TONG
                qry = $"UPDATE hd SET hd.TONG=ROUND(hd.TONGCONG/1.1,0) FROM {Common.Objects.TYPE_HD.HD_MYTV} hd WHERE hd.TYPE_BILL=8 AND hd.ISDATCOC=1 AND hd.TONG>0 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại hóa đơn VAT
                qry = $"UPDATE hd SET hd.VAT=ROUND(hd.TONG*0.1,0) FROM {Common.Objects.TYPE_HD.HD_MYTV} hd WHERE hd.TYPE_BILL=8 AND hd.ISDATCOC=1 AND hd.TONG>0 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Xử lý đặt cọc thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
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
            obj.month_year_time = (obj.month_time < 10 ? "0" + obj.month_time.ToString() : obj.month_time.ToString()) + "/" + obj.year_time;
            obj.block_time = obj.datetime.ToString("yyyy/MM") + "/16";
            obj.month_before = DateTime.Now.AddMonths(-2).ToString("yyyyMM");
            obj.time = obj.time;
            obj.ckhMerginMonth = obj.ckhMerginMonth;
            obj.file = $"TH_{obj.year_time}{obj.month_time}";
            obj.DataSource = Server.MapPath("~/" + obj.DataSource) + obj.time + "\\";
            return obj;
        }
    }
    class PayTVFee
    {
        decimal VTVCab = 38000;
    }

    //    public class MyTVController : BaseController
    //{
    //    const string _BEGIN = "BEGIN\n";
    //    const string _END = "END;";
    //    const string DataSource = Common.Directories.HDDataSourceMyTV;
    //    DataAccess.Connection.SQLServer SQLServer = new DataAccess.Connection.SQLServer();
    //    public ActionResult Index()
    //    {
    //        try
    //        {
    //            FileManagerController.InsertDirectory(Common.Directories.HDData);
    //            ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(Common.Directories.HDData).OrderByDescending(d => d).ToList();
    //        }
    //        catch (Exception ex) { this.danger(ex.Message); }
    //        return View();
    //    }
    //    public ActionResult UploadMytv(string time, string ckhMerginMonth)
    //    {
    //        try
    //        {
    //            FileManagerController.InsertDirectory(DataSource);
    //            var obj = getDefaultObj(time, string.IsNullOrEmpty(ckhMerginMonth) ? false : true, DataSource);
    //            FileManagerController.InsertDirectory(obj.DataSource, false);
    //            int uploadedCount = 0;
    //            if (Request.Files.Count > 0)
    //            {
    //                //TM.IO.FileDirectory.CreateDirectory(obj.DataSource);
    //                FileManagerController.InsertDirectory(obj.DataSource, false);
    //                var fileNameSource = new List<string>();
    //                var fileName = new List<string>();
    //                var fileSavePath = new List<string>();
    //                //Delete old File
    //                //TM.IO.Delete(obj.DataSource, TM.IO.Files(obj.DataSource));

    //                for (int i = 0; i < Request.Files.Count; i++)
    //                {
    //                    var file = Request.Files[i];
    //                    if (!file.FileName.IsExtension(".dbf"))
    //                    {
    //                        this.danger("Tệp phải định dạng .dbf");
    //                        return RedirectToAction("Index");
    //                    }

    //                    if (file.ContentLength > 0)
    //                    {
    //                        fileName.Add(System.IO.Path.GetFileName(file.FileName).ToLower());
    //                        fileSavePath.Add(obj.DataSource + fileName[i]);
    //                        file.SaveAs(fileSavePath[i]);
    //                        uploadedCount++;
    //                        FileManagerController.InsertFile(obj.DataSource + fileName[i], false);
    //                    }
    //                }
    //                var rs = "Tải lên thành công </br>";
    //                foreach (var item in fileName)
    //                    rs += item + "<br/>";
    //                this.success(rs);
    //            }
    //            else
    //                this.danger("Chưa đủ tệp!");
    //        }
    //        catch (Exception ex)
    //        {
    //            this.danger(ex.Message);
    //        }
    //        return RedirectToAction("Index");
    //    }
    //    public ActionResult MYTVUpdateBill(string time, bool ckhMerginMonth)
    //    {
    //        #region Oracle
    //        //var index = 0;
    //        //try
    //        //{
    //        //    var obj = getDefaultObj(time, ckhMerginMonth, DataSource);
    //        //    var qry = $"SELECT * FROM MYTV WHERE to_char(TIME_BILL,'mm/yyyy')='{obj.month_year_time}'";
    //        //    var data = DataAccess.OleDBF.Connection().Query<Models.MYTV>($"SELECT * FROM {obj.file_th}").ToList();
    //        //    DataAccess.OleDBF.ConnectionClose();
    //        //    //Delete MYTV
    //        //    DataAccess.Connection.Oracle.OracleCuoc.Query($"DELETE MYTV WHERE to_char(TIME_BILL,'mm/yyyy')='{obj.month_year_time}'");
    //        //    //Insert MYTV FROM TH_MYTV
    //        //    foreach (var i in data)
    //        //    {
    //        //        index++;
    //        //        i.ID = index;
    //        //        i.TYPE_BILL = 1;
    //        //        i.TIME_BILL = obj.datetime;
    //        //    }
    //        //    DataAccess.Connection.Oracle.OracleCuoc.InsertList(data);

    //        //    this.success("Cập nhật file cước thành công!");
    //        //}
    //        //catch (Exception ex) { this.danger(ex.Message + " - Index: " + index); }
    //        #endregion
    //        var index = 0;
    //        try
    //        {
    //            var obj = getDefaultObj(time, ckhMerginMonth, DataSource);
    //            var qry = $"SELECT * FROM {obj.file}";
    //            var data = DataAccess.OleDBF.Connection().Query<Models.MYTV>(qry);
    //            //Delete MYTV
    //            qry = $"DELETE MYTV WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=3";
    //            SQLServer.Connection.Query(qry);
    //            //Insert MYTV FROM TH_MYTV
    //            foreach (var i in data)
    //            {
    //                index++;
    //                i.ID = Guid.NewGuid();
    //                i.TYPE_BILL = 3;
    //                i.TIME_BILL = obj.datetime;
    //            }
    //            SQLServer.Connection.Insert(data.Trim());
    //            this.success("Cập nhật file cước thành công!");
    //        }
    //        catch (Exception ex) { this.danger(ex.Message + " - Index: " + index); }
    //        return RedirectToAction("Index");
    //    }
    //    public ActionResult MYTVUpdateContact(string time, bool ckhMerginMonth)
    //    {
    //        #region Oracle
    //        //var index = 0;
    //        //try
    //        //{
    //        //    var obj = getDefaultObj(time, ckhMerginMonth, DataSource);
    //        //    //Delete MYTV_PTTB
    //        //    var qry = "DELETE MYTV_PTTB";
    //        //    DataAccess.Connection.Oracle.OracleCuoc.Query<Models.MYTV_PTTB>(qry);
    //        //    //Get MYTV
    //        //    qry = $"SELECT * FROM MYTV WHERE TYPE_BILL=1 AND to_char(TIME_BILL,'mm/yyyy')='{obj.month_year_time}'";
    //        //    var MYTV = DataAccess.Connection.Oracle.OracleCuoc.Query<Models.MYTV>(qry);
    //        //    //Get DB PTTB
    //        //    qry = "SELECT * FROM DANH_BA_MYTV";
    //        //    var dbpttb = DataAccess.Connection.Oracle.OracleHNI.Query<Models.DANH_BA_MYTV>(qry).ToList();
    //        //    //Insert MYTV_PTTB with DB PTTB
    //        //    var data = new List<Models.MYTV_PTTB>();
    //        //    foreach (var i in MYTV)
    //        //    {
    //        //        index++;
    //        //        //
    //        //        if(index== 8569)
    //        //        {
    //        //            index = 0;
    //        //        }
    //        //        var MYTV_PTTB = new Models.MYTV_PTTB();
    //        //        MYTV_PTTB.ID = index;
    //        //        MYTV_PTTB.MYTV_ID = i.ID;
    //        //        MYTV_PTTB.GOICUOCID = 0;
    //        //        MYTV_PTTB.TH_THANG = 0;
    //        //        MYTV_PTTB.TH_SD = 0;
    //        //        MYTV_PTTB.TH_HUY = 0;
    //        //        MYTV_PTTB.CUOC_SD = i.PAYTV_FEE;
    //        //        MYTV_PTTB.CUOC_TB = i.SUB_FEE;
    //        //        MYTV_PTTB.TONG = MYTV_PTTB.CUOC_SD + MYTV_PTTB.CUOC_TB;
    //        //        MYTV_PTTB.VAT = MYTV_PTTB.TONG / 10;
    //        //        MYTV_PTTB.TONGCONG = MYTV_PTTB.TONG + MYTV_PTTB.VAT;
    //        //        MYTV_PTTB.DUPECOUNT = 0;
    //        //        MYTV_PTTB.STATUS = 0;
    //        //        MYTV_PTTB.ISDATMOI = 0;
    //        //        MYTV_PTTB.ISNULLDB = 0;
    //        //        MYTV_PTTB.ISNULLMT = 0;
    //        //        MYTV_PTTB.ISHUY = 0;
    //        //        //
    //        //        var pttb = dbpttb.FirstOrDefault(d => d.LOGINNAME.Trim() == i.USERNAME);
    //        //        if (pttb != null)
    //        //        {
    //        //            MYTV_PTTB.FULLNAME = pttb.FULLNAME;
    //        //            MYTV_PTTB.CUSTCATE = pttb.CUSTCATE;
    //        //            MYTV_PTTB.ADDRESS = pttb.ADDRESS1;
    //        //            MYTV_PTTB.MOBILE = pttb.MOBILE;
    //        //            //MYTV_PTTB.TELEPHONE = pttb.TELEPHONE;
    //        //            MYTV_PTTB.MA_DVI = pttb.MA_DVI;
    //        //            MYTV_PTTB.MA_CBT = pttb.MA_CBT;
    //        //            MYTV_PTTB.MA_TUYEN = pttb.MA_TUYENTHU;
    //        //            MYTV_PTTB.MA_KH = pttb.MA_KH;
    //        //            MYTV_PTTB.MA_TT_HNI = pttb.MA_TT_HNI;
    //        //            MYTV_PTTB.MA_DT = pttb.MA_DT;
    //        //            MYTV_PTTB.MA_ST = pttb.MA_ST;
    //        //            MYTV_PTTB.NGAY_SD = pttb.NGAY_SUDUNG;
    //        //            MYTV_PTTB.SIGNDATE = pttb.SIGNDATE;
    //        //            MYTV_PTTB.REGISTDATE = pttb.REGISTDATE;
    //        //        }
    //        //        data.Add(MYTV_PTTB);
    //        //    }
    //        //    TM.Connection.Oracle.OracleCuoc.InsertList(data);
    //        //    this.success("Cập nhật danh bạ thành công!");
    //        //}
    //        //catch (Exception ex) { this.danger(ex.Message + " - Index: " + index); }
    //        #endregion
    //        var index = 0;
    //        try
    //        {
    //            var obj = getDefaultObj(time, ckhMerginMonth, DataSource);
    //            //Delete MYTV_PTTB
    //            var qry = $"DELETE MYTV_PTTB WHERE TYPE_BILL=3 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
    //            SQLServer.Connection.Query(qry);
    //            //Get MYTV
    //            qry = $"SELECT * FROM MYTV WHERE TYPE_BILL=3 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
    //            var MYTV = SQLServer.Connection.Query<Models.MYTV>(qry);
    //            //Get DB PTTB
    //            qry = "SELECT * FROM DANH_BA_MYTV";
    //            var dbpttb = DataAccess.Connection.Oracle.OracleHNI.Query<Models.DANH_BA_MYTV>(qry).ToList();
    //            //Insert MYTV_PTTB with DB PTTB
    //            var data = new List<Models.HD_MYTV>();
    //            foreach (var i in MYTV)
    //            {
    //                index++;
    //                var _data = new Models.HD_MYTV();
    //                _data.ID = Guid.NewGuid();
    //                _data.MYTV_ID = i.ID;
    //                _data.TYPE_BILL = i.TYPE_BILL;
    //                _data.TIME_BILL = i.TIME_BILL;
    //                _data.ACCOUNT = i.USERNAME;
    //                _data.GOICUOCID = 0;
    //                _data.TH_THANG = 0;
    //                _data.TH_SD = 0;
    //                _data.TH_HUY = 0;
    //                _data.PAYTV_FEE = i.PAYTV_FEE;
    //                _data.SUB_FEE = i.SUB_FEE;
    //                _data.TONG = _data.PAYTV_FEE + _data.SUB_FEE;
    //                _data.VAT = _data.TONG / 10;
    //                _data.TONGCONG = _data.TONG + _data.VAT;
    //                _data.DUPECOUNT = 0;
    //                _data.STATUS = 0;
    //                _data.ISDATMOI = 0;
    //                _data.ISNULLDB = 0;
    //                _data.ISNULLMT = 0;
    //                _data.ISHUY = 0;
    //                //
    //                var pttb = dbpttb.FirstOrDefault(d => d.LOGINNAME.Trim() == i.USERNAME);
    //                if (pttb != null)
    //                {
    //                    _data.TEN_TT = !string.IsNullOrEmpty(pttb.FULLNAME) ? pttb.FULLNAME.Trim() : null;
    //                    _data.CUSTCATE = !string.IsNullOrEmpty(pttb.CUSTCATE) ? pttb.CUSTCATE.Trim() : null;
    //                    _data.DIACHI_TT = !string.IsNullOrEmpty(pttb.ADDRESS1) ? pttb.ADDRESS1.Trim() : null;
    //                    _data.DIENTHOAI_LH = !string.IsNullOrEmpty(pttb.MOBILE) ? pttb.MOBILE.Trim() : null;
    //                    //_data.TELEPHONE = pttb.TELEPHONE.Trim();
    //                    _data.MA_DVI = !string.IsNullOrEmpty(pttb.MA_DVI) ? pttb.MA_DVI.Trim() : null;
    //                    _data.MA_CBT = !string.IsNullOrEmpty(pttb.MA_CBT) ? pttb.MA_CBT.Trim() : null;
    //                    _data.MA_TUYEN = !string.IsNullOrEmpty(pttb.MA_TUYENTHU) ? pttb.MA_TUYENTHU.Trim() : null;
    //                    _data.MA_KH = !string.IsNullOrEmpty(pttb.MA_KH) ? pttb.MA_KH.Trim() : null;
    //                    _data.MA_TT_HNI = !string.IsNullOrEmpty(pttb.MA_TT_HNI) ? pttb.MA_TT_HNI.Trim() : null;
    //                    _data.MA_DT = pttb.MA_DT;
    //                    _data.MS_THUE = !string.IsNullOrEmpty(pttb.MA_ST) ? pttb.MA_ST.Trim() : null;
    //                    _data.NGAY_SD = pttb.NGAY_SUDUNG;
    //                    if (pttb.SIGNDATE.Year > 1752 && pttb.SIGNDATE.Year <= 9999)
    //                        _data.SIGNDATE = pttb.SIGNDATE;
    //                    if (pttb.SIGNDATE.Year > 1752 && pttb.SIGNDATE.Year <= 9999)
    //                        _data.REGISTDATE = pttb.REGISTDATE;
    //                }
    //                data.Add(_data);
    //            }
    //            SQLServer.Connection.Insert(data);
    //            this.success("Cập nhật danh bạ thành công!");
    //        }
    //        catch (Exception ex) { this.danger(ex.Message + " - Index: " + index); }
    //        return RedirectToAction("Index");
    //    }
    //    //public ActionResult MYTVUpdateContact(string time, bool ckhMerginMonth)
    //    //{

    //    //    try
    //    //    {
    //    //        //Kiểm tra tháng đầu vào
    //    //        var month_before = DateTime.Now.AddMonths(-2).ToString("yyyyMM");
    //    //        if (ckhMerginMonth)
    //    //            time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
    //    //        var file = $"mytv_{time}";
    //    //        var dbtv = $"dbtv_{time}";
    //    //        var dskh = $"dskh_{time}";
    //    //        var hdtv_before = $"hdtv{month_before}";
    //    //        TM.OleDBF.DataSource = DataSource;
    //    //        //RemoveCOLUMN
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN account_id");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN stb_serial");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN ip_user");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN ip_server");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN owe_money");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN percentage");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN total_time");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN total_flux");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN paytvmonth");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN paytv_time");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN god_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN mod_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN kod_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN vod_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN etr_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN mega");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN sto_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN spo_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN bh_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN tvs_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN reason");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN chr_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN edu_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN dtl_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN ltr_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN fiber");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN lst_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN vot_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN vctv_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN kpl_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN hbo_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN faf_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN vtv_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN rent_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN fim_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN clg_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN bhd_fee");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} DROP COLUMN cme_fee");
    //    //        //ADDCOLUMN
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN FULLNAME c(100) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN CUSTCATE c(50) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN ADDRESS c(150) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MOBILE c(50) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MA_DVI n1 NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MA_CQ c(15) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MA_DT n(2) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MA_ST c(15) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MA_CBT n(10) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MA_TUYEN c(15) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MA_TT_HNI c(15) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN MA_KH c(15) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN NGAY_SD n(2) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN SIGNDATE d(8) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN REGISTDATE d(8) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN ISDATMOI n(1) NULL");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN status n(2)");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN telephone c(10)");
    //    //        //check NULL DB PTTB
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN isnulldb n(1) NULL");
    //    //        TM.OleDBF.Execute($"UPDATE {file} SET isnulldb=0");
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN isnullmt n(1) NULL");
    //    //        TM.OleDBF.Execute($"UPDATE {file} SET isnullmt=0");
    //    //        //Update th_201708 from dbtv_201708
    //    //        TM.OleDBF.Execute($"UPDATE th SET FULLNAME=(SELECT b FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET CUSTCATE=(SELECT c FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET ADDRESS=(SELECT d FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MOBILE=(SELECT e FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_DVI=(SELECT n FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_CQ=(SELECT o FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_DT=1 FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_DT=(SELECT p FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_ST=(SELECT q FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_CBT=(SELECT val(u) FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_TUYEN=(SELECT v FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_TT_HNI=(SELECT w FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_KH=(SELECT x FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET NGAY_SD=(SELECT t FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET SIGNDATE=(SELECT g FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET REGISTDATE=(SELECT i FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET ISDATMOI=0 FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET ISDATMOI=(SELECT y FROM {dbtv} WHERE a=th.username) FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.status=1 FROM {file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.status=db.j FROM {file} AS th INNER JOIN {dbtv} AS db ON th.username=db.a");

    //    //        //UPDATE NULL DB PTTB
    //    //        TM.OleDBF.Execute($"UPDATE {file} SET isnulldb=1 WHERE FULLNAME IS NULL");
    //    //        //UPDATE Ma tuyến NULL
    //    //        TM.OleDBF.Execute($"UPDATE th SET isnullmt=1 FROM {file} AS th WHERE AT('000',ma_tuyen)!=0");
    //    //        //UPDATE Ma CBT khi mã tuyến NULL
    //    //        TM.OleDBF.Execute($"UPDATE th SET ma_cbt=VAL(TRANSFORM(ma_dvi)+'01') FROM {file} AS th WHERE isnullmt=1");
    //    //        TM.OleDBF.Execute($"UPDATE {file} SET ma_tuyen=STRTRAN(ma_tuyen,'000','001') WHERE isnullmt=1");
    //    //        //Update th_201708 from dskh_201708
    //    //        TM.OleDBF.Execute($"UPDATE th SET FULLNAME=(SELECT c FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET CUSTCATE=(SELECT d FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET ADDRESS=(SELECT e FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MOBILE=(SELECT TRANSFORM(h) FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET MA_DVI=(SELECT n FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET MA_CQ=(SELECT o FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET MA_DT=(SELECT p FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET MA_ST=(SELECT q FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET MA_CBT=(SELECT u FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET MA_TUYEN=(SELECT v FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET MA_TT_HNI=(SELECT w FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET MA_KH=(SELECT x FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET NGAY_SD=(SELECT t FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET SIGNDATE=(SELECT g FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET REGISTDATE=(SELECT i FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //TM.OleDBF.Execute($"UPDATE th SET ISDATMOI=(SELECT y FROM {dskh} WHERE a=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //Update th_201708 from hdtv201707
    //    //        TM.OleDBF.Execute($"UPDATE th SET FULLNAME=(SELECT ten_tb FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET CUSTCATE=(SELECT loai_kh FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET ADDRESS=(SELECT dia_chi FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MOBILE=(SELECT TRANSFORM(mobile) FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_DVI=(SELECT ma_dvi FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_CQ=(SELECT ma_cq FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_ST=(SELECT ma_st FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_CBT=(SELECT ma_cbt FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_TUYEN=(SELECT ma_tuyen FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_TT_HNI=(SELECT ma_tt_hni FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_KH=(SELECT ma_kh FROM {hdtv_before} WHERE account=th.username) FROM {file} AS th WHERE isnulldb=1");
    //    //        //Update ma_dt null
    //    //        TM.OleDBF.Execute($"UPDATE th SET MA_DT=1 FROM {file} AS th WHERE th.ma_dt is null");

    //    //        //Tạo tổng
    //    //        TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN cuoc_tb n(10)");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.cuoc_tb=sub_fee FROM {file} AS th");

    //    //        //TM.OleDBF.Execute($"");
    //    //        //TM.OleDBF.Execute($"");
    //    //        this.success("Cập nhật danh bạ thành công!");
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        this.danger(ex.Message);
    //    //    }
    //    //    return RedirectToAction("Index");
    //    //}
    //    //public ActionResult MYTVTichHop(string time, bool ckhMerginMonth)
    //    //{
    //    //    var obj = getDefaultObj(time, ckhMerginMonth, DataSource);
    //    //    //Create Column
    //    //    try { TM.OleDBF.Execute($"ALTER table {obj.thdv} ADD COLUMN dupecount n(2)"); } catch { }
    //    //    try { TM.OleDBF.Execute($"ALTER table {obj.thdv} ADD COLUMN ishuy n(1)"); } catch { }
    //    //    try { TM.OleDBF.Execute($"ALTER TABLE {obj.file} ADD COLUMN goicuocid n(10) NULL"); } catch { }
    //    //    try { TM.OleDBF.Execute($"ALTER TABLE {obj.file} ADD COLUMN ththang n(1) NULL"); } catch { }
    //    //    try { TM.OleDBF.Execute($"ALTER TABLE {obj.file} ADD COLUMN thsudung n(2) NULL"); } catch { }
    //    //    try { TM.OleDBF.Execute($"ALTER TABLE {obj.file} ADD COLUMN huythdv n(1) NULL"); } catch { }
    //    //    try { TM.OleDBF.Execute($"ALTER TABLE {obj.file} ADD COLUMN tong n(10)"); } catch { }
    //    //    try { TM.OleDBF.Execute($"ALTER TABLE {obj.file} ADD COLUMN vat n(10,2)"); } catch { }
    //    //    try { TM.OleDBF.Execute($"ALTER TABLE {obj.file} ADD COLUMN tongcong n(10)"); } catch { }
    //    //    try
    //    //    {
    //    //        //Delete MyTV Basic
    //    //        //SELECT * FROM mytv_201709 WHERE package='MyTV Basic' AND total_fee=0
    //    //        //SELECT * FROM mytv_201709 WHERE package='MyTV Basic' AND total_fee>0
    //    //        TM.OleDBF.Execute($"DELETE FROM {obj.file} WHERE package='MyTV Basic' AND total_fee=0");
    //    //        TM.OleDBF.Execute($"PACK {obj.file}");

    //    //        //Xử lý file tích hợp
    //    //        //Cập nhật lấy account cho thuê bao megavnn
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET n=f WHERE EMPTY(n)");
    //    //        //Tìm account trùng
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET dupecount=0");
    //    //        TM.OleDBF.Execute($"UPDATE thdv SET dupecount=(SELECT COUNT(*) FROM {obj.thdv} WHERE thdv.n=n) FROM {obj.thdv} thdv");

    //    //        //Tìm tb hủy tích hợp
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET ishuy=0");
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET ishuy=1 WHERE !EMPTY(h)");

    //    //        //Xử lý tb trùng và hủy tích hợp
    //    //        TM.OleDBF.Execute($"DELETE FROM {obj.thdv} WHERE dupecount>1 AND ishuy>0");
    //    //        TM.OleDBF.Execute($"PACK {obj.thdv}");

    //    //        //Tìm thuê bao tích hợp trong tháng
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET ththang=0");
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET ththang=1 WHERE YEAR(g)={obj.year_time} AND MONTH(g)={obj.month_time}");

    //    //        //Tính số ngày tích hợp
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET thsudung=0");
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET thsudung={DateTime.DaysInMonth(obj.year_time, obj.month_time)} WHERE ththang=0");
    //    //        TM.OleDBF.Execute($"UPDATE {obj.thdv} SET thsudung={DateTime.DaysInMonth(obj.year_time, obj.month_time)}-DAY(g) WHERE ththang=1");

    //    //        //Tính tích hợp
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.goicuocid=0 FROM {obj.file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.ththang=thdv.ththang FROM {obj.file} AS th JOIN {obj.thdv} AS thdv ON th.username=thdv.n");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.thsudung=thdv.thsudung FROM {obj.file} AS th JOIN {obj.thdv} AS thdv ON th.username=thdv.n");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.goicuocid=thdv.e FROM {obj.file} AS th JOIN {obj.thdv} AS thdv ON th.username=thdv.n");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.cuoc_tb=bg.gia FROM {obj.file} AS th JOIN banggia AS bg ON th.goicuocid=bg.goicuocid WHERE th.goicuocid>0 AND bg.kieu='MyTV'");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.cuoc_tb=sub_fee FROM {obj.file} AS th WHERE thsudung<15");

    //    //        //Tìm thuê bao hủy tích hợp
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.huythdv=0 FROM {obj.file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.huythdv=1 FROM {obj.file} AS th JOIN {obj.thdv} AS thdv ON th.username=thdv.n WHERE !EMPTY(thdv.h)");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.cuoc_tb=th.sub_fee FROM {obj.file} AS th WHERE th.huythdv=1");

    //    //        //Kiểm tra và xử lý hủy, tạm ngưng trong tháng
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.cuoc_tb=sub_fee FROM {obj.file} AS th WHERE th.status=0 OR th.status=23");

    //    //        //Plus
    //    //        //Xử lý M
    //    //        TM.OleDBF.Execute($"UPDATE tv SET cuoc_tb=bg.gia FROM {obj.file} AS tv JOIN banggia AS bg ON tv.goicuocid=bg.goicuocid INNER JOIN m ON tv.username=m.d WHERE tv.thsudung<15 AND tv.goicuocid>0 AND bg.kieu='MyTV'");
    //    //        //Xử lý FM
    //    //        TM.OleDBF.Execute($"UPDATE tv SET cuoc_tb=bg.gia FROM {obj.file} AS tv JOIN banggia AS bg ON tv.goicuocid=bg.goicuocid INNER JOIN fm ON tv.username=fm.d WHERE thsudung<15 AND tv.goicuocid>0 AND bg.kieu='MyTV'");
    //    //        //MYTV gia đình
    //    //        TM.OleDBF.Execute($"UPDATE tv SET cuoc_tb=sub_fee FROM {obj.file} AS tv JOIN {obj.mytvgd} AS gd ON tv.username=gd.d");

    //    //        //Tính khuyến mại
    //    //        //UPDATE th SET th.cuoc_tb=th.cuoc_tb-((km.dc/100)*th.cuoc_tb) FROM th_201708 AS th JOIN khmai0717 AS km ON th.username=km.b WHERE km.dc_type=2
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.cuoc_tb=th.cuoc_tb*((100-km.f)/100) FROM {obj.file} AS th JOIN {obj.khuyenmai} AS km ON th.username=km.b WHERE km.g=1 AND ((km.d<=CTOD('{obj.datetime.ToShortDateString()}') AND km.e>=CTOD('{obj.datetime.ToShortDateString()}')) OR EMPTY(km.e))");

    //    //        //Tính tổng pay, vat, tổng cộng
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.tong=cuoc_tb+paytv_fee FROM {obj.file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.vat=round(tong/10,0) FROM {obj.file} AS th");
    //    //        TM.OleDBF.Execute($"UPDATE th SET th.tongcong=tong+th.vat FROM {obj.file} AS th");
    //    //        this.success("Xử lý tích hợp thành công!");
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        this.danger(ex.Message);
    //    //    }
    //    //    //Rename username to account
    //    //    try { TM.OleDBF.Execute($"ALTER TABLE {obj.file} RENAME COLUMN username TO account"); } catch { }
    //    //    return RedirectToAction("Index");
    //    //}
    //    //public ActionResult MYTVKhuyenMai()
    //    //{
    //    //    try
    //    //    {

    //    //        this.success("Xử lý khuyến mại thành công!");
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        this.danger(ex.Message);
    //    //    }
    //    //    return RedirectToAction("Index");
    //    //}
    //    //public ActionResult MYTVCalculate()
    //    //{
    //    //    try
    //    //    {
    //    //        this.success("Tính cước thành công!");
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        this.danger(ex.Message);
    //    //    }
    //    //    return RedirectToAction("Index");
    //    //}
    //    //public ActionResult UpdateContact()
    //    //{
    //    //    try
    //    //    {
    //    //        //Update địa chỉ
    //    //        var dia_chi = TM.Connection.Oracle.OracleHNI.Query<Models.DIA_CHI>("SELECT * FROM DIA_CHI").ToList();
    //    //        //
    //    //        var con = TM.Connection.Oracle.OracleCuoc;
    //    //        var index = 0;
    //    //        var qry = $"{_BEGIN}DELETE DIA_CHI;\n{_END}";
    //    //        con.Query(qry);
    //    //        qry = _BEGIN;
    //    //        foreach (var i in dia_chi)
    //    //        {
    //    //            index++;
    //    //            qry += $"INSERT INTO DIA_CHI VALUES({index},{i.DUONGPHO_ID},{i.MAPHO_ID},{i.PHUONGXA_ID},{i.QUANHUYEN_ID},'{i.TEN_DUONGPHO}','{i.DAUTEN}','{i.TEN_PHUONGXA}','{i.TEN_QUANHUYEN}');\n";
    //    //            if (index % 500 == 0)
    //    //            {
    //    //                con.Execute(qry + _END);
    //    //                qry = _BEGIN;
    //    //            }
    //    //        }
    //    //        TM.Connection.Oracle.OracleCuoc.Execute(qry + _END);
    //    //        index = 0;
    //    //        //DB_THUEBAO_DIACHI_BKN
    //    //        //var DB_THUEBAO_DIACHI_BKN = TM.Connection.Oracle.OracleHNI.Query<Models.DB_THUEBAO_DIACHI_BKN>("SELECT * FROM DB_THUEBAO_DIACHI_BKN").ToList();
    //    //        //qry = _BEGIN;
    //    //        //foreach (var i in DB_THUEBAO_DIACHI_BKN)
    //    //        //{
    //    //        //    index++;
    //    //        //    qry += $"INSERT INTO DB_THUEBAO_DIACHI_BKN VALUES({index},{i.DUONGPHO_ID},{i.MAPHO_ID},{i.PHUONGXA_ID},{i.QUANHUYEN_ID},'{i.TEN_DUONGPHO}','{i.DAUTEN}','{i.TEN_PHUONGXA}','{i.TEN_QUANHUYEN}');\n";
    //    //        //    if (index % 500 == 0)
    //    //        //    {
    //    //        //        con.Execute(qry + _END);
    //    //        //        qry = _BEGIN;
    //    //        //    }
    //    //        //}
    //    //        //TM.Connection.Oracle.OracleCuoc.Execute(qry + _END);
    //    //        //index = 0;
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        this.danger(ex.Message);
    //    //    }
    //    //    return RedirectToAction("Index");
    //    //}
    //    Common.DefaultObj getDefaultObj(string time, bool ckhMerginMonth, string DataSource)
    //    {
    //        //Kiểm tra tháng đầu vào
    //        var rs = new Common.DefaultObj();

    //        if (ckhMerginMonth)
    //        {
    //            time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
    //            rs.year_time = int.Parse(time.Substring(0, 4));
    //            rs.month_time = int.Parse(time.Substring(4, 2));
    //        }

    //        rs.month_time = int.Parse(time.Substring(4, 2));
    //        rs.year_time = int.Parse(time.Substring(0, 4));
    //        rs.month_year_time = (rs.month_time < 10 ? "0" + rs.month_time.ToString() : rs.month_time.ToString()) + "/" + rs.year_time;
    //        rs.datetime = new DateTime(rs.year_time, rs.month_time, 1);

    //        rs.month_before = DateTime.Now.AddMonths(-2).ToString("yyyyMM");
    //        rs.time = time;
    //        rs.ckhMerginMonth = ckhMerginMonth;
    //        //rs.file = $"mytv_{time}";
    //        DataAccess.OleDBF.DataSource = rs.DataSource = Server.MapPath("~/" + DataSource) + time + "\\";
    //        return rs;
    //    }
    //}
}