namespace MicroTube.Services.Cryptography
{
	public class MD5HashProvider : IMD5HashProvider
	{
		public byte[] Hash(byte[] input)
		{
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] hashBytes = md5.ComputeHash(input);

				return hashBytes;
			}
		}
		public string HashAsString(byte[] input)
		{
			if (input == null || input.Length == 0)
			{
				throw new ArgumentException("Unable to produce hash, input is empty or null");
			}
			var hashBytes = Hash(input);
			return Convert.ToHexString(hashBytes);
		}
		public string HashAsString(string input)
		{
			var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
			return HashAsString(inputBytes);
		}
	}
}
