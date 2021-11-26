using System;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Worldescape
{
    public class PageNumberHelper
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

        public RangeObservableCollection<string> GeneratePageNumbers(
            long totalPageCount,
            int pageIndex,
            RangeObservableCollection<string> _pageNumbers)
        {            
            if (pageIndex.ToString() == _pageNumbers.FirstOrDefault()) // If current page index is equal to the first page of generated page numbers
            {
                return PopulatePageNumbers(totalPageCount, pageIndex, _pageNumbers);
            }
            else if (pageIndex.ToString() == _pageNumbers.LastOrDefault()) // If the current page index is equal to the last page of generated page numbers
            {
                return PopulatePageNumbers(totalPageCount, pageIndex, _pageNumbers);
            }
            else
            {
                return _pageNumbers;
            }
        }

        public RangeObservableCollection<string> PopulatePageNumbers(
            long totalPageCount,
            int pageIndex,
            RangeObservableCollection<string> _pageNumbers)
        {
            _pageNumbers.Clear();

            if (pageIndex - 2 >= 0)
            {
                _pageNumbers.Add((pageIndex - 2).ToString());
            }
            if (pageIndex - 1 >= 0)
            {
                _pageNumbers.Add((pageIndex - 1).ToString());
            }

            _pageNumbers.Add(pageIndex.ToString());

            if (pageIndex + 1 <= totalPageCount)
            {
                _pageNumbers.Add((pageIndex + 1).ToString());
            }
            if (pageIndex + 2 <= totalPageCount)
            {
                _pageNumbers.Add((pageIndex + 2).ToString());
            }

            if (_pageNumbers.Count < 5)
            {
                if (pageIndex + 3 <= totalPageCount)
                {
                    _pageNumbers.Add((pageIndex + 3).ToString());
                }

                if (pageIndex + 4 <= totalPageCount)
                {
                    _pageNumbers.Add((pageIndex + 4).ToString());
                }
            }

            return _pageNumbers;
        }
    }
}
