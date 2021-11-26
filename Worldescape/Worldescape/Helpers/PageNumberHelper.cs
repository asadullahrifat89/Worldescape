using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Worldescape
{
    public class PageNumberHelper
    {
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
            // Ig total page count is greater than 5 only then make repopulation other wise no need
            if (totalPageCount > 5)
            {
                if (pageIndex.ToString() == _pageNumbers.FirstOrDefault()) // If current page index is equal to the first page of generated page numbers
                {
                    _pageNumbers.Clear();
                    return PopulatePageNumbers(totalPageCount, pageIndex, _pageNumbers);
                }
                else if (pageIndex.ToString() == _pageNumbers.LastOrDefault()) // If the current page index is equal to the last page of generated page numbers
                {
                    _pageNumbers.Clear();
                    return PopulatePageNumbers(totalPageCount, pageIndex, _pageNumbers);
                }
                else
                {
                    return _pageNumbers;
                }
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
            for (int i = pageIndex; i < totalPageCount; i++)
            {
                _pageNumbers.Add(i.ToString());

                if (i >= 5) // Generate upto 5 pages
                {
                    break;
                }
            }

            return _pageNumbers;
        }
    }
}
