namespace MicroTube.Controllers.Videos.Dto
{
	public class SearchControlsDto
	{
		public required IEnumerable<string> SortOptions { get; set; }
		public required IEnumerable<string> TimeFilterOptions { get; set; }
		public required IEnumerable<string> LengthFilterOptions { get; set; }
	}
}
