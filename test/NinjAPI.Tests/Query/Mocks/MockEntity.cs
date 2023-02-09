using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Query.Mocks
{
    public class MockEntity
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }    
        public string? Description { get; set; }
        public decimal Revenue { get; set; }
        public DateTime Created { get; set; }
        public virtual ICollection<MockEntityChild>? Children { get; set; }

    }
}
