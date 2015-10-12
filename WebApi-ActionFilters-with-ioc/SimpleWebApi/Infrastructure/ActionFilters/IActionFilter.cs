using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SimpleWebApi.Infrastructure.ActionFilters
{
    public interface IActionFilter<TAttribute> where TAttribute : Attribute
    {
        Task OnActionExecutingAsync(TAttribute attribute, HttpActionContext actionContext, CancellationToken cancellationToken);
        Task OnActionExecutedAsync(TAttribute attribute, HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken);
    }
}
