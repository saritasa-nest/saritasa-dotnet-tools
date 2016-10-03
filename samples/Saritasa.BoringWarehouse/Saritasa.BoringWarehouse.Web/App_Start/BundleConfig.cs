using System.Web;
using System.Web.Optimization;

namespace Saritasa.BoringWarehouse.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/bower_components/jquery/dist/jquery.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-tmpl").Include(
                        "~/bower_components/jquery-tmpl/jquery.tmpl.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/bower_components/jquery-validation/dist/jquery.validate.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/bower_components/datatables/media/js/jquery.dataTables.js",
                        "~/bower_components/datatables/media/js/dataTables.bootstrap.js",
                        "~/bower_components/datatables-buttons/js/dataTables.buttons.js",
                        "~/bower_components/datatables-buttons/js/buttons.bootstrap.js",
                        "~/bower_components/datatables-select/js/dataTables.select.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/bootbox").Include(
                "~/bower_components/bootbox.js/bootbox.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/properties-list-edit").Include(
                "~/Scripts/propertiesListEdit.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/bower_components/bootstrap/dist/js/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                    "~/bower_components/bootstrap/dist/css/bootstrap.css",
                    "~/bower_components/datatables/media/css/dataTables.bootstrap.css",
                    "~/bower_components/datatables-buttons/css/buttons.bootstrap.css",
                    "~/bower_components/datatables-select/css/select.bootstrap.css",
                    "~/Content/site.css"
            ));
        }
    }
}
