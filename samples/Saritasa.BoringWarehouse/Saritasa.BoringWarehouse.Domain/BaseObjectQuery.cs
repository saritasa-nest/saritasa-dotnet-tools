namespace Saritasa.BoringWarehouse.Domain
{
    using Candy;

    public abstract class BaseObjectQuery
    {
        private SortOrder sortOrder;
        private string sortOrderName;

        public int Limit { get; set; } = 100;

        public int Offset { get; set; } = 0;

        public string SearchPattern { get; set; }

        public string OrderColumn { get; set; }

        public string SortOrderName
        {
            get { return sortOrderName; }
            set
            {
                sortOrderName = value;
                sortOrder = ParseSortOrder(value);
            }
        }

        public SortOrder SortOrder
        {
            get { return sortOrder; }
            set
            {
                sortOrder = value;
                SortOrderName = value.ToString();
            }
        }

        public static SortOrder ParseSortOrder(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return SortOrder.Asc;
            }
            return value.ToLower().StartsWith("asc") ? SortOrder.Asc : SortOrder.Desc;
        }
    }
}
