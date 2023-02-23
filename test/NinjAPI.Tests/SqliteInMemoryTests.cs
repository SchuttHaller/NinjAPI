using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NinjAPI.Tests.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NinjAPI.Tests
{
    public class SqliteInMemoryTests
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<SchoolDbContext> _contextOptions;

        #region ConstructorAndDispose
        public SqliteInMemoryTests()
        {

            
            // Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
            // at the end of the test (see Dispose below).
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // These options will be used by the context instances in this test suite, including the connection opened above.
            _contextOptions = new DbContextOptionsBuilder<SchoolDbContext>()
           .UseSqlite(_connection)
           .Options;

            // Create the schema
            using var context = new SchoolDbContext(_contextOptions);
            context.Database.EnsureCreated();
        }

        SchoolDbContext CreateContext() => new (_contextOptions);

        public void Dispose() => _connection.Dispose();
        #endregion
    }
}
