using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Customers.Api.Models;

namespace Customers.Api.Handlers
{
    public class ResponseHandler : IHttpActionResult
    {
        private List<string> Errors { get; set; }
        private string Message { get; set; }
        private object Result { get; set; }

        private HttpStatusCode StatusCode { get; set; }
        private HttpRequestMessage Request { get; set; }

        public ResponseHandler() { }

        public ResponseHandler(HttpRequestMessage request, HttpStatusCode statusCode, List<string> errors, string message, object result)
        {
            StatusCode = statusCode;
            Errors = errors;
            Message = message;
            Result = result;
            Request = request;
        }

        /// <summary>Creates an <see cref="T:System.Net.Http.HttpResponseMessage" /> asynchronously.</summary>
        /// <returns>A task that, when completed, contains the <see cref="T:System.Net.Http.HttpResponseMessage" />.</returns>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var responseContent = new CustomerResponseModel()
            {
                Errors = Errors,
                Message = Message,
                Result = Result
            };

            var response = new HttpResponseMessage()
            {
                Content = new ObjectContent(responseContent.GetType(), responseContent, new JsonMediaTypeFormatter()),
                StatusCode = StatusCode,
                RequestMessage = Request
            };

            return Task.FromResult(response);
        }
    }
}