using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace ServerConsole.Infrastructure.DelegatingHandlers
{
    /// <summary>
    /// Replacement of content-type: application/json by text/html for
    /// IE compatibility with multipart/form-data requests.
    /// </summary>
    public class JsonForIEDelegateHandler : System.Net.Http.DelegatingHandler
    {
        private readonly string _contentTypeNewValue;

        public JsonForIEDelegateHandler(string contentTypeNewValue = "text/html; charset=utf-8")
        {
            if (string.IsNullOrEmpty(contentTypeNewValue))
                throw new ArgumentNullException("contentTypeNewValue");
            _contentTypeNewValue = contentTypeNewValue;
        }

        public JsonForIEDelegateHandler(HttpConfiguration config, string contentTypeNewValue = "text/html; charset=utf-8")
        {
            if (config == null)
                throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(contentTypeNewValue))
                throw new ArgumentNullException("contentTypeNewValue");
            _contentTypeNewValue = contentTypeNewValue;
            InnerHandler = new HttpControllerDispatcher(config);
        }

        protected override System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>((responseToCompleteTask) =>
            {
                HttpResponseMessage response = responseToCompleteTask.Result;

                const string contentTypeHeader = "Content-Type";
                const string contentTypeJson = "application/json";

                if (response.IsSuccessStatusCode &&
                    response.Content != null &&
                    response.RequestMessage.Headers.Accept != null &&
                    response.RequestMessage.Headers.Accept.Count(t => t.MediaType == contentTypeJson) == 0
                )
                {
                    IEnumerable<string> values;
                    if (response.Content.Headers.TryGetValues(contentTypeHeader, out values))
                    {
                        var first = values.First();
                        if (first.StartsWith(contentTypeJson, StringComparison.OrdinalIgnoreCase))
                        {
                            response.Content.Headers.Remove(contentTypeHeader);
                            response.Content.Headers.Add(contentTypeHeader, _contentTypeNewValue);
                        }
                    }
                }

                return response;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}