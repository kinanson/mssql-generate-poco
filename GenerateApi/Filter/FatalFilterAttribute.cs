using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace GenerateApi.Filter
{
    public class FatalFilterAttribute : ExceptionFilterAttribute
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            LogEventInfo logEvent = new LogEventInfo();
            logEvent.Exception = actionExecutedContext.Exception;
            logger.Fatal(actionExecutedContext.Exception);
        }
    }
}