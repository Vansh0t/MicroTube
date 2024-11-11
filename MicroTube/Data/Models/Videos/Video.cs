using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MicroTube.Data.Models.Reactions;

namespace MicroTube.Data.Models.Videos
{
    public class Video : IReactable
    {
        [Key]
        public Guid Id { get; set; }
        [Column(TypeName = "NVARCHAR"), StringLength(200)]
        public required string Title { get; set; }
        [Column(TypeName = "NVARCHAR"), StringLength(1000)]
        public string? Description { get; set; }
        [ForeignKey(nameof(Uploader))]
        public Guid? UploaderId { get; set; }
        public AppUser? Uploader { get; set; }
        [Column(TypeName = "VARCHAR"), StringLength(2000)]
        public required string Urls { get; set; }
        [Column(TypeName = "VARCHAR"), StringLength(5000)] //TO DO: optimize this
        public required string ThumbnailUrls { get; set; }
        public DateTime UploadTime { get; set; }
        public int LengthSeconds { get; set; }
        public VideoViewsAggregation? VideoViewsAggregation { get; set; }
        public VideoReactionsAggregation? VideoReactionsAggregation { get; set; }
        public VideoSearchIndexing? VideoIndexing { get; set; }
        public int CommentsCount { get; set; }
        [NotMapped]
        public IReactionsAggregation? ReactionsAggregation => VideoReactionsAggregation;
    }
}
