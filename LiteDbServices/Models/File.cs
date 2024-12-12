using LiteDB;
using System.ComponentModel.DataAnnotations;

namespace LiteDbServices.Models
{
    public class File : ILiteEntity
    {
        public ObjectId Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public long FileSize { get; set; }
         
        public FileInfo Info { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        public IEnumerable<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            return results;
        }
    }


}
