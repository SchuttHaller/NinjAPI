using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Models
{
    public class BookStoreDbContext : DbContext
    {
        public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : base(options){}

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId);

            modelBuilder.Entity<SaleItem>()
                .HasOne(s => s.Sale)
                .WithMany(i => i.Items)
                .HasForeignKey(i => i.SaleId);

            modelBuilder.Entity<SaleItem>()
               .HasOne(i => i.Book)
               .WithMany(b => b.SaleItems)
               .HasForeignKey(b => b.BookId);

            modelBuilder.HasDbFunction(typeof(BookStoreDbContext).GetMethod(nameof(FirstChar), new[] { typeof(string) })!)
                .HasName("FirstChar");
            
        }

        // mapping for user-defined function
        public int FirstChar(string arg) => throw new NotSupportedException();
    }
}
