using System.IO;

namespace MVR.FileManagement;

public class SystemFileEntry : FileEntry
{
	public SystemFileEntry(string path)
		: base(path)
	{
		Exists = File.Exists(Path);
		FullPath = System.IO.Path.GetFullPath(Path);
		FullSlashPath = FullPath.Replace('\\', '/');
		if (Exists)
		{
			LastWriteTime = File.GetLastWriteTime(Path);
		}
	}

	public override FileEntryStream OpenStream()
	{
		return new SystemFileEntryStream(this);
	}

	public override FileEntryStreamReader OpenStreamReader()
	{
		return new SystemFileEntryStreamReader(this);
	}
}
