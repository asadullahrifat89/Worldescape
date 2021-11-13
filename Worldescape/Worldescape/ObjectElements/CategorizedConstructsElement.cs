using System.Collections.Generic;
using System.Collections.ObjectModel;
using Worldescape.Shared.Entities;
using Worldescape.Shared.Models;

namespace Worldescape.ObjectElements
{
    public class CategorizedConstructsElement : CoreBase
    {
        public List<ConstructAsset> ChildrenSource { get; set; } = new List<ConstructAsset>();

        public ObservableCollection<ConstructAsset> ChildrenFiltered { get; set; } = new ObservableCollection<ConstructAsset>();

        public bool IsSelected { get; set; }
    }
}
