using System;

namespace WpfMultiScreens.Util
{
    public static class ExceptionExtensions
    {
        public static string GetAllInnerExceptionMessages(this Exception ex)
        {
            return GetRecursiveInnerExceptionMessages(ex);
        }

        private static string GetRecursiveInnerExceptionMessages(Exception ex, string lastMsg = "")
        {
            var msg = ex.Message;
            var msg1 = msg;

            if (ex.InnerException != null)
            {
                if (msg == lastMsg)
                {
                    msg += Environment.NewLine + GetRecursiveInnerExceptionMessages(ex.InnerException, msg1);
                }
                else
                {
                    msg += Environment.NewLine + GetRecursiveInnerExceptionMessages(ex.InnerException, msg1);
                }
            }
            return msg;
        }
    }
}
