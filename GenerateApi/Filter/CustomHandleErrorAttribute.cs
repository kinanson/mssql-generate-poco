using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GenerateApi.Filter
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void OnException(ExceptionContext filterContext)
        {
            LogEventInfo logEvent = new LogEventInfo();
            logEvent.Exception = filterContext.Exception;
            logger.Fatal(filterContext.Exception);
        }
    }
}