using System.Collections.Generic;
using System.Collections.ObjectModel;
using Worldescape.Shared.Entities;

namespace Worldescape.Shared.Models
{
    public class CategorizedConstructsHost : CoreBase
    {
        public List<ConstructAsset> ChildrenSource { get; set; } = new List<ConstructAsset>();

        public ObservableCollection<ConstructAsset> ChildrenFiltered { get; set; } = new ObservableCollection<ConstructAsset>();

        public bool IsSelected { get; set; }
    }
}
