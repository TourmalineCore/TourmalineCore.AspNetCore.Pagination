using System.Collections.Generic;
using System.ComponentModel;

namespace TourmalineCore.AspNetCore.Pagination.Models
{
    public class PaginationParams
    {
        public int Draw { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string OrderBy { get; set; }

        public ListSortDirection OrderingDirection { get; set; }

        public List<ColumnFilter> Filters { get; set; }
    }
}