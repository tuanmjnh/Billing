using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Common
{
    public class Directories
    {
        public const string Uploads = "Uploads\\";
        public const string data = Uploads + "Data\\";
        public const string HDData = data + "HDData\\";
        public const string HDDataSource = data + "HDDataSource\\";
        public const string HDDataSourceGeneral = HDDataSource + "General\\";
        public const string ThanhToanTruoc = HDDataSource + "ThanhToanTruoc\\";
        public const string HDDataSourceMega = HDDataSource + "Mega\\";
        public const string HDDataSourceFiber = HDDataSource + "Fiber\\";
        public const string HDDataSourceFiberGD = HDDataSource + "FiberGD\\";
        public const string HDDataSourceMyTV = HDDataSource + "MyTV\\";
        public const string images = Uploads + "Images\\";
        public const string imagesProduct = images + "Product\\";
        public const string imagesCustomer = images + "Customer\\";
        public const string document = data + "Document\\";
        public const string orther = Uploads + "Orther\\";
        public const string ccbs = data + "ccbs\\";
        public const string DBBak = Uploads + "DBBak\\";
        public const string CA = document + "CA\\";
        public const string IVAN = document + "IVAN\\";
        public const string KTR = document + "KTR\\";
        public const string TraSauReport = data + "Report\\";
        public const string Hopdong = Uploads + "Hopdong\\";
    }
    public class FileManager
    {
        public const string directory = "Directory";
        public const string file = "File";
    }
    public class Objects
    {
        public enum TYPE_HD
        {
            HD_NET = 1,
            HD_MYTV = 2,
            NET = 3,
            MYTV = 4,
            HD_CD = 5,
            HD_DD = 6,
            CD = 7,
            DD = 8,
            HD_MERGIN = 9,
            DB_THANHTOAN_BKN = 10
        }
        public enum TYPE_DISCOUNT
        {
            PERCENT = 1,
            MONEY = 2,
            FIX = 3,
            FIX_IN = 4,
            MYTVGD = 5
        }
        public enum BD_NGAY_TB
        {
            _binh_thuong = 1,
            _khong_tien = 2,
            _su_dung = 3,
            _ket_thuc = 4,
            _khoa = 5,
            _mo = 6,
            _su_dung_ket_thuc = 7,
            _su_dung_khoa = 8,
            _mo_ket_thuc = 9,
            _khoa_mo = 10,
            _mo_khoa = 11,
        }
        public enum ResultCode
        {
            _length = 0,
            _success = 1,
            _error = 2,
            _extension = 3
        }
        public enum TABLE_TYPE
        {
            EXPORT_CUSTOM = 0,
            SQLSERVER = 1,
            DBF = 2,
        }
        public enum SETTINGS_APP_KEY
        {
            MAIN = 0,
        }
        public enum SETTINGS_SUB_KEY
        {
            UPDATE_PRICE_INTERNET = 0,
        }
        public enum GROUP_ITEM
        {
            REPORT_REVENUE = 0
        }
        public enum POSITION
        {
            TOP = 0,
            RIGHT = 1,
            BOTTOM = 2,
            LEFT = 3,
            CENTER = 4
        }
        public enum EXPORT_CUSTOM
        {
            DEFAULT = 1,
            MODULES = 2,
        }
        public static Dictionary<int, string> stk_ma_dvi = new Dictionary<int, string>() {
            {1,"8600 201 004862 t¹i Ng©n hµng N«ng nghiÖp & PTNT tØnh B¾c K¹n" },
            {2,"8605 201 001805 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Ba BÓ, tØnh B¾c K¹n" },
            {3,"8607 201 001374 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn B¹ch Th«ng, tØnh B¾c K¹n" },
            {4,"8601 201 001712 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Chî §ån, tØnh B¾c K¹n" },
            {5,"8606 201 001366 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Chî Míi, tØnh B¾c K¹n" },
            {6,"8603 201 001541 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Na R×, tØnh B¾c K¹n" },
            {7,"8604 201 001429 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Ng©n S¬n, tØnh B¾c K¹n" },
            {8,"8602 201 001375 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn P¸c NÆm, tØnh B¾c K¹n" }
        };
    }
    public class HDDT
    {
        public enum QRCodeType
        {
            Portal = 1,
            HDDT = 2,
            HDGiay = 3,
            AnPhamThuCuoc = 4, //(thông báo cước, biên lai, phiếu thu
            AnPhamQLTT = 5 //bản kê thu cước
        }
        public enum ProvinceCode
        {
            HNI = 1,
            BCN = 2,
        }
        public const string DetailCoDinh = "CUOC MANG CO DINH";
        public const string DetailDiDong = "CUOC MANG DI DONG";
    }
    public class DefaultObj
    {
        public DateTime datetime { get; set; }
        public int month_time { get; set; }
        public int year_time { get; set; }
        public int day_in_month { get; set; }
        public int data_id { get; set; }
        public int data_type { get; set; }
        public string block_time { get; set; }
        public string month_year_time { get; set; }
        public string year_month_time { get; set; }
        public string month_before { get; set; }
        public string time { get; set; }
        public bool ckhMerginMonth { get; set; }
        public bool ckhZipFile { get; set; }
        public bool ckhTCVN3 { get; set; }
        public string file { get; set; }
        public string data_value { get; set; }
        public string DataSource { get; set; }
    }
    public class RemoveMainObj
    {
        public string[] listCol { get; set; }
        public string[] conditionKey { get; set; }
        public string ExtraValue { get; set; }
        public string PrimeryKey { get; set; }
        public int TYPE_BILL { get; set; }
        public string TIME_BILL { get; set; }
        public bool IsExtraValue { get; set; }
    }
    public class RemoveTableObj : RemoveMainObj
    {
        public string table { get; set; }
    }
    public class RemoveListObj : RemoveMainObj
    {
        public List<Models.HD_MERGIN> table { get; set; }
    }
    public class ObjBSTable
    {
        public string sort { get; set; }
        public string order { get; set; }
        public string search { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public int flag { get; set; }
    }
}