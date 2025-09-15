using System.Text;
using Ude;

namespace AI_2;

public static class FileHelpers
{
    public static async Task<string> ReadAllTextAnyEncodingAsync(string filePath,
        CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        // 用Ude检测编码
        var detector = new CharsetDetector();
        detector.Feed(bytes, 0, bytes.Length);
        detector.DataEnd();
        var encoding = Encoding.UTF8;
        if (detector.Charset != null) encoding = Encoding.GetEncoding(detector.Charset);
        return encoding.GetString(bytes);
    }
}