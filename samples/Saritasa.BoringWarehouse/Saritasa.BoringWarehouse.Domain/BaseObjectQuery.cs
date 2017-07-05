using Saritasa.Tools.Common.Utils;

namespace Saritasa.BoringWarehouse.Domain
{
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
            get => sortOrderName;

            set
            {
                sortOrderName = value;
                sortOrder = ParseSortOrder(value);
            }
        }

        public SortOrder SortOrder
        {
            get => sortOrder;

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
                return SortOrder.Ascending;
            }
            return value.ToLower().StartsWith("asc") ? SortOrder.Ascending : SortOrder.Descending;
        }
    }
}
