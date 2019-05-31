using System;

namespace RzdMonitors.Util
{
    public class TimedTaskDto
    {
        public string Name { get; set; }

        public DateTime Start { get; set; }

        public DateTime? End { get; set; }

        public string BgColor { get; set; } //фон

        public string FgColor { get; set; } //текст

        public string BorderColor { get; set; } //цвет бордера, null - нет бордера
    }
}
