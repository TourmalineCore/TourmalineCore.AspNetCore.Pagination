using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TourmalineCore.AspNetCore.Pagination.Extensions;
using TourmalineCore.AspNetCore.Pagination.Models;
using Z.EntityFramework.Plus;

namespace TourmalineCore.AspNetCore.Pagination
{
    public abstract class PageQueryBase<TEntity, TDto>
        where TEntity : class
        where TDto : class
    {
        /// <summary>
        /// Uses DoIncludes, DoFiltration, DoOrdering and Map methods of this class to return a PaginationResult object based on a specified pagination params
        /// </summary>
        /// <param name="initialQueryable"></param>
        /// <param name="paginationParams"></param>
        /// <returns></returns>
        protected async Task<PaginationResult<TDto>> GetPageByPaginationParamsAsync(IQueryable<TEntity> initialQueryable, PaginationParams paginationParams)
        {
            var queryable = DoIncludes(initialQueryable);

            var filteredQueryable = queryable;

            foreach (var filter in paginationParams.Filters)
            {
                filteredQueryable = DoFiltration(filteredQueryable, filter);
            }

            var filteredTotalCountQuery = filteredQueryable
                .DeferredCount()
                .FutureValue();

            var orderedEntitiesPageQuery = DoOrdering(
                        filteredQueryable,
                        paginationParams.OrderBy,
                        paginationParams.OrderingDirection
                    )
                .Page(
                        paginationParams.Page,
                        paginationParams.PageSize
                    )
                .Future();

            var entitiesPage = await orderedEntitiesPageQuery
                .ToListAsync()
                .ConfigureAwait(false);

            return new PaginationResult<TDto>
            {
                Draw = paginationParams.Draw,
                List = await Map(entitiesPage).ConfigureAwait(false),
                TotalCount = filteredTotalCountQuery.Value,
            };
        }

        /// <summary>
        /// Specifies related entities to include in the query results
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> DoIncludes(IQueryable<TEntity> queryable)
        {
            return queryable;
        }

        /// <summary>
        /// Filters a query based on a specified ColumnFilter
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> DoFiltration(IQueryable<TEntity> queryable, ColumnFilter filter)
        {
            return queryable;
        }

        /// <summary>
        /// Sorts a query in a specified order according to a property name
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="orderBy"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        protected virtual IOrderedQueryable<TEntity> DoOrdering(
            IQueryable<TEntity> queryable,
            string orderBy,
            ListSortDirection sortDirection)
        {
            return (IOrderedQueryable<TEntity>)queryable;
        }

        /// <summary>
        /// Maps a List of source entities to ones that will be transferred to a PaginationResult
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected abstract Task<List<TDto>> Map(List<TEntity> entities);
    }
}