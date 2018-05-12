using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Dapper.Contrib.Extensions;
using TM.Helper;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class TSLController : BaseController
    {
        // GET: TSL
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateContactData(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "4";
            try
            {
                //Get DB PTTB
                var qry = $"SELECT kh.KHACHHANG_ID,kh.MA_KH,a.THANHTOAN_ID,tt.MA_TT AS MA_TT_HNI,tt.MAPHO_ID,b.DICHVUVT_ID,tt.TEN_TT,tt.DIACHI_TT,tt.DIENTHOAI_TT AS DIENTHOAI,tt.MST AS MS_THUE,tt.MA_TUYENTHU AS MA_TUYEN,tt.DONVIQL_ID,a.DOITUONG_ID AS MA_DT,tttb.TRANGTHAITB_ID AS TH_SD,a.MA_TB,a.MA_TB AS ACCOUNT,a.DOITUONG_ID,a.GHICHU,a.LOAIHINHTB_ID,a.TRANGTHAITB_ID,a.TBDAYCHUNG_ID,a.NGAY_TRANGTHAITB AS NGAY_TTTB,a.NGAY_SUDUNG AS NGAY_SD,a.NGAY_CN,a.NGAY_HT,a.NGAY_CAT,qh.MA_QUANHUYEN AS MA_DVI,b.MA_LHTB FROM DB_THUEBAO_BKN a,DB_THANHTOAN_BKN tt,DB_KHACHHANG_BKN kh,LOAIHINH_TB_BKN b,TRANGTHAI_TB_BKN tttb, MA_PHO_BKN mp,PHUONG_XA_BKN px,QUAN_HUYEN_BKN qh WHERE a.THANHTOAN_ID=tt.THANHTOAN_ID AND tt.KHACHHANG_ID=kh.KHACHHANG_ID AND a.LOAIHINHTB_ID=b.LOAIHINHTB_ID AND tt.MAPHO_ID=mp.MAPHO_ID AND mp.PHUONGXA_ID=px.PHUONGXA_ID AND px.QUANHUYEN_ID=qh.QUANHUYEN_ID AND b.DICHVUVT_ID={TYPE_BILL} AND a.TRANGTHAITB_ID=tttb.TRANGTHAITB_ID ORDER BY qh.MA_QUANHUYEN,a.NGAY_CN,a.MA_TB";
                var dbpttb = Oracle.Connection.Query<Models.DANH_BA_TSL>(qry);
                //Get data DB_DUONGTHU_BKN
                qry = $"SELECT * FROM DB_DUONGTHU_BKN";
                var dbdt = SQLServer.Connection.Query<Models.DB_DUONGTHU_BKN>(qry);
                //Get data DB KH
                qry = $"SELECT * FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} WHERE FIX=0 AND FLAG=1";//AND TYPE_BILL IN({TYPE_BILL})
                var dbkh = SQLServer.Connection.Query<Models.DB_THANHTOAN_BKN>(qry);
                //Get data TSL Remove
                qry = $"SELECT * FROM HD_TSL WHERE TYPE_BILL=9641 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                var tslRemove = SQLServer.Connection.Query<Models.HD_TSL>(qry);
                //
                var DataInsert = new List<Models.DB_THANHTOAN_BKN>();
                var DataUpdate = new List<Models.DB_THANHTOAN_BKN>();
                var DataInsertHD = new List<Models.HD_TSL>();
                var _dbkh_id = Guid.Empty;
                foreach (var i in dbpttb)
                {
                    //Xóa dữ liệu cũ
                    qry = $"DELETE HD_TSL WHERE TYPE_BILL IN({TYPE_BILL},-1) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                    SQLServer.Connection.Query(qry);
                    //check data TSL Remove
                    if (tslRemove.Any(d => d.ACCOUNT == i.ACCOUNT)) continue;
                    //Cập nhật hóa đơn TSL
                    var hdtsl = new Models.HD_TSL();
                    hdtsl.ID = Guid.NewGuid();
                    hdtsl.DBKH_ID = _dbkh_id;
                    hdtsl.TYPE_BILL = int.Parse(TYPE_BILL);
                    hdtsl.TIME_BILL = obj.datetime;
                    hdtsl.MAPHO_ID = i.MAPHO_ID;
                    hdtsl.THANHTOAN_ID = i.THANHTOAN_ID;
                    hdtsl.THUEBAO_ID = i.THUEBAO_ID;
                    hdtsl.ACCOUNT = i.ACCOUNT;
                    hdtsl.MA_TB = i.MA_TB;
                    hdtsl.DOITUONG_ID = i.DOITUONG_ID;
                    hdtsl.TOC_DO = "TSL";
                    hdtsl.GHICHU = i.GHICHU;
                    hdtsl.TT_THANG = 1;
                    hdtsl.NGAY_TB = obj.day_in_month;
                    hdtsl.LOAIHINHTB_ID = i.LOAIHINHTB_ID;
                    hdtsl.MA_LHTB = i.MA_LHTB;
                    hdtsl.TRANGTHAITB_ID = i.TRANGTHAITB_ID;
                    hdtsl.TBDAYCHUNG_ID = i.TBDAYCHUNG_ID;
                    hdtsl.GOICUOCID = i.GOICUOCID;
                    //hdtsl.ISDATMOI = 0;
                    //hdtsl.ISHUY = 0;
                    //hdtsl.ISTTT = 0;
                    //hdtsl.ISDATCOC = 0;
                    hdtsl.NGAY_TTTB = i.NGAY_TTTB;
                    hdtsl.NGAY_SD = i.NGAY_SD;
                    hdtsl.NGAY_CN = i.NGAY_CN;
                    hdtsl.NGAY_HT = i.NGAY_HT;
                    hdtsl.NGAY_CAT = i.NGAY_CAT;
                    DataInsertHD.Add(hdtsl);

                    //Cập nhật danh bạ TSL
                    var _tmp = dbkh.FirstOrDefault(d => d.MA_TT_HNI == i.MA_TT_HNI);
                    if (_tmp != null)
                    {
                        if (DataUpdate.Any(d => d.MA_TT_HNI == i.MA_TT_HNI)) continue;
                        if (!string.IsNullOrEmpty(i.MA_KH)) _tmp.MA_KH = i.MA_KH.Trim();
                        if (!string.IsNullOrEmpty(i.MA_TT_HNI)) _tmp.MA_TT_HNI = i.MA_TT_HNI.Trim();
                        if (!string.IsNullOrEmpty(i.TEN_TT)) _tmp.TEN_TT = i.TEN_TT.Trim();
                        if (!string.IsNullOrEmpty(i.DIACHI_TT)) _tmp.DIACHI_TT = i.DIACHI_TT.Trim();
                        if (!string.IsNullOrEmpty(i.DIENTHOAI)) _tmp.DIENTHOAI = i.DIENTHOAI.Trim();
                        //if (!string.IsNullOrEmpty(i.MS_THUE)) _tmp.MS_THUE = i.MS_THUE.Trim();
                        //_tmp.BANKNUMBER = null;
                        if (!string.IsNullOrEmpty(i.MA_DVI)) _tmp.MA_DVI = i.MA_DVI.Trim();
                        if (!string.IsNullOrEmpty(i.MA_TUYEN)) _tmp.MA_TUYEN = i.MA_TUYEN.Trim().ToUpper();
                        //if (!string.IsNullOrEmpty(i.MA_CBT)) _tmp.MA_CBT = i.MA_CBT.Trim();
                        var ma_cbt = dbdt.FirstOrDefault(d => d.MA_DT == _tmp.MA_TUYEN);
                        _tmp.MA_CBT = ma_cbt != null ? ma_cbt.MA_DT_GOC : null;
                        //if (!string.IsNullOrEmpty(i.CUSTCATE)) _tmp.CUSTCATE = i.CUSTCATE.Trim();
                        //_tmp.STK = null;
                        _tmp.DONVIQL_ID = i.DONVIQL_ID;
                        _tmp.KHACHHANG_ID = i.KHACHHANG_ID;
                        _tmp.THANHTOAN_ID = i.THANHTOAN_ID;
                        _tmp.MA_DT = i.MA_DT;
                        _tmp.TH_SD = i.TH_SD;
                        _tmp.ISNULL = 0;
                        _tmp.ISNULLMT = 0;
                        _tmp.FIX = 0;
                        _tmp.FLAG = 1;
                        //Account Json
                        var account_json = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.ACCOUNT_JSON>(_tmp.ACCOUNT);
                        account_json.TSL = i.ACCOUNT;
                        _tmp.ACCOUNT = Newtonsoft.Json.JsonConvert.SerializeObject(account_json);
                        //
                        _dbkh_id = _tmp.ID;
                        DataUpdate.Add(_tmp);
                        //SQLServer.Connection.Update(_tmp);
                    }
                    else
                    {
                        if (DataInsert.Any(d => d.MA_TT_HNI == i.MA_TT_HNI)) continue;
                        var _d = new Models.DB_THANHTOAN_BKN();
                        _d.ID = _dbkh_id = Guid.NewGuid();
                        _d.TYPE_BILL = int.Parse(TYPE_BILL);
                        _d.ACCOUNT = _d.MA_TB = i.MA_TB;
                        if (!string.IsNullOrEmpty(i.MA_KH)) _d.MA_KH = i.MA_KH.Trim();
                        if (!string.IsNullOrEmpty(i.MA_TT_HNI)) _d.MA_TT_HNI = i.MA_TT_HNI.Trim();
                        if (!string.IsNullOrEmpty(i.TEN_TT)) _d.TEN_TT = i.TEN_TT.Trim();
                        if (!string.IsNullOrEmpty(i.TEN_TT)) _d.DIACHI_TT = i.TEN_TT.Trim();
                        if (!string.IsNullOrEmpty(i.DIENTHOAI)) _d.DIENTHOAI = i.DIENTHOAI.Trim();
                        //if (!string.IsNullOrEmpty(i.MA_ST)) _d.MS_THUE = i.MA_ST.Trim();
                        //_tmp.BANKNUMBER = null;
                        if (!string.IsNullOrEmpty(i.MA_DVI)) _d.MA_DVI = i.MA_DVI.Trim();
                        if (!string.IsNullOrEmpty(i.MA_TUYEN)) _d.MA_TUYEN = i.MA_TUYEN.Trim().ToUpper();
                        //if (!string.IsNullOrEmpty(i.MA_CBT)) _d.MA_CBT = i.MA_CBT.Trim();
                        var ma_cbt = dbdt.FirstOrDefault(d => d.MA_DT == _d.MA_TUYEN);
                        _d.MA_CBT = ma_cbt != null ? ma_cbt.MA_DT_GOC : null;
                        //if (!string.IsNullOrEmpty(i.CUSTCATE)) _d.CUSTCATE = i.CUSTCATE.Trim();
                        //_tmp.STK = null;
                        _d.DONVIQL_ID = i.DONVIQL_ID;
                        _d.KHACHHANG_ID = i.KHACHHANG_ID;
                        _d.THANHTOAN_ID = i.THANHTOAN_ID;
                        _d.MA_DT = i.MA_DT;
                        _d.TH_SD = i.TH_SD;
                        _d.ISNULL = 0;
                        _d.ISNULLMT = 0;
                        _d.FIX = 0;
                        _d.FLAG = 1;
                        //Account Json
                        var account_json = new Models.ACCOUNT_JSON();
                        account_json.TSL = i.ACCOUNT;
                        _d.ACCOUNT = Newtonsoft.Json.JsonConvert.SerializeObject(account_json);
                        DataInsert.Add(_d);
                    }
                }
                //
                if (DataInsert.Count > 0) SQLServer.Connection.Insert(DataInsert);
                if (DataUpdate.Count > 0) SQLServer.Connection.Update(DataUpdate);
                //
                if (DataInsertHD.Count > 0) SQLServer.Connection.Insert(DataInsertHD);
                //UPDATE NULL
                qry = $@"UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET ISNULLMT=1,MA_TUYEN=REPLACE(MA_TUYEN,'000','001') WHERE MA_TUYEN LIKE '%000' AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});
                         UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET MA_CBT=CAST(CAST(ma_dvi as varchar)+'01' as int) WHERE ISNULLMT=1 AND FIX=0 AND FLAG=1 AND TYPE_BILL IN({TYPE_BILL});";
                SQLServer.Connection.Query(qry);
                //
                return Json(new { success = $"TSL - Cập nhật: {DataUpdate.Count} - Thêm mới: {DataInsert.Count}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateTTHD(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "4";
            var TTHD_ID_HUY = "10,12,13,14,16,17";
            try
            {
                //Cập nhật tổng
                var qry = $"select a.*,b.TTHD_ID from DB_THUEBAO_BKN a,HD_THUEBAO_BKN b,LOAIHINH_TB_BKN lhtb where a.MA_TB=b.MA_TB and a.LOAIHINHTB_ID=lhtb.LOAIHINHTB_ID and lhtb.DICHVUVT_ID=4";
                var dbpttb = Oracle.Connection.Query<Models.TTHD_TSL>(qry);
                qry = $"SELECT * FROM HD_TSL WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                var data = SQLServer.Connection.Query<Models.HD_TSL>(qry);
                foreach (var i in data)
                {
                    var _tmp = dbpttb.FirstOrDefault(d => d.MA_TB == i.MA_TB);
                    if (_tmp != null)
                    {
                        i.TTHD_ID = _tmp.TTHD_ID;
                    }
                }
                SQLServer.Connection.Update(data);
                //
                qry = $"UPDATE HD_TSL SET TYPE_BILL=-1 WHERE TYPE_BILL={TYPE_BILL} AND (TTHD_ID IN({TTHD_ID_HUY}) OR TTHD_ID IS NULL) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"HD_TSL - Cập nhật trạng thái hợp đồng thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateUseDay(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "4";
            try
            {
                //Cập nhật ngày sử dụng
                var qry = $"UPDATE tsl SET tsl.TT_THANG=2,tsl.NGAY_TB={obj.day_in_month}-DAY(NGAY_SD) FROM HD_TSL tsl WHERE tsl.TYPE_BILL={TYPE_BILL} AND FORMAT(tsl.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(tsl.NGAY_SD,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"HD_TSL - Cập nhật ngày sử dụng!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdatePrice(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "4";
            try
            {
                //Cập nhật tổng
                var qry = $"UPDATE hd SET hd.TONG=bg.GIA FROM HD_TSL hd INNER JOIN BGCUOC bg ON bg.PROFILEIP LIKE '%,'+hd.TOC_DO+',%' WHERE TYPE_BILL={TYPE_BILL} AND bg.GOICUOCID=0 AND TT_THANG=1 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                qry = $"UPDATE hd SET hd.TONG=ROUND((bg.GIA/{obj.day_in_month})*NGAY_TB,0) FROM HD_TSL hd INNER JOIN BGCUOC bg ON bg.PROFILEIP LIKE '%,'+hd.TOC_DO+',%' WHERE TYPE_BILL={TYPE_BILL} AND bg.GOICUOCID=0 AND TT_THANG=2 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE HD_TSL SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE HD_TSL SET TONGCONG=TONG+VAT WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"HD_TSL - Cập nhật giá thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyTichHop(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            var TYPE_BILL = "4";
            try
            {
                var qry = $@"UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM HD_TSL hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.ACCOUNT=thdv.ACCOUNT AND thdv.NGAY_KT>=CAST('{obj.block_time}' as datetime) AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0 AND thdv.FLAG=1;
                             UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM HD_TSL hd,DANHBA_GOICUOC_TICHHOP thdv WHERE hd.ACCOUNT=thdv.ACCOUNT AND thdv.NGAY_BD<CAST('{obj.block_time}' as datetime) AND thdv.NGAY_KT IS NULL AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0 AND thdv.FLAG=1;";
                SQLServer.Connection.Query(qry);
                //Xử lý tích hợp thêm
                qry = $"UPDATE hd SET hd.GOICUOCID=thdv.LOAIGOICUOC_ID FROM HD_TSL hd INNER JOIN DANHBA_GOICUOC_TICHHOP thdv ON hd.ACCOUNT=thdv.ACCOUNT WHERE hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.DICHVUVT_ID=8 AND thdv.FIX=1";
                SQLServer.Connection.Query(qry);
                //Cập nhật giá từ bảng giá đối với thuê bao tích hợp
                qry = $@"UPDATE hd SET hd.TONG=bg.GIA+hd.PAYTV_FEE FROM HD_TSL hd INNER JOIN BGCUOC bg ON hd.GOICUOCID=bg.GOICUOCID WHERE hd.GOICUOCID>0 AND bg.DICHVUVT_ID=8 AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật thuê bao tích hợp không tròn tháng
                //qry = $@"UPDATE HD_TSL SET TONG=PAYTV_FEE+SUB_FEE,GOICUOCID=0 WHERE GOICUOCID>0 AND (PAYTV_FEE+SUB_FEE)<TONG AND (NGAY_KHOA is not null or NGAY_MO is not null or NGAY_KT is not null) AND TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE HD_TSL SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE HD_TSL SET TONGCONG=TONG+VAT WHERE TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MYTV} - Cập nhật tích hợp thành công!" }, JsonRequestBehavior.AllowGet);
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
}