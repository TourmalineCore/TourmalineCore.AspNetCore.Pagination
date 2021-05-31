namespace TourmalineCore.AspNetCore.Pagination.Models
{
    public class ColumnFilter
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public struct RangeFilterValues
    {
        public int From { get; set; }

        public int To { get; set; }
    }
}