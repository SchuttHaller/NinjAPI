using NinjAPI.UseExample.DataAccess;
using NinjAPI.UseExample.Models;
using System.Data.Entity;
using System.Web.Http;

namespace NinjAPI.UseExample.Controllers
{
    [RoutePrefix("api/students")]
    public class StudentsController : NinjaController<Student>
    {
        private readonly SchoolContext SchoolContext = new SchoolContext();
        protected override DbContext DbContext => SchoolContext;
    }
}
