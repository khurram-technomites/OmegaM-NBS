using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NowBuySell.Web.Helpers.Encryption
{
	public class Crypto
	{
		public string Hash(string secret, string salt)
		{
			HashAlgorithm hashAlgorithm = SHA512.Create();
			byte[] byteArray = new byte[32];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(byteArray);

			List<byte> pass = new List<byte>(Encoding.Unicode.GetBytes(secret + salt));
			pass.AddRange(byteArray);
			return Convert.ToBase64String(hashAlgorithm.ComputeHash(pass.ToArray()));

			//var keyBytes = Encoding.UTF8.GetBytes(secret);
			//var saltBytes = Encoding.UTF8.GetBytes(salt);
			//var cost = 262;
			//var blockSize = 8;
			//var parallel = 1;
			//var maxThreads = (int?)null;
			//var derivedKeyLength = 64;

			//var bytes = SCrypt.ComputeDerivedKey(keyBytes, saltBytes, cost, blockSize, parallel, maxThreads, derivedKeyLength);
			//return Convert.ToBase64String(bytes);
		}

		public string RetrieveHash(string secret, string salt)
		{
			HashAlgorithm hashAlgorithm = SHA512.Create();
			byte[] computedHash = null;

			byte[] Salt = Encoding.ASCII.GetBytes(salt);
			List<byte> buffer = new List<byte>(Encoding.Unicode.GetBytes(secret));
			buffer.AddRange(Salt);
			computedHash = hashAlgorithm.ComputeHash(buffer.ToArray());
			return Convert.ToBase64String(computedHash);

		}

		internal string RetrieveHash(object password, string v)
		{
			throw new NotImplementedException();
		}

		// Generates a random string with a given size.    
		public string Random(int size, bool lowerCase = false)
		{
			Random _random = new Random();
			var builder = new StringBuilder(size);

			// Unicode/ASCII Letters are divided into two blocks
			// (Letters 65–90 / 97–122):
			// The first group containing the uppercase letters and
			// the second group containing the lowercase.  

			// char is a single Unicode character  
			char offset = lowerCase ? 'a' : 'A';
			const int lettersOffset = 26; // A...Z or a..z: length=26  

			for (var i = 0; i < size; i++)
			{
				var @char = (char)_random.Next(offset, offset + lettersOffset);
				builder.Append(@char);
			}

			return lowerCase ? builder.ToString().ToLower() : builder.ToString();
		}
	}
}