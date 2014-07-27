using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using RsaRijndaelWebApi.Infrastructure.Cryptography;

namespace RsaRijndaelWebApi.Infrastructure.ActionFilters
{
    public class InternalActionFilterAttribute : ActionFilterAttribute
    {
        public const string HeaderKey = "X-Key";

        public const string HeaderIV = "X-IV";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.Request.Headers.Contains(HeaderKey) ||
                !actionContext.Request.Headers.Contains(HeaderIV))
            {
                actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (actionContext.ActionArguments.Count != 0)
            {
                try
                {
                    var cipher = (Cipher)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(Cipher));
                    if (cipher == null)
                        throw new InvalidOperationException("Cipher service not found.");

                    var key1 = cipher.DecryptKey(Convert.FromBase64String(actionContext.Request.Headers.GetValues(HeaderKey).First()));
                    var key2 = cipher.DecryptKey(Convert.FromBase64String(actionContext.Request.Headers.GetValues(HeaderIV).First()));
                    using (var decryptor = cipher.CreateDataDecryptor(key1, key2))
                    {
                        for (int i = 0; i < actionContext.ActionArguments.Count; i++)
                        {
                            var arg = actionContext.ActionArguments.ElementAt(i);
                            var argType = arg.Value.GetType();
                            if (argType == typeof(string))
                            {
                                actionContext.ActionArguments[arg.Key] = decryptor.Decrypt((string)arg.Value);
                            }
                            else if (argType == typeof(byte[]))
                            {
                                actionContext.ActionArguments[arg.Key] = decryptor.Decrypt((byte[])arg.Value);
                            }
                            else
                            {
                                throw new NotSupportedException(string.Format("Type {0} not supported", argType.Name));
                            }
                        }
                    }
                }
                catch 
                {
                    actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                    return;
                }
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response.IsSuccessStatusCode && actionExecutedContext.Response.Content != null)
            {
                HttpContent originalContent = actionExecutedContext.Response.Content;
                long? contentLength = originalContent.Headers.ContentLength;

                if (!contentLength.HasValue || contentLength.Value != 0)
                {
                    var request = actionExecutedContext.Request;
                    try
                    {
                        var actionContext = actionExecutedContext.ActionContext;
                        var cipher = (Cipher)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(Cipher));
                        if (cipher == null)
                            throw new InvalidOperationException("Cipher service not found.");

                        var key1 = cipher.DecryptKey(Convert.FromBase64String(actionContext.Request.Headers.GetValues(HeaderKey).First()));
                        var key2 = cipher.DecryptKey(Convert.FromBase64String(actionContext.Request.Headers.GetValues(HeaderIV).First()));
                        using (var encryptor = cipher.CreateDataEncryptor(key1, key2))
                        {
                            using (var originalContentStream = contentLength.HasValue ? new MemoryStream((int)contentLength.Value) : new MemoryStream())
                            {
                                originalContent.CopyToAsync(originalContentStream);
                                originalContentStream.Position = 0;
                                actionExecutedContext.Response.Content = new ByteArrayContent(encryptor.Encrypt(originalContentStream));
                            }
                        }
                    }
                    catch
                    {
                        actionExecutedContext.Response = request.CreateResponse(HttpStatusCode.Forbidden);
                        return;
                    }
                }
            }
        }
    }
}