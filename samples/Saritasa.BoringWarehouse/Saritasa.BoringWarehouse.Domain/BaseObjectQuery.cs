using System.ComponentModel;

namespace Saritasa.BoringWarehouse.Domain
{
    /// <summary>
    /// Base query object to be used for sorting and filtering.
    /// </summary>
    public abstract class BaseObjectQuery
    {
        private ListSortDirection sortOrder;
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

        public ListSortDirection SortOrder
        {
            get => sortOrder;

            set
            {
                sortOrder = value;
                SortOrderName = value.ToString();
            }
        }

        public static ListSortDirection ParseSortOrder(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return ListSortDirection.Ascending;
            }
            return value.ToLower().StartsWith("asc") ? ListSortDirection.Ascending : ListSortDirection.Descending;
        }
    }
}
