using LiteDB;
using System.ComponentModel.DataAnnotations;
 
namespace LiteDbServices.Models
{
    public interface ILiteEntity
    {
        [BsonId]
        ObjectId Id { get; set; }

        IEnumerable<ValidationResult> Validate();
    }




}
