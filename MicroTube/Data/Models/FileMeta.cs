using System.ComponentModel.DataAnnotations.Schema;

namespace MicroTube.Data.Models
{
	public class FileMeta
	{
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Path { get; set; }
		[NotMapped]
		public string FullPath
		{
			get
			{
				return System.IO.Path.Join(Path, Name);
			}
		}
		[NotMapped]
		public bool Exists
		{
			get
			{
				return File.Exists(FullPath);
			}
		}
	}
}
