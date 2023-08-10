using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HfsChargesContainer.Infrastructure.Models
{
    [Table("GoogleFileSetting")]
    public class GoogleFileSetting
    {
        [Key]
        public int Id { get; set; }
        public string GoogleIdentifier { get; set; }
        public string FileType { get; set; }
        public string Label { get; set; }
        public int FileYear { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
