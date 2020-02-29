using NinjAPI.Common;
using NinjAPI.Query.Expressions;
using NinjAPI.Results;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace NinjAPI.Query
{
    public class QueryableAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        //public string MappingDelegate = null;

        /// <summary>
        /// 
        /// </summary>
        public string DefaultOrder = null;

        /// <summary>
        /// 
        /// </summary>
        private QueryData queryData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw ErrorHelper.ArgumentNull("actionContext");
            }

            HttpRequestMessage request = actionContext.Request;
            if (request == null)
            {
                throw ErrorHelper.ArgumentNull("actionContext.Request");
            }

            if(actionContext.ControllerContext.Controller is INinjaController && DefaultOrder == null)
            {
                var controller = actionContext.ControllerContext.Controller as INinjaController;
                DefaultOrder = controller.DbSetKey.Name;
            }

            //Create Query
            this.queryData = QueryParser.CreateAndValidateQuery(actionContext, DefaultOrder);

            // Placed here 'cause QueryParser.CreateAndValidateQuery will fill ModelState errors if queryData is not valid
            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw ErrorHelper.ArgumentNull("actionExecutedContext");
            }

            HttpRequestMessage request = actionExecutedContext.Request;
            if (request == null)
            {
                throw ErrorHelper.ArgumentNull("actionExecutedContext.Request");
            }

            if (!queryData.IsValid)
            {
                throw new InvalidOperationException("Invalid QueryData");
            }

            HttpResponseMessage response = actionExecutedContext.Response;

            if (response != null && response.IsSuccessStatusCode && response.Content != null)
            {
                if (!(response.Content is ObjectContent responseContent))
                {
                    throw new InvalidOperationException("actionExecutedContext QueryingRequiresObjectContent");
                }

                IQueryable queryableActionResult = null;
                Func<IQueryable, IQueryable> mapper = null;
                Type resultType = responseContent.ObjectType;

                if(responseContent.Value is INinjable ninjable)
                {
                    queryableActionResult = ninjable.Queryable;
                    mapper = ninjable.Mapper;
                }
                else if (responseContent.Value is IEnumerable enumResult)
                {
                    queryableActionResult = enumResult.AsQueryable();
                }
                else if (responseContent.Value is IQueryable queryableResult)
                {
                    queryableActionResult = queryableResult;
                }

                if (queryableActionResult == null)
                    throw new InvalidOperationException("Only Collections Supported");

                //Performe requested query
                IQueryable queryResult = ApplyQuery(queryableActionResult, this.queryData);

                //apply mapper
                if(mapper != null)
                {
                    queryResult = mapper(queryResult);
                }

                //No result
                if (queryResult == null)
                {
                    actionExecutedContext.Response = request.NoContent();
                }

                actionExecutedContext.Response = request.Collection(this.queryData, queryResult);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="queryData"></param>
        /// <returns></returns>
        private IQueryable ApplyQuery(IQueryable queryable, QueryData queryData)
        {
            if (queryable == null)
            {
                throw ErrorHelper.ArgumentNull("queryable");
            }
            if (queryData == null)
            {
                throw ErrorHelper.ArgumentNull("queryOptions");
            }

            return this.queryData.ApplyTo(queryable);
        }
    }
}
