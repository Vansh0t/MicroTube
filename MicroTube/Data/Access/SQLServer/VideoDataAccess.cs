using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MicroTube.Data.Models;
using System.Data;

namespace MicroTube.Data.Access.SQLServer
{
    public class VideoDataAccess
    {
		private readonly IConfiguration _config;

		public VideoDataAccess(IConfiguration config)
		{
			_config = config;
		}
		public async Task<Video?> GetVideoById(int id)
        {
			using IDbConnection connection = new SqlConnection(_config.GetDefaultConnectionString());
			var parameters = new
			{
				Id = id
			};
			string sql = @"SELECT *.video, *.fileMeta
						   FROM dbo.Video video
						   LEFT JOIN dbo.FileMeta fileMeta
						   ON video.FileMetaId = fileMeta.Id
						   WHERE video.Id = @Id";
			var result = await connection.QueryAsync<Video, FileMeta, Video>(sql,
			(video, fileMeta) =>
			{
				video.FileMeta = fileMeta;
				return video;
			});
			return result.FirstOrDefault();
		}
    }
}
