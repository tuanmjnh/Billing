using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Dapper.Contrib;
using TM.Helper;

namespace Billing.Controllers
{
    public class BaseController : Controller
    {
        public Models.MainContext db = new Models.MainContext();
        public ActionResult DownloadFiles(string dir, string DestName = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(DestName))
                {
                    dir = dir.TrimEnd('/', '\\');
                    return TM.IO.FileDirectory.FileContentResult(HttpUtility.UrlDecode(dir), DestName);
                }
                else
                    return TM.IO.FileDirectory.FileContentResult(HttpUtility.UrlDecode(dir));
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }
        public string UrlDownloadFiles(string dir, string DestName = null)
        {
            var rs = Url.Action("DownloadFiles", new { dir = dir, DestName = DestName });
            return rs;
        }
        public static System.Data.SqlClient.SqlConnection Connection()
        {
            try
            {
                return TM.SQL.DBStatic.Connection("MainContext");
            }
            catch (Exception) { return null; }
        }
        public static void ConnectionString()
        {
            TM.SQL.DBStatic.ConstantConnectionString = "MainContext";
        }
        //public static string GetUser(string id)
        //{
        //    using (var dbs = new Models.MainContext())
        //    {
        //        if (id != null)
        //        {
        //            var rs = dbs.users.Find(Guid.Parse(id));
        //            if (rs != null)
        //                if (!String.IsNullOrEmpty(rs.full_name))
        //                    return rs.full_name;
        //                else return rs.username;
        //            else return TM.Common.Language.emptyvl;
        //        }
        //        else return TM.Common.Language.emptyvl;
        //    }
        //}
        //public List<Models.group> getGroups(string AppKey, int flag)
        //{
        //    try
        //    {
        //        return db.groups.Where(d => d.app_key == AppKey && d.flag == flag).OrderBy(d => d.title).ToList();
        //    }
        //    catch (Exception) { return null; }
        //}
        //public List<Models.group> getGroups(string AppKey)
        //{
        //    try
        //    {
        //        return db.groups.Where(d => d.app_key == AppKey && d.flag > 0).OrderBy(d => d.title).ThenBy(d => d.level).ToList();
        //    }
        //    catch (Exception) { return null; }
        //}
        //public static List<Models.setting> AllSetting { get; set; }
        public static List<dynamic> AllSetting { get; set; }
        bool setting = setSetting();
        public static List<dynamic> Settings(string module_key)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals("module_key")).ToList();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key",
                    new { module_key = module_key }).ToList();
            }
            catch (Exception) { return null; }
        }
        public static List<dynamic> Settings(string module_key, string sub_key)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals(module_key) && s.sub_key.Equals(sub_key)).ToList();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key AND sub_key=@sub_key",
                new { module_key = module_key, sub_key = sub_key }).ToList();
            }
            catch (Exception) { return null; }
        }
        public static dynamic Setting(string module_key, string sub_key, string value)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals(module_key) && s.sub_key.Equals(sub_key) && s.value.Equals(value)).FirstOrDefault();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key AND sub_key=@sub_key AND value=@value",
                new { module_key = module_key, sub_key = sub_key, value = value }).First();
            }
            catch (Exception) { return null; }
        }
        public static string Value(string module_key, string sub_key)
        {
            try
            {
                return Settings(module_key, sub_key).First().value;
            }
            catch (Exception) { return null; }
        }
        public static string SubValue(string module_key, string sub_key, string value)
        {
            try
            {
                return Setting(module_key, sub_key, value).sub_value;
            }
            catch (Exception) { return null; }
        }
        public static bool setSetting()
        {
            try
            {
                if (AllSetting == null) LoadSetting();
                return true;
            }
            catch (Exception) { return false; }
        }
        public static void LoadSetting()
        {
            try
            {
                AllSetting = Connection().Query("SELECT * FROM settings").ToList();
            }
            catch (Exception) { }
            //using (var dbs = new Models.PortalContext())
            //{
            //    AllSetting = dbs.settings.ToList();
            //}
        }
        //public static string Root = SettingKey("host").value;

        //public static List<Models.setting> Setting()
        //{
        //    using (var dbs = new Models.PortalContext())
        //    {
        //        return dbs.settings.ToList();
        //    }
        //}
        //public static Models.setting SettingKey(string app_key)
        //{
        //    return Setting().Where(s => s.app_key == app_key).FirstOrDefault();
        //}
        public static string getMonthYear(string str)
        {
            try
            {
                return str[0].ToString() + str[1].ToString() + "/20" + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static string getMonthDayYear(string str)
        {
            try
            {
                return str[0].ToString() + str[1].ToString() + "/1/20" + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static string getYearMonth(string str)
        {
            try
            {
                return str[4].ToString() + str[5].ToString() + "/" + str[0].ToString() + str[1].ToString() + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static bool ReExtensionToLower(string DataSource)
        {
            using (FileManagerController f = new FileManagerController())
            {
                return f.ExtToLower(DataSource);
            }
        }

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
        public class ValidateJsonAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationContext filterContext)
            {
                if (filterContext == null)
                    throw new ArgumentNullException("filterContext");
                var httpContext = filterContext.HttpContext;
                var cookie = httpContext.Request.Cookies[System.Web.Helpers.AntiForgeryConfig.CookieName];
                System.Web.Helpers.AntiForgery.Validate(cookie != null ? cookie.Value : null, httpContext.Request.Headers["__RequestVerificationToken"]);
            }
        }

        protected override void Dispose(bool disposing)
        {
            //if (RoleManager != null) RoleManager.Dispose();
            //if (UserManager != null) UserManager.Dispose();
            if (db != null) db.Dispose();
            base.Dispose(disposing);
        }
        public int UploadBase(string DataSource, string strResult = null, List<string> fileUpload = null, string Extension = ".dbf")
        {
            try
            {
                int uploadedCount = 0;
                if (Request.Files.Count > 0)
                {
                    FileManagerController.InsertDirectory(DataSource, false);
                    if (fileUpload == null) fileUpload = new List<string>();
                    var fileSavePath = new List<string>();
                    //Delete old File
                    //TM.IO.Delete(obj.DataSource, TM.IO.Files(obj.DataSource));

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (!file.FileName.IsExtension(Extension))
                            return (int)Common.Objects.ResultCode._extension;

                        if (file.ContentLength > 0)
                        {
                            if (fileUpload.Count < 1)
                                fileUpload.Add(file.FileName.ToLower());//System.IO.Path.GetFileName(
                            fileSavePath.Add(DataSource + fileUpload[i]);
                            file.SaveAs(System.IO.Path.Combine(fileSavePath[i]));
                            uploadedCount++;
                            FileManagerController.InsertFile(DataSource + fileUpload[i], false);
                        }
                    }
                    var rs = "Tải lên thành công </br>";
                    foreach (var item in fileUpload) rs += item + "<br/>";
                    strResult = rs;
                    return (int)Common.Objects.ResultCode._success;
                }
                else
                    return (int)Common.Objects.ResultCode._length;

            }
            catch (Exception) { throw; }
        }
        public string getMA_DVI(string ma_tuyen)
        {
            //1.BK Bắc Kạn
            //2.BB Ba Bể
            //3.BT Bạch Thông
            //4.CD Chợ Đồn
            //5.CM Chợ Mới
            //6.NR Na Rì
            //7.NS Ngân Sơn
            //8.PN Pác Nặm
            ma_tuyen = ma_tuyen.Substring(1, 2).ToUpper();
            switch (ma_tuyen)
            {
                case "BK":
                    return "1";
                case "BB":
                    return "2";
                case "BT":
                    return "3";
                case "CD":
                    return "4";
                case "CM":
                    return "5";
                case "NR":
                    return "6";
                case "NS":
                    return "7";
                case "PN":
                    return "8";
                default:
                    return "1";
            }
        }
        public string getMA_CBT(string ma_dvi, string ma_tuyen)
        {
            //1.BK Bắc Kạn
            //2.BB Ba Bể
            //3.BT Bạch Thông
            //4.CD Chợ Đồn
            //5.CM Chợ Mới
            //6.NR Na Rì
            //7.NS Ngân Sơn
            //8.PN Pác Nặm
            try
            {

                var ma_cbt = ma_tuyen.Substring(0, 3).ToUpper();

                switch (ma_cbt)
                {
                    case "TBK":
                        return ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TBK0", ma_dvi) : ma_tuyen.Replace("TBK", ma_dvi);
                    case "TBB":
                        return ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TBB0", ma_dvi) : ma_tuyen.Replace("TBB", ma_dvi);
                    case "TBT":
                        return ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TBT0", ma_dvi) : ma_tuyen.Replace("TBT", ma_dvi);
                    case "TCD":
                        return ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TCD0", ma_dvi) : ma_tuyen.Replace("TCD", ma_dvi);
                    case "TCM":
                        return ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TCM0", ma_dvi) : ma_tuyen.Replace("TCM", ma_dvi);
                    case "TNR":
                        return ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TNR0", ma_dvi) : ma_tuyen.Replace("TNR", ma_dvi);
                    case "TNS":
                        return ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TNS0", ma_dvi) : ma_tuyen.Replace("TNS", ma_dvi);
                    case "TPN":
                        return ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TPN0", ma_dvi) : ma_tuyen.Replace("TPN", ma_dvi);
                    default: return $"{ma_dvi}01";
                }

                //if (ma_cbt == "TBB")
                //    ma_cbt = ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TBB0", ma_dvi) : ma_tuyen.Replace("TBB", ma_dvi);
                //else if (ma_cbt == "TBT")
                //    ma_cbt = ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TBT0", ma_dvi) : ma_tuyen.Replace("TBT", ma_dvi);
                //else if (ma_cbt == "TCD")
                //    ma_cbt = ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TCD0", ma_dvi) : ma_tuyen.Replace("TCD", ma_dvi);
                //else if (ma_cbt == "TCM")
                //    ma_cbt = ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TCM0", ma_dvi) : ma_tuyen.Replace("TCM", ma_dvi);
                //else if (ma_cbt == "TNR")
                //    ma_cbt = ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TNR0", ma_dvi) : ma_tuyen.Replace("TNR", ma_dvi);
                //else if (ma_cbt == "TNS")
                //    ma_cbt = ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TNS0", ma_dvi) : ma_tuyen.Replace("TNS", ma_dvi);
                //else if (ma_cbt == "TPN")
                //    ma_cbt = ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TPN0", ma_dvi) : ma_tuyen.Replace("TPN", ma_dvi);
                //else
                //    ma_cbt = ma_tuyen.Length <= 6 ? ma_tuyen.Replace("TBK0", ma_dvi) : ma_tuyen.Replace("TBK", ma_dvi);
                //return ma_cbt;
            }
            catch (Exception) { return null; }
        }
        public string getMA_TUYEN(string ma_dvi)
        {
            //1.BK Bắc Kạn
            //2.BB Ba Bể
            //3.BT Bạch Thông
            //4.CD Chợ Đồn
            //5.CM Chợ Mới
            //6.NR Na Rì
            //7.NS Ngân Sơn
            //8.PN Pác Nặm
            switch (ma_dvi)
            {
                case "2":
                    return "TBB001";
                case "3":
                    return "TBT001";
                case "4":
                    return "TCD001";
                case "5":
                    return "TCM001";
                case "6":
                    return "TNR001";
                case "7":
                    return "TNS001";
                case "8":
                    return "TPN001";
                default:
                    return "TBK001";
            }
        }
    }
}