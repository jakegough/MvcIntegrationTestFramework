using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using MvcIntegrationTestFramework.Interception;

namespace MvcIntegrationTestFramework.Browsing
{
    public class BrowsingSession
    {
        public HttpSessionState Session { get; private set; }
        public readonly HttpCookieCollection Cookies;

        public BrowsingSession()
        {
            Cookies = new HttpCookieCollection();
        }

        public RequestResult Get(string url)
        {
            return ProcessRequest(url, HttpVerbs.Get, new NameValueCollection());
        }

        /// <summary>
        /// Sends a form data post to your url.
        /// </summary>
        /// <example>
        /// <code>
        /// var result = Post("registration/create", new
        /// {
        ///     Form = new
        ///     {
        ///         InvoiceNumber = "10000",
        ///         AmountDue = "10.00",
        ///         Email = "chriso@innovsys.com",
        ///         Password = "welcome",
        ///         ConfirmPassword = "welcome"
        ///     }
        /// });
        /// </code>
        /// </example>
        public RequestResult Post(string url, object formData)
        {
            var formNameValueCollection = NameValueCollectionConversions.ConvertFromObject(formData);
            return ProcessRequest(url, HttpVerbs.Post, formNameValueCollection);
        }

        /// <summary>
        /// Make a request to the app, with custom verb, headers and body
        /// </summary>
        public RequestResult Request(string url, HttpVerbs httpVerb, NameValueCollection headers, byte[] bodyData)
        {
            return ProcessRequest(url, httpVerb, bodyData, headers);
        }

        private RequestResult ProcessRequest(string url, HttpVerbs httpVerb = HttpVerbs.Get, NameValueCollection formValues = null)
        {
            return ProcessRequest(url, httpVerb, NameValueCollectionConversions.SerialiseFormData(formValues), null);
        }

        private RequestResult ProcessRequest(string url, HttpVerbs httpVerb, byte[] bodyData, NameValueCollection headers)
        {
            if (url == null) throw new ArgumentNullException("url");

            // Fix up URLs that incorrectly start with / or ~/
            if (url.StartsWith("~/"))
                url = url.Substring(2);
            else if(url.StartsWith("/"))
                url = url.Substring(1);

            // Parse out the querystring if provided
            string query = "";
            int querySeparatorIndex = url.IndexOf("?", StringComparison.Ordinal);
            if (querySeparatorIndex >= 0) {
                query = url.Substring(querySeparatorIndex + 1);
                url = url.Substring(0, querySeparatorIndex);
            }                

            // Perform the request
            LastRequestData.Reset();
            var output = new StringWriter();
            string httpVerbName = httpVerb.ToString().ToLower();
            var workerRequest = new SimulatedWorkerRequest(url, query, output, Cookies, httpVerbName, bodyData, headers);
            HttpRuntime.ProcessRequest(workerRequest);

            // Capture the output
            AddAnyNewCookiesToCookieCollection();
            Session = LastRequestData.HttpSessionState;

            // In the case of errors, try to pull out some useful info from the HttpContext
            var response = LastRequestData.Response ??
                           RecoverResponseData(LastRequestData.ActionExecutedContext, workerRequest);

            return new RequestResult
            {
                ResponseText = output.ToString(),
                ActionExecutedContext = LastRequestData.ActionExecutedContext,
                ResultExecutedContext = LastRequestData.ResultExecutedContext,
                Response = response
            };
        }

        private HttpResponse RecoverResponseData(ControllerContext ctx, SimulatedWorkerRequest workerRequest)
        {
            if (ctx == null || ctx.HttpContext == null || ctx.HttpContext.Response == null)
            {
                return new HttpResponse(TextWriter.Null)
                {
                    StatusCode = workerRequest.LastStatusCode,
                    StatusDescription = workerRequest.LastStatusDescription
                };
            }

            var orig = ctx.HttpContext.Response;
            return new HttpResponse(TextWriter.Null)
            {
                StatusCode = orig.StatusCode,
                Charset = orig.Charset,
                ContentType = orig.ContentType,
                RedirectLocation = orig.RedirectLocation
            };
        }

        private void AddAnyNewCookiesToCookieCollection()
        {
            if (LastRequestData.Response == null) return;

            var lastResponseCookies = LastRequestData.Response.Cookies;

            foreach (string cookieName in lastResponseCookies) {
                var cookie = lastResponseCookies[cookieName];
                if (Cookies[cookieName] != null)
                    Cookies.Remove(cookieName);
                if(cookie != null && (cookie.Expires == default(DateTime) || cookie.Expires > DateTime.Now))
                    Cookies.Add(cookie);
            }
        }
    }
}