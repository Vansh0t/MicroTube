using MicroTube.Data.Models;
using System.Text.Json.Serialization;

namespace MicroTube.Controllers.Videos.Dto
{
	public class VideoUploadProgressDto
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public VideoUploadStatus Status { get; set; }
		public string? Message { get; set; }
		public int? LengthSeconds { get; set; }
		public int? Fps { get; set; }
		public string? FrameSize { get; set; }
		public string? Format { get; set; }
		public VideoUploadProgressDto(
			string id,
			VideoUploadStatus status,
			string title,
			string? description,
			string? message,
			DateTime timestamp,
			int? lengthSeconds,
			int? fps,
			string? frameSize,
			string? format)
		{
			Id = id;
			Status = status;
			Title = title;
			Description = description;
			Message = message;
			Timestamp = timestamp;
			LengthSeconds = lengthSeconds;
			Fps = fps;
			FrameSize = frameSize;
			Format = format;
		}
	}
}
