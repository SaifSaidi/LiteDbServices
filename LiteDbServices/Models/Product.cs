using LiteDbServices.Attributes;
using LiteDB;
using System.ComponentModel.DataAnnotations;

namespace LiteDbServices.Models
{

    public class ProductDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [EnsureMinimumElements(1)]
        public List<string> Colors { get; set; }
            = [];
    }



    public class Product : ILiteEntity
    {
        public Product() {

            Id = ObjectId.NewObjectId();
        }    

        public ObjectId Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [EnsureMinimumElements(1)]
        public List<string> Colors { get; set; } 
            = [];

        public IEnumerable<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            return results;
        }
    }


}
