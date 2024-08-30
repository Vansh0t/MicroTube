using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroTube.Data.Models
{
	public class FileMeta
	{
		public Guid Id { get; set; }
		[Column(TypeName = "NVARCHAR"), StringLength(100)]
		public required string Name { get; set; }
		[Column(TypeName = "NVARCHAR"), StringLength(150)]
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
