using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace NinjAPI.Validation
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // The controller action is decorated with the [IgnoreModelStateValidation]
            // custom attribute => don't do anything.
            if (actionContext.ActionDescriptor.GetCustomAttributes<IgnoreModelStateValidationAttribute>().Any()) return;

            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }
    }


    public class IgnoreModelStateValidationAttribute : ActionFilterAttribute { }
}
