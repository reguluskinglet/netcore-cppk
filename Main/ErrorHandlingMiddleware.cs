#define DEBUG
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rzdppk.Core.Other;
using System;
using System.Net;
using System.Threading.Tasks;


namespace Rzdppk
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.OK; // 500 if unexpected
            context.Response.ContentType = "application/json; charset=utf-8";

            /* if (exception is MyNotFoundException) code = HttpStatusCode.NotFound;
             else if (exception is MyUnauthorizedException) code = HttpStatusCode.Unauthorized;
             else if (exception is MyException) code = HttpStatusCode.BadRequest;*/
            if (exception is Other.GenaException) code = HttpStatusCode.Conflict;
            else if (exception is Other.NotFoundException) code = HttpStatusCode.NotFound;
            else if (exception is System.Data.SqlClient.SqlException &&
                     ((System.Data.SqlClient.SqlException) exception).ErrorCode == -2146232060)
                    {
                        code = HttpStatusCode.InternalServerError;
                        
                        context.Response.StatusCode = (int)code;
                        return context.Response.WriteAsync(JsonConvert.SerializeObject(new MessageAndTrace{Message =  Error.DbError}));
            }
            else code = HttpStatusCode.InternalServerError;

            //var result = JsonConvert.SerializeObject(exception.Message);

            var result = JsonConvert.SerializeObject(new MessageAndTrace(exception));
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }

        private class MessageAndTrace
        {
            internal MessageAndTrace()
            {
            }

            internal MessageAndTrace(Exception exception)
            {
                Type = exception.GetType().ToString();
                Message = exception.Message;
                Trace = exception.StackTrace.Substring(0, 80);
#if (DEBUG) 
                Trace = exception.StackTrace;
#endif
            }

            public string Type { get; set; }
            public string Message { get; set; }
            public string Trace { get; set; }

        }
    }
}
