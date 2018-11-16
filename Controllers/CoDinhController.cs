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
    public class CoDinhController : BaseController
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
        public JsonResult UpdateContact(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "1";
            try
            {
                var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.CD} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                var data = SQLServer.Connection.Query<Models.CD>(qry);
                //Get DB PTTB
                qry = "SELECT a.*,a.TRANGTHAI_ AS TRANGTHAI FROM DANH_BA_CO_DINH a";
                var dbpttb = Oracle.Connection.Query<Models.DANH_BA_CO_DINH>(qry).ToList();
                //
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var dbkh = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry);
                var DataInsert = new List<Models.DB_THANHTOAN_BKN>();
                var DataUpdate = new List<Models.DB_THANHTOAN_BKN>();
                foreach (var i in data)
                {
                    var _tmp = dbkh.FirstOrDefault(d => d.ACCOUNT == i.SO_TB);
                    var pttb = dbpttb.FirstOrDefault(d => d.MA_TB.Trim() == i.SO_TB);
                    if (_tmp != null)
                    {
                        if (pttb != null)
                        {
                            if (!string.IsNullOrEmpty(pttb.MA_KH)) _tmp.MA_KH = pttb.MA_KH.Trim();
                        }
                        _tmp.MA_TT_HNI = i.MA_TT;
                        _tmp.TEN_TT = i.TEN_TT;
                        _tmp.DIACHI_TT = i.DIACHI_TT;
                        _tmp.DIENTHOAI = i.SO_TB;
                        _tmp.MS_THUE = i.MS_THUE;
                        _tmp.BANKNUMBER = i.BANKNUMBER;
                        _tmp.MA_DVI = i.MA_DVI;
                        _tmp.MA_CBT = i.MA_CBT;
                        _tmp.MA_TUYEN = i.MA_TUYEN;
                        //_tmp.CUSTCATE = i.CUSTCATE;
                        //_tmp.STK = i.STK;
                        _tmp.MA_DT = (!string.IsNullOrEmpty(i.MA_DT) ? Int32.Parse(i.MA_DT) : 1);
                        _tmp.TH_SD = 1;
                        _tmp.ISNULL = 0;
                        _tmp.ISNULLMT = 0;
                        _tmp.FIX = 0;
                        _tmp.FLAG = 1;
                        DataUpdate.Add(_tmp);
                    }
                    else
                    {
                        var _d = new Models.DB_THANHTOAN_BKN();
                        _d.ID = Guid.NewGuid();
                        _d.TYPE_BILL = i.TYPE_BILL;
                        _d.ACCOUNT = _d.MA_TB = i.SO_TB;
                        if (pttb != null)
                        {
                            if (!string.IsNullOrEmpty(pttb.MA_KH)) _d.MA_KH = pttb.MA_KH.Trim();
                        }
                        _d.MA_TT_HNI = i.MA_TT;
                        _d.TEN_TT = i.TEN_TT;
                        _d.DIACHI_TT = i.DIACHI_TT;
                        _d.DIENTHOAI = i.SO_TB;
                        _d.MS_THUE = i.MS_THUE;
                        _d.BANKNUMBER = i.BANKNUMBER;
                        _d.MA_DVI = i.MA_DVI;
                        _d.MA_CBT = i.MA_CBT;
                        _d.MA_TUYEN = i.MA_TUYEN;
                        //_d.CUSTCATE = i.CUSTCATE;
                        //_d.STK = i.STK;
                        _d.MA_DT = (!string.IsNullOrEmpty(i.MA_DT) ? Int32.Parse(i.MA_DT) : 1);
                        _d.TH_SD = 1;
                        _d.ISNULL = 0;
                        _d.ISNULLMT = 0;
                        _d.FIX = 0;
                        _d.FLAG = 1;
                        DataInsert.Add(_d);
                    }
                }
                //
                if (DataInsert.Count > 0) SQLServer.Connection.Insert(DataInsert);
                if (DataUpdate.Count > 0) SQLServer.Connection.Update(DataUpdate);
                //
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_CD} - Cập nhật: {DataUpdate.Count} - Thêm mới: {DataInsert.Count}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult UpdateContactNULL(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDDataSource;
        //    obj = getDefaultObj(obj);
        //    var TYPE_BILL = "8";
        //    try
        //    {
        //        //Get Data
        //        var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE ISNULL>0 AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
        //        var data = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
        //        //Get DB PTTB
        //        qry = "select tt.MA_TT as MA_TT_HNI,tb.MA_TB as LOGINNAME,tt.DIACHI_TT as ADDRESS1,tt.TEN_TT as FULLNAME,tt.DIENTHOAI_TT as MOBILE,tt.KHACHHANG_ID as MA_KH,tt.MAPHO_ID as MA_DVI,tt.MA_TUYENTHU as MA_TUYENTHU from DB_THUEBAO_BKN tb,DB_THANHTOAN_BKN tt where tb.thanhtoan_id=tt.thanhtoan_id";
        //        var dbpttb = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
        //        qry = "select MA_KH,DOITUONGKH_ID as MA_DT,KHACHHANG_ID,KHACHHANG_ID as LOGINNAME from VTT.DB_KHACHHANG_BKN";
        //        var dbpttb_kh = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
        //        qry = "select a.MAPHO_ID as MA_CQ,c.MA_QUANHUYEN as MA_DVI,c.VIETTAT as MA_ST from MA_PHO_BKN a,PHUONG_XA_BKN b,QUAN_HUYEN_BKN c where a.PHUONGXA_ID=b.PHUONGXA_ID and b.QUANHUYEN_ID=c.QUANHUYEN_ID";
        //        var dbpttb_dvi = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
        //        qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=1 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
        //        var dbfix = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
        //        foreach (var i in data)
        //        {
        //            var db = dbpttb.FirstOrDefault(d => d.LOGINNAME == i.ACCOUNT);
        //            if (db != null)
        //            {
        //                i.ISNULL = 2;
        //                if (!string.IsNullOrEmpty(db.FULLNAME)) i.TEN_TT = db.FULLNAME.Trim();
        //                if (!string.IsNullOrEmpty(db.ADDRESS1)) i.DIACHI_TT = db.ADDRESS1.Trim();
        //                if (!string.IsNullOrEmpty(db.MOBILE)) i.DIENTHOAI = db.MOBILE.Trim();
        //                if (!string.IsNullOrEmpty(db.MA_TUYENTHU)) i.MA_TUYEN = db.MA_TUYENTHU.Trim();
        //                if (!string.IsNullOrEmpty(db.MA_DVI)) i.MA_DVI = db.MA_DVI;
        //                if (!string.IsNullOrEmpty(db.MA_CBT)) i.MA_CBT = db.MA_CBT;
        //                //i.MA_KH = !string.IsNullOrEmpty(db.MA_KH) ? db.MA_KH.Trim() : null;
        //                if (!string.IsNullOrEmpty(db.MA_TT_HNI)) i.MA_TT_HNI = db.MA_TT_HNI.Trim();
        //                //i.MA_DT = db.MA_DT == 0 ? 1 : db.MA_DT;
        //                if (!string.IsNullOrEmpty(db.MA_ST)) i.MS_THUE = db.MA_ST.Trim();
        //            }
        //            db = dbpttb_kh.FirstOrDefault(d => d.LOGINNAME == i.MA_KH);
        //            if (db != null)
        //            {
        //                i.ISNULL = 2;
        //                if (!string.IsNullOrEmpty(db.MA_KH)) i.MA_KH = db.MA_KH.Trim();
        //                i.MA_DT = db.MA_DT == 0 ? 1 : db.MA_DT;
        //            }
        //            db = dbpttb_dvi.FirstOrDefault(d => d.MA_CQ == i.MA_DVI);
        //            if (db != null)
        //            {
        //                i.ISNULL = 2;
        //                i.MA_DVI = db.MA_DVI;
        //                if (i.MA_TUYEN == null || i.MA_TUYEN.isNumber()) i.MA_TUYEN = $"T{db.MA_ST}000";
        //            }
        //            //Cập nhật danh bạ Fix
        //            var dbkh = dbfix.FirstOrDefault(d => d.ACCOUNT == i.ACCOUNT);
        //            if (dbkh != null)
        //            {
        //                i.ISNULL = 3;
        //                if (!string.IsNullOrEmpty(dbkh.TEN_TT)) i.TEN_TT = dbkh.TEN_TT.Trim();
        //                if (!string.IsNullOrEmpty(dbkh.DIACHI_TT)) i.DIACHI_TT = dbkh.DIACHI_TT.Trim();
        //                if (!string.IsNullOrEmpty(dbkh.DIENTHOAI)) i.DIENTHOAI = dbkh.DIENTHOAI.Trim();
        //                if (!string.IsNullOrEmpty(dbkh.MA_DVI)) i.MA_DVI = dbkh.MA_DVI.Trim();
        //                if (!string.IsNullOrEmpty(dbkh.MA_TUYEN)) i.MA_TUYEN = dbkh.MA_TUYEN.Trim();
        //                if (!string.IsNullOrEmpty(dbkh.MA_CBT)) i.MA_CBT = dbkh.MA_CBT.Trim();
        //                if (!string.IsNullOrEmpty(dbkh.MA_KH)) i.MA_KH = dbkh.MA_KH.Trim();
        //                if (!string.IsNullOrEmpty(dbkh.MA_TT_HNI)) i.MA_TT_HNI = dbkh.MA_TT_HNI.Trim();
        //                if (!string.IsNullOrEmpty(dbkh.MS_THUE)) i.MS_THUE = dbkh.MS_THUE.Trim();
        //                i.MA_DT = dbkh.MA_DT;
        //            }
        //            //i.KHLON_ID = pttb.KHLON_ID;
        //            //i.LOAIKH_ID = pttb.LOAIKH_ID;
        //            //if (pttb.NGAY_DKY.Year > 1752 && pttb.NGAY_DKY.Year <= 9999)
        //            //    _data.NGAY_DKY = pttb.NGAY_DKY;
        //            //if (pttb.NGAY_CAT.Year > 1752 && pttb.NGAY_CAT.Year <= 9999)
        //            //    _data.NGAY_CAT = pttb.NGAY_CAT;
        //            //if (pttb.NGAY_HUY.Year > 1752 && pttb.NGAY_HUY.Year <= 9999)
        //            //    _data.NGAY_HUY = pttb.NGAY_HUY;
        //            //if (pttb.NGAY_CHUYEN.Year > 1752 && pttb.NGAY_CHUYEN.Year <= 9999)
        //            //    _data.NGAY_CHUYEN = pttb.NGAY_CHUYEN;
        //        }
        //        SQLServer.Connection.Update(data);
        //        //Tìm và cập nhật Mã tuyến null về mặc định
        //        qry = $@"UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET ISNULLMT=1 WHERE MA_TUYEN LIKE '%000' AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});
        //                 UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET MA_CBT=CAST(CAST(ma_dvi as varchar)+'01' as int),MA_TUYEN=REPLACE(MA_TUYEN,'000','001') WHERE ISNULLMT>0 AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})
        //                 UPDATE a SET a.MA_TUYEN='T'+b.VIETTAT+'001' FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} a,QUAN_HUYEN_BKN b WHERE a.MA_DVI=b.MA_QUANHUYEN AND a.MA_TUYEN IS NULL AND a.FIX=0 AND a.FLAG=1 AND a.TYPE_BILL IN({TYPE_BILL});
        //                 UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET MA_KH=MA_TT_HNI WHERE MA_KH IS NULL AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
        //        SQLServer.Connection.Query(qry);
        //        return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật danh bạ thành công {data.Count()} Thuê bao" }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //    finally
        //    {
        //        SQLServer.Close();
        //        Oracle.Close();
        //    }
        //}
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateData(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                var qry = $"DELETE {Common.Objects.TYPE_HD.HD_CD} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_CD} 
                         SELECT NEWID() AS ID,ID AS CD_ID,NEWID() AS DBKH_ID,TYPE_BILL,TIME_BILL,APP_ID,SO_TB,KIEU,TB_DAUCUOI,CUOC_TB,CUOC_DV,CUOC_NH,CUOC_NT,CUOC_PSTN,
                         CUOC_V171,CUOC_VDNK,CUOC_DD,CUOC_IDD,CUOC_V171Q,CUOC_VDNKQ,CUOC_CI,CUOC_KM,CHIET_KHAU,0 AS TONG_IN,TONG,VAT,TONGCONG,DUPE_FLAG
                         FROM {Common.Objects.TYPE_HD.CD} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TONGCONG>0";
                SQLServer.Connection.Query(qry);
                qry = $"UPDATE a SET a.DBKH_ID=b.ID FROM {Common.Objects.TYPE_HD.HD_CD} a INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.SO_TB=b.ACCOUNT WHERE a.TYPE_BILL=b.TYPE_BILL AND b.FIX=0 AND b.FLAG=1";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_CD} - Cập nhật dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
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
}