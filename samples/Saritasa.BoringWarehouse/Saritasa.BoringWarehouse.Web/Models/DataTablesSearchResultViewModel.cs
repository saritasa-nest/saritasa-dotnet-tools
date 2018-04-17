using System.Collections;

namespace Saritasa.BoringWarehouse.Web.Models
{
#pragma warning disable SA1300 // Element must begin with upper-case letter
    public class DataTablesSearchResultViewModel
    {
        public int draw { get; set; }

        public long recordsTotal { get; set; }

        public long recordsFiltered { get; set; }

        public IEnumerable data { get; set; }

        public string error { get; set; }
    }
#pragma warning restore SA1300 // Element must begin with upper-case letter
}
