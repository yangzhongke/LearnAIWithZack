using System.Security.Cryptography;
using System.Text;

namespace AI_2;

public static class HashHelper
{
    public static string GetMd5Hash(string input)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(bytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}

