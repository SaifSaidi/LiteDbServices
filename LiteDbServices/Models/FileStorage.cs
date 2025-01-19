using LiteDB;
using System.ComponentModel.DataAnnotations;

namespace LiteDbServices.Models
{
    public class FileStorage
            : LiteEntity<ObjectId>
    {
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        public FileInfo? Info { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        public new IEnumerable<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            return results;
        }
    }


}
