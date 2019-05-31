using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class TechPass : BaseEntity
    {
        [Required]
        public string Number { get; set; }

        [Required]
        public string PlaceDrawUp { get; set; }

        public DateTime DateDrawUp { get; set; }

        public int TrainId { get; set; }

        public virtual Train Train { get; set; }
    }
}
