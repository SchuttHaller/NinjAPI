# NinjAPI

### What is NinjAPI?

NinjAPI is a REST API builder library based on AspNet WebApi and EF that allows creating CRUD operations, pagination and filter in a fast and easy way.
Created with didactic purposes and to deal with boring product catalog APIs, all inspired by ODATA.

### How do I get started?

 First, configure NinjAPI in your WebApiConfig startup:
 
 ```csharp
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
	{
    	config.NinjAPIConfig();
    }
}
 ```
 
 Once you have created you EF models, create your first NinjaController and declare DbContext property that your API will use:
 
```csharp
[RoutePrefix("api/students")]
public class StudentsController : NinjaController<Students>
{
	private readonly DbContext _DbContext = new SchoolContext();
	protected override DbContext DbContext => _DbContext;
}
```

And that's it, the controller now has GET, POST, PUT and DELETE operations for your model with no more configuration needed.

### Routing

Keep in mind that by this moment you must use the **RoutePrefix** attribute to set the controller route. So the routes for the above example would be:

```
GET    /api/students	
GET    /api/students/{key}	 
POST   /api/students	 
PUT    /api/students/{key} 
DELETE /api/students/{key}
```
An automated routing its pretended to future versions.

### Collection results 

Collection results i.e. `GET    /api/students ` by default will not paginate the results so a request made to this action will return every record on DB. 

#### Pagination
To get a paginated result just provide the query params `?pagesize` and `?page`.`?page` default value takes **1** if it is not received.

##### Extra Headers
When a result is paginated the response will contain two extra headers, **Link** and **X-Total-Count**. Link header has links to first, prev, next and last page and X-Total-Count the total count of entries in db i.e.

```
https://myaplication.com/api/students?page=5&pageSize=10
  
HTTP/1.1 200 OK
Server: Microsoft-IIS/10.0
Date: Tue, 29 Mar 2019 03:23:44 GMT
Content-Type: application/json; charset=utf-8
Status: 200 OK
Link: <https://myaplication.com/api/students?page=1&pageSize=10>; rel="first", 
<https://myaplication.com/api/students?page=4&pageSize=10>; rel="prev", 
<https://myaplication.com/api/students?page=6&pageSize=10>; rel="next", 
<https://myaplication.com/api/students?page=10&pageSize=10>; rel="last"
X-Total-Count: 100

Content-Length: 323
Cache-Control: no-cache
Pragma →no-cache

```

#### Order
You can specify the order of results by providing `?orderby` which can receive up to two columns i.e.

```
https://myaplication.com/api/students?orderby=id
https://myaplication.com/api/students?orderby=lastname
https://myaplication.com/api/students?orderby=id desc/lastname
https://myaplication.com/api/students?orderby=id/lastname desc
```
where `desc` is descending order



#### Filter
To filter results the ```?filter``` param allows expressions in a string with syntax: ```[Field/Column] [operator] [Value]``` and allows the logic operators **and** & **or** to mix expressions.  

i.e.

```
https://myaplication.com/api/students?filter=id eq 1
https://myaplication.com/api/students?filter=EnrollmentDate gt 2014-01-04 and lastname lk Smith
https://myaplication.com/api/students?filter=lastname lk Smith or lastname lk Johnson
```

**Allowed operators**  
  
| Operator | Description           |
|----------|-----------------------|
| eq       | Equals                |
| ne       | Not Equals            |
| gt       | Greater Than          |
| ge       | Greater Than or Equal |
| lt       | Less Than             |
| le       | Less Than or Equal    |
| lk       | Like                  |
| and      | Logical conjunction   |
| or	   | Logical disjunction   |


