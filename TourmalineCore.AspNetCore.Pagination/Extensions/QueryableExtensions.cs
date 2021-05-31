using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TourmalineCore.AspNetCore.Pagination.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Bypasses a number of elements and then returns a specified number of contiguous elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static IQueryable<T> Page<T>(this IQueryable<T> queryable, int page, int pageSize)
        {
            if (pageSize == -1)
            {
                return queryable;
            }

            return queryable
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>
        /// Filters query based on a condition of whether a specified string occurs within value of a specified property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static IQueryable<T> Contains<T>(
            this IQueryable<T> queryable,
            Expression<Func<T, string>> propertyExpression, string term)
        {
            var normalizedTerm = term.NormalizeString();

            return string.IsNullOrWhiteSpace(normalizedTerm)
                ? queryable
                : queryable.Where(CreateContainsPredicateExpression(propertyExpression, term));
        }

        private static Expression<Func<T, bool>> CreateContainsPredicateExpression<T>(Expression<Func<T, string>> propertyExpression, string term)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertyExpression.Body).Member;
            var parameterExpression = Expression.Parameter(typeof(T), "e");
            var accessor = Expression.Property(parameterExpression, propertyInfo);
            var termString = Expression.Constant(term.ToLowerInvariant(), typeof(string));

            var toLowerInfo = typeof(string).GetMethod("ToLower", new Type[0]);
            var toLower = Expression.Call(accessor, toLowerInfo);

            var containsInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var expression = Expression.Call(toLower, containsInfo, termString);

            return Expression.Lambda<Func<T, bool>>(expression, parameterExpression);
        }

        /// <summary>
        /// Filters query based on a condition of whether a specified string equal to a value of property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IQueryable<T> EqualCaseInsensitive<T>(
            this IQueryable<T> queryable,
            Expression<Func<T, string>> propertyExpression, string value)
        {
            var normalizedTerm = value.NormalizeString();

            return string.IsNullOrWhiteSpace(normalizedTerm)
                ? Enumerable.Empty<T>().AsQueryable()
                : queryable.Where(CreateEqualCaseInsensitivePredicateExpression(propertyExpression, value));
        }

        private static Expression<Func<T, bool>> CreateEqualCaseInsensitivePredicateExpression<T>(Expression<Func<T, string>> propertyExpression, string value)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertyExpression.Body).Member;
            var parameterExpression = Expression.Parameter(typeof(T), "e");
            var accessor = Expression.Property(parameterExpression, propertyInfo);
            var valueString = Expression.Constant(value.ToLowerInvariant(), typeof(string));

            var toLowerInfo = typeof(string).GetMethod("ToLower", new Type[0]);
            var toLower = Expression.Call(accessor, toLowerInfo);

            var equalsInfo = typeof(string).GetMethod("Equals", new[] { typeof(string) });
            var expression = Expression.Call(toLower, equalsInfo, valueString);

            return Expression.Lambda<Func<T, bool>>(expression, parameterExpression);
        }

        /// <summary>
        /// Sorts query in a specified order according to a property name
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="propertyName"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(
            this IQueryable<TEntity> queryable,
            string propertyName,
            ListSortDirection sortDirection)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return queryable.OrderBy(x => 0);
            }

            if (queryable.IsOrdered() == false)
            {
                return sortDirection == ListSortDirection.Ascending
                    ? queryable.ApplyOrderingCall("OrderBy", propertyName)
                    : queryable.ApplyOrderingCall("OrderByDescending", propertyName);
            }

            var orderedQueryable = queryable as IOrderedQueryable<TEntity>;

            return sortDirection == ListSortDirection.Ascending
                ? orderedQueryable.ApplyOrderingCall("ThenBy", propertyName)
                : orderedQueryable.ApplyOrderingCall("ThenByDescending", propertyName);
        }

        private static bool IsOrdered<T>(this IQueryable<T> queryable)
        {
            return queryable.Expression.Type == typeof(IOrderedQueryable<T>);
        }

        private static IOrderedQueryable<TEntity> ApplyOrderingCall<TEntity>(
            this IQueryable<TEntity> queryable,
            string orderingMethodName, string propertyName)
        {
            var entityType = typeof(TEntity);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperty(propertyName);
            var arg = Expression.Parameter(entityType, "x");
            var property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, arg);

            //Get methods like System.Linq.Queryable.OrderBy().
            var orderingMethod = typeof(Queryable)
                .GetMethods()
                .Where(m => m.Name == orderingMethodName && m.IsGenericMethodDefinition)
                .Single(m =>
                        m.GetParameters().Length == 2
                    ); //ToDo Put more restriction here to ensure selecting the right overload. Get overload that has 2 parameters

            //The linq's  methods like OrderBy<TSource, TKey> has two generic types, which provided here
            var genericMethod = orderingMethod.MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*Call e.g. query.OrderBy(selector), with query and selector: x=> x.PropName
            Note that we pass the selector as Expression to the method and we don't compile it.
            By doing so EF can extract "order by" columns and generate SQL for it.*/
            return (IOrderedQueryable<TEntity>)genericMethod.Invoke(genericMethod, new object[] { queryable, selector });
        }
    }
}