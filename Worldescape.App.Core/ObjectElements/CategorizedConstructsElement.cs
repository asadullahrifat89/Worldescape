using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Worldescape.App.Core.ObjectElements
{
    public class CategorizedConstructsElement : CoreBase
    {
        public List<Construct> ChildrenSource { get; set; } = new List<Construct>();

        //public RangeObservableCollection<Construct> ChildrenFiltered { get; set; } = new RangeObservableCollection<Construct>();

        public bool IsSelected { get; set; }
    }
}
