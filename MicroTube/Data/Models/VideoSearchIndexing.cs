using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MicroTube.Data.Models
{
	public class VideoSearchIndexing
	{
		[Key]
		[Required]
		[ForeignKey(nameof(Video))]
		public Guid VideoId { get; set; }
		public Video? Video { get; set; }
		[Column(TypeName = "VARCHAR"), StringLength(50)]
		public string? SearchIndexId { get; set; }
		public bool ReindexingRequired { get; set; }
		public DateTime? LastIndexingTime { get; set; }
	}
}
