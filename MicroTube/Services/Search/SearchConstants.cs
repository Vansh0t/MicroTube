using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace MicroTube.Services.Search
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum VideoSortType { Relevance, Time, Views, Rating}
	[JsonConverter(typeof(StringEnumConverter))]
	public enum VideoLengthFilterType { None, Short, Medium, Long}
	[JsonConverter(typeof(StringEnumConverter))]
	public enum VideoTimeFilterType { None, LastDay, LastWeek, LastMonth, LastSixMonths, LastYear, }
	[JsonConverter(typeof(StringEnumConverter))]
	public enum VideoCommentSortType { Top, Newest }
}
