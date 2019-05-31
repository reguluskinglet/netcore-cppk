using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Terminal
{
    public class SignatureTerminal : BaseEntityTerminal
    {
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public Guid InspectionId { get; set; }
        public virtual InspectionTerminal Inspection { get; set; }

        /// <summary>
        /// Подпись  в форме base64
        /// </summary>
        public string CaptionImage { get; set; }
    }
}
