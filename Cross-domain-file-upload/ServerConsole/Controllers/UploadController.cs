using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using File = ServerConsole.Model.File;

namespace ServerConsole.Controllers
{
    public class UploadController : ApiController
    {
        public async Task<File[]> Post()
        {
            var uploadResult = new List<File>();
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            string redirectResponseToUrl = null;
            
            #region Parse multipart/form-data
            
            foreach (var contentStream in provider.Contents)
            {
                var contentDisposition = contentStream.Headers.ContentDisposition;
                if (contentDisposition == null)
                    continue;
                
                if (contentDisposition.FileName != null)
                {
                    var file = new File();
                    uploadResult.Add(file);
                    
                    file.Name = contentDisposition.FileName != null ? contentDisposition.FileName.Trim('\"') : "noname";
                    var fileBuffer = await contentStream.ReadAsByteArrayAsync();
                    file.Content = Encoding.UTF8.GetString(fileBuffer);
                    file.Length = fileBuffer.Length;
                }
                else if (contentDisposition.Name != null)
                {
                    // Champ
                    string fieldName = contentDisposition.Name.Trim('\"');
                    if (fieldName.ToLower() == "redirect")
                    {
                        redirectResponseToUrl = await contentStream.ReadAsStringAsync();
                    }
                }
            }
            
            #endregion
            
            var result = uploadResult.ToArray();
            
            if (redirectResponseToUrl != null)
            {
                #region iframe redirection
                
                string jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                Uri redirectUrl;
                bool redirect;
                if (redirectResponseToUrl.Contains("%s"))
                {
                    redirect = Uri.TryCreate(redirectResponseToUrl.Replace("%s", WebUtility.UrlEncode(jsonResponse)), UriKind.RelativeOrAbsolute, out redirectUrl);
                }
                else
                {
                    if (redirect = Uri.TryCreate(redirectResponseToUrl, UriKind.RelativeOrAbsolute, out redirectUrl))
                    {
                        var builder = new UriBuilder(redirectUrl);
                        if (builder.Query != null && builder.Query.Length > 1)
                            builder.Query = string.Format("{0}&{1}", builder.Query.Substring(1), WebUtility.UrlEncode(jsonResponse));
                        else
                            builder.Query = WebUtility.UrlEncode(jsonResponse);
                        
                        redirectUrl = builder.Uri;
                    }
                }
                if (redirect)
                {
                    var responseMsg = Request.CreateResponse(HttpStatusCode.SeeOther);
                    responseMsg.Headers.Location = redirectUrl;
                    throw new HttpResponseException(responseMsg);
                }
                
                #endregion 
            }

            return result;
        }
    }
}