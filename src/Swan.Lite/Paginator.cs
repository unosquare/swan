using System;

namespace Swan
{
    /// <summary>
    /// A utility class to compute paging or batching offsets.
    /// </summary>
    public class Paginator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Paginator" /> class.
        /// </summary>
        /// <param name="totalCount">The total count of items to page over.</param>
        /// <param name="pageSize">The desired size of individual pages.</param>
        public Paginator(int totalCount, int pageSize)
        {
            TotalCount = totalCount;
            PageSize = pageSize;
            PageCount = ComputePageCount();
        }

        /// <summary>
        /// Gets the desired number of items per page.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Gets the total number of items to page over.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Gets the computed number of pages.
        /// </summary>
        public int PageCount { get; }

        /// <summary>
        /// Gets the start item index of the given page.
        /// </summary>
        /// <param name="pageIndex">Zero-based index of the page.</param>
        /// <returns>The start item index.</returns>
        public int GetFirstItemIndex(int pageIndex)
        {
            pageIndex = FixPageIndex(pageIndex);
            return pageIndex * PageSize;
        }

        /// <summary>
        /// Gets the end item index of the given page.
        /// </summary>
        /// <param name="pageIndex">Zero-based index of the page.</param>
        /// <returns>The end item index.</returns>
        public int GetLastItemIndex(int pageIndex)
        {
            var startIndex = GetFirstItemIndex(pageIndex);
            return Math.Min(startIndex + PageSize - 1, TotalCount - 1);
        }

        /// <summary>
        /// Gets the item count of the given page index.
        /// </summary>
        /// <param name="pageIndex">Zero-based index of the page.</param>
        /// <returns>The number of items that the page contains.</returns>
        public int GetItemCount(int pageIndex)
        {
            pageIndex = FixPageIndex(pageIndex);
            return (pageIndex >= PageCount - 1)
                ? GetLastItemIndex(pageIndex) - GetFirstItemIndex(pageIndex) + 1
                : PageSize;
        }

        /// <summary>
        /// Fixes the index of the page by applying bound logic.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns>A limit-bound index.</returns>
        private int FixPageIndex(int pageIndex)
        {
            if (pageIndex < 0) return 0;

            return pageIndex >= PageCount ? PageCount - 1 : pageIndex;
        }

        /// <summary>
        /// Computes the number of pages for the paginator.
        /// </summary>
        /// <returns>The page count.</returns>
        private int ComputePageCount()
        {
            // include this if when you always want at least 1 page 
            if (TotalCount == 0)
                return 0;

            return TotalCount % PageSize != 0
                ? (TotalCount / PageSize) + 1
                : TotalCount / PageSize;
        }
    }
}
