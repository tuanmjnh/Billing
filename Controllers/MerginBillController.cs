using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using Dapper;
using System.Data.Entity;
using TM.Helper;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class MerginBillController : BaseController
    {
        string app_id = "app_id";
        string tratruoc = "tratruoc";
        string istratruoc = "istratruoc";
        string datcoc = "datcoc";
        string isdatcoc = "isdatcoc";
        // GET: MerginOrder
        public ActionResult Index()
        {
            ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(TM.Common.Directories.HDData).OrderByDescending(d => d).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult UploadFiles(FormCollection collection)
        {
            try
            {
                var ckhMerginMonth = collection["ckhMerginMonth"] != null ? true : false;
                var time = collection["time"].ToString();
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                //Declare
                string hdcd = "hdcd" + time;
                string hddd = "hddd" + time;
                string hdnet = "hdnet" + time;
                string hdtv = "hdtv" + time;

                //Source
                //DataSource = Server.MapPath("~/Uploads/Data/");
                //TM.IO.FileDirectory.CreateDirectory(DataSource);
                var DataSource = TM.Common.Directories.HDData;
                FileManagerController.InsertDirectory(DataSource);
                var fileNameSource = new List<string>();
                var fileName = new List<string>();
                var fileSavePath = new List<string>();
                var dtMergin = new System.Data.DataTable();
                int uploadedCount = 0;
                if (Request.Files.Count > 0)
                {
                    string CurrentMonthYear = System.IO.Path.GetFileName(Request.Files[0].FileName).ToLower().Replace(".dbf", "").RemoveWord();
                    fileNameSource.Add(hdcd + ".dbf");
                    fileNameSource.Add(hddd + ".dbf");
                    fileNameSource.Add(hdnet + ".dbf");
                    fileNameSource.Add(hdtv + ".dbf");
                    DataSource += time + "/";
                    TM.OleDBF.DataSource = DataSource;
                    //TM.IO.FileDirectory.CreateDirectory(DataSource);
                    FileManagerController.InsertDirectory(DataSource);
                    //Delete old File
                    //TM.IO.Delete(DataSource, TM.IO.Files(DataSource));

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (!file.FileName.IsExtension(".dbf"))
                        {
                            this.danger("Tệp phải định dạng .dbf");
                            return RedirectToAction("Index");
                        }

                        if (!fileNameSource.Contains(System.IO.Path.GetFileName(file.FileName).ToLower()))
                        {
                            this.danger("Sai tên tệp");
                            //return RedirectToAction("Index");
                        }

                        if (file.ContentLength > 0)
                        {
                            fileName.Add(System.IO.Path.GetFileName(file.FileName).ToLower());
                            fileSavePath.Add(TM.IO.FileDirectory.MapPath(DataSource) + fileName[i]);
                            file.SaveAs(fileSavePath[i]);
                            uploadedCount++;
                            FileManagerController.InsertFile(DataSource + fileName[i]);
                        }
                    }
                    var rs = "Tải lên thành công </br>";
                    foreach (var item in fileName)
                        rs += item + "<br/>";
                    this.success(rs);
                }
                else
                    this.danger("Vui lòng chọn đủ 4 tệp hdcd+MM+yy.dbf, hddd+MM+yy.dbf, hdnet+MM+yy.dbf, hdtv+MM+yy.dbf !");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult PaidProcess(string time, bool ckhMerginMonth)
        {
            try
            {
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                //Declare
                var hdall = "hdall" + time;
                string hdcd = "hdcd" + time;
                string hddd = "hddd" + time;
                string hdnet = "hdnet" + time;
                string hdtv = "hdtv" + time;
                //Source
                var fileNameSource = new List<string>();
                var dtMergin = new System.Data.DataTable();
                var check_file = 0;
                fileNameSource.Add(hdcd + ".dbf");
                fileNameSource.Add(hddd + ".dbf");
                fileNameSource.Add(hdnet + ".dbf");
                fileNameSource.Add(hdtv + ".dbf");
                //Datasource
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";

                var fileList = TM.IO.FileDirectory.FilesToList(TM.OleDBF.DataSource);
                foreach (var item in fileList)
                {
                    if (fileNameSource.Contains(item))
                        check_file++;
                }
                if (check_file < 4)
                {
                    this.danger("Chưa tải đủ tệp!");
                    return RedirectToAction("Index");
                }

                //Net
                //AddPaidProcess(TM.OleDBF.DataSource, hdnet, time, "account", "tong");
                //AddDepositProcess(TM.OleDBF.DataSource, hdnet, time, "account", "tongcong");
                //TV
                //AddPaidProcess(TM.OleDBF.DataSource, hdtv, time, "account", "cuoc_tb");
                //AddDepositProcess(TM.OleDBF.DataSource, hdtv, time, "account", "tongcong");

                this.success("Cập nhật trả tiền trước, đặt cọc thành công");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult MerginSelf(string time, bool ckhMerginMonth)
        {
            try
            {
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                //Declare
                string hdcd = "hdcd" + time;
                string hddd = "hddd" + time;
                string hdnet = "hdnet" + time;
                string hdtv = "hdtv" + time;
                //Source
                var fileNameSource = new List<string>();
                fileNameSource.Add(hdcd + ".dbf");
                fileNameSource.Add(hddd + ".dbf");
                fileNameSource.Add(hdnet + ".dbf");
                fileNameSource.Add(hdtv + ".dbf");
                //Datasource
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";

                //Delete old File
                //var fileList = TM.IO.FileDirectory.FilesToList(TM.OleDBF.DataSource);
                //foreach (var item in fileList)
                //    if (!fileNameSource.Contains(item.ToLower()))
                //        FileManagerController.DeleteDirFile(TM.OleDBF.DataSource + item); //TM.IO.FileDirectory.Delete(DataSource + item);
                //    else
                //        check_file++;
                var deleteFile = RemoveFileSource(TM.OleDBF.DataSource, ".bak", fileNameSource);

                if (deleteFile.CountException < 4)
                {
                    //this.danger("Vui lòng chọn đủ 4 tệp hdcd+MM+yy.dbf, hddd+MM+yy.dbf, hdnet+MM+yy.dbf, hdtv+MM+yy.dbf !");
                    this.danger("Chưa tải đủ tệp!");
                    return RedirectToAction("Index");
                }

                //Backup File
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hdcd + ".dbf", TM.OleDBF.DataSource + hdcd + "_old.dbf");
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hdcd + "_old.dbf");
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hdnet + ".dbf", TM.OleDBF.DataSource + hdnet + "_old.dbf");
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hdnet + "_old.dbf");
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hdtv + ".dbf", TM.OleDBF.DataSource + hdtv + "_old.dbf");
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hdtv + "_old.dbf");
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hddd + ".dbf", TM.OleDBF.DataSource + hddd + "_old.dbf");
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hddd + "_old.dbf");

                //Remove Duplicate
                RemoveDuplicate(TM.OleDBF.DataSource, hdcd, new string[] { "tong", "vat", "tong_cuoc" },
                    new string[] { "ten_cq", "dia_chi", "ma_st" }, "Remove Duplicate " + hdcd, "dvql_id", "ma_kh1");

                RemoveDuplicate(TM.OleDBF.DataSource, hddd, new string[] { "cuoc_cthue", "thue", "tongcong", "cuoc_kthue", "giamtru" },
                    new string[] { "ten_tt", "diachi_tt", "ms_thue", "taikhoan" }, "Remove Duplicate " + hddd, "ma_dvi", "ma_cq");

                RemoveDuplicate(TM.OleDBF.DataSource, hdnet, new string[] { "tong", "vat", "tongcong" },
                    new string[] { "ten_tt", "diachi_tt", "ms_thue", "dienthoai" }, "Remove Duplicate " + hdnet, "ma_dvi", "ma_tt_hni");

                RemoveDuplicate(TM.OleDBF.DataSource, hdtv, new string[] { "tong", "vat", "tongcong" },
                    new string[] { "ten_tt", "diachi_tt", "ms_thue", "dienthoai" }, "Remove Duplicate " + hdtv, "ma_dvi", "ma_tt_hni");

                ReExtensionToLower(TM.OleDBF.DataSource);

                //Update HDCD
                TM.OleDBF.Execute("ALTER TABLE " + hdcd + " ALTER COLUMN Dia_chi char(100)", hdcd);
                TM.OleDBF.Execute("ALTER TABLE " + hdcd + " ALTER COLUMN Ma_cq char(30)", hdcd);

                //Update Ezpay HDDD
                TM.OleDBF.Execute("UPDATE " + hddd + " SET ezpay=0 WHERE ezpay is null", "UPDATE Ezpay " + hddd);

                //Remove Bak file
                deleteFile = RemoveFileSource(TM.OleDBF.DataSource);

                this.success("Ghép hóa đơn lẻ thành công!");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Mergin(string time, bool ckhMerginMonth)
        {
            try
            {
                //Declare
                var hdall = "hdall" + time;
                string hdcd = "hdcd" + time;
                string hddd = "hddd" + time;
                string hdnet = "hdnet" + time;
                string hdtv = "hdtv" + time;
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                //Source
                var fileNameSource = new List<string>();
                var dtMergin = new System.Data.DataTable();
                var check_file = 0;
                fileNameSource.Add(hdcd + ".dbf");
                fileNameSource.Add(hddd + ".dbf");
                fileNameSource.Add(hdnet + ".dbf");
                fileNameSource.Add(hdtv + ".dbf");
                //Datasource
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";

                var fileList = TM.IO.FileDirectory.FilesToList(TM.OleDBF.DataSource);
                foreach (var item in fileList)
                {
                    if (fileNameSource.Contains(item))
                        check_file++;
                }
                if (check_file < 4)
                {
                    this.danger("Chưa tải đủ tệp!");
                    return RedirectToAction("Index");
                }

                //Xóa file hdall cũ
                FileManagerController.DeleteDirFile(TM.OleDBF.DataSource + hdall + ".dbf");

                //Tạo bảng hóa đơn ghép
                //Mã in
                //- gộp = 1
                //- net = 2
                //- tv = 3
                //- cd = 4
                //- dd = 5
                TM.OleDBF.CreateTable(hdall, new Dictionary<string, string>()
                {
                    {"ma_dvi", "n(2)"},
                    {"ma_tt_hni", "c(20)"},
                    {"acc_net", "c(20)"},
                    {"acc_tv", "c(20)"},
                    {"so_dd", "c(20)"},
                    {"so_cd", "c(20)"},
                    {"ten_tt", "c(100)"},
                    {"diachi_tt", "c(150)"},
                    {"dienthoai", "c(20)"},
                    {"ma_tuyen", "c(10)"},
                    {"ms_thue", "c(15)"},
                    {"banknumber", "c(16)"},
                    {"ma_dt", "n(10)"},
                    {"ma_cbt", "n(15)"},
                    {"tong_cd", "n(15)"},
                    {"tong_dd", "n(15)"},
                    {"tong_net", "n(15)"},
                    {"tong_tv", "n(15)"},
                    {"vat", "n(15,2)"},
                    {"tong", "n(15)"},
                    {"kthue", "n(15)"},
                    {"giam_tru", "n(15)"},
                    {"tongcong", "n(15)"},
                    {"stk", "c(100)"},
                    {"ezpay", "n(1)"},
                    {"kieu", "c(10)"},
                    {"ghep", "n(1)"},
                    {"ma_in", "n(1)"},
                    {"kieu_tt", "n(1)"},
                    {"flag", "n(1)"},
                    {"app_id", "n(10)"},
            });
                #region old
                //TM.OleDBF.Execute(
                //    "CREATE TABLE " + hdall + @"(
                //        [Ma_dvi] n(2),
                //        [ma_cq] c(50),
                //        [acc_net] c(50),
                //        [acc_tv] c(50),
                //        [so_dd] c(50),
                //        [so_cd] c(50),
                //        [ten_tb] c(100),
                //        [dia_chi] c(100),
                //        [ma_tuyen] c(50),
                //        [ma_st] c(15),
                //        [banknumber] c(16),
                //        [ma_dt] n(1),
                //        [ma_cbt] n(15),
                //        [tong_cd] n(12),
                //        [tong_dd] n(12),
                //        [tong_net] n(12),
                //        [tong_tv] n(12),
                //        [vat] n(12, 2),
                //        [tong] n(12),
                //        [kthue] n(12),
                //        [giam_tru] n(12),
                //        [tongcong] n(12),
                //        [ezpay] n(1),
                //        [Kieu] c(10),
                //        [ghep] n(1),
                //        [ma_in] n(1),
                //        [flag] n(1),
                //        [app_id] n(10))", hdall);
                #endregion

                //insert_hdall
                //Nhập hóa đơn cố định vào hóa đơn ghép
                TM.OleDBF.Execute(InsertString(hdall, hdcd, new Dictionary<string, string>()
                {
                    { "ma_dvi", "dvql_id" },
                    { "ma_tt_hni","Ma_kh1" },
                    { "so_cd","so_tb" },
                    { "ten_tt","ten_cq" },
                    { "diachi_tt","dia_chi" },
                    { "ma_tuyen","ma_tuyen" },
                    { "ms_thue","ma_st" },
                    { "ma_dt","ma_dt" },
                    { "ma_cbt","ma_cbt" },
                    { "tong_cd","tong" },
                    { "vat","vat" },
                    { "tong","tong" },
                    { "tongcong","tong_cuoc" },
                    { "ezpay","0" },
                    { "kieu","1" },
                    { "ghep","0" },
                    { "ma_in","4" },
                    { "flag","1" },
                }), "Insert " + hdcd);

                //Nhập hóa đơn net vào hóa đơn ghép
                TM.OleDBF.Execute(InsertString(hdall, hdnet, new Dictionary<string, string>()
                {
                    { "ma_dvi", "ma_dvi" },
                    { "ma_tt_hni","ma_tt_hni" },
                    { "acc_net","account" },
                    { "ten_tt","ten_tt" },
                    { "diachi_tt","diachi_tt" },
                    { "dienthoai","dienthoai" },
                    { "ma_tuyen","ma_tuyen" },
                    { "ms_thue","ms_thue" },
                    { "ma_dt","ma_dt" },
                    { "ma_cbt","ma_cbt" },
                    { "tong_net","Tong" },
                    { "vat","vat" },
                    { "tong","tong" },
                    { "tongcong","tongcong" },
                    { "ezpay","0" },
                    { "kieu","1" },
                    { "ghep","0" },
                    { "ma_in","2" },
                    { "flag","1" },
                }), "Insert " + hdnet);

                //Nhập hóa đơn tv vào hóa đơn ghép
                TM.OleDBF.Execute(InsertString(hdall, hdtv, new Dictionary<string, string>()
                {
                    { "ma_dvi", "ma_dvi" },
                    { "ma_tt_hni","ma_tt_hni" },
                    { "acc_tv","account" },
                    { "ten_tt","ten_tt" },
                    { "diachi_tt","diachi_tt" },
                    { "dienthoai","dienthoai" },
                    { "ma_tuyen","ma_tuyen" },
                    { "ms_thue","ms_thue" },
                    { "ma_dt","ma_dt" },
                    { "ma_cbt","ma_cbt" },
                    { "tong_tv","Tong" },
                    { "vat","vat" },
                    { "tong","tong" },
                    { "tongcong","tongcong" },
                    { "ezpay","0" },
                    { "kieu","1" },
                    { "ghep","0" },
                    { "ma_in","3" },
                    { "flag","1" },
                }), "Insert " + hdtv);

                //Nhập hóa đơn di động vào hóa đơn ghép
                TM.OleDBF.Execute(InsertString(hdall, hddd, new Dictionary<string, string>()
                {
                    { "ma_dvi", "ma_dvi" },
                    { "ma_tt_hni","ma_cq" },
                    { "so_dd","so_tb" },
                    { "ten_tt","ten_tt" },
                    { "diachi_tt","diachi_tt" },
                    { "ma_tuyen","ma_tuyen" },
                    { "ms_thue","ms_thue" },
                    { "banknumber","taikhoan" },
                    { "ma_dt","ma_dt" },
                    { "ma_cbt","ma_cbt" },
                    { "tong_dd","cuoc_cthue" },
                    { "vat","thue" },
                    { "tong","cuoc_cthue+cuoc_kthue" },
                    { "kthue","cuoc_kthue" },
                    { "giam_tru","giamtru" },
                    { "tongcong","tongcong" },
                    { "ezpay","ezpay" },
                    { "kieu","1" },
                    { "ghep","0" },
                    { "ma_in","5" },
                    { "flag","1" },
                }), "Insert " + hddd);

                //Update Ma_dt 0 -> 1
                TM.OleDBF.Execute($"UPDATE {hdall} SET ma_dt=1 WHERE ma_dt=0", hdall);

                //Remove tongcong<=1000
                TM.OleDBF.Execute($"UPDATE {hdall} SET flag=0 WHERE tongcong<=1000", hdall);

                //Tạo thêm cột app_id
                //try
                //{
                //    TM.OleDBF.Execute(string.Format("ALTER TABLE {0} ADD COLUMN {1} n(10)", file, app_id), "Tạo cột app_id - " + extraEX);
                //}
                //catch (Exception) { }
                //Cập nhật app_id = Auto Increment
                //TM.OleDBF.Execute(string.Format("UPDATE {0} SET {1}=RECNO()", hdall, app_id), "Cập nhật app_id = Auto Increment ");

                //Remove Duplicate hdall
                //Xử lý trùng mã thanh toán khác đơn vị
                RemoveDuplicate(TM.OleDBF.DataSource, hdall,
                    new string[] { "tong_cd", "tong_dd", "tong_net", "tong_tv", "vat", "tong", "kthue", "giam_tru", "tongcong" },
                    new string[] { "acc_net", "acc_tv", "so_dd", "so_cd", "diachi_tt", "ma_tuyen", "ms_thue", "ma_dt", "ma_cbt" },
                    "Remove Duplicate hoadon", "ma_dvi", "ma_tt_hni", "ma_in=1");
                //Cập nhật STK
                UpdateSTK_MA_DVI(Common.Objects.stk_ma_dvi, hdall);

                //Update NULL
                TM.OleDBF.Execute($"UPDATE {hdall} SET ezpay=0 WHERE ezpay is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET tong_cd=0 WHERE tong_cd is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET tong_dd=0 WHERE tong_dd is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET tong_net=0 WHERE tong_net is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET tong_tv=0 WHERE tong_tv is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET kthue=0 WHERE kthue is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET giam_tru=0 WHERE giam_tru is null", hdall);

                //Remove Bak file
                var deleteFile = RemoveFileSource(TM.OleDBF.DataSource);

                //Add hdall to FileManager
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hdall + ".dbf");

                //Return Download File
                //return RedirectToAction("DownloadFiles");
                this.success("Ghép hóa đơn thành công");

            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public JsonResult CheckMoney(string time, bool ckhMerginMonth)
        {
            try
            {
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";
                //HDALL
                var TotalHDAll = new TotalHDAll();
                var dt = TM.OleDBF.ToDataTable("SELECT SUM(tong_cd) AS tong_cd,SUM(tong_dd) AS tong_dd,SUM(tong_net) AS tong_net,SUM(tong_tv) AS tong_tv,SUM(tong) AS tong,SUM(vat) AS vat,SUM(kthue) AS kthue,SUM(giam_tru) AS giam_tru,SUM(tongcong) AS tongcong FROM hdall" + time);
                TotalHDAll.tong_cd = (decimal)dt.Rows[0]["tong_cd"];
                TotalHDAll.tong_dd = (decimal)dt.Rows[0]["tong_dd"];
                TotalHDAll.tong_net = (decimal)dt.Rows[0]["tong_net"];
                TotalHDAll.tong_tv = (decimal)dt.Rows[0]["tong_tv"];
                TotalHDAll.tong = (decimal)dt.Rows[0]["tong"];
                TotalHDAll.vat = (decimal)dt.Rows[0]["vat"];
                TotalHDAll.kthue = (decimal)dt.Rows[0]["kthue"];
                TotalHDAll.giam_tru = (decimal)dt.Rows[0]["giam_tru"];
                TotalHDAll.tongcong = (decimal)dt.Rows[0]["tongcong"];
                //HDCD
                var TotalHDCD = new TotalHDCD();
                dt = TM.OleDBF.ToDataTable("SELECT SUM(tong) AS tong,SUM(vat) AS vat,SUM(tong_cuoc) AS tongcong FROM hdcd" + time);
                TotalHDCD.tong = dt.Rows[0]["tong"] != null ? (decimal)dt.Rows[0]["tong"] : 0;
                TotalHDCD.vat = dt.Rows[0]["vat"] != null ? (decimal)dt.Rows[0]["vat"] : 0;
                TotalHDCD.tongcong = dt.Rows[0]["tongcong"] != null ? (decimal)dt.Rows[0]["tongcong"] : 0;
                //HDDD
                var TotalHDDD = new TotalHDDD();
                dt = TM.OleDBF.ToDataTable("SELECT SUM(cuoc_cthue) AS tong,SUM(cuoc_kthue) AS kthue,SUM(giamtru) AS giam_tru,SUM(thue) AS vat,SUM(tongcong) AS tongcong FROM hddd" + time);
                TotalHDDD.tong = dt.Rows[0]["tong"] != null ? (decimal)dt.Rows[0]["tong"] : 0;
                TotalHDDD.kthue = dt.Rows[0]["kthue"] != null ? (decimal)dt.Rows[0]["kthue"] : 0;
                TotalHDDD.giam_tru = dt.Rows[0]["giam_tru"] != null ? (decimal)dt.Rows[0]["giam_tru"] : 0;
                TotalHDDD.vat = dt.Rows[0]["vat"] != null ? (decimal)dt.Rows[0]["vat"] : 0;
                TotalHDDD.tongcong = dt.Rows[0]["tongcong"] != null ? (decimal)dt.Rows[0]["tongcong"] : 0;
                //HDNET
                var TotalHDNET = new TotalHDNET();
                dt = TM.OleDBF.ToDataTable("SELECT SUM(tong) AS tong,SUM(vat) AS vat,SUM(tongcong) AS tongcong FROM hdnet" + time);
                TotalHDNET.tong = (decimal)dt.Rows[0]["tong"];
                TotalHDNET.vat = (decimal)dt.Rows[0]["vat"];
                TotalHDNET.tongcong = (decimal)dt.Rows[0]["tongcong"];
                //HDTV
                var TotalHDTV = new TotalHDTV();
                dt = TM.OleDBF.ToDataTable("SELECT SUM(tong) AS tong,SUM(vat) AS vat,SUM(tongcong) AS tongcong FROM hdtv" + time);
                TotalHDTV.tong = (decimal)dt.Rows[0]["tong"];
                TotalHDTV.vat = (decimal)dt.Rows[0]["vat"];
                TotalHDTV.tongcong = (decimal)dt.Rows[0]["tongcong"];

                return Json(new
                {
                    TotalHDAll = TotalHDAll,
                    TotalHDCD = TotalHDCD,
                    TotalHDDD = TotalHDDD,
                    TotalHDNET = TotalHDNET,
                    TotalHDTV = TotalHDTV,
                    success = TM.Common.Language.msgRecoverSucsess
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        //Private Functions
        //
        private string InsertString(string tblAll, string tblAny, Dictionary<string, string> cols)
        {
            string cols_hdAny = "";
            string cols_hdall = "";
            string insert_hdall = "INSERT INTO " + tblAll + "(";
            foreach (var item in cols)
            {
                cols_hdall += item.Key + ",";
                cols_hdAny += item.Value + " as " + item.Key + ",";
            }

            insert_hdall += cols_hdall.TrimEnd(',') + ") SELECT " + cols_hdAny.TrimEnd(',') + " FROM " + tblAny;
            return insert_hdall;
        }
        private void UpdateSTK_MA_DVI(Dictionary<int, string> arr, string table)
        {
            foreach (var item in arr)
                TM.OleDBF.Execute($"UPDATE {table} SET stk='{item.Value}' WHERE ma_dvi={ item.Key}");
        }
        private void RemoveDuplicate(string DataSource, string file, string[] colsNumberSum, string[] colsString, string extraEX = "", string ma_dvi = "ma_dvi", string ma_tt_hni = "ma_tt_hni", string extraConditions = null)
        {
            #region Coment
            ////Remove Duplicate
            ////Tạo thêm cột app_id
            //ALTER table hdall ADD COLUMN app_id n(10)
            ////cập nhật app_id=Auto Increment
            //UPDATE hdall SET app_id=RECNO()
            ////Tạo thêm cột dupe_flag
            //ALTER table hdall ADD COLUMN dupe_flag n(2)
            ////cập nhật dupe_flag=0
            //UPDATE hdall SET dupe_flag=0
            ////Tìm các đối tượng trùng
            //SELECT * FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi
            ////Cập nhật các đối tượng trùng dupe_flag=1 
            //UPDATE hdall SET dupe_flag=1 WHERE app_id in(SELECT app_id FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi)
            ////Kiểm tra
            //SELECT * FROM hdall WHERE dupe_flag=1
            ////Lọc ra các đối tượng trùng giữ lại
            //SELECT MAX(app_id) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi
            ////Cập nhật lại các đối tượng giữ lại và set dupe_flag=2
            //UPDATE hdall SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi)
            ////Kiểm tra

            ////Tính tổng các đối tượng trùng
            //SELECT SUM(tong_cd) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi
            ////Cập nhật các đối tượng bị trùng
            //UPDATE a SET tong_cd=(SELECT SUM(tong_cd) FROM hdall WHERE ma_cq=a.ma_cq AND ma_dvi=a.ma_dvi) FROM hdall AS a WHERE dupe_flag=2
            ////Xóa các đối tượng trùng
            //DELETE FROM hdall WHERE dupe_flag=1
            //PACK hdall
            #endregion
            //Declares
            string dupe_flag = "dupe_flag";
            string fileDuplication = file + "_duplication.dbf";
            string fileEmptyNull = file + "_EmptyNull.dbf";
            //Xóa File cũ
            TM.IO.FileDirectory.Delete(DataSource + file + "_duplication.dbf");
            //DeleteDirectoryFile(DataSource + file + "_duplication.dbf");
            TM.IO.FileDirectory.Delete(DataSource + file + "_EmptyNull.dbf");
            //DeleteDirectoryFile(DataSource + file + "_EmptyNull.dbf");

            //Tạo thêm cột app_id
            try { TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN {app_id} n(10)", "Tạo cột app_id - " + extraEX); } catch { }
            //Cập nhật app_id = Auto Increment
            TM.OleDBF.Execute($"UPDATE {file} SET {app_id}=RECNO()", "Cập nhật app_id = Auto Increment " + extraEX);

            //Tạo thêm cột dupe_flag
            try { TM.OleDBF.Execute($"ALTER table {file} ADD COLUMN {dupe_flag} n(2)", "Tạo cột dupe_flag - " + extraEX); } catch { }
            //Cập nhật dupe_flag=0
            TM.OleDBF.Execute($"UPDATE {file} SET {dupe_flag}=0", "Cập nhật dupe_flag=0 - " + extraEX);

            string sql = "";
            if (ma_dvi == null)
            {
                //Cập nhật các đối tượng trùng dupe_flag=1
                sql = $@"UPDATE {file} SET {dupe_flag}=1 WHERE {app_id} in(SELECT {app_id} FROM {file} o INNER JOIN 
                (SELECT {ma_tt_hni},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_tt_hni} HAVING COUNT(*) > 1) oc ON o.{ma_tt_hni}=oc.{ma_tt_hni} 
                WHERE NOT EMPTY(o.{ma_tt_hni}))";
                TM.OleDBF.Execute(sql, "Cập nhật các đối tượng trùng set dupe_flag=1 - " + file + " - " + extraEX);

                //Cập nhật lại các đối tượng giữ lại và set dupe_flag=2
                sql = $@"UPDATE {file} SET {dupe_flag}=2 WHERE {app_id} IN(SELECT MAX({app_id}) FROM {file} o INNER JOIN 
                (SELECT {ma_tt_hni},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_tt_hni} HAVING COUNT(*) > 1) oc ON o.{ma_tt_hni}=oc.{ma_tt_hni} 
                WHERE NOT EMPTY(o.{ma_tt_hni}) GROUP BY o.{ma_tt_hni})";
                TM.OleDBF.Execute(sql, "Cập nhật lại các đối tượng giữ lại và set dupe_flag=2 - " + file + " - " + extraEX);
            }
            else
            {
                //var grouplist = "";
                //var grouplist2 = "";
                //foreach (var item in ma_dvi.Split(','))
                //{
                //    grouplist += $"o.{item}=oc.{item} AND ";
                //    grouplist2 += $"o.{item},";
                //}

                ////Cập nhật các đối tượng trùng dupe_flag=1
                //sql = $@"UPDATE {file} SET {dupe_flag}=1 WHERE {app_id} in(SELECT {app_id} FROM {file} o INNER JOIN 
                //(SELECT {ma_cq},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_cq},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_cq}=oc.{ma_cq} 
                //WHERE {grouplist} NOT EMPTY(o.{ma_cq}))";
                //TM.OleDBF.Execute(sql, "Cập nhật các đối tượng trùng set dupe_flag=1 - " + file + " - " + extraEX);
                ////Cập nhật lại các đối tượng giữ lại và set dupe_flag=2
                //sql = $@"UPDATE {file} SET {dupe_flag}=2{(extraConditions != null ? "," + extraConditions + " " : "")} WHERE {app_id} IN(SELECT MAX({app_id}) FROM {file} o INNER JOIN 
                //(SELECT {ma_cq},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_cq},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_cq}=oc.{ma_cq} 
                //WHERE {grouplist} NOT EMPTY(o.{ma_cq}) GROUP BY o.{ma_cq},{grouplist2.TrimEnd(',')})";
                //TM.OleDBF.Execute(sql, "Cập nhật lại các đối tượng giữ lại và set dupe_flag=2 - " + file + " - " + extraEX);

                //Cập nhật các đối tượng trùng dupe_flag=1
                sql = $@"UPDATE {file} SET {dupe_flag}=1 WHERE {app_id} in(SELECT {app_id} FROM {file} o INNER JOIN 
                (SELECT {ma_tt_hni},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_tt_hni},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_tt_hni}=oc.{ma_tt_hni} 
                WHERE o.{ma_dvi}=oc.{ma_dvi} AND NOT EMPTY(o.{ma_tt_hni}))";
                TM.OleDBF.Execute(sql, "Cập nhật các đối tượng trùng set dupe_flag=1 - " + file + " - " + extraEX);
                //Cập nhật lại các đối tượng giữ lại và set dupe_flag=2
                sql = $@"UPDATE {file} SET {dupe_flag}=2{(extraConditions != null ? "," + extraConditions + " " : "")} WHERE {app_id} IN(SELECT MAX({app_id}) FROM {file} o INNER JOIN 
                (SELECT {ma_tt_hni},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_tt_hni},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_tt_hni}=oc.{ma_tt_hni} 
                WHERE o.{ma_dvi}=oc.{ma_dvi} AND NOT EMPTY(o.{ma_tt_hni}) GROUP BY o.{ma_tt_hni},o.{ma_dvi})";
                //o.{ma_dvi}=oc.{ma_dvi} AND
                TM.OleDBF.Execute(sql, "Cập nhật lại các đối tượng giữ lại và set dupe_flag=2 - " + file + " - " + extraEX);
            }


            //Cập nhật các đối tượng bị trùng
            //sql = string.Format(@"UPDATE a SET {0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}),{3}=(SELECT SUM({3}) FROM {1} 
            //WHERE {2}=a.{2}),{4}=(SELECT SUM({4}) FROM {1} WHERE {2}=a.{2}),", tong, file, ma_cq, vat, tongcong);
            //if (extraCols != null)
            //    foreach (var item in extraCols)
            //        sql += string.Format("{0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}),", item, file, ma_cq);
            //sql = sql.Trim(',');
            //sql += string.Format(" FROM {0} AS a WHERE dupe_flag=2", file);
            //TM.OleDBF.Execute(string.Format(@"UPDATE a SET {0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}) FROM {1} AS a WHERE {3}=2", tong, file, ma_cq, dupe_flag), "Cập nhật tổng các đối tượng bị trùng - " + extraEX);
            //TM.OleDBF.Execute(string.Format(@"UPDATE a SET {0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}) FROM {1} AS a WHERE {3}=2", vat, file, ma_cq, dupe_flag), "Cập nhật VAT các đối tượng bị trùng - " + extraEX);
            //TM.OleDBF.Execute(string.Format(@"UPDATE a SET {0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}) FROM {1} AS a WHERE {3}=2", tongcong, file, ma_cq, dupe_flag), "Cập nhật tổng cộng các đối tượng bị trùng - " + extraEX);
            if (colsNumberSum != null)
                foreach (var item in colsNumberSum)
                    TM.OleDBF.Execute($"UPDATE a SET {item}=(SELECT SUM({item}) FROM {file} WHERE {ma_tt_hni}=a.{ma_tt_hni}) FROM {file} AS a WHERE {dupe_flag}=2", "Cập nhật " + item + " của các đối tượng bị trùng - " + extraEX);

            if (colsString != null)
                foreach (var item in colsString)
                    try { TM.OleDBF.Execute($"UPDATE a SET {item}=(SELECT MAX({item}) FROM {file} WHERE {ma_tt_hni}=a.{ma_tt_hni} AND {item} is NOT null AND NOT EMPTY({item})) FROM {file} as a WHERE a.{item} is null OR EMPTY(a.{item})", "Cập nhật " + item + " của các đối tượng bị trùng - " + extraEX); } catch { }
            //if (colsString != null)
            //    foreach (var item in colsString)
            //    {
            //        var dt = TM.OleDBF.ToDataTable($"SELECT {ma_cq},{item} FROM {file} WHERE {item} is NOT null");
            //        foreach (System.Data.DataRow row in dt.Rows)
            //        {
            //            TM.OleDBF.Execute($"UPDATE {file} SET {item}='{row[item].ToString()}' WHERE {ma_cq}='{row[ma_cq].ToString()}'", "Cập nhật " + item + " của các đối tượng bị trùng - " + extraEX);
            //        }
            //    }

            //Tạo bảng chứa bản ghi trùng mã
            //TM.IO.FileDirectory.Copy(DataSource + file + ".dbf", DataSource + fileDuplication);
            //FileManagerController.InsertFile(DataSource + fileDuplication);
            //TM.OleDBF.Execute($"DELETE FROM {fileDuplication}", extraEX);
            //TM.OleDBF.Execute($"PACK {fileDuplication}", extraEX);
            //TM.OleDBF.Execute($"INSERT INTO {fileDuplication} SELECT * FROM {file} WHERE {dupe_flag}=1", extraEX);

            //Tạo bảng chứa bản ghi có mã là NULL hoặc mã là Empty
            //TM.IO.FileDirectory.Copy(DataSource + file + ".dbf", DataSource + fileEmptyNull);
            //FileManagerController.InsertFile(DataSource + fileEmptyNull);
            //TM.OleDBF.Execute($"DELETE FROM {fileEmptyNull}", extraEX);
            //TM.OleDBF.Execute($"PACK {fileEmptyNull}", extraEX);
            //TM.OleDBF.Execute($"INSERT INTO {fileEmptyNull} SELECT * FROM {file} WHERE {ma_cq} is null OR {ma_cq}==''", extraEX);

            //Xóa các đối tượng trùng
            TM.OleDBF.Execute($"DELETE FROM {file} WHERE {dupe_flag}=1", "Xóa các đối tượng trùng - " + extraEX);
            TM.OleDBF.Execute($"PACK {file}", extraEX);

            //Xóa rác
            TM.IO.FileDirectory.Delete(DataSource + file + ".BAK");
            TM.IO.FileDirectory.ReExtensionToLower(DataSource + file + ".DBF");
        }

        public string ReplaceEscape(string str)
        {
            str = str.Replace("'", "''");
            return str;
        }
        public bool checkFile(string str, string reg)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, reg);
        }
        public bool isHD(string str)
        {
            return checkFile(str, @"hd\d{4}.dbf");
        }
        public bool isHDNET(string str)
        {
            return checkFile(str, @"hdnet\d{4}.dbf");
        }
        public bool isHDTV(string str)
        {
            return checkFile(str, @"hdtv\d{4}.dbf");
        }
        public bool checkAll(List<string> fileName)
        {
            foreach (var item in fileName)
                if (!checkFile(item, @"(hd|hdnet|hdtv)\d{4}.dbf")) return false;
            return true;
        }
        //Fix Month Year
        private string FixMonthYear(string time)
        {
            // "20" + time[2].ToString() + time[3].ToString() + time[0].ToString() + time[1].ToString();
            return "";
        }
        //Remove Fil eSource
        private DeleteFileList RemoveFileSource(string source, string extension = ".bak", List<string> exception = null)
        {
            var rs = new DeleteFileList();
            try
            {
                var fileList = TM.IO.FileDirectory.FilesToList(source);
                if (exception == null)
                {
                    foreach (var item in fileList)
                        if (item.ToExtension().ToLower() == extension)
                        {
                            FileManagerController.DeleteDirFile(source + item);
                            rs.CountDelete++;
                        }
                }
                else
                {
                    foreach (var item in fileList)
                        if (!exception.Contains(item.ToLower()))
                        {
                            FileManagerController.DeleteDirFile(TM.OleDBF.DataSource + item);
                            rs.CountDelete++;
                        }
                        else
                            rs.CountException++;
                }
            }
            catch (Exception) { }
            return rs;
        }
    }
    //Class object
    public class DeleteFileList
    {
        public int CountDelete { get; set; }
        public int CountException { get; set; }
    }
    public class TotalHDAll
    {
        public decimal tong_cd, tong_dd, tong_net, tong_tv, tong, vat, kthue, giam_tru, tongcong, datcoc, tratruoc;
    }
    public class TotalHDCD
    {
        public decimal tong, vat, tongcong;
    }
    public class TotalHDDD
    {
        public decimal tong, kthue, giam_tru, vat, tongcong;
    }
    public class TotalHDNET
    {
        public decimal tong, vat, tongcong, datcoc, tratruoc;
    }
    public class TotalHDTV
    {
        public decimal tong, vat, tongcong, datcoc, tratruoc;
    }
}