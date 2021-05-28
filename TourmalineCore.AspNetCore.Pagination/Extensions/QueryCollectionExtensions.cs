using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Primitives;
using TourmalineCore.AspNetCore.Pagination.Models;

namespace TourmalineCore.AspNetCore.Pagination.Extensions
{
    public static class QueryCollectionExtensions
    {
        private static readonly string[] Separator = { "," };

        /// <summary>
        /// Returns a PaginationParams object based on this sequence of Key Value Pairs. Expected keys: draw, page, pageSize, orderBy, filteredByColumns, filteredByValues
        /// </summary>
        /// <param name="collectionEnumerable"></param>
        /// <returns></returns>
        public static PaginationParams GetPaginationParams(this IEnumerable<KeyValuePair<string, StringValues>> collectionEnumerable)
        {
            var collection = collectionEnumerable.ToList();

            var filterColumns = collection
                .GetQueryParamValueAsString("filteredByColumns")
                .Split(Separator, StringSplitOptions.RemoveEmptyEntries);

            var filterValues = collection
                .GetQueryParamValueAsString("filteredByValues")
                .Split(Separator, StringSplitOptions.RemoveEmptyEntries);

            var paginationParams = new PaginationParams
            {
                Draw = collection.GetQueryParamValueAsInt("draw", 1),
                Page = collection.GetQueryParamValueAsInt("page", 1),
                PageSize = collection.GetQueryParamValueAsInt("pageSize", 10),
                OrderBy = collection.GetQueryParamValueAsString("orderBy").FirstCharToUpper(),
                OrderingDirection = collection.GetOrderingDirection("orderingDirection"),
                Filters = filterColumns
                    .Select(
                            (column, i) =>
                                new ColumnFilter
                                {
                                    Name = column.FirstCharToUpper(),
                                    Value = filterValues[i].Trim().ToLowerInvariant(),
                                }
                        )
                    .ToList(),
            };

            return paginationParams;
        }

        private static string GetQueryParamValueAsString(
            this List<KeyValuePair<string, StringValues>> collection,
            string paramName)
        {
            foreach (var keyValuePair in collection)
            {
                if (keyValuePair.Key.Equals(paramName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return keyValuePair.Value.ToString();
                }
            }

            return string.Empty;
        }

        private static int GetQueryParamValueAsInt(
            this List<KeyValuePair<string, StringValues>> collection,
            string paramName, int defaultValue)
        {
            return int.TryParse(collection.GetQueryParamValueAsString(paramName), out var value)
                ? value
                : defaultValue;
        }

        private static ListSortDirection GetOrderingDirection(
            this List<KeyValuePair<string, StringValues>> collection,
            string paramName)
        {
            var orderingDirectionAsStr = collection.GetQueryParamValueAsString(paramName);

            return orderingDirectionAsStr == "desc"
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
    }
}