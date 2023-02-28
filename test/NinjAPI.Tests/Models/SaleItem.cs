using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Models
{
    public class SaleItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }     
        public decimal Price { get; set; }       
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public Guid BookId { get; set; }
        public int SaleId { get; set; }

        public virtual Book Book { get; set; } = default!;
        public virtual Sale Sale { get; set; } = default!;

        public override bool Equals(object? obj)
        {
            if (obj is null) return this is null;
            if (this is null) return obj is null;

            var eObj = (SaleItem)obj;

            return Id == eObj.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
