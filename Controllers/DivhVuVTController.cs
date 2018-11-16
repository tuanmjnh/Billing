using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Helper;
using TM.Message;
using PagedList;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Threading.Tasks;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class DivhVuVTController : BaseController
    {
        TM.Connection.SQLServer SQLServer;
        TM.Connection.Oracle Oracle;
        public ActionResult Index(int? flag, string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, string export)
        {
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                if (searchString != null)
                {
                    page = 1;
                    searchString = searchString.Trim();
                }
                else searchString = currentFilter;
                ViewBag.order = order;
                ViewBag.currentFilter = searchString;
                ViewBag.flag = flag;
                ViewBag.datetime = datetime;
                ViewBag.datetimeType = datetimeType;

                var rs = SQLServer.Connection.Query<Models.DICHVU_VT_BKN>("SELECT * FROM DICHVU_VT_BKN WHERE FLAG>0");//.DICHVU_VT_BKN.Where(m => m.FLAG > 0);

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.TEN_DVVT.ToLower().Contains(searchString.ToLower()) ||
                    d.MA_DVVT.ToLower().Contains(searchString.ToLower()));

                if (!String.IsNullOrEmpty(datetime))
                {
                    var date = datetime.Split('-');
                    if (date.Length > 1)
                    {
                        var dateStart = TM.Format.Formating.StartOfDate(TM.Format.Formating.DateParseExactVNToEN(date[0]));
                        var dateEnd = TM.Format.Formating.EndOfDate(TM.Format.Formating.DateParseExactVNToEN(date[1]));
                        rs = datetimeType == 0 ? rs.Where(d => d.CREATEDAT >= dateStart && d.CREATEDAT <= dateEnd) : rs.Where(d => d.UPDATEDAT >= dateStart && d.UPDATEDAT <= dateEnd);
                    }
                }

                if (flag == 0) rs = rs.Where(d => d.FLAG == 0);
                else rs = rs.Where(d => d.FLAG > 0);

                switch (order)
                {
                    case "ten_asc":
                        rs = rs.OrderBy(d => d.TEN_DVVT);
                        break;
                    case "ten_desc":
                        rs = rs.OrderByDescending(d => d.TEN_DVVT);
                        break;
                    case "ma_asc":
                        rs = rs.OrderBy(d => d.MA_DVVT);
                        break;
                    case "ma_desc":
                        rs = rs.OrderByDescending(d => d.MA_DVVT);
                        break;
                    case "id_asc":
                        rs = rs.OrderBy(d => d.DICHVUVT_ID);
                        break;
                    case "id_desc":
                        rs = rs.OrderByDescending(d => d.DICHVUVT_ID);
                        break;
                    case "orders_desc":
                        rs = rs.OrderByDescending(d => d.ORDERS);
                        break;
                    default:
                        rs = rs.OrderByDescending(d => d.TT_UPLOAD).ThenBy(d => d.ORDERS);
                        break;
                }
                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Dịch vụ viễn thông");
                    return RedirectToAction("Index");
                }

                ViewBag.TotalRecords = rs.Count();
                int pageSize = 15;
                int pageNumber = (page ?? 1);

                return View(rs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex) { this.danger(ex.Message); }
            finally { SQLServer.Close(); }
            return View();
        }

        public ActionResult Update()
        {
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                Oracle = new TM.Connection.Oracle("HNIVNPTBACKAN1");
                var qry = "SELECT a.*,a.VI_TRI AS ORDERS FROM DICHVU_VT_BKN a";
                var hni = Oracle.Connection.Query<Models.DICHVU_VT_BKN>(qry).ToList();
                qry = "SELECT * FROM DICHVU_VT_BKN";
                var data = SQLServer.Connection.Query<Models.DICHVU_VT_BKN>(qry).ToList();
                //
                if (hni.Count < 0)
                {
                    this.warning("Hệ thống HNI không có dữ liệu!");
                    return RedirectToAction("Index");
                }
                //
                if (data.Count < 1)
                {
                    SQLServer.Connection.Insert(hni);
                    return RedirectToAction("Index");
                }
                //
                var Update_Upload = new[] { 6, 8, 9 };
                hni.AddRange(DichVuAddition());
                //
                var insert = hni;
                foreach (var i in data)
                {
                    var tmp = hni.FirstOrDefault(d => d.DICHVUVT_ID == i.DICHVUVT_ID);
                    if (tmp != null)
                    {
                        i.MA_DVVT = tmp.MA_DVVT;
                        i.TEN_DVVT = tmp.TEN_DVVT;
                        i.GHICHU = tmp.GHICHU;
                        i.TINHTP_ID = tmp.TINHTP_ID;
                        i.LOAI_HD = tmp.LOAI_HD;
                        i.MENU = tmp.MENU;
                        i.QUYCHIEU = tmp.QUYCHIEU;
                        i.ORDERS = tmp.ORDERS;
                        i.UPDATEDBY = "Admin";
                        i.UPDATEDAT = DateTime.Now;
                        i.TT_SUDUNG = tmp.TT_SUDUNG;
                        i.TT_UPLOAD = tmp.TT_UPLOAD;
                        if (i.TT_UPLOAD == 0 && Update_Upload.Contains(i.DICHVUVT_ID))
                            i.TT_UPLOAD = 1;
                        i.FLAG = 1;
                        insert.Remove(tmp);
                    }
                }
                foreach (var i in insert)
                {
                    i.CREATEDBY = "Admin";
                    i.CREATEDAT = DateTime.Now;
                    i.UPDATEDBY = i.CREATEDBY;
                    i.UPDATEDAT = i.CREATEDAT;
                    if (i.TT_UPLOAD == 0 && Update_Upload.Contains(i.DICHVUVT_ID))
                        i.TT_UPLOAD = 1;
                    i.FLAG = 1;
                }
                SQLServer.Connection.Update(data);
                if (insert.Count > 0)
                    SQLServer.Connection.Insert(insert);
                this.success("Cập nhật dịch vụ viễn thông thành công!");
                qry = "UPDATE DICHVU_VT_BKN SET ORDERS=999 WHERE orders IS NULL";
                SQLServer.Connection.Query(qry);
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            finally
            {
                SQLServer.Close();
                Oracle.Close();
            }
            return RedirectToAction("Index");
        }
        private List<Models.DICHVU_VT_BKN> DichVuAddition()
        {
            return new List<Models.DICHVU_VT_BKN>()
            {
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9001,MA_DVVT="GNR_KM",TEN_DVVT="Khuyến Mại",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=1,ORDERS=100,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9002,MA_DVVT="GNR_TTT",TEN_DVVT="Thanh Toán trước",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=1,ORDERS=101,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9003,MA_DVVT="GNR_DC",TEN_DVVT="Đặt cọc",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=1,ORDERS=102,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9004,MA_DVVT="GNR_DATA",TEN_DVVT="Data thành phần",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=0,ORDERS=98,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9005,MA_DVVT="ITN_MGN",TEN_DVVT="Internet mergin",GHICHU="Ghép Mega - Fiber - Fiber GD",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=0,ORDERS=98,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9006,MA_DVVT="HD_MGN",TEN_DVVT="HD Mergin All",GHICHU="Ghép hóa đơn",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=0,ORDERS=99,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9661,MA_DVVT="DSL_FIX",TEN_DVVT="MegaVNN Fix",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=0,ORDERS=103,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9662,MA_DVVT="DSL_MGN",TEN_DVVT="MegaVNN Mergin",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=1,ORDERS=104,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9681,MA_DVVT="IPTV_FIX",TEN_DVVT="MyTV Fix",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=0,ORDERS=105,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9682,MA_DVVT="IPTV_MGN",TEN_DVVT="MyTV Mergin",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=1,ORDERS=106,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9691,MA_DVVT="FTTH_FIX",TEN_DVVT="Internet trên cáp quang Fix",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=0,ORDERS=107,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9692,MA_DVVT="FTTH_MGN",TEN_DVVT="Internet trên cáp quang Mergin",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=1,ORDERS=108,FLAG=1},
                new Models.DICHVU_VT_BKN(){DICHVUVT_ID=9693,MA_DVVT="FTTH_GD",TEN_DVVT="Internet trên cáp quang gia đình",TT_SUDUNG=2,CREATEDBY="Admin",CREATEDAT=DateTime.Now,TT_UPLOAD=1,ORDERS=99,FLAG=1},
            };
        }
        [HttpGet]
        public async Task<JsonResult> update_flag(string uid)
        {
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                var data = SQLServer.Connection.Query<Models.DICHVU_VT_BKN>($"SELECT * FROM DICHVU_VT_BKN WHERE IN({uid})").ToList();
                var FLAG = 0;
                foreach (var i in data)
                    i.FLAG = FLAG = i.FLAG == 1 ? 0 : 1;
                await SQLServer.Connection.UpdateAsync(data);
                return Json(new { success = (FLAG == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
    }
}
