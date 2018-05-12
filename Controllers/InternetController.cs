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
    public class InternetController : BaseController
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
            var TYPE_BILL = "6,9,9693,9694";
            try
            {
                #region old
                ////Get Data
                //var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TIEN>0";
                //var NET = SQLServer.Connection.Query<Models.NET>(qry);
                ////Get DB PTTB
                //qry = "SELECT * FROM DANH_BA_INTERNET";//WHERE LOAITB_ID!=6
                //var dbpttb = Oracle.Connection.Query<Models.DANH_BA_INTERNET>(qry).ToList();
                ////Insert MYTV_PTTB with DB PTTB
                //var data = new List<Models.HD_NET>();
                //foreach (var i in NET)
                //{
                //    index++;
                //    var _data = new Models.HD_NET();
                //    _data.ID = Guid.NewGuid();
                //    _data.NET_ID = i.ID;
                //    _data.TYPE_BILL = i.TYPE_BILL;
                //    _data.TIME_BILL = i.TIME_BILL;
                //    _data.ACCOUNT = i.CALLING;
                //    _data.TOC_DO = i.TOC_DO;
                //    _data.TH_SD = 1;
                //    _data.NGAY_SD = i.NGAY_SD;
                //    _data.NGAY_KHOA = i.NGAY_KHOA;
                //    _data.NGAY_MO = i.NGAY_MO;
                //    _data.NGAY_KT = i.NGAY_KT;
                //    //
                //    UpdateContactTB(_data, dbpttb, i.TYPE_BILL);
                //    data.Add(_data);
                //}
                ////Delete Data
                //qry = $"DELETE {Common.Objects.TYPE_HD.HD_NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND (TYPE_BILL=6 OR TYPE_BILL=9 OR TYPE_BILL=9693)";
                //SQLServer.Connection.Query(qry);
                ////
                //SQLServer.Connection.Insert(data);
                #endregion
                var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                var NET = SQLServer.Connection.Query<Models.NET>(qry);
                //Get DB PTTB
                qry = "SELECT * FROM DANH_BA_INTERNET ORDER BY ACCOUNT ASC,TRANGTHAI_ DESC";//WHERE LOAITB_ID!=6
                var dbpttb = Oracle.Connection.Query<Models.DANH_BA_INTERNET>(qry).ToList();
                //
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var dbkh = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry);
                var DataInsert = new List<Models.DB_THANHTOAN_BKN>();
                var DataUpdate = new List<Models.DB_THANHTOAN_BKN>();
                foreach (var i in NET)
                {
                    index++;
                    var _tmp = dbkh.FirstOrDefault(d => d.ACCOUNT == i.CALLING);
                    var pttb = dbpttb.FirstOrDefault(d => d.ACCOUNT.Trim() == i.CALLING);
                    if (!DataInsert.Any(d => d.ACCOUNT == i.CALLING))
                        if (_tmp != null)
                        {
                            UpdateContactTB(_tmp, dbpttb, i.TYPE_BILL);
                            DataUpdate.Add(_tmp);
                        }
                        else
                        {
                            var _data = new Models.DB_THANHTOAN_BKN();
                            _data.ID = Guid.NewGuid();
                            _data.TYPE_BILL = i.TYPE_BILL;
                            _data.ACCOUNT = i.CALLING;
                            UpdateContactTB(_data, dbpttb, i.TYPE_BILL);
                            DataInsert.Add(_data);
                        }
                }
                //
                if (DataInsert.Count > 0) SQLServer.Connection.Insert(DataInsert);
                if (DataUpdate.Count > 0) SQLServer.Connection.Update(DataUpdate);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật: {DataUpdate.Count} - Thêm mới: {DataInsert.Count}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
        }
        private Models.DB_THANHTOAN_BKN UpdateContactTB(Models.DB_THANHTOAN_BKN _data, List<Models.DANH_BA_INTERNET> db, int dvvt)
        {
            try
            {
                dvvt = dvvt == 9693 ? 9 : dvvt;
                var pttb = db.FirstOrDefault(d => d.ACCOUNT.Trim() == _data.ACCOUNT && d.LOAITB_ID == dvvt);
                if (pttb != null)
                {
                    if (!string.IsNullOrEmpty(pttb.MA_TB)) _data.MA_TB = pttb.MA_TB;
                    if (!string.IsNullOrEmpty(pttb.TEN_TT)) _data.TEN_TT = pttb.TEN_TT.Trim();
                    //_data.CUSTCATE = !string.IsNullOrEmpty(pttb.CUSTCATE) ? pttb.CUSTCATE.Trim() : null;
                    if (!string.IsNullOrEmpty(pttb.DIACHI_TT)) _data.DIACHI_TT = pttb.DIACHI_TT.Trim();
                    if (!string.IsNullOrEmpty(pttb.DIENTHOAI_LH)) _data.DIENTHOAI = pttb.DIENTHOAI_LH.Trim();
                    //_data.TELEPHONE = pttb.TELEPHONE.Trim();
                    if (!string.IsNullOrEmpty(pttb.DVQL_ID)) _data.MA_DVI = pttb.DVQL_ID.Trim();
                    if (!string.IsNullOrEmpty(pttb.MA_CBT)) _data.MA_CBT = pttb.MA_CBT.Trim();
                    if (!string.IsNullOrEmpty(pttb.MA_TUYENTHU)) _data.MA_TUYEN = pttb.MA_TUYENTHU.Trim();
                    if (!string.IsNullOrEmpty(pttb.MA_KH)) _data.MA_KH = pttb.MA_KH.Trim();
                    if (!string.IsNullOrEmpty(pttb.MA_TT_HNI)) _data.MA_TT_HNI = pttb.MA_TT_HNI.Trim();
                    if (!string.IsNullOrEmpty(pttb.MS_THUE)) _data.MS_THUE = pttb.MS_THUE.Trim();
                    _data.MA_DT = pttb.MA_DT;
                    _data.KHLON_ID = pttb.KHLON_ID;
                    _data.LOAIKH_ID = pttb.LOAIKH_ID;
                    _data.TH_SD = pttb.TRANGTHAI_;
                    _data.ISNULL = 0;
                    _data.ISNULLMT = 0;
                }
                else
                {
                    _data.MA_DT = 1;
                    _data.TH_SD = 1;
                    _data.ISNULL = 1;
                    _data.ISNULLMT = 1;
                }
                _data.FIX = 0;
                _data.FLAG = 1;
                return _data;
            }
            catch (Exception) { throw; }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateContactNULL(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "6,9,9693,9694";
            try
            {
                //Get Data
                var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE ISNULL>0 AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var data = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
                //Get DB PTTB
                qry = "select tt.MA_TT as MA_TT_HNI,ftth.ACCOUNT as ACCOUNT,tb.MA_TB as MA_TB,tt.DIACHI_TT as DIACHI_TT,tt.TEN_TT as TEN_TT,tt.DIENTHOAI_TT as DIENTHOAI_LH,tt.KHACHHANG_ID as MA_KH,tt.MAPHO_ID as DVQL_ID,tt.MA_TUYENTHU as MA_TUYENTHU from DB_THUEBAO_BKN tb,DB_THANHTOAN_BKN tt,DB_FTTH_BKN ftth where tb.thanhtoan_id=tt.thanhtoan_id and tb.MA_TB=ftth.SO_FTTH";
                var dbftth = Oracle.Connection.Query<Models.DANH_BA_INTERNET>(qry).ToList();
                qry = "select tt.MA_TT as MA_TT_HNI,tb.MA_TB as ACCOUNT,tb.MA_TB as MA_TB,tt.DIACHI_TT as DIACHI_TT,tt.TEN_TT as TEN_TT,tt.DIENTHOAI_TT as DIENTHOAI_LH,tt.KHACHHANG_ID as MA_KH,tt.MAPHO_ID as DVQL_ID,tt.MA_TUYENTHU as MA_TUYENTHU from DB_THUEBAO_BKN tb,DB_THANHTOAN_BKN tt where tb.thanhtoan_id=tt.thanhtoan_id";
                var dbdsl = Oracle.Connection.Query<Models.DANH_BA_INTERNET>(qry).ToList();
                qry = "select MA_KH,DOITUONGKH_ID as KHACHHANG_ID,KHACHHANG_ID as ACCOUNT from VTT.DB_KHACHHANG_BKN";
                var dbpttb_kh = Oracle.Connection.Query<Models.DANH_BA_INTERNET>(qry).ToList();
                qry = "select a.MAPHO_ID as ACCOUNT,c.MA_QUANHUYEN as DVQL_ID,c.VIETTAT as MS_THUE from MA_PHO_BKN a,PHUONG_XA_BKN b,QUAN_HUYEN_BKN c where a.PHUONGXA_ID=b.PHUONGXA_ID and b.QUANHUYEN_ID=c.QUANHUYEN_ID";
                var dbpttb_dvi = Oracle.Connection.Query<Models.DANH_BA_INTERNET>(qry).ToList();
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=1 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var dbfix = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();

                foreach (var i in data)
                {
                    var db = dbftth.FirstOrDefault(d => d.ACCOUNT.Trim() == i.ACCOUNT);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        i.MA_TB = db.MA_TB;
                        if (!string.IsNullOrEmpty(db.TEN_TT)) i.TEN_TT = db.TEN_TT.Trim();
                        if (!string.IsNullOrEmpty(db.DIACHI_TT)) i.DIACHI_TT = db.DIACHI_TT.Trim();
                        if (!string.IsNullOrEmpty(db.DIENTHOAI_LH)) i.DIENTHOAI = db.DIENTHOAI_LH.Trim();
                        if (!string.IsNullOrEmpty(db.MA_TUYENTHU)) i.MA_TUYEN = db.MA_TUYENTHU.Trim();
                        if (!string.IsNullOrEmpty(db.DVQL_ID)) i.MA_DVI = db.DVQL_ID;
                        if (!string.IsNullOrEmpty(db.MA_CBT)) i.MA_CBT = db.MA_CBT;
                        //i.MA_KH = !string.IsNullOrEmpty(db.MA_KH) ? db.MA_KH.Trim() : null;
                        if (!string.IsNullOrEmpty(db.MA_TT_HNI)) i.MA_TT_HNI = db.MA_TT_HNI.Trim();
                        //i.MA_DT = db.MA_DT;
                        if (!string.IsNullOrEmpty(db.MS_THUE)) i.MS_THUE = db.MS_THUE.Trim();
                    }
                    else
                    {
                        db = dbdsl.FirstOrDefault(d => d.ACCOUNT.Trim() == i.ACCOUNT);
                        if (db != null)
                        {
                            i.ISNULL = 2;
                            i.MA_TB = db.MA_TB;
                            if (!string.IsNullOrEmpty(db.TEN_TT)) i.TEN_TT = db.TEN_TT.Trim();
                            if (!string.IsNullOrEmpty(db.DIACHI_TT)) i.DIACHI_TT = db.DIACHI_TT.Trim();
                            if (!string.IsNullOrEmpty(db.DIENTHOAI_LH)) i.DIENTHOAI = db.DIENTHOAI_LH.Trim();
                            if (!string.IsNullOrEmpty(db.MA_TUYENTHU)) i.MA_TUYEN = db.MA_TUYENTHU.Trim();
                            if (!string.IsNullOrEmpty(db.DVQL_ID)) i.MA_DVI = db.DVQL_ID;
                            if (!string.IsNullOrEmpty(db.MA_CBT)) i.MA_CBT = db.MA_CBT;
                            //i.MA_KH = !string.IsNullOrEmpty(db.MA_KH) ? db.MA_KH.Trim() : null;
                            if (!string.IsNullOrEmpty(db.MA_TT_HNI)) i.MA_TT_HNI = db.MA_TT_HNI.Trim();
                            //i.MA_DT = db.MA_DT;
                            if (!string.IsNullOrEmpty(db.MS_THUE)) i.MS_THUE = db.MS_THUE.Trim();
                        }
                    }
                    db = dbpttb_kh.FirstOrDefault(d => d.ACCOUNT == i.MA_KH);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        if (!string.IsNullOrEmpty(db.MA_KH)) i.MA_KH = db.MA_KH.Trim();
                        i.MA_DT = db.MA_DT == 0 ? 1 : db.MA_DT;
                    }
                    db = dbpttb_dvi.FirstOrDefault(d => d.ACCOUNT == i.MA_DVI);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        i.MA_DVI = db.DVQL_ID;
                        if (i.MA_TUYEN == null || i.MA_TUYEN.isNumber()) i.MA_TUYEN = $"T{db.MS_THUE}000";
                    }
                    //Cập nhật danh bạ Fix
                    var dbkh = dbfix.FirstOrDefault(d => d.ACCOUNT == i.ACCOUNT);
                    if (dbkh != null)
                    {
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
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật danh bạ thành công - {data.Count()} Thuê bao" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                Oracle.Close();
                SQLServer.Close();
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateData(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "6,9,9693,9694";
            try
            {
                var qry = $"DELETE {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL IN({TYPE_BILL}) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_NET} 
                         SELECT NEWID() AS ID,ID AS NET_ID,NEWID() AS DBKH_ID,TYPE_BILL,TIME_BILL,CALLING AS ACCOUNT,CALLING AS MA_TB,TOC_DO,1 AS TT_THANG,TBTHG,30 AS NGAY_TB,30 AS NGAY_TB_PTTB,0 AS GOICUOCID,
                         0 AS TH_THANG,0 AS TH_HUY,0 AS DUPECOUNT,0 AS ISDATMOI,0 AS ISHUY,0 AS ISTTT,0 AS ISDATCOC,GIAM_TRU,
                         CUOC_IP,CUOC_EMAIL,0 AS CUOC_DATA,CUOC_SDKH AS CUOC_SD,CUOC_TB,0 AS TONG_TTT,0 AS TONG_DC,0 AS TONG_IN,TIEN AS TONG,ROUND(TIEN*0.1,0) AS VAT,ROUND(TIEN*1.1,0) AS TONGCONG,
                         NULL AS NGAY_DKY,NULL AS NGAY_CAT,NULL AS NGAY_HUY,NULL AS NGAY_CHUYEN,NGAY_SD,NGAY_KHOA,NGAY_MO,NGAY_KT
                         FROM {Common.Objects.TYPE_HD.NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TIEN>0";
                SQLServer.Connection.Query(qry);
                qry = $"UPDATE a SET a.MA_TB=b.MA_TB,a.DBKH_ID=b.ID FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.ACCOUNT=b.ACCOUNT WHERE b.FIX=0 AND b.FLAG=1 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";//a.TYPE_BILL=b.TYPE_BILL AND
                SQLServer.Connection.Query(qry);
                //Get Data Mega
                //var qry = $@"UPDATE a set a.GOICUOCID=0,a.TT_THANG=1,a.TBTHG=b.TBTHG,a.TH_THANG=0,a.TH_HUY=0,a.DUPECOUNT=0,a.ISNULLDB=0,
                //            a.ISNULLMT=0,a.ISHUY=0,a.ISTTT=0,a.ISDATCOC=0,a.GIAM_TRU=b.GIAM_TRU,a.CUOC_IP=b.CUOC_IP,a.CUOC_EMAIL=b.CUOC_EMAIL,
                //            a.CUOC_SD=b.CUOC_SD,a.CUOC_TB=b.CUOC_TB,a.TONG=b.TIEN,a.VAT=ROUND(b.TIEN*0.1,0),a.TONGCONG=ROUND(b.TIEN*1.1,0)
                //            FROM {Common.Objects.TYPE_HD.HD_NET} a join {Common.Objects.TYPE_HD.NET} b ON a.NET_ID=b.ID";
                //SQLServer.Connection.Query(qry);
                //Xóa account của BHXH
                qry = $"DELETE {Common.Objects.TYPE_HD.HD_NET} WHERE ACCOUNT IN('bhxhbk','bhxhns','bhxhtxbk','bhxhnari','bhxhhbt','bhxhbb','bhxhcdon','baohiemxahoicm','baohiemxhpn')";//'bhxhpn'
                SQLServer.Connection.Query(qry);
                //Fix lại thanh toán trước
                qry = $"UPDATE THANHTOANTRUOC SET THUC_TRU=0 WHERE TYPE_BILL IN({TYPE_BILL}) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateUseDay(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                FNCUpdateUseDay(obj, SQLServer, 6);
                FNCUpdateUseDay(obj, SQLServer, 9);
                FNCUpdateUseDay(obj, SQLServer, 9693);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật ngày sử dụng thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdatePrice(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                var qry = $"SELECT * FROM SETTINGS WHERE APP_KEY='{Common.Objects.SETTINGS_APP_KEY.MAIN}' AND SUB_KEY='{Common.Objects.SETTINGS_SUB_KEY.UPDATE_PRICE_INTERNET}'";
                var setttings = SQLServer.Connection.QueryFirstOrDefault<Models.SETTINGS>(qry);

                //Fix Tiền Megavnn
                //qry = $"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONG=130000 WHERE TOC_DO='D70T7P0_B1' AND TONG=91000";
                //SQLServer.Connection.Query(qry);

                if (setttings != null)
                {
                    var val = setttings.VAL.Trim(',').Split(',');
                    var toc_do = "";
                    foreach (var i in val)
                        toc_do += $"'{i}',";
                    toc_do = toc_do.Trim(',');
                    //Cập nhật giá từ bảng giá cho các thuê bao tròn tháng
                    qry = $@"UPDATE hd SET hd.TONG=bg.GIA-hd.GIAM_TRU FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN BGCUOC bg ON bg.PROFILEIP LIKE '%,'+hd.TOC_DO+',%' WHERE hd.GOICUOCID=0 AND hd.TOC_DO {setttings.SUB_VAL}({toc_do}) AND hd.TYPE_BILL=6 AND bg.DICHVUVT_ID=6 AND bg.IS_TH=0 AND bg.FLAG=1 AND hd.TT_THANG={(int)Common.Objects.BD_NGAY_TB._binh_thuong} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                             UPDATE hd SET hd.TONG=bg.GIA-hd.GIAM_TRU FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN BGCUOC bg ON bg.PROFILEIP LIKE '%,'+hd.TOC_DO+',%' WHERE hd.GOICUOCID=0 AND hd.TOC_DO {setttings.SUB_VAL}({toc_do}) AND hd.TYPE_BILL=9 AND bg.DICHVUVT_ID=9 AND bg.IS_TH=0 AND bg.FLAG=1 AND hd.TT_THANG={(int)Common.Objects.BD_NGAY_TB._binh_thuong} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    SQLServer.Connection.Query(qry);
                }
                //Cập nhật giá từ bảng giá cho các thuê bao tròn tháng Fiber Gia đình
                qry = $"UPDATE hd SET hd.TONG=bg.GIA-hd.GIAM_TRU FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN BGCUOC bg ON bg.PROFILEIP LIKE '%,'+hd.TOC_DO+',%' WHERE bg.DICHVUVT_ID=9 AND bg.IS_TH=1 AND bg.FLAG=1 AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._binh_thuong} AND hd.TYPE_BILL=9693 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);

                //Cập nhật vat và tổng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG*0.1,0) WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật giá thành công!" }, JsonRequestBehavior.AllowGet);
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
                //var qry = $"update net SET net.GOICUOCID=thdvtv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} net inner join (select * from DANHBA_GOICUOC_TICHHOP where goicuoc_id in (select thdv.goicuoc_id from {Common.Objects.TYPE_HD.HD_NET} tv,DANHBA_GOICUOC_TICHHOP thdv where tv.ACCOUNT=thdv.ACCOUNT and tv.TYPE_BILL=8 and FORMAT(tv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0)) thdvtv on net.ACCOUNT=thdvtv.ACCOUNT where DICHVUVT_ID=9 and net.TYPE_BILL=9 and FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and thdvtv.NGAY_KT>=CAST('{obj.block_time}' as datetime) AND net.ACCOUNT NOT IN (SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL=6 and FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}')";
                //SQLServer.Connection.Query(qry);

                //qry = $"update net SET net.GOICUOCID=thdvtv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} net inner join (select * from DANHBA_GOICUOC_TICHHOP where goicuoc_id in (select thdv.goicuoc_id from {Common.Objects.TYPE_HD.HD_NET} tv,DANHBA_GOICUOC_TICHHOP thdv where tv.ACCOUNT=thdv.ACCOUNT and tv.TYPE_BILL=8 and FORMAT(tv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0)) thdvtv on net.ACCOUNT=thdvtv.ACCOUNT where DICHVUVT_ID=9 and TYPE_BILL=9 and FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and thdvtv.NGAY_BD<CAST('{obj.block_time}' as datetime) AND thdvtv.NGAY_KT IS NULL AND net.ACCOUNT NOT IN (SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL=6 and FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}')";
                //SQLServer.Connection.Query(qry);
                var qry = $@"UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.MA_TB=thdv.MA_TB AND thdv.NGAY_KT>=CAST('{obj.block_time}' as datetime) AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL=6 AND thdv.DICHVUVT_ID=6 AND thdv.EXTRA_TYPE=0 AND thdv.FIX=0 AND thdv.FLAG=1;
                             UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.MA_TB=thdv.MA_TB AND thdv.NGAY_BD<CAST('{obj.block_time}' as datetime) AND thdv.NGAY_KT IS NULL AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL=6 AND thdv.DICHVUVT_ID=6 AND thdv.EXTRA_TYPE=0 AND thdv.FIX=0 AND thdv.FLAG=1;
                             UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.MA_TB=thdv.MA_TB AND thdv.NGAY_KT>=CAST('{obj.block_time}' as datetime) AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL=9 AND thdv.DICHVUVT_ID=9 AND thdv.EXTRA_TYPE=0 AND thdv.FIX=0 AND thdv.FLAG=1;
                             UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.MA_TB=thdv.MA_TB AND thdv.NGAY_BD<CAST('{obj.block_time}' as datetime) AND thdv.NGAY_KT IS NULL AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL=9 AND thdv.DICHVUVT_ID=9 AND thdv.EXTRA_TYPE=0 AND thdv.FIX=0 AND thdv.FLAG=1;";
                SQLServer.Connection.Query(qry);
                //Xử lý tích hợp thêm
                qry = $@"UPDATE net SET net.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} net INNER JOIN DANHBA_GOICUOC_TICHHOP thdv ON net.ACCOUNT=thdv.ACCOUNT WHERE net.TYPE_BILL=6 AND FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.DICHVUVT_ID=6 AND thdv.EXTRA_TYPE=0 AND thdv.FIX=1 AND thdv.FLAG=1 AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE net SET net.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} net INNER JOIN DANHBA_GOICUOC_TICHHOP thdv ON net.ACCOUNT=thdv.ACCOUNT WHERE net.TYPE_BILL=9 AND FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.DICHVUVT_ID=9 AND thdv.EXTRA_TYPE=0 AND thdv.FIX=1 AND thdv.FLAG=1 AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Cập nhật giá từng bảng giá đối với thuê bao tích hợp
                //qry = $@"UPDATE hd SET hd.TONG=bg.GIA FROM {Common.Objects.TYPE_HD.HD_NET} hd,BGCUOC bg WHERE hd.GOICUOCID=bg.GOICUOCID AND bg.PROFILEIP LIKE '%,'+hd.TOC_DO+',%' AND hd.TYPE_BILL=6 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND bg.FLAG=1 AND bg.DICHVUVT_ID=6;
                //         UPDATE hd SET hd.TONG=bg.GIA FROM {Common.Objects.TYPE_HD.HD_NET} hd,BGCUOC bg WHERE hd.GOICUOCID=bg.GOICUOCID AND bg.PROFILEIP LIKE '%,'+hd.TOC_DO+',%' AND hd.TYPE_BILL=9 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND bg.FLAG=1 AND bg.DICHVUVT_ID=9;";
                qry = $@"UPDATE hd SET hd.TONG=bg.GIA FROM {Common.Objects.TYPE_HD.HD_NET} hd,BGCUOC bg WHERE hd.GOICUOCID=bg.GOICUOCID AND hd.GOICUOCID>0 AND hd.TYPE_BILL=6 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND bg.FLAG=1 AND bg.DICHVUVT_ID=6;
                         UPDATE hd SET hd.TONG=bg.GIA FROM {Common.Objects.TYPE_HD.HD_NET} hd,BGCUOC bg WHERE hd.GOICUOCID=bg.GOICUOCID AND hd.GOICUOCID>0 AND hd.TYPE_BILL=9 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND bg.FLAG=1 AND bg.DICHVUVT_ID=9;";
                SQLServer.Connection.Query(qry);
                //Cập nhật thuê bao tích hợp không tròn tháng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONG=CUOC_SD,GOICUOCID=0 where GOICUOCID>0 AND CUOC_SD<TONG AND (NGAY_KHOA is not null or NGAY_MO is not null or NGAY_KT is not null) AND (TYPE_BILL=9 OR TYPE_BILL=6) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật giá đối với thuê bao chuyển đổi trong tháng
                qry = $@"UPDATE a SET a.TONG=a.CUOC_SD from HD_NET a INNER JOIN HD_NET b on a.ACCOUNT=b.ACCOUNT WHERE a.TYPE_BILL=6 AND b.TYPE_BILL=9693 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE a SET a.TONG=a.CUOC_SD from HD_NET a INNER JOIN HD_NET b on a.ACCOUNT=b.ACCOUNT WHERE a.TYPE_BILL=6 AND b.TYPE_BILL=9 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE a SET a.TONG=a.CUOC_SD from HD_NET a INNER JOIN HD_NET b on a.ACCOUNT=b.ACCOUNT WHERE a.TYPE_BILL=9 AND b.TYPE_BILL=9693 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE a SET a.TONG=a.CUOC_SD from HD_NET a INNER JOIN HD_NET b on a.ACCOUNT=b.ACCOUNT WHERE a.TYPE_BILL=9 AND b.TYPE_BILL=6 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG*0.1,0) WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật tích hợp thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyCuocData(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                var qry = $"UPDATE net SET net.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} net INNER JOIN DANHBA_GOICUOC_TICHHOP thdv ON net.ACCOUNT=thdv.ACCOUNT WHERE thdv.DICHVUVT_ID=9 and net.TYPE_BILL=9693 and FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.EXTRA_TYPE=1 AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);

                //Xử lý tích hợp thêm
                qry = $"UPDATE net SET net.GOICUOCID=thdv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} net INNER JOIN DANHBA_GOICUOC_TICHHOP thdv ON net.ACCOUNT=thdv.ACCOUNT WHERE net.TYPE_BILL=9693 AND FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.DICHVUVT_ID=9 AND thdv.FIX=1 AND thdv.EXTRA_TYPE=1 AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật giá từ bảng giá cho các thuê bao tích hợp
                qry = $@"UPDATE hd SET hd.CUOC_DATA=bg.GIA FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN BGCUOC bg ON hd.GOICUOCID=bg.GOICUOCID WHERE hd.GOICUOCID>0 AND hd.TYPE_BILL=9693 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND bg.FLAG=1 AND bg.DICHVUVT_ID=9004;
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET CUOC_DATA=ROUND((CUOC_DATA/30)*NGAY_TB,0) WHERE TYPE_BILL=9693 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONG=CUOC_SD+CUOC_DATA WHERE TYPE_BILL=9693 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG*0.1,0) WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9693;
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9693;";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật cước data thành phần thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
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
                qry += $"UPDATE hd SET hd.TONG=hd.TONG*((100-dc.VALUE)/100) FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.PERCENT} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                //MONEY
                //qry += $"UPDATE hd SET hd.TONG=hd.TONG-dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MONEY} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                //FIX
                //qry += $"UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=dc.TYPE_BILL AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                //qry += $@"UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT)) AND hd.TYPE_BILL=6 AND hd.ACCOUNT NOT IN (SELECT ACCOUNT FROM HD_NET WHERE TYPE_BILL=9 OR TYPE_BILL=9693);
                //          UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT)) AND hd.TYPE_BILL=9 AND hd.ACCOUNT NOT IN (SELECT ACCOUNT FROM HD_NET WHERE TYPE_BILL=6 OR TYPE_BILL=9693);";

                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG*0.1,0) WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật khuyến mại thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyManu(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            obj.data_id = 782;
            var FixPrice = 204545;
            var FixPriceTH = 131818;
            var TYPE_BILL = 9693;
            try
            {
                //Không tích hợp
                var qry = $"UPDATE a SET a.TONG={FixPrice} FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.DBKH_ID=b.ID WHERE b.MA_CBT='{obj.data_id}' AND a.GOICUOCID=0 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND (a.TYPE_BILL=9 OR a.TYPE_BILL=6) AND a.MA_TB NOT IN (SELECT MA_TB FROM HD_NET WHERE TYPE_BILL={TYPE_BILL})";
                SQLServer.Connection.Query(qry);
                //Tích hợp
                qry = $"UPDATE a SET a.TONG={FixPriceTH}  FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.DBKH_ID=b.ID WHERE b.MA_CBT='{obj.data_id}' AND a.GOICUOCID>0 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND (a.TYPE_BILL=9 OR a.TYPE_BILL=6) AND a.MA_TB NOT IN (SELECT MA_TB FROM HD_NET WHERE TYPE_BILL={TYPE_BILL})";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng
                qry = $@"UPDATE a SET a.VAT=ROUND(a.TONG*0.1,0) FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.DBKH_ID=b.ID WHERE b.MA_CBT='{obj.data_id}' AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND (a.TYPE_BILL=9 OR a.TYPE_BILL=6) AND a.MA_TB NOT IN (SELECT MA_TB FROM HD_NET WHERE TYPE_BILL={TYPE_BILL});
                         UPDATE a SET a.TONGCONG=a.TONG+a.VAT FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.DBKH_ID=b.ID WHERE b.MA_CBT='{obj.data_id}' AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND (a.TYPE_BILL=9 OR a.TYPE_BILL=6) AND a.MA_TB NOT IN (SELECT MA_TB FROM HD_NET WHERE TYPE_BILL={TYPE_BILL});";
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật thuê bao Manu thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyCuocFix(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = 9693;
            try
            {
                var qry = $@"UPDATE a SET a.GIAM_TRU=b.GIAM_TRU,a.CUOC_IP=b.CUOC_IP,a.CUOC_EMAIL=b.CUOC_EMAIL,a.CUOC_SD=b.CUOC_SD,a.CUOC_TB=b.CUOC_TB,a.TONG=b.TONG,a.VAT=b.VAT,a.TONGCONG=b.TONGCONG 
                             FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN (SELECT * FROM {Common.Objects.TYPE_HD.HD_NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9661) b ON a.MA_TB=b.MA_TB 
                             WHERE FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.TYPE_BILL=6 AND a.MA_TB NOT IN (SELECT MA_TB FROM HD_NET WHERE TYPE_BILL={TYPE_BILL});";
                qry += $@"UPDATE a SET a.GIAM_TRU=b.GIAM_TRU,a.CUOC_IP=b.CUOC_IP,a.CUOC_EMAIL=b.CUOC_EMAIL,a.CUOC_SD=b.CUOC_SD,a.CUOC_TB=b.CUOC_TB,a.TONG=b.TONG,a.VAT=b.VAT,a.TONGCONG=b.TONGCONG 
                             FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN (SELECT * FROM {Common.Objects.TYPE_HD.HD_NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9691) b ON a.MA_TB=b.MA_TB 
                             WHERE FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND a.TYPE_BILL=9 AND a.MA_TB NOT IN (SELECT MA_TB FROM HD_NET WHERE TYPE_BILL={TYPE_BILL});";
                SQLServer.Connection.Query(qry);
                qry = $@"SELECT * FROM {Common.Objects.TYPE_HD.HD_NET} a,(SELECT * FROM {Common.Objects.TYPE_HD.HD_NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND (TYPE_BILL=6 OR TYPE_BILL=9)) b WHERE a.MA_TB=b.MA_TB AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND (a.TYPE_BILL=9691 OR a.TYPE_BILL=9661) AND a.MA_TB NOT IN (SELECT MA_TB FROM HD_NET WHERE TYPE_BILL={TYPE_BILL})";
                var rs = SQLServer.Connection.Query<Models.HD_NET>(qry).ToList();
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật cước Fix thành công: {rs.Count} Thuê bao" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyCuocMergin(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                //Mega
                var rsData = new List<Models.HD_NET>();
                var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.HD_NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9662";
                var HDMergin = SQLServer.Connection.Query<Models.HD_NET>(qry).ToList();

                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN}";
                var dbkh = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
                foreach (var i in HDMergin)
                {
                    index++;
                    i.ID = Guid.NewGuid();
                    i.TYPE_BILL = 6;
                    i.TIME_BILL = obj.datetime;
                    var _tmp = dbkh.FirstOrDefault(d => d.ACCOUNT == i.ACCOUNT);
                    if (_tmp != null)
                        i.DBKH_ID = _tmp.ID;
                }
                SQLServer.Connection.Insert(HDMergin);
                rsData.AddRange(HDMergin);
                //Fiber
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.HD_NET} WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TYPE_BILL=9692";
                HDMergin = SQLServer.Connection.Query<Models.HD_NET>(qry).ToList();
                foreach (var i in HDMergin)
                {
                    index++;
                    i.ID = Guid.NewGuid();
                    i.TYPE_BILL = 9;
                    i.TIME_BILL = obj.datetime;
                    var _tmp = dbkh.FirstOrDefault(d => d.ACCOUNT == i.ACCOUNT);
                    if (_tmp != null)
                        i.DBKH_ID = _tmp.ID;
                }
                SQLServer.Connection.Insert(HDMergin);
                rsData.AddRange(HDMergin);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật cước ghép thành công: {rsData.Count()} Thuê bao" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Ghép Mega - Fiber - Fiber GD
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyGhepHDNET(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            long index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            //
            int TYPE_BILL = 9005;
            try
            {
                //Xóa data cũ
                var qry = $"DELETE {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Nhập HD Fiber
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_NET} 
                         SELECT NEWID() AS ID,ID AS NET_ID,DBKH_ID,{TYPE_BILL} AS TYPE_BILL,TIME_BILL,ACCOUNT,MA_TB,TOC_DO,TT_THANG,TBTHG,NGAY_TB,NGAY_TB_PTTB,GOICUOCID,TH_THANG,TH_HUY,DUPECOUNT,ISDATMOI,ISHUY,ISTTT,ISDATCOC,GIAM_TRU,CUOC_IP,CUOC_EMAIL,CUOC_DATA,CUOC_SD,CUOC_TB,TONG_TTT,TONG_DC,TONG_IN,TONG,VAT,TONGCONG,NGAY_DKY,NGAY_CAT,NGAY_HUY,NGAY_CHUYEN,NGAY_SD,NGAY_KHOA,NGAY_MO,NGAY_KT 
                         FROM {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL=9 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật cước Mega chuyển đổi lên Fiber
                qry = $"UPDATE a SET a.TONG=a.TONG+b.TONG,a.TONG_TTT=a.TONG_TTT+b.TONG_TTT,a.TONG_DC=a.TONG_DC+b.TONG_DC,a.TONG_IN=a.TONG_IN+b.TONG_IN,a.CUOC_DATA=a.CUOC_DATA+b.CUOC_DATA,a.CUOC_SD=a.CUOC_SD+b.CUOC_SD,a.CUOC_TB=a.CUOC_TB+b.CUOC_TB FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN {Common.Objects.TYPE_HD.HD_NET} b ON a.ACCOUNT=b.ACCOUNT WHERE a.TYPE_BILL={TYPE_BILL} AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND b.TYPE_BILL=6 AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Nhập cước Mega không chuyển đổi lên Fiber
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_NET} 
                         SELECT NEWID() AS ID,ID AS NET_ID,DBKH_ID,{TYPE_BILL} AS TYPE_BILL,TIME_BILL,ACCOUNT,MA_TB,TOC_DO,TT_THANG,TBTHG,NGAY_TB,NGAY_TB_PTTB,GOICUOCID,TH_THANG,TH_HUY,DUPECOUNT,ISDATMOI,ISHUY,ISTTT,ISDATCOC,GIAM_TRU,CUOC_IP,CUOC_EMAIL,CUOC_DATA,CUOC_SD,CUOC_TB,TONG_TTT,TONG_DC,TONG_IN,TONG,VAT,TONGCONG,NGAY_DKY,NGAY_CAT,NGAY_HUY,NGAY_CHUYEN,NGAY_SD,NGAY_KHOA,NGAY_MO,NGAY_KT 
                         FROM {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL=6 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ACCOUNT NOT IN(SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}')";
                SQLServer.Connection.Query(qry);
                //Cập nhật cước Fiber chuyển đổi lên Fiber GD
                qry = $"UPDATE a SET a.TONG=a.TONG+b.TONG,a.TONG_TTT=a.TONG_TTT+b.TONG_TTT,a.TONG_DC=a.TONG_DC+b.TONG_DC,a.TONG_IN=a.TONG_IN+b.TONG_IN,a.CUOC_DATA=a.CUOC_DATA+b.CUOC_DATA,a.CUOC_SD=a.CUOC_SD+b.CUOC_SD,a.CUOC_TB=a.CUOC_TB+b.CUOC_TB FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN HD_NET b ON a.ACCOUNT=b.ACCOUNT WHERE a.TYPE_BILL={TYPE_BILL} AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND b.TYPE_BILL=9693 AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Nhập cước Fiber không chuyển đổi lên Fiber GD
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_NET} 
                         SELECT NEWID() AS ID,ID AS NET_ID,DBKH_ID,{TYPE_BILL} AS TYPE_BILL,TIME_BILL,ACCOUNT,MA_TB,TOC_DO,TT_THANG,TBTHG,NGAY_TB,NGAY_TB_PTTB,GOICUOCID,TH_THANG,TH_HUY,DUPECOUNT,ISDATMOI,ISHUY,ISTTT,ISDATCOC,GIAM_TRU,CUOC_IP,CUOC_EMAIL,CUOC_DATA,CUOC_SD,CUOC_TB,TONG_TTT,TONG_DC,TONG_IN,TONG,VAT,TONGCONG,NGAY_DKY,NGAY_CAT,NGAY_HUY,NGAY_CHUYEN,NGAY_SD,NGAY_KHOA,NGAY_MO,NGAY_KT 
                         FROM {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL=9693 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ACCOUNT NOT IN(SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}')";
                SQLServer.Connection.Query(qry);

                //Cập nhật giảm trừ tiền và fix giá
                //MONEY
                qry = $"UPDATE hd SET hd.TONG=hd.TONG-dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL={TYPE_BILL} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MONEY} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT))";
                SQLServer.Connection.Query(qry);
                //FIX
                qry = $"UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL={TYPE_BILL} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT))";
                SQLServer.Connection.Query(qry);

                //qry += $"UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL=dc.TYPE_BILL AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                //qry += $@"UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT)) AND hd.TYPE_BILL=6 AND hd.ACCOUNT NOT IN (SELECT ACCOUNT FROM HD_NET WHERE TYPE_BILL=9 OR TYPE_BILL=9693);
                //          UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL BETWEEN dc.NGAY_BD AND dc.NGAY_KT)) AND hd.TYPE_BILL=9 AND hd.ACCOUNT NOT IN (SELECT ACCOUNT FROM HD_NET WHERE TYPE_BILL=6 OR TYPE_BILL=9693);";

                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Xử lý ghép Mega - Fiber - Fiber GD thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
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
            var TYPE_BILL_MERGIN = 9005;
            var DVVT_ID = "6,9";
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
                qry = $"UPDATE ttt SET ttt.FLAG=-1 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.ACCOUNT NOT IN(SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_NET} WHERE TYPE_BILL={TYPE_BILL_MERGIN} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}');";
                SQLServer.Connection.Query(qry);

                //Bước 1: Xử lý các thuê bao còn TTT và (KT hoặc CK)
                //Cập nhật cước từ hóa đơn
                qry = $@"UPDATE ttt SET ttt.FLAG=1,ttt.TONG=hd.TONG,SODU_TONG=ttt.SODU-hd.TONG FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_NET} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=0 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL={TYPE_BILL_MERGIN} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Cập nhật thực trừ cho TTT có số dư tổng > 0
                qry = $@"UPDATE ttt SET ttt.THUC_TRU=ttt.TONG-ttt.EXTRA_TONG FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=1;";
                SQLServer.Connection.Query(qry);
                //Cập nhật thực trừ bằng số dư đối với các TTT có số dư tổng < 0
                qry = $@"UPDATE ttt SET ttt.FLAG=2,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=1 AND ttt.SODU_TONG<0;";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái cho TTT sau khi trừ có số dư tổng < 0 và không có KT hoặc CK
                qry = $@"UPDATE ttt SET ttt.FLAG=3 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=2 AND ttt.MA_TB NOT IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN!='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID IN({DVVT_ID}) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}');";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái cho KT hoặc CK khi TTT có số dư tổng < 0
                qry = $@"UPDATE ttt SET ttt.FLAG=2 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=0 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.MA_TB IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID IN({DVVT_ID}) AND FLAG=2 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}');";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái cho KT hoặc CK khi TTT có số dư tổng > 0
                qry = $@"UPDATE ttt SET ttt.FLAG=8 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=0 AND ttt.MA_TB IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID IN({DVVT_ID}) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FLAG=1);";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái, tổng, thực trừ bằng số tiền còn thiếu cho KT hoặc CK sau khi trừ TTT và số du tổng sau khi trừ
                qry = $@"UPDATE ttt SET ttt.FLAG=4,ttt.THUC_TRU=ttt2.SODU_TONG*-1,ttt.TONG=ttt2.TONG,ttt.SODU_TONG=ttt.SODU+ttt2.SODU_TONG FROM THANHTOANTRUOC ttt,THANHTOANTRUOC ttt2 WHERE ttt.MA_TB=ttt2.MA_TB AND ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=2 AND ttt2.KHOANTIEN='TTT' AND ttt2.TYPE_BILL={TYPE_BILL} AND ttt2.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt2.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt2.FLAG=2;";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái và thực trừ bằng số dư đối với KT hoặc CK có số dư tổng < 0
                qry = $@"UPDATE ttt SET ttt.FLAG=5,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=4 AND ttt.SODU_TONG<0;";
                SQLServer.Connection.Query(qry);

                //Bước 2: Xử lý các thuê bao không còn TTT chỉ còn KT hoặc CK
                //Cập nhật trạng thái, thực trừ, cước từ hóa đơn
                qry = $@"UPDATE ttt SET ttt.FLAG=6,ttt.TONG=hd.TONG,SODU_TONG=ttt.SODU-hd.TONG,ttt.THUC_TRU=hd.TONG FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_NET} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=0 AND hd.TYPE_BILL={TYPE_BILL_MERGIN} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Cập nhật trạng thái, thực trừ cho KT hoặc CK có số dư tổng < 0
                qry = $@"UPDATE ttt SET ttt.FLAG=7,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG=6 AND ttt.SODU_TONG<0;";
                SQLServer.Connection.Query(qry);

                //Bước 3: Cập nhật thực trừ vào hóa đơn
                //Cập nhật các thuê bao có số dư tổng > 0
                qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=hd.TONG,hd.TONG=0 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_NET} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG IN(1,4,6) AND hd.TYPE_BILL={TYPE_BILL_MERGIN} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Cập nhật các thuê bao có số dư tổng < 0
                qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.TONG+SODU_TONG,hd.TONG=ttt.SODU_TONG*-1 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_NET} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.FLAG IN(3,5,7) AND hd.TYPE_BILL={TYPE_BILL_MERGIN} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);

                //Cập nhật vat và tổng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG*0.1,0) WHERE ISTTT=1 AND TYPE_BILL={TYPE_BILL_MERGIN} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE ISTTT=1 AND TYPE_BILL={TYPE_BILL_MERGIN} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật thanh toán trước thành công!" }, JsonRequestBehavior.AllowGet);
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
                //GetSetting
                var qry = "SELECT * FROM SETTINGS WHERE APP_KEY='MAIN' AND SUB_KEY='INTERNET_ACTION_KEY'";
                //var setting = SQLServer.Connection.QueryFirstOrDefault<Models.SETTINGS>(qry);
                //if (setting == null)
                //    return Json(new { danger = $"Vui lòng cấu hình TYPE_BILL trong Setting trước!" }, JsonRequestBehavior.AllowGet);
                //UPDATE CUOC_FIX=0
                //qry = $"UPDATE THANHTOANTRUOC SET CUOC_FIX=0 WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //Check Thuê bao thanh toán trước không có trong hóa đơn
                qry = $"UPDATE ttt SET FLAG=1 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.TIME_BILL>=ttt.NGAY_BD AND ttt.TIME_BILL<ttt.NGAY_KT AND ACCOUNT IN(SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.HD_NET}) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);

                //var _net_type = setting.VAL.Trim(',').Split(',');
                //foreach (var i in _net_type)
                //{
                //    qry = $"UPDATE ttt SET ttt.THUC_TRU=ttt.THUC_TRU+hd.TONG FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_NET} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND hd.TYPE_BILL={i} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //    SQLServer.Connection.Query(qry);
                //}
                qry = $"UPDATE ttt SET ttt.THUC_TRU=hd.TONG FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_NET} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.TIME_BILL>=ttt.NGAY_BD AND ttt.TIME_BILL<ttt.NGAY_KT AND hd.TYPE_BILL={9005} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);

                qry = $"UPDATE hd SET ISTTT=1,hd.TONG_TTT=hd.TONG,TONG=0 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_NET} hd ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.TIME_BILL>=ttt.NGAY_BD AND ttt.TIME_BILL<ttt.NGAY_KT AND hd.TYPE_BILL={9005} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG*0.1,0) WHERE ISTTT=1 AND TYPE_BILL={9005} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE ISTTT=1 AND TYPE_BILL={9005} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật thanh toán trước fix thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Xử lý gán cước in
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyGanCuocIn(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            int TYPE_BILL = 9005;
            try
            {
                //Xử lý FIX_IN
                var qry = $@"UPDATE hd SET hd.TONG_IN=TONG FROM {Common.Objects.TYPE_HD.HD_NET} hd WHERE hd.TYPE_BILL={TYPE_BILL} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                             UPDATE hd SET hd.TONG_IN=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN DISCOUNT dc ON hd.ACCOUNT=dc.ACCOUNT WHERE hd.TYPE_BILL={TYPE_BILL} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX_IN} AND ((hd.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.TIME_BILL>=dc.NGAY_BD AND hd.TIME_BILL<=dc.NGAY_KT));
                             UPDATE hd SET hd.TONG_IN=TONG FROM {Common.Objects.TYPE_HD.HD_NET} hd WHERE hd.TONG<hd.TONG_IN AND hd.TYPE_BILL={TYPE_BILL} AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Update THANHTOANTRUOC + FIX_IN
                qry = $@"UPDATE ttt SET ttt.CUOC_FIX=0 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL=9002 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                qry += $@"UPDATE ttt SET ttt.CUOC_FIX=ttt.THUC_TRU,ttt.THUC_TRU=dc.VALUE FROM THANHTOANTRUOC ttt INNER JOIN DISCOUNT dc ON ttt.ACCOUNT=dc.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL=9002 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(dc.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX_IN} AND ((ttt.TIME_BILL>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (ttt.TIME_BILL>=dc.NGAY_BD AND ttt.TIME_BILL<=dc.NGAY_KT));";
                //qry += $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.THUC_TRU FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN THANHTOANTRUOC ttt ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL=9002 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.CUOC_FIX>0 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                qry += $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.THUC_TRU,TONG_IN=0,TONG=0,VAT=0,TONGCONG=0 FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN THANHTOANTRUOC ttt ON ttt.ACCOUNT=hd.ACCOUNT WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL=9002 AND FORMAT(ttt.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND ttt.CUOC_FIX>0 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);

                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_NET} - Cập nhật gán cước in thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Update UseDay
        public void FNCUpdateUseDay(Common.DefaultObj obj, TM.Connection.SQLServer SQLServer, int TYPE_BILL)
        {
            var qry = "";
            //_binh_thuong
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._binh_thuong} 
                         WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=30
                         WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_khong_tien
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._khong_tien} 
                         WHERE TONG<=0 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=0 
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._khong_tien} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_su_dung
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._su_dung} 
                         WHERE FORMAT(NGAY_SD,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=30-DAY(NGAY_SD) 
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._su_dung} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_ket_thuc
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._ket_thuc} 
                         WHERE FORMAT(NGAY_KT,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=DAY(NGAY_KT) 
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._ket_thuc} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_khoa
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._khoa} 
                         WHERE FORMAT(NGAY_KHOA,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=DAY(NGAY_KHOA) 
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._khoa} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_mo
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._mo} 
                         WHERE FORMAT(NGAY_MO,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=30-DAY(NGAY_MO) 
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._mo} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_mo 2
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=30-DAY(NGAY_SD) 
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._mo} AND NGAY_SD>NGAY_MO AND TYPE_BILL=9693 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_su_dung_ket_thuc
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._su_dung_ket_thuc} 
                         WHERE FORMAT(NGAY_SD,'MM/yyyy')='{obj.month_year_time}' AND (NGAY_SD-NGAY_KT)<0 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=DAY(NGAY_KT)-DAY(NGAY_SD) 
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._su_dung_ket_thuc} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_su_dung_khoa
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._su_dung_khoa} 
                         WHERE FORMAT(NGAY_SD,'MM/yyyy')='{obj.month_year_time}' AND (NGAY_SD-NGAY_KHOA)<0 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=DAY(NGAY_KHOA)-DAY(NGAY_SD) 
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._su_dung_khoa} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_mo_ket_thuc
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._khoa_mo} 
                         WHERE FORMAT(NGAY_MO,'MM/yyyy')='{obj.month_year_time}' AND (NGAY_KT-NGAY_MO)<0 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=DAY(NGAY_KT)-DAY(NGAY_MO)
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._khoa_mo} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_khoa_mo
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._khoa_mo} 
                         WHERE FORMAT(NGAY_KHOA,'MM/yyyy')='{obj.month_year_time}' AND (NGAY_KHOA-NGAY_MO)<0 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=30-DAY(NGAY_MO)
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._khoa_mo} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_khoa_mo 2
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=30-DAY(NGAY_SD)
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._khoa_mo} AND NGAY_SD>NGAY_MO AND TYPE_BILL=9693 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            //_mo_khoa
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TT_THANG={(int)Common.Objects.BD_NGAY_TB._mo_khoa} 
                         WHERE FORMAT(NGAY_MO,'MM/yyyy')='{obj.month_year_time}' AND (NGAY_MO-NGAY_KHOA)<0 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND TT_THANG!={(int)Common.Objects.BD_NGAY_TB._khong_tien};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=DAY(NGAY_KHOA)-DAY(NGAY_MO)
                         WHERE TYPE_BILL={TYPE_BILL} AND TT_THANG={(int)Common.Objects.BD_NGAY_TB._mo_khoa} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
            qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET NGAY_TB=1 WHERE NGAY_TB<1 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
            SQLServer.Connection.Query(qry);
        }
        //
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
    }
}