using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TourmalineCore.AspNetCore.Pagination.Extensions;
using TourmalineCore.AspNetCore.Pagination.Models;

namespace TourmalineCore.AspNetCore.Pagination
{
    public class InMemoryPageQueryBase<TSource>
        where TSource : class
    {
        protected PaginationResult<TSource> GetPageByPaginationParams(List<TSource> list, PaginationParams paginationParams)
        {
            var filteredList = list;

            foreach (var filter in paginationParams.Filters)
            {
                filteredList = DoFiltration(filteredList, filter);
            }

            var orderedPage = DoOrdering(
                        filteredList,
                        paginationParams.OrderBy,
                        paginationParams.OrderingDirection
                    )
                .AsQueryable()
                .Page(
                        paginationParams.Page,
                        paginationParams.PageSize
                    )
                .ToList();

            return new PaginationResult<TSource>
            {
                Draw = paginationParams.Draw,
                List = orderedPage,
                TotalCount = filteredList.Count,
            };
        }

        protected virtual List<TSource> DoFiltration(List<TSource> list, ColumnFilter filter)
        {
            return list;
        }

        protected virtual List<TSource> DoOrdering(List<TSource> list, string orderBy, ListSortDirection sortDirection)
        {
            return list;
        }
    }
}