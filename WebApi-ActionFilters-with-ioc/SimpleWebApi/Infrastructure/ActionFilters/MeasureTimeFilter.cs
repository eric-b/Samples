using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SimpleWebApi.Infrastructure.ActionFilters
{
    /// <summary>
    /// Per request filter.
    /// </summary>
    public class MeasureTimeFilter : IActionFilter<MeasureTimeFilterAttribute>
    {
        private readonly ILogger _logger;
        private DateTime _startedAt;

        public MeasureTimeFilter(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(paramName: "logger");
            _logger = logger;
        }

        public Task OnActionExecutingAsync(MeasureTimeFilterAttribute attribute, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            _startedAt = DateTime.UtcNow;
            _logger.Debug("Executing {0}.{1} with '{2}'...", actionContext.ActionDescriptor.ControllerDescriptor.ControllerName, actionContext.ActionDescriptor.ActionName, attribute.Label);
            return Task.CompletedTask;
        }

        public Task OnActionExecutedAsync(MeasureTimeFilterAttribute attribute, HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            _logger.Debug("{0}.{1} executed in {2:F02} ms.", actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName, actionExecutedContext.ActionContext.ActionDescriptor.ActionName, DateTime.UtcNow.Subtract(_startedAt).TotalMilliseconds);
            return Task.CompletedTask;
        }
    }
}