using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Query.Mocks
{
    public class MockEntityChild
    {
        public int Id { get; set; }
        public int AnotherId { get; set; }
        public string Name { get; set; } = null!;

    }
}
