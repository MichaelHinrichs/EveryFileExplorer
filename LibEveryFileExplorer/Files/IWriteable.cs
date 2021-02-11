using System;

namespace LibEveryFileExplorer.Files
{
    public interface IWriteable
	{
		String GetSaveDefaultFileFilter();
		byte[] Write();
	}
}
