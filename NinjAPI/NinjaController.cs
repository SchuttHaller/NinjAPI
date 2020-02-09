using NinjAPI.Common;
using NinjAPI.Properties;
using NinjAPI.Query;
using NinjAPI.Results;
using System;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace NinjAPI
{
    /// <summary>
    /// Simple bahavior of NinjaControllers, needs to expose its primary key
    /// </summary>
    public interface INinjaController
    {
        PropertyInfo DbSetKey { get; set;}
    }

    /// <summary>
    /// API controller charged with powerfull chakras and automated basic REST methods 
    /// that allows to create a simple API CRUD just providing an EF Entity ant its contexts
    /// </summary>
    /// <typeparam name="CollectionTEntity"></typeparam>
    /// <typeparam name="CollectionDTO"></typeparam>
    /// <typeparam name="SingleTEntity"></typeparam>
    /// <typeparam name="SingleDTO"></typeparam>
    public abstract class NinjaController<CollectionTEntity, CollectionDTO, SingleTEntity, SingleDTO> : ChakraController, INinjaController
        where CollectionTEntity : class
        where CollectionDTO : class
        where SingleTEntity : class
        where SingleDTO : class
    {

        public NinjaController()
        {
            CollectionDbSet = DbContext.Set<CollectionTEntity>();
            SingleDbSet = DbContext.Set<SingleTEntity>();
            DbSetKey = SingleDbSet.GetEntityPrimaryKey(DbContext);
        }

        /// <summary>
        /// DataBase context used by controller
        /// </summary>
        protected abstract DbContext DbContext { get; }

        /// <summary>
        /// DBSet for return GET ALL action, 
        /// sometimes we need to use readonly entities like views to expose less properties
        /// </summary>
        private readonly DbSet<CollectionTEntity> CollectionDbSet;

        /// <summary>
        /// DBSet for return the detail of a single element in db 
        /// </summary>
        private readonly DbSet<SingleTEntity> SingleDbSet;

        /// <summary>
        /// Primary Key used to FIND a single element and to provide a deafult orderby
        /// </summary>
        public PropertyInfo DbSetKey { get; set; }

        /// <summary>
        /// Just access to route values
        /// </summary>
        private NameValueCollection RouteValues => ControllerContext.Request.GetRouteValues();

        /// <summary>
        /// Default method to find element by its key
        /// </summary>
        /// <param name="routeParams"></param>
        /// <returns></returns>
        protected virtual SingleTEntity Find(NameValueCollection routeParams)
        {

            if (TypeHelper.TryCast(routeParams["key"], DbSetKey.PropertyType, out object key))
            {
                return SingleDbSet.Find(key);
            }

            throw ErrorHelper.InvalidOperation(ErrorHelper.Format(Resources.ValueWithNoMatchingType, routeParams["key"], DbSetKey.Name, DbSetKey.PropertyType));
        }

        #region MAPPING METHODS

        /// <summary>
        /// Default mapping for collectionEntityType if an Collection DTO is provided
        /// </summary>
        /// <returns></returns>
        protected virtual CollectionDTO AsDTO(CollectionTEntity entity)
        {
            CollectionDTO dto = Activator.CreateInstance<CollectionDTO>();
            PropertyInfo[] dtoProperties = entity.GetType().GetProperties();

            foreach (PropertyInfo property in dto.GetType().GetProperties())
            {
                PropertyInfo dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == property.Name);
                if (dtoProperty != null)
                    property.SetValue(dto, dtoProperty.GetValue(entity));
            }
            return dto;
        }

        /// <summary>
        ///  Default mapping for SingleEntityType if an DTO is provided
        /// </summary>
        /// <returns></returns>
        protected virtual SingleDTO AsDTO(SingleTEntity entity)
        {
            // not using DTO no mapping needed
            if (typeof(SingleDTO) == typeof(SingleTEntity))
                return entity as SingleDTO;

            SingleDTO dto = Activator.CreateInstance<SingleDTO>();
            PropertyInfo[] dtoProperties = entity.GetType().GetProperties();

            foreach (PropertyInfo property in dto.GetType().GetProperties())
            {
                PropertyInfo dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == property.Name);
                if (dtoProperty != null)
                    property.SetValue(dto, dtoProperty.GetValue(entity));
            }
            return dto;
        }

        /// <summary>
        /// Deafult mapping DTO to Entity -> if using DTOs
        /// </summary>
        /// <returns></returns>
        protected virtual SingleTEntity AsEntity(SingleDTO dto, SingleTEntity source)
        {
            PropertyInfo[] dtoProperties = dto.GetType().GetProperties();

            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                PropertyInfo dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == property.Name);
                if (dtoProperty != null)
                    property.SetValue(source, dtoProperty.GetValue(dto));
            }
            return source;
        }

        /// <summary>
        /// Create to do default mapping of a collection result
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual IQueryable CollectionMapping(IQueryable result)
        {
            //Not using DTO to collection result
            if (typeof(CollectionDTO) == typeof(CollectionTEntity))
                return result;

            return result.Cast<CollectionTEntity>().AsEnumerable().Select(x => AsDTO(x)).AsQueryable();
        }

        #endregion

       
        #region  API REST METHODS
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("")]
        [Queryable(MappingDelegate = "CollectionMapping")]
        public IQueryable<CollectionTEntity> GetAll()
        {
            return CollectionDbSet.AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("{key:minlength(1)}")]
        public HttpResponseMessage Get([FromUri] string key)
        {
            SingleDTO result = AsDTO(Find(RouteValues));

            return result != null ?
                  Ok(result)
                : NotFound();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost, Route("")]
        public virtual HttpResponseMessage Post([FromBody] SingleDTO input)
        {
            //if null input is a bad request...
            if (input == null)
                BadRequest(string.Format(Resources.ArgumentNullOrEmpty, typeof(SingleDTO).Name));

            SingleTEntity newEntry = AsEntity(input, Activator.CreateInstance<SingleTEntity>());
            SingleDbSet.Add(newEntry);

            return ValidateSaveDBContext(DbContext)
                ?? Created(AsDTO(newEntry), Request.RequestUri + "/" + newEntry.GetPropValue(DbSetKey.Name).ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut, Route("{key:minlength(1)}")]
        public virtual HttpResponseMessage Put([FromBody] SingleDTO input)
        {
            //if null input is a bad request...
            if (input == null)
                BadRequest(string.Format(Resources.ArgumentNullOrEmpty, typeof(SingleDTO).Name));

            SingleTEntity originalEntry = Find(RouteValues);
            if (originalEntry == null)
                return NotFound();

            SingleTEntity result = AsEntity(input, originalEntry);
            DbContext.Entry(result).State = EntityState.Modified;
            return ValidateSaveDBContext(DbContext) ?? NoContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpDelete, Route("{key:minlength(1)}")]
        public virtual HttpResponseMessage Delete()
        {
            SingleTEntity chakra = Find(RouteValues);

            if (chakra == null)
                return NotFound();

            SingleDbSet.Remove(chakra);
            return ValidateSaveDBContext(DbContext) ?? NoContent();
        }

        #endregion

        /// <summary>
        /// Validation for dbcontext.SaveChanges() returns null if all is OK or BadRequest with validation errors in other case.
        /// </summary>
        protected HttpResponseMessage ValidateSaveDBContext(DbContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => KeyValuePair.Create(x.PropertyName, x.ErrorMessage)).ToList();
                
                return BadRequest(errorMessages);
            }
            catch (Exception ex)
            {
                return BadRequest(KeyValuePair.Create(ex.Source, ex.Message));
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DbContext != null)
                {
                    DbContext.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }

    public abstract class NinjaController<SingleTEntity, SingleDTO> : NinjaController<SingleTEntity, SingleDTO, SingleTEntity, SingleDTO>
        where SingleTEntity : class
        where SingleDTO : class
    {}

    public abstract class NinjaController<SingleTEntity> : NinjaController<SingleTEntity, SingleTEntity, SingleTEntity, SingleTEntity>
        where SingleTEntity : class
    {}
}
