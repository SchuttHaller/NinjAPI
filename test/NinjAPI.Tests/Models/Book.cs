using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Models
{
    public class Book
    {
        [Key]
        public Guid Id { get; init; }
        public string Name { get; set; } = default!;
        public int AuthorId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public virtual Author Author { get; set; } = default!;

        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

        public override bool Equals(object? obj)
        {
            if (obj is null) return this is null;
            if (this is null) return obj is null;

            var eObj = (Book)obj;

            return Id == eObj.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
