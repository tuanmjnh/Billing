using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Helper;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Billing.Controllers
{
    [Filters.Auth(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.managerBill)]
    public class FileManagerController : BaseController
    {
        TM.Connection.SQLServer SQLServer;
        //public ActionResult Index(string dir, int? flag, string order, string currentFilter, string searchString, int? page, string export)
        //{
        //    if (searchString != null)
        //    {
        //        page = 1;
        //        searchString = searchString.Trim();
        //    }
        //    else searchString = currentFilter;

        //    ViewBag.currentFilter = searchString;
        //    ViewBag.flag = flag;

        //    string path = "~/Uploads" + (dir != null ? dir : "");
        //    ViewBag.directory = TM.IO.FileDirectory.Directories(path);//TM.IO.Directories(path).OrderByDescending(d => d.Name).ToList();
        //    ViewBag.files = TM.IO.FileDirectory.Files(path, new[] { ".dbf", ".txt", ".xls", ".xlsx" });

        //    //Export to any
        //    //if (!String.IsNullOrEmpty(export))
        //    //{
        //    //    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách tài khoản");
        //    //    return RedirectToAction("Index");
        //    //}
        //    return View();
        //}
        //public ActionResult Index(string dir, int? flag, string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, string export)
        //{
        //    try
        //    {
        //        if (searchString != null)
        //        {
        //            page = 1;
        //            searchString = searchString.Trim();
        //        }
        //        else searchString = currentFilter;
        //        ViewBag.dir = dir;
        //        ViewBag.order = order;
        //        ViewBag.currentFilter = searchString;
        //        ViewBag.flag = flag;
        //        ViewBag.datetime = datetime;
        //        ViewBag.datetimeType = datetimeType;

        //        var extension = new[] { ".dbf", ".txt", ".xls", ".xlsx", ".zip", ".xml" };

        //        var rs = db.FILE_MANAGER.ToList().AsEnumerable();
        //        //if (!TM.Common.Auth.inRoles(new[] { TM.Common.roles.admin, TM.Common.roles.superadmin }))
        //        //    rs = rs.Where(d => extension.Contains(d.EXTENSION) || d.TYPE == TM.Common.Objects.FileManager.directory);

        //        if (!String.IsNullOrEmpty(dir))
        //            rs = rs.Where(d => d.SUBDIRECTORY == dir);
        //        else
        //            rs = rs.Where(d => d.LEVELS == 0);

        //        if (!String.IsNullOrEmpty(searchString))
        //            rs = rs.Where(d =>
        //            d.NAME.Contains(searchString) ||
        //            d.FULLNAME.Contains(searchString));

        //        if (!String.IsNullOrEmpty(datetime))
        //        {
        //            var date = datetime.Split('-');
        //            if (date.Length > 1)
        //            {
        //                var tmp0 = date[0].Split('/');
        //                var tmp1 = date[1].Split('/');
        //                if (tmp0.Length > 2 && tmp1.Length > 2)
        //                {
        //                    rs = datetimeType == 0
        //                        ? rs.Where(d =>
        //                         d.CREATIONTIME.Value.Day >= int.Parse(tmp0[0]) &&
        //                         d.CREATIONTIME.Value.Month >= int.Parse(tmp0[1]) &&
        //                         d.CREATIONTIME.Value.Year >= int.Parse(tmp0[2]) &&
        //                         d.CREATIONTIME.Value.Day <= int.Parse(tmp1[0]) &&
        //                         d.CREATIONTIME.Value.Month <= int.Parse(tmp1[1]) &&
        //                         d.CREATIONTIME.Value.Year <= int.Parse(tmp1[2]))
        //                        : rs.Where(d =>
        //                         d.LASTWRITETIME.Value.Day >= int.Parse(tmp0[0]) &&
        //                         d.LASTWRITETIME.Value.Month >= int.Parse(tmp0[1]) &&
        //                         d.LASTWRITETIME.Value.Year >= int.Parse(tmp0[2]) &&
        //                         d.LASTWRITETIME.Value.Day <= int.Parse(tmp1[0]) &&
        //                         d.LASTWRITETIME.Value.Month <= int.Parse(tmp1[1]) &&
        //                         d.LASTWRITETIME.Value.Year <= int.Parse(tmp1[2]));
        //                }
        //            }
        //        }

        //        if (flag == 0) rs = rs.Where(d => d.FLAG == 0);
        //        else rs = rs.Where(d => d.FLAG > 0);

        //        switch (order)
        //        {
        //            default:
        //                rs = rs.OrderByDescending(d => d.CREATIONTIME);
        //                break;
        //            case "name_asc":
        //                rs = rs.OrderBy(d => d.NAME);
        //                break;
        //            case "name_desc":
        //                rs = rs.OrderByDescending(d => d.NAME);
        //                break;
        //            case "type_asc":
        //                rs = rs.OrderBy(d => d.TYPE);
        //                break;
        //            case "type_desc":
        //                rs = rs.OrderByDescending(d => d.TYPE);
        //                break;
        //            case "size_asc":
        //                rs = rs.OrderBy(d => d.LENGTH);
        //                break;
        //            case "size_asc_desc":
        //                rs = rs.OrderByDescending(d => d.LENGTH);
        //                break;
        //            case "createdat_asc":
        //                rs = rs.OrderBy(d => d.CREATIONTIME);
        //                break;
        //            case "createdat_desc":
        //                rs = rs.OrderByDescending(d => d.CREATIONTIME);
        //                break;
        //            case "updatedat_asc":
        //                rs = rs.OrderBy(d => d.LASTWRITETIME);
        //                break;
        //            case "updatedat_desc":
        //                rs = rs.OrderByDescending(d => d.LASTWRITETIME);
        //                break;
        //        }

        //        //Export to any
        //        if (!String.IsNullOrEmpty(export))
        //        {
        //            TM.Exports.ExportExcel(Data.ToDataTable(rs.ToList()), "FileManagerList");
        //            return RedirectToAction("Index");
        //        }

        //        ViewBag.TotalRecords = rs.Count();
        //        int pageSize = 15;
        //        int pageNumber = (page ?? 1);

        //        return View(rs.ToPagedList(pageNumber, pageSize));
        //    }
        //    catch (Exception ex)
        //    {
        //        this.danger(ex.Message);
        //    }
        //    return View();
        //}
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult Select(objBST obj)//string sort, string order, string search, int offset = 0, int limit = 10, int flag = 1
        {
            var index = 0;
            var qry = "";
            var cdt = "";
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                obj.extension = "'.dbf','.txt','.xls','.xlsx','.zip','.xml'";
                //Select
                qry = $"SELECT * FROM FILE_MANAGER WHERE FLAG={obj.flag}";
                //Extension
                if (!Authentication.Auth.inRoles(new[] { Authentication.Roles.admin, Authentication.Roles.superadmin }))
                    if (!string.IsNullOrEmpty(obj.extension)) cdt += $"(EXTENSION IN({obj.extension}) OR TYPE='{TM.Common.Objects.FileManager.directory}') AND ";
                //Sub Dir
                if (!string.IsNullOrEmpty(obj.subDir))
                    cdt += $"SUBDIRECTORY IN('{obj.subDir}') AND ";
                else
                    cdt += $"LEVELS=0 AND ";
                //Get data for Search
                //if (!String.IsNullOrEmpty(obj.search) && obj.search.isNumber())
                //    cdt += $"(GOICUOCID={obj.search}) AND ";
                if (!string.IsNullOrEmpty(obj.search))
                    cdt += $"(PARENT LIKE '%{obj.search}%' OR NAME LIKE '%{obj.search}%' OR SUBDIRECTORY LIKE '%,{obj.search},%' OR EXTENSION LIKE '%,{obj.search},%') AND ";
                //
                if (!string.IsNullOrEmpty(cdt))
                    qry += $" AND {cdt.Substring(0, cdt.Length - 5)}";

                //export
                if (obj.export == 1)
                {
                    //var startDate = DateTime.ParseExact($"{obj.startDate}", "dd/MM/yyyy HH:mm", provider);
                    //var endDate = DateTime.ParseExact($"{obj.endDate}", "dd/MM/yyyy HH:mm", provider);
                    //qry += $" AND tb.FLAG=2 AND tb.UPDATEDAT>=CAST('{startDate.ToString("yyyy-MM-dd")}' as datetime) AND tb.UPDATEDAT<=CAST('{endDate.ToString("yyyy-MM-dd")}' as datetime) ORDER BY tb.MA_DVI,tb.UPDATEDAT";
                    //var export = SQLServer.Connection.Query<Portal.Areas.ND49.Models.ND49Export>(qry);
                    //qry = "SELECT * FROM users";
                    //var user = SQLServer.Connection.Query<Authentication.user>(qry);
                    //foreach (var i in export)
                    //{
                    //    var tmp = user.FirstOrDefault(d => d.username == i.NVQL);
                    //    i.TEN_NVQL = tmp != null ? tmp.full_name : null;
                    //}
                    //var rsJson = Json(new { data = export, SHA = Guid.NewGuid() }, JsonRequestBehavior.AllowGet);
                    //rsJson.MaxJsonLength = int.MaxValue;
                    //return rsJson;
                }
                //
                var data = SQLServer.Connection.Query<Models.FILE_MANAGER>(qry);

                if (data.ToList().Count < 1)
                    return Json(new { total = 0, rows = data }, JsonRequestBehavior.AllowGet);
                //Get total item
                var total = data.Count();
                //Sort And Orders
                if (!string.IsNullOrEmpty(obj.sort))
                {
                    if (obj.sort.ToUpper() == "NAME" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.NAME);
                    else if (obj.sort.ToUpper() == "NAME" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.NAME);
                    else if (obj.sort.ToUpper() == "ATTRIBUTES" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.ATTRIBUTES);
                    else if (obj.sort.ToUpper() == "ATTRIBUTES" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.ATTRIBUTES);
                    else if (obj.sort.ToUpper() == "LENGTH" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.LENGTH);
                    else if (obj.sort.ToUpper() == "LENGTH" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.LENGTH);
                    else if (obj.sort.ToUpper() == "CREATIONTIME" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.CREATIONTIME);
                    else if (obj.sort.ToUpper() == "CREATIONTIME" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.CREATIONTIME);
                    else if (obj.sort.ToUpper() == "LASTWRITETIME" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.LASTWRITETIME);
                    else if (obj.sort.ToUpper() == "LASTWRITETIME" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.LASTWRITETIME);
                    else
                        data = data.OrderByDescending(m => m.CREATIONTIME).ThenBy(m => m.NAME);
                }
                else
                    data = data.OrderByDescending(m => m.CREATIONTIME).ThenBy(m => m.NAME);
                //Page Site
                var rs = data.Skip(obj.offset).Take(obj.limit).ToList();
                var ReturnJson = Json(new { total = total, rows = rs }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception) { return Json(new { danger = "Không tìm thấy dữ liệu, vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
            //return Json(new { success = "Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Download(string files)
        {
            try
            {
                string[] id = files.Split(',');
                if (id.Length == 1)
                {
                    var file = db.FILE_MANAGER.Find(Guid.Parse(id[0]));
                    if (file == null)
                        file = db.FILE_MANAGER.FirstOrDefault(m => m.NAME == id[0]);
                    if (file != null)
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(TM.IO.FileDirectory.MapPath(TM.Common.Directories.Uploads) + file.SUBDIRECTORY + file.NAME);
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file.NAME);
                        //return Json(new { success = "Thành công", url = TM.Common.Directories.Uploads.Trim('\\') + file.SUBDIRECTORY + file.NAME }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (files.Length > 1)
                {
                    List<Guid> listID = new List<Guid>();
                    foreach (var i in id) listID.Add(Guid.Parse(i));
                    var list = db.FILE_MANAGER.Where(d => listID.Contains(d.ID)).ToList();

                    var listFile = new List<string>();
                    foreach (var item in list) listFile.Add(TM.Common.Directories.Uploads + item.SUBDIRECTORY + item.NAME);
                    TM.IO.Zip.DownloadZipToBrowser(listFile);
                    //foreach (var item in id)
                    //{
                    //    var file = db.FileManagers.Find(Guid.Parse(item));
                    //    byte[] fileBytes = System.IO.File.ReadAllBytes(TM.IO.FileDirectory.MapPath(TM.Common.Directories.Uploads) + file.Subdirectory + file.Name);
                    //    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file.Name);
                    //}
                    //return Json(new { success = "Thành công" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = "Thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        //[HttpGet]
        //public ActionResult Download(string dir, string files)
        //{
        //    return TM.IO.FileDirectory.FileContentResult("~/Uploads/" + HttpUtility.UrlDecode(dir) + "/" + files);
        //}
        public JsonResult LoadData()
        {
            try
            {
                var sqldb = new TM.SQL.DB();
                sqldb.Execute("delete from FILE_MANAGER");
                if (InsertDirectoriesFiles(TM.Common.Directories.Uploads))
                    return Json(new { success = "Cập nhật data thành công!" }, JsonRequestBehavior.AllowGet);
                return Json(new { danger = "Lỗi trong quá trình cập nhật data!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult ExtensionToLower()
        {
            try
            {
                if (ExtToLower(TM.Common.Directories.Uploads))
                    return Json(new { success = "Cập nhật Extension thành công!" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { danger = "Lỗi trong quá trình cập nhật Extension!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        public static bool InsertDirectoriesFiles(string path)
        {
            try
            {
                using (var db = new Models.MainContext())
                {
                    var all = getDirectoriesFiles(path.Replace('/', '\\'));
                    foreach (var item in all)
                    {
                        var fullName = item.Key.FullName.Trim('\\');
                        var FileManager = db.FILE_MANAGER.Where(d => d.FULLNAME == fullName).FirstOrDefault();
                        if (FileManager == null)
                        {
                            FileManager = new Models.FILE_MANAGER();
                            FileManager.ID = Guid.NewGuid();
                            FileManager.PARENTID = Guid.Empty;
                            FileManager.PARENT = item.Key.Parent.ToString();
                            FileManager.ROOT = item.Key.Root.ToString();
                            FileManager.SUBDIRECTORY = "\\";
                            FileManager.LEVELS = 0;
                            FileManager.NAME = item.Key.Name;
                            FileManager.FULLNAME = item.Key.FullName;
                            FileManager.EXTENSION = item.Key.Extension;
                            FileManager.EXTENSIONICON = "fa fa-folder";
                            FileManager.TYPE = string.IsNullOrEmpty(item.Key.Extension) ? TM.Common.Objects.FileManager.directory : TM.Common.Objects.FileManager.file;
                            FileManager.ATTRIBUTES = item.Key.Attributes.ToString();
                            FileManager.ATTRIBUTESENUM = (int)item.Key.Attributes;//item.Key.Attributes.ToString();
                                                                                  //FileManager.Description = null;
                            FileManager.CREATIONTIME = item.Key.CreationTime;
                            FileManager.CREATIONTIMEUTC = item.Key.CreationTimeUtc;
                            FileManager.LASTACCESSTIME = item.Key.LastAccessTime;
                            FileManager.LASTACCESSTIMEUTC = item.Key.LastAccessTimeUtc;
                            FileManager.LASTWRITETIME = item.Key.LastWriteTime;
                            FileManager.LASTWRITETIMEUTC = item.Key.LastWriteTimeUtc;
                            FileManager.CREATEDBY = "Admin";//TM.Common.Auth.id().ToString();
                            //FileManager.LastAccessBy = TM.Common.Auth.id().ToString();
                            //FileManager.LastWriteBy = TM.Common.Auth.id().ToString();
                            FileManager.EXISTS = item.Key.Exists;
                            FileManager.FLAG = 1;
                            if (item.Key.Parent.ToString() != TM.Common.Directories.Uploads.Trim('\\'))
                            {
                                var tmp = db.FILE_MANAGER.Local.Where(d => d.NAME == FileManager.PARENT && d.FLAG > 0).FirstOrDefault();

                                if (tmp == null) continue;
                                FileManager.PARENTID = tmp.ID;
                                FileManager.SUBDIRECTORY = tmp.SUBDIRECTORY + FileManager.PARENT + "\\";
                                FileManager.LEVELS = tmp.LEVELS + 1;
                            }
                            db.FILE_MANAGER.Add(FileManager);
                        }
                        //
                        foreach (var file in item.Value)
                        {
                            if (file.Name == "Thumbs.db") continue;
                            fullName = file.FullName.Trim('\\');
                            var FileItem = db.FILE_MANAGER.Where(d => d.FULLNAME == fullName).FirstOrDefault();
                            if (FileItem != null)
                            {
                                FileItem.PARENTID = FileManager.ID;
                                db.Entry(FileItem).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                continue;
                            }
                            FileItem = new Models.FILE_MANAGER();
                            FileItem.ID = Guid.NewGuid();
                            FileItem.PARENTID = FileManager.ID;
                            FileItem.PARENT = item.Key.Name;
                            FileItem.ROOT = item.Key.Root.ToString();
                            FileItem.SUBDIRECTORY = FileManager.SUBDIRECTORY + item.Key.Name + "\\";
                            FileItem.LEVELS = FileManager.LEVELS + 1;
                            FileItem.NAME = file.Name;
                            FileItem.FULLNAME = file.FullName;
                            FileItem.EXTENSION = file.Extension.ToLower();
                            FileItem.EXTENSIONICON = getExtensionIcon(FileItem.EXTENSION);
                            FileItem.TYPE = string.IsNullOrEmpty(file.Extension) ? TM.Common.Objects.FileManager.directory : TM.Common.Objects.FileManager.file;
                            FileItem.ATTRIBUTES = file.Attributes.ToString();
                            FileItem.ATTRIBUTESENUM = (int)file.Attributes;//item.Key.Attributes.ToString();
                            FileItem.LENGTH = file.Length;
                            FileItem.ISREADONLY = file.IsReadOnly;
                            //FileItem.Description = null;
                            FileItem.CREATIONTIME = file.CreationTime;
                            FileItem.CREATIONTIMEUTC = file.CreationTimeUtc;
                            FileItem.LASTACCESSTIME = file.LastAccessTime;
                            FileItem.LASTACCESSTIMEUTC = file.LastAccessTimeUtc;
                            FileItem.LASTWRITETIME = file.LastWriteTime;
                            FileItem.LASTWRITETIMEUTC = file.LastWriteTimeUtc;
                            FileItem.CREATEDBY = "Admin";//TM.Common.Auth.id().ToString();
                            //FileItem.LastAccessBy = TM.Common.Auth.id().ToString();
                            //FileItem.LastWriteBy = TM.Common.Auth.id().ToString();
                            FileItem.EXISTS = file.Exists;
                            FileItem.FLAG = 1;
                            db.FILE_MANAGER.Add(FileItem);
                        }
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
        public static bool InsertDirectory(string directory, bool IsMapPath = true)
        {
            try
            {
                using (var db = new Models.MainContext())
                {
                    TM.IO.FileDirectory.CreateDirectory(directory, IsMapPath);
                    var path = IsMapPath ? TM.IO.FileDirectory.MapPath(directory.Replace('/', '\\')).Trim('\\') : directory.Replace('/', '\\').Trim('\\');
                    var FileManager = db.FILE_MANAGER.Where(d => d.FULLNAME == path).FirstOrDefault();
                    if (FileManager == null)
                    {
                        var item = new System.IO.DirectoryInfo(path);
                        FileManager = new Models.FILE_MANAGER();
                        FileManager.ID = Guid.NewGuid();
                        FileManager.PARENTID = Guid.Empty;
                        FileManager.PARENT = item.Parent.ToString();
                        FileManager.ROOT = item.Root.ToString();
                        FileManager.SUBDIRECTORY = "\\";
                        FileManager.LEVELS = 0;
                        FileManager.NAME = item.Name;
                        FileManager.FULLNAME = item.FullName;
                        FileManager.EXTENSION = item.Extension;
                        FileManager.EXTENSIONICON = "fa fa-folder";
                        FileManager.TYPE = string.IsNullOrEmpty(item.Extension) ? TM.Common.Objects.FileManager.directory : TM.Common.Objects.FileManager.file;
                        FileManager.ATTRIBUTES = item.Attributes.ToString();
                        FileManager.ATTRIBUTESENUM = (int)item.Attributes;//item.Key.Attributes.ToString();
                                                                          //FileManager.Description = null;
                        FileManager.CREATIONTIME = item.CreationTime;
                        FileManager.CREATIONTIMEUTC = item.CreationTimeUtc;
                        FileManager.LASTACCESSTIME = item.LastAccessTime;
                        FileManager.LASTACCESSTIMEUTC = item.LastAccessTimeUtc;
                        FileManager.LASTWRITETIME = item.LastWriteTime;
                        FileManager.LASTWRITETIMEUTC = item.LastWriteTimeUtc;
                        FileManager.CREATEDBY = "Admin";//TM.Common.Auth.id().ToString();
                        //FileManager.LastAccessBy = TM.Common.Auth.id().ToString();
                        //FileManager.LastWriteBy = TM.Common.Auth.id().ToString();
                        FileManager.EXISTS = item.Exists;
                        FileManager.FLAG = 1;
                        if (item.Parent.ToString() != TM.Common.Directories.Uploads.Trim('\\'))
                        {
                            var tmp = db.FILE_MANAGER.Where(d => d.NAME == FileManager.PARENT && d.FLAG > 0).FirstOrDefault();
                            if (tmp == null)
                            {
                                var directories = directory.Split('\\');
                                InsertDirectory(directory.Replace("\\" + FileManager.NAME, ""));
                            }
                            tmp = db.FILE_MANAGER.Where(d => d.NAME == FileManager.PARENT && d.FLAG > 0).FirstOrDefault();
                            FileManager.PARENTID = tmp.ID;
                            FileManager.SUBDIRECTORY = tmp.SUBDIRECTORY + FileManager.PARENT + "\\";
                            FileManager.LEVELS = tmp.LEVELS + 1;
                        }
                        db.FILE_MANAGER.Add(FileManager);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
        public static bool InsertFile(string file, bool IsMapPath = true)
        {
            try
            {
                using (var db = new Models.MainContext())
                {
                    file = IsMapPath ? TM.IO.FileDirectory.MapPath(file) : file;
                    var item = new System.IO.FileInfo(file);
                    item = TM.IO.FileDirectory.ReExtensionToLower(item.FullName, false);
                    var fullname = item.FullName.Replace("\\" + item.Name, "");
                    var directory = db.FILE_MANAGER.Where(d => d.FULLNAME == fullname).FirstOrDefault();
                    if (directory == null) return false;
                    if (item.Name == "Thumbs.db") return true;
                    var FileItem = db.FILE_MANAGER.Where(d => d.FULLNAME == item.FullName).FirstOrDefault();
                    if (FileItem != null)
                    {
                        FileItem.PARENTID = directory.ID;
                        db.Entry(FileItem).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                    FileItem = new Models.FILE_MANAGER();
                    FileItem.ID = Guid.NewGuid();
                    FileItem.PARENTID = directory.ID;
                    FileItem.PARENT = directory.NAME;
                    FileItem.ROOT = directory.ROOT;
                    FileItem.SUBDIRECTORY = directory.SUBDIRECTORY + directory.NAME + "\\";
                    FileItem.LEVELS = directory.LEVELS + 1;
                    FileItem.NAME = item.Name;
                    FileItem.FULLNAME = item.FullName;
                    FileItem.EXTENSION = item.Extension.ToLower();
                    FileItem.EXTENSIONICON = getExtensionIcon(FileItem.EXTENSION);
                    FileItem.TYPE = string.IsNullOrEmpty(item.Extension) ? TM.Common.Objects.FileManager.directory : TM.Common.Objects.FileManager.file;
                    FileItem.ATTRIBUTES = item.Attributes.ToString();
                    FileItem.ATTRIBUTESENUM = (int)item.Attributes;//item.Key.Attributes.ToString();
                    FileItem.LENGTH = item.Length;
                    FileItem.ISREADONLY = item.IsReadOnly;
                    //FileItem.Description = null;
                    FileItem.CREATIONTIME = item.CreationTime;
                    FileItem.CREATIONTIMEUTC = item.CreationTimeUtc;
                    FileItem.LASTACCESSTIME = item.LastAccessTime;
                    FileItem.LASTACCESSTIMEUTC = item.LastAccessTimeUtc;
                    FileItem.LASTWRITETIME = item.LastWriteTime;
                    FileItem.LASTWRITETIMEUTC = item.LastWriteTimeUtc;
                    FileItem.CREATEDBY = "Admin";//TM.Common.Auth.id().ToString();
                    //FileItem.LastAccessBy = TM.Common.Auth.id().ToString();
                    //FileItem.LastWriteBy = TM.Common.Auth.id().ToString();
                    FileItem.EXISTS = item.Exists;
                    FileItem.FLAG = 1;
                    db.FILE_MANAGER.Add(FileItem);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
        public static bool DeleteDirFile(string files, bool IsMapPath = true)
        {
            try
            {
                using (var db = new Models.MainContext())
                {
                    files = IsMapPath ? TM.IO.FileDirectory.MapPath(files) : files;
                    var rs = db.FILE_MANAGER.Where(d => d.FULLNAME == files).FirstOrDefault();

                    if (rs == null)
                    {
                        TM.IO.FileDirectory.Delete(files, false);
                        TM.IO.FileDirectory.DeleteDirectory(files, false);
                        return true;
                    }

                    db.FILE_MANAGER.Remove(rs);
                    foreach (var file in db.FILE_MANAGER.Where(d => d.PARENTID == rs.ID).ToList())
                        db.FILE_MANAGER.Remove(file);

                    if (rs.TYPE == TM.Common.Objects.FileManager.file)
                        TM.IO.FileDirectory.Delete(rs.FULLNAME, false);
                    else
                        TM.IO.FileDirectory.DeleteDirectory(rs.FULLNAME, false);

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
        public bool ExtToLower(string path)
        {
            try
            {
                var all = getDirectoriesFiles(path.Replace('/', '\\'));
                foreach (var item in all)
                    foreach (var file in item.Value)
                    {
                        var newFile = TM.IO.FileDirectory.ReExtensionToLower(file.FullName.Trim('\\'), false);
                        var FileManagers = db.FILE_MANAGER.Where(d => d.FULLNAME == file.FullName).FirstOrDefault();
                        if (FileManagers != null)
                        {
                            FileManagers.NAME = newFile.Name;
                            FileManagers.FULLNAME = newFile.FullName;
                            FileManagers.EXTENSION = newFile.Extension;
                            db.Entry(FileManagers).State = System.Data.Entity.EntityState.Modified;

                        }
                    }
                db.SaveChanges();
                return true;
            }
            catch (Exception) { return false; }
        }
        private List<System.IO.DirectoryInfo> getDirectories(string path)
        {
            var DirectoryInfo = new List<System.IO.DirectoryInfo>();
            foreach (var item in TM.IO.FileDirectory.Directories(path))
            {
                DirectoryInfo.Add(item);
                DirectoryInfo.AddRange(getDirectories(path + item.Name + "\\"));
            }
            return DirectoryInfo;
        }
        private static Dictionary<System.IO.DirectoryInfo, System.IO.FileInfo[]> getDirectoriesFiles(string path, string[] extension = null)
        {
            var DirectoryInfo = new Dictionary<System.IO.DirectoryInfo, System.IO.FileInfo[]>();
            DirectoryInfo.Add(new System.IO.DirectoryInfo(TM.IO.FileDirectory.MapPath(path)), TM.IO.FileDirectory.Files(path, extension));
            DirectoryInfo.AddRange(getDirFiles(path, extension));
            return DirectoryInfo;
        }
        private static Dictionary<System.IO.DirectoryInfo, System.IO.FileInfo[]> getDirFiles(string path, string[] extension = null)
        {
            var DirectoryInfo = new Dictionary<System.IO.DirectoryInfo, System.IO.FileInfo[]>();
            foreach (var item in TM.IO.FileDirectory.Directories(path))
            {
                var FileInfo = TM.IO.FileDirectory.Files(path + item.Name, extension);
                for (int i = 0; i < FileInfo.Length; i++)
                    FileInfo[i] = TM.IO.FileDirectory.ReExtensionToLower(FileInfo[i].FullName, false);
                FileInfo = TM.IO.FileDirectory.Files(path + item.Name, extension);
                DirectoryInfo.Add(item, FileInfo);
                DirectoryInfo.AddRange(getDirFiles(path + item.Name + "\\", extension));
            }
            return DirectoryInfo;
        }
        private static string getExtensionIcon(string extension)
        {
            if (TM.IO.FileDirectory.ImageCodecs().Contains("*" + extension.ToUpper()))
                return "fa fa-file-image-o";
            switch (extension)
            {
                default: return "fa fa-file";
                case ".xls": return "fa fa-file-excel-o";
                case ".xlsx": return "fa fa-file-excel-o";
                case ".dbf": return "fa fa-table";
                case ".txt": return "fa fa-file-text-o";
                case ".doc": return "fa fa-file-text";
                case ".docx": return "fa fa-file-text";
                case ".pdf": return "fa fa-file-pdf-o";
                case ".exe": return "fa fa-ravelry";
                case ".zip": return "fa fa-file-archive-o";
                case ".rar": return "fa fa-ticket";
                case ".xml": return "fa fa-file-code-o";
            }
        }
        public JsonResult update_flag(string uid)
        {
            try
            {
                string[] id = uid.Split(',');
                var flag = 0;
                foreach (var item in id)
                {
                    Guid tmp = Guid.Parse(item);
                    var rs = db.FILE_MANAGER.Find(tmp);
                    rs.FLAG = flag = rs.FLAG == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string[] uid)
        {
            try
            {
                foreach (var item in uid)
                {
                    Guid tmp = Guid.Parse(item);
                    var rs = db.FILE_MANAGER.Find(tmp);
                    db.FILE_MANAGER.Remove(rs);

                    foreach (var file in db.FILE_MANAGER.Where(d => d.PARENTID == rs.ID).ToList())
                        db.FILE_MANAGER.Remove(file);

                    if (rs.TYPE == TM.Common.Objects.FileManager.file)
                        TM.IO.FileDirectory.Delete(rs.FULLNAME, false);
                    else
                        TM.IO.FileDirectory.DeleteDirectory(rs.FULLNAME, false);//TM.Common.Directories.Uploads + rs.Subdirectory.Trim('\\') + rs.Name
                }
                db.SaveChanges();
                return Json(new { success = TM.Common.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult BackupDatabase()
        {
            var SqlServer = new TM.Connection.SQLServer();
            try
            {
                var path = TM.Common.Directories.DBBak;
                TM.IO.FileDirectory.CreateDirectory(path, false);
                //TM.IO.FileDirectory.SetAccessRule(path, false);
                var backup = new TM.SQLServer.Backup(path);
                backup.BackingAll(SqlServer.Connection);
                //InsertDirectory(TM.Common.Directories.DBBak + "test\\abc");
                var a = InsertDirectoriesFiles(path);
                return Json(new { success = "Backup Database thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
            finally { SqlServer.Close(); }
        }
        public class objBST : Common.ObjBSTable
        {
            public string maDvi { get; set; }
            public string timeBill { get; set; }
            public int export { get; set; }
            public string startDate { get; set; }
            public string endDate { get; set; }
            public string extension { get; set; }
            public string subDir { get; set; }
        }
    }
}