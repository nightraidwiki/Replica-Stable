using System.Security.Cryptography;
using System.Text;

namespace Replica.Engine.Host;

internal static class ModuleLoader
{
	public static string Sha256Hex(string input)
	{
		byte[] array = SHA256.HashData(Encoding.UTF8.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}
}
