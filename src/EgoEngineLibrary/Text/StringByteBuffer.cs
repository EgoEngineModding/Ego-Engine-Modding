namespace EgoEngineLibrary.Text;

public record StringByteBuffer(byte[] Buffer, int BufferSize, Dictionary<string, int> StringOffsetMap);