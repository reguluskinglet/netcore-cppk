using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class TaskPrint : BaseEntity
    {
        /// <summary>
        /// Номер задания на печать, должен идти попорядку
        /// </summary>
        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Пользователь создавший задание на печать
        /// </summary>
        public int? UserId { get; set; }

        public virtual User User { get; set; }

        public LabelType LabelType { get; set; }

        public int? TemplateLabelId { get; set; }
        public virtual TemplateLabel TemplateLabel { get; set; }

    }
}
