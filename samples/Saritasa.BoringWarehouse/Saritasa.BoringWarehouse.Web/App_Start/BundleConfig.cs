namespace Saritasa.BoringWarehouse.Web
{
    using System.Web.Optimization;

    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Static/js/jquery.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-tmpl").Include(
                        "~/Static/js/jquery.tmpl.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Static/js/jquery.validate.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/Static/js/jquery.dataTables.js",
                        "~/Static/js/dataTables.bootstrap.js",
                        "~/Static/js/dataTables.buttons.js",
                        "~/Static/js/buttons.bootstrap.js",
                        "~/Static/js/dataTables.select.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/bootbox").Include(
                "~/Static/js/bootbox.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/properties-list-edit").Include(
                "~/Static/js/propertiesListEdit.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Static/js/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Static/js/bootstrap.js",
                      "~/Static/js/respond.js"));

            bundles.Add(new StyleBundle("~/Static/css").Include(
                      "~/Static/css/bootstrap.css",
                      "~/Static/css/dataTables.bootstrap.css",
                      "~/Static/css/buttons.bootstrap.css",
                      "~/Static/css/select.bootstrap.css",
                      "~/Static/css/site.css"
                ));
        }
    }
}
