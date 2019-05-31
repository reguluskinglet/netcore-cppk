using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Grid
{
    public class GridColumnInfo
    {
        public GridColumnInfo()
        {
        }

        public GridColumnInfo(string systemName, string displayName, int? width)
        {
            DisplayName = displayName;
            Name = systemName;
            Width = width;
        }

        /// <summary>
        /// Отоброжаемое поле
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// системное имя
        /// </summary>
        public string Name { get; set; }

        public int? Width { get; set; }
    }
}
