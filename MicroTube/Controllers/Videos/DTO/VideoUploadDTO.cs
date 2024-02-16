namespace MicroTube.Controllers.Videos.DTO
{
    public class VideoUploadDTO
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required IFormFile File { get; set; }
    }
}
