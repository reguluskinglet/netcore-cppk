using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Rzdppk.Model.Base
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
