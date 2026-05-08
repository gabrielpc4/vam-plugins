using System;
using System.Text.RegularExpressions;

namespace MVR.FileManagement;

public abstract class FileEntry
{
	public virtual string Uid { get; protected set; }

	public virtual string Path { get; protected set; }

	public virtual string SlashPath { get; protected set; }

	public virtual string FullPath { get; protected set; }

	public virtual string FullSlashPath { get; protected set; }

	public virtual string Name { get; protected set; }

	public virtual bool Exists { get; protected set; }

	public virtual DateTime LastWriteTime { get; protected set; }

	public FileEntry(string path)
	{
		if (path == null)
		{
			throw new Exception("Null path in FileEntry constructor");
		}
		Path = path.Replace('/', '\\');
		SlashPath = path.Replace('\\', '/');
		FullPath = Path;
		FullSlashPath = SlashPath;
		Uid = SlashPath;
		Name = Regex.Replace(SlashPath, ".*/", string.Empty);
		Exists = true;
	}

	public override string ToString()
	{
		return Path;
	}

	public abstract FileEntryStream OpenStream();

	public abstract FileEntryStreamReader OpenStreamReader();
}
