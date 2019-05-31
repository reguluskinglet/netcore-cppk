using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class TaskPrintItem :BaseEntity
    {
        public int? LabelId { get; set; }

        public virtual Label Label { get; set; }

        public int? TaskPrintId { get; set; }

        public virtual TaskPrint TaskPrint { get; set; }

        /// <summary>
        /// TimeStamp, поле необходимо для отображения очередности печати строк на падачу в принтер
        /// </summary>
        public long TimePrinted { get; set; }

        /// <summary>
        /// Признак того что печать была выполнена
        /// </summary>
        public bool CanPrinted { get; set; }

        public int? UserId { get; set; }

        public virtual User User { get; set; }

        /// <summary>
        /// Дата печати
        /// </summary>
        public DateTime Date => new DateTime(TimePrinted).ToLocalTime();
    }
}
