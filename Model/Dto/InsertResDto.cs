using System;

namespace Rzdppk.Model.Dto
{
    public class InsertResDto
    {
        public bool IsSuccess { get; set; } = false;

        public bool AlreadyExist { get; set; } = false;

        public string Error { get; set; }
    }
}
