namespace MicroTube.Controllers.Videos.Dto
{
    public class VideoUploadDto
    {
        public required string Title { get; set; }
        public required string FileName { get; set; }
        public string? Description { get; set; }
    }
}
