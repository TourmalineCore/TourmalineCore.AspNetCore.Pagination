using System.Linq;
using TourmalineCore.AspNetCore.Pagination.Models;

namespace TourmalineCore.AspNetCore.Pagination.Extensions
{
    public static class ColumnFilterExtensions
    {
        public static RangeFilterValues ParseRangeValues(this ColumnFilter filter)
        {
            var values = filter.Value
                .Split(';')
                .Select(x => x.ParseNullableInt())
                .ToArray();

            return new RangeFilterValues
            {
                From = values[0] ?? int.MinValue,
                To = values[1] ?? int.MaxValue,
            };
        }
    }
}