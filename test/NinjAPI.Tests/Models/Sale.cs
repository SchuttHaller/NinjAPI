using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjAPI.Tests.Models;

namespace NinjAPI.Tests.Models
{
    public class Sale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        
        public virtual List<SaleItem> Items { get; set; } = new List<SaleItem>();

        public override bool Equals(object? obj)
        {
            if (obj is null) return this is null;
            if (this is null) return obj is null;

            var eObj = (Sale)obj;

            return Id == eObj.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
