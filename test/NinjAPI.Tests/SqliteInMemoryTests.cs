using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NinjAPI.Tests.Models;
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
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<BookStoreDbContext> _contextOptions;

        #region ConstructorAndDispose
        public SqliteInMemoryTests()
        {

            
            // Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
            // at the end of the test (see Dispose below).
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add user-defined function
            _connection.CreateFunction("FirstChar", (string arg) => arg[..1]);

            // These options will be used by the context instances in this test suite, including the connection opened above.
            _contextOptions = new DbContextOptionsBuilder<BookStoreDbContext>()
           .UseSqlite(_connection)
           .Options;

            // Create the schema
            using var context = new BookStoreDbContext(_contextOptions);

            if (context.Database.EnsureCreated())
            {
                using var command = _connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT FirstChar('ABCDEFG')
                ";

                var a = command.ExecuteScalar();
            }

            AddTestData(context);
            context.SaveChanges();
        }

        protected BookStoreDbContext CreateContext() => new (_contextOptions);

        public void Dispose() => _connection.Dispose();
        #endregion

        #region Data
        private void AddTestData(BookStoreDbContext context)
        {
            context.AddRange(
              new Author
              {
                  Name = "Ernest Hemingway",
                  Books = new[]
                  {
                       new Book {
                           Id = Guid.Parse("302cdb59-3e22-43ab-8f17-69edf2e8fa59"),
                           Name = "For Whom the Bell Tolls",
                           Stock= 10,
                           Price= 10,
                       },
                        new Book {
                           Id = Guid.Parse("2eeb957c-a1aa-4e9c-a600-16401b29a30a"),
                           Name = "The Old Man and the Sea",
                           Stock= 8,
                           Price= 9,
                       }
                  }
              },
              new Author
              {
                  Name = "Federico García Lorca",
                  Books = new[]
                  {
                       new Book {
                           Id = Guid.Parse("86e68fbe-90c1-475c-84ed-34d2231e8006"),
                           Name = "Bodas de Sangre",
                           Stock= 12,
                           Price= 5,
                       },
                        new Book {
                           Id = Guid.Parse("461a4f87-d280-46fd-bb07-5d0d44da8803"),
                           Name = "Yerma",
                           Stock= 5,
                           Price= 8,
                       }
                  }
              },
              new Author
              {
                  Name = "Hermann Hesse",
                  Books = new[]
                  {
                       new Book {
                           Id = Guid.Parse("11521b88-4d59-4bba-9083-a5299f2cf41f"),
                           Name = "Der Steppenwolf",
                           Stock= 20,
                           Price= 15,
                       },
                       new Book {
                           Id = Guid.Parse("49b77eda-bc7f-4e94-8a34-b9a10251a2c5"),
                           Name = "Demian: Die Geschichte von Emil Sinclairs Jugend",
                           Stock= 18,
                           Price= 13,
                       },
                       new Book {
                           Id = Guid.Parse("ad54c999-49fb-4128-b9fa-ae06fe796bf9"),
                           Name = "Siddhartha",
                           Stock= 6,
                           Price= 12,
                       }
                  }
              },
              new Author
              {
                  Name = "Juan Rulfo",
                  Books = new[]
                  {
                       new Book {
                           Id = Guid.Parse("e0ed92d6-f2d9-47f9-8746-b16310f34f9b"),
                           Name = "Pedro Páramo",
                           Stock= 12,
                           Price= 15,
                       },
                       new Book {
                           Id = Guid.Parse("6d3ad2ec-0785-463a-b4d1-67cc63f9eb62"),
                           Name = "Llano en Llamas",
                           Stock= 5,
                           Price= 13,
                       }
                  }
              });

            context.AddRange(
                new Sale { 
                    Date = DateTime.Parse("2023-01-12T12:30:02"),
                    Items = new[]
                    {
                        new SaleItem { 
                            BookId = Guid.Parse("302cdb59-3e22-43ab-8f17-69edf2e8fa59"),
                            Price = 10,
                            Discount= 0,
                            FinalPrice= 10
                        },
                        new SaleItem {
                            BookId = Guid.Parse("ad54c999-49fb-4128-b9fa-ae06fe796bf9"),
                            Price = 12,
                            Discount= 0,
                            FinalPrice= 12
                        }
                    }
                },
                new Sale
                {
                    Date = DateTime.Parse("2023-02-06T18:10:45"),
                    Items = new[]
                    {
                        new SaleItem {
                            BookId = Guid.Parse("11521b88-4d59-4bba-9083-a5299f2cf41f"),
                            Price = 15,
                            Discount= 0,
                            FinalPrice= 15
                        },
                        new SaleItem {
                            BookId = Guid.Parse("49b77eda-bc7f-4e94-8a34-b9a10251a2c5"),
                            Price = 13,
                            Discount= 0,
                            FinalPrice= 13
                        },
                        new SaleItem {
                            BookId = Guid.Parse("e0ed92d6-f2d9-47f9-8746-b16310f34f9b"),
                            Price = 15,
                            Discount= 0,
                            FinalPrice= 15
                        },
                        new SaleItem {
                            BookId = Guid.Parse("461a4f87-d280-46fd-bb07-5d0d44da8803"),
                            Price = 8,
                            Discount= 2.5m,
                            FinalPrice= 5.5m
                        }
                    }
                },
                new Sale
                {
                    Date = DateTime.Parse("2023-02-24T19:11:11"),
                    Items = new[]
                    {
                        new SaleItem {
                            BookId = Guid.Parse("86e68fbe-90c1-475c-84ed-34d2231e8006"),
                            Price = 5,
                            Discount= 0,
                            FinalPrice= 5
                        },
                        new SaleItem {
                            BookId = Guid.Parse("6d3ad2ec-0785-463a-b4d1-67cc63f9eb62"),
                            Price = 13,
                            Discount= 0,
                            FinalPrice= 13
                        },
                         new SaleItem {
                            BookId = Guid.Parse("2eeb957c-a1aa-4e9c-a600-16401b29a30a"),
                            Price = 9,
                            Discount= 0,
                            FinalPrice= 9
                        }
                    }
                });
        }
        #endregion
    }
}
