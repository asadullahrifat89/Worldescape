using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Worldescape
{
    public class PageNumberHelper
    {
        public RangeObservableCollection<string> GeneratePageNumbers(long _totalPageCount, int _pageIndex, RangeObservableCollection<string> _pageNumbers)
        {
            // Ig total page count is greater than 5 only then make repopulation other wise no need
            if (_totalPageCount > 5)
            {
                if (_pageIndex.ToString() == _pageNumbers.FirstOrDefault()) // If current page index is equal to the first page of generated page numbers
                {
                    return PopulatePageNumbers(_totalPageCount, _pageIndex, _pageNumbers);
                }
                else if (_pageIndex.ToString() == _pageNumbers.LastOrDefault()) // If the current page index is equal to the last page of generated page numbers
                {
                    return PopulatePageNumbers(_totalPageCount, _pageIndex, _pageNumbers);
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

        public RangeObservableCollection<string> PopulatePageNumbers(long _totalPageCount, int _pageIndex, RangeObservableCollection<string> _pageNumbers)
        {
            for (int i = _pageIndex; i < _totalPageCount; i++)
            {
                _pageNumbers.Add(i.ToString());

                if (i >= 5) // Generate upto 5 pages
                {
                    break;
                }
            }

            return _pageNumbers;

            //PagesHolder.ItemsSource = _pageNumbers;
        }
    }
}
