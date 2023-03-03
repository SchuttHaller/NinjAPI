using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NinjAPI.Tests.Models
{
    public class Author
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime BirthDate { get; set; }

        public virtual List<Book> Books { get; set; } = new List<Book>();

        public override bool Equals(object? obj)
        {
            if (obj is null) return this is null;
            if (this is null) return obj is null;

            var eObj = (Author)obj;

            return Id == eObj.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
