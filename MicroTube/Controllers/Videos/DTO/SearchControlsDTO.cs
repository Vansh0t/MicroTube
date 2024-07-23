namespace MicroTube.Controllers.Videos.DTO
{
	public class SearchControlsDTO
	{
		public required IEnumerable<string> SortOptions { get; set; }
		public required IEnumerable<string> TimeFilterOptions { get; set; }
		public required IEnumerable<string> LengthFilterOptions { get; set; }
	}
}
