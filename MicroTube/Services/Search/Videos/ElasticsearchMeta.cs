﻿using Elastic.Clients.Elasticsearch;

namespace MicroTube.Services.Search.Videos
{
	public class ElasticsearchMeta
	{
		public IEnumerable<FieldValue>? LastSort { get; set; }
	}
}