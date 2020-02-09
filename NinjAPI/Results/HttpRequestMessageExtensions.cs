using NinjAPI.Query;
using NinjAPI.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace NinjAPI.Results
{
    public static class HttpRequestMessageExtensions
    {

        /// <summary>
        /// Returns a NameValueCollection of QueryStrings that's easier to work with 
        /// than GetQueryNameValuePairs KevValuePairs collection.
        /// 
        /// If you need to pull a few single values use GetQueryString instead.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static NameValueCollection GetQueryStrings(this HttpRequestMessage request)
        {
            NameValueCollection queryStrings = new NameValueCollection();

            return request.GetQueryNameValuePairs().Aggregate(queryStrings, (result, el) => {
                result.Add(el.Key, el.Value);
                return result;
            });               
        }

        /// <summary>
        /// Returns a NameValueCollection of RouteValues that's easier to work with 
        /// than GetQueryNameValuePairs KevValuePairs collection.
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static NameValueCollection GetRouteValues(this HttpRequestMessage request)
        {
            NameValueCollection queryStrings = new NameValueCollection();

            return request.GetRouteData().Values.Aggregate(queryStrings, (result, el) => {
                result.Add(el.Key, el.Value.ToString());
                return result;
            });
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.OK (200) that a list result from a IQueryable
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="collection"></param>
        /// <param name="entityType"></param>
        /// <param name="headerLinks"></param>
        /// <returns></returns>
        public static HttpResponseMessage Collection(this HttpRequestMessage Request, QueryData queryData, IQueryable collection)
        {
            var response = Request.CreateResponse(HttpStatusCode.OK, collection);
            //long? count = collection.Count(queryData.EntityType)();

            if (queryData.TotalResultCount != null)
            {
                response.Headers.Add("X-Total-Count", queryData.TotalResultCount.ToString());
                if(queryData.TotalResultCount > 0 && queryData.paginate)
                {
                    var headerLinks = queryData.CreatePagingLinks(Request.RequestUri.ToString(), queryData.TotalResultCount ?? 1);
                    response.Headers.Add("Link", string.Join(", ", headerLinks));
                }       
            }

            return response;
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.OK (200).
        /// </summary>
        /// <param name="content">response content</param>
        public static HttpResponseMessage Ok(this HttpRequestMessage Request, object content)
        {
            return Request.CreateResponse(HttpStatusCode.OK, content);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.Created (201)
        /// that sets location header for a new entry.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="location"></param>
        public static HttpResponseMessage Created(this HttpRequestMessage Request, object content, string location)
        {
            var message = Request.CreateResponse(HttpStatusCode.Created, content);
            message.Headers.Location = new Uri(location);
            return message;
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.Accepted (202)
        /// </summary>
        /// <param name="content">response content</param>
        public static HttpResponseMessage Accepted(this HttpRequestMessage Request, object content)
        {
            return Request.CreateResponse(HttpStatusCode.Accepted, content);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.NoContent (204)
        /// </summary>
        /// <returns></returns>
        public static HttpResponseMessage NoContent(this HttpRequestMessage Request)
        {
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.BadRequest (400)
        /// and attach validation errors.
        /// </summary>
        /// <param name="errors"></param>
        public static HttpResponseMessage BadRequest(this HttpRequestMessage Request, params string[] errors)
        {
            var ModelState = new ModelStateDictionary();
            for (int i = 0; i < errors.Length; i++)
                ModelState.AddModelError(i.ToString(), errors[i]);
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,ModelState);
        }

        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.Forbidden (403)
        /// and a description message
        /// </summary>
        /// <param name="message">custom error message</param>
        public static HttpResponseMessage Forbidden(this HttpRequestMessage Request, string message)
        {
            var ModelState = new ModelStateDictionary();
            ModelState.AddModelError("Model", message);
            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, ModelState);
        }


        /// <summary>
        /// Builds an element System.Net.Http.HttpResponseMessage with HttpStatusCode.NotFound (404)
        /// and a description message
        /// </summary>
        /// <param name="message">custom error message</param>
        public static HttpResponseMessage NotFound(this HttpRequestMessage Request, string message)
        {
            var ModelState = new ModelStateDictionary();
            ModelState.AddModelError("Model", message);
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, ModelState);
        }
    }
}
