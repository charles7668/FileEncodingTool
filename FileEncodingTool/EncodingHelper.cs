namespace EncodingChecker;

public static class EncodingHelper
{
    /// <summary>
    /// Detect BOM header
    /// </summary>
    /// <param name="content"></param>
    /// <returns>
    /// length of BOM header
    /// </returns>
    public static int DetectBOM(byte[] content)
    {
        if (content.Length < 4)
            return 0;
        var buffer = new byte[4]; // BOM are shorter than 4 bytes
        Array.Copy(content, buffer, 4);
        // 判断 BOM
        if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            return 3;
        if (buffer[0] == 0xFF && buffer[1] == 0xFE)
            return 2;
        if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            return 2;
        if (buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x00 && buffer[3] == 0x00)
            return 4;
        if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xFE && buffer[3] == 0xFF)
            return 2;
        return 0;
    }

    /// <summary>
    /// Remove BOM header
    /// </summary>
    /// <param name="content"></param>
    /// <param name="bomLength"></param>
    /// <returns>byte array after remove</returns>
    public static byte[] RemoveBOM(byte[] content, int bomLength)
    {
        if (content.Length < bomLength)
            return (byte[])content.Clone();
        var result = new byte[content.Length - bomLength];
        Array.Copy(content, bomLength, result, 0, content.Length - bomLength);
        return result;
    }

    /// <summary>
    /// auto-detect BOM and remove
    /// </summary>
    /// <param name="content"></param>
    /// <returns>byte array after remove</returns>
    public static byte[] RemoveBOM(byte[] content)
    {
        var bomLength = DetectBOM(content);
        return RemoveBOM(content, bomLength);
    }
}