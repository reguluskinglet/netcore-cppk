using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    /// <summary>
    /// Шаблон меток, нужен для принтера
    /// </summary>
    public  class TemplateLabel : BaseEntity
    {
        public string Name { get; set; }

        public string Template { get; set; }
    }
}
