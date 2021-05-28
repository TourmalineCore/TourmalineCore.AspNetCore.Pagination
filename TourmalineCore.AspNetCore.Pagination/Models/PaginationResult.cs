using System.Collections.Generic;

namespace TourmalineCore.AspNetCore.Pagination.Models
{
    public class PaginationResult<T>
        where T : class
    {
        public int Draw { get; set; }

        public List<T> List { get; set; }

        public int TotalCount { get; set; }
    }
}