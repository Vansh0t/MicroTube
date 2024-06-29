namespace MicroTube.Services.Cryptography
{
	public interface IMD5HashProvider
	{
		byte[] Hash(byte[] input);
		string HashAsString(byte[] input);
		string HashAsString(string input);
	}
}