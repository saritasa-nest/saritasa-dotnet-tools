namespace Saritasa.BoringWarehouse.Web.Models
{
    using System.Collections;

    public class DataTablesSearchResultViewModel
    {
        public int draw { get; set; }

        public long recordsTotal { get; set; }

        public long recordsFiltered { get; set; }

        public IEnumerable data { get; set; }

        public string error { get; set; }
    }
}