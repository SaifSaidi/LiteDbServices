using LiteDB;
using System.ComponentModel.DataAnnotations;
 
namespace LiteDbServices.Models
{

    public abstract class LiteEntity<TKey>
    {
        [BsonId]
        public virtual TKey Id { get; set; } = default!;

        

        //IEnumerable<ValidationResult> Validate();
        public IEnumerable<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            return results;
        }

    }




}
