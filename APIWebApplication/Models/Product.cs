using LiteDbServices.Attributes;
using LiteDbServices.Models;
using System.ComponentModel.DataAnnotations;

namespace APIWebApplication.Models
{
    public class Product : LiteEntity<Guid>
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [EnsureMinimumElements(1)]
        public List<string> Colors { get; set; }
            = [];


    }
}
