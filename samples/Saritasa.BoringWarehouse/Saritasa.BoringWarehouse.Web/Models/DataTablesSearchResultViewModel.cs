using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Saritasa.BoringWarehouse.Web.Models
{
    public class DataTablesSearchResultViewModel
    {
        public int draw { get; set; }

        public long recordsTotal { get; set; }

        public long recordsFiltered { get; set; }

        public IEnumerable data { get; set; }

        public string error { get; set; }
    }
}