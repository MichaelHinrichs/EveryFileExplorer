namespace LibEveryFileExplorer.Files
{
    public interface IUseOtherFiles
	{
		void FileOpened(ViewableFile File);
		void FileClosed(ViewableFile File);
	}
}
