using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.TV
{
    public class TVPanel : BaseEntity
    {
        public int TVBoxId { get; set; }
        public virtual TVBox TVBox { get; set; }
        public int Number { get; set; }
        public ScreenType ScreenType { get; set; }
    }
}