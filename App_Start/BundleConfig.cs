using System.Web;
using System.Web.Optimization;

namespace Billing
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/bootstrap-datetimepicker.min.css",
                      "~/Content/font-awesome.min.css"
                      ));

            //Extras js
            bundles.Add(new ScriptBundle("~/bundles/extrajs").Include(
                      "~/Scripts/jquery.validate.min.js",
                      "~/Scripts/jquery.unobtrusive-ajax.min.js",
                      "~/Scripts/jquery.validate.unobtrusive.min.js",
                      "~/Scripts/validator.unobtrusive.parseDynamicContent.js",
                      "~/Scripts/moment-with-locales.min.js",
                      "~/Scripts/moment.min.js",
                      "~/Scripts/bootstrap-datetimepicker.min.js",
                      "~/Scripts/bootstrap-confirmation.min.js",
                      "~/Scripts/tinymce/tinymce.min.js",
                      "~/Scripts/TMUpdateValue.js",
                      "~/Scripts/TMConfirm.js",
                      "~/Scripts/TMLibrary.js",
                      "~/Scripts/TMlanguage.js",
                      "~/Scripts/TMReadImage.js",
                      "~/Scripts/TMForm.js",
                      "~/Scripts/TMTag.js",
                      "~/Scripts/site.js"
                      ));
        }
    }
}
