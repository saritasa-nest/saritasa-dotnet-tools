namespace Saritasa.BoringWarehouse.Web.Models
{
    public class jDataTablesSearch
    {
        public bool regex { get; set; }

        public string value { get; set; }
    }

    public class jDataTablesColumn
    {
        public string data { get; set; }

        public string name { get; set; }

        public bool orderable { get; set; }

        public bool searchable { get; set; }

        public jDataTablesSearch search { get; set; }
    }

    public class jDataTablesOrder
    {
        public int column { get; set; }

        public string dir { get; set; }
    }
}