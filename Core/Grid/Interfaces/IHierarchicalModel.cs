using System.Collections.Generic;

namespace Core.Grid
{
    public interface IHierarchicalModel<TModel>
    {
        int Id { get; set; }

        int? ParentId { get; set; }

        int Level { get; set; }

        IList<TModel> Child { get; set; }

        bool Disabled { get; set; }
    }
}