using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HfsChargesContainer.Infrastructure.Models
{
    [Table("BatchLog")]
    public class BatchLog
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Type { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public bool IsSuccess { get; set; }
        public ICollection<BatchLogError> BatchLogErrors { get; private set; }
    }
}
