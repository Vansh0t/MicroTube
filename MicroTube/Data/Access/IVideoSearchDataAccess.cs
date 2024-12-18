﻿using MicroTube.Data.Models.Videos;

namespace MicroTube.Data.Access
{
    public interface IVideoSearchDataAccess
    {
        Task<VideoSearchSuggestionIndex?> GetSuggestion(string text);
        Task<string> IndexSuggestion(VideoSearchSuggestionIndex suggestionIndex);
        Task<string> IndexVideo(VideoSearchIndex videoIndex);
		Task<VideoSearchIndex?> GetVideoIndex(string id);

	}
}