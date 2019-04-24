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
                    i.KYHOADON = obj.KYHD;
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
                qry = $"DELETE MYTV WHERE KYHOADON={obj.KYHD}";
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
            var Oracle = new TM.Connection.Oracle("DHSX_BACKAN");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "8";
            try
            {
                #region old
                //    //Get Data
                //    var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.MYTV} WHERE KYHOADON={obj.KYHD} AND TOTAL_FEE>0";
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
                //        _data.KYHOADON = i.KYHOADON;
                //        _data.MA_TB = i.USERNAME;
                //        _data.TOC_DO = i.PACKCD;
                //        _data.TH_SD = 1;
                //        _data.NGAY_SD = i.USE_DATE;
                //        _data.NGAY_KHOA = i.SUSPENDATE;
                //        _data.NGAY_MO = i.RESUMEDATE;
                //        _data.NGAY_KT = i.STOP_DATE;
                //        //
                //        var pttb = dbpttb.FirstOrDefault(d => d.LOGINNAME.Trim() == _data.MA_TB);
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
                //            if (!string.IsNullOrEmpty(pttb.MA_TT)) _data.MA_TT = pttb.MA_TT.Trim();
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
                //    qry = $"DELETE {Common.Objects.TYPE_HD.HD_MYTV} WHERE KYHOADON={obj.KYHD}";
                //    SQLServer.Connection.Query(qry);
                //    //
                //    SQLServer.Connection.Insert(data);
                #endregion
                var qry = $"SELECT * FROM {Common.Objects.TYPE_HD.MYTV} WHERE KYHOADON={obj.KYHD}";
                var data = SQLServer.Connection.Query<Models.MYTV>(qry);
                //Get DB PTTB
                qry = $@"select dv.TEN_DV,t.MA_TUYEN,tt.TEN_TT,tt.DIACHI_TT,nv.MA_NV as ma_cbt,nv.TEN_NV,tttb.TRANGTHAI_TB,tt.SO_DT,tt.MST,kh.ma_kh,tb.*,dvvt.ma_dvvt,dvvt.ten_dvvt 
                        from tinhcuoc_bkn.dbkh_{obj.KYHD} kh,tinhcuoc_bkn.dbtt_{obj.KYHD} tt,tinhcuoc_bkn.dbtb_{obj.KYHD} tb,CSS_BKN.TUYENTHU t,ADMIN_BKN.DONVI dv,ADMIN_BKN.NHANVIEN nv,CSS_BKN.TRANGTHAI_TB tttb,CSS_BKN.DICHVU_VT dvvt
                        WHERE tb.khachhang_id=kh.khachhang_id and tb.THANHTOAN_ID=tt.THANHTOAN_ID and tt.TUYENTHU_ID=t.TUYENTHU_ID and tt.DONVI_ID=dv.DONVI_ID and t.NHANVIEN_ID=nv.NHANVIEN_ID and tb.TRANGTHAITB_ID=tttb.TRANGTHAITB_ID 
                        and dvvt.DICHVUVT_ID=tb.DICHVUVT_ID and tb.DICHVUVT_ID=4 and tb.LOAITB_ID in(61) ORDER BY tt.DONVI_ID,nv.MA_NV,t.MA_TUYEN";
                var dbpttb = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                //
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var dbkh = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry);
                var DataInsert = new List<Models.DB_THANHTOAN_BKN>();
                var DataUpdate = new List<Models.DB_THANHTOAN_BKN>();
                foreach (var i in data)
                {
                    var _tmp = dbkh.FirstOrDefault(d => d.MA_TB == i.USERNAME);
                    var pttb = dbpttb.FirstOrDefault(d => d.MA_TB.Trim() == i.USERNAME);
                    if (_tmp != null)
                    {
                        if (pttb != null)
                        {
                            if (!string.IsNullOrEmpty(pttb.MA_KH)) _tmp.MA_KH = pttb.MA_KH.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_TT)) _tmp.MA_TT = pttb.MA_TT.Trim();
                            if (!string.IsNullOrEmpty(pttb.TEN_TT)) _tmp.TEN_TT = pttb.TEN_TT.Trim();
                            if (!string.IsNullOrEmpty(pttb.DIACHI_TT)) _tmp.DIACHI_TT = pttb.DIACHI_TT.Trim();
                            if (!string.IsNullOrEmpty(pttb.SO_DT)) _tmp.DIENTHOAI = pttb.SO_DT.Trim();
                            if (!string.IsNullOrEmpty(pttb.MST)) _tmp.MS_THUE = pttb.MST.Trim();
                            //_tmp.BANKNUMBER = null;
                            _tmp.DONVI_ID = pttb.DONVI_ID;
                            _tmp.DONVITC_ID = pttb.DONVITC_ID;
                            _tmp.KHACHHANG_ID = pttb.KHACHHANG_ID;
                            _tmp.THANHTOAN_ID = pttb.THANHTOAN_ID;
                            _tmp.THUEBAO_ID = pttb.THUEBAO_ID;
                            if (!string.IsNullOrEmpty(pttb.MA_CBT)) _tmp.MA_CBT = pttb.MA_CBT.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_TUYEN)) _tmp.MA_TUYEN = pttb.MA_TUYEN.Trim();
                            //if (!string.IsNullOrEmpty(pttb.CUSTCATE)) _tmp.CUSTCATE = pttb.CUSTCATE.Trim();
                            //_tmp.STK = null;
                            _tmp.MA_DT = pttb.DOITUONG_ID;
                            _tmp.TH_SD = pttb.TRANGTHAITB_ID;
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
                        _d.MA_TB = _d.MA_TB = i.USERNAME;
                        if (pttb != null)
                        {
                            if (!string.IsNullOrEmpty(pttb.MA_KH)) _d.MA_KH = pttb.MA_KH.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_TT)) _d.MA_TT = pttb.MA_TT.Trim();
                            if (!string.IsNullOrEmpty(pttb.TEN_TT)) _d.TEN_TT = pttb.TEN_TT.Trim();
                            if (!string.IsNullOrEmpty(pttb.DIACHI_TT)) _d.DIACHI_TT = pttb.DIACHI_TT.Trim();
                            if (!string.IsNullOrEmpty(pttb.SO_DT)) _d.DIENTHOAI = pttb.SO_DT.Trim();
                            if (!string.IsNullOrEmpty(pttb.MST)) _d.MS_THUE = pttb.MST.Trim();
                            //_tmp.BANKNUMBER = null;
                            _d.DONVI_ID = pttb.DONVI_ID;
                            _d.DONVITC_ID = pttb.DONVITC_ID;
                            _d.KHACHHANG_ID = pttb.KHACHHANG_ID;
                            _d.THANHTOAN_ID = pttb.THANHTOAN_ID;
                            _d.THUEBAO_ID = pttb.THUEBAO_ID;
                            if (!string.IsNullOrEmpty(pttb.MA_CBT)) _d.MA_CBT = pttb.MA_CBT.Trim();
                            if (!string.IsNullOrEmpty(pttb.MA_TUYEN)) _d.MA_TUYEN = pttb.MA_TUYEN.Trim();
                            //if (!string.IsNullOrEmpty(pttb.CUSTCATE)) _d.CUSTCATE = pttb.CUSTCATE.Trim();
                            //_tmp.STK = null;
                            _d.MA_DT = pttb.DOITUONG_ID;
                            _d.TH_SD = pttb.TRANGTHAITB_ID;
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
                qry = $"update db set db.MA_DVI=dv.MA_DVI,db.DONVI_QL_ID=dv.DONVI_QL_ID from DB_THANHTOAN_BKN db,DB_DONVI_BKN dv where dv.DONVI_ID=db.DONVI_ID and type_bill in({TYPE_BILL})";
                //qry = @"update DB_THANHTOAN_BKN set MA_DVI = 1 where DONVI_ID = 5588;
                //        update DB_THANHTOAN_BKN set MA_DVI = 2 where DONVI_ID = 7575;
                //        update DB_THANHTOAN_BKN set MA_DVI = 3 where DONVI_ID = 5590;
                //        update DB_THANHTOAN_BKN set MA_DVI = 4 where DONVI_ID = 5595;
                //        update DB_THANHTOAN_BKN set MA_DVI = 5 where DONVI_ID = 5591;
                //        update DB_THANHTOAN_BKN set MA_DVI = 6 where DONVI_ID = 5594;
                //        update DB_THANHTOAN_BKN set MA_DVI = 7 where DONVI_ID = 5593;
                //        update DB_THANHTOAN_BKN set MA_DVI = 8 where DONVI_ID = 5592;";
                SQLServer.Connection.Query(qry);
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
                qry = "select tt.MA_TT,tb.MA_TB as LOGINNAME,tt.DIACHI_TT as ADDRESS1,tt.TEN_TT as FULLNAME,tt.DIENTHOAI_TT as MOBILE,tt.KHACHHANG_ID as MA_KH,tt.MAPHO_ID as MA_DVI,tt.MA_TUYENTHU as MA_TUYENTHU from DB_THUEBAO_BKN tb,DB_THANHTOAN_BKN tt where tb.thanhtoan_id=tt.thanhtoan_id";
                var dbpttb = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                qry = "select MA_KH,DOITUONGKH_ID as MA_DT,KHACHHANG_ID,KHACHHANG_ID as LOGINNAME from VTT.DB_KHACHHANG_BKN";
                var dbpttb_kh = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                qry = "select a.MAPHO_ID as MA_CQ,c.MA_QUANHUYEN as MA_DVI,c.VIETTAT as MA_ST from MA_PHO_BKN a,PHUONG_XA_BKN b,QUAN_HUYEN_BKN c where a.PHUONGXA_ID=b.PHUONGXA_ID and b.QUANHUYEN_ID=c.QUANHUYEN_ID";
                var dbpttb_dvi = Oracle.Connection.Query<Models.DANH_BA_MYTV>(qry).ToList();
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=1 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL})";
                var dbfix = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry).ToList();
                foreach (var i in data)
                {
                    var db = dbpttb.FirstOrDefault(d => d.MA_TB == i.MA_TB);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        if (!string.IsNullOrEmpty(db.TEN_TT)) i.TEN_TT = db.TEN_TT.Trim();
                        if (!string.IsNullOrEmpty(db.DIACHI_TT)) i.DIACHI_TT = db.DIACHI_TT.Trim();
                        if (!string.IsNullOrEmpty(db.SO_DT)) i.DIENTHOAI = db.SO_DT.Trim();
                        if (!string.IsNullOrEmpty(db.MA_TUYEN)) i.MA_TUYEN = db.MA_TUYEN.Trim();
                        i.DONVI_ID = db.DONVI_ID;
                        i.DONVITC_ID = db.DONVITC_ID;
                        if (!string.IsNullOrEmpty(db.MA_CBT)) i.MA_CBT = db.MA_CBT;
                        //i.MA_KH = !string.IsNullOrEmpty(db.MA_KH) ? db.MA_KH.Trim() : null;
                        if (!string.IsNullOrEmpty(db.MA_TT)) i.MA_TT = db.MA_TT.Trim();
                        //i.MA_DT = db.MA_DT == 0 ? 1 : db.MA_DT;
                        if (!string.IsNullOrEmpty(db.MST)) i.MS_THUE = db.MST.Trim();
                    }
                    db = dbpttb_kh.FirstOrDefault(d => d.MA_KH == i.MA_KH);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        if (!string.IsNullOrEmpty(db.MA_KH)) i.MA_KH = db.MA_KH.Trim();
                        i.MA_DT = db.DOITUONG_ID == 0 ? 1 : db.DOITUONG_ID;
                    }
                    db = dbpttb_dvi.FirstOrDefault(d => d.DONVI_ID == i.DONVI_ID);
                    if (db != null)
                    {
                        i.ISNULL = 2;
                        i.DONVI_ID = db.DONVI_ID;
                        i.DONVITC_ID = db.DONVITC_ID;
                        if (i.MA_TUYEN == null || i.MA_TUYEN.isNumber()) i.MA_TUYEN = $"T{db.MST}000";
                    }
                    //Cập nhật danh bạ Fix
                    var dbkh = dbfix.FirstOrDefault(d => d.MA_TB == i.MA_TB);
                    if (dbkh != null)
                    {
                        //if (dbkh.MA_TB == "bcntv00067946")
                        //{
                        //    dbkh.MA_TB = "bcntv00067946";
                        //}
                        i.ISNULL = 3;
                        if (!string.IsNullOrEmpty(dbkh.TEN_TT)) i.TEN_TT = dbkh.TEN_TT.Trim();
                        if (!string.IsNullOrEmpty(dbkh.DIACHI_TT)) i.DIACHI_TT = dbkh.DIACHI_TT.Trim();
                        if (!string.IsNullOrEmpty(dbkh.DIENTHOAI)) i.DIENTHOAI = dbkh.DIENTHOAI.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_DVI)) i.MA_DVI = dbkh.MA_DVI.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_TUYEN)) i.MA_TUYEN = dbkh.MA_TUYEN.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_CBT)) i.MA_CBT = dbkh.MA_CBT.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_KH)) i.MA_KH = dbkh.MA_KH.Trim();
                        if (!string.IsNullOrEmpty(dbkh.MA_TT)) i.MA_TT = dbkh.MA_TT.Trim();
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
                         UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET MA_KH=MA_TT WHERE MA_KH IS NULL AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});
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
                var qry = $"DELETE {Common.Objects.TYPE_HD.HD_MYTV} WHERE KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_MYTV} 
                         SELECT NEWID() AS ID,KYHOADON,ID AS MYTV_ID,NEWID() AS DBKH_ID,TYPE_BILL,USERNAME AS MA_TB,PACKCD AS TOC_DO,1 AS TT_THANG,
                         {obj.day_in_month} AS NGAY_TB,{obj.day_in_month} AS NGAY_TB_PTTB,0 AS GOICUOCID,0 AS TH_THANG,0 AS TH_HUY,0 AS DUPECOUNT,0 AS ISDATMOI,
                         0 AS ISHUY,0 AS ISTTT,0 AS ISDATCOC,PAYTV_FEE,SUB_FEE,DISCOUNT AS GIAM_TRU,0 AS TONG_TTT,0 AS TONG_DC,0 AS TONG_IN,TOTAL_FEE AS TONG,ROUND(TOTAL_FEE*0.1,0) AS VAT,ROUND(TOTAL_FEE*1.1,0) AS TONGCONG,
                         NULL AS SIGNDATE,NULL AS REGISTDATE,USE_DATE AS NGAY_SD,SUSPENDATE AS NGAY_KHOA,RESUMEDATE AS NGAY_MO,STOP_DATE AS NGAY_KT 
                         FROM {Common.Objects.TYPE_HD.MYTV} WHERE KYHOADON={obj.KYHD} AND TOTAL_FEE>0";
                SQLServer.Connection.Query(qry);
                qry = $"UPDATE hd SET hd.DBKH_ID=tt.ID FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} tt ON hd.MA_TB=tt.MA_TB WHERE hd.TYPE_BILL=tt.TYPE_BILL AND tt.FIX=0 AND tt.FLAG=1 AND hd.KYHOADON={obj.KYHD}";
                //qry = $"UPDATE a SET a.DBKH_ID=b.ID FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.MA_TB=b.MA_TB WHERE a.TYPE_BILL=b.TYPE_BILL AND b.FIX=0 AND b.FLAG=1";
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
                qry = $"UPDATE THANHTOANTRUOC SET THUC_TRU=0 WHERE TYPE_BILL IN({TYPE_BILL}) AND KYHOADON={obj.KYHD}";
                //
                qry = $"select * from {Common.Objects.TYPE_HD.HD_MYTV} where KYHOADON='{obj.KYHD}'";
                var data = SQLServer.Connection.Query<Models.HD_MYTV>(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật dữ liệu thành công - {data.Count()} thuê bao" }, JsonRequestBehavior.AllowGet);
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
            var TYPE_BILL = 8;
            try
            {
                //var qry = $"update tv SET tv.GOICUOCID=thdvnet.GOI_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} tv inner join (select * from DANHBA_GOICUOC_TICHHOP where goicuoc_id in (select thdv.goicuoc_id from HD_NET net,DANHBA_GOICUOC_TICHHOP thdv where net.MA_TB=thdv.MA_TB and (net.TYPE_BILL=6 or net.TYPE_BILL=9) and net.KYHOADON={obj.KYHD} and thdv.KYHOADON={obj.KYHD} AND thdv.FIX=0)) thdvnet on tv.MA_TB=thdvnet.MA_TB where tv.TYPE_BILL in({TYPE_BILL}) and tv.KYHOADON={obj.KYHD} and thdvnet.NGAY_KT>=CAST('{obj.block_time}' as datetime)";
                //SQLServer.Connection.Query(qry);

                //qry = $"update tv SET tv.GOICUOCID=thdvnet.GOI_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} tv inner join (select * from DANHBA_GOICUOC_TICHHOP where goicuoc_id in (select thdv.goicuoc_id from HD_NET net,DANHBA_GOICUOC_TICHHOP thdv where net.MA_TB=thdv.MA_TB and (net.TYPE_BILL=6 or net.TYPE_BILL=9) and net.KYHOADON={obj.KYHD} and thdv.KYHOADON={obj.KYHD} AND thdv.FIX=0)) thdvnet on tv.MA_TB=thdvnet.MA_TB where tv.TYPE_BILL in({TYPE_BILL}) and tv.KYHOADON={obj.KYHD} and thdvnet.NGAY_BD<CAST('{obj.block_time}' as datetime) AND thdvnet.NGAY_KT IS NULL";
                //SQLServer.Connection.Query(qry);

                var qry = $@"UPDATE hd SET hd.GOICUOCID=thdv.GOI_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.MA_TB=thdv.MA_TB AND thdv.NGAY_KT>=CAST('{obj.block_time}' as datetime) AND hd.TYPE_BILL in({TYPE_BILL}) AND hd.KYHOADON={obj.KYHD} AND thdv.KYHOADON={obj.KYHD} AND thdv.FIX=0 AND thdv.FLAG=1;
                             UPDATE hd SET hd.GOICUOCID=thdv.GOI_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.MA_TB=thdv.MA_TB AND thdv.NGAY_DK<CAST('{obj.block_time}' as datetime) AND thdv.NGAY_KT IS NULL AND hd.TYPE_BILL in({TYPE_BILL}) AND hd.KYHOADON={obj.KYHD} AND thdv.KYHOADON={obj.KYHD} AND thdv.FIX=0 AND thdv.FLAG=1;";
                SQLServer.Connection.Query(qry);
                //Xử lý tích hợp thêm
                qry = $"UPDATE tv SET tv.GOICUOCID=thdv.GOI_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} tv INNER JOIN DANHBA_GOICUOC_TICHHOP thdv ON tv.MA_TB=thdv.MA_TB WHERE tv.TYPE_BILL in({TYPE_BILL}) AND tv.KYHOADON={obj.KYHD} AND thdv.DICHVUVT_ID in({TYPE_BILL}) AND thdv.FIX=1 AND thdv.FLAG=1 AND thdv.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật giá từ bảng giá đối với thuê bao tích hợp
                qry = $@"UPDATE hd SET hd.TONG=bg.GIA+hd.PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN BGCUOC bg ON hd.GOICUOCID=bg.GOICUOCID WHERE hd.GOICUOCID>0 AND hd.TYPE_BILL in({TYPE_BILL}) AND hd.KYHOADON={obj.KYHD} AND bg.LOAITB_ID in({TYPE_BILL}) AND bg.FLAG=1";
                SQLServer.Connection.Query(qry);
                //Cập nhật thuê bao tích hợp không tròn tháng
                //qry = $@"UPDATE a SET a.TONG=b.TOTAL_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN {Common.Objects.TYPE_HD.MYTV} b ON a.MYTV_ID=b.ID WHERE a.GOICUOCID>0 AND (a.NGAY_KHOA is not null or a.NGAY_MO is not null or a.NGAY_KT is not null) AND a.TYPE_BILL in({TYPE_BILL}) AND a.KYHOADON={obj.KYHD}";
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONG=PAYTV_FEE+SUB_FEE,GOICUOCID=-1 WHERE GOICUOCID>0 AND (PAYTV_FEE+SUB_FEE)<TONG AND (NGAY_KHOA is not null or NGAY_MO is not null or NGAY_KT is not null) AND TYPE_BILL in({TYPE_BILL}) AND KYHOADON={obj.KYHD};
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONG=PAYTV_FEE+SUB_FEE,GOICUOCID=-1 WHERE GOICUOCID>0 AND (PAYTV_FEE+SUB_FEE)<TONG AND TYPE_BILL in({TYPE_BILL}) AND FORMAT(NGAY_SD,'MM/yyyy')='{obj.month_year_time}' AND KYHOADON={obj.KYHD};";
                // Cập nhật MyTV GD giảm 20%
                //qry = $"UPDATE hd set hd.TONG=(hd.SUB_FEE*0.8)+PAYTV_FEE from HD_MYTV hd,DANHBA_GOICUOC_TICHHOP th where hd.MA_TB=th.MA_TB and hd.TYPE_BILL in({TYPE_BILL}) and th.LOAITB_ID in({TYPE_BILL}) and th.EXTRA_TYPE=1 and hd.KYHOADON={obj.KYHD} and th.KYHOADON={obj.KYHD}";
                //SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL in({TYPE_BILL}) AND KYHOADON={obj.KYHD};
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE TYPE_BILL in({TYPE_BILL}) AND KYHOADON={obj.KYHD};";
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
                qry = $"UPDATE hd SET hd.TONG=round(hd.TONG*((100-dc.VALUE)/100),0) FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.MA_TB=dc.MA_TB WHERE hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD} AND dc.KYHOADON={obj.KYHD} AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.PERCENT} AND ((hd.KYHOADON>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.KYHOADON BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                SQLServer.Connection.Query(qry);
                //MONEY
                qry = $"UPDATE hd SET hd.TONG=hd.TONG-dc.VALUE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.MA_TB=dc.MA_TB WHERE hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD} AND dc.KYHOADON={obj.KYHD} AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MONEY} AND ((hd.KYHOADON>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.KYHOADON BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                SQLServer.Connection.Query(qry);
                //FIX
                qry = $"UPDATE hd SET hd.TONG=dc.VALUE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.MA_TB=dc.MA_TB WHERE hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD} AND dc.KYHOADON={obj.KYHD} AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.FIX} AND ((hd.KYHOADON>=dc.NGAY_BD AND dc.NGAY_KT IS NULL) OR (hd.KYHOADON BETWEEN dc.NGAY_BD AND dc.NGAY_KT));";
                SQLServer.Connection.Query(qry);
                //Gói GD
                qry = $"UPDATE dc SET dc.FLAG=0 FROM DISCOUNT dc WHERE dc.KYHOADON={obj.KYHD} AND dc.TYPEID=5 AND dc.MA_TB IN(SELECT DISTINCT th.MA_TB FROM DANHBA_GOICUOC_TICHHOP th,BGCUOC bg WHERE th.GOI_ID=bg.GOICUOCID and th.DICHVUVT_ID=8 AND th.KYHOADON={obj.KYHD} AND th.NGAY_KT IS NULL)";
                SQLServer.Connection.Query(qry);
                qry = $"UPDATE hd SET hd.TONG=((hd.TONG-PAYTV_FEE)*((100-dc.VALUE)/100))+PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.MA_TB=dc.MA_TB WHERE hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD} AND dc.KYHOADON={obj.KYHD} AND dc.FLAG=1 AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MYTVGD};";
                //qry += $"UPDATE hd SET hd.TONG=TONG+GIAM_TRU-PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.MA_TB=dc.MA_TB WHERE hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD} AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MYTVGD};";
                //qry += $"UPDATE hd SET hd.GIAM_TRU=(dc.VALUE/100)*TONG FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.MA_TB=dc.MA_TB WHERE hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD} AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MYTVGD};";
                //qry += $"UPDATE hd SET hd.TONG=TONG-GIAM_TRU+PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN DISCOUNT dc ON hd.MA_TB=dc.MA_TB WHERE hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD} AND dc.TYPEID={(int)Common.Objects.TYPE_DISCOUNT.MYTVGD};";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL=8 AND KYHOADON={obj.KYHD};
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE TYPE_BILL=8 AND KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật khuyến mại thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyKhuyenMaiHeThong(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                //Tỷ lệ cước thuê bao
                var qry = $"update hd set hd.tong=round((hd.tong-hd.paytv_fee)*((100-km.tyle_ctb)/100),0)+hd.paytv_fee from {Common.Objects.TYPE_HD.HD_MYTV} hd,DB_KHUYEN_MAI km where hd.MA_TB=km.ma_tb and km.tyle_ctb>0 and km.KYHOADON={obj.KYHD} and hd.KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);
                //Tỷ lệ cước sử dụng
                qry = $"update hd set hd.tong=round(hd.paytv_fee*((100-km.tyle_csd)/100),0)+(hd.tong-hd.paytv_fee) from {Common.Objects.TYPE_HD.HD_MYTV} hd,DB_KHUYEN_MAI km where hd.MA_TB=km.ma_tb and km.tyle_csd>0 and km.KYHOADON={obj.KYHD} and hd.KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);
                //Cước thuê bao
                qry = $"update hd set hd.tong=hd.TONG-km.cuoc_tb from {Common.Objects.TYPE_HD.HD_MYTV} hd,DB_KHUYEN_MAI km where hd.MA_TB=km.ma_tb and km.cuoc_tb>0 and km.KYHOADON={obj.KYHD} and hd.KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);
                //Cước sử dụng
                qry = $"update hd set hd.tong=hd.tong-km.cuoc_sd from {Common.Objects.TYPE_HD.HD_MYTV} hd,DB_KHUYEN_MAI km where hd.MA_TB=km.ma_tb and km.cuoc_sd>0 and km.KYHOADON={obj.KYHD} and hd.KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);
                //Tiền thuê bao
                qry = $"update hd set hd.tong=hd.tong-km.tien_tb from {Common.Objects.TYPE_HD.HD_MYTV} hd,DB_KHUYEN_MAI km where hd.MA_TB=km.ma_tb and km.tien_tb>0 and km.KYHOADON={obj.KYHD} and hd.KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);
                //Tiền sử dụng
                qry = $"update hd set hd.tong=hd.tong-km.tien_sd from {Common.Objects.TYPE_HD.HD_MYTV} hd,DB_KHUYEN_MAI km where hd.MA_TB=km.ma_tb and km.tien_sd>0 and km.KYHOADON={obj.KYHD} and hd.KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL=8 AND KYHOADON={obj.KYHD};
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE TYPE_BILL=8 AND KYHOADON={obj.KYHD};";
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
            obj.data_value = "TNS082";
            var FixPrice = 122727;
            try
            {
                //Tích hợp
                var qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONG={FixPrice} WHERE MA_CBT={obj.data_id} AND GOICUOCID>0 AND TYPE_BILL=9 AND KYHOADON={obj.KYHD};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG/10,0) WHERE MA_CBT={obj.data_id} AND GOICUOCID>0 AND TYPE_BILL=9 AND KYHOADON={obj.KYHD};
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE MA_CBT={obj.data_id} AND GOICUOCID>0 AND TYPE_BILL=9 AND KYHOADON={obj.KYHD};";
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
            //var TYPE_BILL = 9002;
            var DVVT_ID = "8";
            //var PayTV = "203,213";
            try
            {
                var qry = $"update hd set hd.TONG_TTT=dc.CUOC_DC+round(dc.TIEN_SD*1.1,0),ISTTT=1 from {Common.Objects.TYPE_HD.HD_MYTV} hd, DATCOC dc where hd.MA_TB=dc.ma_tb and dc.LOAITB_ID in({DVVT_ID}) and hd.KYHOADON={obj.KYHD} and dc.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                // Fix TTT 1 đồng
                qry = $"update hd set hd.TONG_TTT=TONG_TTT+1 from {Common.Objects.TYPE_HD.HD_MYTV} hd where hd.ISTTT=1 and TONG_TTT%100=99 and hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                // 
                qry = $"update hd set hd.TONGCONG=hd.TONGCONG-hd.TONG_TTT from {Common.Objects.TYPE_HD.HD_MYTV} hd where hd.ISTTT=1 and hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //// Lẻ 1 đồng
                //qry = $"update hd set hd.TONGCONG=0,hd.TONG_TTT=hd.TONG_TTT+hd.TONGCONG from {Common.Objects.TYPE_HD.HD_MYTV} hd where hd.ISTTT=1 and hd.TONGCONG=1 and hd.KYHOADON={obj.KYHD}";
                //SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND((TONGCONG/1.1)*0.1,0),TONG=round(TONGCONG/1.1,0) WHERE TYPE_BILL=8 AND KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật thanh toán trước thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        //[HttpPost, ValidateAntiForgeryToken]
        //public JsonResult XuLyThanhToanTruoc(Common.DefaultObj obj)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    obj.DataSource = Common.Directories.HDDataSource;
        //    obj = getDefaultObj(obj);
        //    //
        //    var TYPE_BILL = 9002;
        //    var DVVT_ID = "8";
        //    var PayTV = "203,213";
        //    try
        //    {
        //        //-- Thông tin --
        //        //TTT Tiền thanh toán trước
        //        //KT Khuyến mại tặng cước (tháng)
        //        //CK Tiền chiết khấu

        //        //-- Trạng thái Flag --
        //        //0: Trạng thái ban đầu chưa xử lý
        //        //1: TTT sau khi trừ có số dư tổng > 0
        //        //2: TTT sau khi trừ có số dư tổng < 0
        //        //3: TTT sau khi trừ có số dư tổng < 0 và không có KT hoặc CK (Trả lại tiền thiếu vào hóa đơn)
        //        //4: KT hoặc CK sau khi trừ có số dư tổng > 0
        //        //5: KT hoặc CK sau khi trừ có số dư tổng < 0 (Trả lại tiền thiếu vào hóa đơn)
        //        //6: KT hoặc CK không còn TTT có số dư tổng > 0
        //        //7: KT hoặc CK không còn TTT có số dư tổng < 0 (Trả lại tiền thiếu vào hóa đơn)
        //        //8: KT hoặc CK khi TTT có số dư tổng > 0

        //        //Đặt lại đầu vào
        //        var qry = $@"UPDATE ttt SET ttt.FLAG=0 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD};
        //                     UPDATE ttt SET ttt.FLAG=-1 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.SODU<0;";
        //        SQLServer.Connection.Query(qry);
        //        //Check Thuê bao thanh toán trước không có trong hóa đơn
        //        qry = $"UPDATE ttt SET ttt.FLAG=-1 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.KYHOADON={obj.KYHD} AND ttt.MA_TB NOT IN(SELECT MA_TB FROM {Common.Objects.TYPE_HD.HD_MYTV} WHERE TYPE_BILL={DVVT_ID} AND KYHOADON={obj.KYHD});";
        //        SQLServer.Connection.Query(qry);

        //        //Bước 1: Xử lý các thuê bao còn TTT và (KT hoặc CK)
        //        //Cập nhật cước từ hóa đơn
        //        qry = $@"UPDATE ttt SET ttt.FLAG=1,ttt.TONG=hd.TONG,ttt.EXTRA_TONG=PAYTV_FEE,SODU_TONG=ttt.SODU-(hd.TONG-hd.PAYTV_FEE) FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=0 AND ttt.KYHOADON={obj.KYHD} AND hd.TYPE_BILL={DVVT_ID} AND hd.KYHOADON={obj.KYHD};";
        //        SQLServer.Connection.Query(qry);
        //        //Pay TV VTVCab 38000
        //        // qry = $@"UPDATE ttt SET ttt.FLAG=1,ttt.TONG=hd.TONG,ttt.EXTRA_TONG=hd.PAYTV_FEE,SODU_TONG=ttt.SODU-(hd.TONG-hd.PAYTV_FEE+38000) FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=1 AND ttt.ID_CV IN ({PayTV}) AND ttt.KYHOADON={obj.KYHD} AND hd.TYPE_BILL={DVVT_ID} AND hd.KYHOADON={obj.KYHD};";
        //        qry = $@"UPDATE ttt SET ttt.FLAG=1,ttt.TONG=hd.TONG,ttt.EXTRA_TONG=hd.PAYTV_FEE,SODU_TONG=ttt.SODU-(hd.TONG) FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=1 AND ttt.ID_CV IN ({PayTV}) AND ttt.KYHOADON={obj.KYHD} AND hd.TYPE_BILL={DVVT_ID} AND hd.KYHOADON={obj.KYHD};";
        //        SQLServer.Connection.Query(qry);

        //        //Cập nhật thực trừ cho TTT có số dư tổng > 0
        //        qry = $@"UPDATE ttt SET ttt.THUC_TRU=ttt.TONG-ttt.EXTRA_TONG FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=1;";
        //        SQLServer.Connection.Query(qry);
        //        //Pay TV VTVCab 38000
        //        qry = $@"UPDATE ttt SET ttt.THUC_TRU=ttt.TONG FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.ID_CV IN ({PayTV}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=1;";
        //        SQLServer.Connection.Query(qry);

        //        //Cập nhật thực trừ bằng số dư đối với các TTT có số dư tổng < 0
        //        qry = $@"UPDATE ttt SET ttt.FLAG=2,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=1 AND ttt.SODU_TONG<0;";
        //        SQLServer.Connection.Query(qry);
        //        //Cập nhật trạng thái cho TTT sau khi trừ có số dư tổng < 0 và không có KT hoặc CK
        //        qry = $@"UPDATE ttt SET ttt.FLAG=3 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=2 AND ttt.MA_TB NOT IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN!='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID={DVVT_ID} AND KYHOADON={obj.KYHD});";
        //        SQLServer.Connection.Query(qry);
        //        //Cập nhật trạng thái cho KT hoặc CK khi TTT có số dư tổng < 0
        //        qry = $@"UPDATE ttt SET ttt.FLAG=2 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.FLAG=0 AND ttt.KYHOADON={obj.KYHD} AND ttt.MA_TB IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID={DVVT_ID} AND FLAG=2 AND KYHOADON={obj.KYHD});";
        //        SQLServer.Connection.Query(qry);
        //        //Cập nhật trạng thái cho KT hoặc CK khi TTT có số dư tổng > 0
        //        qry = $@"UPDATE ttt SET ttt.FLAG=8 FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=0 AND ttt.MA_TB IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE KHOANTIEN='TTT' AND TYPE_BILL={TYPE_BILL} AND DVVT_ID={DVVT_ID} AND KYHOADON={obj.KYHD} AND FLAG=1);";
        //        SQLServer.Connection.Query(qry);
        //        //Cập nhật trạng thái, tổng, thực trừ bằng số tiền còn thiếu cho KT hoặc CK sau khi trừ TTT và số du tổng sau khi trừ
        //        qry = $@"UPDATE ttt SET ttt.FLAG=4,ttt.THUC_TRU=ttt2.SODU_TONG*-1,ttt.TONG=ttt2.TONG,ttt.SODU_TONG=ttt.SODU+ttt2.SODU_TONG FROM THANHTOANTRUOC ttt,THANHTOANTRUOC ttt2 WHERE ttt.MA_TB=ttt2.MA_TB AND ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=2 AND ttt2.KHOANTIEN='TTT' AND ttt2.TYPE_BILL={TYPE_BILL} AND ttt2.DVVT_ID={DVVT_ID} AND ttt2.KYHOADON={obj.KYHD} AND ttt2.FLAG=2;";
        //        SQLServer.Connection.Query(qry);
        //        //Cập nhật trạng thái và thực trừ bằng số dư đối với KT hoặc CK có số dư tổng < 0
        //        qry = $@"UPDATE ttt SET ttt.FLAG=5,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=4 AND ttt.SODU_TONG<0;";
        //        SQLServer.Connection.Query(qry);

        //        //Bước 2: Xử lý các thuê bao không còn TTT chỉ còn KT hoặc CK
        //        //Cập nhật trạng thái, thực trừ, cước từ hóa đơn
        //        qry = $@"UPDATE ttt SET ttt.FLAG=6,ttt.TONG=hd.TONG,ttt.EXTRA_TONG=PAYTV_FEE,SODU_TONG=ttt.SODU-(hd.TONG-hd.PAYTV_FEE),ttt.THUC_TRU=hd.TONG-hd.PAYTV_FEE FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=0 AND hd.TYPE_BILL={DVVT_ID} AND hd.KYHOADON={obj.KYHD};";
        //        SQLServer.Connection.Query(qry);
        //        //Cập nhật trạng thái, thực trừ cho KT hoặc CK có số dư tổng < 0
        //        qry = $@"UPDATE ttt SET ttt.FLAG=7,ttt.THUC_TRU=SODU FROM THANHTOANTRUOC ttt WHERE ttt.KHOANTIEN!='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG=6 AND ttt.SODU_TONG<0;";
        //        SQLServer.Connection.Query(qry);

        //        //Bước 3: Cập nhật thực trừ vào hóa đơn
        //        //Cập nhật các thuê bao có số dư tổng > 0
        //        qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=hd.TONG-hd.PAYTV_FEE,hd.TONG=PAYTV_FEE FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG IN(1,4,6) AND hd.TYPE_BILL={DVVT_ID} AND hd.KYHOADON={obj.KYHD};";
        //        SQLServer.Connection.Query(qry);
        //        //Pay TV VTVCab 38000
        //        qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.THUC_TRU,hd.TONG=0 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG IN(1,4,6) AND ttt.ID_CV IN ({PayTV}) AND hd.TYPE_BILL={DVVT_ID} AND hd.KYHOADON={obj.KYHD};";
        //        SQLServer.Connection.Query(qry);

        //        //Cập nhật các thuê bao có số dư tổng < 0
        //        qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.TONG+SODU_TONG,hd.TONG=ttt.SODU_TONG*-1 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG IN(3,5,7) AND hd.TYPE_BILL={DVVT_ID} AND hd.KYHOADON={obj.KYHD};";
        //        SQLServer.Connection.Query(qry);
        //        //Pay TV VTVCab 38000
        //        //qry = $@"UPDATE hd SET hd.ISTTT=1,hd.TONG_TTT=ttt.TONG+SODU_TONG,hd.TONG=ttt.SODU_TONG*-1 FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.TYPE_BILL={TYPE_BILL} AND ttt.DVVT_ID IN({DVVT_ID}) AND ttt.KYHOADON={obj.KYHD} AND ttt.FLAG IN(3,5,7) AND ttt.ID_CV=203 AND hd.TYPE_BILL={DVVT_ID} AND hd.KYHOADON={obj.KYHD};";
        //        //SQLServer.Connection.Query(qry);

        //        //PayTv VTV Cap
        //        //qry = $"UPDATE a SET a.TONG=a.TONG+b.TIENHANMUC FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN THANHTOANTRUOC b ON a.MA_TB=b.MA_TB WHERE b.KHOANTIEN='KT' AND b.TIENHANMUC=-38000 AND a.KYHOADON={obj.KYHD} AND FORMAT(b.KYHOADON,'MM/yyyy')='{obj.month_year_time}';";
        //        //SQLServer.Connection.Query(qry);
        //        //Cập nhật vat và tổng
        //        qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE ISTTT=1 AND TYPE_BILL={DVVT_ID} AND KYHOADON={obj.KYHD};
        //                 UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE ISTTT=1 AND TYPE_BILL={DVVT_ID} AND KYHOADON={obj.KYHD};";
        //        SQLServer.Connection.Query(qry);
        //        return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật thanh toán trước thành công!" }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        //}
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
                var qry = $"UPDATE ttt SET ttt.FLAG=1 FROM THANHTOANTRUOC ttt WHERE ttt.TYPE_BILL={TYPE_BILL} AND CONVERT(datetime,CONVERT(varchar,ttt.KYHOADON))>=ttt.NGAY_BD AND CONVERT(datetime,CONVERT(varchar,ttt.KYHOADON))<ttt.NGAY_KT AND ttt.DVVT_ID in(8) AND ttt.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);

                //qry = $"UPDATE ttt SET ttt.THUC_TRU=hd.TONG-hd.PAYTV_FEE FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND hd.KYHOADON>=ttt.NGAY_BD AND hd.KYHOADON<ttt.NGAY_KT AND ttt.KYHOADON={obj.KYHD} AND hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD}";
                qry = $"UPDATE ttt SET ttt.THUC_TRU=hd.TONGCONG FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND CONVERT(datetime,CONVERT(varchar,hd.KYHOADON))>=ttt.NGAY_BD AND CONVERT(datetime,CONVERT(varchar,hd.KYHOADON))<ttt.NGAY_KT AND ttt.KYHOADON={obj.KYHD} AND hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry, null, null, true, 3000);
                qry = $"UPDATE hd SET hd.TONG_TTT=hd.TONGCONG FROM THANHTOANTRUOC ttt INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON ttt.MA_TB=hd.MA_TB WHERE ttt.KHOANTIEN='TTT' AND ttt.TYPE_BILL={TYPE_BILL} AND CONVERT(datetime,CONVERT(varchar,ttt.KYHOADON))>=ttt.NGAY_BD AND CONVERT(datetime,CONVERT(varchar,ttt.KYHOADON))<ttt.NGAY_KT AND ttt.KYHOADON={obj.KYHD} AND hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //qry = $"UPDATE DATCOC SET SODU=TONGHANMUC-THUC_TRU WHERE KYHOADON={obj.KYHD}";
                // SQLServer.Connection.Query(qry);
                //qry = $"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET ISTTT=1,TONG=PAYTV_FEE WHERE MA_TB IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE TYPE_BILL={TYPE_BILL} AND KYHOADON>=NGAY_BD AND KYHOADON<NGAY_KT AND KYHOADON={obj.KYHD}) AND TYPE_BILL=8 AND KYHOADON={obj.KYHD};";
                qry = $"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET ISTTT=1,TONG=0 WHERE MA_TB IN(SELECT MA_TB FROM THANHTOANTRUOC WHERE TYPE_BILL={TYPE_BILL} AND CONVERT(datetime,CONVERT(varchar,KYHOADON))>=NGAY_BD AND CONVERT(datetime,CONVERT(varchar,KYHOADON))<NGAY_KT AND KYHOADON={obj.KYHD}) AND TYPE_BILL=8 AND KYHOADON={obj.KYHD};";
                SQLServer.Connection.Query(qry);

                ////PayTv VTV Cap
                //qry = $"UPDATE a SET a.TONG=a.TONG+b.TIENHANMUC FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN THANHTOANTRUOC b ON a.MA_TB=b.MA_TB WHERE b.KHOANTIEN='KT' AND b.TIENHANMUC=-38000 AND a.KYHOADON={obj.KYHD} AND FORMAT(b.KYHOADON,'MM/yyyy')='{obj.month_year_time}';";
                //SQLServer.Connection.Query(qry);

                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG*0.1,0) WHERE ISTTT=1 AND TYPE_BILL=8 AND KYHOADON={obj.KYHD};
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE ISTTT=1 AND TYPE_BILL=8 AND KYHOADON={obj.KYHD};";
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
                //var qry = $"UPDATE THANHTOANTRUOC SET FLAG=1 WHERE TYPE_BILL={TYPE_BILL} AND MA_TB IN(SELECT MA_TB FROM {Common.Objects.TYPE_HD.HD_MYTV}) AND KYHOADON={obj.KYHD}";
                //SQLServer.Connection.Query(qry);

                //Check số dư nhỏ hơn 0
                //qry = $"UPDATE THANHTOANTRUOC SET FLAG=0 WHERE TYPE_BILL={TYPE_BILL} AND SODU<1 AND KYHOADON={obj.KYHD}";
                //SQLServer.Connection.Query(qry);
                //Đặt lại đầu vào
                qry = $"UPDATE dc SET dc.FLAG=0 FROM THANHTOANTRUOC dc WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật cước từ hóa đơn
                qry = $"UPDATE dc SET dc.FLAG=3,dc.SODU_TONG=dc.SODU FROM THANHTOANTRUOC dc INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON dc.MA_TB=hd.MA_TB WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=0 AND dc.KYHOADON={obj.KYHD} AND hd.ISTTT=1 AND hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật cước từ hóa đơn
                qry = $"UPDATE dc SET dc.FLAG=1,dc.TONG=hd.TONGCONG,dc.EXTRA_TONG=hd.PAYTV_FEE,dc.SODU_TONG=dc.SODU-hd.TONGCONG FROM THANHTOANTRUOC dc INNER JOIN {Common.Objects.TYPE_HD.HD_MYTV} hd ON dc.MA_TB=hd.MA_TB WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=0 AND dc.KYHOADON={obj.KYHD} AND hd.ISTTT=0 AND hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật thực trừ bằng tổng
                qry = $"UPDATE dc SET dc.THUC_TRU=dc.TONG FROM THANHTOANTRUOC dc WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=1 AND dc.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật thực trưc bằng số dư khi SODU_TONG<0
                qry = $"UPDATE dc SET dc.FLAG=2,dc.THUC_TRU=dc.SODU FROM THANHTOANTRUOC dc WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=1 AND dc.SODU_TONG<0 AND dc.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại hóa đơn khi SODU_TONG>=0
                qry = $"UPDATE hd SET hd.ISDATCOC=1,hd.TONG_DC=dc.THUC_TRU,hd.TONG=0,hd.VAT=0,hd.TONGCONG=0 FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN THANHTOANTRUOC dc ON hd.MA_TB=dc.MA_TB WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=1 AND dc.KYHOADON={obj.KYHD} AND hd.ISTTT=0 AND hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại hóa đơn khi SODU_TONG<0
                qry = $"UPDATE hd SET hd.ISDATCOC=1,hd.TONG_DC=dc.THUC_TRU,hd.TONGCONG=dc.SODU_TONG*-1 FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN THANHTOANTRUOC dc ON hd.MA_TB=dc.MA_TB WHERE dc.TYPE_BILL={TYPE_BILL} AND dc.FLAG=2 AND dc.KYHOADON={obj.KYHD} AND hd.ISTTT=0 AND hd.TYPE_BILL=8 AND hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại hóa đơn TONG
                qry = $"UPDATE hd SET hd.TONG=ROUND(hd.TONGCONG/1.1,0) FROM {Common.Objects.TYPE_HD.HD_MYTV} hd WHERE hd.TYPE_BILL=8 AND hd.ISDATCOC=1 AND hd.TONG>0 AND hd.KYHOADON={obj.KYHD}";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại hóa đơn VAT
                qry = $"UPDATE hd SET hd.VAT=ROUND(hd.TONG*0.1,0) FROM {Common.Objects.TYPE_HD.HD_MYTV} hd WHERE hd.TYPE_BILL=8 AND hd.ISDATCOC=1 AND hd.TONG>0 AND hd.KYHOADON={obj.KYHD}";
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
            obj.KYHD = int.Parse(obj.datetime.ToString("yyyyMM") + "01");
            return obj;
        }
    }
}