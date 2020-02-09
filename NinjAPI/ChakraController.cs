using NinjAPI.Properties;
using NinjAPI.Validation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace NinjAPI
{
    [ValidateModelState]
    public abstract class ChakraController : ApiController
    {
        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.OK (200).
        /// </summary>
        /// <param name="content">response content</param>
        public HttpResponseMessage Ok(object content)
        {
            return Request.CreateResponse(HttpStatusCode.OK, content);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.Created (201)
        /// that sets location header for a new entry.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="location"></param>
        public HttpResponseMessage Created(object content, string location)
        {
            var message = Request.CreateResponse(HttpStatusCode.Created, content);
            message.Headers.Location = new Uri(location);
            return message;
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.Accepted (202)
        /// </summary>
        /// <param name="content">response content</param>
        public HttpResponseMessage Accepted(object content)
        {
            return Request.CreateResponse(HttpStatusCode.Accepted, content);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.NoContent (204)
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage NoContent()
        {
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.BadRequest (400)
        /// and attach validation errors.
        /// </summary>
        /// <param name="errors"></param>
        public HttpResponseMessage BadRequest(List<KeyValuePair<string, string>> errors)
        {
            var ModelState = new ModelStateDictionary();
            foreach(var error in errors)
                ModelState.AddModelError(error.Key, error.Value);
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.BadRequest (400)
        /// and attach validation errors.
        /// </summary>
        /// <param name="errors"></param>
        public HttpResponseMessage BadRequest(KeyValuePair<string, string> error)
        {
            var ModelState = new ModelStateDictionary();
            if (!string.IsNullOrEmpty(error.Value))
                ModelState.AddModelError(error.Key, error.Value);
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.Forbidden (403)
        /// and a description message
        /// </summary>
        /// <param name="message">custom error message</param>
        public HttpResponseMessage Forbidden(string message)
        {
            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, message ?? "Forbidden");
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.NotFound (404)
        /// and a description message
        /// </summary>
        /// <param name="message">custom error message</param>
        public HttpResponseMessage NotFound(string message = null)
        {            
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, message ?? Resources.ResourceNotFound);
        }
    }
}
