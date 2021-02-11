using System;

namespace LibEveryFileExplorer.Files
{
    public interface IConvertable
	{
		String GetConversionFileFilters();
		bool Convert(int FilterIndex, String Path);
	}
}
