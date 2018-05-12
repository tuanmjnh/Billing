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

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class GeneralProcessController : BaseController
    {
        List<Models.DICHVU_VT_BKN> _dvvt = new List<Models.DICHVU_VT_BKN>();
        public ActionResult Index()
        {
            try
            {
                ////Thực hiện hàm truy vấn thông tin khách hàng
                ////GetSubscriberInfoVO clsGetSubriberVO = new GetSubscriberInfoVO();
                ////clsGetSubriberVO = clswebservice.GetSubscriberInfo("bcntxay");
                ////ViewBag.data = clsGetSubriberVO.Name;
                ////ViewBag.data = ServicesPortalMytv.GetListSubscriber().Data.Tables[0];
                //var GetSubscriberRequest = new Vasc.GetSubscriberRequest();
                ////GetSubscriberRequest.IPTVAccount = "bcntv00068241";//"bcntv00072886";
                ////GetSubscriberRequest.InforDevice = "";
                //TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath("Uploads/Danh_sach_ton_01_07_2017.xls");
                //var dt = TM.OleExcel.ToDataSet().Tables[0];
                //dt.Columns.Add("check");
                //foreach (System.Data.DataRow item in dt.Rows)
                //{
                //    if (item[3] == null) continue;
                //    GetSubscriberRequest.IPTVAccount = item[3].ToString();//"bcntv00072886";
                //    GetSubscriberRequest.InforDevice = "";
                //    var val = ServicesPortalMytv.GetSubscriberInfoV2(GetSubscriberRequest).Data.Tables[0];
                //    if (val.Rows.Count > 0)
                //        item["check"] = val.Rows[0]["STATUS_NAME"];
                //}

                FileManagerController.InsertDirectory(Common.Directories.HDDataSource);
                ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(Common.Directories.HDData).OrderByDescending(d => d).ToList();
            }
            catch (Exception ex) { this.danger(ex.Message); }
            return View();
        }
        //Lấy tích hợp từ PTTB
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult LayTichHop(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
            //Lấy dữ liệu tích hợp ghép
            //select ftth.ACCOUNT as account_net,b.ma_tb as account_mytv,a.MA_TB,a.LOAIGOICUOC_ID,a.TEN_GOICUOC,a.NGAY_BD,a.NGAY_KT,a.DICHVUVT_ID from DANHBA_GOICUOC_TICHHOP a,(select * from DANHBA_GOICUOC_TICHHOP where DICHVUVT_ID=8) b,DB_FTTH_BKN ftth where a.GOICUOC_ID=b.GOICUOC_ID AND ftth.SO_FTTH=a.ma_tb and (a.DICHVUVT_ID=6 or a.DICHVUVT_ID=9)
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                //GET DANHBA_GOICUOC_TICHHOP
                //var qry = "SELECT thdv.*,thdv.ACCOUNT_FTTH AS ACCOUNT FROM DANHBA_GOICUOC_TICHHOP thdv ORDER BY MA_TB ASC,NGAY_BD DESC";
                //var qry = $"SELECT thdv.*,thdv.ACCOUNT_FTTH AS ACCOUNT,lm.* FROM DANHBA_GOICUOC_TICHHOP thdv LEFT JOIN (SELECT a.LOAIHINHTB_ID,a.TEN_LHTB,b.DICHVUVT_ID,b.MA_DVVT,b.TEN_DVVT FROM LOAIHINH_TB_BKN a,DICHVU_VT_BKN b WHERE a.DICHVUVT_ID=b.DICHVUVT_ID) lm on thdv.LOAIMAY_ID=lm.LOAIHINHTB_ID where thdv.NGAY_KT is null or thdv.NGAY_KT>=TO_DATE('{obj.datetime.ToString("yyyy/MM/dd")}', 'YYYY/MM/DD') ORDER BY thdv.MA_TB ASC,thdv.NGAY_BD ASC";
                //var qry = $"SELECT thdv.*,ftth.ACCOUNT AS ACCOUNT FROM DANHBA_GOICUOC_TICHHOP thdv LEFT JOIN TB_FTTH_BKN ftth ON thdv.MA_TB=ftth.SO_FTTH ORDER BY thdv.MA_TB ASC,thdv.NGAY_BD DESC";
                //var qry = $"SELECT thdv.*,ftth.ACCOUNT AS ACCOUNT FROM DANHBA_GOICUOC_TICHHOP thdv LEFT JOIN TB_FTTH_BKN ftth ON thdv.MA_TB=ftth.SO_FTTH WHERE thdv.NGAY_KT IS NULL OR TO_CHAR(thdv.NGAY_KT,'MM/YYYY')='{obj.month_year_time}' ORDER BY thdv.MA_TB ASC,thdv.NGAY_BD DESC";
                var qry = $"SELECT thdv.*,ftth.ACCOUNT AS ACCOUNT FROM DANHBA_GOICUOC_TICHHOP thdv LEFT JOIN TB_FTTH_BKN ftth ON thdv.MA_TB=ftth.SO_FTTH WHERE thdv.NGAY_KT IS NULL OR thdv.NGAY_KT>=TO_DATE('{obj.datetime.ToString("yyyy/MM/dd")}', 'YYYY/MM/DD') ORDER BY thdv.MA_TB ASC,thdv.NGAY_BD DESC";
                var pttb = Oracle.Connection.Query<Models.DANHBA_GOICUOC_TICHHOP>(qry).ToList();
                //UPDATE Information
                foreach (var i in pttb)
                {
                    index++;
                    i.ID = index;
                    i.TIME_BILL = obj.datetime;
                    i.DUPE_COUNT = 1;
                    i.NO_DUPE = 0;
                    i.FIX_NGAY_KT = 0;
                    i.DUPE_FLAG = 0;
                    i.TH_THANG = 0;
                    i.TH_SO_NGAY = obj.day_in_month;
                    i.FIX = 0;
                    i.EXTRA_TYPE = 0;
                    i.FLAG = 1;
                }
                //DELETE DANHBA_GOICUOC_TICHHOP
                //qry = $"DELETE DANHBA_GOICUOC_TICHHOP WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //qry = $"DELETE DANHBA_GOICUOC_TICHHOP";
                qry = $"DELETE DANHBA_GOICUOC_TICHHOP WHERE FIX=0";
                SQLServer.Connection.Query(qry);
                //INSERT DANHBA_GOICUOC_TICHHOP
                SQLServer.Connection.Insert(pttb);
                //UPDATE ACCOUNT
                qry = $@"UPDATE DANHBA_GOICUOC_TICHHOP SET ACCOUNT=MA_TB WHERE ACCOUNT IS NULL AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE DANHBA_GOICUOC_TICHHOP SET EXTRA_TYPE=1 WHERE LOAIGOICUOC_ID IN (SELECT GOICUOCID FROM BGCUOC WHERE EXTRA_TYPE=1) AND FIX=0 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //DataAccess.Connection.SQLServer.ConnectionClose();
                return Json(new { success = "Lấy danh sách tích hợp thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
        }
        //Xử lý hủy tích hợp
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyHuyTichHop(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                ////Cập nhật hủy tích hợp khi mytv hủy
                //var qry = $"UPDATE a SET a.FLAG=0 FROM DANHBA_GOICUOC_TICHHOP a,{Common.Objects.TYPE_HD.HD_MYTV} b where a.ACCOUNT=b.ACCOUNT AND b.TH_SD=0 AND a.FIX=0 AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //SQLServer.Connection.Query(qry);
                ////Cập nhật hủy tích hợp khi internet hủy 
                //qry = $"UPDATE a SET a.FLAG=0 FROM DANHBA_GOICUOC_TICHHOP a,{Common.Objects.TYPE_HD.HD_NET} b where a.MA_TB=b.MA_TB AND b.TH_SD=0 AND a.FIX=0 AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";

                //Cập nhật hủy tích hợp
                var qry = $@"UPDATE a SET a.FLAG=0 FROM DANHBA_GOICUOC_TICHHOP a,{Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b where a.MA_TB=b.MA_TB AND b.TH_SD=0 AND a.FIX=0;
                             UPDATE DANHBA_GOICUOC_TICHHOP SET FLAG=0 WHERE DICHVUVT_ID=8 AND FIX=0 AND EXTRA_TYPE=0 AND ACCOUNT NOT IN (SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN});
                             UPDATE DANHBA_GOICUOC_TICHHOP SET FLAG=0 WHERE DICHVUVT_ID=6 AND FIX=0 AND EXTRA_TYPE=0 AND ACCOUNT NOT IN (SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN});
                             UPDATE DANHBA_GOICUOC_TICHHOP SET FLAG=0 WHERE DICHVUVT_ID=9 AND FIX=0 AND EXTRA_TYPE=0 AND ACCOUNT NOT IN (SELECT ACCOUNT FROM {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN});";
                SQLServer.Connection.Query(qry);
                //Hủy tích hợp
                qry = $"UPDATE DANHBA_GOICUOC_TICHHOP SET FLAG=0 WHERE GOICUOC_ID IN(SELECT GOICUOC_ID from DANHBA_GOICUOC_TICHHOP WHERE FLAG=0 AND FIX=0)";
                SQLServer.Connection.Query(qry);
                return Json(new { success = "Xử lý tích hợp thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Xử lý biến động tích hợp
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyTichHop(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                var qry = "";
                //Xóa Thuê bao đã hủy tích hợp từ tháng trước
                //qry = $"DELETE DANHBA_GOICUOC_TICHHOP WHERE NGAY_KT<CAST('{obj.datetime.ToString("yyyy/MM/dd")}' AS datetime) AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //SQLServer.Connection.Query(qry);
                //Tìm account trùng
                qry = $"UPDATE thdv SET DUPE_COUNT=(SELECT COUNT(*) FROM DANHBA_GOICUOC_TICHHOP WHERE thdv.ACCOUNT=ACCOUNT) FROM DANHBA_GOICUOC_TICHHOP thdv WHERE FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);

                //Xử lý trùng không có ngày kết thúc
                qry = $@"UPDATE a SET a.NO_DUPE=b.count_dupe,FIX_NGAY_KT=1 FROM DANHBA_GOICUOC_TICHHOP a inner join (SELECT ACCOUNT,COUNT(*) count_dupe FROM (SELECT * from DANHBA_GOICUOC_TICHHOP WHERE DUPE_COUNT>1 and NGAY_KT is null AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}') as c group by ACCOUNT,TIME_BILL having COUNT(*)>1 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}') b on a.ACCOUNT=b.ACCOUNT AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE a SET FIX_NGAY_KT=0 FROM DANHBA_GOICUOC_TICHHOP a WHERE NO_DUPE>0 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND NGAY_BD IN (SELECT MAX(NGAY_BD) FROM DANHBA_GOICUOC_TICHHOP WHERE ACCOUNT=a.ACCOUNT AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}');
                         UPDATE a SET NGAY_KT=NGAY_BD FROM DANHBA_GOICUOC_TICHHOP a WHERE FIX_NGAY_KT=1 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                //DELETE DANHBA_GOICUOC_TICHHOP WHERE STT>1 AND NO_DUPE>1 AND NGAY_KT IS NULL;";
                SQLServer.Connection.Query(qry);

                //Tìm thuê bao tích hợp trong tháng
                qry = $@"UPDATE DANHBA_GOICUOC_TICHHOP SET TH_THANG=1 WHERE FORMAT(NGAY_BD,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);

                //Tính số ngày sử dụng tích hợp
                //
                qry = $"UPDATE DANHBA_GOICUOC_TICHHOP SET TH_SO_NGAY={obj.day_in_month}-DAY(NGAY_BD) WHERE FORMAT(NGAY_BD,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //
                qry += $"UPDATE DANHBA_GOICUOC_TICHHOP SET TH_SO_NGAY=DAY(NGAY_KT) WHERE FORMAT(NGAY_KT,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                //
                qry += $"UPDATE DANHBA_GOICUOC_TICHHOP SET TH_SO_NGAY=DAY(NGAY_KT)-DAY(NGAY_BD) WHERE FORMAT(NGAY_BD,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(NGAY_KT,'MM/yyyy')='{obj.month_year_time}' AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //
                //DataAccess.Connection.SQLServer.ConnectionClose();
                return Json(new { success = "Xử lý tích hợp thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally
            {
                SQLServer.Close();
            }
        }
        //Tích hợp toàn bộ thuê bao
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyTichHopToanBo(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            long index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            try
            {
                //Xử lý NET
                var qry = $"update hd SET hd.GOICUOCID=thdvtv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_NET} hd inner join (select * from DANHBA_GOICUOC_TICHHOP where goicuoc_id in (select thdv.goicuoc_id from {Common.Objects.TYPE_HD.HD_MYTV} tv,DANHBA_GOICUOC_TICHHOP thdv where tv.ACCOUNT=thdv.ACCOUNT and FORMAT(tv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0)) thdvtv on hd.ACCOUNT=thdvtv.ACCOUNT where FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                qry = $@"UPDATE hd SET hd.TONG=bg.GIA FROM {Common.Objects.TYPE_HD.HD_NET} hd INNER JOIN BGCUOC bg ON hd.GOICUOCID=bg.GOICUOCID WHERE hd.GOICUOCID>0 AND hd.TYPE_BILL=9 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND bg.FLAG=1 AND (bg.DICHVUVT_ID=9 OR bg.DICHVUVT_ID=6);
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONG=(TONG/{obj.day_in_month})*NGAY_TB_PTTB where GOICUOCID>0 AND (NGAY_KHOA is not null or NGAY_MO is not null or NGAY_KT is not null) AND TYPE_BILL=9 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET VAT=ROUND(TONG/10,0) WHERE TYPE_BILL=9 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_NET} SET TONGCONG=TONG+VAT WHERE TYPE_BILL=9 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                //Xử lý MyTV
                qry = $"update hd SET hd.GOICUOCID=thdvtv.LOAIGOICUOC_ID FROM {Common.Objects.TYPE_HD.HD_MYTV} hd inner join (select * from DANHBA_GOICUOC_TICHHOP where goicuoc_id in (select thdv.goicuoc_id from {Common.Objects.TYPE_HD.HD_NET} net,DANHBA_GOICUOC_TICHHOP thdv where net.ACCOUNT=thdv.ACCOUNT and FORMAT(net.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' and FORMAT(thdv.TIME_BILL,'MM/yyyy')='{obj.month_year_time}' AND thdv.FIX=0)) thdvtv on hd.ACCOUNT=thdvtv.ACCOUNT where FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                qry = $@"UPDATE hd SET hd.TONG=bg.GIA+hd.PAYTV_FEE FROM {Common.Objects.TYPE_HD.HD_MYTV} hd INNER JOIN BGCUOC bg ON hd.GOICUOCID=bg.GOICUOCID WHERE hd.GOICUOCID>0 AND bg.DICHVUVT_ID=8 AND hd.TYPE_BILL=8 AND FORMAT(hd.TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONG=(TONG/{obj.day_in_month})*NGAY_TB_PTTB where GOICUOCID>0 AND (NGAY_KHOA is not null or NGAY_MO is not null or NGAY_KT is not null) AND TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET VAT=ROUND(TONG/10,0) WHERE TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_MYTV} SET TONGCONG=TONG+VAT WHERE TYPE_BILL=8 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = "Xử lý tích hợp toàn bộ thuê bao thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Ghép hóa đơn
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult MerginBill(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            long index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            //
            int TYPE_BILL = 9006;
            try
            {
                //Xóa data cũ
                var qry = $"DELETE {Common.Objects.TYPE_HD.HD_MERGIN} WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Nhập HD Internet
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_MERGIN}(ID,HD_ID,DBKH_ID,TYPE_BILL,TIME_BILL,MA_TT_HNI,MA_DVI,ACC_NET,TONG_CD,TONG_DD,TONG_NET,TONG_TV,GIAM_TRU,KIEU,GHEP,MA_IN,KIEU_TT,DUPE_FLAG,FLAG,CUOC_KTHUE,TONG_IN,TONG,VAT,TONGCONG)
                         SELECT NEWID(),a.ID,DBKH_ID,{TYPE_BILL} AS TYPE_BILL,a.TIME_BILL,b.MA_TT_HNI,b.MA_DVI,a.ACCOUNT AS ACC_NET,0 AS TONG_CD,0 AS TONG_DD,a.TONG AS TONG_NET,0 AS TONG_TV,a.GIAM_TRU,1 AS KIEU,0 AS GHEP,9 AS MA_IN,0 AS KIEU_TT,0 AS DUPE_FLAG,1 AS FLAG,0 AS CUOC_KTHUE,a.TONG_IN,a.TONG,0 AS VAT,0 AS TONGCONG 
                         FROM {Common.Objects.TYPE_HD.HD_NET} a INNER JOIN DB_THANHTOAN_BKN b ON a.DBKH_ID=b.ID WHERE a.TYPE_BILL=9005 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Nhập HD MyTV  
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_MERGIN}(ID,HD_ID,DBKH_ID,TYPE_BILL,TIME_BILL,MA_TT_HNI,MA_DVI,ACC_TV,TONG_CD,TONG_DD,TONG_NET,TONG_TV,GIAM_TRU,KIEU,GHEP,MA_IN,KIEU_TT,DUPE_FLAG,FLAG,CUOC_KTHUE,TONG_IN,TONG,VAT,TONGCONG)
                         SELECT NEWID(),a.ID,DBKH_ID,{TYPE_BILL} AS TYPE_BILL,a.TIME_BILL,b.MA_TT_HNI,b.MA_DVI,a.ACCOUNT AS ACC_TV,0 AS TONG_CD,0 AS TONG_DD,0 AS TONG_NET,a.TONG AS TONG_TV,a.GIAM_TRU,1 AS KIEU,0 AS GHEP,8 AS MA_IN,0 AS KIEU_TT,0 AS DUPE_FLAG,1 AS FLAG,0 AS CUOC_KTHUE,a.TONG_IN,a.TONG,0 AS VAT,0 AS TONGCONG 
                         FROM {Common.Objects.TYPE_HD.HD_MYTV} a INNER JOIN DB_THANHTOAN_BKN b ON a.DBKH_ID=b.ID WHERE a.TYPE_BILL=8 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Nhập HD CD
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_MERGIN}(ID,HD_ID,DBKH_ID,TYPE_BILL,TIME_BILL,MA_TT_HNI,MA_DVI,SO_CD,TONG_CD,TONG_DD,TONG_NET,TONG_TV,GIAM_TRU,KIEU,GHEP,MA_IN,KIEU_TT,DUPE_FLAG,FLAG,CUOC_KTHUE,TONG_IN,TONG,VAT,TONGCONG)
                         SELECT NEWID(),a.ID,DBKH_ID,{TYPE_BILL} AS TYPE_BILL,a.TIME_BILL,b.MA_TT_HNI,b.MA_DVI,a.SO_TB AS SO_CD,a.TONG AS TONG_CD,0 AS TONG_DD,0 AS TONG_NET,0 AS TONG_TV,a.CHIET_KHAU AS GIAM_TRU,1 AS KIEU,0 AS GHEP,1 AS MA_IN,0 AS KIEU_TT,0 AS DUPE_FLAG,1 AS FLAG,0 AS CUOC_KTHUE,a.TONG_IN,a.TONG,0 AS VAT,0 AS TONGCONG 
                         FROM {Common.Objects.TYPE_HD.HD_CD} a INNER JOIN DB_THANHTOAN_BKN b ON a.DBKH_ID=b.ID WHERE a.TYPE_BILL=1 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Nhập HD DD
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_MERGIN}(ID,HD_ID,DBKH_ID,TYPE_BILL,TIME_BILL,MA_TT_HNI,MA_DVI,SO_DD,TONG_CD,TONG_DD,TONG_NET,TONG_TV,GIAM_TRU,KIEU,GHEP,MA_IN,KIEU_TT,DUPE_FLAG,FLAG,CUOC_KTHUE,TONG_IN,TONG,VAT,TONGCONG)
                         SELECT NEWID(),a.ID,DBKH_ID,{TYPE_BILL} AS TYPE_BILL,a.TIME_BILL,b.MA_TT_HNI,b.MA_DVI,a.SO_TB AS SO_DD,0 AS TONG_CD,a.TONG AS TONG_DD,0 AS TONG_NET,0 AS TONG_TV,a.GIAM_TRU,1 AS KIEU,0 AS GHEP,2 AS MA_IN,a.EZPAY AS KIEU_TT,0 AS DUPE_FLAG,1 AS FLAG,a.CUOC_KTHUE,a.TONG_IN,a.TONG,0 AS VAT,0 AS TONGCONG 
                         FROM {Common.Objects.TYPE_HD.HD_DD} a INNER JOIN DB_THANHTOAN_BKN b ON a.DBKH_ID=b.ID WHERE a.TYPE_BILL=2 AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Ghép cước
                RemoveDuplicate(new Common.RemoveTableObj
                {
                    table = Common.Objects.TYPE_HD.HD_MERGIN.ToString(),
                    PrimeryKey = "MA_TT_HNI",
                    IsExtraValue = true,
                    ExtraValue = "MA_DVI,TYPE_BILL,TIME_BILL",
                    TYPE_BILL = TYPE_BILL,
                    TIME_BILL = obj.month_year_time
                });
                //Cập nhật STK theo mã đơn vị
                qry = "";
                foreach (var i in stk_ma_dvi())
                    qry += $"UPDATE {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} SET STK=N'{i.Value}' WHERE ma_dvi='{i.Key}';";
                SQLServer.Connection.Query(qry);
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MERGIN} SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_MERGIN} SET TONGCONG=TONG+VAT+CUOC_KTHUE WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"{Common.Objects.TYPE_HD.HD_MERGIN} - Xử lý ghép hóa đơn thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ProcessingHDDT(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            long index = 0;
            obj.DataSource = Common.Directories.HDDataSource;
            obj = getDefaultObj(obj);
            //
            int TYPE_BILL = 9007;
            try
            {
                //Xóa data cũ
                var qry = $"DELETE {Common.Objects.TYPE_HD.HD_MERGIN} WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Tạo data hóa đơn điện tử
                qry = $@"INSERT INTO {Common.Objects.TYPE_HD.HD_MERGIN} 
                         SELECT NEWID(),HD_ID,DBKH_ID,{TYPE_BILL} AS TYPE_BILL,TIME_BILL,APP_ID,MA_TT_HNI,MA_DVI,ACC_NET,ACC_TV,SO_DD,SO_CD,TONG_CD,TONG_DD,TONG_NET,TONG_TV,CUOC_KTHUE,GIAM_TRU,TONG_IN,TONG,VAT,TONGCONG,KIEU,GHEP,MA_IN,KIEU_TT,DUPE_FLAG,FLAG 
                         FROM {Common.Objects.TYPE_HD.HD_MERGIN} WHERE TYPE_BILL=9006 AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'";
                SQLServer.Connection.Query(qry);
                //Ghép cước hóa đơn điện tử
                RemoveDuplicate(new Common.RemoveTableObj
                {
                    table = Common.Objects.TYPE_HD.HD_MERGIN.ToString(),
                    PrimeryKey = "MA_TT_HNI",
                    IsExtraValue = true,
                    ExtraValue = "TYPE_BILL,TIME_BILL",
                    TYPE_BILL = TYPE_BILL,
                    TIME_BILL = obj.month_year_time
                });
                //Cập nhật vat và tổng cộng
                qry = $@"UPDATE {Common.Objects.TYPE_HD.HD_MERGIN} SET VAT=ROUND(TONG*0.1,0) WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';
                         UPDATE {Common.Objects.TYPE_HD.HD_MERGIN} SET TONGCONG=TONG+VAT+CUOC_KTHUE WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}';";
                SQLServer.Connection.Query(qry);
                return Json(new { success = $"Hóa đơn điện tử Xử lý thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Tạo file hóa đơn XML
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateHDFile(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var ok = new System.Text.StringBuilder();
            var err = new System.Text.StringBuilder();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            int TYPE_BILL = 9007;
            var indexError = "";
            try
            {
                //Khai báo biến
                var listKey = new List<string>();
                var NumberToLeter = new TM.Helper.NumberToLeter();
                var hasError = false;
                obj.file = "hoadon";
                string fileNameXMLFull = obj.file + ".xml";
                string fileNameZIPFull = obj.file + ".zip";
                string fileNameXMLError = obj.file + "_Error.xml";
                //Xóa file HDDT cũ
                FileManagerController.DeleteDirFile(obj.DataSource + fileNameZIPFull, false);
                FileManagerController.DeleteDirFile(obj.DataSource + fileNameXMLError, false);
                //
                //Get data from HDDT
                var data = SQLServer.Connection.Query<Models.HD_DT>($"SELECT * FROM {Common.Objects.TYPE_HD.HD_MERGIN} a LEFT JOIN {Common.Objects.TYPE_HD.DB_THANHTOAN_BKN} b ON a.DBKH_ID=b.ID WHERE a.TYPE_BILL={TYPE_BILL} AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.month_year_time}'").OrderBy(d => d.TYPE_BILL).ThenBy(d => d.TONGCONG).ToList();
                //HDDT Hoa don
                using (System.IO.Stream outfile = new System.IO.FileStream(obj.DataSource + fileNameXMLFull, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    ok.Append($"<Invoices><BillTime>{obj.time}</BillTime>").AppendLine();
                    err.Append($"<Invoices><BillTime>{obj.time}</BillTime>").AppendLine();
                    foreach (var i in data)
                    {
                        index++;
                        indexError = $"Index: {index}, APP_ID: {i.APP_ID}";
                        #region List Product
                        //<Product>
                        //<ProdName>Dịch vụ Internet</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>1</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ MyTv</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ Cố định</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>20000</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ Di động</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Khuyến mại, giảm trừ</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        #endregion
                        var check = true;
                        //Check EzPay
                        if (i.KIEU_TT == 1)
                        {
                            err.Append($"<Inv><app_id>{i.APP_ID}</app_id><key>{i.MA_TT_HNI}</key><error>Is Ezpay</error></Inv>").AppendLine();
                            check = false;
                            hasError = true;
                        }
                        //Check Error
                        if (string.IsNullOrEmpty(i.MA_TT_HNI))
                        {
                            err.Append($"<Inv><app_id>{i.APP_ID}</app_id><key>{i.MA_TT_HNI}</key><error>Null Or Empty</error></Inv>").AppendLine();
                            check = false;
                            hasError = true;
                        }
                        else
                        {
                            if (listKey.Contains(i.MA_TT_HNI))
                            {
                                err.Append($"<Inv><app_id>{i.APP_ID}</app_id><key>{i.MA_TT_HNI}</key><error>Trùng mã thanh toán</error></Inv>").AppendLine();
                                check = false;
                                hasError = true;
                            }
                        }
                        //Run
                        if (check)
                        {
                            var Products = $"{getProduct("Dịch vụ Internet", i.TONG_NET)}" +
                                           $"{getProduct("Dịch vụ MyTv", i.TONG_TV)}" +
                                           $"{getProduct("Dịch vụ Cố định", i.TONG_CD)}" +
                                           $"{getProduct("Dịch vụ Di động", i.TONG_DD)}" +
                                           $"{getProduct("Khuyến mại, giảm trừ", i.GIAM_TRU)}";
                            //var so_cd = string.IsNullOrEmpty(dt.Rows[i]["so_cd"].ToString().Trim());
                            //var so_dd = string.IsNullOrEmpty(dt.Rows[i]["so_dd"].ToString().Trim());
                            //var acc_net = string.IsNullOrEmpty(dt.Rows[i]["acc_net"].ToString().Trim());
                            //var acc_tv = string.IsNullOrEmpty(dt.Rows[i]["acc_tv"].ToString().Trim());
                            //var ma_in = dt.Rows[i]["ma_in"].ToString().Trim();
                            var Invoice = "<Invoice>" +
                                            $"<MaThanhToan><![CDATA[{getMaThanhToanHD(obj.datetime.ToString("MMyyyy"), i.MA_TT_HNI, i.MA_IN == 2 ? Common.HDDT.DetailDiDong : Common.HDDT.DetailCoDinh)}]]></MaThanhToan>" +
                                            $"<CusCode><![CDATA[{i.MA_TT_HNI}]]></CusCode>" +
                                            $"<CusName><![CDATA[{(obj.ckhTCVN3 ? i.TEN_TT.TCVN3ToUnicode() : i.TEN_TT)}]]></CusName>" +
                                            $"<CusAddress><![CDATA[{(obj.ckhTCVN3 ? i.DIACHI_TT.TCVN3ToUnicode() : i.DIACHI_TT)}]]></CusAddress>" +
                                            $"<CusPhone><![CDATA[{getCusPhone(i.ACC_NET, i.ACC_TV, i.SO_CD, i.SO_DD)}]]></CusPhone>" +
                                            $"<CusTaxCode>{i.MS_THUE}</CusTaxCode>" +
                                            $"<PaymentMethod>{"TM/CK"}</PaymentMethod>" +
                                            $"<KindOfService>Cước Tháng {obj.month_year_time}</KindOfService>" +
                                            $"<Products>{Products}</Products>" +
                                            $"<Total>{i.TONG}</Total>" +
                                            $"<DiscountAmount>{i.GIAM_TRU}</DiscountAmount>" +
                                             "<VATRate>10</VATRate>" +
                                            $"<VATAmount>{i.VAT}</VATAmount>" +
                                            $"<Amount>{i.TONGCONG}</Amount>" +
                                            $"<AmountInWords><![CDATA[{NumberToLeter.DocTienBangChu(i.TONGCONG, "")}]]></AmountInWords>" +
                                            $"<PaymentStatus>0</PaymentStatus>" +
                                            $"<Extra>{i.MA_CBT};0;0</Extra>" +
                                            $"<ResourceCode>{i.MA_CBT}</ResourceCode>" +
                                        "</Invoice>";
                            Invoice = $"<Inv><key>{i.MA_TT_HNI}{obj.month_time}{obj.year_time}</key>{Invoice}</Inv>";
                            ok.Append(Invoice).AppendLine();
                            listKey.Add(i.MA_TT_HNI);
                        }
                        if (index % 100 == 0)
                        {
                            outfile.Write(System.Text.Encoding.UTF8.GetBytes(ok.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(ok.ToString()));
                            ok = new System.Text.StringBuilder();
                        }
                    }
                    ok.Append("</Invoices>");
                    outfile.Write(System.Text.Encoding.UTF8.GetBytes(ok.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(ok.ToString()));
                    //
                    outfile.Close();
                }
                if (hasError)
                {
                    using (System.IO.Stream outfile = new System.IO.FileStream(obj.DataSource + fileNameXMLError, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                    {
                        err.Append("</Invoices>");
                        outfile.Write(System.Text.Encoding.UTF8.GetBytes(err.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(err.ToString()));
                        outfile.Close();
                    }
                    //this.danger("Có lỗi xảy ra Truy cập File Manager \"~\\" + TM.OleDBF.DataSource.Replace("Uploads\\", "") + "\" để tải file");
                }
                //ZipFile
                if (obj.ckhZipFile)
                    TM.IO.Zip.ZipFile(new List<string>() { obj.DataSource + fileNameXMLFull }, obj.DataSource + fileNameZIPFull, 9, false);
                //Insert To File Manager
                FileManagerController.DeleteDirFile(obj.DataSource + fileNameXMLFull, false);
                FileManagerController.InsertFile(obj.DataSource + fileNameZIPFull, false);
                FileManagerController.InsertFile(obj.DataSource + fileNameXMLError, false);
                return Json(new { success = "Hóa đơn điện tử: Tạo file hóa đơn thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{fileNameZIPFull}") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = $"{ex.Message} - Index: {index}" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        //Tạo file khách hàng XML
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateKHFile(Common.DefaultObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var ok = new System.Text.StringBuilder();
            var err = new System.Text.StringBuilder();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            int TYPE_BILL = 9007;
            try
            {

                //Khai báo biến
                var listKey = new List<string>();
                var NumberToLeter = new TM.Helper.NumberToLeter();
                var hasError = false;
                obj.file = "cus";
                string fileNameXMLFull = obj.file + ".xml";
                string fileNameZIPFull = obj.file + ".zip";
                string fileNameXMLError = obj.file + "_Error.xml";
                //Xóa file HDDT cũ
                FileManagerController.DeleteDirFile(fileNameZIPFull);
                FileManagerController.DeleteDirFile(fileNameXMLError);
                //Get data from HDDT
                var data = SQLServer.Connection.Query<Models.HD_DT>($"SELECT * FROM {Common.Objects.TYPE_HD.HD_MERGIN} WHERE TYPE_BILL={TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.month_year_time}'").OrderBy(d => d.MA_DVI).ThenBy(d => d.TONGCONG).ToList();
                using (System.IO.Stream outfile = new System.IO.FileStream(obj.DataSource + fileNameXMLFull, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    ok.Append("<Customers>").AppendLine();
                    err.Append("<Customers>").AppendLine();
                    foreach (var i in data)
                    {
                        index++;
                        //Check EzPay
                        var check = true;
                        if (i.KIEU_TT == 1)
                        {
                            err.Append($"<Inv><app_id>{i.APP_ID}</app_id><key>{i.MA_TT_HNI}</key><error>Is Ezpay</error></Inv>").AppendLine();
                            check = false;
                            hasError = true;
                        }
                        //Check Error
                        if (string.IsNullOrEmpty(i.MA_TT_HNI))
                        {
                            err.Append($"<Customer><app_id>{i.APP_ID}</app_id><key>{i.MA_TT_HNI}</key><error>Null Or Empty HDDT Khách hàng</error></Customer>").AppendLine();
                            check = false;
                            hasError = true;
                        }
                        else
                        {
                            if (listKey.Contains(i.MA_TT_HNI))
                            {
                                err.Append($"<Customer><app_id>{i.APP_ID}</app_id><key>{i.MA_TT_HNI}</key><error>Trùng mã thanh toán Khách hàng</error></Customer>").AppendLine();
                                check = false;
                                hasError = true;
                            }
                        }
                        //Run
                        if (check)
                        {
                            var customer = "<Customer>" +
                                                $"<Name><![CDATA[{(obj.ckhTCVN3 ? i.TEN_TT.TCVN3ToUnicode() : i.TEN_TT)}]]></Name>" +
                                                $"<Code>{i.MA_TT_HNI}</Code>" +
                                                $"<TaxCode>{i.MS_THUE}</TaxCode>" +
                                                $"<Address><![CDATA[{(obj.ckhTCVN3 ? i.DIACHI_TT.TCVN3ToUnicode() : i.TEN_TT)}]]></Address>" +
                                                 "<BankAccountName></BankAccountName>" +
                                                 "<BankName></BankName>" +
                                                $"<BankNumber>{i.BANKNUMBER}</BankNumber>" +
                                                 "<Email></Email>" +
                                                 "<Fax></Fax>" +
                                                $"<Phone>{getCusPhone(i.ACC_NET, i.ACC_TV, i.SO_DD, i.SO_CD)}</Phone>" +
                                                 "<ContactPerson></ContactPerson>" +
                                                 "<RepresentPerson></RepresentPerson>" +
                                                 "<CusType>0</CusType>" +
                                                $"<MaThanhToan><![CDATA[{getMaThanhToanKH(obj.datetime.ToString("MMyyyy"), i.MA_TT_HNI, i.MA_IN == 2 ? Common.HDDT.DetailDiDong : Common.HDDT.DetailCoDinh)}]]></MaThanhToan>" +
                                           "</Customer>";
                            ok.Append(customer).AppendLine();
                            listKey.Add(i.MA_TT_HNI);
                        }
                        if (index % 100 == 0)
                        {
                            outfile.Write(System.Text.Encoding.UTF8.GetBytes(ok.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(ok.ToString()));
                            ok = new System.Text.StringBuilder();
                        }
                    }
                    ok.Append("</Customers>");
                    outfile.Write(System.Text.Encoding.UTF8.GetBytes(ok.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(ok.ToString()));
                    //
                    outfile.Close();
                }
                if (hasError)
                {
                    using (System.IO.Stream outfile = new System.IO.FileStream(obj.DataSource + fileNameXMLError, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                    {
                        err.Append("</Customers>");
                        outfile.Write(System.Text.Encoding.UTF8.GetBytes(err.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(err.ToString()));
                        outfile.Close();
                    }
                    //this.danger("Có lỗi xảy ra Truy cập File Manager \"~\\" + TM.OleDBF.DataSource.Replace("Uploads\\", "") + "\" để tải file");
                }
                //ZipFile
                if (obj.ckhZipFile)
                    TM.IO.Zip.ZipFile(new List<string>() { obj.DataSource + fileNameXMLFull }, obj.DataSource + fileNameZIPFull, 9, false);
                //Insert To File Manager
                FileManagerController.DeleteDirFile(fileNameXMLFull);
                FileManagerController.InsertFile(fileNameZIPFull);
                FileManagerController.InsertFile(fileNameXMLError);
                return Json(new { success = "Hóa đơn điện tử: Tạo file khách hàng thành công!", url = UrlDownloadFiles($"{Common.Directories.HDData}{obj.time}\\{fileNameZIPFull}") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = $"{ex.Message} - Index: {index}" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        private int GetRandom(Random rd, int value, int min, int max)
        {
            var SQLServer = new TM.Connection.SQLServer();
            try
            {
                value = rd.Next(min, max);
                var tmp = SQLServer.Connection.QueryFirstOrDefault($"SELECT ID FROM DANHBA_GOICUOC_TICHHOP WHERE GOICUOC_ID={value}");
                if (tmp == null) return value;
                else return GetRandom(rd, value, min, max);
            }
            catch (Exception) { throw; }
            finally { SQLServer.Close(); }
        }
        public bool RemoveDuplicate(Common.RemoveTableObj obj)//(string table, string PrimeryKey, bool IsExtraValue = false, string ExtraValue = null)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var ExtraValueList = new List<string>();
                var ExtraValueStr = "";
                var ExtraValueStr2 = "";
                var ExtraValueStr3 = "";
                if (obj.ExtraValue != null)
                {
                    ExtraValueStr = obj.ExtraValue.Trim().Trim(',');
                    ExtraValueList.AddRange(ExtraValueStr.Split(',').Trim());
                    foreach (var i in ExtraValueList)
                    {
                        ExtraValueStr2 += $"a.{i}=b.{i} AND ";
                        ExtraValueStr3 += $"a.{i},";
                    }
                    ExtraValueStr2 = ExtraValueStr2.Substring(0, ExtraValueStr2.Length - 5);
                    ExtraValueStr3 = ExtraValueStr3.Trim(',');
                }
                else
                    obj.IsExtraValue = false;
                //
                var billCondition = $"AND a.TYPE_BILL={obj.TYPE_BILL} AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.TIME_BILL}' AND b.TYPE_BILL={obj.TYPE_BILL} AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.TIME_BILL}'";
                var billCondition2 = $"AND TYPE_BILL={obj.TYPE_BILL} AND FORMAT(TIME_BILL,'MM/yyyy')='{obj.TIME_BILL}'";
                //
                var qry = $"UPDATE a SET DUPE_FLAG=0,APP_ID=b.rownumber FROM {obj.table} a,(SELECT *,ROW_NUMBER() OVER(ORDER BY MA_TT_HNI) AS rownumber FROM {obj.table}) b WHERE a.ID=b.ID {billCondition}";
                SQLServer.Connection.Query(qry);

                if (obj.IsExtraValue)
                    qry = $"UPDATE {obj.table} SET DUPE_FLAG=1 WHERE APP_ID in(SELECT APP_ID FROM {obj.table} a INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {obj.table} GROUP BY {obj.PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) b ON a.{obj.PrimeryKey}=b.{obj.PrimeryKey} WHERE {ExtraValueStr2} {billCondition})";
                else
                    qry = $"UPDATE {obj.table} SET DUPE_FLAG=1 WHERE APP_ID in(SELECT APP_ID FROM {obj.table} a INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*) AS dupeCount FROM {obj.table} GROUP BY {obj.PrimeryKey} HAVING COUNT(*) > 1) b ON a.{obj.PrimeryKey}=b.{obj.PrimeryKey})";
                SQLServer.Connection.Query(qry);

                if (obj.IsExtraValue)
                    qry = $"UPDATE {obj.table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {obj.table} a INNER JOIN (SELECT {obj.PrimeryKey},{obj.ExtraValue},COUNT(*) AS dupeCount FROM {obj.table} GROUP BY {obj.PrimeryKey},{obj.ExtraValue} HAVING COUNT(*) > 1) b ON a.{obj.PrimeryKey}=b.{obj.PrimeryKey} WHERE a.{obj.PrimeryKey}!='' AND {ExtraValueStr2} {billCondition} GROUP BY a.{obj.PrimeryKey},{ExtraValueStr3})";
                //sql = $"UPDATE {table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {table} o INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {table} GROUP BY {obj.PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE {ExtraValueStr2})";
                else
                    qry = $"UPDATE {obj.table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {obj.table} a INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*) AS dupeCount FROM {obj.table} GROUP BY {obj.PrimeryKey} HAVING COUNT(*) > 1) b ON a.{obj.PrimeryKey}=b.{obj.PrimeryKey} GROUP BY a.{obj.PrimeryKey})";
                SQLServer.Connection.Query(qry);
                //Sum Col
                SumCol(new Common.RemoveTableObj { table = obj.table, listCol = new[] { "TONG_CD", "TONG_DD", "TONG_NET", "TONG_TV", "CUOC_KTHUE", "GIAM_TRU", "TONG", "TONG_IN" }, conditionKey = new[] { "MA_TT_HNI" }, ExtraValue = $"WHERE DUPE_FLAG=2 {billCondition2}" });
                AppendCol(new Common.RemoveTableObj { table = obj.table, listCol = new[] { "ACC_NET", "ACC_TV", "SO_DD", "SO_CD" }, conditionKey = new[] { "MA_TT_HNI" }, ExtraValue = $"AND DUPE_FLAG=2 {billCondition2}" });
                qry = $"DELETE FROM {obj.table} WHERE DUPE_FLAG=1";
                SQLServer.Connection.Query(qry);
                return true;
            }
            catch (Exception) { throw; }
            finally { SQLServer.Close(); }
        }
        //public bool RemoveDuplicate(Common.RemoveTableObj obj)//(string table, string PrimeryKey, bool IsExtraValue = false, string ExtraValue = null)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    var index = 0;
        //    try
        //    {
        //        var ExtraValueList = new List<string>();
        //        var ExtraValueStr = "";
        //        var ExtraValueStr2 = "";
        //        var ExtraValueStr3 = "";
        //        if (obj.ExtraValue != null)
        //        {
        //            ExtraValueStr = obj.ExtraValue.Trim().Trim(',');
        //            ExtraValueList.AddRange(ExtraValueStr.Split(',').Trim());
        //            foreach (var i in ExtraValueList)
        //            {
        //                ExtraValueStr2 += $"a.{i}=b.{i} AND ";
        //                ExtraValueStr3 += $"a.{i},";
        //            }
        //            ExtraValueStr2 = ExtraValueStr2.Substring(0, ExtraValueStr2.Length - 5);
        //            ExtraValueStr3 = ExtraValueStr3.Trim(',');
        //        }
        //        else
        //            obj.IsExtraValue = false;
        //        //condition
        //        var condition = "";
        //        var groupCondition = "";
        //        if (obj.TYPE_BILL != null)
        //        {
        //            condition += $"AND a.TYPE_BILL={obj.TYPE_BILL} AND b.TYPE_BILL={obj.TYPE_BILL}";
        //            groupCondition += ",TYPE_BILL";
        //        }

        //        if (obj.TIME_BILL != null)
        //        {
        //            condition += $"AND FORMAT(a.TIME_BILL,'MM/yyyy')='{obj.TIME_BILL}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{obj.TIME_BILL}'";
        //            groupCondition += ",TIME_BILL";
        //        }
        //        //
        //        var qry = $"UPDATE a SET DUPE_FLAG=0,APP_ID=b.rownumber FROM {obj.table} a,(SELECT *,ROW_NUMBER() OVER(ORDER BY MA_TT_HNI) AS rownumber FROM {obj.table}) b WHERE a.ID=b.ID {condition}";
        //        SQLServer.Connection.Query(qry);

        //        if (obj.IsExtraValue)
        //            qry = $"UPDATE {obj.table} SET DUPE_FLAG=1 WHERE APP_ID in(SELECT APP_ID FROM {obj.table} a INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*){groupCondition} AS dupeCount FROM {obj.table} GROUP BY {obj.PrimeryKey},{ExtraValueStr}{groupCondition} HAVING COUNT(*) > 1) b ON a.{obj.PrimeryKey}=b.{obj.PrimeryKey} WHERE {ExtraValueStr2} {condition})";
        //        else
        //            qry = $"UPDATE {obj.table} SET DUPE_FLAG=1 WHERE APP_ID in(SELECT APP_ID FROM {obj.table} a INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*){groupCondition} AS dupeCount FROM {obj.table} GROUP BY {obj.PrimeryKey}{groupCondition} HAVING COUNT(*) > 1) b ON a.{obj.PrimeryKey}=b.{obj.PrimeryKey})";
        //        SQLServer.Connection.Query(qry);

        //        if (obj.IsExtraValue)
        //            qry = $"UPDATE {obj.table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {obj.table} a INNER JOIN (SELECT {obj.PrimeryKey},{obj.ExtraValue},COUNT(*) AS dupeCount FROM {obj.table} GROUP BY {obj.PrimeryKey},{obj.ExtraValue} HAVING COUNT(*) > 1) b ON a.{obj.PrimeryKey}=b.{obj.PrimeryKey} WHERE a.{obj.PrimeryKey}!='' AND {ExtraValueStr2} GROUP BY a.{obj.PrimeryKey},{ExtraValueStr3})";
        //        //sql = $"UPDATE {table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {table} o INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {table} GROUP BY {obj.PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE {ExtraValueStr2})";
        //        else
        //            qry = $"UPDATE {obj.table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {obj.table} a INNER JOIN (SELECT {obj.PrimeryKey},COUNT(*) AS dupeCount FROM {obj.table} GROUP BY {obj.PrimeryKey} HAVING COUNT(*) > 1) b ON a.{obj.PrimeryKey}=b.{obj.PrimeryKey} GROUP BY a.{obj.PrimeryKey})";
        //        SQLServer.Connection.Query(qry);
        //        //Sum Col
        //        SumCol(new Common.RemoveTableObj { table = obj.table, listCol = new[] { "TONG_CD", "TONG_DD", "TONG_NET", "TONG_TV", "CUOC_KTHUE", "GIAM_TRU", "TONG" }, conditionKey = new[] { "MA_TT_HNI" }, ExtraValue = "WHERE DUPE_FLAG=2" });
        //        AppendCol(new Common.RemoveTableObj { table = obj.table, listCol = new[] { "ACC_NET", "ACC_TV", "SO_DD", "SO_CD" }, conditionKey = new[] { "MA_TT_HNI" }, ExtraValue = "AND DUPE_FLAG=2" });
        //        qry = $"DELETE FROM {obj.table} WHERE DUPE_FLAG=1";
        //        SQLServer.Connection.Query(qry);
        //        return true;
        //    }
        //    catch (Exception) { throw; }
        //    finally { SQLServer.Close(); }
        //}
        public bool SumCol(Common.RemoveTableObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = "";
                var conditionKeyStr = "";
                foreach (var i in obj.conditionKey)
                    conditionKeyStr += $"{i}=a.{i} AND ";
                conditionKeyStr = conditionKeyStr.Substring(0, conditionKeyStr.Length - 5);
                //
                foreach (var i in obj.listCol)
                    qry += $"UPDATE a SET {i}=(SELECT SUM({i}) FROM {obj.table} WHERE {conditionKeyStr}) FROM {obj.table} AS a {obj.ExtraValue};";
                SQLServer.Connection.Query(qry);
                return true;
            }
            catch (Exception) { throw; }
            finally { SQLServer.Close(); }
        }
        public bool AppendCol(Common.RemoveTableObj obj)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var index = 0;
            try
            {
                var qry = "";
                var conditionKeyStr = "";
                foreach (var i in obj.conditionKey)
                    conditionKeyStr += $"{i}=a.{i} AND ";
                conditionKeyStr = conditionKeyStr.Substring(0, conditionKeyStr.Length - 5);
                //
                foreach (var i in obj.listCol)
                    qry += $"UPDATE a SET {i}=(SELECT MAX({i}) FROM {obj.table} WHERE {conditionKeyStr} AND {i} IS NOT null AND {i}!='') FROM {obj.table} AS a WHERE (a.{i} IS NULL OR {i}!='') {obj.ExtraValue};";
                SQLServer.Connection.Query(qry);
                return true;
            }
            catch (Exception) { throw; }
            finally { SQLServer.Close(); }
        }
        private Boolean RemoveDuplicate(string DataSource, string file, string[] colsNumberSum, string[] colsString, string extraEX = "", string ma_dvi = "ma_dvi", string ma_tt_hni = "ma_tt_hni", string extraConditions = null)
        {
            #region Coment
            ////Remove Duplicate
            ////Tạo thêm cột APP_ID
            //ALTER table hdall ADD COLUMN APP_ID n(10)
            ////cập nhật APP_ID=Auto Increment
            //UPDATE hdall SET APP_ID=RECNO()
            ////Tạo thêm cột DUPE_FLAG
            //ALTER table hdall ADD COLUMN DUPE_FLAG n(2)
            ////cập nhật DUPE_FLAG=0
            //UPDATE hdall SET DUPE_FLAG=0
            ////Tìm các đối tượng trùng
            //SELECT * FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi
            ////Cập nhật các đối tượng trùng DUPE_FLAG=1 
            //UPDATE hdall SET DUPE_FLAG=1 WHERE APP_ID in(SELECT APP_ID FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi)
            ////Kiểm tra
            //SELECT * FROM hdall WHERE DUPE_FLAG=1
            ////Lọc ra các đối tượng trùng giữ lại
            //SELECT MAX(APP_ID) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi
            ////Cập nhật lại các đối tượng giữ lại và set DUPE_FLAG=2
            //UPDATE hdall SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi)
            ////Kiểm tra

            ////Tính tổng các đối tượng trùng
            //SELECT SUM(tong_cd) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi
            ////Cập nhật các đối tượng bị trùng
            //UPDATE a SET tong_cd=(SELECT SUM(tong_cd) FROM hdall WHERE ma_cq=a.ma_cq AND ma_dvi=a.ma_dvi) FROM hdall AS a WHERE DUPE_FLAG=2
            ////Xóa các đối tượng trùng
            //DELETE FROM hdall WHERE DUPE_FLAG=1
            //PACK hdall
            #endregion
            //Declares
            var SQLServer = new TM.Connection.SQLServer();
            string DUPE_FLAG = "DUPE_FLAG";
            string fileDuplication = file + "_duplication.dbf";
            string fileEmptyNull = file + "_EmptyNull.dbf";
            string APP_ID = "APP_ID";

            string qry = "";
            if (ma_dvi == null)
            {
                //Cập nhật các đối tượng trùng DUPE_FLAG=1
                qry = $@"UPDATE {file} SET {DUPE_FLAG}=1 WHERE {APP_ID} in(SELECT {APP_ID} FROM {file} o INNER JOIN 
                (SELECT {ma_tt_hni},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_tt_hni} HAVING COUNT(*) > 1) oc ON o.{ma_tt_hni}=oc.{ma_tt_hni} 
                WHERE NOT EMPTY(o.{ma_tt_hni}))";
                SQLServer.Connection.Query(qry);

                //Cập nhật lại các đối tượng giữ lại và set DUPE_FLAG=2
                qry = $@"UPDATE {file} SET {DUPE_FLAG}=2 WHERE {APP_ID} IN(SELECT MAX({APP_ID}) FROM {file} o INNER JOIN 
                (SELECT {ma_tt_hni},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_tt_hni} HAVING COUNT(*) > 1) oc ON o.{ma_tt_hni}=oc.{ma_tt_hni} 
                WHERE NOT EMPTY(o.{ma_tt_hni}) GROUP BY o.{ma_tt_hni})";
                SQLServer.Connection.Query(qry);
            }
            else
            {
                //Cập nhật các đối tượng trùng DUPE_FLAG=1
                qry = $@"UPDATE {file} SET {DUPE_FLAG}=1 WHERE {APP_ID} in(SELECT {APP_ID} FROM {file} o INNER JOIN 
                (SELECT {ma_tt_hni},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_tt_hni},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_tt_hni}=oc.{ma_tt_hni} 
                WHERE o.{ma_dvi}=oc.{ma_dvi} AND NOT EMPTY(o.{ma_tt_hni}))";
                SQLServer.Connection.Query(qry);
                //Cập nhật lại các đối tượng giữ lại và set DUPE_FLAG=2
                qry = $@"UPDATE {file} SET {DUPE_FLAG}=2{(extraConditions != null ? "," + extraConditions + " " : "")} WHERE {APP_ID} IN(SELECT MAX({APP_ID}) FROM {file} o INNER JOIN 
                (SELECT {ma_tt_hni},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_tt_hni},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_tt_hni}=oc.{ma_tt_hni} 
                WHERE o.{ma_dvi}=oc.{ma_dvi} AND NOT EMPTY(o.{ma_tt_hni}) GROUP BY o.{ma_tt_hni},o.{ma_dvi})";
                //o.{ma_dvi}=oc.{ma_dvi} AND 
                SQLServer.Connection.Query(qry);
            }


            if (colsNumberSum != null)
                foreach (var item in colsNumberSum)
                {
                    qry = $"UPDATE a SET {item}=(SELECT SUM({item}) FROM {file} WHERE {ma_tt_hni}=a.{ma_tt_hni}) FROM {file} AS a WHERE {DUPE_FLAG}=2";
                    SQLServer.Connection.Query(qry);
                }
            if (colsString != null)
                foreach (var item in colsString)
                {
                    qry = $"UPDATE a SET {item}=(SELECT MAX({item}) FROM {file} WHERE {ma_tt_hni}=a.{ma_tt_hni} AND {item} is NOT null AND NOT EMPTY({item})) FROM {file} as a WHERE a.{item} is null OR EMPTY(a.{item})";
                    try { SQLServer.Connection.Query(qry); } catch { }
                }
            //Xóa các đối tượng trùng
            qry = $"DELETE FROM {file} WHERE {DUPE_FLAG}=1";
            SQLServer.Connection.Query(qry);
            return true;
        }
        //public bool GetDuplicate(string table, string PrimeryKey, string ExtraValue, bool IsExtraValue)
        //{
        //    var SQLServer = new TM.Connection.SQLServer();
        //    try
        //    {
        //        var ExtraValueList = new List<string>();
        //        var ExtraValueStr = "";
        //        var ExtraValueStr2 = "";
        //        var ExtraValueStr3 = "";
        //        if (!string.IsNullOrEmpty(ExtraValue))
        //        {
        //            ExtraValueStr = ExtraValue.Trim().Trim(',');
        //            ExtraValueList.AddRange(ExtraValueStr.Split(',').Trim());
        //            foreach (var i in ExtraValueList)
        //            {
        //                ExtraValueStr2 += $"o.{i}=oc.{i} AND ";
        //                ExtraValueStr3 += $"o.{i},";
        //            }
        //            ExtraValueStr2 = ExtraValueStr2.Substring(0, ExtraValueStr2.Length - 5);
        //            ExtraValueStr3 = ExtraValueStr3.Trim(',');
        //        }
        //        else
        //            IsExtraValue = false;
        //        //
        //        string sql = $"ALTER table {table} ADD COLUMN APP_ID n(10)";
        //        try
        //        {
        //            TM.OleDBF.Execute(sql);
        //        }
        //        catch (Exception) { }
        //        sql = $"ALTER table {table} ADD COLUMN DUPE_FLAG n(2)";
        //        try
        //        {
        //            TM.OleDBF.Execute(sql);
        //        }
        //        catch (Exception) { }
        //        sql = $"UPDATE {table} SET APP_ID=RECNO()";
        //        TM.OleDBF.Execute(sql);
        //        sql = $"UPDATE {table} SET DUPE_FLAG=0";
        //        TM.OleDBF.Execute(sql);

        //        if (IsExtraValue)
        //            sql = $"UPDATE {table} SET DUPE_FLAG=1 WHERE APP_ID in(SELECT APP_ID FROM {table} o INNER JOIN (SELECT {PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {table} GROUP BY {PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) oc ON o.{PrimeryKey}=oc.{PrimeryKey} WHERE {ExtraValueStr2})";
        //        else
        //            sql = $"UPDATE {table} SET DUPE_FLAG=1 WHERE APP_ID in(SELECT APP_ID FROM {table} o INNER JOIN (SELECT {PrimeryKey},COUNT(*) AS dupeCount FROM {table} GROUP BY {PrimeryKey} HAVING COUNT(*) > 1) oc ON o.{PrimeryKey}=oc.{PrimeryKey})";
        //        TM.OleDBF.Execute(sql);

        //        if (IsExtraValue)
        //            sql = $"UPDATE {table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {table} o INNER JOIN (SELECT {PrimeryKey},{ExtraValue},COUNT(*) AS dupeCount FROM {table} GROUP BY {PrimeryKey},{ExtraValue} HAVING COUNT(*) > 1) oc ON o.{PrimeryKey}=oc.{PrimeryKey} WHERE {ExtraValueStr2} GROUP BY {ExtraValueStr3})";
        //        //sql = $"UPDATE {table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {table} o INNER JOIN (SELECT {obj.PrimeryKey},{ExtraValueStr},COUNT(*) AS dupeCount FROM {table} GROUP BY {obj.PrimeryKey},{ExtraValueStr} HAVING COUNT(*) > 1) oc ON o.{obj.PrimeryKey}=oc.{obj.PrimeryKey} WHERE {ExtraValueStr2})";
        //        else
        //            sql = $"UPDATE {table} SET DUPE_FLAG=2 WHERE APP_ID IN(SELECT MAX(APP_ID) FROM {table} o INNER JOIN (SELECT {PrimeryKey},COUNT(*) AS dupeCount FROM {table} GROUP BY {PrimeryKey} HAVING COUNT(*) > 1) oc ON o.{PrimeryKey}=oc.{PrimeryKey} GROUP BY o.{PrimeryKey})";
        //        TM.OleDBF.Execute(sql);

        //        sql = $"DELETE FROM {table} WHERE DUPE_FLAG=0";
        //        TM.OleDBF.Execute(sql);
        //        return true;
        //    }
        //    catch (Exception) { throw; }
        //    finally { SQLServer.Close(); }
        //}
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
        public Dictionary<int, string> stk_ma_dvi()
        {
            return new Dictionary<int, string>() {
                { 2,"8605 201 001805 tại Ngân hàng Nông nghiệp & PTNT Huyện Ba Bể, Tỉnh Bắc Kạn" },
                { 3,"8607 201 001374 tại Ngân hàng Nông nghiệp & PTNT Huyện Bạch Thông, Tỉnh Bắc Kạn" },
                { 4,"8601 201 001712 tại Ngân hàng Nông nghiệp & PTNT Huyện Chợ Đồn, Tỉnh Bắc Kạn" },
                { 5,"8606 201 001366 tại Ngân hàng Nông nghiệp & PTNT Huyện Chợ Mới, Tỉnh Bắc Kạn" },
                { 6,"8603 201 001541 tại Ngân hàng Nông nghiệp & PTNT Huyện Na Rì, Tỉnh Bắc Kạn" },
                { 7,"8604 201 001429 tại Ngân hàng Nông nghiệp & PTNT Huyện Ngân Sơn, Tỉnh Bắc Kạn" },
                { 8,"8602 201 001375 tại Ngân hàng Nông nghiệp & PTNT Huyện Pác Nặm, Tỉnh Bắc Kạn" }
            };
        }
        Models.DICHVU_VT_BKN DVVT(int id)
        {
            return _dvvt.FirstOrDefault(d => d.DICHVUVT_ID == id);
        }
        //Hóa đơn điện tử
        private string getProduct(string ProdName, decimal Amount, string ProdUnit = null, int? ProdQuantity = null, decimal? ProdPrice = null)
        {
            return $"<Product><ProdName>{ProdName}</ProdName><ProdUnit>{ProdUnit}</ProdUnit><ProdQuantity>{ProdQuantity}</ProdQuantity><ProdPrice>{ProdPrice}</ProdPrice><Amount>{Amount}</Amount></Product>";
        }
        private string getCusPhone(string acc_net, string acc_tv, string so_cd, string so_dd)
        {
            if (!string.IsNullOrEmpty(acc_net))
                return acc_net;
            else if (!string.IsNullOrEmpty(acc_tv))
                return acc_tv;
            else if (!string.IsNullOrEmpty(so_cd))
                return so_cd;
            else
                return so_dd;
        }
        private string fixMaThanhToan(string ma_cq, string preFixMain = "06", int dfLenght = 13)
        {
            ma_cq = ma_cq.Trim();
            var count = ma_cq.Length;
            var preFixMaCQ = "";
            if (count < dfLenght)
                for (int i = 0; i < dfLenght - count; i++)
                {
                    preFixMaCQ = " " + preFixMaCQ;
                }
            else if (count > dfLenght) dfLenght = count;

            return preFixMain + dfLenght + ma_cq + preFixMaCQ;
        }
        private string getMaThanhToanHD(string valueTime, string ma_cq, string Details)
        {
            //Ver 1
            //string first = "0002010102112620970415010686973800115204123453037045802VN5910VIETINBANK6005HANOI6106100000";
            //Ver 2
            string first = "0002020102112620994814010686973800115204000153037045802VN5914VNPT VINAPHONE6005HANOI6106100000";
            string time = "0106" + valueTime;
            string province = "0703" + Common.HDDT.ProvinceCode.BCN;
            string QRType = "0818" + (int)Common.HDDT.QRCodeType.HDDT;
            string last = time + fixMaThanhToan(ma_cq) + province + QRType + Details;
            string tagLength = "62" + last.Length.ToString();
            return first + tagLength + last;
            //"<MaThanhToan><![CDATA[0002010102112620970415010686973800115204123453037045802VN5909VIETINBANK6005HANOI6106100000626301060720170613  024357434690703BCN08172CUOC MANG CODINH]]></MaThanhToan>"
        }
        private string getMaThanhToanKH(string valueTime, string ma_cq, string Details)
        {
            //Ver 1
            //string first = "0002010102112620970415010686973800115204123453037045802VN5910VIETINBANK6005HANOI6106100000";
            //Ver 2
            string first = "0002020102112620994814010686973800115204000153037045802VN5914VNPT VINAPHONE6005HANOI6106100000";
            string province = "0703" + Common.HDDT.ProvinceCode.BCN;
            string QRType = "0818" + (int)Common.HDDT.QRCodeType.HDDT;
            string last = fixMaThanhToan(ma_cq) + province + QRType + Details;
            string tagLength = "62" + last.Length.ToString();
            return first + tagLength + last;
        }
    }
}