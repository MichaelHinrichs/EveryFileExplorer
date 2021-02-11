namespace LibEveryFileExplorer.Compression
{
    public interface ICompressable
	{
		byte[] Compress(byte[] Data);
	}
}
