using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Worldescape
{
    public class PaginationHelper
    {
        public long GetTotalPageCount(int pageSize, long dataCount)
        {
            var totalPageCount = dataCount < pageSize ? 1 : (long)Math.Ceiling(dataCount / (decimal)pageSize);
            return totalPageCount;
        }

        public int GetNextPageNumber(
            long totalPageCount,
            int pageIndex)
        {
            pageIndex++;

            if (pageIndex > totalPageCount)
            {
                pageIndex = (int)totalPageCount;
            }

            return pageIndex;
        }

        public int GetPreviousPageNumber(
            long totalPageCount,
            int pageIndex)
        {
            pageIndex--;

            if (pageIndex < totalPageCount - 1)
            {
                pageIndex = 0;
            }

            return pageIndex;
        }

        public RangeObservableCollection<PageNumber> GeneratePageNumbers(
            long totalPageCount,
            int pageIndex,
            RangeObservableCollection<PageNumber> _pageNumbers)
        {
            if (pageIndex.ToString() == _pageNumbers.FirstOrDefault().Number) // If current page index is equal to the first page of generated page numbers
            {
                _pageNumbers = PopulatePageNumbers(totalPageCount, pageIndex, _pageNumbers);
            }
            else if (pageIndex.ToString() == _pageNumbers.LastOrDefault().Number) // If the current page index is equal to the last page of generated page numbers
            {
                _pageNumbers = PopulatePageNumbers(totalPageCount, pageIndex, _pageNumbers);
            }

            foreach (PageNumber pageNumbner in _pageNumbers)
            {
                pageNumbner.BorderThickness = new Thickness(0);
            }

            if (_pageNumbers.FirstOrDefault(x => x.Number == pageIndex.ToString()) is PageNumber pageNumber)
            {
                pageNumber.BorderThickness = new Thickness(2);
            }

            return _pageNumbers;
        }

        public RangeObservableCollection<PageNumber> PopulatePageNumbers(
            long totalPageCount,
            int pageIndex,
            RangeObservableCollection<PageNumber> _pageNumbers)
        {
            _pageNumbers.Clear();

            // Add previous two buttons
            if (pageIndex - 2 >= 0)
            {
                _pageNumbers.Add(new PageNumber((pageIndex - 2).ToString()));
            }
            if (pageIndex - 1 >= 0)
            {
                _pageNumbers.Add(new PageNumber((pageIndex - 1).ToString()));
            }

            // Add own button
            _pageNumbers.Add(new PageNumber(pageIndex.ToString()));

            // Add next two buttons
            if (pageIndex + 1 <= totalPageCount)
            {
                _pageNumbers.Add(new PageNumber((pageIndex + 1).ToString()));
            }
            if (pageIndex + 2 <= totalPageCount)
            {
                _pageNumbers.Add(new PageNumber((pageIndex + 2).ToString()));
            }

            // If page numbers are less than 5 then try to add more two next buttons
            if (_pageNumbers.Count < 5)
            {
                if (pageIndex + 3 <= totalPageCount)
                {
                    _pageNumbers.Add(new PageNumber((pageIndex + 3).ToString()));
                }

                if (pageIndex + 4 <= totalPageCount)
                {
                    _pageNumbers.Add(new PageNumber((pageIndex + 4).ToString()));
                }
            }

            return _pageNumbers;
        }
    }
}
