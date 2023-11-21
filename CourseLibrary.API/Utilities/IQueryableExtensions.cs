using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace CourseLibrary.API.Utilities
{
    public static class IQueryableExtensions
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int pageSize, int pageNumber)
        {
            var count = await query.CountAsync();
            var items = await query
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToArrayAsync();
            return new PagedList<T>(items, count, pageSize, pageNumber);
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (mappingDictionary is null)
            {
                throw new ArgumentNullException(nameof(mappingDictionary));
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            var orderByString = string.Empty;

            // the orderBy string is separated by ",", so we split it
            var orderByAfterSplit = orderBy.Split(',');

            // apply each orderby clause
            foreach (var orderbyClause in orderByAfterSplit)
            {
                // trim the orderbyClause as it might contain leading
                // or trailing spaces. We can't trim the var in foreach, so we use another var
                var trimmedOrderbyClause = orderbyClause.Trim();

                // if the sort option ends with " desc", we order descending, otherwise ascending
                var orderDescending = trimmedOrderbyClause.EndsWith(" desc");

                // remove " asc" or " desc" from the orderbyClause so we get the property name to look for in the mapping dictionary
                var indexOfFirstSpace = trimmedOrderbyClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedOrderbyClause : trimmedOrderbyClause.Remove(indexOfFirstSpace);

                // find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing.");
                }

                // get the PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertyName];
                if (propertyMappingValue is null)
                {
                    throw new ArgumentNullException(nameof(propertyMappingValue));
                }

                // revert sort order if necessary
                if (propertyMappingValue.Revert)
                {
                    orderDescending = !orderDescending;
                }

                // Run through the property names
                foreach (var destinationProperty in propertyMappingValue.DestinationProperties)
                {
                    orderByString = orderByString + (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ") + destinationProperty + (orderDescending ? " descending" : " ascending");
                }
            }

            return source.OrderBy(orderByString);
        }
    }
}
