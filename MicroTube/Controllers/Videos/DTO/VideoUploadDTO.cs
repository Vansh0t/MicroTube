namespace MicroTube.Controllers.Videos.Dto
{
    public class VideoUploadDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required IFormFile File { get; set; }
    }
}
