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
    public class HDDTBillController : BaseController
    {
        string app_id = "app_id";
        public ActionResult Index()
        {
            try
            {
                FileManagerController.InsertDirectory(Common.Directories.HDData);
                ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(Common.Directories.HDData).OrderByDescending(d => d).ToList();
            }
            catch (Exception ex) { this.danger(ex.Message); }
            return View();
        }
        //Upload
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GeneralUpload(Common.DefaultObj obj)
        {
            var index = 0;
            obj.DataSource = Common.Directories.HDDataSourceGeneral;
            obj = getDefaultObj(obj);
            string strUpload = "Tải tệp thành công!";
            try
            {
                TM.IO.FileDirectory.CreateDirectory(obj.DataSource, false);
                var uploadData = UploadBase(obj.DataSource, strUpload);
                if (uploadData == (int)Common.Objects.ResultCode._extension)
                    return Json(new { danger = "Tệp phải định dạng .dbf!" }, JsonRequestBehavior.AllowGet);
                else if (uploadData == (int)Common.Objects.ResultCode._length)
                    return Json(new { danger = "Chưa đủ tệp!" }, JsonRequestBehavior.AllowGet);
                //else if (uploadData == (int)Common.Objects.ResultCode._success)
                //    return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = strUpload }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        //Tạo file hóa đơn XML
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyHD(Common.DefaultObj obj)
        {
            var ok = new System.Text.StringBuilder();
            var err = new System.Text.StringBuilder();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            var hddttime = obj.datetime.ToString("MMyyyy");
            var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
            var indexError = "";
            try
            {
                var hdall = "hdall" + obj.time;
                var hdallhddt = "hdall_hddt" + obj.time;
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + obj.time + "\\";
                //Khai báo biến
                var listKey = new List<string>();
                var NumberToLeter = new TM.Helper.NumberToLeter();
                var hasError = false;
                string fileName = "hoadon";
                string fileNameDBFHDDT = TM.OleDBF.DataSource + hdallhddt + ".dbf";
                string fileNameXMLFull = TM.OleDBF.DataSource + fileName + ".xml";
                string fileNameZIPFull = TM.OleDBF.DataSource + fileName + ".zip";
                string fileNameXMLError = TM.OleDBF.DataSource + fileName + "_Error.xml";
                //Xóa file HDDT cũ
                FileManagerController.DeleteDirFile(fileNameDBFHDDT);
                FileManagerController.DeleteDirFile(fileNameZIPFull);
                FileManagerController.DeleteDirFile(fileNameXMLError);
                //
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hdall + ".dbf", fileNameDBFHDDT);
                //Xử lý trùng mã thanh toán khác đơn vị
                RemoveDuplicate(TM.OleDBF.DataSource, hdallhddt,
                    new string[] { "tong_cd", "tong_dd", "tong_net", "tong_tv", "vat", "tong", "kthue", "giam_tru", "tongcong" },
                    new string[] { "acc_net", "acc_tv", "so_dd", "so_cd", "diachi_tt", "ma_tuyen", "ms_thue", "ma_dt", "ma_cbt", "banknumber" },
                    "Remove Duplicate hoadon", null);
                //Get data from HDDT
                var data = FoxPro.Connection.Query<Models.HD_DT>($"SELECT * FROM {hdallhddt}").ToList().Trim(); //TM.OleDBF.ToDataTable("SELECT * FROM " + hdallhddt);
                //HDDT Hoa don
                using (System.IO.Stream outfile = new System.IO.FileStream(TM.IO.FileDirectory.MapPath(fileNameXMLFull), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
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
                                            $"<MaThanhToan><![CDATA[{getMaThanhToanHD(hddttime, i.MA_TT_HNI, i.MA_IN == 2 ? Common.HDDT.DetailDiDong : Common.HDDT.DetailCoDinh)}]]></MaThanhToan>" +
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
                            Invoice = $"<Inv><key>{i.MA_TT_HNI}{hddttime}</key>{Invoice}</Inv>";
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
                    using (System.IO.Stream outfile = new System.IO.FileStream(TM.IO.FileDirectory.MapPath(fileNameXMLError), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                    {
                        err.Append("</Invoices>");
                        outfile.Write(System.Text.Encoding.UTF8.GetBytes(err.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(err.ToString()));
                        outfile.Close();
                    }
                    //this.danger("Có lỗi xảy ra Truy cập File Manager \"~\\" + TM.OleDBF.DataSource.Replace("Uploads\\", "") + "\" để tải file");
                }
                //ZipFile
                if (obj.ckhZipFile)
                    TM.IO.Zip.ZipFile(new List<string>() { fileNameXMLFull }, fileNameZIPFull, 9);
                //Insert To File Manager
                FileManagerController.DeleteDirFile(fileNameXMLFull);
                FileManagerController.InsertFile(fileNameZIPFull);
                FileManagerController.InsertFile(fileNameXMLError);
                return Json(new { success = $"Tạo hóa đơn điện tử thành công! Truy cập File Manager \"~\\{TM.OleDBF.DataSource.Replace("Uploads\\", "")}\" để tải file" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = $"{ex.Message} - Index: {index}" }, JsonRequestBehavior.AllowGet); }
            finally { FoxPro.Close(); }
        }
        //Tạo file khách hàng XML
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyKH(Common.DefaultObj obj)
        {
            var ok = new System.Text.StringBuilder();
            var err = new System.Text.StringBuilder();
            var index = 0;
            obj.DataSource = Common.Directories.HDData;
            obj = getDefaultObj(obj);
            var FoxPro = new TM.Connection.OleDBF(obj.DataSource);
            try
            {

                var listKey = new List<string>();
                var hasError = false;
                var fileName = "cus";
                var hdallhddt = "hdall_hddt" + obj.time;
                var fileNameDBFHDDT = TM.OleDBF.DataSource + hdallhddt + ".dbf";
                var fileNameXMLFull = TM.OleDBF.DataSource + fileName + ".xml";
                var fileNameZIPFull = TM.OleDBF.DataSource + fileName + ".zip";
                var fileNameXMLError = TM.OleDBF.DataSource + fileName + "_Error.xml";
                //Xóa file HDDT cũ
                FileManagerController.DeleteDirFile(fileNameZIPFull);
                FileManagerController.DeleteDirFile(fileNameXMLError);
                //Get data from HDDT
                var data = FoxPro.Connection.Query<Models.HD_DT>($"SELECT * FROM {hdallhddt}").ToList().Trim(); //TM.OleDBF.ToDataTable("SELECT * FROM " + hdallhddt);
                using (System.IO.Stream outfile = new System.IO.FileStream(TM.IO.FileDirectory.MapPath(fileNameXMLFull), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    ok.Append("<Customers>").AppendLine();
                    err.Append("<Customers>").AppendLine();
                    foreach (var i in data)
                    {
                        index++;
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
                                                $"<MaThanhToan><![CDATA[{getMaThanhToanKH(obj.month_year_time, i.MA_TT_HNI, i.MA_IN == 2 ? Common.HDDT.DetailDiDong : Common.HDDT.DetailCoDinh)}]]></MaThanhToan>" +
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
                    using (System.IO.Stream outfile = new System.IO.FileStream(TM.IO.FileDirectory.MapPath(fileNameXMLError), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                    {
                        err.Append("</Customers>");
                        outfile.Write(System.Text.Encoding.UTF8.GetBytes(err.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(err.ToString()));
                        outfile.Close();
                    }
                    //this.danger("Có lỗi xảy ra Truy cập File Manager \"~\\" + TM.OleDBF.DataSource.Replace("Uploads\\", "") + "\" để tải file");
                }
                //ZipFile
                if (obj.ckhZipFile)
                    TM.IO.Zip.ZipFile(new List<string>() { fileNameXMLFull }, fileNameZIPFull, 9);
                //Insert To File Manager
                FileManagerController.DeleteDirFile(fileNameXMLFull);
                FileManagerController.InsertFile(fileNameZIPFull);
                FileManagerController.InsertFile(fileNameXMLError);
                //Insert To File Manager hdallhddt
                FileManagerController.InsertFile(fileNameDBFHDDT);
                return Json(new { success = $"Tạo hóa đơn điện tử thành công! Truy cập File Manager \"~\\{TM.OleDBF.DataSource.Replace("Uploads\\", "")}\" để tải file" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = $"{ex.Message} - Index: {index}" }, JsonRequestBehavior.AllowGet); }
            finally { FoxPro.Close(); }
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
    }
}