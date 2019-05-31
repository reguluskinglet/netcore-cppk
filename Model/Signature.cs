using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    /// <summary>
    /// Подпись Акта
    /// </summary>
    public class Signature : BaseEntityRef
    {
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public int InspectionId { get; set; }
        public virtual Inspection Inspection { get; set; }

        /// <summary>
        /// Подпись
        /// </summary>
        public string CaptionImage { get; set; }
    }
}
