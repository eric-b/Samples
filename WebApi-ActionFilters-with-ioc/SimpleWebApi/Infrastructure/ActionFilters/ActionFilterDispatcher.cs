using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SimpleWebApi.Infrastructure.ActionFilters
{
    public class ActionFilterDispatcher : IActionFilter
    {
        public bool AllowMultiple
        {
            get
            {
                return true;
            }
        }

        private readonly Func<Type, IEnumerable<object>> _container;

        public ActionFilterDispatcher(Func<Type, IEnumerable<object>> container)
        {
            if (container == null)
                throw new ArgumentNullException(paramName: "container");
            _container = container;
        }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext context,
            CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            HttpActionDescriptor descriptor = context.ActionDescriptor;
            Attribute[] attributes = descriptor.ControllerDescriptor
                .GetCustomAttributes<Attribute>(inherit: true)
                .Concat(descriptor.GetCustomAttributes<Attribute>(inherit: true))
                .ToArray();

            var attributeAndFilters = new Tuple<dynamic, dynamic[]>[attributes.Length];
            for (int i = 0; i < attributes.Length; i++)
            {
                Attribute attribute = attributes[i];
                Type attrType = attribute.GetType();
                Type filterType = typeof(IActionFilter<>).MakeGenericType(attrType);
                dynamic[] filters = _container.Invoke(filterType).ToArray();
                attributeAndFilters[i] = new Tuple<dynamic, dynamic[]>((dynamic)attribute, filters);
            }

            foreach (var element in attributeAndFilters)
            {
                foreach (dynamic actionFilter in element.Item2)
                {
                    await actionFilter.OnActionExecutingAsync(element.Item1, context, cancellationToken);
                }
            }

            var executedContext = new HttpActionExecutedContext(context, exception: null);
            if (context.Response == null)
            {
                try
                {
                    executedContext.Response = await continuation();
                }
                catch (Exception ex)
                {
                    context.Response = null;
                    if (ex is HttpResponseException)
                        return ((HttpResponseException)ex).Response;

                    return context.Request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, ex);
                }
            }

            foreach (var element in attributeAndFilters)
            {
                foreach (dynamic actionFilter in element.Item2)
                {
                    await actionFilter.OnActionExecutedAsync(element.Item1, executedContext, cancellationToken);
                }
            }

            return executedContext.Response;
        }
    }
}