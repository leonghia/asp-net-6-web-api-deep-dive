using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.API.Utilities
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(T[] items, int count, int pageSize, int pageNumber)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling((double)count / pageSize);
            AddRange(items);
        }

        public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> query, int pageSize, int pageNumber)
        {
            var count = await query.CountAsync<T>();
            var items = await query
                .Skip<T>(pageSize * (pageNumber - 1))
                .Take<T>(pageSize)
                .ToArrayAsync<T>();
            return new PagedList<T>(items, count, pageSize, pageNumber);
        }
    }
}
